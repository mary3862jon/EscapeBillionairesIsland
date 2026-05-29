using UnityEngine;
namespace Spoonacci
{
    // BEACH LIFE on the open sand between the hub and the palm ring.
    // - ~10 beach umbrellas: a slim wooden pole + a colorful canopy sphere (squashed,
    //   collider removed = pass-through), each paired with a flat towel decal and a
    //   sunbather Civilian (Mode.Sunbathe) lounging beside it.
    // - 1 beach volleyball court: two solid posts + a net made of thin pass-through
    //   cubes, with 2-3 wandering players (small patrolRadius so they stay courtside).
    // - A few sandcastles: stacked tan cubes topped with little cone turrets.
    //
    // EVERY placement is anchored via WorldLayout.TryOpenSpot(out pos, 3f, -78, 78);
    // if no open spot is found that cluster is skipped, so nothing lands on a road,
    // building, pool, fountain, or out of bounds. Clusters are kept tight & tidy.
    public static class BeachGoersBuilder
    {
        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("BeachGoers", Vector3.zero);
            hub.transform.SetParent(root, false);

            BuildUmbrellas(hub.transform, 10);
            BuildVolleyball(hub.transform);
            BuildSandcastles(hub.transform, 4);
        }

        // ---- beach umbrellas + towels + sunbathers ---------------------------
        static void BuildUmbrellas(Transform parent, int target)
        {
            // Bright striped-beach canopy palette.
            Color[] canopy =
            {
                new Color(0.92f, 0.26f, 0.24f), // red
                new Color(0.98f, 0.78f, 0.18f), // sunny yellow
                new Color(0.20f, 0.55f, 0.85f), // sky blue
                new Color(0.30f, 0.72f, 0.45f), // mint green
                new Color(0.95f, 0.55f, 0.20f), // tangerine
                new Color(0.85f, 0.40f, 0.70f), // pink
            };
            Color[] towels =
            {
                new Color(0.95f, 0.95f, 0.92f), // cream
                new Color(0.30f, 0.45f, 0.80f), // navy stripe
                new Color(0.90f, 0.45f, 0.45f), // coral
                new Color(0.45f, 0.80f, 0.75f), // teal
            };
            Color pole = new Color(0.62f, 0.45f, 0.28f); // varnished wood

            int placed = 0, guard = 0;
            while (placed < target && guard < target * 6)
            {
                guard++;
                Vector3 pos;
                if (!WorldLayout.TryOpenSpot(out pos, 3f, -78f, 78f)) continue;
                placed++;

                Color ccol = canopy[Random.Range(0, canopy.Length)];
                var cluster = BuildKit.Root("Umbrella_" + placed, pos);
                cluster.transform.SetParent(parent, false);
                cluster.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                // Pole — slim solid cylinder, slightly tilted for a relaxed look.
                float poleH = Random.Range(2.4f, 2.9f);
                float tilt = Random.Range(-7f, 7f);
                var p = BuildKit.Cylinder("Pole", cluster.transform,
                    new Vector3(0f, poleH * 0.5f, 0f),
                    new Vector3(0.09f, poleH * 0.5f, 0.09f), pole, 0.1f, 0.45f);
                p.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);

                // Canopy — squashed sphere dome (pass-through, collider removed by helper).
                float top = poleH + 0.05f;
                Vector3 canopyLocal = new Vector3(
                    Mathf.Sin(tilt * Mathf.Deg2Rad) * top * 0.5f, top - 0.15f, 0f);
                var dome = BuildKit.Sphere("Canopy", cluster.transform, canopyLocal,
                    new Vector3(3.2f, 1.0f, 3.2f), ccol, 0f, 0.3f, keepCollider: false);
                dome.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
                // Tiny finial knob on top.
                BuildKit.Sphere("Finial", cluster.transform,
                    canopyLocal + new Vector3(0f, 0.45f, 0f),
                    new Vector3(0.18f, 0.18f, 0.18f), ccol * 0.7f, 0.2f, 0.4f, keepCollider: false);

                // Towel — flat decal on the sand beside the pole (collider removed).
                Color towelCol = towels[Random.Range(0, towels.Length)];
                var towel = BuildKit.Cube("Towel", cluster.transform,
                    new Vector3(0.9f, 0.02f, 0f),
                    new Vector3(1.4f, 0.04f, 2.4f), towelCol, 0f, 0.2f);
                var tc = towel.GetComponent<Collider>();
                if (tc != null) Object.Destroy(tc);

                // Sunbather — a bikini woman lounging on the towel under the shade.
                // Build INACTIVE so bikini/hair colours are set before BuildBody() runs.
                Vector3 towelWorld = cluster.transform.TransformPoint(new Vector3(0.9f, 0f, 0f));
                Color biki = BikiniColor();
                var bGo = new GameObject("Sunbather_" + placed);
                bGo.SetActive(false);
                bGo.transform.position = towelWorld;
                var bather = bGo.AddComponent<BikiniWoman>();
                bather.mode = Civilian.Mode.Sunbathe;
                bather.bikiniTop = biki;
                bather.bikiniBottom = biki;
                bather.hairColor = BeachHair();
                bGo.SetActive(true);
                // recline on the towel, facing out from the umbrella
                bather.transform.rotation = Quaternion.Euler(-74f, cluster.transform.eulerAngles.y + 90f, 0f);
            }
        }

        // ---- beach volleyball court ------------------------------------------
        static void BuildVolleyball(Transform parent)
        {
            // Need a roomier open patch for the court — pad 4f, retry a few times.
            Vector3 pos = Vector3.zero;
            if (!WorldLayout.TryOpenSpot(out pos, 4f, -74f, 74f)) return;

            var court = BuildKit.Root("Volleyball", pos);
            court.transform.SetParent(parent, false);
            float yaw = Random.Range(0f, 360f);
            court.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            Color postCol = new Color(0.55f, 0.40f, 0.26f);
            Color netCol = new Color(0.93f, 0.93f, 0.90f);
            float span = 5.0f;     // distance between posts
            float postH = 2.4f;
            float netTop = 2.2f;
            float netBottom = 1.0f;

            // Two solid posts (colliders kept).
            for (int side = -1; side <= 1; side += 2)
            {
                BuildKit.Cylinder("Post", court.transform,
                    new Vector3(span * 0.5f * side, postH * 0.5f, 0f),
                    new Vector3(0.1f, postH * 0.5f, 0.1f), postCol, 0.1f, 0.4f);
            }

            // Net = grid of thin pass-through cubes between the posts.
            int cols = 9;
            int rows = 3;
            float netW = span - 0.4f;
            float netH = netTop - netBottom;
            // Horizontal threads.
            for (int rIdx = 0; rIdx <= rows; rIdx++)
            {
                float y = netBottom + netH * (rIdx / (float)rows);
                MakeNetBar("NetH", court.transform,
                    new Vector3(0f, y, 0f), new Vector3(netW, 0.04f, 0.04f), netCol);
            }
            // Vertical threads.
            for (int cIdx = 0; cIdx <= cols; cIdx++)
            {
                float x = -netW * 0.5f + netW * (cIdx / (float)cols);
                MakeNetBar("NetV", court.transform,
                    new Vector3(x, netBottom + netH * 0.5f, 0f),
                    new Vector3(0.04f, netH, 0.04f), netCol);
            }
            // Tape band along the top edge (slightly brighter).
            MakeNetBar("NetTape", court.transform,
                new Vector3(0f, netTop, 0f), new Vector3(netW, 0.1f, 0.06f), Color.white);

            // 2-3 players wandering close to the court (small radius so they stay near).
            int players = Random.Range(2, 4);
            for (int i = 0; i < players; i++)
            {
                // Spread players to both sides of the net.
                float pside = (i % 2 == 0) ? -1f : 1f;
                Vector3 local = new Vector3(
                    Random.Range(-2.0f, 2.0f), 0f, pside * Random.Range(1.4f, 2.6f));
                Vector3 world = court.transform.TransformPoint(local);
                var pl = BikiniWoman.Spawn("Volleyer_" + i, world, Civilian.Mode.Wander);
                pl.walkSpeed = Random.Range(1.6f, 2.4f);
                pl.patrolCenter = world;
                pl.patrolRadius = 2.0f; // courtside only
            }

            // The ball — a small white sphere resting near center court (pass-through).
            BuildKit.Sphere("VolleyBall", court.transform,
                new Vector3(Random.Range(-1f, 1f), 0.3f, Random.Range(-0.5f, 0.5f)),
                new Vector3(0.45f, 0.45f, 0.45f), Color.white, 0f, 0.5f, keepCollider: false);
        }

        static void MakeNetBar(string name, Transform parent, Vector3 local, Vector3 scale, Color col)
        {
            var bar = BuildKit.Cube(name, parent, local, scale, col, 0f, 0.25f);
            var c = bar.GetComponent<Collider>();
            if (c != null) Object.Destroy(c); // net is pass-through decoration
        }

        // ---- sandcastles -----------------------------------------------------
        static void BuildSandcastles(Transform parent, int target)
        {
            Color sandLight = new Color(0.86f, 0.76f, 0.55f);
            Color sandDark = new Color(0.78f, 0.67f, 0.46f);

            int placed = 0, guard = 0;
            while (placed < target && guard < target * 6)
            {
                guard++;
                Vector3 pos;
                if (!WorldLayout.TryOpenSpot(out pos, 3f, -78f, 78f)) continue;
                placed++;

                var castle = BuildKit.Root("Sandcastle_" + placed, pos);
                castle.transform.SetParent(parent, false);
                castle.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                Color tone = Color.Lerp(sandDark, sandLight, Random.value);

                // Base keep — wide squat cube (solid obstacle, collider kept).
                float baseW = Random.Range(1.0f, 1.4f);
                float baseH = Random.Range(0.5f, 0.75f);
                BuildKit.Cube("Keep", castle.transform,
                    new Vector3(0f, baseH * 0.5f, 0f),
                    new Vector3(baseW, baseH, baseW), tone, 0f, 0.15f);

                // Upper tier — smaller stacked cube.
                float topW = baseW * Random.Range(0.5f, 0.65f);
                float topH = baseH * Random.Range(0.7f, 1.0f);
                float topY = baseH + topH * 0.5f;
                BuildKit.Cube("Tier", castle.transform,
                    new Vector3(0f, topY, 0f),
                    new Vector3(topW, topH, topW), tone * 1.04f, 0f, 0.15f);

                // Corner turrets — little cylinders capped with cone-ish squashed spheres.
                float c = baseW * 0.42f;
                Vector3[] corners =
                {
                    new Vector3( c, 0f,  c), new Vector3(-c, 0f,  c),
                    new Vector3( c, 0f, -c), new Vector3(-c, 0f, -c),
                };
                foreach (var corner in corners)
                {
                    float tH = Random.Range(0.55f, 0.85f);
                    BuildKit.Cylinder("Turret", castle.transform,
                        new Vector3(corner.x, tH * 0.5f, corner.z),
                        new Vector3(0.16f, tH * 0.5f, 0.16f), tone * 0.97f, 0f, 0.15f);
                    // Cone roof (squashed sphere, pass-through).
                    BuildKit.Sphere("TurretCap", castle.transform,
                        new Vector3(corner.x, tH + 0.12f, corner.z),
                        new Vector3(0.26f, 0.3f, 0.26f), tone * 1.08f, 0f, 0.2f, keepCollider: false);
                }

                // Central spire flag.
                var flagPole = BuildKit.Cylinder("FlagPole", castle.transform,
                    new Vector3(0f, topY + topH * 0.5f + 0.25f, 0f),
                    new Vector3(0.03f, 0.25f, 0.03f), new Color(0.5f, 0.36f, 0.22f), 0.1f, 0.4f);
                var fc = flagPole.GetComponent<Collider>();
                if (fc != null) Object.Destroy(fc);
                var flag = BuildKit.Cube("Flag", castle.transform,
                    new Vector3(0.1f, topY + topH * 0.5f + 0.45f, 0f),
                    new Vector3(0.18f, 0.12f, 0.02f),
                    new Color(0.9f, 0.25f, 0.25f), 0f, 0.3f);
                var flc = flag.GetComponent<Collider>();
                if (flc != null) Object.Destroy(flc);

                // A couple of decorative pebble "stones" half-buried at the moat edge.
                int pebbles = Random.Range(2, 5);
                Vector2 center = new Vector2(pos.x, pos.z);
                for (int pb = 0; pb < pebbles; pb++)
                {
                    float ang = Random.Range(0f, Mathf.PI * 2f);
                    float dist = baseW * Random.Range(0.9f, 1.4f);
                    Vector3 pl = new Vector3(Mathf.Cos(ang) * dist, 0.04f, Mathf.Sin(ang) * dist);
                    Vector2 wp = center + new Vector2(pl.x, pl.z);
                    if (WorldLayout.Blocked(wp, 0.2f)) continue;
                    BuildKit.Sphere("Shell", castle.transform, pl,
                        new Vector3(0.12f, 0.06f, 0.12f),
                        new Color(0.95f, 0.92f, 0.85f), 0f, 0.35f, keepCollider: false);
                }
            }
        }

        // ---- small color helpers --------------------------------------------
        static Color RandomShirt()
        {
            Color[] c =
            {
                new Color(0.95f, 0.55f, 0.30f), new Color(0.30f, 0.65f, 0.85f),
                new Color(0.90f, 0.40f, 0.55f), new Color(0.40f, 0.78f, 0.50f),
                new Color(0.98f, 0.82f, 0.30f), new Color(0.85f, 0.85f, 0.90f),
            };
            return c[Random.Range(0, c.Length)];
        }

        static Color RandomTrunks()
        {
            Color[] c =
            {
                new Color(0.15f, 0.35f, 0.65f), new Color(0.70f, 0.20f, 0.25f),
                new Color(0.20f, 0.50f, 0.40f), new Color(0.35f, 0.30f, 0.55f),
                new Color(0.90f, 0.60f, 0.20f),
            };
            return c[Random.Range(0, c.Length)];
        }

        static Color BikiniColor()
        {
            Color[] c =
            {
                new Color(1f, 0.15f, 0.55f),  new Color(0.15f, 0.85f, 1f),
                new Color(0.95f, 0.85f, 0.1f), new Color(0.5f, 1f, 0.3f),
                new Color(0.9f, 0.2f, 0.2f),   new Color(0.75f, 0.3f, 1f),
                new Color(1f, 0.4f, 0.75f),    new Color(0.1f, 0.1f, 0.12f),
            };
            return c[Random.Range(0, c.Length)];
        }

        static Color BeachHair()
        {
            Color[] c =
            {
                new Color(0.12f, 0.08f, 0.05f), new Color(0.35f, 0.22f, 0.10f),
                new Color(0.85f, 0.72f, 0.40f), new Color(0.55f, 0.30f, 0.12f),
                new Color(0.05f, 0.05f, 0.06f), new Color(0.90f, 0.45f, 0.55f),
            };
            return c[Random.Range(0, c.Length)];
        }
    }
}
