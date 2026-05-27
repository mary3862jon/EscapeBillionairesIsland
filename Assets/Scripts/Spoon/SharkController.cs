using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Post-fusion: player controls a shark with Sir Spoonacci lodged in its snout.
    // WASD = swim, Mouse Y orbit affects camera pitch (handled by ThirdPersonCamera).
    // Space = dash boost. The shark stays in water (clamped Y).
    [RequireComponent(typeof(Rigidbody))]
    public class SharkController : MonoBehaviour
    {
        public float swimSpeed = 6.5f;
        public float turnSpeed = 360f;
        public float dashSpeed = 14f;
        public Transform cameraTransform;

        Rigidbody rb;
        Vector2 moveInput;
        bool dashing;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearDamping = 1.5f;
            rb.angularDamping = 4f;
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
            float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
            moveInput = new Vector2(h, v);
            dashing = kb.spaceKey.isPressed;
        }

        void FixedUpdate()
        {
            Vector3 fwd = cameraTransform != null ? cameraTransform.forward : transform.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : transform.right;
            fwd.y = 0f; right.y = 0f;
            fwd.Normalize(); right.Normalize();

            Vector3 dir = fwd * moveInput.y + right * moveInput.x;
            if (dir.sqrMagnitude > 1f) dir.Normalize();

            float speed = dashing ? dashSpeed : swimSpeed;
            Vector3 v = dir * speed;
            v.y = Mathf.Lerp(rb.linearVelocity.y, 0f, 4f * Time.fixedDeltaTime); // stay near surface
            rb.linearVelocity = v;

            // clamp Y to water level
            Vector3 p = rb.position;
            p.y = Mathf.Clamp(p.y, -1.5f, 0.5f);
            rb.position = p;

            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
                rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, target, turnSpeed * Time.fixedDeltaTime));
            }
        }
    }
}
