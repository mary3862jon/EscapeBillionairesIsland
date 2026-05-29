using UnityEngine;

namespace Spoonacci
{
    // ─────────────────────────────────────────────────────────────────────────
    //  ZuckLabArenaBuilder — a sleek glass + concrete AI DATACENTER for the
    //  Mark Zuckersnort boss fight.
    //
    //  Build(center) drops a cold, futuristic compute hall around the given world
    //  point:
    //      • a polished Concrete floor inset with a CarbonFiber-panelled core slab
    //      • two long banks of server RACKS lining the hall, each rack stippled
    //        with hundreds of blinking emissive status LEDs (teal/blue/green)
    //      • holographic 'METAVERSE' RING PROJECTORS — emissive torus rings built
    //        from many thin cylinder segments, slowly counter-rotating in mid-air
    //      • huge glowing WALL SCREENS (emissive panels) ringing the back walls
    //      • brushed-metal structural columns + a coffered ceiling grid with
    //        recessed cool-white light strips and real point lights
    //      • a low keynote dais at centre (kept clear for the boss)
    //
    //  Cold blue/teal emission palette throughout. Everything is multi-part with
    //  metallic/smoothness contrast + emission, per the project's aesthetic bar.
    //
    //  Everything parents under one "Zuck AI Datacenter" root. Decorative parts
    //  have colliders stripped; only structures the player must not pass through
    //  (rack cabinets, columns, wall slabs) keep theirs.
    // ─────────────────────────────────────────────────────────────────────────
    public static class ZuckLabArenaBuilder
    {
        // ── cold lab palette ─────────────────────────────────────────────────
        static readonly Color FloorTint  = new Color(0.74f, 0.77f, 0.80f);
        static readonly Color PanelTint  = new Color(0.86f, 0.90f, 0.95f);
        static readonly Color RackBody   = new Color(0.10f, 0.12f, 0.15f);
        static readonly Color SteelLite  = new Color(0.66f, 0.70f, 0.76f);
        static readonly Color GlassTint  = new Color(0.42f, 0.62f, 0.78f);

        // emissive accents
        static readonly Color Teal  = new Color(0.10f, 0.85f, 0.95f);
        static readonly Color Blue  = new Color(0.20f, 0.55f, 1.00f);
        static readonly Color Green = new Color(0.15f, 1.00f, 0.55f);

        public static void Build(Vector3 center)
        {
            var root = BuildKit.Root("Zuck AI Datacenter", center).transform;

            BuildFloor(root);
            BuildColumnsAndCeiling(root);
            BuildServerBank(root, new Vector3(-12.5f, 0f, 0f), -1f);  // left bank, doors face +X (inward)
            BuildServerBank(root, new Vector3( 12.5f, 0f, 0f),  1f);  // right bank, doors face -X (inward)
            BuildWallScreens(root);
            BuildHoloProjectors(root);
            BuildKeynoteDais(root);
        }

        // ── polished concrete floor + carbon-fibre core slab ─────────────────
        static void BuildFloor(Transform root)
        {
            // big polished concrete pad (cool grey, high smoothness for a wet sheen)
            var floor = BuildKit.CubeTex("LabFloor", root, new Vector3(0f, 0.05f, 0f),
                new Vector3(38f, 0.1f, 38f), BossTextures.Concrete,
                FloorTint, 0.15f, 0.78f, new Vector2(9f, 9f));
            // floor is walkable; keep its (flat) collider as the ground inside the hall

            // inset carbon-fibre core slab under the centre (the "compute core")
            var core = BuildKit.CubeTex("CoreSlab", root, new Vector3(0f, 0.11f, 0f),
                new Vector3(16f, 0.04f, 16f), BossTextures.CarbonFiber,
                Color.white, 0.45f, 0.7f, new Vector2(6f, 6f));
            NoCol(core);

            // glowing inlay grid lines crossing the core (teal data-channels)
            for (int i = -2; i <= 2; i++)
            {
                var lineX = BuildKit.Cube("InlayX" + i, root, new Vector3(0f, 0.13f, i * 3.4f),
                    new Vector3(15.6f, 0.02f, 0.12f), Teal, 0f, 1f);
                Emit(lineX, Teal * 2.0f); NoCol(lineX);
                var lineZ = BuildKit.Cube("InlayZ" + i, root, new Vector3(i * 3.4f, 0.13f, 0f),
                    new Vector3(0.12f, 0.02f, 15.6f), Teal, 0f, 1f);
                Emit(lineZ, Teal * 2.0f); NoCol(lineZ);
            }
        }

        // ── brushed-metal columns + coffered ceiling with light strips ───────
        static void BuildColumnsAndCeiling(Transform root)
        {
            const float ceilingY = 9.0f;

            // 4 corner + 4 mid columns
            Vector3[] cols =
            {
                new Vector3(-15f, 0f, -15f), new Vector3( 15f, 0f, -15f),
                new Vector3( 15f, 0f,  15f), new Vector3(-15f, 0f,  15f),
                new Vector3(  0f, 0f, -16f), new Vector3(  0f, 0f,  16f),
                new Vector3(-16f, 0f,   0f), new Vector3( 16f, 0f,   0f),
            };
            foreach (var c in cols)
            {
                // structural column keeps its collider
                BuildKit.CubeTex("Column", root, c + Vector3.up * (ceilingY * 0.5f),
                    new Vector3(0.9f, ceilingY, 0.9f), BossTextures.BrushedMetal,
                    SteelLite, 0.8f, 0.5f, new Vector2(1f, 6f));
                // glowing data-conduit running up each column
                var conduit = BuildKit.Cube("Conduit", root, c + new Vector3(0.48f, ceilingY * 0.5f, 0f),
                    new Vector3(0.06f, ceilingY - 0.6f, 0.18f), Blue, 0f, 1f);
                Emit(conduit, Blue * 1.8f); NoCol(conduit);
            }

            // dark coffered ceiling slab
            var ceil = BuildKit.CubeTex("Ceiling", root, new Vector3(0f, ceilingY, 0f),
                new Vector3(36f, 0.4f, 36f), BossTextures.CarbonFiber,
                new Color(0.18f, 0.20f, 0.24f), 0.4f, 0.45f, new Vector2(10f, 10f));
            NoCol(ceil);

            // recessed cool-white light strips in a grid + a few real lights
            for (int gx = -2; gx <= 2; gx++)
            for (int gz = -2; gz <= 2; gz++)
            {
                var pos = new Vector3(gx * 7f, ceilingY - 0.28f, gz * 7f);
                var strip = BuildKit.Cube("CeilStrip", root, pos,
                    new Vector3(4.2f, 0.08f, 0.5f), new Color(0.85f, 0.95f, 1f), 0f, 0.9f);
                Emit(strip, new Color(0.7f, 0.9f, 1f) * 2.6f); NoCol(strip);

                // sprinkle a handful of real point lights so the hall actually glows
                if ((gx + gz) % 2 == 0)
                {
                    var lg = new GameObject("CeilLight");
                    lg.transform.SetParent(root, false);
                    lg.transform.localPosition = pos + Vector3.down * 0.3f;
                    var L = lg.AddComponent<Light>();
                    L.type = LightType.Point;
                    L.range = 11f;
                    L.intensity = 1.3f;
                    L.color = new Color(0.72f, 0.86f, 1f);
                }
            }
        }

        // ── a long bank of server racks. `inwardX` = +1/-1 points the LED face
        //    toward the hall centre. ──────────────────────────────────────────
        static void BuildServerBank(Transform root, Vector3 origin, float inwardX)
        {
            var bank = BuildKit.Root("ServerBank", root.position + origin).transform;
            bank.SetParent(root, true);
            bank.localPosition = origin;

            const int count = 7;
            for (int i = 0; i < count; i++)
            {
                float z = Mathf.Lerp(-11f, 11f, i / (float)(count - 1));
                BuildRack(bank, new Vector3(0f, 0f, z), inwardX, i);
            }

            // a continuous raised cable tray running along the top of the bank
            var tray = BuildKit.CubeTex("CableTray", bank, new Vector3(0f, 3.5f, 0f),
                new Vector3(1.2f, 0.18f, 23f), BossTextures.BrushedMetal,
                SteelLite, 0.8f, 0.45f, new Vector2(1f, 8f));
            NoCol(tray);
        }

        static void BuildRack(Transform bank, Vector3 pos, float inwardX, int idx)
        {
            var rack = BuildKit.Root("Rack" + idx, bank.position + pos).transform;
            rack.SetParent(bank, true);
            rack.localPosition = pos;

            // cabinet body — solid obstacle (keeps collider)
            BuildKit.CubeTex("Cabinet", rack, new Vector3(0f, 1.6f, 0f),
                new Vector3(1.5f, 3.2f, 2.6f), BossTextures.BrushedMetal,
                RackBody, 0.75f, 0.4f, new Vector2(1f, 3f));

            // dark front bezel facing the hall (carbon-fibre)
            float faceX = inwardX * 0.78f;
            var bezel = BuildKit.CubeTex("Bezel", rack, new Vector3(faceX, 1.6f, 0f),
                new Vector3(0.06f, 3.0f, 2.4f), BossTextures.CarbonFiber,
                new Color(0.06f, 0.07f, 0.09f), 0.5f, 0.5f, new Vector2(2f, 4f));
            NoCol(bezel);

            // grid of blinking status LEDs across the bezel
            int rows = 9, cols = 4;
            for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                float ly = 0.4f + r * 0.30f;
                float lz = -0.85f + c * 0.55f;
                // colour theme per LED: mostly teal, some blue, a few green
                int pick = (r * 7 + c * 3 + idx) % 10;
                Color led = pick < 6 ? Teal : (pick < 9 ? Blue : Green);
                var bulb = BuildKit.Cube("LED", rack,
                    new Vector3(faceX + inwardX * 0.05f, ly, lz),
                    new Vector3(0.04f, 0.10f, 0.10f), led, 0f, 1f);
                NoCol(bulb);
                Emit(bulb, led * 2.4f);
                // self-contained blink so the wall of LEDs feels alive
                var blink = bulb.AddComponent<LedBlink>();
                blink.baseColor = led * 2.4f;
                blink.phase = (r * 1.7f + c * 0.9f + idx * 2.3f);
                blink.speed = 3f + (pick % 4);
            }

            // ventilation slats along the top
            for (int s = 0; s < 5; s++)
            {
                var slat = BuildKit.Cube("Vent", rack,
                    new Vector3(faceX + inwardX * 0.02f, 2.9f, -0.9f + s * 0.45f),
                    new Vector3(0.05f, 0.05f, 0.35f), new Color(0.05f, 0.06f, 0.07f), 0.6f, 0.3f);
                NoCol(slat);
            }
        }

        // ── huge emissive wall screens ringing the back walls ────────────────
        static void BuildWallScreens(Transform root)
        {
            // back wall (north) + side strips: big "metaverse" billboards
            ScreenPanel(root, new Vector3(-7f, 4.2f, 17.5f), new Vector3(9f, 5f, 0.3f), 0f,
                new Color(0.12f, 0.45f, 0.95f));
            ScreenPanel(root, new Vector3( 7f, 4.2f, 17.5f), new Vector3(9f, 5f, 0.3f), 0f,
                new Color(0.10f, 0.80f, 0.92f));
            ScreenPanel(root, new Vector3(-7f, 4.2f, -17.5f), new Vector3(9f, 5f, 0.3f), 180f,
                new Color(0.10f, 0.80f, 0.92f));
            ScreenPanel(root, new Vector3( 7f, 4.2f, -17.5f), new Vector3(9f, 5f, 0.3f), 180f,
                new Color(0.12f, 0.45f, 0.95f));
        }

        // a framed wall screen with a brushed-metal bezel + emissive glass face.
        static void ScreenPanel(Transform root, Vector3 pos, Vector3 size, float yaw, Color glow)
        {
            var panel = BuildKit.Root("WallScreen", root.position + pos).transform;
            panel.SetParent(root, true);
            panel.localPosition = pos;
            panel.localRotation = Quaternion.Euler(0f, yaw, 0f);

            // metal frame (solid — also acts as the back wall section, keeps collider)
            BuildKit.CubeTex("ScreenFrame", panel, Vector3.zero,
                new Vector3(size.x, size.y, size.z), BossTextures.BrushedMetal,
                new Color(0.16f, 0.18f, 0.21f), 0.8f, 0.5f, new Vector2(3f, 2f));

            // emissive glass display, inset slightly toward the hall (-Z local)
            var face = BuildKit.Cube("ScreenFace", panel,
                new Vector3(0f, 0f, -size.z * 0.55f),
                new Vector3(size.x - 0.5f, size.y - 0.5f, 0.06f), glow, 0f, 0.95f);
            Emit(face, glow * 1.9f); NoCol(face);
            // slow brightness pulse like a live feed
            var pulse = face.AddComponent<ScreenFlicker>();
            pulse.baseColor = glow * 1.9f;
            pulse.phase = pos.x + pos.z;

            // a couple of darker UI bars across the face for a "dashboard" read
            for (int b = 0; b < 2; b++)
            {
                var bar = BuildKit.Cube("UiBar", panel,
                    new Vector3(0f, (b == 0 ? 1.4f : -1.4f), -size.z * 0.62f),
                    new Vector3(size.x - 1f, 0.25f, 0.04f), new Color(0.02f, 0.05f, 0.08f), 0f, 0.4f);
                NoCol(bar);
            }
        }

        // ── holographic 'metaverse' ring projectors ──────────────────────────
        static void BuildHoloProjectors(Transform root)
        {
            // three projector pylons around the hall, each emitting a floating ring
            Vector3[] spots =
            {
                new Vector3(-7f, 0f,  8f),
                new Vector3( 7f, 0f,  8f),
                new Vector3( 0f, 0f, -9f),
            };
            Color[] tints = { Teal, Blue, Green };
            for (int i = 0; i < spots.Length; i++)
                BuildProjector(root, spots[i], tints[i], i);
        }

        static void BuildProjector(Transform root, Vector3 pos, Color tint, int idx)
        {
            var proj = BuildKit.Root("HoloProjector" + idx, root.position + pos).transform;
            proj.SetParent(root, true);
            proj.localPosition = pos;

            // squat brushed-metal emitter base (solid)
            BuildKit.Cylinder("EmitterBase", proj, new Vector3(0f, 0.35f, 0f),
                new Vector3(1.3f, 0.35f, 1.3f), new Color(0.18f, 0.2f, 0.24f), 0.8f, 0.5f);
            var lensRing = BuildKit.Cylinder("EmitterLens", proj, new Vector3(0f, 0.72f, 0f),
                new Vector3(0.9f, 0.05f, 0.9f), tint, 0f, 1f);
            Emit(lensRing, tint * 2.6f); NoCol(lensRing);

            // the floating holographic ring(s): emissive torus built from thin
            // cylinder segments. Two concentric counter-rotating rings.
            var holo = new GameObject("HoloRings");
            holo.transform.SetParent(proj, false);
            holo.transform.localPosition = new Vector3(0f, 3.2f, 0f);

            MakeRing(holo.transform, 2.0f, 24, tint, 0.10f);
            MakeRing(holo.transform, 1.35f, 18, Color.Lerp(tint, Color.white, 0.4f), 0.08f);

            // a faint emissive beam linking emitter → rings
            var beam = BuildKit.Cylinder("HoloBeam", proj, new Vector3(0f, 1.95f, 0f),
                new Vector3(0.12f, 1.25f, 0.12f), tint, 0f, 1f);
            Emit(beam, tint * 1.3f); NoCol(beam);

            // counter-rotation animator on the holo node
            var spin = holo.AddComponent<HoloRingSpin>();
            spin.speedA = 35f + idx * 6f;
        }

        // one emissive ring lying flat in the XZ plane, made of `seg` thin
        // cylinder chords. Parented under `parent`; an inner child node "RingA/B"
        // lets HoloRingSpin counter-rotate the two rings.
        static void MakeRing(Transform parent, float radius, int seg, Color tint, float thick)
        {
            var ring = new GameObject(radius > 1.6f ? "RingA" : "RingB");
            ring.transform.SetParent(parent, false);
            ring.transform.localPosition = Vector3.zero;

            var mat = ShaderCache.MakeMat(tint, 0f, 1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", tint * 2.4f);

            float chord = 2f * Mathf.PI * radius / seg * 1.08f;  // slight overlap
            for (int i = 0; i < seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                var p = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
                var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                c.name = "Seg";
                c.transform.SetParent(ring.transform, false);
                c.transform.localPosition = p;
                // cylinder default axis is +Y; lay it flat and tangent to the ring
                c.transform.localRotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 90f);
                c.transform.localScale = new Vector3(thick, chord * 0.5f, thick);
                Object.Destroy(c.GetComponent<Collider>());
                c.GetComponent<Renderer>().sharedMaterial = mat;
            }
        }

        // ── low keynote dais at centre (boss stands here; kept clear) ────────
        static void BuildKeynoteDais(Transform root)
        {
            var dais = BuildKit.Cylinder("KeynoteDais", root, new Vector3(0f, 0.18f, 0f),
                new Vector3(5.5f, 0.18f, 5.5f), new Color(0.14f, 0.15f, 0.18f), 0.5f, 0.6f);
            NoCol(dais);
            // glowing rim ring around the dais
            var rim = BuildKit.Cylinder("DaisRim", root, new Vector3(0f, 0.30f, 0f),
                new Vector3(5.7f, 0.05f, 5.7f), Teal, 0f, 1f);
            Emit(rim, Teal * 2.2f); NoCol(rim);
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

    // ── tiny self-contained animators (keep the arena builder self-sufficient) ──

    // Pulses a single LED's emission on/off so the rack walls shimmer.
    public class LedBlink : MonoBehaviour
    {
        public Color baseColor = Color.cyan;
        public float phase = 0f;
        public float speed = 3f;
        Material _mat;

        void Start()
        {
            var r = GetComponent<Renderer>();
            if (r != null) _mat = r.material; // instance so each LED blinks alone
        }

        void Update()
        {
            if (_mat == null) return;
            float t = Mathf.Sin(Time.time * speed + phase) * 0.5f + 0.5f;
            // mostly-on with occasional dim flickers
            float k = Mathf.Lerp(0.25f, 1.25f, t * t);
            _mat.SetColor("_EmissionColor", baseColor * k);
        }
    }

    // Slow brightness pulse for the big wall screens (live-feed feel).
    public class ScreenFlicker : MonoBehaviour
    {
        public Color baseColor = Color.cyan;
        public float phase = 0f;
        Material _mat;

        void Start()
        {
            var r = GetComponent<Renderer>();
            if (r != null) _mat = r.material;
        }

        void Update()
        {
            if (_mat == null) return;
            float t = Mathf.Sin(Time.time * 1.6f + phase) * 0.5f + 0.5f;
            float micro = Mathf.Sin(Time.time * 11f + phase) * 0.06f;
            _mat.SetColor("_EmissionColor", baseColor * (0.8f + t * 0.35f + micro));
        }
    }

    // Counter-rotates the two holographic rings (RingA one way, RingB the other).
    public class HoloRingSpin : MonoBehaviour
    {
        public float speedA = 35f;
        Transform _a, _b;

        void Start()
        {
            _a = transform.Find("RingA");
            _b = transform.Find("RingB");
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (_a != null) _a.Rotate(0f, speedA * dt, 0f, Space.Self);
            if (_b != null) _b.Rotate(0f, -speedA * 1.4f * dt, 0f, Space.Self);
            // gentle bob of the whole projection
            var p = transform.localPosition;
            p.y = 3.2f + Mathf.Sin(Time.time * 1.3f + speedA) * 0.12f;
            transform.localPosition = p;
        }
    }
}
