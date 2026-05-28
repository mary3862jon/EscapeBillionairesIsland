using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Walking-eye-level third-person follow camera.
    // - Mouse ALWAYS orbits the camera (no right-click required). Cursor is locked while game is in play.
    // - Scroll wheel snaps between 4 preset zoom levels (no slow incremental scroll).
    // - SphereCast wall-collision so we never sit inside walls/spoons.
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public float[] zoomLevels = { 3.5f, 6f, 9f, 13f };
        public int zoomIndex = 1;
        public float height = 1.8f;
        public float fov = 65f;
        public float orbitSpeed = 220f;
        public float pitchMin = -12f;
        public float pitchMax = 50f;
        public float followLerp = 9f;
        public float collisionPadding = 0.35f;

        public float distance => zoomLevels[Mathf.Clamp(zoomIndex, 0, zoomLevels.Length - 1)];

        float yaw;
        float pitch = 12f;
        float scrollDebounce;

        void Start()
        {
            var cam = GetComponent<Camera>();
            if (cam != null) cam.fieldOfView = fov;
            LockCursor();
            if (target != null) SnapToTarget();
        }

        void SnapToTarget()
        {
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 offset = rot * new Vector3(0f, 0f, -distance) + Vector3.up * height;
            transform.position = target.position + offset;
            transform.LookAt(target.position + Vector3.up * 0.7f);
        }

        void LockCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        void LateUpdate()
        {
            if (target == null) return;

            // unlock cursor if a menu (pause / settings) is up — detect Time.timeScale==0
            bool menuOpen = Time.timeScale == 0f;
            if (menuOpen)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                return; // freeze camera while menu open
            }
            else
            {
                if (Cursor.lockState != CursorLockMode.Locked) LockCursor();
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                // ALWAYS read mouse delta — no RMB requirement
                var delta = mouse.delta.ReadValue();
                yaw += delta.x * orbitSpeed * Time.deltaTime * 0.05f;
                pitch -= delta.y * orbitSpeed * Time.deltaTime * 0.05f;
                pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

                // Scroll = snap zoom levels (debounced so a single scroll-tick = single step)
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
