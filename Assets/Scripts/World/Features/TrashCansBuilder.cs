using UnityEngine;
using System.Collections.Generic;
namespace Spoonacci
{
    // ~12 municipal trash cans placed along sidewalks / near roads.
    // Each can = short cylinder body + rim + lid, in varied civic colors, SOLID (collider kept).
    // A couple sport an overflowing pile of trash (tiny cubes, colliders removed).
    public static class TrashCansBuilder
    {
        // Varied municipal palette: forest green (recycling), graphite gray, navy blue, brick red.
        private static readonly Color[] _bodyCols =
        {
            new Color(0.13f, 0.40f, 0.20f), // green
            new Color(0.28f, 0.30f, 0.33f), // graphite
            new Color(0.16f, 0.24f, 0.45f), // navy
            new Color(0.48f, 0.13f, 0.11f), // brick red
        };

        // Litter colors for the overflow piles.
        private static readonly Color[] _litterCols =
        {
            new Color(0.85f, 0.82f, 0.74f), // crumpled paper
            new Color(0.90f, 0.65f, 0.18f), // food wrapper
            new Color(0.20f, 0.55f, 0.85f), // plastic cup
            new Color(0.30f, 0.30f, 0.32f), // dark scrap
            new Color(0.75f, 0.20f, 0.25f), // soda can
        };

        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("TrashCans", Vector3.zero).transform;
            hub.SetParent(root, true);

            int made = 0;
            const int target = 12;

            // ---- Primary: use a subset of sidewalk slots (spaced wide) -------------
            var slots = WorldLayout.SidewalkSlots(18f, 7.2f);
            // shuffle a little so we don't always grab the same first dozen
            for (int i = 0; i < slots.Count; i++)
            {
                int j = Random.Range(i, slots.Count);
                var tmp = slots[i]; slots[i] = slots[j]; slots[j] = tmp;
            }

            // Take roughly every other slot so cans aren't crammed bench-to-can.
            for (int i = 0; i < slots.Count && made < target; i += 2)
            {
                var s = slots[i];
                // Slot pos is already on a sidewalk; double-check it's not on a road/footprint.
                if (WorldLayout.Blocked(new Vector2(s.pos.x, s.pos.z), 0.6f)) continue;
                MakeCan(hub, made, s.pos, s.yaw);
                made++;
            }

            // ---- Fallback: top up with open spots near roads ----------------------
            int guard = 0;
            while (made < target && guard < 120)
            {
                guard++;
                if (!WorldLayout.TryOpenSpot(out Vector3 p, 2.0f, -82f, 82f, 30)) continue;
                // bias toward roadside: only accept if reasonably close to some road segment
                float nearest = float.MaxValue;
                var p2 = new Vector2(p.x, p.z);
                foreach (var seg in WorldLayout.Roads)
                {
                    float d = WorldLayout.DistToSeg(p2, seg.a, seg.b);
                    if (d < nearest) nearest = d;
                }
                if (nearest > 16f) continue; // keep cans roadside, not in the middle of nowhere
                float yaw = Random.Range(0f, 360f);
                MakeCan(hub, made, p, yaw);
                made++;
            }
        }

        // ---------------------------------------------------------------
        private static void MakeCan(Transform hub, int id, Vector3 pos, float yaw)
        {
            Color body = _bodyCols[id % _bodyCols.Length];

            var can = BuildKit.Root($"TrashCan_{id}", new Vector3(pos.x, 0f, pos.z)).transform;
            can.SetParent(hub, true);
            can.localRotation = Quaternion.Euler(0f, yaw, 0f);

            // slight height variety so the row isn't perfectly uniform
            float h = Random.Range(0.42f, 0.50f);
            float rad = Random.Range(0.36f, 0.42f);

            // body — short solid cylinder (KEEP collider)
            BuildKit.Cylinder($"Body_{id}", can,
                new Vector3(0f, h, 0f), new Vector3(rad, h, rad),
                body, 0.45f, 0.4f);

            // a couple of vertical "slat" ribs for that municipal look (pass-through)
            int ribs = 6;
            for (int r = 0; r < ribs; r++)
            {
                float ang = r * (360f / ribs) * Mathf.Deg2Rad;
                float rx = Mathf.Cos(ang) * (rad * 0.96f);
                float rz = Mathf.Sin(ang) * (rad * 0.96f);
                var rib = BuildKit.Cube($"Rib_{id}_{r}", can,
                    new Vector3(rx, h, rz), new Vector3(0.05f, h * 1.7f, 0.05f),
                    body * 0.7f, 0.5f, 0.3f);
                Object.Destroy(rib.GetComponent<Collider>());
            }

            // rim — slightly wider ring just under the lid (KEEP collider, it's structural)
            BuildKit.Cylinder($"Rim_{id}", can,
                new Vector3(0f, h * 2f - 0.02f, 0f), new Vector3(rad + 0.04f, 0.05f, rad + 0.04f),
                body * 0.6f, 0.55f, 0.45f);

            bool overflow = (id % 5 == 1) || (id % 5 == 3); // ~2 of every 5 overflow
            float lidY = h * 2f + 0.06f;

            if (overflow)
            {
                // open-ish lid pushed up by trash + a litter pile spilling over the rim
                var lid = BuildKit.Cylinder($"Lid_{id}", can,
                    new Vector3(0f, lidY + 0.12f, 0f), new Vector3(rad + 0.03f, 0.04f, rad + 0.03f),
                    body * 0.5f, 0.5f, 0.5f);
                lid.transform.localRotation = Quaternion.Euler(Random.Range(8f, 22f), Random.Range(0f, 360f), 0f);
                Object.Destroy(lid.GetComponent<Collider>());

                // overflowing trash: tiny scattered cubes mounded on top (colliders removed)
                int bits = Random.Range(6, 10);
                for (int b = 0; b < bits; b++)
                {
                    float bx = Random.Range(-rad * 0.8f, rad * 0.8f);
                    float bz = Random.Range(-rad * 0.8f, rad * 0.8f);
                    float by = lidY + Random.Range(0.0f, 0.22f);
                    float sc = Random.Range(0.06f, 0.13f);
                    var c = _litterCols[Random.Range(0, _litterCols.Length)];
                    var bit = BuildKit.Cube($"Litter_{id}_{b}", can,
                        new Vector3(bx, by, bz),
                        new Vector3(sc, sc * Random.Range(0.5f, 1.2f), sc),
                        c, 0.1f, 0.3f);
                    bit.transform.localRotation = Quaternion.Euler(
                        Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
                    Object.Destroy(bit.GetComponent<Collider>());
                }
            }
            else
            {
                // closed domed lid (pass-through decoration) + small handle knob
                var lid = BuildKit.Cylinder($"Lid_{id}", can,
                    new Vector3(0f, lidY, 0f), new Vector3(rad + 0.03f, 0.05f, rad + 0.03f),
                    body * 0.55f, 0.5f, 0.5f);
                Object.Destroy(lid.GetComponent<Collider>());

                var dome = BuildKit.Sphere($"LidDome_{id}", can,
                    new Vector3(0f, lidY + 0.04f, 0f), new Vector3(rad * 1.4f, 0.18f, rad * 1.4f),
                    body * 0.55f, 0.4f, 0.45f);

                var knob = BuildKit.Sphere($"LidKnob_{id}", can,
                    new Vector3(0f, lidY + 0.14f, 0f), new Vector3(0.1f, 0.1f, 0.1f),
                    body * 0.45f, 0.5f, 0.5f);

                // tidy cans get a little "trash slot" slit on the front (pass-through accent)
                var slot = BuildKit.Cube($"Slot_{id}", can,
                    new Vector3(0f, h * 1.4f, rad + 0.02f), new Vector3(rad * 0.7f, 0.07f, 0.02f),
                    new Color(0.05f, 0.05f, 0.06f), 0.1f, 0.3f);
                Object.Destroy(slot.GetComponent<Collider>());
            }
        }
    }
}
