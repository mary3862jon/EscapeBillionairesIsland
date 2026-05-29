using UnityEngine;
namespace Spoonacci
{
    public static class PerimeterBuilder
    {
        public static void Build(Transform root)
        {
            var hedgeRoot = BuildKit.Root("PerimeterHedge", Vector3.zero);
            hedgeRoot.transform.SetParent(root, true);

            // Tunables for the boundary ring.
            const float bound = 95f;      // perimeter distance from origin
            const float segLen = 6f;      // length of each hedge block
            const float overlap = 1.2f;   // overlap so there are no gaps
            const float step = segLen - overlap;
            const float hedgeH = 3.6f;    // hedge height
            const float hedgeT = 1.6f;    // hedge thickness

            // Lay the four sides. The gate gap (for the blocked road) is on the -z side near x=0.
            BuildSide(hedgeRoot.transform, bound, segLen, step, hedgeH, hedgeT, Side.North); // +z
            BuildSide(hedgeRoot.transform, bound, segLen, step, hedgeH, hedgeT, Side.East);  // +x
            BuildSide(hedgeRoot.transform, bound, segLen, step, hedgeH, hedgeT, Side.West);  // -x
            BuildSide(hedgeRoot.transform, bound, segLen, step, hedgeH, hedgeT, Side.South); // -z (with road gap)

            BuildBlockedRoad(root);
        }

        private enum Side { North, East, South, West }

        private static void BuildSide(Transform parent, float bound, float segLen, float step,
                                      float hedgeH, float hedgeT, Side side)
        {
            // The road gap straddles x in [-7.5, 7.5] on the South (-z) side.
            const float gapHalf = 7.5f;

            Color hedgeTint = new Color(0.32f, 0.5f, 0.22f);
            Vector2 tile = new Vector2(2f, 1f);

            int idx = 0;
            for (float t = -bound; t <= bound + 0.01f; t += step, idx++)
            {
                Vector3 pos;
                Vector3 scale;
                bool skip = false;

                switch (side)
                {
                    case Side.North: // along +z edge, varying x
                        pos = new Vector3(t, hedgeH * 0.5f, bound);
                        scale = new Vector3(segLen, hedgeH, hedgeT);
                        break;
                    case Side.South: // along -z edge, varying x  (road gap here)
                        pos = new Vector3(t, hedgeH * 0.5f, -bound);
                        scale = new Vector3(segLen, hedgeH, hedgeT);
                        if (t > -gapHalf && t < gapHalf) skip = true;
                        break;
                    case Side.East: // along +x edge, varying z
                        pos = new Vector3(bound, hedgeH * 0.5f, t);
                        scale = new Vector3(hedgeT, hedgeH, segLen);
                        break;
                    default: // West, along -x edge, varying z
                        pos = new Vector3(-bound, hedgeH * 0.5f, t);
                        scale = new Vector3(hedgeT, hedgeH, segLen);
                        break;
                }

                if (!skip)
                {
                    // Slight per-segment height jitter so the hedge looks hand-trimmed, not extruded.
                    float jitter = (Random.value - 0.5f) * 0.35f;
                    var s2 = scale;
                    s2.y = hedgeH + jitter;
                    var p2 = pos;
                    p2.y = s2.y * 0.5f;

                    // KEEP collider — this is the real blocking wall.
                    BuildKit.CubeTex("Hedge_" + side + "_" + idx, parent, p2, s2,
                        ProceduralTextures.Leaves, hedgeTint, 0f, 0.15f, tile);
                }

                // A decorative stone+iron fence post at every other segment boundary.
                if (idx % 2 == 0)
                {
                    Vector3 postPos;
                    switch (side)
                    {
                        case Side.North: postPos = new Vector3(t - step * 0.5f, 0f, bound); break;
                        case Side.South: postPos = new Vector3(t - step * 0.5f, 0f, -bound); break;
                        case Side.East:  postPos = new Vector3(bound, 0f, t - step * 0.5f); break;
                        default:         postPos = new Vector3(-bound, 0f, t - step * 0.5f); break;
                    }
                    if (Mathf.Abs(postPos.x) <= 99f && Mathf.Abs(postPos.z) <= 99f)
                        BuildPost(parent, postPos);
                }
            }
        }

        private static void BuildPost(Transform parent, Vector3 basePos)
        {
            // Stone plinth (keep collider) + iron finial sphere on top.
            float plinthH = 4.0f;
            Color stone = new Color(0.55f, 0.54f, 0.5f);
            var p = basePos;
            p.y = plinthH * 0.5f;
            BuildKit.CubeTex("FencePost", parent, p, new Vector3(0.7f, plinthH, 0.7f),
                ProceduralTextures.Stone, stone, 0.1f, 0.3f, new Vector2(1f, 2f));

            var top = basePos;
            top.y = plinthH + 0.25f;
            BuildKit.Sphere("PostFinial", parent, top, new Vector3(0.5f, 0.5f, 0.5f),
                new Color(0.12f, 0.12f, 0.14f), 0.85f, 0.7f);
        }

        // ---------------------------------------------------------------------
        // Diegetic blocked road on the South (-z) side, centered near (0, -90).
        // ---------------------------------------------------------------------
        private static void BuildBlockedRoad(Transform root)
        {
            var rd = BuildKit.Root("BlockedRoad", new Vector3(0f, 0f, -90f));
            rd.transform.SetParent(root, true);
            Transform R = rd.transform;

            // Asphalt road slab leading to the (sealed) exit — flat decal, no collider.
            var slab = BuildKit.Cube("RoadSlab", R, new Vector3(0f, 0.02f, 0f),
                new Vector3(14f, 0.04f, 16f), new Color(0.15f, 0.15f, 0.16f), 0f, 0.1f);
            Object.Destroy(slab.GetComponent<Collider>());

            // Dashed center line — flat decals.
            for (int i = -3; i <= 3; i++)
            {
                var dash = BuildKit.Cube("RoadDash_" + i, R, new Vector3(0f, 0.03f, i * 2.2f),
                    new Vector3(0.4f, 0.04f, 1.1f), new Color(0.85f, 0.78f, 0.2f), 0f, 0.1f);
                Object.Destroy(dash.GetComponent<Collider>());
            }

            // --- Crashed / parked cars (keep colliders) ---
            BuildCar(R, new Vector3(-3.2f, 0f, 1.5f), 0f, new Color(0.7f, 0.1f, 0.1f));      // parked
            BuildCar(R, new Vector3(3.0f, 0f, 3.5f), 24f, new Color(0.15f, 0.3f, 0.6f));     // skidded/rotated

            // --- Construction barriers (white/orange thin striped cubes, keep colliders) ---
            for (int i = 0; i < 5; i++)
            {
                float x = -6f + i * 3f;
                bool orange = (i % 2 == 0);
                Color c = orange ? new Color(0.95f, 0.45f, 0.05f) : new Color(0.92f, 0.92f, 0.9f);
                BuildKit.Cube("Barrier_" + i, R, new Vector3(x, 0.6f, -3.5f),
                    new Vector3(2.6f, 1.2f, 0.25f), c, 0f, 0.25f);
                // little stand legs
                BuildKit.Cube("BarrierLeg_" + i, R, new Vector3(x, 0.2f, -3.5f),
                    new Vector3(0.2f, 0.4f, 1.0f), new Color(0.3f, 0.3f, 0.32f), 0.2f, 0.3f);
            }

            // --- Traffic cones (orange, keep colliders) ---
            float[] coneX = { -5.5f, -2f, 1.5f, 4.5f, -3.8f, 2.8f };
            float[] coneZ = { -1.5f, -1.0f, -1.6f, -1.2f, 5.5f, 6.0f };
            for (int i = 0; i < coneX.Length; i++)
            {
                BuildCone(R, new Vector3(coneX[i], 0f, coneZ[i]));
            }

            // --- Ribbon tape strung between two posts (thin bright cubes, NO collider) ---
            BuildTapeRun(R, new Vector3(-7f, 0f, -1.5f), new Vector3(7f, 0f, -1.5f));

            // --- "ROAD CLOSED" sign ---
            BuildKit.Cylinder("SignPost", R, new Vector3(0f, 1.4f, -4.2f),
                new Vector3(0.15f, 2.8f, 0.15f), new Color(0.4f, 0.4f, 0.42f), 0.4f, 0.4f);
            var signBoard = BuildKit.Cube("SignBoard", R, new Vector3(0f, 3.0f, -4.2f),
                new Vector3(3.0f, 1.0f, 0.1f), new Color(0.9f, 0.2f, 0.15f), 0f, 0.3f);
            BuildKit.Label(signBoard, "ROAD CLOSED — CONSTRUCTION",
                Color.white, 22, new Vector3(0f, 0f, -0.12f));

            // --- Construction worker NPCs (Idle, hi-vis orange) ---
            Color hiVis = new Color(0.97f, 0.5f, 0.05f);
            Color darkPants = new Color(0.2f, 0.2f, 0.25f);
            var w1 = BuildKit.Civ("Worker_A", new Vector3(-4f, 0f, -88f), Civilian.Mode.Idle, hiVis, darkPants);
            var w2 = BuildKit.Civ("Worker_B", new Vector3(5f, 0f, -85f), Civilian.Mode.Idle, hiVis, darkPants);
            if (w1 != null) w1.transform.SetParent(root, true);
            if (w2 != null) w2.transform.SetParent(root, true);
        }

        private static void BuildCar(Transform parent, Vector3 localPos, float yawDeg, Color body)
        {
            var car = BuildKit.Root("Car", Vector3.zero);
            car.transform.SetParent(parent, false);
            car.transform.localPosition = localPos;
            car.transform.localRotation = Quaternion.Euler(0f, yawDeg, 0f);
            Transform C = car.transform;

            // Lower body (keep collider so player bumps into it).
            BuildKit.Cube("Body", C, new Vector3(0f, 0.55f, 0f),
                new Vector3(2.0f, 0.8f, 4.2f), body, 0.35f, 0.55f);
            // Cabin
            BuildKit.Cube("Cabin", C, new Vector3(0f, 1.25f, -0.2f),
                new Vector3(1.8f, 0.7f, 2.2f), body * 0.85f, 0.35f, 0.55f);
            // Dark window strips
            Color glass = new Color(0.08f, 0.1f, 0.12f);
            BuildKit.Cube("WinFront", C, new Vector3(0f, 1.3f, 0.95f),
                new Vector3(1.7f, 0.55f, 0.08f), glass, 0.1f, 0.85f);
            BuildKit.Cube("WinBack", C, new Vector3(0f, 1.3f, -1.35f),
                new Vector3(1.7f, 0.55f, 0.08f), glass, 0.1f, 0.85f);
            BuildKit.Cube("WinL", C, new Vector3(-0.92f, 1.3f, -0.2f),
                new Vector3(0.08f, 0.5f, 2.0f), glass, 0.1f, 0.85f);
            BuildKit.Cube("WinR", C, new Vector3(0.92f, 1.3f, -0.2f),
                new Vector3(0.08f, 0.5f, 2.0f), glass, 0.1f, 0.85f);

            // Wheels (cylinders, rotated to roll along z; keep colliders).
            Color tire = new Color(0.06f, 0.06f, 0.07f);
            float[] wx = { -1.0f, 1.0f, -1.0f, 1.0f };
            float[] wz = { 1.4f, 1.4f, -1.4f, -1.4f };
            for (int i = 0; i < 4; i++)
            {
                var wheel = BuildKit.Cylinder("Wheel_" + i, C, new Vector3(wx[i], 0.4f, wz[i]),
                    new Vector3(0.7f, 0.18f, 0.7f), tire, 0.2f, 0.3f);
                wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
        }

        private static void BuildCone(Transform parent, Vector3 localPos)
        {
            Color orange = new Color(0.97f, 0.42f, 0.05f);
            // Flat base
            BuildKit.Cube("ConeBase", parent, localPos + new Vector3(0f, 0.04f, 0f),
                new Vector3(0.7f, 0.08f, 0.7f), orange * 0.8f, 0f, 0.2f);
            // Tapered stack of cylinders to fake a cone shape (keep colliders).
            BuildKit.Cylinder("ConeLow", parent, localPos + new Vector3(0f, 0.25f, 0f),
                new Vector3(0.5f, 0.25f, 0.5f), orange, 0f, 0.25f);
            BuildKit.Cylinder("ConeMid", parent, localPos + new Vector3(0f, 0.6f, 0f),
                new Vector3(0.3f, 0.18f, 0.3f), new Color(0.95f, 0.95f, 0.95f), 0f, 0.3f);
            BuildKit.Cylinder("ConeTop", parent, localPos + new Vector3(0f, 0.85f, 0f),
                new Vector3(0.15f, 0.12f, 0.15f), orange, 0f, 0.25f);
        }

        private static void BuildTapeRun(Transform parent, Vector3 a, Vector3 b)
        {
            // Two posts (keep colliders).
            BuildKit.Cylinder("TapePostA", parent, a + new Vector3(0f, 0.7f, 0f),
                new Vector3(0.12f, 1.4f, 0.12f), new Color(0.35f, 0.35f, 0.38f), 0.3f, 0.4f);
            BuildKit.Cylinder("TapePostB", parent, b + new Vector3(0f, 0.7f, 0f),
                new Vector3(0.12f, 1.4f, 0.12f), new Color(0.35f, 0.35f, 0.38f), 0.3f, 0.4f);

            // Tape as thin bright cubes spanning between posts at two heights — NO collider.
            float length = Vector3.Distance(a, b);
            Vector3 mid = (a + b) * 0.5f;
            Vector3 dir = (b - a).normalized;
            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            Color red = new Color(0.85f, 0.12f, 0.1f);
            Color white = new Color(0.95f, 0.95f, 0.92f);
            float[] heights = { 0.85f, 1.25f };
            for (int h = 0; h < heights.Length; h++)
            {
                // Split into red/white striped chunks.
                int chunks = 8;
                float chunkLen = length / chunks;
                for (int i = 0; i < chunks; i++)
                {
                    float along = -length * 0.5f + (i + 0.5f) * chunkLen;
                    Color c = (i % 2 == 0) ? red : white;
                    var tape = BuildKit.Cube("Tape_" + h + "_" + i, parent, Vector3.zero,
                        Vector3.one, c, 0f, 0.2f);
                    tape.transform.localPosition = new Vector3(
                        mid.x + dir.x * along, heights[h], mid.z + dir.z * along);
                    tape.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
                    tape.transform.localScale = new Vector3(0.06f, 0.1f, chunkLen * 0.95f);
                    Object.Destroy(tape.GetComponent<Collider>());
                }
            }
        }
    }
}
