using UnityEngine;
using System.Collections.Generic;
namespace Spoonacci
{
    // Park benches lining the sidewalks of Billionaire's Island. Every bench is
    // anchored to a WorldLayout.SidewalkSlot, so it always sits BESIDE a road,
    // never on it, and is rotated to face the asphalt where the foot traffic flows.
    public static class BenchesBuilder
    {
        // Sun-bleached teak for the slats, near-black wrought iron for the legs.
        private static readonly Color WoodSlat  = new Color(0.55f, 0.36f, 0.18f);
        private static readonly Color WoodSlat2 = new Color(0.48f, 0.30f, 0.14f);
        private static readonly Color BenchLeg  = new Color(0.16f, 0.16f, 0.18f);

        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("Benches", Vector3.zero).transform;
            hub.SetParent(root, true);

            // Grab evenly-spaced sidewalk slots; each already faces the road via slot.yaw.
            List<WorldLayout.Slot> slots = WorldLayout.SidewalkSlots(spacing: 14f, sideOffset: 6.8f);
            if (slots == null || slots.Count == 0) return;

            // Take every other slot so benches feel spread out, capped at ~14.
            int made = 0;
            for (int i = 0; i < slots.Count && made < 14; i += 2)
            {
                var slot = slots[i];
                MakeBench(hub, made, slot.pos, slot.yaw);
                made++;
            }
        }

        // A wooden park bench: 3 seat slats, 2 backrest slats, two dark iron leg
        // frames plus an under-seat brace. Built in local space then placed/rotated
        // so its open seat faces the road. Local +Z points toward the road (the
        // slot's yaw is the road-facing direction), so the backrest goes to -Z.
        private static void MakeBench(Transform hub, int id, Vector3 pos, float yaw)
        {
            var b = BuildKit.Root($"Bench_{id}", pos).transform;
            b.SetParent(hub, true);
            b.localRotation = Quaternion.Euler(0f, yaw, 0f);

            // --- seat slats (run along local X, the length of the bench) ---
            for (int i = 0; i < 3; i++)
            {
                BuildKit.Cube($"Slat_{i}", b,
                    new Vector3(0f, 0.48f, 0.20f - i * 0.18f),
                    new Vector3(1.8f, 0.06f, 0.15f),
                    (i % 2 == 0) ? WoodSlat : WoodSlat2, 0f, 0.3f);
            }

            // --- backrest slats (sit at the rear, -Z, tilted back slightly) ---
            for (int i = 0; i < 2; i++)
            {
                var back = BuildKit.Cube($"Back_{i}", b,
                    new Vector3(0f, 0.78f + i * 0.22f, -0.28f),
                    new Vector3(1.8f, 0.06f, 0.16f),
                    (i % 2 == 0) ? WoodSlat2 : WoodSlat, 0f, 0.3f);
                back.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
            }

            // --- dark iron leg frames on each end (KEEP colliders: solid prop) ---
            BuildKit.Cube("LegL", b, new Vector3(-0.78f, 0.24f, 0f),
                new Vector3(0.1f, 0.48f, 0.55f), BenchLeg, 0.5f, 0.35f);
            BuildKit.Cube("LegR", b, new Vector3(0.78f, 0.24f, 0f),
                new Vector3(0.1f, 0.48f, 0.55f), BenchLeg, 0.5f, 0.35f);
            // armrests capping the leg frames
            BuildKit.Cube("ArmL", b, new Vector3(-0.78f, 0.56f, 0.05f),
                new Vector3(0.1f, 0.06f, 0.45f), BenchLeg, 0.5f, 0.35f);
            BuildKit.Cube("ArmR", b, new Vector3(0.78f, 0.56f, 0.05f),
                new Vector3(0.1f, 0.06f, 0.45f), BenchLeg, 0.5f, 0.35f);
            // under-seat brace tying the two legs together
            BuildKit.Cube("Brace", b, new Vector3(0f, 0.12f, 0f),
                new Vector3(1.7f, 0.06f, 0.08f), BenchLeg, 0.5f, 0.35f);

            // --- on ~1/3 of benches, seat an idle civilian ---
            if (id % 3 == 0)
            {
                // Seat point: slightly off-centre on the bench, at the seat surface,
                // converted to world space because BuildKit.Civ takes a WORLD pos.
                Vector3 seatLocal = new Vector3(Random.Range(-0.45f, 0.45f), 0f, 0f);
                Vector3 seatWorld = b.TransformPoint(seatLocal);
                var c = BuildKit.Civ($"BenchSitter_{id}",
                    new Vector3(seatWorld.x, 0f, seatWorld.z),
                    Civilian.Mode.Idle,
                    new Color(Random.value, Random.value, Random.value),
                    new Color(0.20f, 0.22f, 0.28f));
                // Face the same way the bench does (toward the road).
                if (c != null) c.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            }
        }
    }
}
