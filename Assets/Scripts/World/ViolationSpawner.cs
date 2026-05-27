using System.Collections.Generic;
using UnityEngine;

namespace Spoonacci
{
    // Every few seconds, pick a random non-violator civilian and flag them as a violator.
    // Adds variety so the player always has someone to bonk.
    public class ViolationSpawner : MonoBehaviour
    {
        public float interval = 7f;
        public int maxConcurrent = 4;

        readonly string[] violations = new[]
        {
            "LITTERING",
            "VANDALISM",
            "TAX EVASION",
            "INSIDER TRADING",
            "PEEING IN POOL",
            "QUEUE JUMPING",
            "NFT MINTING",
            "SHARK BAITING",
            "LOUD CHEWING",
            "CRYPTO SHILLING",
            "PARKING VIOLATION",
            "BAD TIPPING",
        };

        float nextTick;

        void Update()
        {
            if (Time.time < nextTick) return;
            nextTick = Time.time + interval;

            // count active violators
            int active = 0;
            foreach (var _ in ViolatorRegistry.All) active++;
            if (active >= maxConcurrent) return;

            // pick a random civilian that's not already a violator
            var all = Object.FindObjectsByType<Civilian>(FindObjectsSortMode.None);
            var pool = new List<Civilian>(all.Length);
            foreach (var c in all)
                if (c != null && !c.IsViolator && !c.HasSpoonMark) pool.Add(c);
            if (pool.Count == 0) return;

            var pick = pool[Random.Range(0, pool.Count)];
            string crime = violations[Random.Range(0, violations.Length)];
            pick.SetViolator(true, crime);
        }
    }
}
