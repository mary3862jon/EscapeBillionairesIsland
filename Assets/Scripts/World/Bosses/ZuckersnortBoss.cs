using System.Collections.Generic;
using UnityEngine;

namespace Spoonacci
{
    // ─────────────────────────────────────────────────────────────────────────
    //  ZuckersnortBoss — Mark Zuckersnort, the AI-lab keynote android.
    //
    //  A NORMAL boss (NOT a god-boss) → isGodBoss stays false, so per the POLICE
    //  RULE he goes passive when GameState.PoliceMode is on (the base CanEngage()
    //  handles that for us).
    //
    //  Phase 1 (Minions):     5 "selfie"/surveillance camera drones — a heavier
    //                         swarm than the other bosses — orbit and harass.
    //  Phase 2 (Gimmick):     Zuck runs his keynote. He periodically COMMANDS the
    //                         surviving drone swarm to DIVE-BOMB the player's
    //                         position, and between commands he emits an EXPANDING
    //                         HOLOGRAPHIC SHOCKWAVE RING (a growing emissive torus)
    //                         that the player must dodge. He stands stiff on his
    //                         dais clutching a tablet; bonk him (Q / LMB, in range,
    //                         facing) to drain CLOUT via the base DamageClout().
    //                         A clean bonk during the brief post-shockwave "buffer"
    //                         window lands a bonus hit.
    //  Phase 3 (Humiliation): his keynote CRASHES — the wall screens blue-screen,
    //                         his hologram collapses onto him, tablet dies, and a
    //                         "KEYNOTE CRASHED" gag floats overhead.
    // ─────────────────────────────────────────────────────────────────────────
    public class ZuckersnortBoss : BossFight
    {
        Transform _zuck;          // the figure (turns stiffly to track the player)
        Transform _tablet;        // glowing tablet (pulses while presenting)
        Material  _tabletMat;
        Material  _hoodieGlowMat; // the chest "AI" emblem
        Vector3   _daisCenter;

        readonly List<Material> _screenMats = new List<Material>();   // wall screens to blue-screen

        // gimmick timing
        float _nextShockAt;
        float _nextDiveAt;
        float _bufferUntil;       // post-shockwave window where a bonk gives bonus
        bool  _bonusPrimed;
        readonly List<HoloShockwave> _liveShocks = new List<HoloShockwave>();

        public ZuckersnortBoss()
        {
            bossName        = "Mark Zuckersnort";   // MUST match registry exactly
            isGodBoss       = false;                // NORMAL boss — passive in police mode
            tokenReward     = 90;
            arenaRadius     = 19f;
            maxClout        = 100f;
            bonkRange       = 3.3f;
            bonkCloutDamage = 11f;
        }

        // ── BuildBoss — pale hoodie android with a tablet, stiff posture ─────
        protected override void BuildBoss()
        {
            _daisCenter = transform.position;

            var skin    = ShaderCache.MakeMat(new Color(0.90f, 0.78f, 0.70f), 0f, 0.20f); // pale, matte
            var hoodie  = ShaderCache.MakeTextured(BossTextures.CarbonFiber, new Color(0.62f, 0.66f, 0.72f), 0.1f, 0.35f, new Vector2(2, 3));
            var hoodTrim= ShaderCache.MakeMat(new Color(0.50f, 0.54f, 0.60f), 0.2f, 0.45f);
            var jeans   = ShaderCache.MakeMat(new Color(0.20f, 0.24f, 0.34f), 0.1f, 0.4f);
            var shoes   = ShaderCache.MakeMat(new Color(0.92f, 0.92f, 0.95f), 0.1f, 0.6f); // white sneakers
            var hair    = ShaderCache.MakeMat(new Color(0.45f, 0.32f, 0.20f), 0f, 0.25f);

            // small keynote podium pedestal under the figure (on the dais)
            var podiumRoot = new GameObject("Zuck.Podium");
            podiumRoot.transform.SetParent(transform, false);
            podiumRoot.transform.localPosition = Vector3.zero;
            BuildKit.Cylinder("PodDrum", podiumRoot.transform, new Vector3(0f, 0.4f, 0f),
                new Vector3(1.5f, 0.4f, 1.5f), new Color(0.12f, 0.13f, 0.16f), 0.6f, 0.55f);
            var ringTop = BuildKit.Cylinder("PodRing", podiumRoot.transform, new Vector3(0f, 0.82f, 0f),
                new Vector3(1.7f, 0.05f, 1.7f), new Color(0.10f, 0.85f, 0.95f), 0f, 1f);
            ringTop.GetComponent<Renderer>().sharedMaterial.EnableKeyword("_EMISSION");
            ringTop.GetComponent<Renderer>().sharedMaterial.SetColor("_EmissionColor", new Color(0.1f, 0.85f, 0.95f) * 2.2f);
            Object.Destroy(ringTop.GetComponent<Collider>());

            // figure root on top of the pedestal (~y=0.9)
            var fig = new GameObject("Zuck.Figure");
            fig.transform.SetParent(transform, false);
            fig.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            _zuck = fig.transform;

            // sneakers + legs
            Shoe(fig.transform, new Vector3(-0.2f, 0.1f, 0.05f), shoes);
            Shoe(fig.transform, new Vector3( 0.2f, 0.1f, 0.05f), shoes);
            var legL = BuildKit.Cylinder("LegL", fig.transform, new Vector3(-0.2f, 0.55f, 0f), new Vector3(0.24f, 0.45f, 0.24f), default, 0, 0);
            legL.GetComponent<Renderer>().sharedMaterial = jeans; Object.Destroy(legL.GetComponent<Collider>());
            var legR = BuildKit.Cylinder("LegR", fig.transform, new Vector3( 0.2f, 0.55f, 0f), new Vector3(0.24f, 0.45f, 0.24f), default, 0, 0);
            legR.GetComponent<Renderer>().sharedMaterial = jeans; Object.Destroy(legR.GetComponent<Collider>());

            // torso (zip hoodie) — the bonk hit-collider lives here
            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso";
            torso.transform.SetParent(fig.transform, false);
            torso.transform.localPosition = new Vector3(0f, 1.32f, 0f);
            torso.transform.localScale = new Vector3(0.74f, 0.58f, 0.6f);
            torso.GetComponent<Renderer>().sharedMaterial = hoodie;
            bossBodyCollider = torso.GetComponent<Collider>();
            if (bossBodyCollider != null) bossBodyCollider.isTrigger = false;

            // hoodie zip + pocket trim
            var zip = BuildKit.Cube("Zip", fig.transform, new Vector3(0f, 1.32f, 0.30f), new Vector3(0.06f, 0.8f, 0.05f), default, 0, 0);
            zip.GetComponent<Renderer>().sharedMaterial = hoodTrim; Object.Destroy(zip.GetComponent<Collider>());
            var pocket = BuildKit.Cube("Pocket", fig.transform, new Vector3(0f, 1.02f, 0.28f), new Vector3(0.46f, 0.22f, 0.08f), default, 0, 0);
            pocket.GetComponent<Renderer>().sharedMaterial = hoodTrim; Object.Destroy(pocket.GetComponent<Collider>());

            // glowing "AI" emblem on the chest (cold teal)
            var emblem = BuildKit.Cube("AiEmblem", fig.transform, new Vector3(0f, 1.55f, 0.31f), new Vector3(0.22f, 0.22f, 0.04f), new Color(0.1f, 0.8f, 0.95f), 0f, 1f);
            Object.Destroy(emblem.GetComponent<Collider>());
            _hoodieGlowMat = emblem.GetComponent<Renderer>().sharedMaterial;
            _hoodieGlowMat.EnableKeyword("_EMISSION");
            _hoodieGlowMat.SetColor("_EmissionColor", new Color(0.15f, 0.9f, 1f) * 2.2f);

            // hood bunched at the back of the neck
            var hood = BuildKit.Sphere("Hood", fig.transform, new Vector3(0f, 1.78f, -0.16f), new Vector3(0.5f, 0.32f, 0.4f), default, 0, 0.3f);
            hood.GetComponent<Renderer>().sharedMaterial = hoodie;

            // stiff arms — both held forward holding the tablet (presenter pose)
            var armL = BuildKit.Cylinder("ArmL", fig.transform, new Vector3(-0.42f, 1.35f, 0.18f), new Vector3(0.18f, 0.4f, 0.18f), default, 0, 0);
            armL.transform.localRotation = Quaternion.Euler(70f, 0, 6f);
            armL.GetComponent<Renderer>().sharedMaterial = hoodie; Object.Destroy(armL.GetComponent<Collider>());
            var armR = BuildKit.Cylinder("ArmR", fig.transform, new Vector3(0.42f, 1.35f, 0.18f), new Vector3(0.18f, 0.4f, 0.18f), default, 0, 0);
            armR.transform.localRotation = Quaternion.Euler(70f, 0, -6f);
            armR.GetComponent<Renderer>().sharedMaterial = hoodie; Object.Destroy(armR.GetComponent<Collider>());
            BuildKit.Sphere("HandL", fig.transform, new Vector3(-0.34f, 1.12f, 0.48f), Vector3.one * 0.16f, new Color(0.9f, 0.78f, 0.7f), 0f, 0.2f);
            BuildKit.Sphere("HandR", fig.transform, new Vector3(0.34f, 1.12f, 0.48f), Vector3.one * 0.16f, new Color(0.9f, 0.78f, 0.7f), 0f, 0.2f);

            // head
            var head = BuildKit.Sphere("Head", fig.transform, new Vector3(0f, 1.95f, 0f), new Vector3(0.42f, 0.46f, 0.42f), default, 0, 0.2f);
            head.GetComponent<Renderer>().sharedMaterial = skin;
            // flat bowl-cut fringe
            var bowl = BuildKit.Sphere("Hair", fig.transform, new Vector3(0f, 2.08f, 0f), new Vector3(0.48f, 0.30f, 0.48f), default, 0, 0.25f);
            bowl.GetComponent<Renderer>().sharedMaterial = hair;
            var fringe = BuildKit.Cube("Fringe", fig.transform, new Vector3(0f, 1.98f, 0.22f), new Vector3(0.46f, 0.12f, 0.12f), default, 0, 0);
            fringe.GetComponent<Renderer>().sharedMaterial = hair; Object.Destroy(fringe.GetComponent<Collider>());
            // unblinking emissive android eyes
            EyeDot(fig.transform, new Vector3(-0.11f, 1.97f, 0.2f));
            EyeDot(fig.transform, new Vector3( 0.11f, 1.97f, 0.2f));

            // the glowing presentation tablet held out in both hands
            var tablet = BuildKit.Cube("KeynoteTablet", fig.transform, new Vector3(0f, 1.15f, 0.6f), new Vector3(0.5f, 0.03f, 0.34f), new Color(0.04f, 0.05f, 0.07f), 0.5f, 0.6f);
            tablet.transform.localRotation = Quaternion.Euler(60f, 0, 0);
            Object.Destroy(tablet.GetComponent<Collider>());
            _tablet = tablet.transform;
            var screen = BuildKit.Cube("TabletScreen", tablet.transform, new Vector3(0f, 0.7f, 0f), new Vector3(0.86f, 1f, 0.86f), default, 0, 0);
            Object.Destroy(screen.GetComponent<Collider>());
            _tabletMat = ShaderCache.MakeMat(new Color(0.1f, 0.7f, 1f), 0f, 1f);
            _tabletMat.EnableKeyword("_EMISSION");
            _tabletMat.SetColor("_EmissionColor", new Color(0.15f, 0.75f, 1f) * 1.8f);
            screen.GetComponent<Renderer>().sharedMaterial = _tabletMat;

            // floating satirical name
            BuildKit.Label(fig.gameObject, "Mark Zuckersnort", new Color(0.5f, 1f, 0.9f), 24, new Vector3(0f, 2.6f, 0f));

            // collect the arena's wall-screen faces so the keynote can blue-screen them.
            CollectScreens();

            _nextShockAt = Time.time + 3.5f;
            _nextDiveAt  = Time.time + 5.5f;
        }

        void Shoe(Transform parent, Vector3 pos, Material mat)
        {
            var s = BuildKit.Cube("Shoe", parent, pos, new Vector3(0.26f, 0.16f, 0.42f), default, 0, 0);
            s.GetComponent<Renderer>().sharedMaterial = mat;
            Object.Destroy(s.GetComponent<Collider>());
        }

        void EyeDot(Transform parent, Vector3 pos)
        {
            var e = BuildKit.Sphere("Eye", parent, pos, Vector3.one * 0.07f, new Color(0.2f, 0.95f, 1f), 0f, 1f);
            var m = e.GetComponent<Renderer>().sharedMaterial;
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", new Color(0.25f, 1f, 1f) * 2.4f);
        }

        // Find the emissive wall-screen faces built by ZuckLabArenaBuilder so the
        // humiliation can flip them to a blue-screen-of-death. Best-effort & robust.
        void CollectScreens()
        {
            _screenMats.Clear();
            var all = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
            foreach (var r in all)
            {
                if (r == null || r.gameObject.name != "ScreenFace") continue;
                if (r.sharedMaterial != null) _screenMats.Add(r.material); // instance to edit safely
            }
        }

        // ── SpawnMinions — 5 surveillance/"selfie" drones (heavier swarm) ────
        protected override void SpawnMinions()
        {
            for (int i = 0; i < 5; i++)
            {
                float ang = i / 5f * Mathf.PI * 2f;
                Vector3 p = _daisCenter + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * 6f + Vector3.up * 4.2f;
                SpawnDrone(p, 30f);
            }
        }

        // ── TickGimmick — shockwave rings to dodge + commanded dive-bombs ────
        protected override void TickGimmick(float dt)
        {
            float now = Time.time;

            // Zuck tracks the player with a stiff, robotic slerp (slower = stiffer)
            if (_zuck != null && player != null)
            {
                Vector3 look = player.position - _zuck.position; look.y = 0f;
                if (look.sqrMagnitude > 0.01f)
                    _zuck.rotation = Quaternion.Slerp(_zuck.rotation, Quaternion.LookRotation(look), dt * 1.6f);
            }

            // tablet + emblem presenting shimmer
            if (_tabletMat != null)
            {
                float s = 1.6f + Mathf.Sin(now * 5f) * 0.35f;
                _tabletMat.SetColor("_EmissionColor", new Color(0.15f, 0.75f, 1f) * s);
            }
            if (_hoodieGlowMat != null)
            {
                float s = 2.0f + Mathf.Sin(now * 3f) * 0.4f;
                _hoodieGlowMat.SetColor("_EmissionColor", new Color(0.15f, 0.9f, 1f) * s);
            }

            // prune dead shockwaves
            for (int i = _liveShocks.Count - 1; i >= 0; i--)
                if (_liveShocks[i] == null) _liveShocks.RemoveAt(i);

            // emit an expanding holographic shockwave ring on a cadence that
            // tightens as his clout drops (more desperate keynote)
            if (now >= _nextShockAt)
            {
                EmitShockwave();
                float gap = Mathf.Lerp(4.5f, 2.2f, 1f - CloutFrac);
                _nextShockAt = now + gap;
                // brief buffer window where a clean bonk lands a bonus
                _bufferUntil = now + 2.2f;
                _bonusPrimed = true;
            }

            // command the surviving drone swarm to dive-bomb the player's spot
            if (now >= _nextDiveAt)
            {
                CommandDiveBomb();
                float gap = Mathf.Lerp(6f, 3.2f, 1f - CloutFrac);
                _nextDiveAt = now + gap;
            }

            // bonus-clout during the buffer window (mirrors Magnus reload-bonk)
            if (_bonusPrimed && now <= _bufferUntil && player != null && bossBodyCollider != null)
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
                        _bonusPrimed = false;
                        DamageClout(bonkCloutDamage);
                    }
                }
            }
        }

        void EmitShockwave()
        {
            if (SoundFx.Instance != null) SoundFx.Instance.Swoosh();
            CameraShake.Shake(0.35f);
            Vector3 origin = bossBodyCollider != null
                ? new Vector3(bossBodyCollider.bounds.center.x, 0.6f, bossBodyCollider.bounds.center.z)
                : _daisCenter + Vector3.up * 0.6f;
            var go = new GameObject("Holo Shockwave");
            go.transform.position = origin;
            var w = go.AddComponent<HoloShockwave>();
            w.Build(arenaRadius);
            _liveShocks.Add(w);
        }

        void CommandDiveBomb()
        {
            if (player == null) return;
            if (SoundFx.Instance != null) SoundFx.Instance.Chime();
            // a flash on the tablet "issuing the command"
            if (_tabletMat != null) _tabletMat.SetColor("_EmissionColor", new Color(1f, 0.3f, 0.3f) * 2.5f);
            // re-arm any surviving drones into a brief dive at the player's spot
            var drones = Object.FindObjectsByType<BossDrone>(FindObjectsSortMode.None);
            foreach (var d in drones)
            {
                if (d == null || !d.Alive) continue;
                var dive = d.GetComponent<DroneDiveCommand>();
                if (dive == null) dive = d.gameObject.AddComponent<DroneDiveCommand>();
                dive.Trigger(player.position + Vector3.up * 1.2f);
            }
        }

        // ── DoHumiliation — keynote crashes: blue-screens + hologram collapse ─
        protected override void DoHumiliation()
        {
            if (SoundFx.Instance != null) { SoundFx.Instance.Ouch(); SoundFx.Instance.Sparkle(); }
            CameraShake.Shake(1.0f);

            // tablet + emblem die (presentation over)
            if (_tabletMat != null) _tabletMat.SetColor("_EmissionColor", Color.black);
            if (_hoodieGlowMat != null) _hoodieGlowMat.SetColor("_EmissionColor", Color.black);

            // blue-screen every wall screen we collected
            var bsod = new Color(0.05f, 0.18f, 0.85f);
            CollectScreens();
            foreach (var m in _screenMats)
            {
                if (m == null) continue;
                m.color = bsod;
                m.SetColor("_BaseColor", bsod);
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", bsod * 1.6f);
            }
            // also stop the live-feed flicker so the BSOD stays solid
            foreach (var f in Object.FindObjectsByType<ScreenFlicker>(FindObjectsSortMode.None))
                if (f != null) f.enabled = false;

            Vector3 headPos = bossBodyCollider != null
                ? bossBodyCollider.bounds.center + Vector3.up * 1.4f
                : transform.position + Vector3.up * 3f;

            // his hologram destabilises and COLLAPSES onto him
            var holoGo = new GameObject("Collapsing Hologram");
            holoGo.transform.position = headPos + Vector3.up * 5f;
            holoGo.AddComponent<CollapsingHologram>().Collapse(headPos.y + 0.3f);

            // sparks/glitch burst at his head + around the dais
            BonkBurst.Spawn(headPos);
            for (int i = 0; i < 5; i++)
                BonkBurst.Spawn(_daisCenter + new Vector3(Random.Range(-2f, 2f), Random.Range(0.4f, 2f), Random.Range(-2f, 2f)));

            // "KEYNOTE CRASHED" gag floating over him
            var gag = new GameObject("KeynoteCrashed");
            gag.transform.position = headPos + Vector3.up * 2.0f;
            var w = gag.AddComponent<WorldLabel>();
            w.text = "KEYNOTE CRASHED"; w.color = new Color(0.4f, 0.7f, 1f); w.fontSize = 30;

            // slump the android — system halt
            if (_zuck != null) _zuck.localRotation = Quaternion.Euler(14f, _zuck.localEulerAngles.y, 6f);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  HoloShockwave — an expanding emissive holographic ring (built from thin
    //  cylinder chords) that sweeps outward across the floor for the player to
    //  jump/dodge. Purely a spectacle telegraph (does no damage itself); it grows,
    //  fades, and self-destructs.
    // ─────────────────────────────────────────────────────────────────────────
    public class HoloShockwave : MonoBehaviour
    {
        readonly List<Transform> _segs = new List<Transform>();
        Material _mat;
        float _radius;
        float _maxRadius;
        float _speed = 7.5f;
        Color _tint = new Color(0.15f, 0.85f, 1f);

        public void Build(float maxRadius)
        {
            _maxRadius = Mathf.Max(4f, maxRadius);
            _radius = 1.2f;

            _mat = ShaderCache.MakeMat(_tint, 0f, 1f);
            _mat.EnableKeyword("_EMISSION");
            _mat.SetColor("_EmissionColor", _tint * 3.2f);

            // a generous ring of chords; they rescale as the radius grows
            const int seg = 40;
            for (int i = 0; i < seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                c.name = "ShockSeg";
                c.transform.SetParent(transform, false);
                Object.Destroy(c.GetComponent<Collider>());
                c.GetComponent<Renderer>().sharedMaterial = _mat;
                // store the angle in the name-free way: rotate now, position/scale in Update
                c.transform.localRotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 90f);
                _segs.Add(c.transform);
            }
            Layout();
        }

        void Layout()
        {
            int seg = _segs.Count;
            if (seg == 0) return;
            float chord = 2f * Mathf.PI * _radius / seg * 1.1f;
            for (int i = 0; i < seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                var t = _segs[i];
                if (t == null) continue;
                t.localPosition = new Vector3(Mathf.Cos(a) * _radius, 0f, Mathf.Sin(a) * _radius);
                t.localScale = new Vector3(0.16f, chord * 0.5f, 0.16f);
            }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            _radius += _speed * dt;
            Layout();

            // fade emission as it expands, then die
            float frac = Mathf.Clamp01(_radius / _maxRadius);
            if (_mat != null)
                _mat.SetColor("_EmissionColor", _tint * (3.2f * (1f - frac) + 0.2f));

            if (_radius >= _maxRadius)
                Destroy(gameObject);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  DroneDiveCommand — attached on demand to a BossDrone; for a short burst it
    //  swoops the drone down toward a commanded point (the player's last spot),
    //  then removes itself so the drone resumes its normal orbit. Cosmetic dive —
    //  it doesn't override the drone's hp/bonk logic, just nudges its transform.
    // ─────────────────────────────────────────────────────────────────────────
    public class DroneDiveCommand : MonoBehaviour
    {
        Vector3 _target;
        float _until;
        bool _active;

        public void Trigger(Vector3 target)
        {
            _target = target;
            _until = Time.time + 1.1f;
            _active = true;
            if (SoundFx.Instance != null) SoundFx.Instance.Swoosh();
        }

        void Update()
        {
            if (!_active) return;
            if (Time.time >= _until) { _active = false; Destroy(this); return; }
            // accelerate toward the commanded point, overriding the gentle orbit lerp
            transform.position = Vector3.MoveTowards(transform.position, _target, 16f * Time.deltaTime);
            // aggressive nose-down tilt during the dive
            Vector3 look = _target - transform.position;
            if (look.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), Time.deltaTime * 8f);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CollapsingHologram — the finisher prop. A glitchy emissive wireframe globe
    //  (the "metaverse" hologram) that destabilises, plummets onto the boss,
    //  flickers and shrinks away.
    // ─────────────────────────────────────────────────────────────────────────
    public class CollapsingHologram : MonoBehaviour
    {
        float _vel;
        float _restY;
        bool  _landed;
        float _killAt;
        Material _mat;
        readonly List<Transform> _rings = new List<Transform>();

        public void Collapse(float restY)
        {
            _restY = restY;
            _killAt = Time.time + 5f;

            _mat = ShaderCache.MakeMat(new Color(0.2f, 0.75f, 1f), 0f, 1f);
            _mat.EnableKeyword("_EMISSION");
            _mat.SetColor("_EmissionColor", new Color(0.25f, 0.8f, 1f) * 3f);

            // a wireframe-ish globe: 3 orthogonal rings of chords
            BuildRing(Vector3.zero);                                   // XZ plane
            BuildRing(new Vector3(90f, 0f, 0f));                       // XY plane
            BuildRing(new Vector3(0f, 0f, 90f));                       // YZ plane

            transform.localScale = Vector3.one;
        }

        void BuildRing(Vector3 euler)
        {
            var ring = new GameObject("HoloRing");
            ring.transform.SetParent(transform, false);
            ring.transform.localRotation = Quaternion.Euler(euler);
            _rings.Add(ring.transform);

            const int seg = 22;
            float radius = 1.6f;
            float chord = 2f * Mathf.PI * radius / seg * 1.1f;
            for (int i = 0; i < seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                c.name = "Seg";
                c.transform.SetParent(ring.transform, false);
                c.transform.localPosition = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
                c.transform.localRotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 90f);
                c.transform.localScale = new Vector3(0.07f, chord * 0.5f, 0.07f);
                Object.Destroy(c.GetComponent<Collider>());
                c.GetComponent<Renderer>().sharedMaterial = _mat;
            }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            // glitchy flicker throughout
            if (_mat != null)
            {
                float f = (Mathf.Sin(Time.time * 30f) > 0.2f) ? 3f : 0.6f;
                _mat.SetColor("_EmissionColor", new Color(0.25f, 0.8f, 1f) * f);
            }
            // spin the rings chaotically as it destabilises
            for (int i = 0; i < _rings.Count; i++)
                if (_rings[i] != null)
                    _rings[i].Rotate((i + 1) * 90f * dt, (i + 2) * 70f * dt, 0f, Space.Self);

            if (Time.time >= _killAt) { Destroy(gameObject); return; }

            if (!_landed)
            {
                _vel += 20f * dt;
                transform.position += Vector3.down * _vel * dt;
                if (transform.position.y <= _restY)
                {
                    var p = transform.position; p.y = _restY; transform.position = p;
                    _landed = true;
                    if (SoundFx.Instance != null) SoundFx.Instance.Bonk();
                    CameraShake.Shake(0.8f);
                    BonkBurst.Spawn(transform.position);
                }
            }
            else
            {
                // collapse: shrink away in the last second
                float remain = _killAt - Time.time;
                if (remain < 1f)
                    transform.localScale = Vector3.one * Mathf.Clamp01(remain);
            }
        }
    }
}
