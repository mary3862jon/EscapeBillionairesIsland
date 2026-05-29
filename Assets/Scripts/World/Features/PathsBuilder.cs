using UnityEngine;

namespace Spoonacci
{
    // Lays a flat, top-down-readable road + sidewalk + paint network across the
    // island, stitching the origin hub to all four corner mega-zones plus short
    // spurs to the Pool and the Shady Alley. Every piece is a thin FLAT decal
    // (no collider) so the player glides over it.
    public static class PathsBuilder
    {
        // ---- palette --------------------------------------------------------
        static readonly Color Asphalt   = new Color(0.16f, 0.16f, 0.18f); // dark road body
        static readonly Color Sidewalk  = new Color(0.74f, 0.72f, 0.66f); // light flanking walk
        static readonly Color DashYellow= new Color(0.95f, 0.82f, 0.18f); // center dashes
        static readonly Color StripeWhite= new Color(0.93f, 0.93f, 0.90f);// crosswalk / spur stones

        const float ROAD_Y   = 0.02f;  // road body
        const float WALK_Y   = 0.024f; // sidewalks slightly above so edges read
        const float PAINT_Y  = 0.03f;  // paint sits on top of the road

        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("PathsNetwork", Vector3.zero);
            hub.transform.SetParent(root, false);
            var T = hub.transform;

            // Key anchors (XZ).
            Vector2 origin = new Vector2(0f, 0f);
            Vector2 beff   = new Vector2(-80f, -80f);
            Vector2 vault  = new Vector2( 80f, -80f);
            Vector2 zuck   = new Vector2(-80f,  80f);
            Vector2 magnus = new Vector2( 80f,  80f);
            Vector2 pool   = new Vector2(-15f,  -2f);
            Vector2 alley  = new Vector2(-22f,  22f);

            // ---- four main paved arteries from the hub to each corner -------
            MainRoad(T, origin, beff);
            MainRoad(T, origin, vault);
            MainRoad(T, origin, zuck);
            MainRoad(T, origin, magnus);

            // ---- short spurs to interior zones (lighter stone path) ---------
            Spur(T, origin, pool);
            Spur(T, origin, alley);

            // ---- hub plaza: a paved square + crosswalks ----------------------
            HubPlaza(T, origin);
        }

        // =====================================================================
        // A full main road = asphalt body + two flanking sidewalks + center dashes.
        // =====================================================================
        static void MainRoad(Transform parent, Vector2 from, Vector2 to)
        {
            const float roadW = 6f;
            const float walkW = 1.6f;

            Segment(parent, from, to, roadW, ProceduralTextures.Stone, Asphalt, 0f, 0.18f, ROAD_Y, "Road");

            // perpendicular offset for the two sidewalks
            Vector2 dir = (to - from).normalized;
            Vector2 perp = new Vector2(-dir.y, dir.x);
            float off = roadW * 0.5f + walkW * 0.5f;

            Segment(parent, from + perp * off, to + perp * off, walkW,
                ProceduralTextures.Stone, Sidewalk, 0f, 0.25f, WALK_Y, "Sidewalk");
            Segment(parent, from - perp * off, to - perp * off, walkW,
                ProceduralTextures.Stone, Sidewalk, 0f, 0.25f, WALK_Y, "Sidewalk");

            CenterDashes(parent, from, to);
        }

        // Painted center-line dashes marching down the middle of the road.
        static void CenterDashes(Transform parent, Vector2 from, Vector2 to)
        {
            Vector2 d = to - from;
            float len = d.magnitude;
            Vector2 dir = d / Mathf.Max(len, 0.001f);
            float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;

            const float dash = 2.2f;
            const float gap  = 2.6f;
            float step = dash + gap;
            int n = Mathf.FloorToInt(len / step);

            for (int i = 0; i < n; i++)
            {
                float t = (i + 0.5f) * step;
                Vector2 p = from + dir * t;
                var slab = BuildKit.Cube("Dash", parent,
                    new Vector3(p.x, PAINT_Y, p.y),
                    new Vector3(0.45f, 0.04f, dash),
                    DashYellow, 0f, 0.1f);
                slab.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
                Strip(slab);
            }
        }

        // Lighter stone footpath spur to an interior destination, with a faint edge.
        static void Spur(Transform parent, Vector2 from, Vector2 to)
        {
            Segment(parent, from, to, 3f,
                ProceduralTextures.Stone, new Color(0.62f, 0.6f, 0.55f), 0f, 0.22f, ROAD_Y, "SpurPath");

            // little stepping accents so the spur reads differently from main roads
            Vector2 d = to - from;
            float len = d.magnitude;
            Vector2 dir = d / Mathf.Max(len, 0.001f);
            float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            int n = Mathf.FloorToInt(len / 3.2f);
            for (int i = 0; i < n; i++)
            {
                Vector2 p = from + dir * ((i + 0.5f) * 3.2f);
                var stone = BuildKit.Cube("Step", parent,
                    new Vector3(p.x, PAINT_Y, p.y),
                    new Vector3(1.4f, 0.04f, 0.9f),
                    StripeWhite, 0f, 0.15f);
                stone.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
                Strip(stone);
            }
        }

        // Paved square at the origin hub + four crosswalks fanning out.
        static void HubPlaza(Transform parent, Vector2 c)
        {
            var plaza = BuildKit.CubeTex("HubPlaza", parent,
                new Vector3(c.x, ROAD_Y, c.y),
                new Vector3(16f, 0.04f, 16f),
                ProceduralTextures.Stone, new Color(0.3f, 0.3f, 0.32f), 0f, 0.2f, new Vector2(4f, 4f));
            Strip(plaza);

            // crosswalks on the +X, -X, +Z, -Z edges of the plaza
            Crosswalk(parent, new Vector2(c.x + 9f, c.y), 90f);
            Crosswalk(parent, new Vector2(c.x - 9f, c.y), 90f);
            Crosswalk(parent, new Vector2(c.x, c.y + 9f), 0f);
            Crosswalk(parent, new Vector2(c.x, c.y - 9f), 0f);
        }

        // A zebra crossing: a row of white stripes centered at p, oriented by yaw.
        static void Crosswalk(Transform parent, Vector2 p, float yaw)
        {
            Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
            const int stripes = 6;
            const float spacing = 0.85f;
            float start = -(stripes - 1) * spacing * 0.5f;
            for (int i = 0; i < stripes; i++)
            {
                // stripe offset runs along local X, then rotated into world
                Vector3 localOff = new Vector3(start + i * spacing, 0f, 0f);
                Vector3 worldOff = rot * localOff;
                var s = BuildKit.Cube("Stripe", parent,
                    new Vector3(p.x + worldOff.x, PAINT_Y, p.y + worldOff.z),
                    new Vector3(0.45f, 0.04f, 4.5f),
                    StripeWhite, 0f, 0.1f);
                s.transform.localRotation = rot;
                Strip(s);
            }
        }

        // =====================================================================
        // Core helper: place a rotated flat textured slab spanning two XZ points.
        // =====================================================================
        static GameObject Segment(Transform parent, Vector2 from, Vector2 to, float width,
            Texture2D tex, Color tint, float metallic, float smoothness, float y, string name)
        {
            Vector2 mid = (from + to) * 0.5f;
            Vector2 d = to - from;
            float len = d.magnitude;
            if (len < 0.01f) len = width; // degenerate guard

            float angle = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg; // align local Z to direction

            float tileLen = Mathf.Max(1f, len / 4f);
            var go = BuildKit.CubeTex(name, parent,
                new Vector3(mid.x, y, mid.y),
                new Vector3(width, 0.04f, len),
                tex, tint, metallic, smoothness, new Vector2(width / 4f, tileLen));
            go.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            Strip(go);
            return go;
        }

        // Make a piece a true flat decal: drop its collider so the player can't trip.
        static void Strip(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
        }
    }
}
