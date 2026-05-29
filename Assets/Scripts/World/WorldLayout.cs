using System.Collections.Generic;
using UnityEngine;

namespace Spoonacci
{
    // SINGLE SOURCE OF TRUTH for island spatial layout.
    // Every feature builder MUST consult this before placing anything, so props never
    // land on roads, in the pool, inside buildings, or out of bounds. This is what
    // prevents "bench in the middle of the road" style mistakes.
    //
    // Coordinates are XZ (Vector2). Y is always built upward from the ground (y=0).
    public static class WorldLayout
    {
        // ---- key anchors (XZ) ------------------------------------------------
        public static readonly Vector2 Origin = new Vector2(0f, 0f);
        public static readonly Vector2 Beff   = new Vector2(-80f, -80f);
        public static readonly Vector2 Vault  = new Vector2( 80f, -80f);
        public static readonly Vector2 Zuck   = new Vector2(-80f,  80f);
        public static readonly Vector2 Magnus = new Vector2( 50f,  30f);
        public static readonly Vector2 Pool   = new Vector2(-15f,  -2f);
        public static readonly Vector2 Alley  = new Vector2(-22f,  22f);
        public static readonly Vector2 Fountain = new Vector2(6f, -6f);

        public const float Bounds   = 90f;   // playable half-extent; barrier sits at ~92
        public const float PlazaHalf = 9f;    // hub plaza half-size (square at Origin)

        // ---- road / path network (centerlines) ------------------------------
        public struct Seg { public Vector2 a, b; public float half; }

        // half = corridor half-width INCLUDING flanking sidewalks (keep-out radius).
        public static readonly Seg[] Roads =
        {
            new Seg { a = Origin, b = Beff,   half = 5.0f },
            new Seg { a = Origin, b = Vault,  half = 5.0f },
            new Seg { a = Origin, b = Zuck,   half = 5.0f },
            new Seg { a = Origin, b = Magnus, half = 5.0f },
            new Seg { a = Origin, b = Pool,   half = 2.4f }, // spur
            new Seg { a = Origin, b = Alley,  half = 2.4f }, // spur
        };

        // The four MAIN roads only (for sidewalk furniture placement).
        public static int MainRoadCount => 4;

        // Bike lane route (a gentle east-west lane south of the hub). Cyclists ride this.
        public static readonly Vector2 BikeLaneA = new Vector2(-62f, -36f);
        public static readonly Vector2 BikeLaneB = new Vector2( 62f, -36f);

        // ---- building / zone footprints to keep clear (x, z, radius) ---------
        public static readonly Vector3[] Footprints =
        {
            new Vector3(12f,   8f,  8f),  // Salon
            new Vector3(20f,   6f,  6f),  // Skin Kiosk
            new Vector3(22f, -10f,  7f),  // Tiki Bar
            new Vector3(0f,   14f,  7f),  // Deck chairs
            new Vector3(-15f, -2f, 15f),  // Central Pool (party zone)
            new Vector3(-22f, 22f, 14f),  // Shady Alley
            new Vector3(6f,   -6f,  7f),  // Fountain
            new Vector3(0f,    0f,  6f),  // player spawn
            new Vector3(-80f, -80f, 30f), // Beff yacht zone
            new Vector3( 80f, -80f, 28f), // Crypto vault zone
            new Vector3(-80f,  80f, 28f), // Zuck lab zone
            new Vector3( 50f,  30f, 22f), // Magnus launch-complex arena
        };

        // ---- geometry helpers -----------------------------------------------
        public static float DistToSeg(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float len2 = ab.sqrMagnitude;
            if (len2 < 1e-5f) return Vector2.Distance(p, a);
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / len2);
            return Vector2.Distance(p, a + ab * t);
        }

        // On any road/spur corridor (optionally widened by pad), or inside the hub plaza.
        public static bool OnRoad(Vector2 p, float pad = 0f)
        {
            if (Mathf.Abs(p.x - Origin.x) <= PlazaHalf + pad && Mathf.Abs(p.y - Origin.y) <= PlazaHalf + pad)
                return true;
            foreach (var s in Roads)
                if (DistToSeg(p, s.a, s.b) <= s.half + pad) return true;
            return false;
        }

        public static bool InFootprint(Vector2 p, float pad = 0f)
        {
            foreach (var f in Footprints)
            {
                float dx = p.x - f.x, dz = p.y - f.y, r = f.z + pad;
                if (dx * dx + dz * dz < r * r) return true;
            }
            return false;
        }

        public static bool OutOfBounds(Vector2 p, float pad = 0f)
            => Mathf.Abs(p.x) > Bounds - pad || Mathf.Abs(p.y) > Bounds - pad;

        // The master test: true if NOTHING new should be placed here.
        public static bool Blocked(Vector2 p, float pad = 1f)
            => OutOfBounds(p, 2f) || OnRoad(p, pad) || InFootprint(p, pad);

        // Random open spot that passes Blocked(). Returns false if it can't find one.
        public static bool TryOpenSpot(out Vector3 pos, float pad = 1.5f, float min = -85f, float max = 85f, int tries = 40)
        {
            for (int i = 0; i < tries; i++)
            {
                var p = new Vector2(Random.Range(min, max), Random.Range(min, max));
                if (!Blocked(p, pad)) { pos = new Vector3(p.x, 0f, p.y); return true; }
            }
            pos = Vector3.zero;
            return false;
        }

        // ---- sidewalk furniture placement -----------------------------------
        // A spot just OUTSIDE a main road's sidewalk, with a yaw facing the road.
        public struct Slot { public Vector3 pos; public float yaw; }

        // Evenly spaced slots flanking the four main roads, on both sides, skipping the
        // plaza and the far zone ends. Use for benches, lamps, billboards, trash cans.
        // sideOffset is distance from the road centerline (≈ road.half + a bit).
        public static List<Slot> SidewalkSlots(float spacing = 12f, float sideOffset = 6.5f)
        {
            var slots = new List<Slot>();
            for (int r = 0; r < MainRoadCount; r++)
            {
                var s = Roads[r];
                Vector2 dir = (s.b - s.a).normalized;
                Vector2 perp = new Vector2(-dir.y, dir.x);
                float len = Vector2.Distance(s.a, s.b);
                for (float t = PlazaHalf + 6f; t < len - 18f; t += spacing)
                {
                    Vector2 on = s.a + dir * t;
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Vector2 p = on + perp * (sideOffset * side);
                        if (InFootprint(p, 1f) || OutOfBounds(p, 2f)) continue;
                        // face the road: forward (+Z) points back toward the centerline
                        Vector2 toRoad = (on - p).normalized;
                        float yaw = Mathf.Atan2(toRoad.x, toRoad.y) * Mathf.Rad2Deg;
                        slots.Add(new Slot { pos = new Vector3(p.x, 0f, p.y), yaw = yaw });
                    }
                }
            }
            return slots;
        }
    }
}
