using UnityEngine;

namespace Spoonacci
{
    // ─────────────────────────────────────────────────────────────────────────
    //  ChadVaultArenaBuilder — a neon CRYPTO VAULT / CASINO arena.
    //
    //  Build(center) drops a degen casino-vault complex around the world point:
    //      • a circular polished-marble casino floor ringed with hazard curbing
    //      • a GIANT emissive gold coin VAULT DOOR (multi-ring spoked wheel) set
    //        into a rusted-steel back wall, with a chunky locking handle
    //      • flanking stacks of glowing gold coins (cylinder stacks w/ symbols)
    //      • a pulsing candlestick-chart wall (green/red bars that breathe)
    //      • neon 'TO THE MOON' signage built from emissive bar segments
    //      • purple-neon floor strips + casino spotlights
    //
    //  Palette: RustedSteel + gold + purple-neon (matches the spec). The pile of
    //  coins Chad stands on is built by CryptoChadBoss; the centre is left clear.
    //
    //  Everything is parented under one "Chad Crypto Vault" root. Decorative parts
    //  get their colliders stripped; only the back wall / vault frame block.
    // ─────────────────────────────────────────────────────────────────────────
    public static class ChadVaultArenaBuilder
    {
        // palette
        static readonly Color Gold      = new Color(0.95f, 0.74f, 0.18f);
        static readonly Color GoldDeep  = new Color(0.72f, 0.50f, 0.08f);
        static readonly Color Purple    = new Color(0.62f, 0.18f, 0.95f);
        static readonly Color PurpleEm  = new Color(0.55f, 0.12f, 1f);
        static readonly Color Cyan      = new Color(0.25f, 0.95f, 0.95f);
        static readonly Color Green     = new Color(0.15f, 0.95f, 0.35f);
        static readonly Color Red       = new Color(0.95f, 0.18f, 0.18f);

        public static void Build(Vector3 center)
        {
            var root = BuildKit.Root("Chad Crypto Vault", center).transform;

            BuildFloor(root);
            BuildHazardRing(root);
            BuildBackWall(root, new Vector3(0f, 0f, -13.5f));
            BuildVaultDoor(root, new Vector3(0f, 4.6f, -13.0f));
            BuildCoinStacks(root);
            BuildCandlestickWall(root, new Vector3(11.5f, 0f, -7f));
            BuildMoonSign(root, new Vector3(0f, 11.5f, -12.6f));
            BuildNeonFloorStrips(root);
            BuildCasinoSpots(root);
        }

        // ── polished marble casino floor + a recessed neon ring ──────────────
        static void BuildFloor(Transform root)
        {
            var floor = BuildKit.Cylinder("CasinoFloor", root, new Vector3(0f, 0.04f, 0f),
                new Vector3(36f, 0.08f, 36f), Color.white, 0.2f, 0.7f);
            floor.GetComponent<Renderer>().sharedMaterial =
                ShaderCache.MakeTextured(ProceduralTextures.Marble, new Color(0.16f, 0.12f, 0.22f),
                    0.25f, 0.85f, new Vector2(6f, 6f));

            // raised felt/casino dais directly under centre (where Chad's coin pile sits)
            var dais = BuildKit.Cylinder("Dais", root, new Vector3(0f, 0.28f, 0f),
                new Vector3(10f, 0.4f, 10f), new Color(0.08f, 0.07f, 0.12f), 0.1f, 0.4f);
            dais.GetComponent<Renderer>().sharedMaterial =
                ShaderCache.MakeTextured(BossTextures.Concrete, new Color(0.12f, 0.10f, 0.16f),
                    0.1f, 0.35f, new Vector2(3f, 3f));

            // glowing purple neon trim ring around the dais
            var ring = BuildKit.Cylinder("DaisNeon", root, new Vector3(0f, 0.5f, 0f),
                new Vector3(10.4f, 0.06f, 10.4f), Purple, 0.2f, 0.9f);
            Emit(ring, PurpleEm * 2.6f);
            NoCol(ring);
        }

        // ── hazard-stripe curb ring around the floor edge ────────────────────
        static void BuildHazardRing(Transform root)
        {
            const int seg = 30;
            const float r = 34.5f;
            var ring = BuildKit.Root("HazardRing", root.position).transform;
            ring.SetParent(root, false);
            for (int i = 0; i < seg; i++)
            {
                float a = (i / (float)seg) * Mathf.PI * 2f;
                var p = new Vector3(Mathf.Cos(a) * r, 0.22f, Mathf.Sin(a) * r);
                var curb = BuildKit.CubeTex("Curb" + i, ring, p,
                    new Vector3(7.4f, 0.42f, 0.9f), BossTextures.HazardStripe,
                    Color.white, 0.0f, 0.2f, new Vector2(2f, 1f));
                curb.transform.localRotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg + 90f, 0f);
                NoCol(curb);
            }
        }

        // ── rusted-steel vault back wall the door is set into ────────────────
        static void BuildBackWall(Transform root, Vector3 basePos)
        {
            // main slab — keeps collider so the player can't walk out the back
            var wall = BuildKit.CubeTex("VaultBackWall", root, basePos + new Vector3(0f, 6f, 0f),
                new Vector3(26f, 12f, 1.4f), BossTextures.RustedSteel,
                new Color(0.55f, 0.56f, 0.6f), 0.7f, 0.4f, new Vector2(8f, 4f));

            // bolted brushed-metal trim frame around the door cavity
            var frame = BuildKit.CubeTex("VaultFrame", root, basePos + new Vector3(0f, 4.6f, 0.75f),
                new Vector3(11.5f, 11.5f, 0.5f), BossTextures.BrushedMetal,
                new Color(0.7f, 0.62f, 0.4f), 0.9f, 0.6f, new Vector2(3f, 3f));
            NoCol(frame);

            // rivet studs around the frame
            for (int i = 0; i < 16; i++)
            {
                float a = i / 16f * Mathf.PI * 2f;
                var stud = BuildKit.Sphere("Rivet" + i, frame.transform,
                    new Vector3(Mathf.Cos(a) * 0.47f, Mathf.Sin(a) * 0.47f, -0.55f),
                    new Vector3(0.05f, 0.05f, 0.05f), GoldDeep, 0.95f, 0.7f);
                NoCol(stud);
            }
        }

        // ── giant emissive gold coin VAULT DOOR (spoked wheel) ───────────────
        static void BuildVaultDoor(Transform root, Vector3 pos)
        {
            var door = BuildKit.Root("VaultDoor", root.position + pos).transform;
            door.SetParent(root, false);
            door.localPosition = pos;

            var goldMat = ShaderCache.MakeMat(Gold, 1f, 0.85f);
            goldMat.EnableKeyword("_EMISSION");
            goldMat.SetColor("_EmissionColor", new Color(0.9f, 0.65f, 0.12f) * 1.4f);

            // big round door disc (flattened cylinder, faces +Z)
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "DoorDisc"; disc.transform.SetParent(door, false);
            disc.transform.localPosition = new Vector3(0f, 0f, 0.95f);
            disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            disc.transform.localScale = new Vector3(8.6f, 0.4f, 8.6f);
            disc.GetComponent<Renderer>().sharedMaterial = goldMat;
            NoCol(disc);

            // concentric raised rings (brushed metal) for depth
            float[] ringR = { 7.4f, 5.6f, 3.6f };
            for (int k = 0; k < ringR.Length; k++)
            {
                var r = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                r.name = "DoorRing" + k; r.transform.SetParent(door, false);
                r.transform.localPosition = new Vector3(0f, 0f, 1.0f + k * 0.05f);
                r.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                r.transform.localScale = new Vector3(ringR[k], 0.5f - k * 0.08f, ringR[k]);
                r.GetComponent<Renderer>().sharedMaterial =
                    ShaderCache.MakeTextured(BossTextures.BrushedMetal,
                        new Color(0.78f, 0.62f, 0.28f), 0.95f, 0.7f, new Vector2(2f, 2f));
                NoCol(r);
            }

            // engraved ₿ bitcoin emblem at the centre (emissive)
            var emblem = BuildKit.Cube("BtcStem", door, new Vector3(0f, 0f, 1.3f),
                new Vector3(0.5f, 3.0f, 0.25f), Gold, 1f, 0.9f);
            Emit(emblem, new Color(1f, 0.8f, 0.2f) * 2.4f); NoCol(emblem);
            for (int s = 0; s < 2; s++)
            {
                var bump = BuildKit.Cube("BtcBump" + s, door,
                    new Vector3(0.55f, 0.7f - s * 1.4f, 1.3f), new Vector3(1.1f, 0.9f, 0.25f), Gold, 1f, 0.9f);
                Emit(bump, new Color(1f, 0.8f, 0.2f) * 2.4f); NoCol(bump);
            }
            // the two little vertical ticks of the ₿
            for (int s = 0; s < 2; s++)
            {
                var tick = BuildKit.Cube("BtcTick" + s, door,
                    new Vector3(-0.15f, 1.9f - s * 3.8f, 1.32f), new Vector3(0.22f, 0.7f, 0.22f), Gold, 1f, 0.9f);
                Emit(tick, new Color(1f, 0.8f, 0.2f) * 2.4f); NoCol(tick);
            }

            // 8 radial spokes
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * 360f;
                var spoke = BuildKit.Cube("Spoke" + i, door, Vector3.zero,
                    new Vector3(0.4f, 6.6f, 0.3f), new Color(0.6f, 0.5f, 0.2f), 0.95f, 0.65f);
                spoke.transform.localRotation = Quaternion.Euler(0f, 0f, a);
                spoke.transform.localPosition = Quaternion.Euler(0f, 0f, a) * Vector3.zero + new Vector3(0f, 0f, 1.15f);
                NoCol(spoke);
            }

            // chunky 5-spoke locking handle wheel, slightly proud of the door
            var hub = BuildKit.Cylinder("HandleHub", door, new Vector3(0f, 0f, 1.55f),
                new Vector3(1.1f, 0.18f, 1.1f), new Color(0.5f, 0.42f, 0.18f), 1f, 0.7f);
            hub.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            NoCol(hub);
            for (int i = 0; i < 5; i++)
            {
                float a = i / 5f * 360f;
                var arm = BuildKit.Cylinder("HandleArm" + i, door, Vector3.zero,
                    new Vector3(0.22f, 1.6f, 0.22f), new Color(0.55f, 0.46f, 0.2f), 1f, 0.7f);
                arm.transform.localRotation = Quaternion.Euler(0f, 0f, a);
                arm.transform.localPosition = Quaternion.Euler(0f, 0f, a) * new Vector3(0f, 1.4f, 0f) + new Vector3(0f, 0f, 1.55f);
                NoCol(arm);
                var grip = BuildKit.Sphere("HandleGrip" + i, door,
                    Quaternion.Euler(0f, 0f, a) * new Vector3(0f, 2.7f, 0f) + new Vector3(0f, 0f, 1.55f),
                    new Vector3(0.4f, 0.4f, 0.4f), Gold, 1f, 0.85f);
                Emit(grip, new Color(1f, 0.75f, 0.15f) * 1.6f); NoCol(grip);
            }

            // slow-spin the handle so the vault feels alive
            door.gameObject.AddComponent<VaultHandleSpin>();
        }

        // ── flanking stacks of glowing gold coins ────────────────────────────
        static void BuildCoinStacks(Transform root)
        {
            Vector3[] spots =
            {
                new Vector3(-8.5f, 0f, -6f),
                new Vector3( 8.5f, 0f, -6f),
                new Vector3(-6.5f, 0f,  2f),
                new Vector3( 6.5f, 0f,  2f),
            };
            int[] counts = { 7, 6, 5, 5 };
            for (int i = 0; i < spots.Length; i++)
                BuildCoinStack(root, spots[i], counts[i], i);
        }

        static void BuildCoinStack(Transform root, Vector3 pos, int count, int idx)
        {
            var stack = BuildKit.Root("CoinStack" + idx, root.position + pos).transform;
            stack.SetParent(root, false);
            stack.localPosition = pos;

            var coinMat = ShaderCache.MakeMat(Gold, 1f, 0.85f);
            coinMat.EnableKeyword("_EMISSION");
            coinMat.SetColor("_EmissionColor", new Color(0.85f, 0.6f, 0.12f) * 1.1f);
            var edgeMat = ShaderCache.MakeMat(GoldDeep, 1f, 0.7f);

            float h = 0.18f;
            for (int k = 0; k < count; k++)
            {
                float jx = Mathf.Sin(k * 2.3f + idx) * 0.06f;
                var coin = BuildKit.Cylinder("Coin" + k, stack,
                    new Vector3(jx, 0.4f + k * h, 0f), new Vector3(1.5f, h * 0.5f, 1.5f),
                    Gold, 1f, 0.85f);
                coin.GetComponent<Renderer>().sharedMaterial = (k % 2 == 0) ? coinMat : edgeMat;
                NoCol(coin);
            }
            // a tilted top coin showing its face + symbol
            float topY = 0.4f + count * h;
            var faceCoin = BuildKit.Cylinder("CoinTop", stack, new Vector3(0.15f, topY + 0.1f, 0.1f),
                new Vector3(1.5f, h * 0.5f, 1.5f), Gold, 1f, 0.85f);
            faceCoin.GetComponent<Renderer>().sharedMaterial = coinMat;
            faceCoin.transform.localRotation = Quaternion.Euler(72f, idx * 40f, 0f);
            NoCol(faceCoin);
            // little emissive $ symbol on top
            var sym = BuildKit.Cube("CoinSym", stack, new Vector3(0.15f, topY + 0.25f, 0.1f),
                new Vector3(0.18f, 0.7f, 0.06f), Cyan, 0.3f, 0.9f);
            Emit(sym, Cyan * 2.2f); NoCol(sym);
        }

        // ── pulsing candlestick-chart wall (green/red OHLC bars) ─────────────
        static void BuildCandlestickWall(Transform root, Vector3 basePos)
        {
            var wall = BuildKit.Root("CandlestickWall", root.position + basePos).transform;
            wall.SetParent(root, false);
            wall.localPosition = basePos;
            wall.localRotation = Quaternion.Euler(0f, -55f, 0f);

            // dark glass backing board
            var board = BuildKit.Cube("ChartBoard", wall, new Vector3(0f, 5f, 0f),
                new Vector3(12f, 9f, 0.3f), new Color(0.04f, 0.05f, 0.08f), 0.3f, 0.85f);
            NoCol(board);
            // neon frame
            var frame = BuildKit.Cube("ChartFrame", wall, new Vector3(0f, 5f, -0.08f),
                new Vector3(12.5f, 9.5f, 0.2f), Cyan, 0.4f, 0.9f);
            Emit(frame, Cyan * 1.4f); NoCol(frame);

            // candlesticks across the board — alternating bull/bear, mounted to a
            // pulsing controller so they breathe (pump-and-dump vibe).
            const int n = 11;
            for (int i = 0; i < n; i++)
            {
                float x = Mathf.Lerp(-5.2f, 5.2f, i / (float)(n - 1));
                bool bull = (i % 3 != 1);                 // mostly green, some red
                float bodyH = 1.0f + Mathf.Abs(Mathf.Sin(i * 1.7f)) * 3.2f;
                float baseY = 1.6f + Mathf.Sin(i * 0.9f) * 0.8f;
                Color c = bull ? Green : Red;

                // wick
                var wick = BuildKit.Cube("Wick" + i, wall, new Vector3(x, baseY + bodyH * 0.5f, 0.18f),
                    new Vector3(0.08f, bodyH + 1.6f, 0.08f), c, 0.3f, 0.8f);
                Emit(wick, c * 1.2f); NoCol(wick);
                // body
                var body = BuildKit.Cube("Candle" + i, wall, new Vector3(x, baseY + bodyH * 0.5f, 0.22f),
                    new Vector3(0.7f, bodyH, 0.16f), c, 0.3f, 0.85f);
                Emit(body, c * 1.8f); NoCol(body);

                var pulse = body.AddComponent<CandlePulse>();
                pulse.baseEmission = c * 1.8f;
                pulse.phase = i * 0.55f;
                pulse.body = body.transform;
                pulse.baseScaleY = bodyH;
            }
        }

        // ── neon 'TO THE MOON' signage from emissive bar segments ────────────
        static void BuildMoonSign(Transform root, Vector3 pos)
        {
            var sign = BuildKit.Root("MoonSign", root.position + pos).transform;
            sign.SetParent(root, false);
            sign.localPosition = pos;

            // backing bar
            var back = BuildKit.Cube("SignBack", sign, new Vector3(0f, 0f, 0.2f),
                new Vector3(13f, 2.4f, 0.25f), new Color(0.05f, 0.04f, 0.09f), 0.3f, 0.7f);
            NoCol(back);

            // word as a row of emissive segment-bars (purple/cyan neon tubes)
            // we don't render glyphs procedurally; instead a stylised neon strip +
            // a crisp WorldLabel for the actual readable text.
            int bars = 11;
            for (int i = 0; i < bars; i++)
            {
                float x = Mathf.Lerp(-5.8f, 5.8f, i / (float)(bars - 1));
                bool cy = (i % 2 == 0);
                Color c = cy ? Cyan : PurpleEm;
                var seg = BuildKit.Cube("MoonSeg" + i, sign, new Vector3(x, 0f, 0f),
                    new Vector3(0.85f, 1.5f, 0.18f), c, 0.3f, 0.9f);
                Emit(seg, c * 2.6f); NoCol(seg);
                var pulse = seg.AddComponent<CandlePulse>();
                pulse.baseEmission = c * 2.6f;
                pulse.phase = i * 0.4f;
            }

            // readable label sitting just in front of the neon bars
            var lblGo = new GameObject("MoonLabel");
            lblGo.transform.SetParent(sign, false);
            lblGo.transform.localPosition = new Vector3(0f, 0.1f, -0.6f);
            var w = lblGo.AddComponent<WorldLabel>();
            w.text = "TO THE MOON"; w.color = new Color(1f, 0.85f, 0.3f); w.fontSize = 34;

            // a little rocket-to-the-moon icon: gold crescent + tiny rocket
            var moon = BuildKit.Sphere("Moon", sign, new Vector3(6.6f, 0.2f, 0f),
                new Vector3(1.3f, 1.3f, 1.3f), new Color(0.95f, 0.92f, 0.75f), 0.2f, 0.6f);
            Emit(moon, new Color(0.95f, 0.9f, 0.6f) * 1.4f);
        }

        // ── purple neon strips embedded in the floor (radial) ────────────────
        static void BuildNeonFloorStrips(Transform root)
        {
            const int n = 8;
            for (int i = 0; i < n; i++)
            {
                float a = i / (float)n * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var strip = BuildKit.Cube("FloorNeon" + i, root, dir * 20f + Vector3.up * 0.1f,
                    new Vector3(0.4f, 0.04f, 22f), (i % 2 == 0) ? PurpleEm : Cyan, 0.3f, 0.9f);
                strip.transform.localRotation = Quaternion.LookRotation(dir);
                Emit(strip, ((i % 2 == 0) ? PurpleEm : Cyan) * 2.0f);
                NoCol(strip);
            }
        }

        // ── casino spotlights raking the dais ────────────────────────────────
        static void BuildCasinoSpots(Transform root)
        {
            Vector3[] spots =
            {
                new Vector3(-13f, 0f,  10f),
                new Vector3( 13f, 0f,  10f),
                new Vector3(-12f, 0f, -10f),
                new Vector3( 12f, 0f, -10f),
            };
            Color[] cols =
            {
                new Color(0.7f, 0.3f, 1f),
                new Color(0.3f, 0.9f, 1f),
                new Color(1f, 0.7f, 0.2f),
                new Color(0.9f, 0.2f, 0.6f),
            };
            for (int i = 0; i < spots.Length; i++)
                BuildSpot(root, spots[i], cols[i]);
        }

        static void BuildSpot(Transform root, Vector3 pos, Color col)
        {
            var mast = BuildKit.Root("SpotMast", root.position + pos).transform;
            mast.SetParent(root, false);
            mast.localPosition = pos;

            float h = 8.5f;
            var pole = BuildKit.CubeTex("SpotPole", mast, new Vector3(0f, h * 0.5f, 0f),
                new Vector3(0.3f, h, 0.3f), BossTextures.BrushedMetal,
                new Color(0.5f, 0.5f, 0.55f), 0.85f, 0.5f, new Vector2(1f, 6f));
            // pole keeps collider — slim obstacle

            var headBox = BuildKit.Cube("SpotHead", mast, new Vector3(0f, h, 0f),
                new Vector3(0.9f, 0.7f, 1.1f), new Color(0.1f, 0.1f, 0.12f), 0.7f, 0.5f);
            NoCol(headBox);
            Vector3 flat = new Vector3(-pos.x, 0f, -pos.z);
            if (flat.sqrMagnitude > 0.01f)
                headBox.transform.localRotation = Quaternion.LookRotation(flat.normalized);

            var lens = BuildKit.Cube("SpotLens", headBox.transform, new Vector3(0f, 0f, 0.6f),
                new Vector3(0.7f, 0.5f, 0.12f), col, 0.2f, 0.9f);
            Emit(lens, col * 3.2f); NoCol(lens);

            var lightGo = new GameObject("SpotLight");
            lightGo.transform.SetParent(headBox.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 0f, 0.7f);
            var L = lightGo.AddComponent<Light>();
            L.type = LightType.Spot;
            L.range = 40f;
            L.spotAngle = 55f;
            L.intensity = 3.0f;
            L.color = col;
            lightGo.transform.localRotation = Quaternion.Euler(38f, 0f, 0f);
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

    // Slow-spins the vault locking handle so the door reads as a live mechanism.
    public class VaultHandleSpin : MonoBehaviour
    {
        void Update()
        {
            transform.Rotate(0f, 0f, 12f * Time.deltaTime, Space.Self);
        }
    }

    // Breathing emissive pulse for candlesticks / neon segments. Optionally
    // scales a body transform vertically so the chart "pumps". Instances its own
    // material so each bar pulses independently. Fully self-contained.
    public class CandlePulse : MonoBehaviour
    {
        public Color baseEmission = Color.green;
        public float phase = 0f;
        public float speed = 2.4f;
        public Transform body;        // optional: vertical-scale target
        public float baseScaleY = 1f;

        Material _mat;
        Vector3 _baseScale;
        Vector3 _basePos;

        void Start()
        {
            var r = GetComponent<Renderer>();
            if (r != null) _mat = r.material;   // instance
            if (body != null)
            {
                _baseScale = body.localScale;
                _basePos = body.localPosition;
            }
        }

        void Update()
        {
            float t = Mathf.Sin(Time.time * speed + phase) * 0.5f + 0.5f;
            if (_mat != null)
            {
                float k = Mathf.Lerp(0.45f, 1.25f, t);
                _mat.SetColor("_EmissionColor", baseEmission * k);
            }
            if (body != null && baseScaleY > 0f)
            {
                float grow = Mathf.Lerp(0.7f, 1.25f, t);
                var s = _baseScale; s.y = baseScaleY * grow; body.localScale = s;
                // keep the candle's base pinned while it grows upward
                var p = _basePos; p.y = _basePos.y + (baseScaleY * grow - baseScaleY) * 0.5f;
                body.localPosition = p;
            }
        }
    }
}
