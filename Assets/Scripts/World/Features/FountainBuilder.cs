using UnityEngine;
namespace Spoonacci
{
    // Grand ornamental multi-tier fountain at the reserved Fountain footprint (6,0,-6).
    // EXEMPT from the Blocked() scatter rule for its OWN reserved zone.
    public static class FountainBuilder
    {
        public static void Build(Transform root)
        {
            Vector2 anchor = WorldLayout.Fountain;            // (6,-6)
            Vector3 C = new Vector3(anchor.x, 0f, anchor.y);  // world center

            var hub = BuildKit.Root("GrandFountain", C);
            hub.transform.SetParent(root, true);
            Transform h = hub.transform;

            // ---- Marble / stone palette with gold trim ----
            Color marble   = new Color(0.94f, 0.93f, 0.90f);
            Color marbleHi = new Color(0.97f, 0.96f, 0.94f);
            Color stone    = new Color(0.80f, 0.78f, 0.74f);
            Color gold     = new Color(0.92f, 0.76f, 0.30f);
            Color waterTeal= new Color(0.30f, 0.72f, 0.92f);
            Color waterDeep= new Color(0.16f, 0.58f, 0.86f);

            // ================= GROUND APRON (flat decal, no collider) =================
            var apron = BuildKit.CubeTex("FountainApron", h, new Vector3(0f, 0.02f, 0f),
                new Vector3(13f, 0.04f, 13f), ProceduralTextures.Stone, stone, 0.05f, 0.5f, new Vector2(5f, 5f));
            Object.Destroy(apron.GetComponent<Collider>());

            // ================= TIER 0 — wide low basin (stacked wide cylinders, KEEP colliders) =================
            // Outer curb wall
            BuildKit.Cylinder("BasinCurb", h, new Vector3(0f, 0.45f, 0f),
                new Vector3(11f, 0.9f, 11f), marble, 0.1f, 0.55f);
            // Slightly inset rim cap (a touch wider visual lip)
            BuildKit.Cylinder("BasinRim", h, new Vector3(0f, 0.92f, 0f),
                new Vector3(11.4f, 0.18f, 11.4f), marbleHi, 0.1f, 0.65f);
            // Gold inlay band on the curb
            var gband0 = BuildKit.Cylinder("BasinGoldBand", h, new Vector3(0f, 0.62f, 0f),
                new Vector3(11.15f, 0.16f, 11.15f), gold, 0.9f, 0.8f);
            Object.Destroy(gband0.GetComponent<Collider>());
            // Inner stone floor (visual depth, no collider)
            var bfloor = BuildKit.CubeTex("BasinFloor", h, new Vector3(0f, 0.10f, 0f),
                new Vector3(9.6f, 0.08f, 9.6f), ProceduralTextures.Stone, new Color(0.45f, 0.72f, 0.85f), 0f, 0.6f, new Vector2(6f, 6f));
            Object.Destroy(bfloor.GetComponent<Collider>());
            // Tier 0 water disc (Water tex, NO collider)
            WaterDisc(h, new Vector3(0f, 0.55f, 0f), 9.4f, waterTeal);

            // ================= CENTRAL PEDESTAL COLUMN =================
            BuildKit.Cylinder("PedestalBase", h, new Vector3(0f, 1.05f, 0f),
                new Vector3(3.2f, 0.5f, 3.2f), marble, 0.1f, 0.6f);
            BuildKit.Cylinder("Column", h, new Vector3(0f, 2.3f, 0f),
                new Vector3(1.6f, 2.0f, 1.6f), marbleHi, 0.1f, 0.65f);
            // gold collar near top of column (no collider)
            var collar = BuildKit.Cylinder("ColumnCollar", h, new Vector3(0f, 3.25f, 0f),
                new Vector3(1.75f, 0.18f, 1.75f), gold, 0.9f, 0.85f);
            Object.Destroy(collar.GetComponent<Collider>());

            // ================= TIER 1 — first shrinking upper bowl =================
            BuildKit.Cylinder("Tier1Bowl", h, new Vector3(0f, 3.6f, 0f),
                new Vector3(5.6f, 0.6f, 5.6f), marble, 0.1f, 0.6f);
            var t1g = BuildKit.Cylinder("Tier1Gold", h, new Vector3(0f, 3.85f, 0f),
                new Vector3(5.75f, 0.12f, 5.75f), gold, 0.9f, 0.8f);
            Object.Destroy(t1g.GetComponent<Collider>());
            WaterDisc(h, new Vector3(0f, 3.78f, 0f), 4.9f, waterDeep);
            // little stem up to tier 2
            BuildKit.Cylinder("Stem2", h, new Vector3(0f, 4.45f, 0f),
                new Vector3(1.0f, 1.0f, 1.0f), marbleHi, 0.1f, 0.65f);

            // ================= TIER 2 — second shrinking upper bowl =================
            BuildKit.Cylinder("Tier2Bowl", h, new Vector3(0f, 5.05f, 0f),
                new Vector3(3.4f, 0.5f, 3.4f), marble, 0.1f, 0.6f);
            var t2g = BuildKit.Cylinder("Tier2Gold", h, new Vector3(0f, 5.25f, 0f),
                new Vector3(3.55f, 0.1f, 3.55f), gold, 0.9f, 0.8f);
            Object.Destroy(t2g.GetComponent<Collider>());
            WaterDisc(h, new Vector3(0f, 5.2f, 0f), 2.9f, waterTeal);
            // final stem
            BuildKit.Cylinder("Stem3", h, new Vector3(0f, 5.7f, 0f),
                new Vector3(0.6f, 0.8f, 0.6f), marbleHi, 0.1f, 0.65f);

            // ================= TIER 3 — small crowning bowl =================
            BuildKit.Cylinder("Tier3Bowl", h, new Vector3(0f, 6.15f, 0f),
                new Vector3(1.8f, 0.4f, 1.8f), marbleHi, 0.1f, 0.6f);
            WaterDisc(h, new Vector3(0f, 6.28f, 0f), 1.4f, waterDeep);

            // gold finial orb on top
            BuildKit.Sphere("Finial", h, new Vector3(0f, 6.75f, 0f),
                new Vector3(0.5f, 0.5f, 0.5f), gold, 0.95f, 0.9f);

            // ================= SPRAY — small white spheres rising from the top (NO colliders) =================
            var sprayRoot = new GameObject("Spray");
            sprayRoot.transform.SetParent(h, false);
            sprayRoot.transform.localPosition = new Vector3(0f, 7.0f, 0f);
            int dropCount = 26;
            for (int i = 0; i < dropCount; i++)
            {
                float ang = UnityEngine.Random.value * Mathf.PI * 2f;
                float spread = UnityEngine.Random.Range(0.05f, 0.85f);
                float hgt = UnityEngine.Random.Range(0.1f, 2.4f);
                Vector3 p = new Vector3(Mathf.Cos(ang) * spread, hgt, Mathf.Sin(ang) * spread);
                float sz = UnityEngine.Random.Range(0.10f, 0.26f);
                var drop = BuildKit.Sphere("Droplet" + i, sprayRoot.transform, p,
                    new Vector3(sz, sz, sz), new Color(0.95f, 0.98f, 1f), 0.05f, 0.95f);
                // BuildKit.Sphere already strips collider (keepCollider default false)
            }
            // gentle animated bob/scatter on the spray
            sprayRoot.AddComponent<FountainSpray>();

            // ================= RING OF BENCHES / LOW LEDGE SEATS (KEEP colliders) =================
            Color seatStone = new Color(0.82f, 0.80f, 0.76f);
            int seatCount = 6;
            float seatRing = 8.6f;
            for (int i = 0; i < seatCount; i++)
            {
                float a = (i / (float)seatCount) * Mathf.PI * 2f;
                Vector3 sp = new Vector3(Mathf.Cos(a) * seatRing, 0.25f, Mathf.Sin(a) * seatRing);
                var seat = BuildKit.Cube("LedgeSeat" + i, h, sp,
                    new Vector3(2.4f, 0.5f, 0.9f), seatStone, 0.05f, 0.5f);
                // rotate so the long axis is tangent to the ring (faces inward toward fountain)
                seat.transform.localRotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f);
                // gold trim strip along the seat top (no collider)
                var trim = BuildKit.Cube("SeatTrim" + i, seat.transform, new Vector3(0f, 0.27f, 0f),
                    new Vector3(2.42f, 0.06f, 0.92f), gold, 0.85f, 0.8f);
                Object.Destroy(trim.GetComponent<Collider>());
            }

            // ================= ADMIRING CIVS (Mode.Idle), placed in OPEN spots =================
            PlaceAdmirer(C, new Vector3(7.2f, 0f, 7.2f), new Color(0.85f, 0.30f, 0.35f), new Color(0.20f, 0.22f, 0.28f));
            PlaceAdmirer(C, new Vector3(-7.4f, 0f, 6.6f), new Color(0.25f, 0.55f, 0.80f), new Color(0.15f, 0.15f, 0.18f));
            PlaceAdmirer(C, new Vector3(6.8f, 0f, -7.4f), new Color(0.30f, 0.70f, 0.45f), new Color(0.25f, 0.20f, 0.15f));

            // ================= NAMEPLATE =================
            BuildKit.Label(hub, "GRAND FOUNTAIN", new Color(0.95f, 0.85f, 0.45f), 26, new Vector3(0f, 8.0f, 0f));
        }

        // Flat circular water surface (Water texture, collider removed).
        private static void WaterDisc(Transform parent, Vector3 localPos, float diameter, Color tint)
        {
            var disc = BuildKit.Cylinder("WaterDisc", parent, localPos,
                new Vector3(diameter, 0.06f, diameter), waterDummy(tint), 0.15f, 0.92f);
            // re-skin with the Water texture for a rippled look
            var mr = disc.GetComponent<MeshRenderer>();
            if (mr != null)
                mr.sharedMaterial = ShaderCache.MakeTextured(ProceduralTextures.Water, tint, 0.15f, 0.92f, new Vector2(3f, 3f));
            Object.Destroy(disc.GetComponent<Collider>());
        }

        // tiny helper to keep the cylinder constructor happy before re-skin
        private static Color waterDummy(Color c) { return c; }

        // Place an admiring civ; nudge to an open spot if the chosen offset is Blocked (outside the fountain footprint).
        private static void PlaceAdmirer(Vector3 center, Vector3 offset, Color shirt, Color pants)
        {
            Vector3 wp = center + offset;
            Vector2 xz = new Vector2(wp.x, wp.z);
            if (WorldLayout.OnRoad(xz) || WorldLayout.OutOfBounds(xz, 1f))
            {
                if (WorldLayout.TryOpenSpot(out Vector3 alt, 1.5f, -85f, 85f, 40))
                    wp = new Vector3(alt.x, 0f, alt.z);
            }
            var c = BuildKit.Civ("FountainAdmirer", wp, Civilian.Mode.Idle, shirt, pants);
            // face the fountain center
            if (c != null)
            {
                Vector3 look = new Vector3(center.x - wp.x, 0f, center.z - wp.z);
                if (look.sqrMagnitude > 0.001f)
                    c.transform.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);
            }
        }
    }

    // Gently animates the spray droplets so the top of the fountain shimmers.
    public class FountainSpray : MonoBehaviour
    {
        private Transform[] _drops;
        private Vector3[] _base;
        private float[] _phase;
        private float[] _amp;

        void Start()
        {
            int n = transform.childCount;
            _drops = new Transform[n];
            _base = new Vector3[n];
            _phase = new float[n];
            _amp = new float[n];
            for (int i = 0; i < n; i++)
            {
                _drops[i] = transform.GetChild(i);
                _base[i] = _drops[i].localPosition;
                _phase[i] = UnityEngine.Random.value * Mathf.PI * 2f;
                _amp[i] = UnityEngine.Random.Range(0.15f, 0.55f);
            }
        }

        void Update()
        {
            if (_drops == null) return;
            float t = Time.time * 2.2f;
            for (int i = 0; i < _drops.Length; i++)
            {
                if (_drops[i] == null) continue;
                float bob = Mathf.Sin(t + _phase[i]) * _amp[i];
                float swirl = Mathf.Cos(t * 0.6f + _phase[i]) * 0.06f;
                Vector3 b = _base[i];
                _drops[i].localPosition = new Vector3(b.x + swirl, b.y + bob, b.z + swirl);
            }
        }
    }
}
