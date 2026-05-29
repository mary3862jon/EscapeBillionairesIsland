using UnityEngine;
using System.Collections.Generic;

namespace Spoonacci
{
    // Decorative planters: square stone/marble boxes (solid) with soil and a
    // green shrub / small flower cluster on top (pass-through). They frame the
    // hub plaza corners and dot the sidewalks so the world feels manicured.
    public static class PlantersBuilder
    {
        private static readonly Color SoilCol   = new Color(0.20f, 0.13f, 0.08f);
        private static readonly Color ShrubCol  = new Color(0.16f, 0.46f, 0.19f);
        private static readonly Color MarbleTint = new Color(0.92f, 0.90f, 0.86f);
        private static readonly Color StoneTint  = new Color(0.62f, 0.61f, 0.60f);

        // little flower palette for the cluster variant
        private static readonly Color[] FlowerCols =
        {
            new Color(0.95f, 0.30f, 0.45f), // pink
            new Color(0.98f, 0.80f, 0.25f), // gold
            new Color(0.75f, 0.45f, 0.95f), // lilac
            new Color(0.98f, 0.55f, 0.20f), // orange
            new Color(0.95f, 0.95f, 0.98f), // white
        };

        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("Planters", Vector3.zero).transform;
            hub.SetParent(root, true);

            int id = 0;

            // -------- Plaza-framing planters at the corners/edges --------
            // PlazaHalf ~9, so push out to ~10.5 to hug the plaza edge.
            float r = WorldLayout.PlazaHalf + 1.5f;
            Vector2 plazaCenter = WorldLayout.Origin;
            Vector2[] plazaSpots =
            {
                new Vector2( r,  r), new Vector2(-r,  r),
                new Vector2( r, -r), new Vector2(-r, -r), // 4 corners
                new Vector2( r,  0f), new Vector2(-r,  0f),
                new Vector2( 0f,  r), new Vector2( 0f, -r), // 4 edge midpoints
            };

            foreach (var off in plazaSpots)
            {
                Vector2 p = plazaCenter + off;
                // HARD RULE: never let a plaza planter sit on a road, inside a
                // reserved footprint (Salon/Deck chairs/Fountain/spawn), or OOB.
                if (WorldLayout.OnRoad(p)) continue;
                if (WorldLayout.InFootprint(p, 0.5f)) continue;
                if (WorldLayout.OutOfBounds(p, 2f)) continue;
                MakePlanter(hub, id++, new Vector3(p.x, 0f, p.y), 0f);
            }

            // -------- Sidewalk planters along the road network --------
            var slots = WorldLayout.SidewalkSlots(16f, 6.5f);
            // Spread them out so we don't crowd every slot; aim for ~6 more,
            // giving a healthy total of ~12+ planters across the map.
            int sidewalkTarget = 6;
            int placed = 0;
            // step through slots so they are distributed, not clumped
            int step = Mathf.Max(1, slots.Count / Mathf.Max(1, sidewalkTarget));
            for (int i = 0; i < slots.Count && placed < sidewalkTarget; i += step)
            {
                var slot = slots[i];
                Vector2 p2 = new Vector2(slot.pos.x, slot.pos.z);
                if (WorldLayout.Blocked(p2, 1.2f)) continue;
                MakePlanter(hub, id++, slot.pos, slot.yaw);
                placed++;
            }
        }

        // ----------------------------------------------------------------
        private static void MakePlanter(Transform hub, int id, Vector3 pos, float yaw)
        {
            var p = BuildKit.Root($"Planter_{id}", new Vector3(pos.x, 0f, pos.z)).transform;
            p.SetParent(hub, true);
            p.localRotation = Quaternion.Euler(0f, yaw, 0f);

            bool marble = (id % 2 == 0);
            var tex  = marble ? ProceduralTextures.Marble : ProceduralTextures.Stone;
            var tint = marble ? MarbleTint : StoneTint;

            // Box body (square, KEEP collider — solid obstacle).
            float w = 1.1f;          // half-ish footprint width
            float h = 0.45f;         // wall height
            BuildKit.CubeTex($"Box_{id}", p,
                new Vector3(0f, h, 0f),
                new Vector3(w, h * 2f, w),
                tex, tint, 0.1f, 0.35f, new Vector2(1f, 1f));

            // A slim rim cap for a finished look (pass-through, thin).
            var rim = BuildKit.CubeTex($"Rim_{id}", p,
                new Vector3(0f, h * 2f + 0.04f, 0f),
                new Vector3(w + 0.08f, 0.08f, w + 0.08f),
                tex, tint * 1.05f, 0.1f, 0.4f, new Vector2(1f, 1f));
            Object.Destroy(rim.GetComponent<Collider>());

            // Soil fill, just below the rim (pass-through).
            var soil = BuildKit.Cube($"Soil_{id}", p,
                new Vector3(0f, h * 2f - 0.05f, 0f),
                new Vector3(w - 0.12f, 0.12f, w - 0.12f),
                SoilCol, 0f, 0.15f);
            Object.Destroy(soil.GetComponent<Collider>());

            float topY = h * 2f + 0.05f; // local y where greenery starts

            // Alternate between a leafy shrub and a flower cluster.
            if (id % 3 == 2)
                FlowerCluster(p, id, topY);
            else
                Shrub(p, id, topY);
        }

        // Rounded leafy shrub: a main blob plus a couple of smaller bumps.
        private static void Shrub(Transform p, int id, float topY)
        {
            Color baseCol = ShrubCol * Random.Range(0.85f, 1.12f);
            var main = BuildKit.Sphere($"Shrub_{id}", p,
                new Vector3(0f, topY + 0.5f, 0f),
                new Vector3(1.0f, 0.95f, 1.0f), baseCol, 0f, 0.2f);
            Object.Destroy(main.GetComponent<Collider>());

            for (int i = 0; i < 3; i++)
            {
                float ox = Random.Range(-0.32f, 0.32f);
                float oz = Random.Range(-0.32f, 0.32f);
                float sc = Random.Range(0.45f, 0.7f);
                var bump = BuildKit.Sphere($"ShrubBump_{id}_{i}", p,
                    new Vector3(ox, topY + 0.7f + Random.Range(0f, 0.25f), oz),
                    new Vector3(sc, sc, sc),
                    ShrubCol * Random.Range(0.95f, 1.15f), 0f, 0.2f);
                Object.Destroy(bump.GetComponent<Collider>());
            }
        }

        // Small flower cluster: a low green mound + a few colorful blossom dots.
        private static void FlowerCluster(Transform p, int id, float topY)
        {
            // low foliage mound
            var mound = BuildKit.Sphere($"Mound_{id}", p,
                new Vector3(0f, topY + 0.18f, 0f),
                new Vector3(0.9f, 0.45f, 0.9f),
                ShrubCol * 0.95f, 0f, 0.2f);
            Object.Destroy(mound.GetComponent<Collider>());

            int n = Random.Range(5, 8);
            for (int i = 0; i < n; i++)
            {
                float ang = (360f / n) * i + Random.Range(-12f, 12f);
                float rad = Random.Range(0.15f, 0.42f);
                float ox = Mathf.Cos(ang * Mathf.Deg2Rad) * rad;
                float oz = Mathf.Sin(ang * Mathf.Deg2Rad) * rad;
                var col = FlowerCols[Random.Range(0, FlowerCols.Length)];

                // tiny stem
                var stem = BuildKit.Cylinder($"Stem_{id}_{i}", p,
                    new Vector3(ox, topY + 0.22f, oz),
                    new Vector3(0.03f, 0.18f, 0.03f),
                    new Color(0.18f, 0.4f, 0.18f), 0f, 0.2f);
                Object.Destroy(stem.GetComponent<Collider>());

                // blossom head
                var head = BuildKit.Sphere($"Bloom_{id}_{i}", p,
                    new Vector3(ox, topY + 0.42f + Random.Range(0f, 0.08f), oz),
                    new Vector3(0.16f, 0.16f, 0.16f), col, 0f, 0.3f);
                Object.Destroy(head.GetComponent<Collider>());
            }
        }
    }
}
