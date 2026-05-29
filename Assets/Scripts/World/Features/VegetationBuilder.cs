using UnityEngine;
using System.Collections.Generic;

namespace Spoonacci
{
    // Lush decorative greenery scattered across the open sand of the island.
    // Palms, bushes, hedges, flowers, rocks and grass tufts — placed with
    // natural jitter so the beach feels alive without blocking the player.
    public static class VegetationBuilder
    {
        // Footprints to stay clear of (world x,z). Radius applied per-center below.
        private static readonly Vector2[] ExclusionCenters = new Vector2[]
        {
            new Vector2(12f, 8f),   // Salon
            new Vector2(20f, 6f),   // Skin Kiosk
            new Vector2(22f, -10f), // Tiki Bar
            new Vector2(0f, 14f),   // Deck chairs
            new Vector2(-15f, -2f), // Central Pool
            new Vector2(-22f, 22f), // Shady Alley
            new Vector2(-80f, -80f),// Beff Yacht
            new Vector2(80f, -80f), // Crypto Vault
            new Vector2(-80f, 80f), // Zuck Lab
            new Vector2(80f, 80f),  // Magnus Mansion
        };

        private const float ExclusionRadius = 7f;
        private const float SpawnClearRadius = 5f;   // around origin where player spawns
        private const float PlayMin = -90f;
        private const float PlayMax = 90f;

        // Cached green palette for variety.
        private static readonly Color LeafGreen = new Color(0.18f, 0.46f, 0.16f);
        private static readonly Color LeafGreenDark = new Color(0.12f, 0.34f, 0.12f);
        private static readonly Color LeafGreenLight = new Color(0.30f, 0.58f, 0.22f);
        private static readonly Color RockGrey = new Color(0.46f, 0.46f, 0.48f);
        private static readonly Color RockGreyDark = new Color(0.34f, 0.34f, 0.37f);

        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("Vegetation", Vector3.zero);
            hub.transform.SetParent(root, false);
            var parent = hub.transform;

            // --- Palms (~20) ---
            for (int i = 0; i < 20; i++)
            {
                Vector3 p = FindSpot(14f); // palms want a little extra spacing
                if (p.x == float.MaxValue) continue;
                BuildPalm(parent, p, Random.Range(0.85f, 1.35f));
            }

            // --- Bushes / shrubs (~40) ---
            for (int i = 0; i < 40; i++)
            {
                Vector3 p = FindSpot(0f);
                if (p.x == float.MaxValue) continue;
                BuildBush(parent, p);
            }

            // --- Hedge rows near the hub (solid, keep colliders) ---
            BuildHedgeRow(parent, new Vector3(6f, 0f, -2f), Vector3.right, 5, 0f);
            BuildHedgeRow(parent, new Vector3(-4f, 0f, 18f), Vector3.right, 4, 0f);
            BuildHedgeRow(parent, new Vector3(28f, 0f, 2f), Vector3.forward, 4, 0f);
            BuildHedgeRow(parent, new Vector3(-8f, 0f, 6f), Vector3.forward, 3, 90f);

            // --- Flower clusters (~25) ---
            for (int i = 0; i < 25; i++)
            {
                Vector3 p = FindSpot(0f);
                if (p.x == float.MaxValue) continue;
                BuildFlowerCluster(parent, p);
            }

            // --- Rocks (~15, solid) ---
            for (int i = 0; i < 15; i++)
            {
                Vector3 p = FindSpot(0f);
                if (p.x == float.MaxValue) continue;
                BuildRock(parent, p);
            }

            // --- Tropical grass tufts (~18) ---
            for (int i = 0; i < 18; i++)
            {
                Vector3 p = FindSpot(0f);
                if (p.x == float.MaxValue) continue;
                BuildGrassTuft(parent, p);
            }
        }

        // ---------- placement ----------

        // Returns a valid world position, or Vector3 with x==float.MaxValue if none found.
        private static Vector3 FindSpot(float extraClearance)
        {
            for (int attempt = 0; attempt < 30; attempt++)
            {
                float x = Random.Range(PlayMin, PlayMax);
                float z = Random.Range(PlayMin, PlayMax);
                var v = new Vector2(x, z);

                if (v.magnitude < SpawnClearRadius + extraClearance) continue;

                bool blocked = false;
                for (int i = 0; i < ExclusionCenters.Length; i++)
                {
                    if (Vector2.Distance(v, ExclusionCenters[i]) < ExclusionRadius + extraClearance)
                    {
                        blocked = true;
                        break;
                    }
                }
                if (blocked) continue;

                return new Vector3(x, 0f, z);
            }
            return new Vector3(float.MaxValue, 0f, 0f);
        }

        // ---------- palm ----------

        private static void BuildPalm(Transform parent, Vector3 basePos, float scale)
        {
            var palm = BuildKit.Root("Palm", basePos);
            palm.transform.SetParent(parent, false);
            var t = palm.transform;

            float trunkRadius = 0.32f * scale;
            int segments = Random.Range(2, 4); // 2-3 stacked tilted cylinders
            float segLen = 2.6f * scale;

            // Lean direction for the whole curve.
            float leanDir = Random.Range(0f, 360f);
            Vector3 lean = new Vector3(Mathf.Cos(leanDir * Mathf.Deg2Rad), 0f, Mathf.Sin(leanDir * Mathf.Deg2Rad));

            Vector3 cursor = Vector3.zero;     // local, bottom of next segment
            float curTilt = Random.Range(2f, 6f);
            Vector3 topCenter = Vector3.zero;
            float topRadius = trunkRadius;

            for (int s = 0; s < segments; s++)
            {
                // increasing tilt toward the top gives a gentle curve
                curTilt += Random.Range(3f, 7f);
                float tiltRad = curTilt * Mathf.Deg2Rad;
                Vector3 dir = (Vector3.up * Mathf.Cos(tiltRad) + lean * Mathf.Sin(tiltRad)).normalized;

                float r = Mathf.Lerp(trunkRadius, trunkRadius * 0.7f, (float)s / segments);
                Vector3 segCenter = cursor + dir * (segLen * 0.5f);

                var seg = BuildKit.Cylinder("Trunk" + s, t, segCenter,
                    new Vector3(r, segLen * 0.5f, r), new Color(0.55f, 0.40f, 0.24f),
                    0f, 0.18f);
                // texture the trunk
                Retexture(seg, ProceduralTextures.Wood, new Color(0.62f, 0.46f, 0.30f), 0f, 0.2f, new Vector2(1f, 2f));
                // orient cylinder along dir
                seg.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir);

                cursor = cursor + dir * segLen;
                topCenter = cursor;
                topRadius = r;
            }

            // Crown base lump
            var crown = BuildKit.Sphere("Crown", t, topCenter,
                Vector3.one * (topRadius * 2.4f), LeafGreenDark, 0f, 0.25f, false);
            Retexture(crown, ProceduralTextures.Leaves, LeafGreenDark, 0f, 0.25f, Vector2.one);

            // 5-6 drooping fronds
            int fronds = Random.Range(5, 7);
            for (int f = 0; f < fronds; f++)
            {
                float ang = (360f / fronds) * f + Random.Range(-12f, 12f);
                float aRad = ang * Mathf.Deg2Rad;
                Vector3 outDir = new Vector3(Mathf.Cos(aRad), -0.32f, Mathf.Sin(aRad)).normalized;
                float frondLen = Random.Range(2.6f, 3.6f) * scale;

                // mid + tip spheres make a drooping leaf shape
                Vector3 mid = topCenter + outDir * (frondLen * 0.5f) + Vector3.up * 0.3f;
                Vector3 tip = topCenter + outDir * frondLen + Vector3.down * (frondLen * 0.18f);

                Color leaf = (f % 2 == 0) ? LeafGreen : LeafGreenLight;

                var fMid = BuildKit.Sphere("FrondMid" + f, t, mid,
                    new Vector3(1.5f, 0.42f, 1.5f) * scale, leaf, 0f, 0.3f, false);
                fMid.transform.localRotation = Quaternion.LookRotation(outDir, Vector3.up);
                Retexture(fMid, ProceduralTextures.Leaves, leaf, 0f, 0.3f, Vector2.one);

                var fTip = BuildKit.Sphere("FrondTip" + f, t, tip,
                    new Vector3(0.9f, 0.3f, 0.9f) * scale, leaf, 0f, 0.3f, false);
                Retexture(fTip, ProceduralTextures.Leaves, leaf, 0f, 0.3f, Vector2.one);
            }

            // a few coconuts
            int nuts = Random.Range(2, 5);
            for (int n = 0; n < nuts; n++)
            {
                float a = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector3 np = topCenter + new Vector3(Mathf.Cos(a), -0.2f, Mathf.Sin(a)) * (topRadius * 1.6f);
                BuildKit.Sphere("Coconut" + n, t, np, Vector3.one * 0.45f * scale,
                    new Color(0.32f, 0.22f, 0.12f), 0f, 0.25f, false);
            }

            // random yaw so palms don't all lean the same visual way
            t.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }

        // ---------- bush ----------

        private static void BuildBush(Transform parent, Vector3 basePos)
        {
            var bush = BuildKit.Root("Bush", basePos);
            bush.transform.SetParent(parent, false);
            var t = bush.transform;

            int blobs = Random.Range(3, 6);
            float spread = Random.Range(0.6f, 1.1f);
            for (int i = 0; i < blobs; i++)
            {
                float a = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float rad = Random.Range(0f, spread);
                Vector3 lp = new Vector3(Mathf.Cos(a) * rad, Random.Range(0.4f, 0.9f), Mathf.Sin(a) * rad);
                float sz = Random.Range(0.8f, 1.5f);
                Color c = Color.Lerp(LeafGreenDark, LeafGreenLight, Random.value);
                var s = BuildKit.Sphere("Blob" + i, t, lp, Vector3.one * sz, c, 0f, 0.28f, false);
                Retexture(s, ProceduralTextures.Leaves, c, 0f, 0.28f, Vector2.one);
                // bushes: walk-through, collider already removed by Sphere default
            }
        }

        // ---------- hedge ----------

        private static void BuildHedgeRow(Transform parent, Vector3 start, Vector3 dir, int count, float extraYaw)
        {
            dir = dir.normalized;
            var hedgeRoot = BuildKit.Root("Hedge", start);
            hedgeRoot.transform.SetParent(parent, false);
            var t = hedgeRoot.transform;

            float seg = 2.0f;
            for (int i = 0; i < count; i++)
            {
                Vector3 lp = dir * (i * seg) + Vector3.up * 0.7f;
                var box = BuildKit.CubeTex("HedgeSeg" + i, t, lp,
                    new Vector3(2.0f, 1.4f, 1.1f), ProceduralTextures.Leaves,
                    Color.Lerp(LeafGreen, LeafGreenDark, 0.4f), 0f, 0.25f, new Vector2(2f, 1f));
                box.transform.localRotation = Quaternion.LookRotation(dir, Vector3.up);
                // KEEP collider — hedges are solid.
            }
            t.localRotation = Quaternion.Euler(0f, extraYaw, 0f);
        }

        // ---------- flowers ----------

        private static readonly Color[] FlowerColors = new Color[]
        {
            new Color(0.95f, 0.25f, 0.35f), // red
            new Color(0.98f, 0.78f, 0.20f), // yellow
            new Color(0.85f, 0.40f, 0.85f), // magenta
            new Color(0.95f, 0.55f, 0.20f), // orange
            new Color(0.55f, 0.55f, 0.95f), // periwinkle
            new Color(0.98f, 0.98f, 0.98f), // white
        };

        private static void BuildFlowerCluster(Transform parent, Vector3 basePos)
        {
            var cl = BuildKit.Root("Flowers", basePos);
            cl.transform.SetParent(parent, false);
            var t = cl.transform;

            int flowers = Random.Range(4, 8);
            for (int i = 0; i < flowers; i++)
            {
                float a = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float rad = Random.Range(0f, 0.8f);
                Vector3 foot = new Vector3(Mathf.Cos(a) * rad, 0f, Mathf.Sin(a) * rad);
                float h = Random.Range(0.5f, 0.95f);

                // thin green stem
                var stem = BuildKit.Cylinder("Stem" + i, t, foot + Vector3.up * (h * 0.5f),
                    new Vector3(0.04f, h * 0.5f, 0.04f), LeafGreen, 0f, 0.2f);
                if (stem.GetComponent<Collider>() != null) Object.Destroy(stem.GetComponent<Collider>());

                // bright bloom on top
                Color fc = FlowerColors[Random.Range(0, FlowerColors.Length)];
                BuildKit.Sphere("Bloom" + i, t, foot + Vector3.up * (h + 0.05f),
                    Vector3.one * Random.Range(0.18f, 0.30f), fc, 0f, 0.5f, false);
            }
        }

        // ---------- rocks ----------

        private static void BuildRock(Transform parent, Vector3 basePos)
        {
            bool cube = Random.value < 0.45f;
            float sz = Random.Range(0.7f, 2.4f);
            Color c = Color.Lerp(RockGreyDark, RockGrey, Random.value);

            if (cube)
            {
                var r = BuildKit.Cube("Rock", parent, basePos + Vector3.up * (sz * 0.4f),
                    new Vector3(sz, sz * Random.Range(0.6f, 1.0f), sz * Random.Range(0.8f, 1.2f)),
                    c, 0f, 0.15f);
                Retexture(r, ProceduralTextures.Stone, c, 0f, 0.15f, Vector2.one);
                r.transform.localRotation = Quaternion.Euler(Random.Range(-8f, 8f), Random.Range(0f, 360f), Random.Range(-8f, 8f));
                // KEEP collider
            }
            else
            {
                var r = BuildKit.Sphere("Rock", parent, basePos + Vector3.up * (sz * 0.35f),
                    new Vector3(sz, sz * Random.Range(0.55f, 0.85f), sz * Random.Range(0.8f, 1.1f)),
                    c, 0f, 0.15f, true); // keep collider
                Retexture(r, ProceduralTextures.Stone, c, 0f, 0.15f, Vector2.one);
                r.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }
        }

        // ---------- grass tuft ----------

        private static void BuildGrassTuft(Transform parent, Vector3 basePos)
        {
            var tuft = BuildKit.Root("GrassTuft", basePos);
            tuft.transform.SetParent(parent, false);
            var t = tuft.transform;

            int blades = Random.Range(5, 9);
            for (int i = 0; i < blades; i++)
            {
                float h = Random.Range(0.5f, 1.1f);
                float yaw = Random.Range(0f, 360f);
                float tilt = Random.Range(8f, 28f);
                Color c = Color.Lerp(LeafGreen, LeafGreenLight, Random.value);

                var blade = BuildKit.Cube("Blade" + i, t, Vector3.up * (h * 0.5f),
                    new Vector3(0.06f, h, 0.16f), c, 0f, 0.25f);
                blade.transform.localRotation = Quaternion.Euler(tilt * Mathf.Cos(yaw * Mathf.Deg2Rad),
                    yaw, tilt * Mathf.Sin(yaw * Mathf.Deg2Rad));
                if (blade.GetComponent<Collider>() != null) Object.Destroy(blade.GetComponent<Collider>());
            }
        }

        // ---------- util ----------

        // Apply a textured Lit material to an existing renderer (overrides BuildKit's flat color).
        private static void Retexture(GameObject go, Texture2D tex, Color tint, float metallic, float smoothness, Vector2 tile)
        {
            if (go == null) return;
            var rend = go.GetComponent<Renderer>();
            if (rend == null) return;
            rend.sharedMaterial = ShaderCache.MakeTextured(tex, tint, metallic, smoothness, tile);
        }
    }
}
