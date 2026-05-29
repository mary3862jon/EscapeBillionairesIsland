using UnityEngine;
namespace Spoonacci
{
    // Scatters ~18 natural rock / boulder clusters on open sand.
    // Each cluster = 1-3 squashed grey-brown spheres/cubes, SOLID (colliders kept),
    // placed via WorldLayout.TryOpenSpot so nothing lands on roads/buildings/pools.
    // A few tiny pebble accents ring the bigger rocks (colliders removed = pass-through).
    public static class RocksBuilder
    {
        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("Rocks", Vector3.zero);
            hub.transform.SetParent(root, false);

            // Palette of natural grey-brown stone tones.
            Color[] tones =
            {
                new Color(0.46f, 0.44f, 0.40f), // weathered grey
                new Color(0.40f, 0.36f, 0.31f), // grey-brown
                new Color(0.52f, 0.49f, 0.45f), // pale stone
                new Color(0.34f, 0.31f, 0.27f), // dark basalt
                new Color(0.49f, 0.42f, 0.34f), // sandy brown
            };

            int placed = 0;
            int target = 18;
            int guard = 0;

            while (placed < target && guard < target * 6)
            {
                guard++;
                Vector3 pos;
                // pad 2f keeps boulders clear of roads / footprints / bounds.
                if (!WorldLayout.TryOpenSpot(out pos, 2f))
                    continue;

                placed++;
                Vector2 center = new Vector2(pos.x, pos.z);

                // Cluster identity.
                var cluster = BuildKit.Root("RockCluster_" + placed, pos);
                cluster.transform.SetParent(hub.transform, false);
                cluster.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                int blobs = Random.Range(1, 4); // 1-3 main rock blobs
                Color baseTone = tones[Random.Range(0, tones.Length)];
                float bigRadius = 0f;

                for (int b = 0; b < blobs; b++)
                {
                    // First blob centered, others nudged so they read as a clump.
                    Vector3 off = b == 0
                        ? Vector3.zero
                        : new Vector3(Random.Range(-1.1f, 1.1f), 0f, Random.Range(-1.1f, 1.1f));

                    // Varied size: a couple of big boulders, some mid, some low.
                    float s = Random.Range(0.9f, 2.6f);
                    if (b > 0) s *= Random.Range(0.55f, 0.85f); // satellites smaller than the lead
                    bigRadius = Mathf.Max(bigRadius, s);

                    // Squash vertically so it sits like a boulder, not a ball.
                    float sy = s * Random.Range(0.55f, 0.8f);
                    Vector3 scale = new Vector3(
                        s * Random.Range(0.85f, 1.2f),
                        sy,
                        s * Random.Range(0.85f, 1.2f));

                    // Rest half-buried in sand so the base meets ground naturally.
                    float yc = sy * 0.5f - sy * 0.18f;
                    Vector3 local = new Vector3(off.x, yc, off.z);

                    // Slight tone variation between blobs in the same cluster.
                    Color tone = baseTone * Random.Range(0.85f, 1.12f);
                    tone.a = 1f;

                    GameObject rock;
                    // Mix of rounded boulders (spheres) and blocky chunks (cubes).
                    if (Random.value < 0.7f)
                    {
                        rock = BuildKit.Sphere("Boulder", cluster.transform, local, scale,
                            tone, 0f, 0.12f, keepCollider: true); // SOLID obstacle -> collider kept
                    }
                    else
                    {
                        rock = BuildKit.Cube("Chunk", cluster.transform, local, scale,
                            tone, 0f, 0.1f); // Cube already has BoxCollider (solid)
                    }
                    // Random tilt for irregular, natural look.
                    rock.transform.localRotation = Quaternion.Euler(
                        Random.Range(-12f, 12f),
                        Random.Range(0f, 360f),
                        Random.Range(-12f, 12f));
                }

                // Tiny pebble accents ringing the bigger clusters (pass-through decoration).
                if (bigRadius >= 1.6f)
                {
                    int pebbles = Random.Range(3, 7);
                    for (int p = 0; p < pebbles; p++)
                    {
                        float ang = Random.Range(0f, Mathf.PI * 2f);
                        float dist = bigRadius * Random.Range(0.7f, 1.25f);
                        Vector3 pl = new Vector3(Mathf.Cos(ang) * dist, 0f, Mathf.Sin(ang) * dist);

                        // Verify the pebble's WORLD position is still on open ground.
                        Vector2 wp = center + new Vector2(pl.x, pl.z);
                        if (WorldLayout.Blocked(wp, 0.3f))
                            continue;

                        float ps = Random.Range(0.12f, 0.3f);
                        pl.y = ps * 0.35f;
                        Color pebTone = baseTone * Random.Range(0.8f, 1.15f);
                        pebTone.a = 1f;

                        var peb = BuildKit.Sphere("Pebble", cluster.transform, pl,
                            new Vector3(ps * Random.Range(1f, 1.6f), ps * 0.5f, ps * Random.Range(1f, 1.6f)),
                            pebTone, 0f, 0.18f, keepCollider: false); // pass-through -> no collider
                        var col = peb.GetComponent<Collider>();
                        if (col != null) Object.Destroy(col); // belt-and-braces
                    }
                }
            }
        }
    }
}
