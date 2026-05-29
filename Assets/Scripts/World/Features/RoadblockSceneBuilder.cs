using UnityEngine;

namespace Spoonacci
{
    // A diegetic "you can't leave the island this way" scene near the south
    // boundary: a road that appears to head off the island is blocked by a
    // staged traffic accident + roadworks. Two crashed/parked cars, orange
    // cones, striped construction barriers, red/white ribbon tape strung
    // between posts, a "ROAD CLOSED" sign, a gravel/dirt pile, and a few
    // hi-vis idle workers.
    //
    // This is a "Roadblock" builder: it intentionally sits inside the
    // road-exit corridor at the south edge, so it is EXEMPT from the Blocked()
    // scatter rule for its own zone. Everything still stays within the world.
    public static class RoadblockSceneBuilder
    {
        // Scene anchor: a road leaving the island at the south boundary.
        static readonly Vector3 Anchor = new Vector3(0f, 0f, -88f);

        // Palette.
        static readonly Color Orange   = new Color(0.95f, 0.42f, 0.06f);
        static readonly Color HiVis    = new Color(0.98f, 0.55f, 0.05f);
        static readonly Color White    = new Color(0.93f, 0.93f, 0.90f);
        static readonly Color Red       = new Color(0.82f, 0.10f, 0.10f);
        static readonly Color Dark      = new Color(0.06f, 0.06f, 0.08f);
        static readonly Color Glass     = new Color(0.10f, 0.13f, 0.16f);
        static readonly Color Dirt      = new Color(0.36f, 0.26f, 0.15f);
        static readonly Color Gravel    = new Color(0.45f, 0.42f, 0.38f);
        static readonly Color Metal     = new Color(0.55f, 0.56f, 0.60f);
        static readonly Color Pants     = new Color(0.14f, 0.14f, 0.18f);

        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("RoadblockScene", Anchor);
            if (root != null) hub.transform.SetParent(root, true);
            var t = hub.transform;

            // Faint roadworks dirt patch decal so the asphalt reads "torn up".
            BuildDirtDecal(t);

            // Two cars: one parked square across the road, one skidded & tilted.
            BuildCar(t, new Vector3(-3.2f, 0f, 1.5f), 0f, new Color(0.70f, 0.12f, 0.12f));   // red sedan, square
            BuildCar(t, new Vector3(3.0f, 0f, -0.8f), 22f, new Color(0.18f, 0.30f, 0.62f), tilt: 6f); // blue, skidded

            // A line of orange traffic cones fanned across the road.
            BuildConeLine(t);

            // Two striped construction barriers (A-frame trestles).
            BuildBarrier(t, new Vector3(-5.5f, 0f, 3.0f), 8f);
            BuildBarrier(t, new Vector3(5.2f, 0f, 3.4f), -10f);

            // Red/white ribbon tape strung between short posts across the gap.
            BuildRibbonRun(t, new Vector3(-7f, 0f, 4.2f), new Vector3(7f, 0f, 4.2f));

            // Gravel / dirt pile (the "excavator left a mess" look).
            BuildGravelPile(t, new Vector3(-1.5f, 0f, 5.5f));
            BuildMiniDigger(t, new Vector3(4.0f, 0f, 6.0f), -30f);

            // The headline sign.
            BuildSign(t);

            // Hi-vis idle workers loitering by the barrier.
            BuildWorkers(t);
        }

        // ---------------------------------------------------------------- decal

        static void BuildDirtDecal(Transform parent)
        {
            var d = BuildKit.Cube("RoadworksDirt", parent,
                new Vector3(0f, 0.02f, 2.5f),
                new Vector3(16f, 0.04f, 9f),
                new Color(0.30f, 0.24f, 0.17f), 0f, 0.12f);
            Strip(d);

            // A couple of darker churned patches.
            for (int i = 0; i < 4; i++)
            {
                float x = -5f + i * 3.3f;
                var p = BuildKit.Cube("DirtPatch", parent,
                    new Vector3(x, 0.025f, 1.5f + (i % 2) * 2.4f),
                    new Vector3(2.6f, 0.04f, 2.0f),
                    new Color(0.22f, 0.17f, 0.11f), 0f, 0.1f);
                Strip(p);
            }
        }

        // ------------------------------------------------------------------ car

        // Boxy cube body + dark window strip + 4 cylinder wheels. KEEP colliders
        // on the chassis so the player is physically blocked.
        static void BuildCar(Transform parent, Vector3 local, float yaw, Color body, float tilt = 0f)
        {
            var carGo = BuildKit.Root("Car", parent.position); // temp world; reparent
            carGo.transform.SetParent(parent, false);
            carGo.transform.localPosition = local;
            carGo.transform.localRotation = Quaternion.Euler(tilt, yaw, tilt * 0.4f);
            var c = carGo.transform;

            // Lower body (chassis) — solid obstacle, KEEP collider.
            BuildKit.Cube("Chassis", c,
                new Vector3(0f, 0.55f, 0f), new Vector3(2.0f, 0.8f, 4.2f),
                body, 0.25f, 0.55f);

            // Cabin / greenhouse — solid, KEEP collider.
            BuildKit.Cube("Cabin", c,
                new Vector3(0f, 1.2f, -0.2f), new Vector3(1.8f, 0.75f, 2.2f),
                body, 0.25f, 0.55f);

            // Window strips (dark glass) wrapping the cabin — decorative, strip.
            var winSide = BuildKit.Cube("WinL", c,
                new Vector3(0.92f, 1.25f, -0.2f), new Vector3(0.06f, 0.5f, 2.0f),
                Glass, 0.1f, 0.85f);
            Strip(winSide);
            var winR = BuildKit.Cube("WinR", c,
                new Vector3(-0.92f, 1.25f, -0.2f), new Vector3(0.06f, 0.5f, 2.0f),
                Glass, 0.1f, 0.85f);
            Strip(winR);
            var windshield = BuildKit.Cube("Windshield", c,
                new Vector3(0f, 1.3f, 0.9f), new Vector3(1.7f, 0.55f, 0.06f),
                Glass, 0.1f, 0.9f);
            Strip(windshield);
            var rearWin = BuildKit.Cube("RearWin", c,
                new Vector3(0f, 1.3f, -1.3f), new Vector3(1.7f, 0.5f, 0.06f),
                Glass, 0.1f, 0.9f);
            Strip(rearWin);

            // Headlights / tail lights as small accents.
            var hlL = BuildKit.Cube("HeadL", c, new Vector3(0.6f, 0.55f, 2.12f),
                new Vector3(0.3f, 0.2f, 0.06f), new Color(1f, 0.95f, 0.7f), 0.2f, 0.9f);
            Strip(hlL);
            var hlR = BuildKit.Cube("HeadR", c, new Vector3(-0.6f, 0.55f, 2.12f),
                new Vector3(0.3f, 0.2f, 0.06f), new Color(1f, 0.95f, 0.7f), 0.2f, 0.9f);
            Strip(hlR);
            var tl = BuildKit.Cube("TailLights", c, new Vector3(0f, 0.55f, -2.12f),
                new Vector3(1.6f, 0.18f, 0.06f), Red, 0.2f, 0.8f);
            Strip(tl);

            // 4 wheels — cylinders rotated to lie as discs facing X.
            var wheelRot = Quaternion.Euler(0f, 0f, 90f);
            float wr = 0.42f;
            float wy = wr;
            float wx = 1.02f, wz = 1.45f;
            BuildWheel(c, "WheelFL", new Vector3(wx, wy, wz), wr, wheelRot);
            BuildWheel(c, "WheelFR", new Vector3(-wx, wy, wz), wr, wheelRot);
            BuildWheel(c, "WheelRL", new Vector3(wx, wy, -wz), wr, wheelRot);
            BuildWheel(c, "WheelRR", new Vector3(-wx, wy, -wz), wr, wheelRot);
        }

        static void BuildWheel(Transform parent, string name, Vector3 pos, float r, Quaternion rot)
        {
            var w = BuildKit.Cylinder(name, parent, pos,
                new Vector3(r * 2f, 0.18f, r * 2f), Dark, 0.2f, 0.35f);
            w.transform.localRotation = rot;
            Strip(w); // wheels decorative; chassis already blocks
            var hub = BuildKit.Sphere(name + "_Hub", w.transform,
                Vector3.zero, new Vector3(0.45f, 0.45f, 0.45f), Metal, 0.6f, 0.7f);
            Strip(hub);
        }

        // ---------------------------------------------------------------- cones

        static void BuildConeLine(Transform parent)
        {
            // A diagonal fan of cones in front of the cars (closer to island side).
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                float x = Mathf.Lerp(-7f, 7f, t);
                float z = 7.5f - Mathf.Abs(x) * 0.18f; // gentle arc
                BuildCone(parent, new Vector3(x, 0f, z));
            }
        }

        static void BuildCone(Transform parent, Vector3 local)
        {
            var coneGo = BuildKit.Root("Cone", parent.position);
            coneGo.transform.SetParent(parent, false);
            coneGo.transform.localPosition = local;
            var c = coneGo.transform;

            // Square base — solid little obstacle, KEEP collider.
            BuildKit.Cube("ConeBase", c, new Vector3(0f, 0.04f, 0f),
                new Vector3(0.5f, 0.08f, 0.5f), Dark, 0f, 0.3f);

            // Tapered body via stacked shrinking cylinders.
            int seg = 4;
            for (int s = 0; s < seg; s++)
            {
                float ty = 0.1f + s * 0.16f;
                float rad = Mathf.Lerp(0.34f, 0.07f, s / (float)(seg - 1));
                var ring = BuildKit.Cylinder("ConeSeg", c,
                    new Vector3(0f, ty, 0f), new Vector3(rad, 0.09f, rad),
                    Orange, 0f, 0.4f);
                Strip(ring); // body decorative; base holds collider
            }
            // White reflective stripe band.
            var band = BuildKit.Cylinder("ConeStripe", c,
                new Vector3(0f, 0.26f, 0f), new Vector3(0.27f, 0.06f, 0.27f),
                White, 0.1f, 0.6f);
            Strip(band);
        }

        // --------------------------------------------------------------- barrier

        // Striped white/orange plank on two A-frame legs. KEEP collider (solid).
        static void BuildBarrier(Transform parent, Vector3 local, float yaw)
        {
            var bGo = BuildKit.Root("Barrier", parent.position);
            bGo.transform.SetParent(parent, false);
            bGo.transform.localPosition = local;
            bGo.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            var b = bGo.transform;

            float plankLen = 3.6f;
            float plankY = 0.85f;

            // Plank backing (solid) — KEEP collider.
            BuildKit.Cube("Plank", b, new Vector3(0f, plankY, 0f),
                new Vector3(plankLen, 0.32f, 0.1f), White, 0f, 0.4f);

            // Diagonal orange stripes painted on the plank face (decorative).
            int stripes = 6;
            for (int i = 0; i < stripes; i++)
            {
                float x = Mathf.Lerp(-plankLen * 0.4f, plankLen * 0.4f, i / (float)(stripes - 1));
                var st = BuildKit.Cube("Stripe", b,
                    new Vector3(x, plankY, 0.06f),
                    new Vector3(0.28f, 0.34f, 0.04f), Orange, 0f, 0.45f);
                st.transform.localRotation = Quaternion.Euler(0f, 0f, 38f);
                Strip(st);
            }

            // Legs (A-frame) — KEEP collider so they stand as obstacles.
            for (int s = -1; s <= 1; s += 2)
            {
                float lx = plankLen * 0.42f * s;
                var legF = BuildKit.Cube("LegFront", b,
                    new Vector3(lx, plankY * 0.5f, 0.18f),
                    new Vector3(0.1f, plankY, 0.1f), Metal, 0.6f, 0.5f);
                legF.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
                var legB = BuildKit.Cube("LegBack", b,
                    new Vector3(lx, plankY * 0.5f, -0.18f),
                    new Vector3(0.1f, plankY, 0.1f), Metal, 0.6f, 0.5f);
                legB.transform.localRotation = Quaternion.Euler(-18f, 0f, 0f);
            }
        }

        // ----------------------------------------------------------------- ribbon

        // Short posts (KEEP collider) with red/white tape segments (collider REMOVED).
        static void BuildRibbonRun(Transform parent, Vector3 a, Vector3 b)
        {
            float tapeY = 0.85f;
            // Posts at each end and at midpoint.
            var pA = BuildPost(parent, a);
            var pMid = BuildPost(parent, (a + b) * 0.5f);
            var pB = BuildPost(parent, b);

            BuildTapeSpan(parent, a, (a + b) * 0.5f, tapeY);
            BuildTapeSpan(parent, (a + b) * 0.5f, b, tapeY);
        }

        static GameObject BuildPost(Transform parent, Vector3 local)
        {
            var post = BuildKit.Cylinder("RibbonPost", parent,
                new Vector3(local.x, 0.55f, local.z),
                new Vector3(0.14f, 0.55f, 0.14f), Metal, 0.6f, 0.5f);
            // KEEP collider — it's a solid post.
            var cap = BuildKit.Sphere("PostCap", post.transform,
                new Vector3(0f, 1.0f, 0f), new Vector3(0.5f, 0.5f, 0.5f), Red, 0.2f, 0.6f);
            Strip(cap);
            return post;
        }

        // A thin bright tape cube spanning two points, collider REMOVED.
        static void BuildTapeSpan(Transform parent, Vector3 a, Vector3 b, float y)
        {
            Vector3 mid = new Vector3((a.x + b.x) * 0.5f, y, (a.z + b.z) * 0.5f);
            Vector3 dir = new Vector3(b.x - a.x, 0f, b.z - a.z);
            float len = dir.magnitude;
            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            // Build two stacked thin bands: red then white pattern via 2 slabs.
            var tape = BuildKit.Cube("RibbonTape", parent, mid,
                new Vector3(0.04f, 0.14f, len), Red, 0f, 0.3f);
            tape.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            Strip(tape); // pass-through decoration

            // White hazard dashes layered on top of the red base.
            int dashes = Mathf.Max(2, Mathf.RoundToInt(len / 0.6f));
            for (int i = 0; i < dashes; i += 2)
            {
                float tt = (i + 0.5f) / dashes;
                Vector3 p = Vector3.Lerp(a, b, tt);
                var dash = BuildKit.Cube("TapeDash", parent,
                    new Vector3(p.x, y, p.z),
                    new Vector3(0.05f, 0.15f, 0.3f), White, 0f, 0.4f);
                dash.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
                Strip(dash);
            }
        }

        // --------------------------------------------------------------- gravel

        static void BuildGravelPile(Transform parent, Vector3 local)
        {
            var pileGo = BuildKit.Root("GravelPile", parent.position);
            pileGo.transform.SetParent(parent, false);
            pileGo.transform.localPosition = local;
            var p = pileGo.transform;

            // Mound base — KEEP collider (you can't walk through the pile).
            BuildKit.Cube("PileBase", p, new Vector3(0f, 0.35f, 0f),
                new Vector3(3.2f, 0.7f, 2.4f), Dirt, 0f, 0.18f);
            // Heaped top via shrinking cubes.
            var mid = BuildKit.Cube("PileMid", p, new Vector3(-0.1f, 0.85f, 0.1f),
                new Vector3(2.2f, 0.6f, 1.6f), Gravel, 0f, 0.15f);
            Strip(mid);
            var top = BuildKit.Cube("PileTop", p, new Vector3(0.15f, 1.25f, -0.05f),
                new Vector3(1.2f, 0.5f, 0.9f), Dirt, 0f, 0.15f);
            Strip(top);

            // A scatter of loose gravel chunks (decorative).
            for (int i = 0; i < 8; i++)
            {
                float rx = (UnityEngine.Random.value - 0.5f) * 4.0f;
                float rz = (UnityEngine.Random.value - 0.5f) * 3.0f;
                float s = 0.18f + UnityEngine.Random.value * 0.22f;
                var rock = BuildKit.Sphere("GravelChunk", p,
                    new Vector3(rx, s * 0.5f, rz), new Vector3(s, s * 0.7f, s),
                    Color.Lerp(Gravel, Dirt, UnityEngine.Random.value), 0f, 0.2f);
                Strip(rock);
            }

            // A leaning shovel stuck in the pile.
            var handle = BuildKit.Cylinder("Shovel", p, new Vector3(0.6f, 1.0f, 0.2f),
                new Vector3(0.07f, 0.9f, 0.07f), new Color(0.5f, 0.35f, 0.18f), 0f, 0.3f);
            handle.transform.localRotation = Quaternion.Euler(0f, 30f, 28f);
            Strip(handle);
            var blade = BuildKit.Cube("ShovelBlade", handle.transform,
                new Vector3(0f, -1.0f, 0f), new Vector3(0.4f, 0.05f, 0.5f), Metal, 0.7f, 0.6f);
            Strip(blade);
        }

        // A tiny stylized excavator silhouette (cab + arm + bucket). KEEP body collider.
        static void BuildMiniDigger(Transform parent, Vector3 local, float yaw)
        {
            var dGo = BuildKit.Root("MiniDigger", parent.position);
            dGo.transform.SetParent(parent, false);
            dGo.transform.localPosition = local;
            dGo.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            var d = dGo.transform;

            var yellow = new Color(0.92f, 0.74f, 0.06f);

            // Tracks (two long dark blocks) — KEEP collider.
            for (int s = -1; s <= 1; s += 2)
            {
                BuildKit.Cube("Track", d, new Vector3(0.5f * s, 0.3f, 0f),
                    new Vector3(0.5f, 0.5f, 2.4f), Dark, 0.2f, 0.3f);
            }
            // Lower body / turntable — KEEP collider.
            BuildKit.Cube("DiggerBody", d, new Vector3(0f, 0.85f, -0.2f),
                new Vector3(1.5f, 0.7f, 1.8f), yellow, 0.3f, 0.5f);
            // Cab — KEEP collider.
            BuildKit.Cube("DiggerCab", d, new Vector3(-0.2f, 1.5f, -0.5f),
                new Vector3(1.0f, 0.9f, 1.0f), yellow, 0.3f, 0.5f);
            var cabWin = BuildKit.Cube("CabWin", d, new Vector3(-0.2f, 1.55f, 0.0f),
                new Vector3(0.85f, 0.6f, 0.06f), Glass, 0.1f, 0.85f);
            Strip(cabWin);

            // Boom arm (decorative).
            var boom = BuildKit.Cube("Boom", d, new Vector3(0.3f, 1.4f, 0.8f),
                new Vector3(0.22f, 0.22f, 1.8f), yellow, 0.3f, 0.5f);
            boom.transform.localRotation = Quaternion.Euler(-35f, 0f, 0f);
            Strip(boom);
            var stick = BuildKit.Cube("Stick", boom.transform,
                new Vector3(0f, -0.4f, 1.0f), new Vector3(0.18f, 1.2f, 0.18f), yellow, 0.3f, 0.5f);
            stick.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);
            Strip(stick);
            var bucket = BuildKit.Cube("Bucket", d, new Vector3(0.3f, 0.2f, 2.0f),
                new Vector3(0.6f, 0.5f, 0.6f), Metal, 0.6f, 0.5f);
            Strip(bucket);
        }

        // ------------------------------------------------------------------ sign

        static void BuildSign(Transform parent)
        {
            var signGo = BuildKit.Root("RoadClosedSign", parent.position);
            signGo.transform.SetParent(parent, false);
            signGo.transform.localPosition = new Vector3(0f, 0f, 8.6f);
            var s = signGo.transform;

            // Two posts — KEEP collider.
            for (int sd = -1; sd <= 1; sd += 2)
            {
                BuildKit.Cylinder("SignPost", s, new Vector3(1.3f * sd, 1.0f, 0f),
                    new Vector3(0.14f, 1.0f, 0.14f), Metal, 0.6f, 0.5f);
            }
            // Orange sign board — KEEP collider (it's a panel you bump).
            var board = BuildKit.Cube("SignBoard", s, new Vector3(0f, 2.2f, 0f),
                new Vector3(3.4f, 1.4f, 0.12f), HiVis, 0f, 0.4f);

            // Black border frame.
            var frame = BuildKit.Cube("SignFrame", board.transform,
                new Vector3(0f, 0f, -0.4f), new Vector3(1.05f, 1.1f, 0.4f), Dark, 0f, 0.3f);
            Strip(frame);

            // The diegetic text.
            BuildKit.Label(board, "ROAD CLOSED\nCONSTRUCTION", Dark, 30,
                new Vector3(0f, 0f, -0.2f));

            // A blinking-style amber lamp on top to draw the eye.
            var lampGo = new GameObject("WarnLamp");
            lampGo.transform.SetParent(s, false);
            lampGo.transform.localPosition = new Vector3(0f, 3.1f, 0f);
            var bulb = BuildKit.Sphere("Bulb", lampGo.transform, Vector3.zero,
                new Vector3(0.32f, 0.32f, 0.32f), new Color(1f, 0.7f, 0.1f), 0.2f, 0.9f);
            Strip(bulb);
            var l = lampGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.6f, 0.1f);
            l.intensity = 2.4f;
            l.range = 9f;
            lampGo.AddComponent<FlickerLight>();
        }

        // --------------------------------------------------------------- workers

        static void BuildWorkers(Transform parent)
        {
            // Local-to-world positions for the workers (idle, hi-vis orange).
            Vector3[] spots =
            {
                new Vector3(-6.0f, 0f, 5.5f),
                new Vector3(2.5f, 0f, 5.0f),
                new Vector3(6.2f, 0f, 4.5f),
            };

            for (int i = 0; i < spots.Length; i++)
            {
                Vector3 world = parent.TransformPoint(spots[i]);
                world.y = 0f;
                var civ = BuildKit.Civ("RoadWorker_" + i, world, Civilian.Mode.Idle, HiVis, Pants);
                if (civ == null) continue;
                civ.mode = Civilian.Mode.Idle;
                civ.shirtColor = HiVis;
                civ.pantsColor = Pants;

                // A hard hat parented to the worker's head.
                if (civ.HeadTransform != null)
                {
                    var hat = BuildKit.Sphere("HardHat", civ.HeadTransform,
                        new Vector3(0f, 0.18f, 0f), new Vector3(0.42f, 0.32f, 0.42f),
                        new Color(0.95f, 0.85f, 0.1f), 0f, 0.55f);
                    Strip(hat);
                    var brim = BuildKit.Cube("HatBrim", civ.HeadTransform,
                        new Vector3(0f, 0.10f, 0.12f), new Vector3(0.46f, 0.04f, 0.2f),
                        new Color(0.95f, 0.85f, 0.1f), 0f, 0.5f);
                    Strip(brim);
                }
            }
        }

        // ---------------------------------------------------------------- utils

        static void Strip(GameObject go)
        {
            if (go == null) return;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
        }
    }
}
