using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Over-shoulder follow with orbit. Defaults tuned for SEE-MORE: high angle, wider FOV.
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 8f;
        public float height = 4.5f;
        public float fov = 70f;
        public float orbitSpeed = 220f;
        public float pitchMin = 5f;
        public float pitchMax = 70f;
        public float followLerp = 9f;

        float yaw;
        float pitch = 32f;

        void Start()
        {
            var cam = GetComponent<Camera>();
            if (cam != null) cam.fieldOfView = fov;
        }

        void LateUpdate()
        {
            if (target == null) return;

            var mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.isPressed)
            {
                var delta = mouse.delta.ReadValue();
                yaw += delta.x * orbitSpeed * Time.deltaTime * 0.05f;
                pitch -= delta.y * orbitSpeed * Time.deltaTime * 0.05f;
                pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
            }

            // mouse wheel zoom
            if (mouse != null)
            {
                float scroll = mouse.scroll.ReadValue().y * 0.01f;
                distance = Mathf.Clamp(distance - scroll, 4f, 16f);
            }

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 offset = rot * new Vector3(0f, 0f, -distance) + Vector3.up * height;
            Vector3 desired = target.position + offset;

            transform.position = Vector3.Lerp(transform.position, desired, followLerp * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 0.6f);
        }
    }
}
