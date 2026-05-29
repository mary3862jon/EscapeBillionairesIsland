using UnityEngine;

namespace Spoonacci
{
    public static class FlowersBuilder
    {
        // Bright tropical petal palette.
        private static readonly Color[] PetalColors = new Color[]
        {
            new Color(1.00f, 0.30f, 0.62f), // hot pink
            new Color(1.00f, 0.85f, 0.15f), // sunny yellow
            new Color(1.00f, 0.55f, 0.10f), // orange
            new Color(0.65f, 0.25f, 0.85f), // purple
            new Color(0.95f, 0.18f, 0.18f), // red
            new Color(1.00f, 0.45f, 0.75f), // light pink
            new Color(0.80f, 0.35f, 0.95f), // violet
        };

        private static readonly Color StemColor = new Color(0.18f, 0.55f, 0.20f); // leafy green
        private static readonly Color CenterColor = new Color(1.00f, 0.92f, 0.55f); // pollen center

        public static void Build(Transform root)
        {
            // Container for all flower clusters.
            GameObject containerGO = new GameObject("FlowersRoot");
            containerGO.transform.SetParent(root, false);
            Transform container = containerGO.transform;

            const int clusterCount = 30;

            for (int i = 0; i < clusterCount; i++)
            {
                Vector3 spot;
                // Find an open patch of sand that is not on a road, in a footprint, or out of bounds.
                if (!WorldLayout.TryOpenSpot(out spot, 1.2f))
                {
                    // No room found for this attempt; keep trying the remaining clusters.
                    continue;
                }

                BuildCluster(container, spot, i);
            }
        }

        private static void BuildCluster(Transform parent, Vector3 spot, int index)
        {
            // Cluster pivot anchored at the open spot (ground level y=0).
            GameObject clusterGO = new GameObject("FlowerCluster_" + index);
            clusterGO.transform.SetParent(parent, false);
            clusterGO.transform.position = new Vector3(spot.x, 0f, spot.z);
            Transform cluster = clusterGO.transform;

            int flowers = Random.Range(4, 8); // 4-7 flowers

            for (int f = 0; f < flowers; f++)
            {
                // Spread flowers within a tight radius so the cluster reads as one bunch.
                float ang = Random.Range(0f, Mathf.PI * 2f);
                float rad = Random.Range(0f, 0.6f);
                Vector3 local = new Vector3(Mathf.Cos(ang) * rad, 0f, Mathf.Sin(ang) * rad);

                BuildFlower(cluster, local, f);
            }
        }

        private static void BuildFlower(Transform cluster, Vector3 baseLocal, int idx)
        {
            // Varied stem heights for a natural look.
            float stemHeight = Random.Range(0.45f, 0.85f);
            float stemThick = Random.Range(0.04f, 0.07f);

            // Thin green stem. Cylinder default height is ~2 units, so scale.y is half the height.
            Vector3 stemScale = new Vector3(stemThick, stemHeight * 0.5f, stemThick);
            Vector3 stemPos = new Vector3(baseLocal.x, stemHeight * 0.5f, baseLocal.z);

            GameObject stem = BuildKit.Cylinder(
                "Stem_" + idx,
                cluster,
                stemPos,
                stemScale,
                StemColor,
                0f,
                0.25f);
            // Pass-through decoration — remove collider.
            Collider stemCol = stem.GetComponent<Collider>();
            if (stemCol != null) Object.Destroy(stemCol);

            // Bright petal sphere on top of the stem.
            Color petal = PetalColors[Random.Range(0, PetalColors.Length)];
            float petalSize = Random.Range(0.18f, 0.30f);
            Vector3 petalPos = new Vector3(baseLocal.x, stemHeight + petalSize * 0.4f, baseLocal.z);

            // Sphere collider already removed by BuildKit unless keepCollider=true.
            BuildKit.Sphere(
                "Petal_" + idx,
                cluster,
                petalPos,
                new Vector3(petalSize, petalSize, petalSize),
                petal,
                0f,
                0.55f);

            // Small bright pollen center for extra pop.
            float centerSize = petalSize * 0.45f;
            Vector3 centerPos = new Vector3(baseLocal.x, stemHeight + petalSize * 0.4f, baseLocal.z);

            BuildKit.Sphere(
                "Center_" + idx,
                cluster,
                centerPos,
                new Vector3(centerSize, centerSize, centerSize),
                CenterColor,
                0f,
                0.6f);
        }
    }
}
