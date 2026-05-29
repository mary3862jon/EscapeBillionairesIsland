using UnityEngine;
using System.Collections.Generic;

namespace Spoonacci
{
    // Paints a flat bike lane decal from WorldLayout.BikeLaneA to WorldLayout.BikeLaneB:
    // a tinted strip, white edge lines, periodic painted bicycle glyphs, crossing
    // hatch marks where it overlaps main roads, and "BIKE LANE" signage at each end.
    // ALL flat painted pieces have their colliders removed. This builder is EXEMPT
    // from the scatter-Blocked rule for its OWN lane corridor (BikeLane zone), and is
    // allowed to cross the main roads (with crossing markings).
    public static class BikeLaneBuilder
    {
        const float LaneWidth = 2.4f;
        const float DecalY = 0.02f;
        const float DecalThk = 0.04f;

        public static void Build(Transform root)
        {
            Vector2 a2 = WorldLayout.BikeLaneA;
            Vector2 b2 = WorldLayout.BikeLaneB;

            Vector3 a = new Vector3(a2.x, 0f, a2.y);
            Vector3 b = new Vector3(b2.x, 0f, b2.y);

            Vector3 dir = (b - a);
            float length = dir.magnitude;
            dir.Normalize();
            Vector3 side = Vector3.Cross(Vector3.up, dir).normalized; // perpendicular, in XZ
            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            Vector3 mid = (a + b) * 0.5f;

            GameObject zone = BuildKit.Root("BikeLane", mid);
            zone.transform.SetParent(root, true);
            Transform z = zone.transform;

            // ---- Materials ----
            Color laneTint = new Color(0.20f, 0.55f, 0.32f, 1f);   // green bike-lane green
            Material laneMat = BuildKit.Mat(laneTint, 0f, 0.25f);
            Material whiteMat = BuildKit.Mat(new Color(0.95f, 0.95f, 0.92f), 0f, 0.2f);
            Material crossMat = BuildKit.Mat(new Color(0.96f, 0.85f, 0.30f), 0f, 0.2f); // crossing hatch
            Material postMat = BuildKit.Mat(new Color(0.18f, 0.18f, 0.20f), 0.5f, 0.4f);
            Material signMat = BuildKit.Mat(new Color(0.15f, 0.45f, 0.85f), 0.1f, 0.5f);

            // ---- Main lane strip (built as a few overlapping flat segments so it
            // hugs the ground; rotated to lane yaw) ----
            BuildFlat(z, "LaneStrip", mid, new Vector3(LaneWidth, DecalThk, length), laneMat, yaw);

            // ---- White edge lines along both sides ----
            float edgeOff = LaneWidth * 0.5f - 0.12f;
            BuildFlat(z, "EdgeL", mid + side * edgeOff, new Vector3(0.16f, DecalThk * 1.2f, length), whiteMat, yaw);
            BuildFlat(z, "EdgeR", mid - side * edgeOff, new Vector3(0.16f, DecalThk * 1.2f, length), whiteMat, yaw);

            // ---- Periodic painted bicycle glyphs + crossing hatch where on a road ----
            int glyphCount = Mathf.Max(2, Mathf.RoundToInt(length / 14f));
            for (int i = 0; i <= glyphCount; i++)
            {
                float t = (float)i / glyphCount;
                Vector3 p = Vector3.Lerp(a, b, t);
                Vector2 p2 = new Vector2(p.x, p.z);

                if (WorldLayout.OnRoad(p2, 0.5f))
                {
                    // crossing zebra-style hatch across the lane at this point
                    BuildCrossingHatch(z, p, dir, side, crossMat, yaw);
                }
                else
                {
                    // painted bicycle symbol
                    BuildBicycleGlyph(z, p, dir, side, whiteMat, yaw);
                }
            }

            // ---- "BIKE LANE" signage on a low post at each end ----
            BuildEndSign(z, a, dir, postMat, signMat);
            BuildEndSign(z, b, -dir, postMat, signMat);

            // ---- A short run of painted ground text near the middle (slabs) ----
            BuildGroundWord(z, mid - dir * (length * 0.18f), dir, side, whiteMat, yaw);
        }

        // A flat decal cube with collider removed, rotated about Y by yaw.
        static GameObject BuildFlat(Transform parent, string name, Vector3 worldPos, Vector3 scale, Material mat, float yaw)
        {
            // Build at origin under a temp, then place. BuildKit.Cube uses LOCAL pos when parented.
            GameObject go = BuildKit.Cube(name, parent, Vector3.zero, scale, mat.color, 0f, mat.GetFloat("_Smoothness"));
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
            go.transform.position = new Vector3(worldPos.x, DecalY + scale.y * 0.5f, worldPos.z);
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            var c = go.GetComponent<Collider>();
            if (c != null) Object.Destroy(c);
            return go;
        }

        // Simple white bicycle: two ring-ish wheels (flat squares) + a frame bar + a chevron.
        static void BuildBicycleGlyph(Transform parent, Vector3 center, Vector3 dir, Vector3 side, Material mat, float yaw)
        {
            float wheel = 0.55f;
            float gap = 0.95f;
            // two wheels (drawn as flat thin rings approximated by small squares)
            DrawRing(parent, center - dir * gap * 0.5f, wheel, mat, yaw);
            DrawRing(parent, center + dir * gap * 0.5f, wheel, mat, yaw);
            // frame bar between wheels
            BuildFlat(parent, "bikeFrame", center, new Vector3(0.14f, DecalThk * 1.3f, gap), mat, yaw);
            // direction chevron ahead of the bike
            Vector3 chev = center + dir * 1.4f;
            BuildFlat(parent, "bikeChevA", chev - side * 0.25f, new Vector3(0.12f, DecalThk * 1.3f, 0.7f), mat, yaw + 30f);
            BuildFlat(parent, "bikeChevB", chev + side * 0.25f, new Vector3(0.12f, DecalThk * 1.3f, 0.7f), mat, yaw - 30f);
        }

        // Approximate a wheel ring with 8 short flat arcs (thin squares).
        static void DrawRing(Transform parent, Vector3 center, float radius, Material mat, float yaw)
        {
            int seg = 8;
            for (int i = 0; i < seg; i++)
            {
                float ang = (i / (float)seg) * Mathf.PI * 2f;
                Vector3 off = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * radius;
                GameObject g = BuildFlat(parent, "wheelSeg", center + off, new Vector3(0.12f, DecalThk * 1.2f, 0.42f), mat,
                    yaw + (ang * Mathf.Rad2Deg) + 90f);
            }
        }

        // Yellow hatch marks across the lane where it crosses a road.
        static void BuildCrossingHatch(Transform parent, Vector3 center, Vector3 dir, Vector3 side, Material mat, float yaw)
        {
            int bars = 5;
            float span = LaneWidth + 0.6f;
            for (int i = 0; i < bars; i++)
            {
                float o = (i / (float)(bars - 1) - 0.5f) * span;
                BuildFlat(parent, "crossBar", center + side * o, new Vector3(0.22f, DecalThk * 1.3f, 1.6f), mat, yaw);
            }
        }

        // Low post + a BuildKit.Label that reads "BIKE LANE", facing back along the lane.
        static void BuildEndSign(Transform parent, Vector3 end, Vector3 inwardDir, Material postMat, Material signMat)
        {
            // step the post just off the painted strip so it does not sit on the lane center
            Vector3 basePos = end - inwardDir * 1.0f;
            Vector2 b2 = new Vector2(basePos.x, basePos.z);
            // keep the post inside the world; nudge inward if it would leave bounds
            if (WorldLayout.OutOfBounds(b2, 1f))
                basePos = end + inwardDir * 1.0f;

            GameObject post = BuildKit.Cylinder("BikeSignPost", parent,
                new Vector3(basePos.x, 1.1f, basePos.z), new Vector3(0.12f, 1.1f, 0.12f), postMat.color, 0.5f, 0.4f);
            // post keeps its collider (solid obstacle)

            GameObject plate = BuildKit.Cube("BikeSignPlate", parent,
                new Vector3(basePos.x, 2.3f, basePos.z), new Vector3(1.6f, 0.9f, 0.08f), signMat.color, 0.1f, 0.5f);
            float faceYaw = Mathf.Atan2(inwardDir.x, inwardDir.z) * Mathf.Rad2Deg;
            plate.transform.rotation = Quaternion.Euler(0f, faceYaw, 0f);

            BuildKit.Label(plate, "BIKE LANE", Color.white, 28, new Vector3(0f, 0f, 0.06f));
        }

        // Spell a short painted "BIKE" word on the ground using simple slabs (flat decals).
        static void BuildGroundWord(Transform parent, Vector3 center, Vector3 dir, Vector3 side, Material mat, float yaw)
        {
            // Four blocky letterform clusters laid along the lane reading toward travel.
            // Each "letter" is a small set of bars; kept simple/legible from above.
            for (int letter = 0; letter < 4; letter++)
            {
                Vector3 lp = center + dir * (letter - 1.5f) * 1.3f;
                // vertical stem
                BuildFlat(parent, "wordStem", lp - side * 0.3f, new Vector3(0.16f, DecalThk * 1.2f, 1.1f), mat, yaw + 90f);
                // two cross bars to suggest letters
                BuildFlat(parent, "wordBarTop", lp + dir * 0.45f, new Vector3(0.6f, DecalThk * 1.2f, 0.16f), mat, yaw + 90f);
                BuildFlat(parent, "wordBarBot", lp - dir * 0.45f, new Vector3(0.6f, DecalThk * 1.2f, 0.16f), mat, yaw + 90f);
            }
        }
    }
}
