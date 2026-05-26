using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // WASD/arrow movement, camera-relative. Space = hop.
    [RequireComponent(typeof(Rigidbody))]
    public class SpoonController : MonoBehaviour
    {
        public float moveSpeed = 4.5f;
        public float turnSpeed = 720f;
        public float hopForce = 4.5f;
        public Transform cameraTransform;

        Rigidbody rb;
        Vector2 moveInput;
        bool jumpQueued;
        bool grounded;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            float h = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f)
                    - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
            float v = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f)
                    - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
            moveInput = new Vector2(h, v);

            if (kb.spaceKey.wasPressedThisFrame && grounded)
                jumpQueued = true;
        }

        void FixedUpdate()
        {
            // camera-relative direction
            Vector3 fwd = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;
            fwd.y = 0f; right.y = 0f;
            fwd.Normalize(); right.Normalize();

            Vector3 desired = fwd * moveInput.y + right * moveInput.x;
            if (desired.sqrMagnitude > 1f) desired.Normalize();

            Vector3 vel = rb.linearVelocity;
            Vector3 horiz = desired * moveSpeed;
            vel.x = horiz.x;
            vel.z = horiz.z;
            rb.linearVelocity = vel;

            if (desired.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(desired, Vector3.up);
                rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, target, turnSpeed * Time.fixedDeltaTime));
            }

            if (jumpQueued)
            {
                rb.AddForce(Vector3.up * hopForce, ForceMode.VelocityChange);
                jumpQueued = false;
                grounded = false;
            }
        }

        void OnCollisionStay(Collision c)
        {
            foreach (var contact in c.contacts)
            {
                if (contact.normal.y > 0.5f) { grounded = true; return; }
            }
        }

        void OnCollisionExit(Collision c) { grounded = false; }
    }
}
