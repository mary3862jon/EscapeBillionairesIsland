using UnityEngine;
namespace Spoonacci
{
    public static class PoolBuilder
    {
        public static void Build(Transform root)
        {
            // Complex centered at world (-15, 0, -2)
            Vector3 C = new Vector3(-15f, 0f, -2f);
            var hub = BuildKit.Root("PoolComplex", C);
            hub.transform.SetParent(root, true);
            Transform h = hub.transform;

            // ---- Color palette (tropical, rich) ----
            Color marble = new Color(0.93f, 0.92f, 0.88f);
            Color stoneTint = new Color(0.85f, 0.84f, 0.80f);
            Color waterBlue = new Color(0.20f, 0.70f, 0.95f);
            Color deepWater = new Color(0.10f, 0.55f, 0.85f);
            Color teak = new Color(0.55f, 0.36f, 0.18f);
            Color chrome = new Color(0.85f, 0.86f, 0.88f);
            Color cushionWhite = new Color(0.96f, 0.96f, 0.93f);

            // ================= POOL DECK (marble) =================
            // Big raised deck platform, local coords (parent = hub)
            var deck = BuildKit.CubeTex("Deck", h, new Vector3(0f, 0.06f, 0f),
                new Vector3(40f, 0.12f, 30f), ProceduralTextures.Marble, marble, 0.05f, 0.55f, new Vector2(8f, 6f));
            // Inlay stone border ring (4 thin slabs) as flat decals
            DeckBorder(h, stoneTint);

            // ================= POOL BASIN (sunken look via wall lip) =================
            // L-shaped infinity pool: main rectangle + secondary arm
            // Pool lip walls (low marble curbs the player bumps into)
            float lipY = 0.30f;
            // main basin footprint roughly x[-12..8], z[-9..7]; arm x[-12..-2], z[7..13]
            PoolLip(h, new Vector3(-2f, lipY, -1f), new Vector3(22f, 0.6f, 18f), marble);   // main
            PoolLip(h, new Vector3(-7f, lipY, 10f), new Vector3(12f, 0.6f, 8f), marble);    // arm

            // Water surface for main basin (flat, no collider)
            Water(h, new Vector3(-2f, 0.16f, -1f), new Vector3(20.4f, 0.06f, 16.4f), waterBlue);
            Water(h, new Vector3(-7f, 0.18f, 10f), new Vector3(10.4f, 0.05f, 6.4f), deepWater);

            // Infinity edge glint strip on far side
            var edge = BuildKit.CubeTex("InfinityEdge", h, new Vector3(-2f, 0.20f, -10.3f),
                new Vector3(20.6f, 0.04f, 0.8f), ProceduralTextures.Water, new Color(0.6f, 0.9f, 1f), 0.1f, 0.9f, new Vector2(6f, 1f));
            Object.Destroy(edge.GetComponent<Collider>());

            // Tiled pool floor underneath water (visual depth)
            var floor = BuildKit.CubeTex("PoolFloor", h, new Vector3(-2f, 0.02f, -1f),
                new Vector3(20.4f, 0.04f, 16.4f), ProceduralTextures.Stone, new Color(0.4f, 0.75f, 0.9f), 0.0f, 0.6f, new Vector2(10f, 8f));
            Object.Destroy(floor.GetComponent<Collider>());

            // ================= DIVING BOARD =================
            DivingBoard(h, new Vector3(7.5f, 0f, -7f), teak, chrome);

            // ================= WATER SLIDE =================
            WaterSlide(h, new Vector3(-13f, 0f, 11f));

            // ================= POOL LADDER RAILS =================
            LadderRails(h, new Vector3(2f, 0f, 7.2f), chrome);
            LadderRails(h, new Vector3(-9f, 0f, -8.8f), chrome);

            // ================= SUN LOUNGERS + PARASOLS =================
            Color[] parasolCols = {
                new Color(0.95f, 0.30f, 0.30f), new Color(0.30f, 0.70f, 0.95f),
                new Color(0.98f, 0.78f, 0.20f), new Color(0.40f, 0.85f, 0.50f),
                new Color(0.85f, 0.45f, 0.90f), new Color(0.95f, 0.55f, 0.20f)
            };
            Vector3[] loungerSpots = {
                new Vector3(12f, 0f, 8f), new Vector3(12f, 0f, 3f), new Vector3(12f, 0f, -2f),
                new Vector3(-16f, 0f, -6f), new Vector3(-16f, 0f, -1f), new Vector3(-16f, 0f, 4f)
            };
            for (int i = 0; i < loungerSpots.Length; i++)
            {
                float face = loungerSpots[i].x > 0 ? -1f : 1f; // face the pool
                SunLounger(h, loungerSpots[i], parasolCols[i % parasolCols.Length], cushionWhite, teak, face);
            }

            // ================= INFLATABLE FLOATS on water =================
            Inflatable(h, new Vector3(-4f, 0.32f, 2f), new Color(0.98f, 0.85f, 0.30f), true);  // donut-ish
            Inflatable(h, new Vector3(3f, 0.30f, -4f), new Color(0.95f, 0.40f, 0.55f), false);
            Inflatable(h, new Vector3(-8f, 0.34f, 9f), new Color(0.30f, 0.90f, 0.85f), true);

            // ================= POOLSIDE BAR NOOK =================
            PoolBar(h, new Vector3(16f, 0f, -10f), teak, marble, chrome);

            // ================= PALM / PLANTER ACCENTS =================
            Planter(h, new Vector3(-18f, 0f, -10f));
            Planter(h, new Vector3(16f, 0f, 12f));
            Planter(h, new Vector3(-18f, 0f, 12f));
            Planter(h, new Vector3(18f, 0f, 5f));

            // ================= "POOL" SIGN on a post =================
            var post = BuildKit.Cylinder("PoolSignPost", h, new Vector3(-19f, 1.6f, 2f),
                new Vector3(0.18f, 1.6f, 0.18f), chrome, 0.7f, 0.8f);
            var signPlate = BuildKit.Cube("PoolSignPlate", h, new Vector3(-19f, 3.4f, 2f),
                new Vector3(2.6f, 1.0f, 0.12f), new Color(0.10f, 0.45f, 0.75f), 0.2f, 0.6f);
            BuildKit.Label(signPlate, "POOL", Color.white, 44, new Vector3(0f, 0f, -0.15f));

            // ================= PEOPLE (world positions) =================
            // Convert local to world: world = C + local
            // Swimmers (Idle, low in water)
            SpawnSwimmer(C + new Vector3(-5f, 0f, 0f), new Color(0.2f, 0.6f, 0.9f));
            SpawnSwimmer(C + new Vector3(0f, 0f, -3f), new Color(0.9f, 0.4f, 0.4f));
            SpawnSwimmer(C + new Vector3(2f, 0f, 3f), new Color(0.95f, 0.8f, 0.3f));
            SpawnSwimmer(C + new Vector3(-7f, 0f, 9f), new Color(0.5f, 0.85f, 0.5f));

            // Loungers (Sunbathe on chairs) — match lounger spots
            SpawnLounger(C + new Vector3(12f, 0.55f, 8f), new Color(0.95f, 0.3f, 0.3f));
            SpawnLounger(C + new Vector3(12f, 0.55f, 3f), new Color(0.3f, 0.7f, 0.95f));
            SpawnLounger(C + new Vector3(-16f, 0.55f, -6f), new Color(0.98f, 0.78f, 0.2f));
            SpawnLounger(C + new Vector3(-16f, 0.55f, 4f), new Color(0.4f, 0.85f, 0.5f));

            // Attendant near the bar
            var att = BuildKit.Civ("PoolAttendant", C + new Vector3(15f, 0f, -8f), Civilian.Mode.Idle,
                new Color(0.95f, 0.95f, 0.98f), new Color(0.15f, 0.15f, 0.2f));
            att.skinColor = new Color(0.82f, 0.62f, 0.45f);
        }

        // ---------- helpers ----------

        static void Water(Transform h, Vector3 pos, Vector3 scale, Color c)
        {
            var w = BuildKit.CubeTex("Water", h, pos, scale, ProceduralTextures.Water, c, 0.15f, 0.92f, new Vector2(scale.x * 0.4f, scale.z * 0.4f));
            Object.Destroy(w.GetComponent<Collider>());
        }

        static void DeckBorder(Transform h, Color stoneTint)
        {
            float y = 0.13f, t = 0.04f;
            Vector3[] segs = {
                new Vector3(0f, y, 15f), new Vector3(0f, y, -15f),
            };
            // top/bottom long borders
            foreach (var p in segs)
            {
                var b = BuildKit.CubeTex("DeckBorder", h, p, new Vector3(40f, t, 1.2f), ProceduralTextures.Stone, stoneTint, 0.05f, 0.5f, new Vector2(20f, 1f));
                Object.Destroy(b.GetComponent<Collider>());
            }
            // left/right
            foreach (var p in new[] { new Vector3(20f, y, 0f), new Vector3(-20f, y, 0f) })
            {
                var b = BuildKit.CubeTex("DeckBorder", h, p, new Vector3(1.2f, t, 30f), ProceduralTextures.Stone, stoneTint, 0.05f, 0.5f, new Vector2(1f, 15f));
                Object.Destroy(b.GetComponent<Collider>());
            }
        }

        static void PoolLip(Transform h, Vector3 center, Vector3 outer, Color c)
        {
            float lh = outer.y;
            float w = 0.6f; // wall thickness
            float hx = outer.x * 0.5f, hz = outer.z * 0.5f;
            // four curb walls (player bumps these)
            BuildKit.CubeTex("Lip", h, center + new Vector3(0f, lh * 0.5f, hz), new Vector3(outer.x, lh, w), ProceduralTextures.Marble, c, 0.05f, 0.6f, new Vector2(6f, 1f));
            BuildKit.CubeTex("Lip", h, center + new Vector3(0f, lh * 0.5f, -hz), new Vector3(outer.x, lh, w), ProceduralTextures.Marble, c, 0.05f, 0.6f, new Vector2(6f, 1f));
            BuildKit.CubeTex("Lip", h, center + new Vector3(hx, lh * 0.5f, 0f), new Vector3(w, lh, outer.z), ProceduralTextures.Marble, c, 0.05f, 0.6f, new Vector2(1f, 6f));
            BuildKit.CubeTex("Lip", h, center + new Vector3(-hx, lh * 0.5f, 0f), new Vector3(w, lh, outer.z), ProceduralTextures.Marble, c, 0.05f, 0.6f, new Vector2(1f, 6f));
        }

        static void DivingBoard(Transform h, Vector3 baseLocal, Color teak, Color chrome)
        {
            // two posts
            BuildKit.Cylinder("DivePost", h, baseLocal + new Vector3(-0.6f, 0.7f, 0f), new Vector3(0.2f, 0.7f, 0.2f), chrome, 0.7f, 0.8f);
            BuildKit.Cylinder("DivePost", h, baseLocal + new Vector3(0.6f, 0.7f, 0f), new Vector3(0.2f, 0.7f, 0.2f), chrome, 0.7f, 0.8f);
            // angled plank extending over the water (-z toward pool)
            var plank = BuildKit.Cube("DivePlank", h, baseLocal + new Vector3(0f, 1.45f, -2.2f), new Vector3(1.0f, 0.14f, 5f), teak, 0.1f, 0.5f);
            plank.transform.localRotation = Quaternion.Euler(-4f, 0f, 0f);
            // grip handrails
            BuildKit.Cylinder("DiveRail", h, baseLocal + new Vector3(-0.55f, 1.9f, 0.6f), new Vector3(0.06f, 0.5f, 0.06f), chrome, 0.8f, 0.85f);
            BuildKit.Cylinder("DiveRail", h, baseLocal + new Vector3(0.55f, 1.9f, 0.6f), new Vector3(0.06f, 0.5f, 0.06f), chrome, 0.8f, 0.85f);
        }

        static void WaterSlide(Transform h, Vector3 baseLocal)
        {
            Color slideBlue = new Color(0.2f, 0.55f, 0.95f);
            Color slideYel = new Color(0.98f, 0.82f, 0.25f);
            // support tower
            BuildKit.Cube("SlideTower", h, baseLocal + new Vector3(0f, 1.5f, 0f), new Vector3(1.6f, 3.0f, 1.6f), slideYel, 0.1f, 0.5f);
            // top landing
            BuildKit.Cube("SlideTop", h, baseLocal + new Vector3(0.8f, 3.0f, 0f), new Vector3(1.6f, 0.2f, 1.6f), slideBlue, 0.1f, 0.5f);
            // angled chute segments stepping down toward water (+x +z)
            var s1 = BuildKit.Cube("SlideChute", h, baseLocal + new Vector3(2.2f, 2.4f, 1.0f), new Vector3(1.4f, 0.2f, 2.6f), slideBlue, 0.1f, 0.6f);
            s1.transform.localRotation = Quaternion.Euler(35f, 25f, 0f);
            var s2 = BuildKit.Cube("SlideChute", h, baseLocal + new Vector3(3.6f, 1.3f, 2.4f), new Vector3(1.4f, 0.2f, 2.6f), slideYel, 0.1f, 0.6f);
            s2.transform.localRotation = Quaternion.Euler(40f, 35f, 0f);
            var s3 = BuildKit.Cube("SlideChute", h, baseLocal + new Vector3(5.0f, 0.5f, 3.6f), new Vector3(1.4f, 0.2f, 2.4f), slideBlue, 0.1f, 0.6f);
            s3.transform.localRotation = Quaternion.Euler(28f, 40f, 0f);
        }

        static void LadderRails(Transform h, Vector3 lipLocal, Color chrome)
        {
            BuildKit.Cylinder("LadderRail", h, lipLocal + new Vector3(-0.4f, 0.7f, 0f), new Vector3(0.08f, 0.7f, 0.08f), chrome, 0.85f, 0.9f);
            BuildKit.Cylinder("LadderRail", h, lipLocal + new Vector3(0.4f, 0.7f, 0f), new Vector3(0.08f, 0.7f, 0.08f), chrome, 0.85f, 0.9f);
            // curved top bar (single cube)
            BuildKit.Cube("LadderTop", h, lipLocal + new Vector3(0f, 1.3f, 0f), new Vector3(0.9f, 0.08f, 0.08f), chrome, 0.85f, 0.9f);
        }

        static void SunLounger(Transform h, Vector3 local, Color parasol, Color cushion, Color frame, float face)
        {
            var node = BuildKit.Root("Lounger", h.position + local);
            node.transform.SetParent(h, true);
            node.transform.localRotation = Quaternion.Euler(0f, face > 0 ? 90f : -90f, 0f);
            Transform n = node.transform;
            // base frame
            BuildKit.Cube("LoungerFrame", n, new Vector3(0f, 0.25f, 0f), new Vector3(2.2f, 0.12f, 0.9f), frame, 0.6f, 0.6f);
            // cushion seat
            BuildKit.Cube("LoungerCushion", n, new Vector3(0f, 0.34f, 0f), new Vector3(2.0f, 0.12f, 0.8f), cushion, 0.0f, 0.3f);
            // reclined backrest
            var back = BuildKit.Cube("LoungerBack", n, new Vector3(-0.95f, 0.6f, 0f), new Vector3(0.6f, 0.7f, 0.78f), cushion, 0.0f, 0.3f);
            back.transform.localRotation = Quaternion.Euler(0f, 0f, 55f);
            // legs
            foreach (var dx in new[] { -0.9f, 0.9f })
                foreach (var dz in new[] { -0.35f, 0.35f })
                    BuildKit.Cylinder("LoungerLeg", n, new Vector3(dx, 0.1f, dz), new Vector3(0.08f, 0.1f, 0.08f), frame, 0.6f, 0.6f);
            // rolled towel
            Color towelCol = new Color(0.3f + Random.value * 0.6f, 0.3f + Random.value * 0.6f, 0.3f + Random.value * 0.6f);
            var towel = BuildKit.Cube("Towel", n, new Vector3(0.6f, 0.45f, 0f), new Vector3(0.5f, 0.18f, 0.7f), towelCol, 0f, 0.2f);
            // parasol pole + canopy
            BuildKit.Cylinder("ParasolPole", n, new Vector3(0.0f, 1.3f, 0.6f), new Vector3(0.1f, 1.3f, 0.1f), frame, 0.5f, 0.5f);
            var canopy = BuildKit.Sphere("ParasolCanopy", n, new Vector3(0.0f, 2.6f, 0.6f), new Vector3(3.0f, 1.0f, 3.0f), parasol, 0.0f, 0.4f);
            // (sphere collider already removed by default)
            canopy.transform.localScale = new Vector3(3.0f, 1.2f, 3.0f);
        }

        static void Inflatable(Transform h, Vector3 local, Color c, bool ring)
        {
            if (ring)
            {
                var r = BuildKit.Sphere("FloatRing", h, local, new Vector3(1.8f, 0.5f, 1.8f), c, 0.0f, 0.5f);
                // hole illusion: inner darker sphere
                var inner = BuildKit.Sphere("FloatHole", h, local + new Vector3(0f, 0.02f, 0f), new Vector3(0.9f, 0.55f, 0.9f), new Color(0.15f, 0.5f, 0.8f), 0.1f, 0.7f);
            }
            else
            {
                var f = BuildKit.Cube("FloatMat", h, local, new Vector3(1.4f, 0.2f, 2.4f), c, 0.0f, 0.4f);
                Object.Destroy(f.GetComponent<Collider>());
            }
        }

        static void PoolBar(Transform h, Vector3 local, Color teak, Color marble, Color chrome)
        {
            var node = BuildKit.Root("PoolBar", h.position + local);
            node.transform.SetParent(h, true);
            Transform n = node.transform;
            // bar counter body
            BuildKit.CubeTex("BarBody", n, new Vector3(0f, 0.6f, 0f), new Vector3(5f, 1.2f, 1.4f), ProceduralTextures.Wood, teak, 0.1f, 0.5f, new Vector2(4f, 1f));
            // marble countertop
            BuildKit.CubeTex("BarTop", n, new Vector3(0f, 1.24f, 0f), new Vector3(5.4f, 0.12f, 1.7f), ProceduralTextures.Marble, marble, 0.1f, 0.7f, new Vector2(4f, 1f));
            // back shelf
            BuildKit.Cube("BarShelf", n, new Vector3(0f, 1.6f, 0.9f), new Vector3(5f, 0.1f, 0.4f), teak, 0.1f, 0.5f);
            // bottles on the shelf (thin cylinders)
            Color[] bottleCols = {
                new Color(0.2f, 0.6f, 0.2f), new Color(0.7f, 0.2f, 0.2f), new Color(0.9f, 0.8f, 0.3f),
                new Color(0.3f, 0.4f, 0.8f), new Color(0.8f, 0.5f, 0.2f), new Color(0.6f, 0.2f, 0.7f),
                new Color(0.1f, 0.1f, 0.1f)
            };
            for (int i = 0; i < bottleCols.Length; i++)
            {
                float bx = -2.0f + i * 0.66f;
                BuildKit.Cylinder("Bottle", n, new Vector3(bx, 1.95f, 0.9f), new Vector3(0.12f, 0.28f, 0.12f), bottleCols[i], 0.2f, 0.85f);
            }
            // stools in front
            for (int i = 0; i < 4; i++)
            {
                float sx = -1.8f + i * 1.2f;
                BuildKit.Cylinder("StoolLeg", n, new Vector3(sx, 0.45f, -1.4f), new Vector3(0.12f, 0.45f, 0.12f), chrome, 0.8f, 0.85f);
                BuildKit.Cylinder("StoolSeat", n, new Vector3(sx, 0.95f, -1.4f), new Vector3(0.5f, 0.08f, 0.5f), new Color(0.8f, 0.2f, 0.2f), 0.1f, 0.5f);
            }
        }

        static void Planter(Transform h, Vector3 local)
        {
            Color pot = new Color(0.85f, 0.83f, 0.78f);
            Color trunk = new Color(0.45f, 0.30f, 0.16f);
            Color frond = new Color(0.20f, 0.65f, 0.25f);
            // pot
            BuildKit.CubeTex("PlanterPot", h, local + new Vector3(0f, 0.45f, 0f), new Vector3(1.4f, 0.9f, 1.4f), ProceduralTextures.Stone, pot, 0.05f, 0.5f, new Vector2(2f, 2f));
            // trunk
            BuildKit.Cylinder("PalmTrunk", h, local + new Vector3(0f, 2.2f, 0f), new Vector3(0.3f, 1.6f, 0.3f), trunk, 0.0f, 0.3f);
            // fronds (pass-through foliage, collider removed)
            for (int i = 0; i < 6; i++)
            {
                float a = i * 60f * Mathf.Deg2Rad;
                Vector3 off = new Vector3(Mathf.Cos(a) * 1.1f, 3.9f, Mathf.Sin(a) * 1.1f);
                var fr = BuildKit.Sphere("PalmFrond", h, local + off, new Vector3(2.0f, 0.3f, 0.7f), frond, 0.0f, 0.3f);
                fr.transform.localRotation = Quaternion.Euler(0f, i * 60f, 25f);
            }
            // crown cluster
            BuildKit.Sphere("PalmCrown", h, local + new Vector3(0f, 3.9f, 0f), new Vector3(1.1f, 0.7f, 1.1f), frond, 0f, 0.3f);
        }

        static void SpawnSwimmer(Vector3 world, Color shirt)
        {
            // low in the water: drop the civ down a bit
            var c = BuildKit.Civ("Swimmer", world + new Vector3(0f, 0.05f, 0f), Civilian.Mode.Idle, shirt, new Color(0.1f, 0.2f, 0.4f));
            c.skinColor = new Color(0.85f, 0.65f, 0.48f);
            // sink the visual a touch so they read as "in the water"
            c.transform.position = world - new Vector3(0f, 0.45f, 0f);
        }

        static void SpawnLounger(Vector3 world, Color shirt)
        {
            var c = BuildKit.Civ("Sunbather", world, Civilian.Mode.Sunbathe, shirt, new Color(0.2f, 0.2f, 0.25f));
            c.skinColor = new Color(0.88f, 0.66f, 0.46f);
        }
    }
}
