using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spoonacci
{
    // ─────────────────────────────────────────────────────────────────────────
    //  TitleScreenBootstrapper — the premium "attract screen" for
    //  Sir Spoonacci: Escape the Billionaire's Island.
    //
    //  Builds a fully code-driven cinematic title:
    //    • A hero spoon on a glowing pedestal under 3-point lighting + spotlight.
    //    • A slow cinematic camera orbiting the hero.
    //    • Atmosphere: raining gold coins + confetti, drifting clouds, sweeping
    //      searchlight cones, a distant neon island skyline, and the 4 boss-arena
    //      icons (rocket / yacht / datacenter / vault) glowing in the dark.
    //    • A modern, FOMO-driven IMGUI overlay: pulsing CTA, scrolling ticker,
    //      live-player counter, season countdown, NEW / SEASON 1 badge, stars.
    //
    //  HARD WIRING preserved: NEW GAME / CONTINUE / QUIT + the music mute button
    //  behave exactly as before. All IMGUI drawing goes through UiTheme / GUI.*
    //  (never GUIStyle.Draw) so it is repaint-safe.
    // ─────────────────────────────────────────────────────────────────────────
    public class TitleScreenBootstrapper : MonoBehaviour
    {
        GameObject heroSpoon;
        Camera cam;

        // overlay state
        GUIStyle titleStyle, subStyle, ctaStyle, btnStyle, btnDisabledStyle,
                 muteStyle, tickerStyle, badgeStyle, liveStyle, countdownStyle,
                 starStyle, footerStyle;
        float sceneStart;
        int livePlayers = 14209;
        float liveTick;
        float tickerScroll;
        const float SeasonSeconds = 47f * 3600f + 18f * 60f + 42f; // 47h:18m:42s
        string[] tickerMsgs;

        // ── boot ──────────────────────────────────────────────────────────────
        void Awake()
        {
            // make sure cursor is usable in built games (some setups hide it by default)
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            _ = SoundFx.Instance;
            _ = SaveSystem.Instance;
            MissionManager.BootstrapDefaults();
            SettingsManager.Load();
            _ = MusicPlayer.Instance;
            gameObject.AddComponent<PostFxBoost>();
            gameObject.AddComponent<LanguageToggle>();
            gameObject.AddComponent<GuiFontInstaller>(); // GTA-style bold font + outline on ALL text
            // No PauseMenu on the title screen — title already has its own buttons
            BuildScene();
        }

        // ── 3D backdrop ─────────────────────────────────────────────────────────
        void BuildScene()
        {
            sceneStart = Time.time;

            tickerMsgs = new[]
            {
                "NEW: 4 BOSS FIGHTS JUST DROPPED",
                "Magnus Tusk humiliated moments ago",
                "2,481,903 billionaires bonked worldwide",
                "SEASON 1 — limited time",
                "Trending #1 on the island",
                "Unlock the Golden Ladle skin this week only",
                "Yacht raid + Datacenter heist now LIVE",
            };

            // ── moody night-sky / deep teal ambience (premium attract mood) ──
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor     = new Color(0.10f, 0.12f, 0.22f);
            RenderSettings.ambientEquatorColor = new Color(0.16f, 0.10f, 0.20f);
            RenderSettings.ambientGroundColor  = new Color(0.04f, 0.04f, 0.08f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.06f, 0.07f, 0.13f);
            RenderSettings.fogDensity = 0.014f;

            // ── 3-POINT LIGHTING ──
            // key (warm, top-front)
            var keyGo = new GameObject("Key Light");
            keyGo.transform.rotation = Quaternion.Euler(48f, 24f, 0f);
            var key = keyGo.AddComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.7f;
            key.color = new Color(1f, 0.93f, 0.78f);
            key.shadows = LightShadows.Soft;

            // warm fill (low, soft, from front-left)
            var fillGo = new GameObject("Warm Fill");
            fillGo.transform.rotation = Quaternion.Euler(-12f, -40f, 0f);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.55f;
            fill.color = new Color(1f, 0.55f, 0.30f);

            // cool rim (from behind, separates hero from the dark)
            var rimGo = new GameObject("Cool Rim");
            rimGo.transform.rotation = Quaternion.Euler(18f, 200f, 0f);
            var rim = rimGo.AddComponent<Light>();
            rim.type = LightType.Directional;
            rim.intensity = 1.25f;
            rim.color = new Color(0.35f, 0.65f, 1f);

            // bright spotlight straight down on the hero
            var spotGo = new GameObject("Hero Spotlight");
            spotGo.transform.position = new Vector3(0f, 9f, -1.2f);
            spotGo.transform.rotation = Quaternion.Euler(72f, 0f, 0f);
            var spot = spotGo.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.spotAngle = 42f;
            spot.range = 22f;
            spot.intensity = 7f;
            spot.color = new Color(1f, 0.97f, 0.85f);
            spot.shadows = LightShadows.Soft;

            // sparkle point light hugging the spoon
            var sparkGo = new GameObject("Spoon Sparkle");
            sparkGo.transform.position = new Vector3(0f, 3.2f, -2.2f);
            var spark = sparkGo.AddComponent<Light>();
            spark.type = LightType.Point;
            spark.color = new Color(1f, 0.88f, 0.6f);
            spark.intensity = 3.5f;
            spark.range = 8f;

            // ── DARK MARBLE STAGE FLOOR ──
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Stage Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(8f, 1f, 8f);
            floor.GetComponent<Renderer>().sharedMaterial =
                ShaderCache.MakeTextured(ProceduralTextures.Marble, new Color(0.10f, 0.10f, 0.16f),
                                         0.35f, 0.78f, new Vector2(10f, 10f));

            // ── GLOWING PEDESTAL ──
            BuildPedestal();

            // ── HERO SPOON ──
            heroSpoon = new GameObject("Sir Spoonacci");
            heroSpoon.transform.position = new Vector3(0f, 2.05f, 0f);
            heroSpoon.transform.localScale = Vector3.one * 4.2f;
            heroSpoon.AddComponent<ProceduralSpoonBuilder>();

            // ── CINEMATIC CAMERA (orbits the hero) ──
            var camGo = new GameObject("Main Camera");
            cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();
            cam.fieldOfView = 46f;
            cam.backgroundColor = new Color(0.03f, 0.04f, 0.08f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            var orbit = camGo.AddComponent<TitleCameraOrbit>();
            orbit.target = new Vector3(0f, 2.6f, 0f);
            orbit.radius = 8.6f;
            orbit.height = 4.0f;
            orbit.degPerSec = 7f;     // slow + subtle
            orbit.bobAmp = 0.45f;

            // ── ATMOSPHERE ──
            BuildSearchlights();
            BuildSkyline();
            BuildBossIcons();
            BuildClouds();
            BuildCoinsAndConfetti();
        }

        void BuildPedestal()
        {
            // cylindrical plinth
            var col = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            col.name = "Pedestal";
            col.transform.position = new Vector3(0f, 0.55f, 0f);
            col.transform.localScale = new Vector3(2.4f, 0.55f, 2.4f);
            Destroy(col.GetComponent<Collider>());
            col.GetComponent<Renderer>().sharedMaterial =
                ShaderCache.MakeTextured(BossTextures.BrushedMetal, new Color(0.18f, 0.18f, 0.24f),
                                         0.9f, 0.85f, new Vector2(2f, 1f));

            // emissive gold ring cap that glows
            var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.name = "Pedestal Glow Ring";
            cap.transform.position = new Vector3(0f, 1.12f, 0f);
            cap.transform.localScale = new Vector3(2.2f, 0.06f, 2.2f);
            Destroy(cap.GetComponent<Collider>());
            cap.GetComponent<Renderer>().sharedMaterial = MakeEmissive(UiTheme.Gold, 2.4f);
        }

        void BuildSearchlights()
        {
            // 3 thin emissive cones rotating like premium attract-screen searchlights
            Color[] beamCols =
            {
                new Color(1f, 0.85f, 0.4f),
                new Color(0.4f, 0.8f, 1f),
                new Color(1f, 0.45f, 0.75f),
            };
            for (int i = 0; i < 3; i++)
            {
                var pivot = new GameObject("Searchlight " + i);
                pivot.transform.position = new Vector3(Mathf.Lerp(-9f, 9f, i / 2f), 0.2f, 7.5f);

                // a tall thin cone (inverted) made from a stretched cylinder
                var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                beam.name = "Beam";
                Destroy(beam.GetComponent<Collider>());
                beam.transform.SetParent(pivot.transform, false);
                beam.transform.localScale = new Vector3(0.6f, 9f, 0.6f);
                beam.transform.localPosition = new Vector3(0f, 9f, 0f);
                var bm = MakeEmissive(beamCols[i], 1.4f);
                bm.color = new Color(beamCols[i].r, beamCols[i].g, beamCols[i].b, 0.18f);
                beam.GetComponent<Renderer>().sharedMaterial = bm;

                var spin = pivot.AddComponent<TitleSearchlightSpin>();
                spin.speed = (i % 2 == 0 ? 1f : -1f) * (14f + i * 5f);
                spin.tilt = 26f + i * 4f;
                spin.phase = i * 1.7f;
            }
        }

        void BuildSkyline()
        {
            // distant neon island skyline silhouette — a row of emissive slabs far back
            var root = new GameObject("Neon Skyline");
            root.transform.position = new Vector3(0f, 0f, 26f);
            var rng = new System.Random(99);
            Color[] neon =
            {
                new Color(0.2f, 0.9f, 1f), new Color(1f, 0.35f, 0.8f),
                new Color(0.6f, 1f, 0.4f), new Color(1f, 0.8f, 0.25f),
            };
            for (int i = 0; i < 22; i++)
            {
                var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
                b.name = "Tower";
                Destroy(b.GetComponent<Collider>());
                b.transform.SetParent(root.transform, false);
                float h = (float)(2.5 + rng.NextDouble() * 9.0);
                float w = (float)(1.2 + rng.NextDouble() * 1.6);
                b.transform.localPosition = new Vector3((i - 11) * 2.5f, h * 0.5f, (float)(rng.NextDouble() * 6.0));
                b.transform.localScale = new Vector3(w, h, w);
                var c = neon[i % neon.Length];
                b.GetComponent<Renderer>().sharedMaterial = MakeEmissive(c * 0.9f, 1.1f);
            }
            // a couple of leaning palm silhouettes for the island vibe
            for (int i = 0; i < 4; i++)
            {
                var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Destroy(trunk.GetComponent<Collider>());
                trunk.transform.SetParent(root.transform, false);
                trunk.transform.localPosition = new Vector3((i - 1.5f) * 9f, 3f, -4f);
                trunk.transform.localScale = new Vector3(0.3f, 3f, 0.3f);
                trunk.transform.localRotation = Quaternion.Euler(0f, 0f, (i % 2 == 0 ? 10f : -10f));
                trunk.GetComponent<Renderer>().sharedMaterial = MakeEmissive(new Color(0.1f, 0.5f, 0.3f), 0.5f);
            }
        }

        void BuildBossIcons()
        {
            // 4 glowing primitive "icons" hinting the boss arenas, floating in the dark
            // rocket (Tusk), yacht (Beff), datacenter (server), vault (banker)
            BuildBossIcon("Rocket Arena",   new Vector3(-7.5f, 5.5f, 9f),  new Color(1f, 0.4f, 0.3f), MakeRocketIcon);
            BuildBossIcon("Yacht Arena",    new Vector3( 7.5f, 4.8f, 9f),  new Color(0.3f, 0.9f, 1f), MakeYachtIcon);
            BuildBossIcon("Datacenter",     new Vector3(-6.0f, 7.5f, 12f), new Color(0.5f, 1f, 0.5f), MakeServerIcon);
            BuildBossIcon("Vault Arena",    new Vector3( 6.0f, 7.8f, 12f), new Color(1f, 0.82f, 0.3f), MakeVaultIcon);
        }

        delegate void IconBuilder(Transform parent, Color glow);

        void BuildBossIcon(string name, Vector3 pos, Color glow, IconBuilder build)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.9f;
            build(go.transform, glow);
            var float_ = go.AddComponent<TitleBossIconGlow>();
            float_.phase = Random.value * 6f;
            float_.spin = Random.Range(10f, 24f) * (Random.value > 0.5f ? 1f : -1f);
        }

        void MakeRocketIcon(Transform parent, Color glow)
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Destroy(body.GetComponent<Collider>());
            body.transform.SetParent(parent, false);
            body.transform.localScale = new Vector3(0.6f, 1f, 0.6f);
            body.GetComponent<Renderer>().sharedMaterial = MakeEmissive(glow, 1.3f);
            var fin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(fin.GetComponent<Collider>());
            fin.transform.SetParent(parent, false);
            fin.transform.localPosition = new Vector3(0f, -0.8f, 0f);
            fin.transform.localScale = new Vector3(1.4f, 0.3f, 0.2f);
            fin.GetComponent<Renderer>().sharedMaterial = MakeEmissive(glow * 0.8f, 1.1f);
        }

        void MakeYachtIcon(Transform parent, Color glow)
        {
            var hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(hull.GetComponent<Collider>());
            hull.transform.SetParent(parent, false);
            hull.transform.localScale = new Vector3(1.8f, 0.4f, 0.7f);
            hull.GetComponent<Renderer>().sharedMaterial = MakeEmissive(glow, 1.2f);
            var sail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(sail.GetComponent<Collider>());
            sail.transform.SetParent(parent, false);
            sail.transform.localPosition = new Vector3(0.2f, 0.7f, 0f);
            sail.transform.localScale = new Vector3(0.6f, 1.0f, 0.05f);
            sail.GetComponent<Renderer>().sharedMaterial = MakeEmissive(Color.white, 1.0f);
        }

        void MakeServerIcon(Transform parent, Color glow)
        {
            var rack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(rack.GetComponent<Collider>());
            rack.transform.SetParent(parent, false);
            rack.transform.localScale = new Vector3(0.9f, 1.4f, 0.7f);
            rack.GetComponent<Renderer>().sharedMaterial = MakeEmissive(glow * 0.7f, 1.0f);
            for (int i = 0; i < 3; i++)
            {
                var led = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(led.GetComponent<Collider>());
                led.transform.SetParent(parent, false);
                led.transform.localPosition = new Vector3(0.3f, 0.45f - i * 0.45f, 0.4f);
                led.transform.localScale = new Vector3(0.15f, 0.1f, 0.05f);
                led.GetComponent<Renderer>().sharedMaterial = MakeEmissive(glow, 2.2f);
            }
        }

        void MakeVaultIcon(Transform parent, Color glow)
        {
            var door = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(door.GetComponent<Collider>());
            door.transform.SetParent(parent, false);
            door.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            door.transform.localScale = new Vector3(1.2f, 0.18f, 1.2f);
            door.GetComponent<Renderer>().sharedMaterial = MakeEmissive(glow, 1.4f);
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(handle.GetComponent<Collider>());
            handle.transform.SetParent(parent, false);
            handle.transform.localPosition = new Vector3(0f, 0f, -0.25f);
            handle.transform.localScale = new Vector3(0.9f, 0.12f, 0.12f);
            handle.GetComponent<Renderer>().sharedMaterial = MakeEmissive(Color.white, 1.2f);
        }

        void BuildClouds()
        {
            // drifting translucent cloud slabs high up to give parallax/depth
            for (int i = 0; i < 7; i++)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                c.name = "Cloud";
                Destroy(c.GetComponent<Collider>());
                c.transform.position = new Vector3(Random.Range(-16f, 16f), Random.Range(10f, 16f), Random.Range(8f, 20f));
                c.transform.localScale = new Vector3(Random.Range(5f, 9f), 0.6f, Random.Range(2f, 4f));
                var m = MakeEmissive(new Color(0.5f, 0.45f, 0.65f), 0.25f);
                m.color = new Color(0.6f, 0.55f, 0.7f, 0.22f);
                c.GetComponent<Renderer>().sharedMaterial = m;
                var drift = c.AddComponent<TitleCloudDrift>();
                drift.speed = Random.Range(0.3f, 0.9f) * (Random.value > 0.5f ? 1f : -1f);
                drift.span = 18f;
            }
        }

        void BuildCoinsAndConfetti()
        {
            var coinMat = MakeEmissive(new Color(0.95f, 0.78f, 0.25f), 0.9f);
            // raining + floating gold coins
            for (int i = 0; i < 24; i++)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                c.name = "Coin";
                Destroy(c.GetComponent<Collider>());
                c.transform.position = new Vector3(Random.Range(-7f, 7f), Random.Range(1f, 9f), Random.Range(-3f, 5f));
                c.transform.localScale = new Vector3(0.32f, 0.04f, 0.32f);
                c.GetComponent<Renderer>().sharedMaterial = coinMat;
                c.AddComponent<TitleCoinFloat>();
            }

            // colourful confetti that gently rains and respawns at the top
            Color[] conf =
            {
                new Color(1f, 0.3f, 0.4f), new Color(0.3f, 0.7f, 1f), new Color(0.4f, 1f, 0.5f),
                new Color(1f, 0.85f, 0.3f), new Color(0.85f, 0.4f, 1f),
            };
            for (int i = 0; i < 60; i++)
            {
                var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
                p.name = "Confetti";
                Destroy(p.GetComponent<Collider>());
                p.transform.position = new Vector3(Random.Range(-9f, 9f), Random.Range(2f, 12f), Random.Range(-4f, 6f));
                p.transform.localScale = new Vector3(0.14f, 0.14f, 0.02f);
                p.GetComponent<Renderer>().sharedMaterial = MakeEmissive(conf[i % conf.Length], 0.7f);
                var fall = p.AddComponent<TitleConfettiFall>();
                fall.fallSpeed = Random.Range(0.8f, 2.0f);
                fall.swayAmp = Random.Range(0.3f, 1.2f);
                fall.swaySpeed = Random.Range(0.6f, 2.0f);
                fall.spin = Random.Range(60f, 280f);
                fall.topY = 12f;
                fall.bottomY = 0.3f;
            }
        }

        // ── per-frame: live counter ticks ──────────────────────────────────────
        void Update()
        {
            liveTick += Time.unscaledDeltaTime;
            if (liveTick >= 0.9f)
            {
                liveTick = 0f;
                livePlayers += Random.Range(-7, 22); // jitter up, manufacture "live" feel
                if (livePlayers < 12000) livePlayers = 12000 + Random.Range(0, 400);
            }
            tickerScroll += Time.unscaledDeltaTime * 90f; // px/sec
        }

        // ── OVERLAY ─────────────────────────────────────────────────────────────
        void OnGUI()
        {
            EnsureStyles();
            float W = Screen.width, H = Screen.height;
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 3.2f);

            DrawTicker(W);

            // ── TITLE ── (sits on a dark cinematic scrim so it reads over the skyline)
            string title = "SIR  SPOONACCI";
            float tw = Mathf.Min(1100f, W - 80f); float th = 116f;
            float tx = (W - tw) * 0.5f; float ty = H * 0.11f;

            // letterbox scrim band behind the title block (dark → fades out)
            float scrimY = ty - 22f, scrimH = 230f;
            UiTheme.Accent(new Rect(0f, scrimY, W, scrimH), new Color(0f, 0f, 0f, 0.42f));
            UiTheme.Accent(new Rect(0f, scrimY, W, 2f), new Color(UiTheme.Gold.r, UiTheme.Gold.g, UiTheme.Gold.b, 0.25f));
            UiTheme.Accent(new Rect(0f, scrimY + scrimH - 2f, W, 2f), new Color(UiTheme.Gold.r, UiTheme.Gold.g, UiTheme.Gold.b, 0.25f));

            // big heavy gold title with a thick crisp outline (GTA-style)
            UiTheme.Outline(new Rect(tx, ty, tw, th), title, titleStyle, 3f, new Color(0f, 0f, 0f, 1f));
            // subtitle / tagline + social proof
            UiTheme.Label(new Rect(tx, ty + th - 8f, tw, 40f), Loc.T("title.subtitle"), subStyle);
            UiTheme.Label(new Rect(tx, ty + th + 30f, tw, 30f),
                "★★★★★   \"the funniest spoon game ever\"", starStyle);

            // ── NEW / SEASON 1 BADGE (top-left, glowing) ──
            DrawBadge(pulse);

            // ── LIVE PLAYERS + COUNTDOWN (under the title) ──
            DrawLiveAndCountdown(W, ty + 168f);

            // ── BUTTONS ──
            bool hasSave = System.IO.File.Exists(SaveSystem.Path);
            float bw = 440f, bh = 76f, gap = 18f;
            float bx = (W - bw) * 0.5f;
            float by = H * 0.50f;

            // pulsing primary CTA — grows + glows
            float s = 1f + 0.045f * pulse;
            float cw = bw * s, ch = bh * s;
            var ctaRect = new Rect((W - cw) * 0.5f, by - (ch - bh) * 0.5f, cw, ch);
            // glow halo behind the CTA
            UiTheme.Accent(new Rect(ctaRect.x - 6f, ctaRect.y - 6f, ctaRect.width + 12f, ctaRect.height + 12f),
                           new Color(1f, 0.8f, 0.3f, 0.10f + 0.10f * pulse));
            var ctaColor = ctaStyle.normal.textColor;
            ctaStyle.normal.textColor = Color.Lerp(UiTheme.GoldSoft, Color.white, pulse);
            if (UiTheme.Button(ctaRect, "▶  " + Loc.T("title.new_game"), ctaStyle))
                StartNew();
            ctaStyle.normal.textColor = ctaColor;

            float by2 = by + bh + gap + 8f;
            if (hasSave)
            {
                if (UiTheme.Button(new Rect(bx, by2, bw, bh), Loc.T("title.continue"), btnStyle))
                    Continue();
            }
            else
            {
                UiTheme.Card(new Rect(bx, by2, bw, bh));
                UiTheme.Label(new Rect(bx, by2, bw, bh), Loc.T("title.continue"), btnDisabledStyle);
            }

            float by3 = by2 + bh + gap;
            if (UiTheme.Button(new Rect(bx, by3, bw, bh), Loc.T("title.quit"), btnStyle))
                Quit();

            // ── MUSIC MUTE (top-right) ──
            bool muted = MusicPlayer.Instance != null && MusicPlayer.Instance.IsMuted;
            float mw = 200f, mh = 48f;
            if (UiTheme.Button(new Rect(W - mw - 24f, 24f, mw, mh),
                    muted ? Loc.T("ui.music_off") : Loc.T("ui.music_on"), muteStyle))
                MusicPlayer.Instance?.ToggleMute();

            // ── FOOTER ──
            string footer = hasSave
                ? Loc.T("title.save_found") + "  ·  " + Loc.T("title.tokens") + ": " + GameState.TrollTokens
                  + "  ·  " + Loc.T("title.missions") + ": " + MissionManager.CompletedIds.Count
                : Loc.T("title.no_save");
            GUI.Label(new Rect(20f, H - 36f, W - 40f, 24f), footer, footerStyle);
            var ver = new GUIStyle(footerStyle) { alignment = TextAnchor.MiddleRight };
            GUI.Label(new Rect(20f, H - 36f, W - 40f, 24f), "v0.6 dev build", ver);
        }

        void DrawTicker(float W)
        {
            float h = 34f;
            UiTheme.Accent(new Rect(0f, 0f, W, h), new Color(0.08f, 0.06f, 0.04f, 0.85f));
            UiTheme.Accent(new Rect(0f, h, W, 2f), new Color(UiTheme.Gold.r, UiTheme.Gold.g, UiTheme.Gold.b, 0.6f));

            // build one long string of messages and scroll it leftward, wrapping
            string joined = "";
            for (int i = 0; i < tickerMsgs.Length; i++)
                joined += tickerMsgs[i] + "        •        ";
            // measure rough width to know when to wrap
            float strW = tickerStyle.CalcSize(new GUIContent(joined)).x;
            if (strW < 1f) strW = W;
            float x = -(tickerScroll % strW);
            // draw twice so it loops seamlessly
            GUI.Label(new Rect(x, 1f, strW + W, h), joined, tickerStyle);
            GUI.Label(new Rect(x + strW, 1f, strW + W, h), joined, tickerStyle);
        }

        void DrawBadge(float pulse)
        {
            float bx = 24f, by = 48f, bw = 132f, bh = 60f;
            var r = new Rect(bx, by, bw, bh);
            // pulsing red-hot badge
            Color hot = Color.Lerp(new Color(0.85f, 0.15f, 0.2f), new Color(1f, 0.45f, 0.2f), pulse);
            UiTheme.Accent(new Rect(r.x - 4f, r.y - 4f, r.width + 8f, r.height + 8f), new Color(1f, 0.3f, 0.2f, 0.18f * pulse));
            UiTheme.Accent(r, hot);
            var badge = new GUIStyle(badgeStyle);
            GUI.Label(new Rect(r.x, r.y + 4f, r.width, 26f), "NEW", badge);
            var s1 = new GUIStyle(badgeStyle) { fontSize = UiScale.Font(15) };
            GUI.Label(new Rect(r.x, r.y + 30f, r.width, 22f), "SEASON 1", s1);
        }

        void DrawLiveAndCountdown(float W, float y)
        {
            // ● live players (left of centre) + season countdown (right of centre)
            float boxW = 300f, boxH = 40f;
            float lx = W * 0.5f - boxW - 12f;
            float rx = W * 0.5f + 12f;

            UiTheme.Card(new Rect(lx, y, boxW, boxH));
            // pulsing red dot
            float dotPulse = 0.4f + 0.6f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 2.4f));
            UiTheme.Accent(new Rect(lx + 16f, y + boxH * 0.5f - 6f, 12f, 12f), new Color(1f, 0.25f, 0.25f, dotPulse));
            var live = new GUIStyle(liveStyle) { alignment = TextAnchor.MiddleLeft };
            GUI.Label(new Rect(lx + 38f, y, boxW - 44f, boxH), livePlayers.ToString("N0") + " playing now", live);

            // countdown
            UiTheme.Card(new Rect(rx, y, boxW, boxH));
            float remain = Mathf.Max(0f, SeasonSeconds - (Time.time - sceneStart));
            int hh = (int)(remain / 3600f);
            int mm = (int)((remain % 3600f) / 60f);
            int ss = (int)(remain % 60f);
            var cd = new GUIStyle(countdownStyle) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(rx, y, boxW, boxH),
                string.Format("SEASON ENDS  {0:00}:{1:00}:{2:00}", hh, mm, ss), cd);
        }

        // ── styles ──────────────────────────────────────────────────────────────
        void EnsureStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label)
            { fontSize = UiScale.Font(86), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
              normal = { textColor = new Color(1f, 0.95f, 0.7f) } };
            subStyle = new GUIStyle(GUI.skin.label)
            { fontSize = UiScale.Font(26), fontStyle = FontStyle.Italic, alignment = TextAnchor.MiddleCenter,
              normal = { textColor = new Color(1f, 0.85f, 0.4f) } };
            starStyle = new GUIStyle(GUI.skin.label)
            { fontSize = UiScale.Font(18), fontStyle = FontStyle.Italic, alignment = TextAnchor.MiddleCenter,
              normal = { textColor = UiTheme.TextDone } };
            ctaStyle = new GUIStyle(GUI.skin.label)
            { fontSize = UiScale.Font(34), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
              normal = { textColor = UiTheme.GoldSoft } };
            btnStyle = new GUIStyle(GUI.skin.label)
            { fontSize = UiScale.Font(30), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
              normal = { textColor = UiTheme.TextMain } };
            btnDisabledStyle = new GUIStyle(GUI.skin.label)
            { fontSize = UiScale.Font(30), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
              normal = { textColor = new Color(0.55f, 0.55f, 0.6f, 0.7f) } };
            muteStyle = new GUIStyle(GUI.skin.label)
            { fontSize = UiScale.Font(20), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
              normal = { textColor = UiTheme.GoldSoft } };
            tickerStyle = new GUIStyle(GUI.skin.label)
            { fontSize = UiScale.Font(18), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft,
              normal = { textColor = UiTheme.GoldSoft } };
            badgeStyle = new GUIStyle(GUI.skin.label)
            { fontSize = UiScale.Font(22), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
              normal = { textColor = Color.white } };
            liveStyle = new GUIStyle(GUI.skin.label)
            { fontSize = UiScale.Font(18), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft,
              normal = { textColor = UiTheme.TextMain } };
            countdownStyle = new GUIStyle(GUI.skin.label)
            { fontSize = UiScale.Font(18), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
              normal = { textColor = UiTheme.TextWarn } };
            footerStyle = new GUIStyle(GUI.skin.label)
            { fontSize = UiScale.Font(16), normal = { textColor = new Color(0.95f, 0.95f, 0.95f, 0.7f) } };
        }

        // ── ACTIONS (wiring preserved exactly) ───────────────────────────────────
        void StartNew()
        {
            try
            {
                GameState.Reset();
                MissionManager.RehydrateCompleted(null);
                SoundFx.Instance.LevelUp();
                Debug.Log("[Title] Loading CutleryChamber...");
                SceneManager.LoadScene("CutleryChamber");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Title] StartNew failed: " + e.Message + " — scene 'CutleryChamber' likely not in Build Settings.");
                LoadIndexFallback(2); // CutleryChamber is index 2 per build settings
            }
        }

        void Continue()
        {
            try
            {
                SoundFx.Instance.Chime();
                string next = GameState.TunnelDug ? "SampleScene" : "CutleryChamber";
                Debug.Log("[Title] Continue → " + next);
                SceneManager.LoadScene(next);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Title] Continue failed: " + e.Message);
                LoadIndexFallback(GameState.TunnelDug ? 1 : 2);
            }
        }

        void LoadIndexFallback(int idx)
        {
            try { SceneManager.LoadScene(idx); }
            catch (System.Exception e) { Debug.LogError("[Title] Even index load failed: " + e.Message + " — check Build Profiles → Scenes In Build."); }
        }

        void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ── shared helpers ────────────────────────────────────────────────────
        static Material MakeEmissive(Color c, float intensity)
        {
            var m = new Material(ShaderCache.Lit) { color = c };
            m.SetColor("_BaseColor", c);
            m.SetFloat("_Metallic", 0.2f);
            m.SetFloat("_Smoothness", 0.6f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", c * intensity);
            return m;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helper MonoBehaviours — all UNIQUELY named (Title*) to avoid collisions.
    // ─────────────────────────────────────────────────────────────────────────

    // Slow cinematic orbit around the hero, with a gentle vertical bob.
    public class TitleCameraOrbit : MonoBehaviour
    {
        public Vector3 target = Vector3.zero;
        public float radius = 8f;
        public float height = 4f;
        public float degPerSec = 7f;
        public float bobAmp = 0.4f;
        float angle = 200f; // start slightly behind-left for a flattering 3/4 view

        void Update()
        {
            angle += degPerSec * Time.unscaledDeltaTime;
            float rad = angle * Mathf.Deg2Rad;
            float y = height + Mathf.Sin(Time.unscaledTime * 0.4f) * bobAmp;
            transform.position = new Vector3(Mathf.Sin(rad) * radius, y, Mathf.Cos(rad) * radius) + new Vector3(target.x, 0f, target.z);
            transform.LookAt(target);
        }
    }

    // Rotating searchlight pivot: yaws around with a fixed tilt so the emissive
    // beam cone sweeps the sky like a premium attract screen.
    public class TitleSearchlightSpin : MonoBehaviour
    {
        public float speed = 18f;
        public float tilt = 26f;
        public float phase;
        void Update()
        {
            float yaw = (Time.unscaledTime + phase) * speed;
            float wobble = Mathf.Sin((Time.unscaledTime + phase) * 0.7f) * 8f;
            transform.rotation = Quaternion.Euler(tilt + wobble, yaw, 0f);
        }
    }

    // Distant neon island skyline / boss-arena icon: bob + slow spin so the
    // glowing silhouettes feel alive in the dark.
    public class TitleBossIconGlow : MonoBehaviour
    {
        public float phase;
        public float spin = 16f;
        Vector3 origin;
        void Awake() { origin = transform.position; }
        void Update()
        {
            transform.position = origin + Vector3.up * Mathf.Sin(Time.unscaledTime * 0.8f + phase) * 0.4f;
            transform.Rotate(Vector3.up, spin * Time.unscaledDeltaTime, Space.World);
        }
    }

    // Translucent cloud slab drifting sideways, wrapping at the edges for parallax.
    public class TitleCloudDrift : MonoBehaviour
    {
        public float speed = 0.5f;
        public float span = 18f;
        float baseX;
        void Awake() { baseX = transform.position.x; }
        void Update()
        {
            var p = transform.position;
            p.x += speed * Time.unscaledDeltaTime;
            if (p.x > baseX + span) p.x = baseX - span;
            if (p.x < baseX - span) p.x = baseX + span;
            transform.position = p;
        }
    }

    // Confetti flake: falls, sways, spins, and respawns at the top — endless rain.
    public class TitleConfettiFall : MonoBehaviour
    {
        public float fallSpeed = 1.4f;
        public float swayAmp = 0.6f;
        public float swaySpeed = 1.2f;
        public float spin = 160f;
        public float topY = 12f;
        public float bottomY = 0.3f;
        float baseX, phase;
        Vector3 axis;
        void Awake()
        {
            baseX = transform.position.x;
            phase = Random.value * 10f;
            axis = Random.onUnitSphere;
        }
        void Update()
        {
            var p = transform.position;
            p.y -= fallSpeed * Time.unscaledDeltaTime;
            p.x = baseX + Mathf.Sin(Time.unscaledTime * swaySpeed + phase) * swayAmp;
            if (p.y < bottomY) { p.y = topY; }
            transform.position = p;
            transform.Rotate(axis, spin * Time.unscaledDeltaTime, Space.Self);
        }
    }

    // Floating gold coin (kept from the original title): gentle bob + spin.
    public class TitleCoinFloat : MonoBehaviour
    {
        float phase;
        Vector3 origin;
        void Awake() { origin = transform.position; phase = Random.value * 10f; transform.localRotation = Quaternion.Euler(Random.value * 90f, Random.value * 360f, Random.value * 90f); }
        void Update()
        {
            transform.position = origin + Vector3.up * Mathf.Sin(Time.unscaledTime + phase) * 0.35f;
            transform.Rotate(Vector3.up, 70f * Time.unscaledDeltaTime, Space.Self);
        }
    }
}
