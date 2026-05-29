using UnityEngine;

namespace Spoonacci
{
    // LAVISH infinity pool centered at WorldLayout.Pool (-15, 0, -2).
    // This builder owns the "Central Pool" footprint, so it is EXEMPT from the
    // Blocked() scatter rule for its OWN zone. Everything is parented to a Root
    // placed at the pool's world position and built with LOCAL coordinates, so
    // all props stay within ~15 units of the pool center by construction.
    public static class PoolBasinBuilder
    {
        public static void Build(Transform root)
        {
            Vector3 center = new Vector3(WorldLayout.Pool.x, 0f, WorldLayout.Pool.y);
            var hub = BuildKit.Root("PoolBasin", center).transform;
            hub.SetParent(root, true);

            // ---- palette ----------------------------------------------------
            Color deckTint   = new Color(0.96f, 0.93f, 0.86f);   // warm marble
            Color stoneTint  = new Color(0.80f, 0.78f, 0.72f);   // coping
            Color waterTint  = new Color(0.25f, 0.72f, 0.95f);   // tropical blue
            Color chrome      = new Color(0.85f, 0.88f, 0.92f);

            BuildDeck(hub, deckTint, stoneTint);
            BuildBasin(hub, waterTint, stoneTint, chrome);
            BuildDivingBoard(hub, chrome);
            BuildSlide(hub);
            BuildLoungers(hub);
            BuildFloats(hub);
            BuildCocktailBar(hub);
        }

        // -------------------------------------------------------------------
        // DECK: big marble platform + a coping ring around the water.
        // -------------------------------------------------------------------
        private static void BuildDeck(Transform hub, Color marble, Color stone)
        {
            // Main marble deck (solid, KEEP collider).
            BuildKit.CubeTex("Deck", hub, new Vector3(0f, 0.06f, 0f),
                new Vector3(26f, 0.12f, 24f), ProceduralTextures.Marble, marble,
                0.05f, 0.55f, new Vector2(8f, 8f));

            // A darker stone trim border just inside the deck edge (flat decal).
            var trim = BuildKit.CubeTex("DeckTrim", hub, new Vector3(0f, 0.13f, 0f),
                new Vector3(20f, 0.04f, 18f), ProceduralTextures.Stone, stone,
                0.1f, 0.4f, new Vector2(6f, 6f));
            Object.Destroy(trim.GetComponent<Collider>());
        }

        // -------------------------------------------------------------------
        // BASIN: recessed blue water, raised coping walls, infinity overflow lip.
        // Pool interior occupies roughly the central 14 x 12 of the deck.
        // -------------------------------------------------------------------
        private static void BuildBasin(Transform hub, Color water, Color stone, Color chrome)
        {
            float w = 14f, d = 12f;          // water surface size
            float coping = 0.45f;            // coping wall height above deck
            float copeT = 0.55f;             // coping thickness

            // Water surface, slightly recessed below the coping top (decorative).
            var surf = BuildKit.CubeTex("Water", hub, new Vector3(0f, 0.22f, 0f),
                new Vector3(w, 0.06f, d), ProceduralTextures.Water, water,
                0.0f, 0.95f, new Vector2(3f, 3f));
            Object.Destroy(surf.GetComponent<Collider>());

            // Coping walls (KEEP collider) on N / E / W. South side is the infinity edge.
            float hx = w * 0.5f + copeT * 0.5f;
            float hz = d * 0.5f + copeT * 0.5f;
            BuildKit.CubeTex("CopeN", hub, new Vector3(0f, coping * 0.5f + 0.12f, hz),
                new Vector3(w + copeT * 2f, coping, copeT), ProceduralTextures.Stone, stone, 0.1f, 0.4f, new Vector2(4f, 1f));
            BuildKit.CubeTex("CopeE", hub, new Vector3(hx, coping * 0.5f + 0.12f, 0f),
                new Vector3(copeT, coping, d), ProceduralTextures.Stone, stone, 0.1f, 0.4f, new Vector2(1f, 4f));
            BuildKit.CubeTex("CopeW", hub, new Vector3(-hx, coping * 0.5f + 0.12f, 0f),
                new Vector3(copeT, coping, d), ProceduralTextures.Stone, stone, 0.1f, 0.4f, new Vector2(1f, 4f));

            // INFINITY EDGE (south, -z): a thin overflow lip flush with the water,
            // plus a catch-basin sheet just below it so the water "spills" forever.
            float sz = -d * 0.5f - copeT * 0.5f;
            var lip = BuildKit.CubeTex("InfinityLip", hub, new Vector3(0f, 0.20f, sz),
                new Vector3(w + copeT * 2f, 0.05f, copeT * 0.8f), ProceduralTextures.Stone, stone, 0.2f, 0.6f, new Vector2(4f, 1f));
            // lip is a structural edge -> KEEP collider (it is a low wall you can stand against)

            // Overflow sheet: a second, lower water band that catches the spill.
            var spill = BuildKit.CubeTex("OverflowSheet", hub, new Vector3(0f, 0.08f, sz - 0.8f),
                new Vector3(w + copeT * 2f, 0.04f, 1.4f), ProceduralTextures.Water, water,
                0.0f, 0.95f, new Vector2(4f, 1f));
            Object.Destroy(spill.GetComponent<Collider>());

            // Chrome ladder rails (NE corner) descending into the pool.
            float lx = w * 0.5f - 1.2f, lz = d * 0.5f - 0.4f;
            BuildKit.Cylinder("LadderRailA", hub, new Vector3(lx - 0.35f, 0.55f, lz), new Vector3(0.08f, 0.55f, 0.08f), chrome, 0.85f, 0.85f);
            BuildKit.Cylinder("LadderRailB", hub, new Vector3(lx + 0.35f, 0.55f, lz), new Vector3(0.08f, 0.55f, 0.08f), chrome, 0.85f, 0.85f);
            // curved rail tops
            var arc = BuildKit.Cylinder("LadderArc", hub, new Vector3(lx, 1.05f, lz), new Vector3(0.08f, 0.45f, 0.08f), chrome, 0.85f, 0.85f);
            arc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        // -------------------------------------------------------------------
        // DIVING BOARD: two posts + a springboard plank cantilevered over water.
        // Placed on the east coping, reaching out toward the pool center.
        // -------------------------------------------------------------------
        private static void BuildDivingBoard(Transform hub, Color chrome)
        {
            float baseX = 8.0f, baseZ = -3.5f;
            // posts
            BuildKit.Cylinder("DivePostA", hub, new Vector3(baseX, 0.45f, baseZ - 0.4f), new Vector3(0.18f, 0.45f, 0.18f), chrome, 0.8f, 0.8f);
            BuildKit.Cylinder("DivePostB", hub, new Vector3(baseX, 0.45f, baseZ + 0.4f), new Vector3(0.18f, 0.45f, 0.18f), chrome, 0.8f, 0.8f);
            // springboard plank reaching west over the water (KEEP collider, you walk on it)
            BuildKit.Cube("Springboard", hub, new Vector3(baseX - 2.6f, 0.92f, baseZ),
                new Vector3(5.4f, 0.12f, 0.9f), new Color(0.95f, 0.95f, 0.9f), 0.1f, 0.7f);
        }

        // -------------------------------------------------------------------
        // WATER SLIDE: a few angled cube segments curving down into the pool.
        // -------------------------------------------------------------------
        private static void BuildSlide(Transform hub)
        {
            Color slide = new Color(1f, 0.45f, 0.55f); // tropical coral
            float sx = -8.5f, sz = 4.5f;

            // tall tower leg
            BuildKit.Cylinder("SlideTower", hub, new Vector3(sx, 1.6f, sz + 1.2f), new Vector3(0.4f, 1.6f, 0.4f), new Color(0.9f, 0.9f, 0.85f), 0.2f, 0.5f);
            var platform = BuildKit.Cube("SlideTop", hub, new Vector3(sx, 3.2f, sz + 1.2f), new Vector3(1.6f, 0.18f, 1.6f), slide, 0.1f, 0.6f);

            // angled chute segments stepping down toward the pool (curve via yaw + pitch)
            var s1 = BuildKit.Cube("SlideSeg1", hub, new Vector3(sx + 0.9f, 2.6f, sz + 0.4f), new Vector3(2.4f, 0.18f, 1.3f), slide, 0.1f, 0.6f);
            s1.transform.localRotation = Quaternion.Euler(25f, 30f, 0f);
            var s2 = BuildKit.Cube("SlideSeg2", hub, new Vector3(sx + 2.2f, 1.7f, sz - 0.4f), new Vector3(2.6f, 0.18f, 1.3f), slide, 0.1f, 0.6f);
            s2.transform.localRotation = Quaternion.Euler(30f, 55f, 0f);
            var s3 = BuildKit.Cube("SlideSeg3", hub, new Vector3(sx + 3.4f, 0.9f, sz - 1.6f), new Vector3(2.6f, 0.18f, 1.3f), slide, 0.1f, 0.6f);
            s3.transform.localRotation = Quaternion.Euler(22f, 80f, 0f);
            // splash exit into pool
            var s4 = BuildKit.Cube("SlideExit", hub, new Vector3(sx + 4.4f, 0.5f, sz - 2.8f), new Vector3(2.2f, 0.18f, 1.3f), slide, 0.1f, 0.6f);
            s4.transform.localRotation = Quaternion.Euler(15f, 92f, 0f);

            // chute side rails (pass-through decoration, remove colliders)
            foreach (var seg in new[] { s1, s2, s3, s4 })
            {
                var rail = BuildKit.Cube("ChuteRail", seg.transform, new Vector3(0f, 0.25f, 0.6f), new Vector3(1f, 0.4f, 0.05f), new Color(1f, 0.7f, 0.3f), 0.1f, 0.5f);
                Object.Destroy(rail.GetComponent<Collider>());
                var rail2 = BuildKit.Cube("ChuteRail2", seg.transform, new Vector3(0f, 0.25f, -0.6f), new Vector3(1f, 0.4f, 0.05f), new Color(1f, 0.7f, 0.3f), 0.1f, 0.5f);
                Object.Destroy(rail2.GetComponent<Collider>());
            }
        }

        // -------------------------------------------------------------------
        // SUN LOUNGERS (8) with parasol umbrellas + rolled towels.
        // Arranged along the north and west deck edges, facing the water.
        // -------------------------------------------------------------------
        private static void BuildLoungers(Transform hub)
        {
            // North row (5), facing -z toward pool. West row (3), facing +x.
            Vector3[] spots =
            {
                new Vector3(-9f, 0f,  9.5f), new Vector3(-5f, 0f, 9.5f), new Vector3(-1f, 0f, 9.5f),
                new Vector3(3f, 0f, 9.5f),  new Vector3(7f, 0f, 9.5f),
                new Vector3(-11.5f, 0f, 4f), new Vector3(-11.5f, 0f, 0f), new Vector3(-11.5f, 0f, -4f),
            };
            float[] yaws = { 0f, 0f, 0f, 0f, 0f, 90f, 90f, 90f };

            for (int i = 0; i < spots.Length; i++)
                BuildLounger(hub, spots[i], yaws[i], i);
        }

        private static void BuildLounger(Transform hub, Vector3 pos, float yaw, int idx)
        {
            var node = new GameObject("Lounger" + idx);
            node.transform.SetParent(hub, false);
            node.transform.localPosition = pos;
            node.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            var t = node.transform;

            Color frame = new Color(0.95f, 0.92f, 0.85f);
            Color[] cushions = { new Color(0.95f, 0.35f, 0.45f), new Color(0.2f, 0.7f, 0.85f), new Color(1f, 0.78f, 0.25f), new Color(0.45f, 0.8f, 0.5f) };
            Color cush = cushions[idx % cushions.Length];

            // frame seat (KEEP collider)
            BuildKit.Cube("Seat", t, new Vector3(0f, 0.35f, 0f), new Vector3(0.9f, 0.12f, 2.0f), frame, 0.2f, 0.5f);
            // cushion (pass-through decoration)
            var c = BuildKit.Cube("Cushion", t, new Vector3(0f, 0.44f, 0.1f), new Vector3(0.8f, 0.08f, 1.7f), cush, 0.05f, 0.55f);
            Object.Destroy(c.GetComponent<Collider>());
            // raised backrest
            var back = BuildKit.Cube("Backrest", t, new Vector3(0f, 0.7f, -0.85f), new Vector3(0.8f, 0.5f, 0.1f), cush, 0.05f, 0.55f);
            back.transform.localRotation = Quaternion.Euler(-35f, 0f, 0f);
            Object.Destroy(back.GetComponent<Collider>());
            // legs
            BuildKit.Cube("LegA", t, new Vector3(-0.35f, 0.18f, 0.8f), new Vector3(0.08f, 0.36f, 0.08f), frame, 0.2f, 0.5f);
            BuildKit.Cube("LegB", t, new Vector3(0.35f, 0.18f, 0.8f), new Vector3(0.08f, 0.36f, 0.08f), frame, 0.2f, 0.5f);

            // rolled towel at the foot (pass-through cylinder, remove collider)
            Color[] towels = { new Color(1f, 1f, 0.95f), new Color(0.9f, 0.5f, 0.6f), new Color(0.5f, 0.85f, 0.95f) };
            var towel = BuildKit.Cylinder("Towel", t, new Vector3(0f, 0.5f, 0.85f), new Vector3(0.14f, 0.4f, 0.14f), towels[idx % towels.Length], 0.0f, 0.3f);
            towel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Object.Destroy(towel.GetComponent<Collider>());

            // parasol umbrella beside the lounger
            BuildParasol(t, new Vector3(0.9f, 0f, -0.4f), idx);
        }

        private static void BuildParasol(Transform parent, Vector3 local, int idx)
        {
            // pole (KEEP collider)
            BuildKit.Cylinder("ParasolPole", parent, local + new Vector3(0f, 1.3f, 0f), new Vector3(0.08f, 1.3f, 0.08f), new Color(0.6f, 0.55f, 0.5f), 0.3f, 0.5f);
            // canopy (sphere half -> squashed; pass-through, collider already removed by Sphere default)
            Color[] canopy = { new Color(1f, 0.55f, 0.3f), new Color(0.2f, 0.7f, 0.9f), new Color(1f, 0.85f, 0.3f), new Color(0.9f, 0.3f, 0.5f) };
            var dome = BuildKit.Sphere("ParasolCanopy", parent, local + new Vector3(0f, 2.55f, 0f), new Vector3(2.8f, 1.3f, 2.8f), canopy[idx % canopy.Length], 0.05f, 0.4f);
            // squash to a dome (keep top hemisphere look) by scaling under a clip cube would be overkill; just flatten.
            dome.transform.localScale = new Vector3(2.8f, 1.1f, 2.8f);
        }

        // -------------------------------------------------------------------
        // INFLATABLE FLOATS (4) on the water surface. Toruses approximated by a
        // ring of flattened spheres so they read as donut tubes. Colliders off.
        // -------------------------------------------------------------------
        private static void BuildFloats(Transform hub)
        {
            float wy = 0.32f; // just above water surface
            Vector3[] centers =
            {
                new Vector3(-3f, wy, 2f), new Vector3(2.5f, wy, -1.5f),
                new Vector3(-1f, wy, -3f), new Vector3(4f, wy, 2.5f),
            };
            Color[] cols = { new Color(1f, 0.55f, 0.2f), new Color(1f, 0.4f, 0.6f), new Color(0.3f, 0.85f, 0.9f), new Color(1f, 0.85f, 0.3f) };

            for (int f = 0; f < centers.Length; f++)
            {
                var ring = new GameObject("Float" + f);
                ring.transform.SetParent(hub, false);
                ring.transform.localPosition = centers[f];
                ring.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                float r = 0.95f;
                int beads = 10;
                for (int i = 0; i < beads; i++)
                {
                    float ang = (i / (float)beads) * Mathf.PI * 2f;
                    var bead = BuildKit.Sphere("Tube", ring.transform,
                        new Vector3(Mathf.Cos(ang) * r, 0f, Mathf.Sin(ang) * r),
                        new Vector3(0.55f, 0.4f, 0.55f), cols[f], 0.0f, 0.5f);
                    // collider already removed by Sphere default
                }
                // some floats get a flat disc center (an inner mattress)
                if (f % 2 == 0)
                {
                    var disc = BuildKit.Cylinder("FloatDisc", ring.transform, new Vector3(0f, -0.05f, 0f),
                        new Vector3(1.3f, 0.05f, 1.3f), new Color(1f, 1f, 1f, 1f), 0.0f, 0.4f);
                    Object.Destroy(disc.GetComponent<Collider>());
                }
            }
        }

        // -------------------------------------------------------------------
        // POOLSIDE COCKTAIL BAR NOOK: counter, back shelf, stools, bottles.
        // Tucked into the SE corner of the deck, away from the infinity edge spill.
        // -------------------------------------------------------------------
        private static void BuildCocktailBar(Transform hub)
        {
            var node = new GameObject("CocktailBar");
            node.transform.SetParent(hub, false);
            node.transform.localPosition = new Vector3(10.5f, 0f, 7.5f);
            node.transform.localRotation = Quaternion.Euler(0f, -125f, 0f);
            var t = node.transform;

            Color wood = new Color(0.5f, 0.32f, 0.18f);
            Color top = new Color(0.85f, 0.8f, 0.7f);

            // counter base (KEEP collider)
            BuildKit.CubeTex("BarBase", t, new Vector3(0f, 0.55f, 0f), new Vector3(4.2f, 1.1f, 1.0f), ProceduralTextures.Wood, wood, 0.1f, 0.4f, new Vector2(2f, 1f));
            // counter top overhang (KEEP collider)
            BuildKit.CubeTex("BarTop", t, new Vector3(0f, 1.16f, 0.15f), new Vector3(4.6f, 0.12f, 1.4f), ProceduralTextures.Marble, top, 0.1f, 0.6f, new Vector2(2f, 1f));

            // back shelf (KEEP collider)
            BuildKit.Cube("BackShelf", t, new Vector3(0f, 1.5f, -0.7f), new Vector3(4.0f, 0.1f, 0.4f), wood, 0.1f, 0.4f);
            BuildKit.Cube("ShelfPostL", t, new Vector3(-1.9f, 1.0f, -0.7f), new Vector3(0.12f, 2.0f, 0.12f), wood, 0.1f, 0.4f);
            BuildKit.Cube("ShelfPostR", t, new Vector3(1.9f, 1.0f, -0.7f), new Vector3(0.12f, 2.0f, 0.12f), wood, 0.1f, 0.4f);

            // bottles on the shelf (pass-through decoration)
            Color[] liquor = { new Color(0.2f, 0.8f, 0.4f), new Color(0.9f, 0.7f, 0.2f), new Color(0.8f, 0.2f, 0.3f), new Color(0.3f, 0.5f, 0.9f), new Color(0.9f, 0.5f, 0.8f) };
            for (int i = 0; i < 7; i++)
            {
                float x = -1.6f + i * 0.55f;
                var b = BuildKit.Cylinder("Bottle" + i, t, new Vector3(x, 1.75f, -0.7f), new Vector3(0.12f, 0.28f, 0.12f), liquor[i % liquor.Length], 0.1f, 0.8f);
                Object.Destroy(b.GetComponent<Collider>());
                var neck = BuildKit.Cylinder("BottleNeck" + i, t, new Vector3(x, 2.05f, -0.7f), new Vector3(0.05f, 0.1f, 0.05f), liquor[i % liquor.Length], 0.1f, 0.8f);
                Object.Destroy(neck.GetComponent<Collider>());
            }

            // a couple of cocktail glasses on the counter (pass-through)
            for (int i = 0; i < 3; i++)
            {
                var g = BuildKit.Cylinder("Glass" + i, t, new Vector3(-1.2f + i * 1.2f, 1.32f, 0.3f), new Vector3(0.1f, 0.08f, 0.1f), new Color(0.95f, 0.85f, 0.4f, 1f), 0.0f, 0.9f);
                Object.Destroy(g.GetComponent<Collider>());
            }

            // bar stools in front (KEEP colliders)
            for (int i = 0; i < 3; i++)
            {
                float x = -1.3f + i * 1.3f;
                BuildKit.Cylinder("StoolSeat" + i, t, new Vector3(x, 0.75f, 1.2f), new Vector3(0.45f, 0.08f, 0.45f), new Color(0.9f, 0.3f, 0.4f), 0.1f, 0.5f);
                BuildKit.Cylinder("StoolLeg" + i, t, new Vector3(x, 0.37f, 1.2f), new Vector3(0.1f, 0.37f, 0.1f), new Color(0.7f, 0.7f, 0.72f), 0.6f, 0.6f);
            }

            // tiki-style thatch canopy over the bar (pass-through)
            var canopy = BuildKit.Cube("BarCanopy", t, new Vector3(0f, 2.6f, -0.2f), new Vector3(5.2f, 0.15f, 2.6f), new Color(0.6f, 0.45f, 0.25f), 0.0f, 0.3f);
            Object.Destroy(canopy.GetComponent<Collider>());
            BuildKit.Cube("CanopyPostL", t, new Vector3(-2.3f, 1.3f, 0.9f), new Vector3(0.12f, 2.6f, 0.12f), wood, 0.1f, 0.4f);
            BuildKit.Cube("CanopyPostR", t, new Vector3(2.3f, 1.3f, 0.9f), new Vector3(0.12f, 2.6f, 0.12f), wood, 0.1f, 0.4f);
        }
    }
}
