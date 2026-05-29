using System.Collections.Generic;
using UnityEngine;

namespace Spoonacci
{
    // ─────────────────────────────────────────────────────────────────────────
    //  MagnusTuskBoss — the GOD-BOSS rocket mogul.
    //
    //  Phase 1 (Minions):     4 attack drones guard the launch pad.
    //  Phase 2 (Gimmick):     Magnus periodically LAUNCHES a detailed multi-stage
    //                         rocket that arcs toward the player's position. The
    //                         player dodges; during each reload window Magnus is
    //                         exposed at his podium — bonk him (Q / LMB in range,
    //                         facing him) to drain his CLOUT via DamageClout().
    //                         Telegraphed by a pulsing red beacon + countdown beep.
    //  Phase 3 (Humiliation): his final rocket fizzles on the pad and drops a
    //                         giant Plastic Pete spoon on his head — confetti,
    //                         scorch, "STREAM ENDED" gag.
    //
    //  GOD-BOSS: isGodBoss = true → CanEngage() lets him fight even in police mode.
    //
    //  How DamageClout fires:
    //    1. Base BossFight.DetectBonkOnBoss() (runs every Gimmick frame) checks
    //       Q/LMB pressed + within bonkRange + facing dot >= 0.4 against
    //       bossBodyCollider (assigned here on Magnus's torso) → DamageClout(bonkCloutDamage).
    //    2. We additionally reward a *clean* bonk landed during the post-launch
    //       reload window with a bonus DamageClout() call (see TickGimmick).
    // ─────────────────────────────────────────────────────────────────────────
    public class MagnusTuskBoss : BossFight
    {
        Transform _magnus;          // the figure (rotates to face / taunt)
        Transform _tablet;          // glowing control tablet (pulses on launch)
        Transform _beacon;          // launch-warning beacon (telegraph)
        Material  _beaconMat;
        Material  _tabletMat;
        Vector3   _padCenter;       // where rockets spawn from

        float _nextLaunchAt;
        float _reloadUntil;         // window where a bonk gives bonus clout damage
        bool  _launchTelegraphed;
        float _telegraphUntil;
        float _beaconPhase;
        bool  _bonusBonkPrimed;     // one bonus per reload window

        readonly List<TuskRocket> _liveRockets = new List<TuskRocket>();

        public MagnusTuskBoss()
        {
            bossName        = "Magnus Tusk";
            isGodBoss       = true;
            tokenReward     = 120;
            arenaRadius     = 20f;
            maxClout        = 100f;
            bonkRange       = 3.4f;
            bonkCloutDamage = 11f;
        }

        // ── BuildBoss — detailed rocket-mogul figure on a command podium ─────
        protected override void BuildBoss()
        {
            _padCenter = transform.position;

            var skin    = ShaderCache.MakeMat(new Color(0.92f, 0.74f, 0.60f), 0f, 0.30f);
            var jacket  = ShaderCache.MakeTextured(BossTextures.BrushedMetal, new Color(0.34f, 0.30f, 0.26f), 0.25f, 0.45f, new Vector2(1, 1));
            var jacketTrim = ShaderCache.MakeMat(new Color(0.55f, 0.42f, 0.22f), 0.6f, 0.6f);
            var pants   = ShaderCache.MakeMat(new Color(0.10f, 0.10f, 0.12f), 0.1f, 0.4f);
            var boots   = ShaderCache.MakeMat(new Color(0.05f, 0.05f, 0.06f), 0.2f, 0.5f);
            var hair    = ShaderCache.MakeMat(new Color(0.30f, 0.18f, 0.08f), 0f, 0.25f);

            // command podium (textured concrete drum + metal collar + holo tablet)
            var podiumRoot = new GameObject("Magnus.Podium");
            podiumRoot.transform.SetParent(transform, false);
            podiumRoot.transform.localPosition = Vector3.zero;

            BuildKit.CubeTex("PodiumBase", podiumRoot.transform, new Vector3(0f, 0.25f, 0f),
                new Vector3(3.2f, 0.5f, 3.2f), BossTextures.Concrete, new Color(0.78f, 0.78f, 0.8f), 0.1f, 0.3f, new Vector2(2, 2));
            BuildKit.Cylinder("PodiumDrum", podiumRoot.transform, new Vector3(0f, 0.75f, 0f),
                new Vector3(2.0f, 0.5f, 2.0f), new Color(0.18f, 0.19f, 0.22f), 0.7f, 0.55f);
            var collar = BuildKit.Cylinder("PodiumCollar", podiumRoot.transform, new Vector3(0f, 1.1f, 0f),
                new Vector3(2.2f, 0.06f, 2.2f), new Color(0.6f, 0.48f, 0.18f), 1f, 0.85f);
            Object.Destroy(collar.GetComponent<Collider>());

            // figure root sits on the podium top (~y=1.3)
            var fig = new GameObject("Magnus.Figure");
            fig.transform.SetParent(transform, false);
            fig.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            _magnus = fig.transform;

            // boots + legs
            Boot(fig.transform, new Vector3(-0.22f, 0.12f, 0f), boots);
            Boot(fig.transform, new Vector3( 0.22f, 0.12f, 0f), boots);
            var legL = BuildKit.Cylinder("LegL", fig.transform, new Vector3(-0.22f, 0.55f, 0f), new Vector3(0.26f, 0.45f, 0.26f), default, 0, 0);
            legL.GetComponent<Renderer>().sharedMaterial = pants; Object.Destroy(legL.GetComponent<Collider>());
            var legR = BuildKit.Cylinder("LegR", fig.transform, new Vector3( 0.22f, 0.55f, 0f), new Vector3(0.26f, 0.45f, 0.26f), default, 0, 0);
            legR.GetComponent<Renderer>().sharedMaterial = pants; Object.Destroy(legR.GetComponent<Collider>());

            // torso (flight jacket) — brushed-metal sheen, slightly puffed
            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso";
            torso.transform.SetParent(fig.transform, false);
            torso.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            torso.transform.localScale = new Vector3(0.78f, 0.6f, 0.62f);
            torso.GetComponent<Renderer>().sharedMaterial = jacket;
            // give the torso the bonk hit-collider; everything else decorative
            bossBodyCollider = torso.GetComponent<Collider>();
            if (bossBodyCollider != null) bossBodyCollider.isTrigger = false;

            // jacket collar/zip trim
            var zip = BuildKit.Cube("Zip", fig.transform, new Vector3(0f, 1.35f, 0.30f), new Vector3(0.07f, 0.85f, 0.05f), default, 0, 0);
            zip.GetComponent<Renderer>().sharedMaterial = jacketTrim; Object.Destroy(zip.GetComponent<Collider>());
            var collarL = BuildKit.Cube("CollarL", fig.transform, new Vector3(-0.18f, 1.78f, 0.22f), new Vector3(0.26f, 0.12f, 0.18f), default, 0, 0);
            collarL.transform.localRotation = Quaternion.Euler(0, 0, 25f);
            collarL.GetComponent<Renderer>().sharedMaterial = jacketTrim; Object.Destroy(collarL.GetComponent<Collider>());
            var collarR = BuildKit.Cube("CollarR", fig.transform, new Vector3(0.18f, 1.78f, 0.22f), new Vector3(0.26f, 0.12f, 0.18f), default, 0, 0);
            collarR.transform.localRotation = Quaternion.Euler(0, 0, -25f);
            collarR.GetComponent<Renderer>().sharedMaterial = jacketTrim; Object.Destroy(collarR.GetComponent<Collider>());

            // arms (one cocked on hip — smug pose)
            var armL = BuildKit.Cylinder("ArmL", fig.transform, new Vector3(-0.5f, 1.4f, 0f), new Vector3(0.20f, 0.4f, 0.20f), default, 0, 0);
            armL.transform.localRotation = Quaternion.Euler(0, 0, 20f);
            armL.GetComponent<Renderer>().sharedMaterial = jacket; Object.Destroy(armL.GetComponent<Collider>());
            var armR = BuildKit.Cylinder("ArmR", fig.transform, new Vector3(0.5f, 1.45f, 0.05f), new Vector3(0.20f, 0.38f, 0.20f), default, 0, 0);
            armR.transform.localRotation = Quaternion.Euler(40f, 0, -28f);
            armR.GetComponent<Renderer>().sharedMaterial = jacket; Object.Destroy(armR.GetComponent<Collider>());
            // gloved hands
            var handL = BuildKit.Sphere("HandL", fig.transform, new Vector3(-0.66f, 1.12f, 0.04f), Vector3.one * 0.2f, new Color(0.05f, 0.05f, 0.06f), 0.2f, 0.5f);
            var handR = BuildKit.Sphere("HandR", fig.transform, new Vector3(0.58f, 1.2f, 0.28f), Vector3.one * 0.2f, new Color(0.05f, 0.05f, 0.06f), 0.2f, 0.5f);

            // head
            var head = BuildKit.Sphere("Head", fig.transform, new Vector3(0f, 2.0f, 0f), new Vector3(0.44f, 0.48f, 0.44f), default, 0, 0.3f);
            head.GetComponent<Renderer>().sharedMaterial = skin;
            // hair swoosh
            var hairGo = BuildKit.Sphere("Hair", fig.transform, new Vector3(0f, 2.16f, -0.04f), new Vector3(0.5f, 0.34f, 0.5f), default, 0, 0.25f);
            hairGo.GetComponent<Renderer>().sharedMaterial = hair;
            var fringe = BuildKit.Cube("Fringe", fig.transform, new Vector3(0f, 2.18f, 0.2f), new Vector3(0.5f, 0.1f, 0.18f), default, 0, 0);
            fringe.transform.localRotation = Quaternion.Euler(18f, 0, 0);
            fringe.GetComponent<Renderer>().sharedMaterial = hair; Object.Destroy(fringe.GetComponent<Collider>());

            // tinted aviator goggles (emissive amber lenses)
            var goggleBand = BuildKit.Cube("GoggleBand", fig.transform, new Vector3(0f, 2.05f, 0.0f), new Vector3(0.5f, 0.14f, 0.46f), new Color(0.04f, 0.04f, 0.05f), 0.3f, 0.4f);
            Object.Destroy(goggleBand.GetComponent<Collider>());
            var lensMat = ShaderCache.MakeMat(new Color(0.9f, 0.55f, 0.1f), 0f, 0.95f);
            lensMat.EnableKeyword("_EMISSION");
            lensMat.SetColor("_EmissionColor", new Color(1f, 0.55f, 0.1f) * 1.4f);
            var lensL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lensL.name = "LensL"; lensL.transform.SetParent(fig.transform, false);
            lensL.transform.localPosition = new Vector3(-0.13f, 2.06f, 0.2f); lensL.transform.localScale = new Vector3(0.16f, 0.13f, 0.06f);
            Object.Destroy(lensL.GetComponent<Collider>()); lensL.GetComponent<Renderer>().sharedMaterial = lensMat;
            var lensR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lensR.name = "LensR"; lensR.transform.SetParent(fig.transform, false);
            lensR.transform.localPosition = new Vector3(0.13f, 2.06f, 0.2f); lensR.transform.localScale = new Vector3(0.16f, 0.13f, 0.06f);
            Object.Destroy(lensR.GetComponent<Collider>()); lensR.GetComponent<Renderer>().sharedMaterial = lensMat;

            // glowing control tablet in his cocked hand
            var tablet = BuildKit.Cube("ControlTablet", fig.transform, new Vector3(0.6f, 1.25f, 0.42f), new Vector3(0.42f, 0.02f, 0.28f), new Color(0.04f, 0.05f, 0.07f), 0.5f, 0.6f);
            tablet.transform.localRotation = Quaternion.Euler(50f, 0, 0);
            Object.Destroy(tablet.GetComponent<Collider>());
            _tablet = tablet.transform;
            var screen = BuildKit.Cube("TabletScreen", tablet.transform, new Vector3(0f, 0.6f, 0f), new Vector3(0.86f, 1f, 0.86f), default, 0, 0);
            Object.Destroy(screen.GetComponent<Collider>());
            _tabletMat = ShaderCache.MakeMat(new Color(0.1f, 0.7f, 1f), 0f, 1f);
            _tabletMat.EnableKeyword("_EMISSION");
            _tabletMat.SetColor("_EmissionColor", new Color(0.2f, 0.8f, 1f) * 1.6f);
            screen.GetComponent<Renderer>().sharedMaterial = _tabletMat;

            // floating satirical name
            BuildKit.Label(fig.gameObject, "Magnus Tusk", new Color(1f, 0.6f, 0.1f), 24, new Vector3(0f, 2.7f, 0f));

            // launch-warning beacon on the podium collar (telegraph)
            var beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            beacon.name = "LaunchBeacon";
            beacon.transform.SetParent(podiumRoot.transform, false);
            beacon.transform.localPosition = new Vector3(0f, 1.3f, 1.0f);
            beacon.transform.localScale = Vector3.one * 0.3f;
            Object.Destroy(beacon.GetComponent<Collider>());
            _beaconMat = ShaderCache.MakeMat(new Color(0.4f, 0.05f, 0.05f), 0f, 1f);
            _beaconMat.EnableKeyword("_EMISSION");
            _beaconMat.SetColor("_EmissionColor", new Color(0.4f, 0.05f, 0.05f));
            beacon.GetComponent<Renderer>().sharedMaterial = _beaconMat;
            _beacon = beacon.transform;

            _nextLaunchAt = Time.time + 3.5f;
        }

        void Boot(Transform parent, Vector3 pos, Material mat)
        {
            var b = BuildKit.Cube("Boot", parent, pos, new Vector3(0.3f, 0.22f, 0.42f), default, 0, 0);
            b.GetComponent<Renderer>().sharedMaterial = mat;
            Object.Destroy(b.GetComponent<Collider>());
        }

        // ── SpawnMinions — 4 drones ringing the pad ─────────────────────────
        protected override void SpawnMinions()
        {
            for (int i = 0; i < 4; i++)
            {
                float ang = i / 4f * Mathf.PI * 2f;
                Vector3 p = _padCenter + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * 5f + Vector3.up * 4.5f;
                SpawnDrone(p, 34f);
            }
        }

        // ── TickGimmick — telegraphed rocket launches + bonk-during-reload ──
        protected override void TickGimmick(float dt)
        {
            // keep Magnus facing the player and taunt-bob
            if (_magnus != null && player != null)
            {
                Vector3 look = player.position - _magnus.position; look.y = 0f;
                if (look.sqrMagnitude > 0.01f)
                    _magnus.rotation = Quaternion.Slerp(_magnus.rotation, Quaternion.LookRotation(look), dt * 3f);
            }

            // tablet idle shimmer
            if (_tabletMat != null)
            {
                float s = 1.4f + Mathf.Sin(Time.time * 4f) * 0.3f;
                _tabletMat.SetColor("_EmissionColor", new Color(0.2f, 0.8f, 1f) * s);
            }

            // prune dead rockets
            for (int i = _liveRockets.Count - 1; i >= 0; i--)
                if (_liveRockets[i] == null) _liveRockets.RemoveAt(i);

            float now = Time.time;

            // telegraph window (1.6s) before each launch: beacon pulses red + beep
            if (!_launchTelegraphed && now >= _nextLaunchAt - 1.6f && now < _nextLaunchAt)
            {
                _launchTelegraphed = true;
                _telegraphUntil = _nextLaunchAt;
                if (SoundFx.Instance != null) SoundFx.Instance.Chime();
            }
            if (_launchTelegraphed && _beaconMat != null)
            {
                float blink = 0.5f + 0.5f * Mathf.Sin(now * 22f);
                _beaconMat.SetColor("_EmissionColor", new Color(1f, 0.1f, 0.05f) * (0.4f + blink * 3f));
                if (_beacon != null) _beacon.localScale = Vector3.one * (0.3f + blink * 0.12f);
            }

            // fire
            if (now >= _nextLaunchAt)
            {
                LaunchRocket();
                _launchTelegraphed = false;
                if (_beaconMat != null) _beaconMat.SetColor("_EmissionColor", new Color(0.4f, 0.05f, 0.05f));
                if (_beacon != null) _beacon.localScale = Vector3.one * 0.3f;
                // reload window: Magnus exposed; a clean bonk now lands a bonus
                _reloadUntil = now + 2.6f;
                _bonusBonkPrimed = true;
                // next launch sooner as his clout drops (gets desperate)
                float gap = Mathf.Lerp(5.5f, 2.8f, 1f - CloutFrac);
                _nextLaunchAt = now + gap;
            }

            // bonus-clout: during the reload window, a clean facing bonk in range
            // counts double. The base DetectBonkOnBoss() already drains the normal
            // amount; here we top it up for landing it while he's reloading.
            if (_bonusBonkPrimed && now <= _reloadUntil && player != null && bossBodyCollider != null)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                var mouse = UnityEngine.InputSystem.Mouse.current;
                bool swung = (kb != null && kb.qKey.wasPressedThisFrame)
                          || (mouse != null && mouse.leftButton.wasPressedThisFrame);
                if (swung)
                {
                    Vector3 origin = bossBodyCollider.bounds.center;
                    Vector3 to = origin - player.position; to.y = 0f;
                    if (to.magnitude <= bonkRange && Vector3.Dot(player.forward, to.normalized) >= 0.4f)
                    {
                        _bonusBonkPrimed = false;       // one bonus per window
                        DamageClout(bonkCloutDamage);   // counterattack reward
                    }
                }
            }
        }

        void LaunchRocket()
        {
            if (SoundFx.Instance != null) { SoundFx.Instance.Swoosh(); SoundFx.Instance.LevelUp(); }
            CameraShake.Shake(0.5f);

            Vector3 spawn = _padCenter + new Vector3(0f, 1.2f, 2.4f);
            var go = new GameObject("Tusk Rocket");
            go.transform.position = spawn;
            var r = go.AddComponent<TuskRocket>();
            Vector3 aim = player != null ? player.position : _padCenter + Vector3.forward * 10f;
            r.Build(aim);
            _liveRockets.Add(r);
        }

        // ── DoHumiliation — fizzled rocket drops a giant spoon on his head ──
        protected override void DoHumiliation()
        {
            if (SoundFx.Instance != null) { SoundFx.Instance.Ouch(); SoundFx.Instance.Sparkle(); }
            CameraShake.Shake(1.0f);

            // kill the beacon + tablet glow (stream over)
            if (_beaconMat != null) _beaconMat.SetColor("_EmissionColor", Color.black);
            if (_tabletMat != null) _tabletMat.SetColor("_EmissionColor", Color.black);

            Vector3 headPos = bossBodyCollider != null
                ? bossBodyCollider.bounds.center + Vector3.up * 1.6f
                : transform.position + Vector3.up * 4f;

            // a fizzled rocket puffs out over the pad
            BonkBurst.Spawn(_padCenter + new Vector3(0f, 1.5f, 2.4f));

            // GIANT Plastic Pete spoon falls onto Magnus
            var spoonGo = new GameObject("Plastic Pete (Giant)");
            spoonGo.transform.position = headPos + Vector3.up * 9f;
            spoonGo.AddComponent<GiantSpoonDrop>().Drop(headPos.y + 0.4f);

            // scorch ring on the pad
            var scorch = BuildKit.CubeTex("Scorch", transform, new Vector3(0f, 0.52f, 2.4f),
                new Vector3(2.6f, 0.04f, 2.6f), BossTextures.Tarmac, new Color(0.15f, 0.13f, 0.1f), 0.1f, 0.2f, new Vector2(1, 1));
            Object.Destroy(scorch.GetComponent<Collider>());

            // confetti / stars
            for (int i = 0; i < 5; i++)
                BonkBurst.Spawn(headPos + new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 1.5f), Random.Range(-1f, 1f)));

            // "STREAM ENDED" gag floating over him
            var gag = new GameObject("StreamEnded");
            gag.transform.position = headPos + Vector3.up * 2.2f;
            var w = gag.AddComponent<WorldLabel>();
            w.text = "STREAM ENDED"; w.color = new Color(1f, 0.2f, 0.2f); w.fontSize = 30;

            // slump the figure
            if (_magnus != null) _magnus.localRotation = Quaternion.Euler(18f, _magnus.localEulerAngles.y, 8f);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  TuskRocket — a cinematic multi-stage rocket that arcs toward a target
    //  and detonates. Tapered stages + nosecone + fins + engine bell + a big
    //  emissive exhaust plume and trailing smoke puffs.
    // ─────────────────────────────────────────────────────────────────────────
    public class TuskRocket : MonoBehaviour
    {
        Vector3 _vel;
        float _gravity = 9.0f;
        float _die;
        float _smokeNext;
        Transform _flame;
        Material _flameMat;
        bool _spent;

        public void Build(Vector3 target)
        {
            var hull   = ShaderCache.MakeTextured(BossTextures.BrushedMetal, new Color(0.92f, 0.92f, 0.94f), 0.6f, 0.7f, new Vector2(1, 2));
            var hull2  = ShaderCache.MakeMat(new Color(0.85f, 0.86f, 0.9f), 0.7f, 0.8f);
            var bandRed = ShaderCache.MakeMat(new Color(0.85f, 0.12f, 0.1f), 0.3f, 0.6f);
            var fins   = ShaderCache.MakeMat(new Color(0.2f, 0.22f, 0.26f), 0.7f, 0.6f);
            var bell   = ShaderCache.MakeMat(new Color(0.15f, 0.13f, 0.12f), 0.85f, 0.55f);

            // model points +Y as "up"/forward of travel; we orient transform to velocity.
            // lower (1st) stage — fat
            var s1 = BuildKit.Cylinder("Stage1", transform, new Vector3(0f, 0.7f, 0f), new Vector3(0.7f, 0.7f, 0.7f), default, 0, 0);
            s1.GetComponent<Renderer>().sharedMaterial = hull; Object.Destroy(s1.GetComponent<Collider>());
            // red band
            var band = BuildKit.Cylinder("Band", transform, new Vector3(0f, 1.35f, 0f), new Vector3(0.72f, 0.08f, 0.72f), default, 0, 0);
            band.GetComponent<Renderer>().sharedMaterial = bandRed; Object.Destroy(band.GetComponent<Collider>());
            // upper (2nd) stage — narrower, tapered interstage
            var inter = BuildKit.Cylinder("Interstage", transform, new Vector3(0f, 1.5f, 0f), new Vector3(0.6f, 0.12f, 0.6f), default, 0, 0);
            inter.GetComponent<Renderer>().sharedMaterial = hull2; Object.Destroy(inter.GetComponent<Collider>());
            var s2 = BuildKit.Cylinder("Stage2", transform, new Vector3(0f, 2.0f, 0f), new Vector3(0.5f, 0.45f, 0.5f), default, 0, 0);
            s2.GetComponent<Renderer>().sharedMaterial = hull2; Object.Destroy(s2.GetComponent<Collider>());

            // nosecone (capsule, scaled to a point)
            var nose = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            nose.name = "Nosecone"; nose.transform.SetParent(transform, false);
            nose.transform.localPosition = new Vector3(0f, 2.7f, 0f);
            nose.transform.localScale = new Vector3(0.5f, 0.35f, 0.5f);
            Object.Destroy(nose.GetComponent<Collider>());
            nose.GetComponent<Renderer>().sharedMaterial = bandRed;
            var tip = BuildKit.Sphere("Tip", transform, new Vector3(0f, 3.05f, 0f), new Vector3(0.5f, 0.5f, 0.5f), new Color(0.85f, 0.12f, 0.1f), 0.3f, 0.6f);
            tip.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);

            // 4 fins at the base
            for (int i = 0; i < 4; i++)
            {
                float ang = i / 4f * 360f;
                var fin = BuildKit.Cube("Fin" + i, transform, Vector3.zero, new Vector3(0.08f, 0.55f, 0.55f), default, 0, 0);
                fin.GetComponent<Renderer>().sharedMaterial = fins; Object.Destroy(fin.GetComponent<Collider>());
                fin.transform.localRotation = Quaternion.Euler(0f, ang, 0f);
                fin.transform.localPosition = Quaternion.Euler(0f, ang, 0f) * new Vector3(0.5f, 0.25f, 0f);
            }

            // engine bell
            var bellGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bellGo.name = "Bell"; bellGo.transform.SetParent(transform, false);
            bellGo.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            bellGo.transform.localScale = new Vector3(0.55f, 0.18f, 0.55f);
            Object.Destroy(bellGo.GetComponent<Collider>());
            bellGo.GetComponent<Renderer>().sharedMaterial = bell;

            // big emissive exhaust plume
            var flame = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            flame.name = "Exhaust"; flame.transform.SetParent(transform, false);
            flame.transform.localPosition = new Vector3(0f, -0.7f, 0f);
            flame.transform.localScale = new Vector3(0.45f, 0.7f, 0.45f);
            Object.Destroy(flame.GetComponent<Collider>());
            _flameMat = ShaderCache.MakeMat(new Color(1f, 0.6f, 0.1f), 0f, 1f);
            _flameMat.EnableKeyword("_EMISSION");
            _flameMat.SetColor("_EmissionColor", new Color(1f, 0.55f, 0.1f) * 4.5f);
            flame.GetComponent<Renderer>().sharedMaterial = _flameMat;
            _flame = flame.transform;

            // ballistic arc toward target: solve for a lofted trajectory
            Vector3 d = target - transform.position;
            Vector3 dFlat = new Vector3(d.x, 0f, d.z);
            float range = Mathf.Max(2f, dFlat.magnitude);
            float t = Mathf.Clamp(range / 16f, 1.0f, 2.6f);  // flight time
            Vector3 vFlat = dFlat / t;
            float vy = (d.y + 0.5f * _gravity * t * t) / t;
            _vel = vFlat + Vector3.up * vy;

            _die = Time.time + t + 0.4f;
            _smokeNext = 0f;

            // launch puff
            BonkBurst.Spawn(transform.position);
        }

        void Update()
        {
            if (_spent) return;
            float dt = Time.deltaTime;
            _vel += Vector3.down * _gravity * dt;
            transform.position += _vel * dt;

            // orient +Y (rocket up-axis) along velocity
            if (_vel.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.FromToRotation(Vector3.up, _vel.normalized);

            // flicker the plume
            if (_flameMat != null)
            {
                float f = 3.5f + Mathf.Sin(Time.time * 40f) * 1.2f;
                _flameMat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.1f) * f);
            }
            if (_flame != null)
                _flame.localScale = new Vector3(0.45f, 0.7f + Mathf.Sin(Time.time * 30f) * 0.18f, 0.45f);

            // trailing smoke puffs
            if (Time.time >= _smokeNext)
            {
                _smokeNext = Time.time + 0.08f;
                BonkBurst.Spawn(transform.position - _vel.normalized * 0.8f);
            }

            // detonate on ground or timeout
            if (transform.position.y <= 0.3f || Time.time >= _die)
                Detonate();
        }

        void Detonate()
        {
            if (_spent) return;
            _spent = true;
            if (SoundFx.Instance != null) SoundFx.Instance.Bonk();
            CameraShake.Shake(0.8f);
            Vector3 p = transform.position; p.y = Mathf.Max(0.3f, p.y);
            for (int i = 0; i < 4; i++)
                BonkBurst.Spawn(p + new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 1f), Random.Range(-1f, 1f)));
            Destroy(gameObject);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  GiantSpoonDrop — the Plastic Pete finisher prop. A huge spoon (bowl +
    //  handle) that plummets onto Magnus, bounces once, and lingers.
    // ─────────────────────────────────────────────────────────────────────────
    public class GiantSpoonDrop : MonoBehaviour
    {
        float _vel;
        float _restY;
        bool  _landed;
        float _killAt;

        public void Drop(float restY)
        {
            _restY = restY;
            _killAt = Time.time + 6f;

            var plastic = ShaderCache.MakeMat(new Color(0.95f, 0.95f, 0.98f), 0.1f, 0.85f);

            // bowl
            var bowl = BuildKit.Sphere("Bowl", transform, new Vector3(0f, 0f, 0f), new Vector3(2.2f, 1.1f, 2.6f), default, 0, 0);
            bowl.GetComponent<Renderer>().sharedMaterial = plastic;
            // handle
            var handle = BuildKit.Cylinder("Handle", transform, new Vector3(0f, 0f, -2.4f), new Vector3(0.45f, 1.9f, 0.45f), default, 0, 0);
            handle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            handle.GetComponent<Renderer>().sharedMaterial = plastic;
            Object.Destroy(handle.GetComponent<Collider>());

            // "PLASTIC PETE" stamp
            var w = new GameObject("PeteLabel");
            w.transform.SetParent(transform, false);
            w.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            var lbl = w.AddComponent<WorldLabel>();
            lbl.text = "PLASTIC PETE"; lbl.color = new Color(0.6f, 0.85f, 1f); lbl.fontSize = 20;

            transform.rotation = Quaternion.Euler(12f, 30f, 6f);
        }

        void Update()
        {
            if (Time.time >= _killAt) { Destroy(gameObject); return; }
            if (_landed) return;
            float dt = Time.deltaTime;
            _vel += 22f * dt;
            transform.position += Vector3.down * _vel * dt;
            transform.Rotate(0f, 60f * dt, 0f, Space.World);
            if (transform.position.y <= _restY)
            {
                var p = transform.position; p.y = _restY; transform.position = p;
                _landed = true;
                if (SoundFx.Instance != null) SoundFx.Instance.Bonk();
                CameraShake.Shake(0.9f);
                BonkBurst.Spawn(transform.position);
            }
        }
    }
}
