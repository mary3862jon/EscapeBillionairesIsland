using System.Collections.Generic;
using UnityEngine;

namespace Spoonacci
{
    // ─────────────────────────────────────────────────────────────────────────
    //  BossFight — reusable 3-phase billionaire boss-fight state machine.
    //
    //  Concrete bosses (e.g. MagnusTuskBoss) subclass this and implement:
    //      BuildBoss()      – assemble the boss body + arena props (primitives).
    //      SpawnMinions()   – spawn the drone wave for phase 1 (use SpawnDrone()).
    //      TickGimmick(dt)  – per-frame phase-2 spectacle (rockets, beams, …).
    //      DoHumiliation()  – one-shot finisher gag when clout hits zero.
    //
    //  Engagement: the fight only runs while the player is inside arenaRadius AND
    //  CanEngage() is true. Non-god bosses go passive in police mode (so the cops
    //  are the threat then); the god boss always fights.
    //
    //  Phase flow:
    //      Idle ──player enters & CanEngage──▶ Minions
    //      Minions ──all drones down──▶ Gimmick
    //      Gimmick ──clout drained to 0 (player bonks the boss)──▶ Humiliation
    //      Humiliation ──short delay──▶ Defeated  (rewards + MarkBonked)
    // ─────────────────────────────────────────────────────────────────────────
    public abstract class BossFight : MonoBehaviour
    {
        public enum Phase { Idle, Minions, Gimmick, Humiliation, Defeated }

        // ---- tunables (set by subclass ctor / BuildBoss) --------------------
        public string bossName    = "Boss";
        public bool   isGodBoss   = false;
        public int    tokenReward = 60;
        public float  arenaRadius = 18f;
        public float  maxClout    = 100f;

        // ---- public read-only state -----------------------------------------
        public bool   Defeated     { get; private set; }
        public float  CloutFrac    => maxClout <= 0f ? 0f : Mathf.Clamp01(_clout / maxClout);
        public Phase  CurrentPhase { get; private set; } = Phase.Idle;
        public string PhaseLabel   { get; private set; } = "";

        // ---- internals -------------------------------------------------------
        protected Transform player;
        SpoonController _spoon;
        float _clout;
        readonly List<BossDrone> _drones = new List<BossDrone>();
        int _dronesDown;
        bool _humiliationStarted;
        float _humiliationDoneAt;
        bool _engagedOnce;
        BossCloutBar _bar;

        // bonk-on-boss detection (boss is not a Civilian, so we mirror the
        // manual-swing test from BonkAttack: Q / LMB pressed while in range & facing).
        protected Collider bossBodyCollider;   // optional: assign in BuildBoss for range origin
        public float bonkRange = 3.2f;
        public float bonkCloutDamage = 12f;
        float _bonkCooldown;

        // ---------------------------------------------------------------------
        protected virtual void Awake()
        {
            _spoon = Object.FindFirstObjectByType<SpoonController>();
            if (_spoon != null) player = _spoon.transform;
            _clout = maxClout;
            BuildBoss();
            PhaseLabel = "";
        }

        // Subclasses build their body + arena here.
        protected abstract void BuildBoss();
        // Phase 1: spawn the drone wave (call SpawnDrone() for each).
        protected abstract void SpawnMinions();
        // Phase 2: per-frame spectacle.
        protected abstract void TickGimmick(float dt);
        // Phase 3: one-shot finisher gag.
        protected abstract void DoHumiliation();

        // Whether this boss is allowed to fight right now.
        protected virtual bool CanEngage() => isGodBoss || !GameState.PoliceMode;

        // ---------------------------------------------------------------------
        protected virtual void Update()
        {
            if (Defeated) return;
            float dt = Time.deltaTime;

            bool playerNear = player != null &&
                              Vector3.Distance(transform.position, player.position) <= arenaRadius;
            bool engage = CanEngage() && playerNear;

            if (!engage)
            {
                // disengage: hide bar but keep progress
                if (_bar != null) _bar.gameObject.SetActive(false);
                if (CurrentPhase == Phase.Idle) return;
                // mid-fight walk-away just pauses; don't reset clout (feels fair)
                return;
            }

            // (re)show the clout bar pointed at us
            EnsureBar();
            if (_bar != null && !_bar.gameObject.activeSelf) _bar.gameObject.SetActive(true);

            if (!_engagedOnce)
            {
                _engagedOnce = true;
                EnterPhase(Phase.Minions);
            }

            switch (CurrentPhase)
            {
                case Phase.Minions:
                    // advance once every spawned drone is down
                    PruneDrones();
                    if (_drones.Count > 0 && _dronesDown >= _drones.Count)
                        EnterPhase(Phase.Gimmick);
                    break;

                case Phase.Gimmick:
                    SafeTickGimmick(dt);
                    DetectBonkOnBoss();
                    if (_clout <= 0f)
                        EnterPhase(Phase.Humiliation);
                    break;

                case Phase.Humiliation:
                    if (!_humiliationStarted)
                    {
                        _humiliationStarted = true;
                        SafeDoHumiliation();
                        _humiliationDoneAt = Time.time + 2.4f;
                    }
                    if (Time.time >= _humiliationDoneAt)
                        EnterPhase(Phase.Defeated);
                    break;
            }
        }

        void EnterPhase(Phase p)
        {
            CurrentPhase = p;
            switch (p)
            {
                case Phase.Minions:
                    PhaseLabel = Loc.T("boss.phase.minions");
                    _dronesDown = 0;
                    SafeSpawnMinions();
                    // if the subclass spawned nothing, skip straight to the gimmick
                    if (_drones.Count == 0) EnterPhase(Phase.Gimmick);
                    break;
                case Phase.Gimmick:
                    PhaseLabel = Loc.T("boss.phase.gimmick");
                    break;
                case Phase.Humiliation:
                    PhaseLabel = Loc.T("boss.phase.humiliation");
                    break;
                case Phase.Defeated:
                    PhaseLabel = Loc.T("boss.phase.defeated");
                    WinFight();
                    break;
            }
        }

        void WinFight()
        {
            Defeated = true;
            if (!string.IsNullOrEmpty(bossName))
                BillionaireRegistry.MarkBonked(bossName);
            GameState.TrollTokens += tokenReward;
            if (SoundFx.Instance != null) { SoundFx.Instance.LevelUp(); SoundFx.Instance.Sparkle(); }
            // victory flash on the bar, then it hides itself
            if (_bar != null) _bar.FlashVictory();
        }

        // ---- bonk → clout (mirrors BonkAttack.ManualSwing gating) ------------
        void DetectBonkOnBoss()
        {
            if (player == null) return;
            if (Time.time < _bonkCooldown) return;

            var kb = UnityEngine.InputSystem.Keyboard.current;
            var mouse = UnityEngine.InputSystem.Mouse.current;
            bool swung = (kb != null && kb.qKey.wasPressedThisFrame)
                      || (mouse != null && mouse.leftButton.wasPressedThisFrame);
            if (!swung) return;

            Vector3 origin = bossBodyCollider != null ? bossBodyCollider.bounds.center : transform.position;
            Vector3 to = origin - player.position; to.y = 0f;
            float d = to.magnitude;
            if (d > bonkRange) return;
            float dot = Vector3.Dot(player.forward, to.normalized);
            if (dot < 0.4f) return;   // must be roughly facing the boss

            _bonkCooldown = Time.time + 0.35f;
            DamageClout(bonkCloutDamage);
        }

        // Public so gimmicks (e.g. reflecting a rocket back) can also drain clout.
        public void DamageClout(float amount)
        {
            if (Defeated || amount <= 0f) return;
            _clout = Mathf.Max(0f, _clout - amount);
            if (SoundFx.Instance != null) { SoundFx.Instance.Bonk(); }
            CameraShake.Shake(0.6f);
            Vector3 burst = bossBodyCollider != null
                ? bossBodyCollider.bounds.center + Vector3.up * 0.5f
                : transform.position + Vector3.up * 2f;
            BonkBurst.Spawn(burst);
        }

        // ---- drone management -----------------------------------------------
        // Helper for subclasses: build + register a drone at a world position.
        protected BossDrone SpawnDrone(Vector3 pos, float hp = 30f)
        {
            var go = new GameObject(bossName + " Drone");
            go.transform.position = pos;
            var d = go.AddComponent<BossDrone>();
            d.Init(player, this, hp);
            _drones.Add(d);
            return d;
        }

        // Called by a drone when it dies.
        public void OnDroneDown(BossDrone d)
        {
            if (d == null) return;
            _dronesDown++;
        }

        void PruneDrones()
        {
            // count drones that have vanished without reporting (safety net)
            int alive = 0;
            foreach (var d in _drones) if (d != null && d.Alive) alive++;
            int gone = _drones.Count - alive;
            if (gone > _dronesDown) _dronesDown = gone;
        }

        // ---- safe wrappers (never throw every frame) ------------------------
        void SafeSpawnMinions() { try { SpawnMinions(); } catch (System.Exception e) { Debug.LogWarning("[BossFight] SpawnMinions: " + e.Message); } }
        void SafeTickGimmick(float dt) { try { TickGimmick(dt); } catch (System.Exception e) { Debug.LogWarning("[BossFight] TickGimmick: " + e.Message); } }
        void SafeDoHumiliation() { try { DoHumiliation(); } catch (System.Exception e) { Debug.LogWarning("[BossFight] DoHumiliation: " + e.Message); } }

        void EnsureBar()
        {
            if (_bar != null) return;
            var existing = Object.FindFirstObjectByType<BossCloutBar>();
            if (existing != null) _bar = existing;
            else
            {
                var go = new GameObject("BossCloutBar");
                _bar = go.AddComponent<BossCloutBar>();
            }
            _bar.Track(this);
        }
    }
}
