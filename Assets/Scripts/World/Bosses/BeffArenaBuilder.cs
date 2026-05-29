using UnityEngine;

namespace Spoonacci
{
    // ─────────────────────────────────────────────────────────────────────────
    //  BeffArenaBuilder — an opulent moored MEGA-YACHT deck arena.
    //
    //  Build(center) drops a long multi-deck yacht around the given world point:
    //      • a long brushed-metal HULL with a white superstructure stack
    //      • a sun-deck floor (white composite) the player fights on, ringed by
    //        a polished chrome RAILING with vertical balusters
    //      • a glowing INFINITY POOL set into the aft deck
    //      • a towering CHAMPAGNE-GLASS PYRAMID centrepiece (stacked emissive
    //        glass cones + a coupe rim per tier) — left at deck-centre for the
    //        boss to topple
    //      • deck LOUNGERS with parasols
    //      • a radar / comms MAST with a sweeping emissive dish
    //      • gold trim, mooring bollards, a gangway
    //
    //  Everything is parented under one "Beff Jezos Mega-Yacht" root. Decorative
    //  parts have colliders stripped; the hull sides + superstructure keep theirs
    //  so the player is contained on the deck.
    //
    //  The champagne pyramid is built here (static, decorative tiers). The BOSS
    //  spawns its OWN topple-able tiers separately so its TickGimmick can cascade
    //  them — this centrepiece is the permanent showpiece behind that.
    // ─────────────────────────────────────────────────────────────────────────
    public static class BeffArenaBuilder
    {
        // palette
        static readonly Color HullWhite = new Color(0.93f, 0.94f, 0.96f);
        static readonly Color HullSteel = new Color(0.74f, 0.77f, 0.82f);
        static readonly Color Gold      = new Color(0.85f, 0.68f, 0.24f);
        static readonly Color Chrome    = new Color(0.80f, 0.83f, 0.88f);
        static readonly Color Teak      = new Color(0.62f, 0.45f, 0.26f);
        static readonly Color GlassTint = new Color(0.85f, 0.90f, 0.78f);

        public static void Build(Vector3 center)
        {
            var root = BuildKit.Root("Beff Jezos Mega-Yacht", center).transform;

            BuildHull(root);
            BuildDeck(root);
            BuildSuperstructure(root);
            BuildRailing(root);
            BuildInfinityPool(root);
            BuildChampagnePyramid(root, new Vector3(0f, 0.65f, -2.5f));
            BuildLoungers(root);
            BuildRadarMast(root);
            BuildBollards(root);
            BuildGangway(root);
        }

        // ── long multi-deck hull (brushed-metal w/ a white boot stripe) ──────
        static void BuildHull(Transform root)
        {
            // the water it floats on (large dark emissive-ish disc)
            var water = BuildKit.Cylinder("YachtWater", root, new Vector3(0f, -0.6f, 0f),
                new Vector3(58f, 0.3f, 58f), Color.white, 0.1f, 0.85f);
            water.GetComponent<Renderer>().sharedMaterial =
                ShaderCache.MakeTextured(ProceduralTextures.Water, new Color(0.10f, 0.26f, 0.34f),
                    0.1f, 0.9f, new Vector2(7f, 7f));
            NoCol(water);

            // main hull body — long brushed-metal box, walls kept solid (containment)
            var hull = BuildKit.CubeTex("Hull", root, new Vector3(0f, -0.5f, 0f),
                new Vector3(16f, 2.2f, 40f), BossTextures.BrushedMetal,
                HullSteel, 0.8f, 0.55f, new Vector2(6f, 2f));
            // hull keeps its collider — the player can't walk off into the sea

            // dark waterline / boot stripe
            var boot = BuildKit.Cube("BootStripe", root, new Vector3(0f, -1.35f, 0f),
                new Vector3(16.2f, 0.5f, 40.2f), new Color(0.06f, 0.07f, 0.09f), 0.5f, 0.5f);
            NoCol(boot);

            // gold sheer line along the deck edge
            var sheer = BuildKit.Cube("GoldSheer", root, new Vector3(0f, 0.62f, 0f),
                new Vector3(16.4f, 0.14f, 40.4f), Gold, 0.9f, 0.85f);
            Emit(sheer, Gold * 0.35f);
            NoCol(sheer);

            // tapered bow wedge (pointed front, +Z)
            var bow = BuildKit.Cube("Bow", root, new Vector3(0f, -0.5f, 21.5f),
                new Vector3(16f, 2.2f, 5f), HullSteel, 0.8f, 0.55f);
            bow.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            bow.transform.localScale = new Vector3(11.3f, 2.2f, 11.3f);
            bow.GetComponent<Renderer>().sharedMaterial =
                ShaderCache.MakeTextured(BossTextures.BrushedMetal, HullSteel, 0.8f, 0.55f, new Vector2(3f, 3f));
        }

        // ── white composite sun-deck floor (the fight surface) ───────────────
        static void BuildDeck(Transform root)
        {
            var deck = BuildKit.Cube("SunDeck", root, new Vector3(0f, 0.55f, -1f),
                new Vector3(15.4f, 0.2f, 37f), HullWhite, 0.15f, 0.75f);
            // teak inlay strips running fore-aft for a real-yacht look
            for (int i = -3; i <= 3; i++)
            {
                var plank = BuildKit.CubeTex("TeakPlank" + i, root,
                    new Vector3(i * 1.9f, 0.66f, -1f), new Vector3(0.5f, 0.04f, 36f),
                    ProceduralTextures.Wood, Teak, 0.05f, 0.4f, new Vector2(1f, 14f));
                NoCol(plank);
            }
        }

        // ── tiered white superstructure (bridge stack) at the stern (-Z) ─────
        static void BuildSuperstructure(Transform root)
        {
            // tier 1 — wide saloon block
            var t1 = BuildKit.Cube("Saloon", root, new Vector3(0f, 2.0f, -14.5f),
                new Vector3(13f, 2.6f, 8f), HullWhite, 0.2f, 0.6f);
            GoldBand(root, new Vector3(0f, 0.85f, -14.5f), new Vector3(13.2f, 0.12f, 8.2f));
            WindowStrip(root, new Vector3(0f, 2.2f, -10.45f), new Vector3(11f, 1.1f, 0.1f));

            // tier 2 — owner's deck
            var t2 = BuildKit.Cube("OwnerDeck", root, new Vector3(0f, 4.6f, -15.5f),
                new Vector3(10f, 2.2f, 6f), HullWhite, 0.2f, 0.6f);
            WindowStrip(root, new Vector3(0f, 4.7f, -12.55f), new Vector3(8.5f, 0.9f, 0.1f));

            // tier 3 — bridge / sky-lounge with a curved blue-glass visor
            var t3 = BuildKit.Cube("Bridge", root, new Vector3(0f, 6.7f, -16.2f),
                new Vector3(7.5f, 1.9f, 4.5f), HullWhite, 0.2f, 0.6f);
            var visor = BuildKit.Cube("BridgeGlass", root, new Vector3(0f, 6.8f, -14.05f),
                new Vector3(6.6f, 1.3f, 0.12f), new Color(0.12f, 0.45f, 0.62f), 0.3f, 0.95f);
            visor.transform.localRotation = Quaternion.Euler(-14f, 0f, 0f);
            Emit(visor, new Color(0.2f, 0.6f, 0.85f) * 0.6f);
            NoCol(visor);

            // funnel / exhaust stack with gold ring
            var funnel = BuildKit.Cylinder("Funnel", root, new Vector3(0f, 8.4f, -17.5f),
                new Vector3(2.4f, 1.3f, 1.6f), new Color(0.10f, 0.11f, 0.14f), 0.7f, 0.5f);
            funnel.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
            NoCol(funnel);
            var fRing = BuildKit.Cylinder("FunnelRing", root, new Vector3(0f, 8.9f, -17.7f),
                new Vector3(2.5f, 0.12f, 1.7f), Gold, 0.95f, 0.85f);
            Emit(fRing, Gold * 0.5f);
            NoCol(fRing);
        }

        static void GoldBand(Transform root, Vector3 pos, Vector3 scale)
        {
            var b = BuildKit.Cube("GoldBand", root, pos, scale, Gold, 0.95f, 0.85f);
            Emit(b, Gold * 0.35f);
            NoCol(b);
        }

        static void WindowStrip(Transform root, Vector3 pos, Vector3 scale)
        {
            var w = BuildKit.Cube("WindowStrip", root, pos, scale,
                new Color(0.06f, 0.10f, 0.14f), 0.4f, 0.95f);
            Emit(w, new Color(0.15f, 0.35f, 0.5f) * 0.4f);
            NoCol(w);
        }

        // ── polished chrome railing with balusters around the deck edge ──────
        static void BuildRailing(Transform root)
        {
            float halfX = 7.5f, zFront = 17f, zBack = -12.5f;
            var railMat = ShaderCache.MakeMat(Chrome, 0.9f, 0.85f);

            // top rails (4 sides) — front/back run along X, port/starboard along Z
            float zMid = (zFront + zBack) * 0.5f;
            float zLen = zFront - zBack;
            TopRail(root, new Vector3(0f, 1.55f, zFront), new Vector3(halfX * 2f, 0.08f, 0.08f), railMat);
            TopRail(root, new Vector3(-halfX, 1.55f, zMid), new Vector3(0.08f, 0.08f, zLen), railMat);
            TopRail(root, new Vector3( halfX, 1.55f, zMid), new Vector3(0.08f, 0.08f, zLen), railMat);

            // balusters along port & starboard
            int n = 18;
            for (int i = 0; i <= n; i++)
            {
                float z = Mathf.Lerp(zBack, zFront, i / (float)n);
                Baluster(root, new Vector3(-halfX, 1.05f, z), railMat);
                Baluster(root, new Vector3( halfX, 1.05f, z), railMat);
            }
            // balusters across the bow
            int m = 8;
            for (int i = 0; i <= m; i++)
            {
                float x = Mathf.Lerp(-halfX, halfX, i / (float)m);
                Baluster(root, new Vector3(x, 1.05f, zFront), railMat);
            }
        }

        static void TopRail(Transform root, Vector3 pos, Vector3 scale, Material mat)
        {
            var r = BuildKit.Cube("TopRail", root, pos, scale, Chrome, 0.9f, 0.85f);
            r.GetComponent<Renderer>().sharedMaterial = mat;
            NoCol(r);
        }

        static void Baluster(Transform root, Vector3 pos, Material mat)
        {
            var b = BuildKit.Cylinder("Baluster", root, pos, new Vector3(0.06f, 0.5f, 0.06f), Chrome, 0.9f, 0.85f);
            b.GetComponent<Renderer>().sharedMaterial = mat;
            NoCol(b);
        }

        // ── glowing infinity pool set into the deck (forward) ────────────────
        static void BuildInfinityPool(Transform root)
        {
            Vector3 c = new Vector3(0f, 0.6f, 9.5f);

            // chrome coping frame
            var coping = BuildKit.Cube("PoolCoping", root, c + new Vector3(0f, 0.02f, 0f),
                new Vector3(7.2f, 0.18f, 5.2f), Chrome, 0.85f, 0.8f);
            NoCol(coping);

            // recessed water — emissive teal, glows from within
            var waterMat = ShaderCache.MakeMat(new Color(0.10f, 0.55f, 0.62f), 0.0f, 0.95f);
            waterMat.EnableKeyword("_EMISSION");
            waterMat.SetColor("_EmissionColor", new Color(0.12f, 0.6f, 0.7f) * 1.3f);
            var pool = BuildKit.Cube("InfinityPool", root, c + new Vector3(0f, 0.03f, 0f),
                new Vector3(6.6f, 0.12f, 4.6f), Color.white, 0f, 0.95f);
            pool.GetComponent<Renderer>().sharedMaterial = waterMat;
            NoCol(pool);

            // a couple of subtle ripple-line strips
            for (int i = 0; i < 3; i++)
            {
                var rip = BuildKit.Cube("Ripple" + i, root,
                    c + new Vector3(0f, 0.10f, -1.4f + i * 1.4f), new Vector3(6.0f, 0.02f, 0.08f),
                    new Color(0.7f, 0.95f, 1f), 0f, 0.9f);
                Emit(rip, new Color(0.5f, 0.9f, 1f) * 0.8f);
                NoCol(rip);
            }
        }

        // ── champagne-glass pyramid centrepiece (stacked emissive glass) ─────
        // A square stack of coupe glasses: each tier is a grid of stems + bowls,
        // glowing faint amber. Permanent showpiece (the boss topples its OWN copy).
        static void BuildChampagnePyramid(Transform root, Vector3 basePos)
        {
            var pyr = BuildKit.Root("ChampagnePyramid", root.position + basePos).transform;
            pyr.SetParent(root, true);
            pyr.localPosition = basePos;

            // base plinth (mirror-chrome)
            var plinth = BuildKit.Cylinder("PyramidPlinth", pyr, new Vector3(0f, 0.05f, 0f),
                new Vector3(5.0f, 0.18f, 5.0f), Chrome, 0.95f, 0.9f);
            NoCol(plinth);

            var glassMat = ShaderCache.MakeMat(GlassTint, 0.1f, 0.96f);
            glassMat.EnableKeyword("_EMISSION");
            glassMat.SetColor("_EmissionColor", new Color(0.95f, 0.82f, 0.4f) * 0.9f);
            var bubbleMat = ShaderCache.MakeMat(new Color(0.98f, 0.9f, 0.5f), 0f, 0.95f);
            bubbleMat.EnableKeyword("_EMISSION");
            bubbleMat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.45f) * 1.5f);

            int tiers = 4;
            float spacing = 1.05f;
            float tierH = 0.95f;
            for (int t = 0; t < tiers; t++)
            {
                int g = tiers - t;                       // 4,3,2,1 glasses per side
                float y = 0.18f + t * tierH;
                float off = (g - 1) * 0.5f * spacing;
                for (int ix = 0; ix < g; ix++)
                for (int iz = 0; iz < g; iz++)
                {
                    Vector3 p = new Vector3(ix * spacing - off, y, iz * spacing - off);
                    BuildCoupe(pyr, p, glassMat, bubbleMat);
                }
            }

            // a hero glass crowning the very top
            BuildCoupe(pyr, new Vector3(0f, 0.18f + tiers * tierH, 0f), glassMat, bubbleMat);
        }

        // a single coupe glass: foot + stem + bowl + a fizzing bubble inside
        static void BuildCoupe(Transform parent, Vector3 pos, Material glass, Material bubble)
        {
            var foot = BuildKit.Cylinder("CoupeFoot", parent, pos + new Vector3(0f, 0.04f, 0f),
                new Vector3(0.34f, 0.03f, 0.34f), Color.white, 0.1f, 0.95f);
            foot.GetComponent<Renderer>().sharedMaterial = glass; NoCol(foot);
            var stem = BuildKit.Cylinder("CoupeStem", parent, pos + new Vector3(0f, 0.26f, 0f),
                new Vector3(0.07f, 0.22f, 0.07f), Color.white, 0.1f, 0.95f);
            stem.GetComponent<Renderer>().sharedMaterial = glass; NoCol(stem);
            // bowl — a shallow inverted cone (cylinder scaled wide-top is fine as a coupe)
            var bowl = BuildKit.Cylinder("CoupeBowl", parent, pos + new Vector3(0f, 0.56f, 0f),
                new Vector3(0.52f, 0.16f, 0.52f), Color.white, 0.1f, 0.96f);
            bowl.GetComponent<Renderer>().sharedMaterial = glass; NoCol(bowl);
            // glowing fizz inside
            var fizz = BuildKit.Sphere("Fizz", parent, pos + new Vector3(0f, 0.6f, 0f),
                new Vector3(0.34f, 0.12f, 0.34f), Color.white, 0f, 0.9f);
            fizz.GetComponent<Renderer>().sharedMaterial = bubble; NoCol(fizz);
        }

        // ── deck loungers with parasols ──────────────────────────────────────
        static void BuildLoungers(Transform root)
        {
            Vector3[] spots =
            {
                new Vector3(-5.5f, 0.65f, 13.5f),
                new Vector3( 5.5f, 0.65f, 13.5f),
                new Vector3(-5.5f, 0.65f, 4.5f),
                new Vector3( 5.5f, 0.65f, 4.5f),
            };
            for (int i = 0; i < spots.Length; i++)
                BuildLounger(root, spots[i], i, (spots[i].x > 0f) ? -25f : 25f);
        }

        static void BuildLounger(Transform root, Vector3 pos, int idx, float yaw)
        {
            var l = BuildKit.Root("Lounger" + idx, root.position + pos).transform;
            l.SetParent(root, true);
            l.localPosition = pos;
            l.localRotation = Quaternion.Euler(0f, yaw, 0f);

            var cushion = ShaderCache.MakeMat(new Color(0.96f, 0.96f, 0.94f), 0.05f, 0.4f);
            var frame   = ShaderCache.MakeMat(Chrome, 0.9f, 0.85f);

            var seat = BuildKit.Cube("Seat", l, new Vector3(0f, 0.18f, 0f), new Vector3(0.9f, 0.16f, 2.0f), Color.white, 0, 0);
            seat.GetComponent<Renderer>().sharedMaterial = cushion; NoCol(seat);
            var back = BuildKit.Cube("Back", l, new Vector3(0f, 0.5f, -0.85f), new Vector3(0.9f, 0.7f, 0.16f), Color.white, 0, 0);
            back.transform.localRotation = Quaternion.Euler(-32f, 0f, 0f);
            back.GetComponent<Renderer>().sharedMaterial = cushion; NoCol(back);
            // chrome legs
            for (int sx = -1; sx <= 1; sx += 2)
            for (int sz = -1; sz <= 1; sz += 2)
            {
                var leg = BuildKit.Cylinder("Leg", l, new Vector3(sx * 0.4f, 0.05f, sz * 0.85f),
                    new Vector3(0.07f, 0.1f, 0.07f), Chrome, 0.9f, 0.85f);
                leg.GetComponent<Renderer>().sharedMaterial = frame; NoCol(leg);
            }

            // parasol pole + canopy
            var pole = BuildKit.Cylinder("ParasolPole", l, new Vector3(0.55f, 1.0f, 0.6f),
                new Vector3(0.08f, 1.0f, 0.08f), Chrome, 0.9f, 0.85f);
            pole.GetComponent<Renderer>().sharedMaterial = frame; NoCol(pole);
            var canopy = BuildKit.Cylinder("Canopy", l, new Vector3(0.55f, 2.0f, 0.6f),
                new Vector3(2.6f, 0.06f, 2.6f), new Color(0.9f, 0.8f, 0.3f), 0.2f, 0.5f);
            // cone-ish: scale top via a sphere cap on top
            NoCol(canopy);
            var cap = BuildKit.Sphere("CanopyCap", l, new Vector3(0.55f, 2.1f, 0.6f),
                new Vector3(2.6f, 0.7f, 2.6f), new Color(0.92f, 0.82f, 0.32f), 0.2f, 0.5f);
            NoCol(cap);
        }

        // ── radar / comms mast with a sweeping emissive dish ─────────────────
        static void BuildRadarMast(Transform root)
        {
            Vector3 basePos = new Vector3(0f, 7.7f, -16.5f);
            var mast = BuildKit.Cylinder("RadarMast", root, basePos + new Vector3(0f, 2.0f, 0f),
                new Vector3(0.22f, 2.0f, 0.22f), Chrome, 0.85f, 0.7f);
            NoCol(mast);

            // cross-yard
            var yard = BuildKit.Cube("MastYard", root, basePos + new Vector3(0f, 3.4f, 0f),
                new Vector3(3.2f, 0.1f, 0.1f), Chrome, 0.85f, 0.7f);
            NoCol(yard);

            // navigation lights on the yard tips
            var navR = BuildKit.Sphere("NavR", root, basePos + new Vector3(1.6f, 3.4f, 0f),
                new Vector3(0.18f, 0.18f, 0.18f), new Color(0.1f, 0.9f, 0.2f), 0.1f, 0.6f);
            Emit(navR, new Color(0.1f, 1f, 0.2f) * 2.5f);
            var navL = BuildKit.Sphere("NavL", root, basePos + new Vector3(-1.6f, 3.4f, 0f),
                new Vector3(0.18f, 0.18f, 0.18f), new Color(0.9f, 0.1f, 0.1f), 0.1f, 0.6f);
            Emit(navL, new Color(1f, 0.1f, 0.1f) * 2.5f);

            // sweeping radar dish (a flat cylinder that spins)
            var dish = BuildKit.Cylinder("RadarDish", root, basePos + new Vector3(0f, 4.3f, 0f),
                new Vector3(1.6f, 0.08f, 1.6f), new Color(0.8f, 0.83f, 0.88f), 0.7f, 0.7f);
            dish.transform.localRotation = Quaternion.Euler(70f, 0f, 0f);
            Emit(dish, new Color(0.3f, 0.5f, 0.7f) * 0.4f);
            NoCol(dish);
            dish.AddComponent<YachtSpin>().speed = 80f;

            // emissive beacon atop the mast
            var beacon = BuildKit.Sphere("MastBeacon", root, basePos + new Vector3(0f, 5.1f, 0f),
                new Vector3(0.3f, 0.3f, 0.3f), new Color(1f, 0.9f, 0.6f), 0.1f, 0.6f);
            Emit(beacon, new Color(1f, 0.85f, 0.4f) * 2.2f);
            beacon.AddComponent<BeaconPulse>().phase = 1.3f;
        }

        // ── mooring bollards with looped ropes along the deck edge ───────────
        static void BuildBollards(Transform root)
        {
            float[] zs = { 15f, 8f, 0f, -8f };
            foreach (var z in zs)
            foreach (var sx in new[] { -7.4f, 7.4f })
            {
                var bol = BuildKit.Cylinder("Bollard", root, new Vector3(sx, 0.95f, z),
                    new Vector3(0.35f, 0.4f, 0.35f), new Color(0.12f, 0.12f, 0.14f), 0.8f, 0.5f);
                NoCol(bol);
                var cap = BuildKit.Sphere("BollardCap", root, new Vector3(sx, 1.32f, z),
                    new Vector3(0.42f, 0.2f, 0.42f), Gold, 0.9f, 0.8f);
                Emit(cap, Gold * 0.3f);
                NoCol(cap);
            }
        }

        // ── gangway down the port side toward the island ────────────────────
        static void BuildGangway(Transform root)
        {
            var gang = BuildKit.Cube("Gangway", root, new Vector3(-8.6f, 0.0f, -6f),
                new Vector3(2.0f, 0.16f, 6.0f), HullWhite, 0.2f, 0.6f);
            gang.transform.localRotation = Quaternion.Euler(0f, 0f, 18f);
            NoCol(gang);
            // chrome handrails
            for (int s = -1; s <= 1; s += 2)
            {
                var hr = BuildKit.Cube("GangRail", root, new Vector3(-8.6f + s * 0.95f, 0.6f, -6f),
                    new Vector3(0.06f, 0.06f, 6.0f), Chrome, 0.9f, 0.85f);
                hr.transform.localRotation = Quaternion.Euler(0f, 0f, 18f);
                NoCol(hr);
            }
        }

        // ── helpers ──────────────────────────────────────────────────────────
        static void NoCol(GameObject g)
        {
            var c = g.GetComponent<Collider>();
            if (c != null) Object.Destroy(c);
        }

        static void Emit(GameObject g, Color emission)
        {
            var m = g.GetComponent<Renderer>().sharedMaterial;
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission);
        }
    }

    // Tiny self-contained spinner for the radar dish (keeps the builder
    // self-sufficient, mirroring BeaconPulse's pattern in MagnusArenaBuilder).
    public class YachtSpin : MonoBehaviour
    {
        public float speed = 60f;
        public Vector3 axis = Vector3.up;
        void Update() => transform.Rotate(axis, speed * Time.deltaTime, Space.Self);
    }
}
