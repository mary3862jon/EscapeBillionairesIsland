using UnityEngine;

namespace Spoonacci
{
    // Adds wandering TOURIST groups to bring the island to life.
    // ~5 small clusters of 3-4 tourists each, in bright holiday-shirt colors,
    // milling about open spots. A couple of them clutch a chest-held "camera"
    // and one in each cluster sports a flat sunhat. Lively and varied.
    public static class TouristsBuilder
    {
        // Bright, garish holiday shirt palette — loud tropical tones.
        static readonly Color[] HolidayShirts =
        {
            new Color(0.98f, 0.36f, 0.18f), // sunset orange
            new Color(0.18f, 0.78f, 0.92f), // pool-water cyan
            new Color(0.96f, 0.82f, 0.16f), // beach yellow
            new Color(0.95f, 0.32f, 0.62f), // flamingo pink
            new Color(0.36f, 0.84f, 0.40f), // palm green
            new Color(0.62f, 0.40f, 0.92f), // tropical violet
            new Color(0.99f, 0.52f, 0.24f), // mango
            new Color(0.20f, 0.62f, 0.96f), // sky blue
        };

        // Casual shorts/pants colors.
        static readonly Color[] HolidayPants =
        {
            new Color(0.90f, 0.88f, 0.82f), // khaki / beige
            new Color(0.16f, 0.20f, 0.28f), // navy
            new Color(0.78f, 0.30f, 0.28f), // brick red shorts
            new Color(0.30f, 0.36f, 0.44f), // slate
            new Color(0.85f, 0.85f, 0.88f), // white linen
        };

        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("Tourists", new Vector3(0f, 0f, 0f));
            if (root != null) hub.transform.SetParent(root, true);

            const int clusterCount = 5;
            int globalIdx = 0;

            for (int c = 0; c < clusterCount; c++)
            {
                // Find an open, road-free, footprint-free spot for the cluster center.
                if (!WorldLayout.TryOpenSpot(out Vector3 center, 3f, -60f, 60f))
                    continue; // no room — skip this cluster

                var clusterGo = new GameObject("TouristGroup_" + c);
                clusterGo.transform.SetParent(hub.transform, false);
                clusterGo.transform.position = center;

                int members = 3 + Random.Range(0, 2); // 3 or 4 tourists
                int hatMember = Random.Range(0, members); // exactly one gets a sunhat

                for (int m = 0; m < members; m++)
                {
                    // Spread members around the cluster center; retry a few times to
                    // avoid spawning directly on a road/building (they may wander later).
                    Vector3 spawn = center;
                    for (int attempt = 0; attempt < 8; attempt++)
                    {
                        Vector2 off = Random.insideUnitCircle * 4.5f;
                        Vector3 cand = center + new Vector3(off.x, 0f, off.y);
                        if (!WorldLayout.Blocked(new Vector2(cand.x, cand.z), 1f))
                        {
                            spawn = cand;
                            break;
                        }
                        spawn = cand; // last candidate kept if all blocked (they'll wander off)
                    }

                    var shirt = HolidayShirts[(globalIdx + Random.Range(0, HolidayShirts.Length)) % HolidayShirts.Length];
                    var pants = HolidayPants[Random.Range(0, HolidayPants.Length)];
                    var skin = RandomSkin();

                    var civ = BuildKit.Civ("Tourist_" + c + "_" + m, spawn, Civilian.Mode.Wander, shirt, pants);
                    if (civ != null)
                    {
                        civ.mode = Civilian.Mode.Wander;
                        civ.patrolCenter = center;            // roam around the shared cluster center
                        civ.patrolRadius = 8f;
                        civ.walkSpeed = 1.1f + Random.value * 0.8f; // relaxed sightseeing pace
                        civ.shirtColor = shirt;
                        civ.pantsColor = pants;
                        civ.skinColor = skin;

                        // Hold a camera at the chest for a couple of tourists per cluster.
                        if (m % 2 == 0)
                            AttachCamera(civ.transform);

                        // One tourist in each cluster wears a flat sunhat.
                        if (m == hatMember && civ.HeadTransform != null)
                            AttachSunhat(civ.HeadTransform, shirt);
                    }

                    globalIdx++;
                }
            }
        }

        // A small black camera held at chest height, parented so it travels with the tourist.
        static void AttachCamera(Transform tourist)
        {
            // Tourist torso sits around y=1.3, facing +Z (forward). Hold the camera
            // out front at chest level.
            var cam = BuildKit.Cube("TouristCamera", tourist,
                new Vector3(0f, 1.25f, 0.34f),
                new Vector3(0.22f, 0.16f, 0.14f),
                new Color(0.06f, 0.06f, 0.07f), 0.3f, 0.45f);
            StripCollider(cam);

            // A protruding lens barrel on the front of the camera.
            var lens = BuildKit.Cylinder("CameraLens", cam.transform,
                new Vector3(0f, 0f, 0.6f),
                new Vector3(0.7f, 0.4f, 0.7f),
                new Color(0.10f, 0.10f, 0.14f), 0.5f, 0.6f);
            lens.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // barrel points forward (+Z)
            StripCollider(lens);

            // Glassy lens glint on the very front.
            var glass = BuildKit.Sphere("CameraGlass", cam.transform,
                new Vector3(0f, 0f, 0.92f),
                new Vector3(0.5f, 0.5f, 0.2f),
                new Color(0.35f, 0.55f, 0.75f), 0.2f, 0.92f);
            StripCollider(glass);

            // Neck strap hint: a thin dark band up to the neck.
            var strap = BuildKit.Cube("CameraStrap", cam.transform,
                new Vector3(0f, 1.6f, -0.6f),
                new Vector3(0.55f, 3.4f, 0.18f),
                new Color(0.12f, 0.12f, 0.14f), 0f, 0.3f);
            strap.transform.localRotation = Quaternion.Euler(28f, 0f, 0f);
            StripCollider(strap);
        }

        // A flat wide-brim sunhat sitting on top of the head.
        static void AttachSunhat(Transform head, Color accent)
        {
            // Head sphere is scaled ~0.34 and centered near localY 1.85 on the body;
            // its own local space here is the head's, so place the hat just above it.
            var strawTan = new Color(0.86f, 0.74f, 0.46f);

            // Wide flat brim.
            var brim = BuildKit.Cylinder("SunhatBrim", head,
                new Vector3(0f, 0.7f, 0f),
                new Vector3(2.6f, 0.08f, 2.6f),
                strawTan, 0f, 0.2f);
            StripCollider(brim);

            // Rounded crown on top of the brim.
            var crown = BuildKit.Cylinder("SunhatCrown", head,
                new Vector3(0f, 0.95f, 0f),
                new Vector3(1.5f, 0.45f, 1.5f),
                strawTan, 0f, 0.2f);
            StripCollider(crown);

            // Colorful ribbon band around the base of the crown, tinted to the shirt.
            var band = BuildKit.Cylinder("SunhatBand", head,
                new Vector3(0f, 0.78f, 0f),
                new Vector3(1.56f, 0.18f, 1.56f),
                accent, 0f, 0.35f);
            StripCollider(band);
        }

        // Varied believable skin tones.
        static Color RandomSkin()
        {
            Color[] tones =
            {
                new Color(0.98f, 0.84f, 0.72f),
                new Color(0.92f, 0.74f, 0.60f),
                new Color(0.78f, 0.58f, 0.42f),
                new Color(0.60f, 0.42f, 0.30f),
                new Color(0.44f, 0.30f, 0.22f),
            };
            return tones[Random.Range(0, tones.Length)];
        }

        static void StripCollider(GameObject go)
        {
            if (go == null) return;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
        }
    }
}
