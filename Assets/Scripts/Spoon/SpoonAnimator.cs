using UnityEngine;

namespace Spoonacci
{
    // Procedural animation for Sir Spoonacci.
    // - Walk: bowl rocks side-to-side + handle bobs (waddle)
    // - Hop: anticipation squash → stretch in air → landing squash
    // - Idle: subtle Y bob + slow head tilt
    // - Bonk: forward-back swing
    [DefaultExecutionOrder(50)]
    public class SpoonAnimator : MonoBehaviour
    {
        public Transform visual;
        public float walkSpeed = 12f;        // rock cycles per second multiplier
        public float walkRockDeg = 14f;      // side-to-side roll while walking
        public float walkBobAmp = 0.06f;     // vertical bob
        public float idleBobAmp = 0.03f;
        public float idleBobSpeed = 1.6f;

        Rigidbody rb;
        float walkPhase;
        float bonkTimer;
        Vector3 baseLocalPos;
        Quaternion baseLocalRot;
        Vector3 squashScale = Vector3.one;
        float landingPulse;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (visual == null)
            {
                var b = GetComponent<ProceduralSpoonBuilder>();
                if (b != null) visual = b.Visual;
            }
            if (visual != null)
            {
                baseLocalPos = visual.localPosition;
                baseLocalRot = visual.localRotation;
            }
        }

        public void TriggerLandSquash() { landingPulse = 1f; }
        public void TriggerBonkSwing() { bonkTimer = 0.35f; }

        // Re-point at a freshly rebuilt visual (SpoonTypeSwitcher rebuilds the mesh on
        // keys 1-9). Without this the animator keeps a reference to the DESTROYED old
        // visual and all animation — including the bonk swing — silently dies.
        public void Rebind(Transform v)
        {
            visual = v;
            if (visual != null)
            {
                baseLocalPos = visual.localPosition;
                baseLocalRot = visual.localRotation;
            }
        }

        void LateUpdate()
        {
            // self-heal if the visual was rebuilt and we weren't rebound
            if (visual == null)
            {
                var b = GetComponent<ProceduralSpoonBuilder>();
                if (b != null && b.Visual != null) Rebind(b.Visual);
                if (visual == null) return;
            }

            Vector3 horiz = rb != null ? rb.linearVelocity : Vector3.zero;
            horiz.y = 0f;
            float speed = horiz.magnitude;

            // walk phase advances with speed
            walkPhase += speed * walkSpeed * Time.deltaTime;

            // rock = sin(phase) on local Z; bob = abs(sin) for double-step y
            float walkBlend = Mathf.Clamp01(speed / 3.5f);
            float roll = Mathf.Sin(walkPhase) * walkRockDeg * walkBlend;
            float walkBob = Mathf.Abs(Mathf.Sin(walkPhase)) * walkBobAmp * walkBlend;
            float idleBob = Mathf.Sin(Time.time * idleBobSpeed) * idleBobAmp * (1f - walkBlend);
            float headTilt = Mathf.Sin(Time.time * 0.7f) * 3f * (1f - walkBlend); // subtle idle head sway

            // bonk swing — quick forward dip + recover
            float bonkPitch = 0f;
            if (bonkTimer > 0f)
            {
                float t = 1f - (bonkTimer / 0.35f); // 0..1
                bonkPitch = Mathf.Sin(t * Mathf.PI) * 55f; // pitch forward
                bonkTimer -= Time.deltaTime;
            }

            visual.localPosition = baseLocalPos + Vector3.up * (walkBob + idleBob);
            visual.localRotation = baseLocalRot * Quaternion.Euler(bonkPitch + headTilt * 0.4f, 0f, roll);

            // landing squash
            if (landingPulse > 0f)
            {
                landingPulse -= Time.deltaTime * 3.5f;
                float k = Mathf.Clamp01(landingPulse);
                squashScale = new Vector3(1f + 0.18f * k, 1f - 0.22f * k, 1f + 0.18f * k);
            }
            else
            {
                squashScale = Vector3.Lerp(squashScale, Vector3.one, Time.deltaTime * 8f);
            }
            visual.localScale = squashScale;
        }
    }
}
