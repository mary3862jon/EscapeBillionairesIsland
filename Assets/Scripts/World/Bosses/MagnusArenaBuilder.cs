using UnityEngine;

namespace Spoonacci
{
    // ─────────────────────────────────────────────────────────────────────────
    //  MagnusArenaBuilder — a detailed SpaceX-style rocket-launch complex.
    //
    //  Build(center) drops a full launch pad around the given world point:
    //      • circular tarmac apron ringed with hazard-stripe curbing
    //      • a dark concrete flame trench / blast scorch under pad-centre
    //      • a tall steel GANTRY TOWER built from truss segments (crossed beams,
    //        NOT a solid box) with a swing-out service arm
    //      • 3 frost-ringed fuel tanks (brushed-metal capsules)
    //      • floodlight masts with emissive lamps
    //      • pulsing red warning beacons
    //
    //  The rocket itself is built/owned by MagnusTuskBoss, so the exact pad centre
    //  is left clear for it.
    //
    //  Everything is parented under one "Magnus Launch Complex" root. Decorative
    //  parts have their colliders stripped; only structures the player must not
    //  walk through (tower legs, fuel tanks) keep theirs.
    // ─────────────────────────────────────────────────────────────────────────
    public static class MagnusArenaBuilder
    {
        // colour palette
        static readonly Color SteelDark = new Color(0.30f, 0.31f, 0.34f);
        static readonly Color SteelLite = new Color(0.62f, 0.64f, 0.68f);
        static readonly Color FrostC    = new Color(0.86f, 0.92f, 0.97f);

        public static void Build(Vector3 center)
        {
            var root = BuildKit.Root("Magnus Launch Complex", center).transform;

            BuildPad(root);
            BuildFlameTrench(root);
            BuildHazardRing(root);
            BuildGantryTower(root, new Vector3(-9f, 0f, -2.5f));
            BuildFuelFarm(root);
            BuildFloodMasts(root);
            BuildBeacons(root);
        }

        // ── circular tarmac apron ────────────────────────────────────────────
        static void BuildPad(Transform root)
        {
            // big flat cylinder of tarmac; thin disc, kept walkable (no side-collider issues)
            var pad = BuildKit.Cylinder("TarmacPad", root, new Vector3(0f, 0.04f, 0f),
                new Vector3(34f, 0.08f, 34f), Color.white, 0.05f, 0.18f);
            pad.GetComponent<Renderer>().sharedMaterial =
                ShaderCache.MakeTextured(BossTextures.Tarmac, new Color(0.9f, 0.9f, 0.92f),
                    0.05f, 0.18f, new Vector2(9f, 9f));

            // a raised concrete launch table directly under pad-centre (rocket sits here)
            var table = BuildKit.Cylinder("LaunchTable", root, new Vector3(0f, 0.35f, 0f),
                new Vector3(9f, 0.55f, 9f), Color.white, 0.1f, 0.3f);
            table.GetComponent<Renderer>().sharedMaterial =
                ShaderCache.MakeTextured(BossTextures.Concrete, new Color(0.78f, 0.78f, 0.8f),
                    0.1f, 0.3f, new Vector2(3f, 3f));

            // four hold-down clamps around the table edge (steel pylons)
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
                var p = new Vector3(Mathf.Cos(a) * 3.7f, 1.0f, Mathf.Sin(a) * 3.7f);
                var clamp = BuildKit.CubeTex("HoldDown" + i, root, p,
                    new Vector3(0.45f, 1.3f, 0.45f), BossTextures.BrushedMetal,
                    SteelLite, 0.85f, 0.55f, new Vector2(1f, 2f));
                NoCol(clamp);
                // angled cap pointing inward
                var cap = BuildKit.Cube("HoldDownCap" + i, clamp.transform,
                    new Vector3(0f, 0.5f, 0f), new Vector3(1.15f, 0.5f, 1.15f),
                    SteelDark, 0.8f, 0.4f);
                NoCol(cap);
            }
        }

        // ── dark concrete flame trench / scorch under the pad ────────────────
        static void BuildFlameTrench(Transform root)
        {
            // scorched disc sitting just above tarmac, beneath the launch table
            var scorch = BuildKit.Cylinder("BlastScorch", root, new Vector3(0f, 0.09f, 0f),
                new Vector3(13f, 0.06f, 13f), Color.white, 0.0f, 0.05f);
            scorch.GetComponent<Renderer>().sharedMaterial =
                ShaderCache.MakeTextured(BossTextures.Concrete, new Color(0.18f, 0.17f, 0.16f),
                    0.0f, 0.05f, new Vector2(4f, 4f));
            NoCol(scorch);

            // a deflector wedge to one side (the angled flame diverter)
            var wedge = BuildKit.CubeTex("FlameDeflector", root, new Vector3(0f, 0.5f, 6.5f),
                new Vector3(6f, 1.0f, 3.5f), BossTextures.Concrete,
                new Color(0.32f, 0.30f, 0.28f), 0.05f, 0.1f, new Vector2(3f, 2f));
            wedge.transform.localRotation = Quaternion.Euler(28f, 0f, 0f);
            NoCol(wedge);
        }

        // ── hazard-stripe curb ring around the apron edge ────────────────────
        static void BuildHazardRing(Transform root)
        {
            const int seg = 28;
            const float r = 33f;
            var ring = BuildKit.Root("HazardRing", root.position).transform;
            ring.SetParent(root, false);
            for (int i = 0; i < seg; i++)
            {
                float a = (i / (float)seg) * Mathf.PI * 2f;
                var p = new Vector3(Mathf.Cos(a) * r, 0.22f, Mathf.Sin(a) * r);
                var curb = BuildKit.CubeTex("Curb" + i, ring, p,
                    new Vector3(7.6f, 0.42f, 0.9f), BossTextures.HazardStripe,
                    Color.white, 0.0f, 0.2f, new Vector2(2f, 1f));
                curb.transform.localRotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg + 90f, 0f);
                NoCol(curb);
            }
        }

        // ── steel gantry tower (truss segments) + service arm ────────────────
        static void BuildGantryTower(Transform root, Vector3 basePos)
        {
            var tower = BuildKit.Root("GantryTower", root.position + basePos).transform;
            tower.SetParent(root, true);
            tower.localPosition = basePos;

            const int floors = 7;
            const float h = 2.6f;        // segment height
            const float w = 3.2f;        // tower footprint half-width*2
            float hw = w * 0.5f;

            // 4 vertical legs (rusted steel, collide so player can't pass through)
            Vector3[] corners =
            {
                new Vector3(-hw, 0, -hw), new Vector3( hw, 0, -hw),
                new Vector3( hw, 0,  hw), new Vector3(-hw, 0,  hw),
            };
            float totalH = floors * h;
            foreach (var c in corners)
            {
                var leg = BuildKit.CubeTex("Leg", tower,
                    c + Vector3.up * (totalH * 0.5f),
                    new Vector3(0.34f, totalH, 0.34f), BossTextures.RustedSteel,
                    Color.white, 0.7f, 0.35f, new Vector2(1f, 8f));
                // legs keep their collider — structural, player blocked
            }

            // per-floor horizontal rails + crossed diagonal braces on each face
            for (int f = 0; f < floors; f++)
            {
                float y0 = f * h;
                float yMid = y0 + h * 0.5f;
                float yTop = y0 + h;

                // horizontal girders at the top of each segment (4 sides)
                AddGirder(tower, new Vector3(0, yTop, -hw), new Vector3(w, 0.18f, 0.18f));
                AddGirder(tower, new Vector3(0, yTop,  hw), new Vector3(w, 0.18f, 0.18f));
                AddGirder(tower, new Vector3(-hw, yTop, 0), new Vector3(0.18f, 0.18f, w));
                AddGirder(tower, new Vector3( hw, yTop, 0), new Vector3(0.18f, 0.18f, w));

                // crossed diagonal braces on the two visible faces (X members)
                AddBrace(tower, new Vector3(0, yMid, -hw), w, h, true,  false);
                AddBrace(tower, new Vector3(0, yMid, -hw), w, h, false, false);
                AddBrace(tower, new Vector3(-hw, yMid, 0), w, h, true,  true);
                AddBrace(tower, new Vector3(-hw, yMid, 0), w, h, false, true);
            }

            // crew/service platform near the top
            var plat = BuildKit.CubeTex("ServicePlatform", tower,
                new Vector3(0, totalH - 0.3f, 0), new Vector3(w + 0.6f, 0.25f, w + 0.6f),
                BossTextures.BrushedMetal, SteelLite, 0.7f, 0.4f, new Vector2(3f, 3f));
            NoCol(plat);

            // swing-out service arm reaching toward the pad centre
            float armLen = Mathf.Abs(basePos.x) - 1.5f;
            var arm = BuildKit.CubeTex("ServiceArm", tower,
                new Vector3(hw + armLen * 0.5f, totalH - 1.6f, 0f),
                new Vector3(armLen, 0.35f, 0.7f), BossTextures.BrushedMetal,
                SteelLite, 0.75f, 0.45f, new Vector2(4f, 1f));
            NoCol(arm);
            // arm railing
            var rail = BuildKit.Cube("ArmRail", arm.transform, new Vector3(0f, 0.6f, 0.45f),
                new Vector3(1f, 0.18f, 0.06f), SteelDark, 0.6f, 0.3f);
            NoCol(rail);

            // a yellow caution lamp on the arm tip
            var lamp = BuildKit.Sphere("ArmLamp", arm.transform, new Vector3(0.48f, 0.3f, 0f),
                new Vector3(0.3f, 0.3f, 0.3f), new Color(1f, 0.8f, 0.1f), 0.2f, 0.6f);
            Emit(lamp, new Color(1f, 0.7f, 0.05f) * 2.5f);
        }

        static void AddGirder(Transform tower, Vector3 pos, Vector3 scale)
        {
            var g = BuildKit.CubeTex("Girder", tower, pos, scale,
                BossTextures.RustedSteel, Color.white, 0.65f, 0.3f, new Vector2(2f, 1f));
            NoCol(g);
        }

        // one diagonal member of an X-brace. `flip` swaps the diagonal; `sideZ`
        // orients it on a Z-facing tower face instead of an X-facing one.
        static void AddBrace(Transform tower, Vector3 center, float span, float height,
                             bool flip, bool sideZ)
        {
            float len = Mathf.Sqrt(span * span + height * height);
            float ang = Mathf.Atan2(height, span) * Mathf.Rad2Deg * (flip ? 1f : -1f);
            var b = BuildKit.CubeTex("Brace", tower, center,
                new Vector3(len, 0.14f, 0.14f), BossTextures.RustedSteel,
                Color.white, 0.65f, 0.3f, new Vector2(3f, 1f));
            if (sideZ)
                b.transform.localRotation = Quaternion.Euler(ang, 90f, 0f);
            else
                b.transform.localRotation = Quaternion.Euler(0f, 0f, ang);
            NoCol(b);
        }

        // ── fuel tank farm: 3 frost-ringed brushed-metal capsules ────────────
        static void BuildFuelFarm(Transform root)
        {
            Vector3[] spots =
            {
                new Vector3(10.5f, 0f,  6f),
                new Vector3(12.5f, 0f, -1f),
                new Vector3(10.5f, 0f, -8f),
            };
            float[] heights = { 6.5f, 5.0f, 7.0f };
            for (int i = 0; i < spots.Length; i++)
                BuildTank(root, spots[i], heights[i], i);
        }

        static void BuildTank(Transform root, Vector3 pos, float height, int idx)
        {
            var tank = BuildKit.Root("FuelTank" + idx, root.position + pos).transform;
            tank.SetParent(root, true);
            tank.localPosition = pos;

            float r = 1.7f;
            // cylindrical body (keeps collider — solid obstacle)
            var body = BuildKit.Cylinder("TankBody", tank, new Vector3(0f, height * 0.5f, 0f),
                new Vector3(r * 2f, height * 0.5f, r * 2f), Color.white, 0.65f, 0.55f);
            body.GetComponent<Renderer>().sharedMaterial =
                ShaderCache.MakeTextured(BossTextures.BrushedMetal, new Color(0.82f, 0.85f, 0.88f),
                    0.7f, 0.6f, new Vector2(3f, 4f));

            // domed top + bottom caps
            var top = BuildKit.Sphere("TankTop", tank, new Vector3(0f, height, 0f),
                new Vector3(r * 2f, r * 1.4f, r * 2f), new Color(0.8f, 0.83f, 0.86f), 0.7f, 0.6f);
            var bot = BuildKit.Sphere("TankBot", tank, new Vector3(0f, 0f, 0f),
                new Vector3(r * 2f, r * 1.2f, r * 2f), new Color(0.8f, 0.83f, 0.86f), 0.7f, 0.6f);

            // frost rings — bands of icy white toward the bottom (cryogenic look)
            int rings = 3;
            for (int k = 0; k < rings; k++)
            {
                float fy = Mathf.Lerp(0.8f, height * 0.55f, k / (float)(rings - 1));
                var ring = BuildKit.Cylinder("Frost" + k, tank, new Vector3(0f, fy, 0f),
                    new Vector3(r * 2f + 0.08f, 0.22f - k * 0.05f, r * 2f + 0.08f),
                    FrostC, 0.1f, 0.85f);
                NoCol(ring);
            }
            // a vent pipe up the side
            var pipe = BuildKit.Cylinder("VentPipe", tank, new Vector3(r + 0.2f, height * 0.5f, 0f),
                new Vector3(0.25f, height * 0.5f, 0.25f), SteelDark, 0.8f, 0.4f);
            NoCol(pipe);
            // hazard placard
            var placard = BuildKit.CubeTex("TankPlacard", tank,
                new Vector3(0f, height * 0.45f, r + 0.05f), new Vector3(1.1f, 1.1f, 0.06f),
                BossTextures.HazardStripe, Color.white, 0f, 0.2f, new Vector2(1f, 1f));
            NoCol(placard);
        }

        // ── floodlight masts ─────────────────────────────────────────────────
        static void BuildFloodMasts(Transform root)
        {
            Vector3[] spots =
            {
                new Vector3(-13f, 0f,  12f),
                new Vector3( 13f, 0f,  13f),
                new Vector3(-14f, 0f, -11f),
                new Vector3( 12f, 0f, -13f),
            };
            foreach (var s in spots) BuildMast(root, s);
        }

        static void BuildMast(Transform root, Vector3 pos)
        {
            var mast = BuildKit.Root("FloodMast", root.position + pos).transform;
            mast.SetParent(root, true);
            mast.localPosition = pos;

            float h = 7.5f;
            var pole = BuildKit.CubeTex("MastPole", mast, new Vector3(0f, h * 0.5f, 0f),
                new Vector3(0.3f, h, 0.3f), BossTextures.BrushedMetal,
                SteelLite, 0.75f, 0.4f, new Vector2(1f, 6f));
            // pole keeps collider — slim obstacle, fine to block

            // cross-head holding 3 lamps, angled toward pad centre
            var head = BuildKit.Cube("MastHead", mast, new Vector3(0f, h, 0f),
                new Vector3(2.6f, 0.2f, 0.4f), SteelDark, 0.7f, 0.4f);
            NoCol(head);
            // orient the head to face the centre
            Vector3 flat = new Vector3(-pos.x, 0f, -pos.z);
            if (flat.sqrMagnitude > 0.01f)
                head.transform.localRotation = Quaternion.LookRotation(flat.normalized) *
                                               Quaternion.Euler(0f, 90f, 0f);

            for (int i = -1; i <= 1; i++)
            {
                var lamp = BuildKit.Cube("Floodlamp", head.transform,
                    new Vector3(i * 0.9f, 0f, 0.35f), new Vector3(0.7f, 0.5f, 0.25f),
                    new Color(1f, 0.97f, 0.85f), 0.2f, 0.6f);
                NoCol(lamp);
                Emit(lamp, new Color(1f, 0.96f, 0.8f) * 3.2f);
            }
            // a real point light so the pad actually glows at night
            var lightGo = new GameObject("FloodLight");
            lightGo.transform.SetParent(head.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, -0.3f, 0.5f);
            var L = lightGo.AddComponent<Light>();
            L.type = LightType.Spot;
            L.range = 38f;
            L.spotAngle = 70f;
            L.intensity = 2.4f;
            L.color = new Color(1f, 0.97f, 0.88f);
            lightGo.transform.localRotation = Quaternion.Euler(35f, 0f, 0f);
        }

        // ── pulsing red warning beacons on short posts around the pad ────────
        static void BuildBeacons(Transform root)
        {
            const int n = 6;
            const float r = 16f;
            for (int i = 0; i < n; i++)
            {
                float a = (i / (float)n) * Mathf.PI * 2f + 0.4f;
                var p = new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
                var post = BuildKit.CubeTex("BeaconPost" + i, root,
                    p + Vector3.up * 0.7f, new Vector3(0.22f, 1.4f, 0.22f),
                    BossTextures.BrushedMetal, SteelLite, 0.7f, 0.4f, new Vector2(1f, 2f));
                NoCol(post);
                var bulb = BuildKit.Sphere("Beacon" + i, post.transform,
                    new Vector3(0f, 0.55f, 0f), new Vector3(1.6f, 1.6f, 1.6f),
                    new Color(1f, 0.12f, 0.08f), 0.1f, 0.6f);
                Emit(bulb, new Color(1f, 0.08f, 0.05f) * 2.8f);
                var pulse = bulb.AddComponent<BeaconPulse>();
                pulse.phase = i * 0.7f;
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

    // Tiny self-contained pulse component for the warning beacons:
    // slowly brightens/dims the emission and spins gently. Lives here so the
    // arena builder is fully self-sufficient (per the contract's allowance).
    public class BeaconPulse : MonoBehaviour
    {
        public float phase = 0f;
        public float speed = 2.2f;
        Material _mat;
        Color _base;

        void Start()
        {
            var r = GetComponent<Renderer>();
            if (r != null)
            {
                // instance the material so each beacon pulses independently
                _mat = r.material;
                _base = _mat.GetColor("_EmissionColor");
            }
        }

        void Update()
        {
            transform.Rotate(0f, 60f * Time.deltaTime, 0f, Space.Self);
            if (_mat == null) return;
            float t = (Mathf.Sin(Time.time * speed + phase) * 0.5f + 0.5f);
            float k = Mathf.Lerp(0.35f, 1.15f, t);
            _mat.SetColor("_EmissionColor", _base * k);
        }
    }
}
