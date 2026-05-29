using UnityEngine;

namespace Spoonacci
{
    // Road network — DERIVED ENTIRELY FROM WorldLayout so the visible asphalt exactly
    // matches the keep-out corridors that every other builder respects. If WorldLayout
    // moves an anchor, the roads follow automatically.
    //
    // Layout produced:
    //   * 4 main roads (Origin -> Beff / Vault / Zuck / Magnus): ~6m asphalt body,
    //     two flanking light sidewalk strips, dashed yellow center line.
    //   * 2 spurs (Origin -> Pool / Alley): narrow ~3m stone footpath.
    //   * 18x18 hub plaza at Origin (PlazaHalf*2) with 4 zebra crosswalks on its edges.
    //
    // EVERYTHING here is a FLAT DECAL: thin (y-scale ~0.03), sitting at y~0.02-0.03,
    // and with its collider REMOVED so the player walks straight over it.
    public static class PathsBuilder
    {
        // Decal heights, layered so paint always reads on top of asphalt.
        const float YBody  = 0.020f; // asphalt slab
        const float YWalk  = 0.024f; // sidewalk strips
        const float YPlaza = 0.022f; // plaza floor
        const float YPaint = 0.030f; // center dashes, crosswalk bars

        static readonly Color AsphaltCol  = new Color(0.16f, 0.16f, 0.18f);
        static readonly Color SidewalkCol = new Color(0.72f, 0.70f, 0.66f);
        static readonly Color StoneCol    = new Color(0.55f, 0.53f, 0.50f);
        static readonly Color PlazaCol    = new Color(0.78f, 0.76f, 0.72f);
        static readonly Color DashCol     = new Color(0.92f, 0.82f, 0.22f);
        static readonly Color ZebraCol    = new Color(0.95f, 0.95f, 0.95f);

        public static void Build(Transform root)
        {
            var holder = BuildKit.Root("Paths", Vector3.zero);
            holder.transform.SetParent(root, false);
            var T = holder.transform;

            // ---- 4 main roads -------------------------------------------------
            for (int i = 0; i < WorldLayout.MainRoadCount; i++)
            {
                var s = WorldLayout.Roads[i];
                BuildMainRoad(T, s.a, s.b, $"MainRoad_{i}");
            }

            // ---- 2 narrow stone spurs ----------------------------------------
            for (int i = WorldLayout.MainRoadCount; i < WorldLayout.Roads.Length; i++)
            {
                var s = WorldLayout.Roads[i];
                BuildSpur(T, s.a, s.b, $"Spur_{i}");
            }

            // ---- hub plaza + crosswalks --------------------------------------
            BuildPlaza(T);
        }

        // ===================================================================
        //  MAIN ROAD: asphalt body + two sidewalk strips + dashed center line
        // ===================================================================
        static void BuildMainRoad(Transform parent, Vector2 from, Vector2 to, string name)
        {
            const float bodyW = 6.0f;
            const float walkW = 1.7f;

            // asphalt body
            Segment(parent, $"{name}_Body", from, to, bodyW, YBody, AsphaltCol, 0f, 0.18f);

            // flanking sidewalks (offset perpendicular from centerline)
            Vector2 dir  = (to - from).normalized;
            Vector2 perp = new Vector2(-dir.y, dir.x);
            float off = bodyW * 0.5f + walkW * 0.5f;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 a = from + perp * (off * side);
                Vector2 b = to   + perp * (off * side);
                Segment(parent, $"{name}_Walk{side}", a, b, walkW, YWalk, SidewalkCol, 0f, 0.10f);
            }

            // dashed center line — short bars along the run, skipping the plaza area
            float len = Vector2.Distance(from, to);
            const float dashLen = 2.2f, gap = 2.2f, dashW = 0.22f;
            float step = dashLen + gap;
            for (float t = WorldLayout.PlazaHalf + 2f; t + dashLen < len - 2f; t += step)
            {
                Vector2 a = from + dir * t;
                Vector2 b = from + dir * (t + dashLen);
                Segment(parent, $"{name}_Dash", a, b, dashW, YPaint, DashCol, 0f, 0.05f);
            }
        }

        // ===================================================================
        //  SPUR: narrow stone footpath
        // ===================================================================
        static void BuildSpur(Transform parent, Vector2 from, Vector2 to, string name)
        {
            const float w = 3.0f;
            Segment(parent, $"{name}_Path", from, to, w, YBody, StoneCol, 0f, 0.12f);
        }

        // ===================================================================
        //  HUB PLAZA + 4 zebra crosswalks
        // ===================================================================
        static void BuildPlaza(Transform parent)
        {
            Vector2 o = WorldLayout.Origin;
            float side = WorldLayout.PlazaHalf * 2f; // 18

            // plaza floor slab (flat, no collider)
            var floor = BuildKit.Cube("HubPlaza", parent,
                new Vector3(o.x, YPlaza, o.y),
                new Vector3(side, 0.03f, side),
                PlazaCol, 0f, 0.15f);
            StripCollider(floor);

            // 4 zebra crosswalks, one on each plaza edge, just off the floor edge.
            float edge = WorldLayout.PlazaHalf + 1.6f;
            Crosswalk(parent, new Vector3(o.x + edge, 0f, o.y), 90f, "CW_PX"); // east edge (X road)
            Crosswalk(parent, new Vector3(o.x - edge, 0f, o.y), 90f, "CW_NX"); // west edge
            Crosswalk(parent, new Vector3(o.x, 0f, o.y + edge), 0f,  "CW_PZ"); // north edge (Z road)
            Crosswalk(parent, new Vector3(o.x, 0f, o.y - edge), 0f,  "CW_NZ"); // south edge
        }

        // A row of white zebra bars. yaw=0 lays bars across a Z-running corridor;
        // yaw=90 lays them across an X-running corridor.
        static void Crosswalk(Transform parent, Vector3 center, float yaw, string name)
        {
            const int bars = 5;
            const float barLen = 4.6f; // spans the road width
            const float barW   = 0.55f;
            const float pitch  = 0.95f;
            float startT = -((bars - 1) * pitch) * 0.5f;

            Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
            Vector3 stepDir = rot * Vector3.forward; // bars step along this axis

            for (int i = 0; i < bars; i++)
            {
                float t = startT + i * pitch;
                Vector3 pos = center + stepDir * t;
                pos.y = YPaint;
                var bar = BuildKit.Cube($"{name}_{i}", parent, pos,
                    new Vector3(barLen, 0.03f, barW), ZebraCol, 0f, 0.05f);
                bar.transform.localRotation = rot;
                StripCollider(bar);
            }
        }

        // ===================================================================
        //  Core helper: lay a thin slab between two XZ points and orient it.
        //  angle = Atan2(dx, dz)*Rad2Deg rotates the slab's +Z down the run.
        // ===================================================================
        static GameObject Segment(Transform parent, string name, Vector2 from, Vector2 to,
                                  float width, float y, Color col, float metallic, float smoothness)
        {
            Vector2 mid = (from + to) * 0.5f;
            float len = Vector2.Distance(from, to);
            float dx = to.x - from.x;
            float dz = to.y - from.y;
            float angle = Mathf.Atan2(dx, dz) * Mathf.Rad2Deg;

            var slab = BuildKit.Cube(name, parent,
                new Vector3(mid.x, y, mid.y),
                new Vector3(width, 0.03f, len),
                col, metallic, smoothness);
            slab.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            StripCollider(slab);
            return slab;
        }

        static void StripCollider(GameObject go)
        {
            var c = go.GetComponent<Collider>();
            if (c != null) Object.Destroy(c);
        }
    }
}
