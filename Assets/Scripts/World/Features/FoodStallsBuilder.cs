using UnityEngine;

namespace Spoonacci
{
    // Builds 4 charming, satirical food/drink stalls near the hub but off the roads.
    public static class FoodStallsBuilder
    {
        // Satirical menu names paired with a product-prop palette.
        private static readonly string[] Menus =
        {
            "SMOOTHIES",
            "CAVIAR DOGS",
            "GOLD-LEAF FRIES",
            "SPOON FUEL"
        };

        private static readonly Color[] ProductColors =
        {
            new Color(0.85f, 0.30f, 0.55f), // smoothie pink
            new Color(0.10f, 0.10f, 0.12f), // caviar black
            new Color(0.95f, 0.80f, 0.15f), // gold fries
            new Color(0.55f, 0.75f, 0.95f)  // spoon fuel blue
        };

        // Awning stripe alternates between these per stall.
        private static readonly Color[] AwningA =
        {
            new Color(0.85f, 0.20f, 0.20f),
            new Color(0.20f, 0.45f, 0.80f),
            new Color(0.90f, 0.65f, 0.10f),
            new Color(0.30f, 0.65f, 0.35f)
        };

        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("FoodStalls", Vector3.zero);
            hub.transform.SetParent(root, false);

            var woodTint = new Color(0.78f, 0.62f, 0.42f);

            for (int i = 0; i < Menus.Length; i++)
            {
                Vector3 spot;
                // Constrain near hub center, with generous pad so vendor + props clear roads/footprints.
                if (!WorldLayout.TryOpenSpot(out spot, 4f, -40f, 40f, 60))
                    continue;
                if (WorldLayout.Blocked(new Vector2(spot.x, spot.z), 4f))
                    continue;

                BuildStall(hub.transform, spot, i, woodTint);
            }
        }

        private static void BuildStall(Transform parent, Vector3 basePos, int idx, Color woodTint)
        {
            // Random facing so the row doesn't look gridded; counter front faces +Z in local space.
            float yawDeg = Random.Range(0f, 360f);

            var stall = BuildKit.Root("Stall_" + Menus[idx], basePos);
            stall.transform.SetParent(parent, false);
            stall.transform.rotation = Quaternion.Euler(0f, yawDeg, 0f);

            var t = stall.transform; // children placed in local space relative to this

            // --- Wooden kiosk counter (solid, KEEP collider) ---
            var counter = BuildKit.CubeTex(
                "Counter", t,
                new Vector3(0f, 0.55f, 0f),
                new Vector3(2.6f, 1.1f, 1.1f),
                ProceduralTextures.Wood, woodTint, 0.05f, 0.25f,
                new Vector2(2f, 1f));
            // counter keeps its collider (solid obstacle)

            // Counter top lip (thin plank, decorative — pass-through)
            var lip = BuildKit.CubeTex(
                "CounterTop", t,
                new Vector3(0f, 1.13f, 0.05f),
                new Vector3(2.8f, 0.08f, 1.25f),
                ProceduralTextures.Wood, woodTint * 1.1f, 0.05f, 0.3f,
                new Vector2(2f, 1f));
            Object.Destroy(lip.GetComponent<Collider>());

            // --- Support posts (solid) ---
            float postX = 1.25f, postZ = 0.45f;
            float postY = 1.5f, postH = 3.0f;
            BuildPost(t, new Vector3(-postX, postY, -postZ), postH, woodTint);
            BuildPost(t, new Vector3(postX, postY, -postZ), postH, woodTint);
            BuildPost(t, new Vector3(-postX, postY, postZ), postH, woodTint);
            BuildPost(t, new Vector3(postX, postY, postZ), postH, woodTint);

            // --- Striped awning (thin angled cubes) ---
            BuildAwning(t, idx);

            // --- Menu board Label (satirical literal text) ---
            // A small backing board behind the text for readability.
            var board = BuildKit.Cube(
                "MenuBoard", t,
                new Vector3(0f, 2.55f, -0.55f),
                new Vector3(1.9f, 0.7f, 0.07f),
                new Color(0.12f, 0.12f, 0.14f), 0.1f, 0.2f);
            Object.Destroy(board.GetComponent<Collider>());
            BuildKit.Label(board, Menus[idx], new Color(1f, 0.95f, 0.6f), 34, new Vector3(0f, 0f, -0.06f));

            // --- Product props on the counter (small spheres/cubes) ---
            BuildProducts(t, idx);

            // --- Vendor Civ (Idle) behind the counter ---
            // Compute WORLD position behind counter (local -Z side).
            Vector3 vendorLocal = new Vector3(0f, 0f, -0.95f);
            Vector3 vendorWorld = stall.transform.TransformPoint(vendorLocal);
            vendorWorld.y = 0f;
            var vendor = BuildKit.Civ(
                "Vendor_" + Menus[idx], vendorWorld, Civilian.Mode.Idle,
                AwningA[idx], new Color(0.2f, 0.2f, 0.25f));
            // Face the counter front (toward +Z local => toward customers)
            vendor.transform.rotation = Quaternion.Euler(0f, stall.transform.eulerAngles.y, 0f);

            // --- A couple of customer Civs nearby (in front of counter) ---
            for (int c = 0; c < 2; c++)
            {
                float side = (c == 0) ? -0.7f : 0.7f;
                Vector3 custLocal = new Vector3(side, 0f, 1.6f + Random.Range(-0.2f, 0.4f));
                Vector3 custWorld = stall.transform.TransformPoint(custLocal);
                custWorld.y = 0f;

                // Keep customers off roads/footprints; if blocked, skip this one.
                if (WorldLayout.Blocked(new Vector2(custWorld.x, custWorld.z), 0.6f))
                    continue;

                var cust = BuildKit.Civ(
                    "Customer_" + Menus[idx] + "_" + c, custWorld, Civilian.Mode.Idle,
                    RandomShirt(), RandomPants());
                // Face the counter (opposite of vendor)
                cust.transform.rotation = Quaternion.Euler(0f, stall.transform.eulerAngles.y + 180f, 0f);
            }
        }

        private static void BuildPost(Transform t, Vector3 pos, float height, Color woodTint)
        {
            var post = BuildKit.CubeTex(
                "Post", t, pos,
                new Vector3(0.12f, height, 0.12f),
                ProceduralTextures.Wood, woodTint * 0.85f, 0.05f, 0.25f,
                new Vector2(1f, 3f));
            // solid post: keep collider
        }

        private static void BuildAwning(Transform t, int idx)
        {
            Color a = AwningA[idx];
            Color b = Color.white;
            // 6 stripes spanning the front, tilted forward like a market canopy.
            int stripes = 6;
            float totalW = 3.2f;
            float stripeW = totalW / stripes;
            float startX = -totalW * 0.5f + stripeW * 0.5f;

            var awn = BuildKit.Root("Awning", Vector3.zero);
            awn.transform.SetParent(t, false);
            awn.transform.localPosition = new Vector3(0f, 3.0f, 0.35f);
            awn.transform.localRotation = Quaternion.Euler(-22f, 0f, 0f); // angled forward/down

            for (int s = 0; s < stripes; s++)
            {
                Color col = (s % 2 == 0) ? a : b;
                var strip = BuildKit.Cube(
                    "Stripe_" + s, awn.transform,
                    new Vector3(startX + s * stripeW, 0f, 0f),
                    new Vector3(stripeW * 0.94f, 0.06f, 1.6f),
                    col, 0.0f, 0.15f);
                Object.Destroy(strip.GetComponent<Collider>()); // canopy = pass-through deco
            }

            // Scalloped front valance (small hanging flaps) for charm.
            for (int s = 0; s < stripes; s++)
            {
                Color col = (s % 2 == 0) ? a : b;
                var flap = BuildKit.Cube(
                    "Valance_" + s, awn.transform,
                    new Vector3(startX + s * stripeW, -0.1f, 0.78f),
                    new Vector3(stripeW * 0.9f, 0.22f, 0.05f),
                    col, 0.0f, 0.15f);
                Object.Destroy(flap.GetComponent<Collider>());
            }
        }

        private static void BuildProducts(Transform t, int idx)
        {
            Color prod = ProductColors[idx];
            float topY = 1.22f;

            // Three product props arranged on the counter top.
            switch (idx)
            {
                case 0: // SMOOTHIES — tall cups
                    for (int j = 0; j < 3; j++)
                    {
                        float x = -0.7f + j * 0.7f;
                        var cup = BuildKit.Cylinder(
                            "Cup_" + j, t,
                            new Vector3(x, topY + 0.18f, 0.15f),
                            new Vector3(0.16f, 0.2f, 0.16f),
                            prod, 0.0f, 0.5f);
                        Object.Destroy(cup.GetComponent<Collider>());
                        var lid = BuildKit.Sphere("Dome_" + j, t,
                            new Vector3(x, topY + 0.4f, 0.15f),
                            new Vector3(0.16f, 0.1f, 0.16f),
                            new Color(0.9f, 0.95f, 1f), 0.1f, 0.85f);
                    }
                    break;

                case 1: // CAVIAR DOGS — buns with black dots
                    for (int j = 0; j < 3; j++)
                    {
                        float x = -0.7f + j * 0.7f;
                        var bun = BuildKit.Cube("Bun_" + j, t,
                            new Vector3(x, topY + 0.08f, 0.15f),
                            new Vector3(0.34f, 0.14f, 0.18f),
                            new Color(0.85f, 0.65f, 0.35f), 0.0f, 0.2f);
                        Object.Destroy(bun.GetComponent<Collider>());
                        var caviar = BuildKit.Sphere("Caviar_" + j, t,
                            new Vector3(x, topY + 0.17f, 0.15f),
                            new Vector3(0.22f, 0.08f, 0.12f),
                            prod, 0.2f, 0.7f);
                    }
                    break;

                case 2: // GOLD-LEAF FRIES — gold cones
                    for (int j = 0; j < 3; j++)
                    {
                        float x = -0.7f + j * 0.7f;
                        var holder = BuildKit.Cylinder("FryCone_" + j, t,
                            new Vector3(x, topY + 0.16f, 0.15f),
                            new Vector3(0.13f, 0.18f, 0.13f),
                            new Color(0.6f, 0.1f, 0.1f), 0.0f, 0.3f);
                        Object.Destroy(holder.GetComponent<Collider>());
                        var fries = BuildKit.Cube("Fries_" + j, t,
                            new Vector3(x, topY + 0.36f, 0.15f),
                            new Vector3(0.16f, 0.22f, 0.16f),
                            prod, 0.8f, 0.85f);
                    }
                    break;

                default: // SPOON FUEL — energy bottles
                    for (int j = 0; j < 3; j++)
                    {
                        float x = -0.7f + j * 0.7f;
                        var bottle = BuildKit.Cylinder("Bottle_" + j, t,
                            new Vector3(x, topY + 0.19f, 0.15f),
                            new Vector3(0.12f, 0.22f, 0.12f),
                            prod, 0.1f, 0.9f);
                        Object.Destroy(bottle.GetComponent<Collider>());
                        var cap = BuildKit.Cylinder("Cap_" + j, t,
                            new Vector3(x, topY + 0.42f, 0.15f),
                            new Vector3(0.07f, 0.06f, 0.07f),
                            new Color(0.85f, 0.85f, 0.2f), 0.6f, 0.5f);
                        Object.Destroy(cap.GetComponent<Collider>());
                    }
                    break;
            }
        }

        private static Color RandomShirt()
        {
            return new Color(Random.Range(0.3f, 0.95f), Random.Range(0.3f, 0.95f), Random.Range(0.3f, 0.95f));
        }

        private static Color RandomPants()
        {
            return new Color(Random.Range(0.15f, 0.5f), Random.Range(0.15f, 0.5f), Random.Range(0.2f, 0.55f));
        }
    }
}
