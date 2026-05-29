using UnityEngine;

namespace Spoonacci
{
    // The gritty SHADY ALLEY at WorldLayout.Alley (-22, 22).
    //
    // A tight, atmospheric back-corridor: two tall narrow walls form a passage,
    // a grimy dark floor slab underfoot, dumpsters + stacked crates + trash bags,
    // bright graffiti panels splashed on the walls, papers blowing on the ground,
    // a flickering amber lamp overhead, a cryptic neon "Alley" sign, three shady
    // dealer NPCs loitering, and a sketchy van parked at the alley mouth.
    //
    // This builder is EXEMPT from the scatter-Blocked() rule for its OWN footprint
    // (it intentionally fills the reserved Alley zone), but it stays well within the
    // footprint and clear of the Origin->Alley spur road, which approaches from the SE.
    public static class AlleyBuilder
    {
        public static void Build(Transform root)
        {
            // Anchor the alley a touch NORTH/WEST of the footprint center so the
            // corridor opening (the "mouth") sits on the far side from the spur road
            // that comes in from the south-east.
            Vector2 a2 = WorldLayout.Alley;            // (-22, 22)
            Vector3 anchor = new Vector3(a2.x - 1f, 0f, a2.y + 3f);

            var hub = BuildKit.Root("ShadyAlley", anchor);
            if (root != null) hub.transform.SetParent(root, true);
            Transform A = hub.transform;

            // The corridor runs along local Z. Walls flank it along local X.
            // We rotate the whole hub so the corridor opening faces roughly NW,
            // away from the spur road.
            A.rotation = Quaternion.Euler(0f, 30f, 0f);

            // ---- palette ----------------------------------------------------
            Color wallCol   = new Color(0.34f, 0.33f, 0.31f); // dingy concrete
            Color wallCol2  = new Color(0.28f, 0.27f, 0.26f); // darker patch
            Color floorCol  = new Color(0.12f, 0.12f, 0.13f); // grimy asphalt
            Color rustGreen = new Color(0.18f, 0.30f, 0.22f); // dumpster green
            Color rustRed   = new Color(0.42f, 0.16f, 0.12f); // rusty dumpster
            Color crateCol  = new Color(0.45f, 0.32f, 0.18f); // wood crate
            Color bagCol    = new Color(0.06f, 0.06f, 0.07f); // black trash bag

            // ---- grimy floor slab (FLAT decal: thin, no collider) ----------
            var floor = BuildKit.Cube("AlleyFloor", A, new Vector3(0f, 0.02f, 0f),
                new Vector3(6.4f, 0.04f, 16f), floorCol, 0.05f, 0.18f);
            Object.Destroy(floor.GetComponent<Collider>());

            // a couple of grimy oil/stain patches on the floor (decals)
            AddDecal(A, new Vector3(-0.8f, 0.03f, -2.5f), new Vector3(2.2f, 0.04f, 3.0f),
                new Color(0.05f, 0.05f, 0.06f), 0f);
            AddDecal(A, new Vector3(1.0f, 0.03f, 4.0f), new Vector3(1.6f, 0.04f, 2.2f),
                new Color(0.08f, 0.07f, 0.05f), 0f);

            // ---- two tall narrow walls (SOLID — keep colliders) ------------
            // Left and right walls forming the corridor (corridor ~5.6 wide inside).
            float wallH = 7.5f;
            float wallX = 3.1f;
            float wallLen = 16f;
            BuildKit.Cube("WallLeft", A, new Vector3(-wallX, wallH * 0.5f, 0f),
                new Vector3(0.6f, wallH, wallLen), wallCol, 0f, 0.12f);
            BuildKit.Cube("WallRight", A, new Vector3(wallX, wallH * 0.5f, 0f),
                new Vector3(0.6f, wallH, wallLen), wallCol2, 0f, 0.12f);

            // a low back wall closing one end of the corridor
            BuildKit.Cube("WallBack", A, new Vector3(0f, wallH * 0.45f, -wallLen * 0.5f),
                new Vector3(wallX * 2f + 0.6f, wallH * 0.9f, 0.6f), wallCol2, 0f, 0.12f);

            // grimy darker patches stuck to the walls (thin, no collider)
            AddWallPatch(A, new Vector3(-wallX + 0.32f, 4.2f, 2.0f), new Vector3(0.05f, 2.0f, 3.5f),
                new Color(0.20f, 0.19f, 0.18f));
            AddWallPatch(A, new Vector3(wallX - 0.32f, 2.8f, -3.0f), new Vector3(0.05f, 2.4f, 4.0f),
                new Color(0.22f, 0.20f, 0.18f));

            // ---- bright graffiti panels (thin cubes, NO collider) ----------
            AddGraffiti(A, new Vector3(-wallX + 0.33f, 2.4f, -1.5f), new Vector3(0.04f, 2.0f, 3.2f),
                new Color(1f, 0.15f, 0.55f));   // hot magenta
            AddGraffiti(A, new Vector3(wallX - 0.33f, 3.2f, 1.5f), new Vector3(0.04f, 1.6f, 2.6f),
                new Color(0.15f, 0.95f, 0.6f));  // toxic green
            AddGraffiti(A, new Vector3(-wallX + 0.33f, 1.6f, 5.0f), new Vector3(0.04f, 1.2f, 2.0f),
                new Color(0.25f, 0.55f, 1f));    // electric blue
            AddGraffiti(A, new Vector3(wallX - 0.33f, 4.6f, -5.5f), new Vector3(0.04f, 1.0f, 1.8f),
                new Color(1f, 0.85f, 0.1f));     // acid yellow

            // ---- dumpster #1 (green, lid) — SOLID -------------------------
            BuildDumpster(A, new Vector3(-1.9f, 0f, -5.6f), rustGreen);
            // ---- dumpster #2 (rusty red) — SOLID --------------------------
            BuildDumpster(A, new Vector3(1.9f, 0f, 4.5f), rustRed);

            // ---- stacked wood crates (SOLID) ------------------------------
            BuildCrate(A, new Vector3(2.0f, 0.45f, -5.2f), 0.9f, crateCol);
            BuildCrate(A, new Vector3(2.0f, 1.35f, -5.2f), 0.9f, crateCol * 0.9f);
            BuildCrate(A, new Vector3(2.55f, 0.45f, -4.3f), 0.9f, crateCol * 0.95f);
            BuildCrate(A, new Vector3(-2.1f, 0.4f, 6.4f), 0.8f, crateCol);
            BuildCrate(A, new Vector3(-2.0f, 1.2f, 6.5f), 0.8f, crateCol * 0.92f);

            // ---- trash bags (keepCollider = true) -------------------------
            TrashBag(A, new Vector3(-1.0f, 0.4f, 6.8f), 0.8f, bagCol);
            TrashBag(A, new Vector3(-0.4f, 0.35f, 7.1f), 0.7f, bagCol * 1.4f);
            TrashBag(A, new Vector3(1.2f, 0.38f, -3.6f), 0.75f, bagCol);
            TrashBag(A, new Vector3(0.6f, 0.3f, -4.0f), 0.6f, bagCol * 1.3f);

            // ---- scattered papers (FLAT decals, no collider) --------------
            for (int i = 0; i < 9; i++)
            {
                float px = Random.Range(-2.4f, 2.4f);
                float pz = Random.Range(-7f, 7.5f);
                var paper = AddDecal(A, new Vector3(px, 0.03f + i * 0.001f, pz),
                    new Vector3(Random.Range(0.25f, 0.5f), 0.02f, Random.Range(0.3f, 0.55f)),
                    new Color(0.78f, 0.76f, 0.7f), 0.05f);
                paper.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }

            // ---- flickering amber point light (FlickerLight) --------------
            var lampGo = BuildKit.Root("AlleyLamp", Vector3.zero);
            lampGo.transform.SetParent(A, false);
            lampGo.transform.localPosition = new Vector3(0.4f, 6.4f, 1.0f);
            // a small fixture housing on the wall
            var fixture = BuildKit.Cube("LampFixture", A, new Vector3(wallX - 0.4f, 6.4f, 1.0f),
                new Vector3(0.4f, 0.3f, 0.5f), new Color(0.1f, 0.1f, 0.1f), 0.3f, 0.2f);
            Object.Destroy(fixture.GetComponent<Collider>());
            var l = lampGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.66f, 0.28f);
            l.intensity = 3.2f;
            l.range = 14f;
            lampGo.AddComponent<FlickerLight>();

            // ---- cryptic neon sign ----------------------------------------
            BuildKit.Label(hub, "Alley", new Color(1f, 0.2f, 0.5f), 26,
                new Vector3(0f, 8.2f, 0f));

            // ---- 3 shady dealer civs (Idle, dark clothes) -----------------
            // place them INSIDE the corridor (world coords via A.TransformPoint).
            SpawnDealer(A, new Vector3(-0.6f, 0f, 0.0f),  new Color(0.10f, 0.10f, 0.12f), new Color(0.08f, 0.08f, 0.10f));
            SpawnDealer(A, new Vector3(0.9f, 0f, -2.2f),  new Color(0.14f, 0.10f, 0.10f), new Color(0.07f, 0.07f, 0.08f));
            SpawnDealer(A, new Vector3(-0.2f, 0f, 3.0f),  new Color(0.09f, 0.12f, 0.10f), new Color(0.06f, 0.06f, 0.07f));

            // ---- sketchy parked van at the alley mouth (SOLID) ------------
            // The mouth is the +Z open end of the corridor; park the van just outside it.
            BuildVan(A, new Vector3(0.2f, 0f, 10.2f));
        }

        // ---------------------------------------------------------------------
        private static GameObject AddDecal(Transform parent, Vector3 pos, Vector3 scale, Color col, float smooth)
        {
            var g = BuildKit.Cube("Decal", parent, pos, scale, col, 0f, smooth);
            Object.Destroy(g.GetComponent<Collider>());
            return g;
        }

        private static void AddWallPatch(Transform parent, Vector3 pos, Vector3 scale, Color col)
        {
            var g = BuildKit.Cube("WallGrime", parent, pos, scale, col, 0f, 0.1f);
            Object.Destroy(g.GetComponent<Collider>());
        }

        private static void AddGraffiti(Transform parent, Vector3 pos, Vector3 scale, Color col)
        {
            // bright tag panel — thin, decorative, NO collider
            var g = BuildKit.Cube("Graffiti", parent, pos, scale, col, 0f, 0.55f);
            Object.Destroy(g.GetComponent<Collider>());
        }

        private static void BuildDumpster(Transform parent, Vector3 basePos, Color col)
        {
            // body SOLID — keep collider
            BuildKit.Cube("DumpsterBody", parent,
                basePos + new Vector3(0f, 0.7f, 0f),
                new Vector3(1.6f, 1.4f, 1.1f), col, 0.25f, 0.2f);
            // lid decorative — no collider
            var lid = BuildKit.Cube("DumpsterLid", parent,
                basePos + new Vector3(0f, 1.5f, 0f),
                new Vector3(1.7f, 0.18f, 1.2f), col * 0.8f, 0.25f, 0.25f);
            Object.Destroy(lid.GetComponent<Collider>());
        }

        private static void BuildCrate(Transform parent, Vector3 pos, float size, Color col)
        {
            // SOLID — keep collider
            BuildKit.CubeTex("Crate", parent, pos, new Vector3(size, size, size),
                ProceduralTextures.Wood, col, 0.05f, 0.15f, new Vector2(1f, 1f));
        }

        private static void TrashBag(Transform parent, Vector3 pos, float size, Color col)
        {
            // keepCollider = true (solid-ish obstacle)
            BuildKit.Sphere("TrashBag", parent, pos,
                new Vector3(size, size * 0.85f, size), col, 0.0f, 0.55f, true);
        }

        private static void SpawnDealer(Transform alley, Vector3 local, Color shirt, Color pants)
        {
            Vector3 world = alley.TransformPoint(local);
            world.y = 0f;
            var c = BuildKit.Civ("AlleyDealer", world, Civilian.Mode.Idle, shirt, pants);
            if (c != null)
            {
                c.skinColor = new Color(0.62f, 0.5f, 0.42f);
                c.patrolCenter = world;
                c.patrolRadius = 1.5f;
                c.gameObject.AddComponent<DrugDealer>();
            }
        }

        private static void BuildVan(Transform parent, Vector3 basePos)
        {
            Color vanCol = new Color(0.55f, 0.52f, 0.48f); // dirty white
            Color glass  = new Color(0.08f, 0.10f, 0.12f);
            Color tire   = new Color(0.05f, 0.05f, 0.05f);

            // main cargo body (SOLID — keep collider)
            BuildKit.Cube("VanBody", parent, basePos + new Vector3(0f, 1.25f, 0f),
                new Vector3(2.4f, 2.0f, 4.6f), vanCol, 0.2f, 0.25f);
            // cab front (SOLID)
            BuildKit.Cube("VanCab", parent, basePos + new Vector3(0f, 0.95f, 2.6f),
                new Vector3(2.3f, 1.4f, 1.2f), vanCol * 0.95f, 0.2f, 0.25f);

            // windshield + side glass (decor, no collider)
            var ws = BuildKit.Cube("VanWindshield", parent, basePos + new Vector3(0f, 1.5f, 3.2f),
                new Vector3(2.0f, 0.9f, 0.1f), glass, 0.1f, 0.85f);
            Object.Destroy(ws.GetComponent<Collider>());
            var sg = BuildKit.Cube("VanSideGlass", parent, basePos + new Vector3(1.16f, 1.5f, 1.4f),
                new Vector3(0.06f, 0.8f, 2.4f), glass, 0.1f, 0.85f);
            Object.Destroy(sg.GetComponent<Collider>());

            // wheels (decor cylinders, no collider — body collider handles blocking)
            float wy = 0.45f;
            VanWheel(parent, basePos + new Vector3(1.25f, wy, 1.6f), tire);
            VanWheel(parent, basePos + new Vector3(-1.25f, wy, 1.6f), tire);
            VanWheel(parent, basePos + new Vector3(1.25f, wy, -1.6f), tire);
            VanWheel(parent, basePos + new Vector3(-1.25f, wy, -1.6f), tire);
        }

        private static void VanWheel(Transform parent, Vector3 pos, Color col)
        {
            var w = BuildKit.Cylinder("VanWheel", parent, pos,
                new Vector3(0.9f, 0.18f, 0.9f), col, 0.1f, 0.15f);
            w.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Object.Destroy(w.GetComponent<Collider>());
        }
    }
}
