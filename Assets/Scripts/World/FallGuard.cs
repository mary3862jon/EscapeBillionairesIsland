using UnityEngine;

namespace Spoonacci
{
    // Safety net so the player can never end up below the world.
    // Remembers the last grounded position and teleports back up if we fall through.
    [RequireComponent(typeof(Rigidbody))]
    public class FallGuard : MonoBehaviour
    {
        public float killY = -3f;       // below this = considered fallen through
        public float safeMaxY = 6f;     // only record "safe" spots near ground level

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
        }
    }
}
