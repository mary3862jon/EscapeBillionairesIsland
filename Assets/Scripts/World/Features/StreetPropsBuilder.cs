using UnityEngine;
using System.Collections.Generic;
namespace Spoonacci
{
    public static class StreetPropsBuilder
    {
        // building footprints to avoid (x, z, clearance radius)
        private static readonly Vector3[] _avoid = new Vector3[]
        {
            new Vector3(12, 8, 8),    // Salon
            new Vector3(20, 6, 6),    // Skin Kiosk
            new Vector3(22, -10, 7),  // Tiki Bar
            new Vector3(0, 14, 7),    // Deck chairs
            new Vector3(-15, -2, 14), // Central Pool
            new Vector3(-22, 22, 14), // Shady Alley
            new Vector3(0, 0, 5),     // spawn
            new Vector3(6, -6, 7),    // fountain (reserve)
        };

        private static bool Blocked(float x, float z, float pad = 5f)
        {
            foreach (var a in _avoid)
            {
                float dx = x - a.x, dz = z - a.y;
                if (dx * dx + dz * dz < (a.z + pad) * (a.z + pad)) return true;
            }
            if (x < -85f || x > 85f || z < -85f || z > 85f) return true;
            return false;
        }

        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("StreetProps", Vector3.zero).transform;
            hub.SetParent(root, true);

            StreetLamps(hub);
            Benches(hub);
            TrashCans(hub);
            Planters(hub);
            Fountain(hub);
            Stalls(hub);
            Billboards(hub);
        }

        // ---------------------------------------------------------------
        private static readonly Color PoleCol = new Color(0.18f, 0.19f, 0.22f);
        private static readonly Color WarmGlass = new Color(1f, 0.93f, 0.72f);

        private static void StreetLamps(Transform hub)
        {
            // hand-placed positions roughly along central walkways
            Vector3[] spots =
            {
                new Vector3(8, 0, -2), new Vector3(-6, 0, 6), new Vector3(14, 0, -16),
                new Vector3(-2, 0, 20), new Vector3(26, 0, 2), new Vector3(-30, 0, 8),
                new Vector3(4, 0, 28), new Vector3(-12, 0, -22), new Vector3(34, 0, -8),
                new Vector3(18, 0, 22), new Vector3(-36, 0, -14), new Vector3(40, 0, 14),
            };
            int idx = 0;
            foreach (var s in spots)
            {
                if (Blocked(s.x, s.z)) { idx++; continue; }
                var pole = BuildKit.Cylinder($"LampPole_{idx}", hub,
                    new Vector3(s.x, 2.4f, s.z), new Vector3(0.18f, 2.4f, 0.18f),
                    PoleCol, 0.7f, 0.5f);
                // small base
                BuildKit.Cylinder($"LampBase_{idx}", hub,
                    new Vector3(s.x, 0.12f, s.z), new Vector3(0.45f, 0.12f, 0.45f),
                    PoleCol, 0.6f, 0.4f);
                // arm
                BuildKit.Cube($"LampArm_{idx}", hub,
                    new Vector3(s.x + 0.35f, 4.7f, s.z), new Vector3(0.7f, 0.1f, 0.1f),
                    PoleCol, 0.7f, 0.5f);
                // lamp head
                var head = BuildKit.Cube($"LampHead_{idx}", hub,
                    new Vector3(s.x + 0.7f, 4.55f, s.z), new Vector3(0.4f, 0.3f, 0.4f),
                    WarmGlass, 0.1f, 0.85f);
                Object.Destroy(head.GetComponent<Collider>());

                // real light on roughly half of them
                if (idx % 2 == 0)
                {
                    var lgo = new GameObject($"LampLight_{idx}");
                    lgo.transform.SetParent(hub, false);
                    lgo.transform.position = new Vector3(s.x + 0.7f, 4.4f, s.z);
                    var l = lgo.AddComponent<Light>();
                    l.type = LightType.Point;
                    l.color = new Color(1f, 0.88f, 0.6f);
                    l.intensity = 2.2f;
                    l.range = 9f;
                }
                idx++;
            }
        }

        // ---------------------------------------------------------------
        private static readonly Color WoodSlat = new Color(0.55f, 0.36f, 0.18f);
        private static readonly Color BenchLeg = new Color(0.22f, 0.22f, 0.24f);

        private static void Benches(Transform hub)
        {
            int made = 0, guard = 0;
            while (made < 10 && guard < 200)
            {
                guard++;
                float x = Random.Range(-70f, 70f);
                float z = Random.Range(-70f, 70f);
                if (Blocked(x, z)) continue;
                float rot = Random.Range(0f, 360f);
                MakeBench(hub, made, x, z, rot);
                made++;
            }
        }

        private static void MakeBench(Transform hub, int id, float x, float z, float rot)
        {
            var b = BuildKit.Root($"Bench_{id}", new Vector3(x, 0, z)).transform;
            b.SetParent(hub, true);
            b.localRotation = Quaternion.Euler(0, rot, 0);
            // seat slats
            for (int i = 0; i < 3; i++)
            {
                BuildKit.Cube($"Slat_{i}", b,
                    new Vector3(0, 0.5f, -0.18f + i * 0.18f),
                    new Vector3(1.8f, 0.06f, 0.15f), WoodSlat, 0f, 0.3f);
            }
            // back slats
            for (int i = 0; i < 2; i++)
            {
                BuildKit.Cube($"Back_{i}", b,
                    new Vector3(0, 0.78f + i * 0.22f, -0.28f),
                    new Vector3(1.8f, 0.06f, 0.15f), WoodSlat, 0f, 0.3f);
            }
            // legs
            BuildKit.Cube("LegL", b, new Vector3(-0.8f, 0.25f, 0), new Vector3(0.1f, 0.5f, 0.5f), BenchLeg, 0.4f, 0.3f);
            BuildKit.Cube("LegR", b, new Vector3(0.8f, 0.25f, 0), new Vector3(0.1f, 0.5f, 0.5f), BenchLeg, 0.4f, 0.3f);

            // sometimes a sitting civ
            if (id % 3 == 0)
            {
                var seat = b.TransformPoint(new Vector3(Random.Range(-0.4f, 0.4f), 0f, 0f));
                var civ = BuildKit.Civ($"BenchSitter_{id}", new Vector3(seat.x, 0, seat.z),
                    Civilian.Mode.Idle,
                    new Color(Random.value, Random.value, Random.value),
                    new Color(0.2f, 0.2f, 0.25f));
            }
        }

        // ---------------------------------------------------------------
        private static void TrashCans(Transform hub)
        {
            Color[] cols = {
                new Color(0.15f, 0.4f, 0.2f), new Color(0.3f, 0.32f, 0.34f),
                new Color(0.5f, 0.12f, 0.1f)
            };
            int made = 0, guard = 0;
            while (made < 10 && guard < 200)
            {
                guard++;
                float x = Random.Range(-72f, 72f);
                float z = Random.Range(-72f, 72f);
                if (Blocked(x, z, 4f)) continue;
                var c = cols[made % cols.Length];
                BuildKit.Cylinder($"TrashCan_{made}", hub,
                    new Vector3(x, 0.45f, z), new Vector3(0.4f, 0.45f, 0.4f), c, 0.5f, 0.4f);
                // lid
                var lid = BuildKit.Cylinder($"TrashLid_{made}", hub,
                    new Vector3(x, 0.94f, z), new Vector3(0.44f, 0.05f, 0.44f),
                    c * 0.7f, 0.5f, 0.5f);
                Object.Destroy(lid.GetComponent<Collider>());
                made++;
            }
        }

        // ---------------------------------------------------------------
        private static void Planters(Transform hub)
        {
            Color shrub = new Color(0.15f, 0.45f, 0.18f);
            int made = 0, guard = 0;
            while (made < 10 && guard < 200)
            {
                guard++;
                float x = Random.Range(-74f, 74f);
                float z = Random.Range(-74f, 74f);
                if (Blocked(x, z, 4.5f)) continue;
                bool marble = made % 2 == 0;
                var tex = marble ? ProceduralTextures.Marble : ProceduralTextures.Stone;
                var tint = marble ? new Color(0.92f, 0.9f, 0.86f) : new Color(0.6f, 0.6f, 0.62f);
                BuildKit.CubeTex($"Planter_{made}", hub,
                    new Vector3(x, 0.4f, z), new Vector3(1.0f, 0.8f, 1.0f),
                    tex, tint, 0.1f, 0.3f, new Vector2(1, 1));
                // shrub on top (pass-through)
                var s = BuildKit.Sphere($"Shrub_{made}", hub,
                    new Vector3(x, 1.15f, z), new Vector3(0.95f, 0.85f, 0.95f),
                    shrub * Random.Range(0.85f, 1.1f), 0f, 0.2f);
                // a smaller bump for variety
                var s2 = BuildKit.Sphere($"ShrubTop_{made}", hub,
                    new Vector3(x + Random.Range(-0.2f, 0.2f), 1.5f, z + Random.Range(-0.2f, 0.2f)),
                    new Vector3(0.55f, 0.5f, 0.55f), shrub * 1.05f, 0f, 0.2f);
                made++;
            }
        }

        // ---------------------------------------------------------------
        private static void Fountain(Transform hub)
        {
            Vector3 c = new Vector3(6f, 0f, -6f);
            Color stone = new Color(0.78f, 0.76f, 0.72f);
            var f = BuildKit.Root("Fountain", c).transform;
            f.SetParent(hub, true);

            // tier 1 (wide basin wall)
            BuildKit.Cylinder("Basin1", f, new Vector3(0, 0.3f, 0), new Vector3(3.0f, 0.3f, 3.0f), stone, 0.1f, 0.4f);
            // tier 2
            BuildKit.Cylinder("Basin2", f, new Vector3(0, 0.85f, 0), new Vector3(1.8f, 0.3f, 1.8f), stone, 0.1f, 0.4f);
            // tier 3 / pedestal
            BuildKit.Cylinder("Pedestal", f, new Vector3(0, 1.3f, 0), new Vector3(0.5f, 0.5f, 0.5f), stone, 0.1f, 0.4f);

            // water discs (collider removed)
            var w1 = BuildKit.Cylinder("Water1", f, new Vector3(0, 0.55f, 0), new Vector3(2.7f, 0.04f, 2.7f),
                Color.white, 0f, 0.9f);
            ApplyWater(w1);
            var w2 = BuildKit.Cylinder("Water2", f, new Vector3(0, 1.05f, 0), new Vector3(1.5f, 0.04f, 1.5f),
                Color.white, 0f, 0.9f);
            ApplyWater(w2);

            // spray spheres
            Color spray = new Color(0.7f, 0.85f, 1f, 1f);
            for (int i = 0; i < 5; i++)
            {
                float h = 1.7f + i * 0.18f;
                float sc = 0.22f - i * 0.025f;
                BuildKit.Sphere($"Spray_{i}", f, new Vector3(0, h, 0),
                    new Vector3(sc, sc, sc), spray, 0f, 0.95f);
            }
            // a couple of side jets
            BuildKit.Sphere("SpraySide1", f, new Vector3(0.3f, 1.6f, 0.1f), new Vector3(0.15f, 0.15f, 0.15f), spray, 0f, 0.95f);
            BuildKit.Sphere("SpraySide2", f, new Vector3(-0.25f, 1.55f, -0.15f), new Vector3(0.13f, 0.13f, 0.13f), spray, 0f, 0.95f);
        }

        private static void ApplyWater(GameObject go)
        {
            Object.Destroy(go.GetComponent<Collider>());
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
                mr.sharedMaterial = ShaderCache.MakeTextured(
                    ProceduralTextures.Water, new Color(0.6f, 0.85f, 1f, 1f), 0f, 0.92f, new Vector2(2, 2));
        }

        // ---------------------------------------------------------------
        private static void Stalls(Transform hub)
        {
            MakeStall(hub, 0, new Vector3(-8f, 0f, -14f), 35f, "SMOOTHIES",
                new Color(0.95f, 0.5f, 0.2f), new Color(0.95f, 0.95f, 0.5f));
            MakeStall(hub, 1, new Vector3(30f, 0f, 16f), -50f, "ARTISAN ICE",
                new Color(0.2f, 0.6f, 0.85f), new Color(0.9f, 0.95f, 0.97f));
        }

        private static void MakeStall(Transform hub, int id, Vector3 pos, float rot, string menu,
            Color awningA, Color awningB)
        {
            if (Blocked(pos.x, pos.z, 3f)) pos = new Vector3(pos.x + 8f, 0, pos.z + 8f);
            var st = BuildKit.Root($"Stall_{id}", pos).transform;
            st.SetParent(hub, true);
            st.localRotation = Quaternion.Euler(0, rot, 0);

            // counter / kiosk body (wood textured)
            BuildKit.CubeTex($"StallBody_{id}", st, new Vector3(0, 0.6f, 0),
                new Vector3(2.4f, 1.2f, 1.2f), ProceduralTextures.Wood,
                new Color(0.7f, 0.5f, 0.32f), 0f, 0.3f, new Vector2(2, 1));
            // counter top
            BuildKit.Cube($"StallTop_{id}", st, new Vector3(0, 1.25f, 0.1f),
                new Vector3(2.5f, 0.1f, 1.4f), new Color(0.85f, 0.82f, 0.78f), 0.1f, 0.5f);
            // back posts
            BuildKit.Cube($"PostL_{id}", st, new Vector3(-1.1f, 1.6f, -0.5f), new Vector3(0.12f, 2.0f, 0.12f), WoodSlat, 0f, 0.3f);
            BuildKit.Cube($"PostR_{id}", st, new Vector3(1.1f, 1.6f, -0.5f), new Vector3(0.12f, 2.0f, 0.12f), WoodSlat, 0f, 0.3f);

            // striped awning (thin tilted cubes, pass-through)
            for (int i = 0; i < 6; i++)
            {
                var col = (i % 2 == 0) ? awningA : awningB;
                var aw = BuildKit.Cube($"Awning_{id}_{i}", st,
                    new Vector3(-1.0f + i * 0.4f, 2.5f, 0.45f),
                    new Vector3(0.4f, 0.04f, 1.6f), col, 0f, 0.3f);
                aw.transform.localRotation = Quaternion.Euler(20f, 0, 0);
                Object.Destroy(aw.GetComponent<Collider>());
            }

            // vendor civ behind counter
            var vp = st.TransformPoint(new Vector3(0, 0, -0.7f));
            BuildKit.Civ($"Vendor_{id}", new Vector3(vp.x, 0, vp.z), Civilian.Mode.Idle,
                new Color(0.9f, 0.9f, 0.95f), new Color(0.2f, 0.25f, 0.3f));

            // menu label above counter
            var anchor = BuildKit.Cube($"MenuAnchor_{id}", st, new Vector3(0, 2.2f, -0.5f),
                new Vector3(0.05f, 0.05f, 0.05f), new Color(0, 0, 0, 0), 0f, 0f);
            Object.Destroy(anchor.GetComponent<Collider>());
            BuildKit.Label(anchor, menu, new Color(1f, 0.95f, 0.7f), 26, new Vector3(0, 0.4f, 0));
        }

        // ---------------------------------------------------------------
        private static void Billboards(Transform hub)
        {
            var signs = new List<(Vector3 pos, float rot, string txt, Color col)>
            {
                (new Vector3(-4f, 0, 12f), 20f, "LIVE LAUGH LAUNDER", new Color(1f, 0.9f, 0.5f)),
                (new Vector3(36f, 0, -4f), -40f, "BILLIONAIRES ONLY", new Color(1f, 0.6f, 0.6f)),
                (new Vector3(-34f, 0, -6f), 70f, "TAX IS FOR THE POOR", new Color(0.7f, 1f, 0.7f)),
                (new Vector3(16f, 0, 30f), 110f, "NO WAGES BEYOND THIS POINT", new Color(0.7f, 0.85f, 1f)),
                (new Vector3(-40f, 0, 20f), -25f, "YACHT PARKING FULL", new Color(1f, 0.8f, 0.95f)),
            };

            int id = 0;
            foreach (var s in signs)
            {
                Vector3 p = s.pos;
                if (Blocked(p.x, p.z, 4f)) { p += new Vector3(7f, 0, 7f); if (Blocked(p.x, p.z, 3f)) { id++; continue; } }
                var bb = BuildKit.Root($"Billboard_{id}", p).transform;
                bb.SetParent(hub, true);
                bb.localRotation = Quaternion.Euler(0, s.rot, 0);

                // post
                BuildKit.Cube($"BBPost_{id}", bb, new Vector3(0, 1.6f, 0),
                    new Vector3(0.18f, 3.2f, 0.18f), PoleCol, 0.6f, 0.4f);
                // board panel
                BuildKit.Cube($"BBPanel_{id}", bb, new Vector3(0, 3.4f, 0),
                    new Vector3(3.6f, 1.4f, 0.12f), new Color(0.12f, 0.13f, 0.16f), 0.2f, 0.3f);
                // glowing accent strip (pass-through)
                var strip = BuildKit.Cube($"BBStrip_{id}", bb, new Vector3(0, 2.6f, 0.07f),
                    new Vector3(3.6f, 0.08f, 0.04f), s.col, 0f, 0.8f);
                Object.Destroy(strip.GetComponent<Collider>());

                // text label on the panel
                var anchor = BuildKit.Cube($"BBAnchor_{id}", bb, new Vector3(0, 3.4f, 0.1f),
                    new Vector3(0.05f, 0.05f, 0.05f), new Color(0, 0, 0, 0), 0f, 0f);
                Object.Destroy(anchor.GetComponent<Collider>());
                BuildKit.Label(anchor, s.txt, s.col, 22, Vector3.zero);
                id++;
            }
        }
    }
}
