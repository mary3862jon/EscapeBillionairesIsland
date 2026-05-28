using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // No-mouse third-person follow camera. Sits behind the player at a fixed angle.
    // Camera yaw = player yaw (so steering with A/D also turns the camera).
    // Scroll wheel still snaps zoom (3.5 / 6 / 9 / 13). Cursor stays visible at all times.
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public float[] zoomLevels = { 3.5f, 6f, 9f, 13f };
        public int zoomIndex = 1;
        public float height = 1.8f;
        public float fov = 65f;
        public float pitch = 18f;          // fixed downward pitch
        public float followLerp = 9f;
        public float collisionPadding = 0.35f;

        public float distance => zoomLevels[Mathf.Clamp(zoomIndex, 0, zoomLevels.Length - 1)];

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
            Quaternion rot = Quaternion.Euler(pitch, 0f, 0f);
            Vector3 offset = rot * new Vector3(0f, 0f, -distance) + Vector3.up * height;
            transform.position = target.position + offset;
            transform.LookAt(target.position + Vector3.up * 0.7f);
        }

        void LateUpdate()
        {
            if (target == null) return;

            // scroll-wheel snap zoom (no other mouse-camera link)
            var mouse = Mouse.current;
            if (mouse != null)
            {
                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 5f && Time.unscaledTime > scrollDebounce)
                {
                    zoomIndex = Mathf.Clamp(zoomIndex + (scroll > 0 ? -1 : 1), 0, zoomLevels.Length - 1);
                    scrollDebounce = Time.unscaledTime + 0.15f;
                }
            }

            // Camera FIXED in world space — does NOT rotate with the player.
            // Player moves world-space N/S/E/W with WASD. Camera only follows position.
            Quaternion rot = Quaternion.Euler(pitch, 0f, 0f);
            Vector3 offset = rot * new Vector3(0f, 0f, -distance) + Vector3.up * height;
            Vector3 desired = target.position + offset;

            // wall-collision
            Vector3 from = target.position + Vector3.up * 0.6f;
            Vector3 dir = desired - from;
            float reqDist = dir.magnitude;
            if (reqDist > 0.01f && Physics.SphereCast(from, 0.45f, dir.normalized, out RaycastHit hit, reqDist, ~0, QueryTriggerInteraction.Ignore))
            {
                desired = from + dir.normalized * Mathf.Max(1.6f, hit.distance - collisionPadding);
            }

            transform.position = Vector3.Lerp(transform.position, desired, followLerp * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 0.7f);
        }
    }
}
