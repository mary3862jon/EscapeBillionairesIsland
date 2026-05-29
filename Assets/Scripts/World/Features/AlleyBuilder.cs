using UnityEngine;

namespace Spoonacci
{
    // Gritty SHADY ALLEY at world (-22, 0, 22) — a dark, grimy contrast to the
    // glossy resort: corridor walls, dumpsters, crates, trash, graffiti, a
    // flickering amber lamp, a sketchy van, and three shady dealer NPCs.
    public static class AlleyBuilder
    {
        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("ShadyAlley", new Vector3(-22f, 0f, 22f));
            if (root != null) hub.transform.SetParent(root, true);
            var T = hub.transform;

            // Palette — dark, desaturated, dirty.
            Color concrete = new Color(0.20f, 0.20f, 0.22f);
            Color concreteDark = new Color(0.13f, 0.13f, 0.15f);
            Color dumpsterGreen = new Color(0.10f, 0.16f, 0.11f);
            Color dumpsterRust = new Color(0.30f, 0.16f, 0.09f);
            Color trashBlack = new Color(0.06f, 0.06f, 0.07f);
            Color paper = new Color(0.78f, 0.74f, 0.66f);

            // The alley corridor runs along the local Z axis. Two tall narrow
            // walls flank a ~5-wide gap. Floor sits between them.
            float corridorHalf = 2.6f;   // half-width of walkable gap
            float wallLen = 22f;         // length of corridor
            float wallH = 7.5f;          // tall walls
            float wallThick = 0.7f;

            // --- Grimy concrete floor slab (FLAT decal, no collider) ---
            var floor = BuildKit.Cube("AlleyFloor", T,
                new Vector3(0f, 0.02f, 0f),
                new Vector3(corridorHalf * 2f + 0.4f, 0.04f, wallLen),
                concreteDark, 0f, 0.05f);
            Object.Destroy(floor.GetComponent<Collider>());

            // Faint grime/stain streaks on the floor (flat decals, no collider).
            for (int i = 0; i < 6; i++)
            {
                float sz = Random.Range(0.6f, 1.6f);
                var stain = BuildKit.Cube("FloorStain", T,
                    new Vector3(Random.Range(-corridorHalf + 0.5f, corridorHalf - 0.5f),
                                0.025f,
                                Random.Range(-wallLen * 0.45f, wallLen * 0.45f)),
                    new Vector3(sz, 0.03f, sz * Random.Range(0.7f, 1.3f)),
                    new Color(0.08f, 0.08f, 0.09f), 0f, 0.02f);
                Object.Destroy(stain.GetComponent<Collider>());
            }

            // --- Two tall narrow corridor walls (KEEP colliders) ---
            BuildWall(T, "WallLeft", new Vector3(-corridorHalf - wallThick * 0.5f, wallH * 0.5f, 0f),
                new Vector3(wallThick, wallH, wallLen), concrete);
            BuildWall(T, "WallRight", new Vector3(corridorHalf + wallThick * 0.5f, wallH * 0.5f, 0f),
                new Vector3(wallThick, wallH, wallLen), concrete);

            // A back wall capping one end of the alley (dead-end vibe).
            BuildWall(T, "WallBack", new Vector3(0f, wallH * 0.5f, -wallLen * 0.5f + wallThick * 0.5f),
                new Vector3(corridorHalf * 2f + wallThick * 2f, wallH, wallThick), concreteDark);

            // --- Graffiti panels (brightly tinted thin cubes ON the walls, no collider) ---
            Color[] graff = {
                new Color(1f, 0.15f, 0.45f),   // hot pink
                new Color(0.2f, 1f, 0.6f),     // toxic green
                new Color(1f, 0.85f, 0.1f),    // acid yellow
                new Color(0.3f, 0.6f, 1f),     // electric blue
                new Color(1f, 0.4f, 0.0f)      // orange
            };
            float wallInner = corridorHalf - 0.02f;
            for (int i = 0; i < 5; i++)
            {
                bool leftSide = (i % 2 == 0);
                float gx = leftSide ? -wallInner : wallInner;
                float gz = Random.Range(-wallLen * 0.4f, wallLen * 0.4f);
                float gy = Random.Range(1.6f, 4.5f);
                var g = BuildKit.Cube("Graffiti", T,
                    new Vector3(gx, gy, gz),
                    new Vector3(0.05f, Random.Range(0.8f, 1.6f), Random.Range(1.0f, 2.2f)),
                    graff[i % graff.Length], 0f, 0.25f);
                Object.Destroy(g.GetComponent<Collider>());
            }

            // --- Dumpsters (dark cubes, KEEP collider so player bumps) ---
            var dumpA = BuildKit.Cube("Dumpster", T,
                new Vector3(-corridorHalf + 1.0f, 0.85f, -4.5f),
                new Vector3(1.9f, 1.7f, 2.8f), dumpsterGreen, 0.3f, 0.2f);
            // slightly open lid
            var lidA = BuildKit.Cube("DumpsterLid", dumpA.transform,
                new Vector3(0f, 0.95f, -0.6f),
                new Vector3(1.0f, 0.12f, 2.9f), dumpsterRust, 0.2f, 0.15f);
            lidA.transform.localRotation = Quaternion.Euler(-25f, 0f, 0f);

            var dumpB = BuildKit.Cube("Dumpster2", T,
                new Vector3(corridorHalf - 1.0f, 0.7f, 3.0f),
                new Vector3(1.7f, 1.4f, 2.4f), dumpsterRust, 0.35f, 0.25f);

            // --- Stacked crates / boxes (Wood texture, KEEP collider) ---
            CrateStack(T, new Vector3(corridorHalf - 0.9f, 0f, -6.0f));
            // a lone crate near the mouth
            BuildKit.CubeTex("Crate", T,
                new Vector3(-corridorHalf + 0.8f, 0.4f, 6.0f),
                new Vector3(0.8f, 0.8f, 0.8f),
                ProceduralTextures.Wood, new Color(0.6f, 0.5f, 0.38f), 0f, 0.2f,
                new Vector2(1f, 1f));

            // --- Trash bags (dark spheres scattered, KEEP collider to bump) ---
            for (int i = 0; i < 7; i++)
            {
                float bx = Random.Range(-corridorHalf + 0.6f, corridorHalf - 0.6f);
                float bz = Random.Range(-wallLen * 0.45f, wallLen * 0.45f);
                float bs = Random.Range(0.5f, 0.9f);
                BuildKit.Sphere("TrashBag", T,
                    new Vector3(bx, bs * 0.45f, bz),
                    new Vector3(bs, bs * 0.85f, bs),
                    Color.Lerp(trashBlack, new Color(0.12f, 0.12f, 0.10f), Random.value),
                    0f, 0.08f, true);
            }

            // --- Scattered loose papers (flat thin decals, no collider) ---
            for (int i = 0; i < 10; i++)
            {
                var p = BuildKit.Cube("Paper", T,
                    new Vector3(Random.Range(-corridorHalf + 0.4f, corridorHalf - 0.4f),
                                0.03f,
                                Random.Range(-wallLen * 0.45f, wallLen * 0.45f)),
                    new Vector3(0.28f, 0.02f, 0.36f),
                    Color.Lerp(paper, Color.gray, Random.value * 0.4f), 0f, 0.05f);
                p.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                Object.Destroy(p.GetComponent<Collider>());
            }

            // --- Flickering amber point light (moody) ---
            var lgo = new GameObject("AlleyLamp");
            lgo.transform.position = T.TransformPoint(new Vector3(0f, 5.5f, -wallLen * 0.25f));
            lgo.transform.SetParent(T, true);
            var l = lgo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.55f, 0.18f);
            l.intensity = 3.5f;
            l.range = 13f;
            l.shadows = LightShadows.Soft;
            lgo.AddComponent<FlickerLight>();
            // A small physical lamp housing on the wall under the light.
            BuildKit.Cube("LampBox", T,
                new Vector3(corridorHalf - 0.05f, 5.4f, -wallLen * 0.25f),
                new Vector3(0.25f, 0.4f, 0.5f),
                new Color(0.05f, 0.05f, 0.05f), 0.4f, 0.3f);

            // --- Cryptic neon sign on the wall ---
            BuildKit.Label(dumpA, "Alley", new Color(1f, 0.4f, 0.1f), 30,
                new Vector3(0f, 2.4f, 0f));

            // --- Sketchy parked van at the alley mouth (cubes, KEEP collider) ---
            BuildVan(T, new Vector3(corridorHalf + 1.6f, 0f, wallLen * 0.5f - 1.5f));

            // --- 3 shady dealer NPCs (Idle, dark clothes, DrugDealer marker) ---
            Color[] darkShirts = {
                new Color(0.10f, 0.10f, 0.12f),
                new Color(0.14f, 0.10f, 0.10f),
                new Color(0.08f, 0.12f, 0.10f)
            };
            Vector3[] dealerLocal = {
                new Vector3(-corridorHalf + 1.1f, 0f, -2.0f),
                new Vector3(corridorHalf - 1.1f, 0f, 1.0f),
                new Vector3(0f, 0f, -wallLen * 0.4f)
            };
            for (int i = 0; i < 3; i++)
            {
                Vector3 wpos = T.TransformPoint(dealerLocal[i]);
                var c = BuildKit.Civ("AlleyDealer" + i, wpos, Civilian.Mode.Idle,
                    darkShirts[i], new Color(0.07f, 0.07f, 0.08f));
                c.gameObject.AddComponent<DrugDealer>();
            }
        }

        // Solid wall block — keeps its collider so the player bumps into it.
        private static void BuildWall(Transform parent, string name, Vector3 localPos, Vector3 scale, Color c)
        {
            var w = BuildKit.Cube(name, parent, localPos, scale, c, 0f, 0.08f);
            // subtle grime band near the base
            var grime = BuildKit.Cube(name + "Grime", w.transform,
                new Vector3(0f, -0.42f, 0f), new Vector3(1.02f, 0.16f, 1.02f),
                new Color(0.07f, 0.07f, 0.08f), 0f, 0.05f);
            Object.Destroy(grime.GetComponent<Collider>());
        }

        // A small stack of wooden crates (KEEP colliders).
        private static void CrateStack(Transform parent, Vector3 baseLocal)
        {
            var tint = new Color(0.55f, 0.45f, 0.33f);
            BuildKit.CubeTex("Crate", parent,
                baseLocal + new Vector3(0f, 0.45f, 0f), new Vector3(0.9f, 0.9f, 0.9f),
                ProceduralTextures.Wood, tint, 0f, 0.2f, new Vector2(1f, 1f));
            BuildKit.CubeTex("Crate", parent,
                baseLocal + new Vector3(0.15f, 1.35f, 0.1f), new Vector3(0.8f, 0.8f, 0.8f),
                ProceduralTextures.Wood, tint * 0.92f, 0f, 0.2f, new Vector2(1f, 1f));
            BuildKit.CubeTex("Crate", parent,
                baseLocal + new Vector3(-0.4f, 0.4f, 0.7f), new Vector3(0.75f, 0.8f, 0.75f),
                ProceduralTextures.Wood, tint * 1.05f, 0f, 0.2f, new Vector2(1f, 1f));
        }

        // Sketchy panel van built from cubes (KEEP colliders).
        private static void BuildVan(Transform parent, Vector3 baseLocal)
        {
            var bodyCol = new Color(0.22f, 0.22f, 0.24f);
            var van = BuildKit.Root("SketchyVan", Vector3.zero);
            van.transform.SetParent(parent, false);
            van.transform.localPosition = baseLocal;
            van.transform.localRotation = Quaternion.Euler(0f, 12f, 0f);
            var vt = van.transform;

            // main cargo body
            BuildKit.Cube("VanBody", vt, new Vector3(0f, 1.1f, -0.3f),
                new Vector3(1.9f, 1.9f, 3.6f), bodyCol, 0.3f, 0.3f);
            // cab
            BuildKit.Cube("VanCab", vt, new Vector3(0f, 0.95f, 2.0f),
                new Vector3(1.85f, 1.5f, 1.4f), bodyCol * 0.95f, 0.3f, 0.3f);
            // windshield
            var ws = BuildKit.Cube("VanGlass", vt, new Vector3(0f, 1.45f, 2.72f),
                new Vector3(1.6f, 0.7f, 0.08f), new Color(0.05f, 0.07f, 0.09f), 0.6f, 0.7f);
            Object.Destroy(ws.GetComponent<Collider>());
            // bumper
            BuildKit.Cube("VanBumper", vt, new Vector3(0f, 0.45f, 2.78f),
                new Vector3(1.9f, 0.25f, 0.2f), new Color(0.05f, 0.05f, 0.05f), 0.5f, 0.4f);

            // wheels (no collider needed; body has it)
            float[] wz = { 1.7f, -1.3f };
            for (int s = 0; s < 2; s++)
            {
                for (int w = 0; w < 2; w++)
                {
                    float wx = (w == 0) ? -0.95f : 0.95f;
                    var wheel = BuildKit.Cylinder("VanWheel", vt,
                        new Vector3(wx, 0.45f, wz[s]),
                        new Vector3(0.5f, 0.18f, 0.5f),
                        new Color(0.04f, 0.04f, 0.04f), 0.1f, 0.2f);
                    wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    Object.Destroy(wheel.GetComponent<Collider>());
                }
            }
        }
    }
}
