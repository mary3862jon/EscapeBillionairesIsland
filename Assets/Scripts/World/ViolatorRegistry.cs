using System.Collections.Generic;
using UnityEngine;

namespace Spoonacci
{
    // Global lookup so BonkAttack doesn't have to FindObjectsOfType every frame.
    public static class ViolatorRegistry
    {
        static readonly HashSet<Civilian> _set = new HashSet<Civilian>();
        public static IEnumerable<Civilian> All => _set;
        public static void Register(Civilian c) => _set.Add(c);
        public static void Unregister(Civilian c) => _set.Remove(c);

        public static Civilian NearestTo(Vector3 pos, float maxDist)
        {
            Civilian best = null;
            float bestSqr = maxDist * maxDist;
            foreach (var c in _set)
            {
                if (c == null) continue;
                float d = (c.transform.position - pos).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = c; }
            }
            return best;
        }
    }
}
