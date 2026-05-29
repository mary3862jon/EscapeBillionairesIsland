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
        // Invisible play-area wall. Set boundsXZ for a square area, OR boundsX/boundsZ
        // for a rectangle (e.g. the 50x30 prison). 0 on an axis = no wall on that axis.
        public float boundsXZ = 0f;
        public float boundsX = 0f;
        public float boundsZ = 0f;
        public Vector2 boundsCenter = Vector2.zero; // center of the play area on XZ

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
            // walk out past the walls/perimeter and end up on the bare ground plane
            // staring at the back of the world. Works for any scene (island, prison…).
            float limX = boundsX > 0f ? boundsX : boundsXZ;
            float limZ = boundsZ > 0f ? boundsZ : boundsXZ;
            if (limX > 0f || limZ > 0f)
            {
                Vector3 p = transform.position;
                Vector3 v = rb != null ? rb.linearVelocity : Vector3.zero;
                bool hit = false;
                if (limX > 0f)
                {
                    float hiX = boundsCenter.x + limX, loX = boundsCenter.x - limX;
                    if (p.x > hiX) { p.x = hiX; if (v.x > 0f) v.x = 0f; hit = true; }
                    if (p.x < loX) { p.x = loX; if (v.x < 0f) v.x = 0f; hit = true; }
                }
                if (limZ > 0f)
                {
                    float hiZ = boundsCenter.y + limZ, loZ = boundsCenter.y - limZ;
                    if (p.z > hiZ) { p.z = hiZ; if (v.z > 0f) v.z = 0f; hit = true; }
                    if (p.z < loZ) { p.z = loZ; if (v.z < 0f) v.z = 0f; hit = true; }
                }
                if (hit)
                {
                    transform.position = p;
                    if (rb != null) rb.linearVelocity = v;
                }
            }
        }
    }
}
