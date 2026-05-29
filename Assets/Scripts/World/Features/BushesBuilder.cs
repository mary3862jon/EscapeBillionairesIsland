using UnityEngine;

namespace Spoonacci
{
    // Scatters ~50 leafy bush clusters across open sand. Each cluster is a
    // huddle of 3-6 overlapping green spheres with varied size and shade so
    // they read as soft, organic shrubs. Colliders are removed so the player
    // (and civilians) can stroll straight through the foliage. Placement is
    // delegated entirely to WorldLayout.TryOpenSpot so nothing ever lands on a
    // road, building footprint, pool, or out of bounds.
    public static class BushesBuilder
    {
        private const int ClusterCount = 50;

        // A small spread of leaf greens — from deep shade to sun-bleached tips.
        private static readonly Color[] Greens = new Color[]
        {
            new Color(0.11f, 0.30f, 0.11f), // deep shadow green
            new Color(0.15f, 0.38f, 0.14f), // forest green
            new Color(0.20f, 0.46f, 0.17f), // mid leaf green
            new Color(0.27f, 0.54f, 0.20f), // fresh green
            new Color(0.34f, 0.60f, 0.24f), // sunlit green
            new Color(0.40f, 0.65f, 0.30f), // pale highlight green
            new Color(0.24f, 0.50f, 0.26f), // slightly bluish green
        };

        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("Bushes", Vector3.zero);
            hub.transform.SetParent(root, false);
            var parent = hub.transform;

            for (int i = 0; i < ClusterCount; i++)
            {
                // WorldLayout is the spatial authority: only build where it
                // hands us a genuinely open patch of sand.
                if (!WorldLayout.TryOpenSpot(out Vector3 pos, 1.5f)) continue;
                BuildBushCluster(parent, pos, i);
            }
        }

        // ---------- one bush cluster ----------

        private static void BuildBushCluster(Transform parent, Vector3 basePos, int index)
        {
            var bush = BuildKit.Root("BushCluster" + index, basePos);
            bush.transform.SetParent(parent, false);
            var t = bush.transform;

            // Overall bushiness varies per cluster.
            int blobs = Random.Range(3, 7);            // 3-6 inclusive
            float spread = Random.Range(0.55f, 1.15f); // horizontal huddle radius
            float baseScale = Random.Range(0.85f, 1.35f);

            // Pick a dominant hue for this bush, then jitter each blob around it
            // so a single shrub still has internal light/shadow variation.
            int hueIdx = Random.Range(0, Greens.Length);
            Color hue = Greens[hueIdx];

            for (int i = 0; i < blobs; i++)
            {
                // Position blobs in a low, overlapping huddle.
                float a = Random.Range(0f, Mathf.PI * 2f);
                float rad = Random.Range(0f, spread);
                float h = Random.Range(0.35f, 0.95f) * baseScale;
                Vector3 lp = new Vector3(Mathf.Cos(a) * rad, h, Mathf.Sin(a) * rad);

                float sz = Random.Range(0.75f, 1.55f) * baseScale;

                // Blend the cluster hue toward a neighbour shade for variety.
                Color neighbour = Greens[(hueIdx + Random.Range(-1, 2) + Greens.Length) % Greens.Length];
                Color c = Color.Lerp(hue, neighbour, Random.value * 0.6f);

                // keepCollider defaults false -> Sphere already strips the
                // collider, so the player walks straight through the foliage.
                var s = BuildKit.Sphere("Blob" + i, t, lp,
                    new Vector3(sz, sz * Random.Range(0.85f, 1.1f), sz),
                    c, 0f, 0.26f, false);

                // Leafy texture overrides the flat fill for a foliage look.
                var rend = s.GetComponent<Renderer>();
                if (rend != null)
                    rend.sharedMaterial = ShaderCache.MakeTextured(
                        ProceduralTextures.Leaves, c, 0f, 0.26f, Vector2.one);

                // Safety net: ensure nothing solid was left on this pass-through prop.
                var col = s.GetComponent<Collider>();
                if (col != null) Object.Destroy(col);
            }

            // Gentle random yaw so identical huddles don't read as clones.
            t.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }
    }
}
