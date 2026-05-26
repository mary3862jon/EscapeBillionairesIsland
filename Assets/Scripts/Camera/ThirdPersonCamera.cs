using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Simple over-shoulder follow camera with mouse orbit.
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 5f;
        public float height = 2.2f;
        public float orbitSpeed = 180f;
        public float pitchMin = -20f;
        public float pitchMax = 60f;
        public float followLerp = 12f;

        float yaw;
        float pitch = 18f;

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

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 offset = rot * new Vector3(0f, 0f, -distance) + Vector3.up * height;
            Vector3 desired = target.position + offset;

            transform.position = Vector3.Lerp(transform.position, desired, followLerp * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 0.4f);
        }
    }
}
