using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // WASD/arrow movement, camera-relative. Space = hop. F = bonk (if violator nearby, locks on).
    [RequireComponent(typeof(Rigidbody))]
    public class SpoonController : MonoBehaviour
    {
        public float moveSpeed = 7.5f;   // bumped 4.8 → 7.5 (was too slow)
        public float turnSpeed = 1100f;
        public float hopForce = 6.2f;
        public float sprintMultiplier = 1.6f; // hold Shift to sprint
        public Transform cameraTransform;

        Rigidbody rb;
        SpoonAnimator anim;
        Vector2 moveInput;
        bool jumpQueued;
        bool grounded;
        bool wasGrounded;
        float nextFootstep;

        // lunge state — when F bonks a violator we override movement briefly
        public bool LungeActive { get; private set; }
        Vector3 lungeTargetPos;
        float lungeUntil;
        const float LUNGE_SPEED = 18f;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            anim = GetComponent<SpoonAnimator>();
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
            if (LungeActive)
            {
                Vector3 to = lungeTargetPos - transform.position; to.y = 0f;
                if (to.magnitude < 0.6f || Time.time > lungeUntil)
                {
                    LungeActive = false;
                    rb.linearVelocity = Vector3.zero;
                }
                else
                {
                    Vector3 dir = to.normalized;
                    Vector3 vel = dir * LUNGE_SPEED;
                    vel.y = rb.linearVelocity.y;
                    rb.linearVelocity = vel;
                    if (dir.sqrMagnitude > 0.01f)
                        rb.MoveRotation(Quaternion.LookRotation(dir, Vector3.up));
                }
                return;
            }

            // camera-relative direction
            Vector3 fwd = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;
            fwd.y = 0f; right.y = 0f;
            fwd.Normalize(); right.Normalize();

            Vector3 desired = fwd * moveInput.y + right * moveInput.x;
            if (desired.sqrMagnitude > 1f) desired.Normalize();

            // sprint with Shift
            var kbS = Keyboard.current;
            float speed = moveSpeed;
            if (kbS != null && (kbS.leftShiftKey.isPressed || kbS.rightShiftKey.isPressed)) speed *= sprintMultiplier;

            Vector3 horiz = desired * speed;
            var v = rb.linearVelocity;
            v.x = horiz.x; v.z = horiz.z;
            rb.linearVelocity = v;

            if (desired.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(desired, Vector3.up);
                rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, target, turnSpeed * Time.fixedDeltaTime));
            }

            if (jumpQueued)
            {
                rb.AddForce(Vector3.up * hopForce, ForceMode.VelocityChange);
                SoundFx.Instance.Jump();
                jumpQueued = false;
                grounded = false;
            }

            // footstep tick — every ~0.35s while moving on ground
            if (grounded && horiz.sqrMagnitude > 0.04f && Time.time > nextFootstep)
            {
                SoundFx.Instance.Footstep();
                nextFootstep = Time.time + 0.35f;
            }

            // landing detection
            if (grounded && !wasGrounded && anim != null) anim.TriggerLandSquash();
            wasGrounded = grounded;
        }

        public void StartLungeTo(Vector3 worldPos, float maxDuration = 0.7f)
        {
            LungeActive = true;
            lungeTargetPos = worldPos;
            lungeUntil = Time.time + maxDuration;
        }

        void OnCollisionStay(Collision c)
        {
            foreach (var contact in c.contacts)
                if (contact.normal.y > 0.5f) { grounded = true; return; }
        }

        void OnCollisionExit(Collision c) { grounded = false; }
    }
}
