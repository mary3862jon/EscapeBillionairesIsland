using UnityEngine;

namespace Spoonacci
{
    // Safety net so the player can never end up below — OR outside — the world.
    // Remembers the last grounded position and teleports back up if we fall through,
    // and holds the player inside the play area with a soft invisible wall.
    [RequireComponent(typeof(Rigidbody))]
    public class FallGuard : MonoBehaviour
    {
        public float killY = -3f;       // below this = considered fallen through
        public float safeMaxY = 6f;     // only record "safe" spots near ground level
        public float boundsXZ = 0f;     // >0 = invisible wall at +/-boundsXZ on X and Z (0 = off)

        Rigidbody rb;
        Vector3 lastSafe;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            lastSafe = transform.position;
        }

        void FixedUpdate()
        {
            float y = transform.position.y;
            if (y < killY)
            {
                transform.position = lastSafe + Vector3.up * 1.5f;
                if (rb != null) rb.linearVelocity = Vector3.zero;
                return;
            }
            // record a fresh safe point only when standing roughly on the surface
            if (y > -0.4f && y < safeMaxY)
                lastSafe = transform.position;

            // Soft invisible wall: keep the player inside the play area so they can't
            // walk out past the perimeter hedge / through the gate and end up on the
            // bare ground plane staring at the back of the world.
            if (boundsXZ > 0f)
            {
                Vector3 p = transform.position;
                Vector3 v = rb != null ? rb.linearVelocity : Vector3.zero;
                bool hit = false;
                if (p.x >  boundsXZ) { p.x =  boundsXZ; if (v.x > 0f) v.x = 0f; hit = true; }
                if (p.x < -boundsXZ) { p.x = -boundsXZ; if (v.x < 0f) v.x = 0f; hit = true; }
                if (p.z >  boundsXZ) { p.z =  boundsXZ; if (v.z > 0f) v.z = 0f; hit = true; }
                if (p.z < -boundsXZ) { p.z = -boundsXZ; if (v.z < 0f) v.z = 0f; hit = true; }
                if (hit)
                {
                    transform.position = p;
                    if (rb != null) rb.linearVelocity = v;
                }
            }
        }
    }
}
