using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Walking-eye-level third-person follow camera.
    // Two control modes:
    //   • TRADITIONAL — orbit only while holding right-mouse (default)
    //   • FPS         — cursor locked, mouse always looks (toggle with Tab)
    // Also raycasts from target → desired camera position and pulls in on hit, so we never sit inside walls.
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 5.5f;
        public float height = 1.8f;
        public float fov = 65f;
        public float orbitSpeed = 220f;
        public float pitchMin = -12f;
        public float pitchMax = 50f;
        public float followLerp = 9f;
        public float collisionPadding = 0.35f;

        public bool fpsMode = false;
        float yaw;
        float pitch = 12f;

        void Start()
        {
            var cam = GetComponent<Camera>();
            if (cam != null) cam.fieldOfView = fov;
            ApplyCursor();
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.tabKey.wasPressedThisFrame)
            {
                fpsMode = !fpsMode;
                ApplyCursor();
            }
        }

        void ApplyCursor()
        {
            if (fpsMode)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        void LateUpdate()
        {
            if (target == null) return;

            var mouse = Mouse.current;
            bool look = fpsMode || (mouse != null && mouse.rightButton.isPressed);
            if (mouse != null && look)
            {
                var delta = mouse.delta.ReadValue();
                yaw += delta.x * orbitSpeed * Time.deltaTime * 0.05f;
                pitch -= delta.y * orbitSpeed * Time.deltaTime * 0.05f;
                pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
            }
            if (mouse != null)
            {
                float scroll = mouse.scroll.ReadValue().y * 0.01f;
                distance = Mathf.Clamp(distance - scroll, 2.5f, 14f);
            }

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 offset = rot * new Vector3(0f, 0f, -distance) + Vector3.up * height;
            Vector3 desired = target.position + offset;

            // wall-collision: cast from target to desired, pull in if blocked
            Vector3 from = target.position + Vector3.up * 0.6f;
            Vector3 dir = desired - from;
            float reqDist = dir.magnitude;
            if (reqDist > 0.01f && Physics.SphereCast(from, 0.3f, dir.normalized, out RaycastHit hit, reqDist, ~0, QueryTriggerInteraction.Ignore))
            {
                desired = from + dir.normalized * Mathf.Max(0.8f, hit.distance - collisionPadding);
            }

            transform.position = Vector3.Lerp(transform.position, desired, followLerp * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 0.7f);
        }
    }
}
