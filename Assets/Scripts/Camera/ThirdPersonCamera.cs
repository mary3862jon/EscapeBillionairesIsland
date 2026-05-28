using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Third-person follow.
    //   • Default: camera locked at a fixed angle behind player (yaw stays where you left it)
    //   • HOLD Right Mouse Button: drag the mouse to orbit yaw+pitch (FAST sensitivity)
    //   • Scroll: snap zoom (3.5 / 6 / 9 / 13)
    //   • SphereCast wall-collision so the camera never sits outside walls or above the ceiling
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public float[] zoomLevels = { 3.5f, 6f, 9f, 13f };
        public int zoomIndex = 1;
        public float height = 2.0f;
        public float fov = 65f;
        public float orbitSpeed = 480f;        // FAST mouse-look (was 220)
        public float pitchMin = -10f;
        public float pitchMax = 55f;
        public float followLerp = 9f;
        public float collisionPadding = 0.4f;

        public float distance => zoomLevels[Mathf.Clamp(zoomIndex, 0, zoomLevels.Length - 1)];

        float yaw;
        float pitch = 22f;
        float scrollDebounce;

        void Start()
        {
            var cam = GetComponent<Camera>();
            if (cam != null) cam.fieldOfView = fov;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (target != null) SnapToTarget();
        }

        void SnapToTarget()
        {
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 offset = rot * new Vector3(0f, 0f, -distance) + Vector3.up * height;
            transform.position = target.position + offset;
            transform.LookAt(target.position + Vector3.up * 0.7f);
        }

        void LateUpdate()
        {
            if (target == null) return;

            var mouse = Mouse.current;
            if (mouse != null)
            {
                // Right-click drag = orbit camera (yaw + pitch), FAST
                if (mouse.rightButton.isPressed)
                {
                    var delta = mouse.delta.ReadValue();
                    yaw   += delta.x * orbitSpeed * Time.deltaTime * 0.05f;
                    pitch -= delta.y * orbitSpeed * Time.deltaTime * 0.05f;
                    pitch  = Mathf.Clamp(pitch, pitchMin, pitchMax);
                }

                // Snap-zoom via scroll wheel
                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 5f && Time.unscaledTime > scrollDebounce)
                {
                    zoomIndex = Mathf.Clamp(zoomIndex + (scroll > 0 ? -1 : 1), 0, zoomLevels.Length - 1);
                    scrollDebounce = Time.unscaledTime + 0.15f;
                }
            }

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 offset = rot * new Vector3(0f, 0f, -distance) + Vector3.up * height;
            Vector3 desired = target.position + offset;

            // Wall + ceiling collision so camera never leaves the room
            Vector3 from = target.position + Vector3.up * 0.6f;
            Vector3 dir = desired - from;
            float reqDist = dir.magnitude;
            if (reqDist > 0.01f && Physics.SphereCast(from, 0.5f, dir.normalized, out RaycastHit hit, reqDist, ~0, QueryTriggerInteraction.Ignore))
            {
                desired = from + dir.normalized * Mathf.Max(1.4f, hit.distance - collisionPadding);
            }
            // Hard cap so the camera can never go above interior ceiling (5m by default)
            // — a separate raycast straight UP from desired position
            if (Physics.Raycast(desired, Vector3.up, out RaycastHit upHit, 1.0f, ~0, QueryTriggerInteraction.Ignore))
                desired.y = Mathf.Min(desired.y, upHit.point.y - 0.4f);

            transform.position = Vector3.Lerp(transform.position, desired, followLerp * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 0.7f);
        }
    }
}
