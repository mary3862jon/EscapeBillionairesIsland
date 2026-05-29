using System.Collections.Generic;
using UnityEngine;

namespace Spoonacci
{
    // ─────────────────────────────────────────────────────────────────────────
    //  CryptoChadBoss — the degen vault/casino bro. NORMAL boss (NOT a god-boss):
    //  goes passive in police mode so the cops become the threat instead.
    //
    //  Phase 1 (Minions):     3-4 'lambo' hover-drones patrol the vault.
    //  Phase 2 (Gimmick):     PUMP-AND-DUMP. Chad pumps the bags: a telegraphed
    //                         shower of GIANT emissive coins rains down around the
    //                         player (dodge them). Then the market CRASHES — a red
    //                         shockwave ring expands across the floor. During the
    //                         dump Chad is hyperventilating over his portfolio and
    //                         EXPOSED — bonk him (Q / LMB in range, facing him) to
    //                         drain his CLOUT via DamageClout(). A clean bonk landed
    //                         while the market is crashing lands a bonus hit.
    //  Phase 3 (Humiliation): his portfolio flatlines to ZERO, coins rain on his
    //                         head, the chart goes blood-red, "REKT" floats above.
    //
    //  Registry name MUST stay exactly "Crypto Chad" so the mission completes.
    // ─────────────────────────────────────────────────────────────────────────
    public class CryptoChadBoss : BossFight
    {
        Transform _chad;            // the bro figure (faces / panics)
        Transform _phone;           // glowing trading phone (pulses on pump)
        Material  _phoneMat;
        Material  _chainMat;        // gold chains (flash on dump)
        Transform _ticker;          // floating portfolio ticker
        Material  _tickerMat;
        WorldLabel _tickerLabel;
        Vector3   _center;          // arena centre (coin pile)

        float _nextPumpAt;
        bool  _pumpTelegraphed;
        float _pumpFireAt;
        float _crashUntil;          // window where a bonk lands a bonus
        bool  _crashBonusPrimed;
        float _phonePhase;
        int   _portfolio = 100;     // shown on the ticker, drains with clout

        readonly List<ChadCoin> _liveCoins = new List<ChadCoin>();
        readonly List<CrashWave> _liveWaves = new List<CrashWave>();

        public CryptoChadBoss()
        {
            bossName        = "Crypto Chad";   // EXACT registry name
            isGodBoss       = false;           // NORMAL boss — passive in police mode
            tokenReward     = 90;
            arenaRadius     = 19f;
            maxClout        = 100f;
            bonkRange       = 3.4f;
            bonkCloutDamage = 11f;
        }

        // ── BuildBoss — flashy bro in shades + gold chains on a pile of coins ─
        protected override void BuildBoss()
        {
            _center = transform.position;

            var skin    = ShaderCache.MakeMat(new Color(0.90f, 0.66f, 0.50f), 0f, 0.30f);
            var tank    = ShaderCache.MakeMat(new Color(0.92f, 0.92f, 0.95f), 0.05f, 0.55f); // white tank top
            var shorts  = ShaderCache.MakeMat(new Color(0.08f, 0.09f, 0.12f), 0.1f, 0.4f);
            var sneaker = ShaderCache.MakeMat(new Color(0.95f, 0.2f, 0.45f), 0.2f, 0.7f);
            var hair    = ShaderCache.MakeMat(new Color(0.16f, 0.10f, 0.05f), 0f, 0.35f);
            var gold    = ShaderCache.MakeMat(new Color(0.95f, 0.74f, 0.18f), 1f, 0.85f);
            gold.EnableKeyword("_EMISSION");
            gold.SetColor("_EmissionColor", new Color(0.85f, 0.6f, 0.12f) * 1.2f);
            _chainMat = gold;

            // pile of glowing coins he stands on
            BuildCoinPile(transform);

            // figure root sits atop the coin pile (~y=1.2)
            var fig = new GameObject("Chad.Figure");
            fig.transform.SetParent(transform, false);
            fig.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            _chad = fig.transform;

            // sneakers + legs
            Sneaker(fig.transform, new Vector3(-0.22f, 0.12f, 0.04f), sneaker);
            Sneaker(fig.transform, new Vector3( 0.22f, 0.12f, 0.04f), sneaker);
            var legL = BuildKit.Cylinder("LegL", fig.transform, new Vector3(-0.22f, 0.55f, 0f), new Vector3(0.28f, 0.45f, 0.28f), default, 0, 0);
            legL.GetComponent<Renderer>().sharedMaterial = shorts; Object.Destroy(legL.GetComponent<Collider>());
            var legR = BuildKit.Cylinder("LegR", fig.transform, new Vector3( 0.22f, 0.55f, 0f), new Vector3(0.28f, 0.45f, 0.28f), default, 0, 0);
            legR.GetComponent<Renderer>().sharedMaterial = shorts; Object.Destroy(legR.GetComponent<Collider>());

            // torso (tank top, swole) — gets the bonk hit-collider
            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso";
            torso.transform.SetParent(fig.transform, false);
            torso.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            torso.transform.localScale = new Vector3(0.92f, 0.62f, 0.66f);   // gym-bro chest
            torso.GetComponent<Renderer>().sharedMaterial = tank;
            bossBodyCollider = torso.GetComponent<Collider>();
            if (bossBodyCollider != null) bossBodyCollider.isTrigger = false;

            // arms (one flexing a phone, one fist-up)
            var armL = BuildKit.Cylinder("ArmL", fig.transform, new Vector3(-0.56f, 1.45f, 0f), new Vector3(0.24f, 0.4f, 0.24f), default, 0, 0);
            armL.transform.localRotation = Quaternion.Euler(0, 0, 28f);
            armL.GetComponent<Renderer>().sharedMaterial = skin; Object.Destroy(armL.GetComponent<Collider>());
            var armR = BuildKit.Cylinder("ArmR", fig.transform, new Vector3(0.5f, 1.5f, 0.18f), new Vector3(0.24f, 0.38f, 0.24f), default, 0, 0);
            armR.transform.localRotation = Quaternion.Euler(58f, 0, -22f);
            armR.GetComponent<Renderer>().sharedMaterial = skin; Object.Destroy(armR.GetComponent<Collider>());
            var fistL = BuildKit.Sphere("FistL", fig.transform, new Vector3(-0.74f, 1.92f, 0.02f), Vector3.one * 0.22f, new Color(0.90f, 0.66f, 0.50f), 0f, 0.3f);
            var handR = BuildKit.Sphere("HandR", fig.transform, new Vector3(0.62f, 1.28f, 0.46f), Vector3.one * 0.22f, new Color(0.90f, 0.66f, 0.50f), 0f, 0.3f);

            // gold chains layered on the chest
            for (int i = 0; i < 3; i++)
            {
                var chain = BuildKit.Cylinder("Chain" + i, fig.transform,
                    new Vector3(0f, 1.62f - i * 0.12f, 0.30f),
                    new Vector3(0.5f - i * 0.05f, 0.03f, 0.5f - i * 0.05f), default, 0, 0);
                chain.transform.localRotation = Quaternion.Euler(78f, 0f, 0f);
                chain.GetComponent<Renderer>().sharedMaterial = _chainMat;
                Object.Destroy(chain.GetComponent<Collider>());
            }
            // big $ medallion
            var medal = BuildKit.Cylinder("Medallion", fig.transform, new Vector3(0f, 1.32f, 0.42f),
                new Vector3(0.34f, 0.04f, 0.34f), default, 0, 0);
            medal.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            medal.GetComponent<Renderer>().sharedMaterial = _chainMat;
            Object.Destroy(medal.GetComponent<Collider>());
            var dollar = BuildKit.Cube("DollarSym", fig.transform, new Vector3(0f, 1.32f, 0.47f),
                new Vector3(0.07f, 0.28f, 0.04f), new Color(0.1f, 0.9f, 0.4f), 0.3f, 0.9f);
            { var m = dollar.GetComponent<Renderer>().sharedMaterial; m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", new Color(0.1f, 0.9f, 0.4f) * 1.6f); }
            Object.Destroy(dollar.GetComponent<Collider>());

            // head
            var head = BuildKit.Sphere("Head", fig.transform, new Vector3(0f, 2.06f, 0f), new Vector3(0.46f, 0.5f, 0.46f), default, 0, 0.3f);
            head.GetComponent<Renderer>().sharedMaterial = skin;
            // slicked-back hair + man-bun
            var hairGo = BuildKit.Sphere("Hair", fig.transform, new Vector3(0f, 2.2f, -0.06f), new Vector3(0.5f, 0.36f, 0.5f), default, 0, 0.3f);
            hairGo.GetComponent<Renderer>().sharedMaterial = hair;
            var bun = BuildKit.Sphere("ManBun", fig.transform, new Vector3(0f, 2.3f, -0.22f), Vector3.one * 0.22f, default, 0, 0.3f);
            bun.GetComponent<Renderer>().sharedMaterial = hair;
            // stubble jaw shadow
            var jaw = BuildKit.Sphere("Jaw", fig.transform, new Vector3(0f, 1.92f, 0.06f), new Vector3(0.44f, 0.28f, 0.42f), new Color(0.55f, 0.42f, 0.34f), 0f, 0.25f);
            Object.Destroy(jaw.GetComponent<Collider>());

            // mirrored aviator sunglasses (emissive cyan lenses)
            var glassBand = BuildKit.Cube("ShadeBand", fig.transform, new Vector3(0f, 2.1f, 0f), new Vector3(0.52f, 0.12f, 0.46f), new Color(0.03f, 0.03f, 0.04f), 0.4f, 0.5f);
            Object.Destroy(glassBand.GetComponent<Collider>());
            var lensMat = ShaderCache.MakeMat(new Color(0.1f, 0.85f, 0.9f), 0.6f, 0.95f);
            lensMat.EnableKeyword("_EMISSION");
            lensMat.SetColor("_EmissionColor", new Color(0.1f, 0.9f, 0.95f) * 1.5f);
            var lensL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lensL.name = "LensL"; lensL.transform.SetParent(fig.transform, false);
            lensL.transform.localPosition = new Vector3(-0.14f, 2.11f, 0.21f); lensL.transform.localScale = new Vector3(0.17f, 0.13f, 0.06f);
            Object.Destroy(lensL.GetComponent<Collider>()); lensL.GetComponent<Renderer>().sharedMaterial = lensMat;
            var lensR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lensR.name = "LensR"; lensR.transform.SetParent(fig.transform, false);
            lensR.transform.localPosition = new Vector3(0.14f, 2.11f, 0.21f); lensR.transform.localScale = new Vector3(0.17f, 0.13f, 0.06f);
            Object.Destroy(lensR.GetComponent<Collider>()); lensR.GetComponent<Renderer>().sharedMaterial = lensMat;

            // glowing trading phone in his raised right hand
            var phone = BuildKit.Cube("TradingPhone", fig.transform, new Vector3(0.66f, 1.32f, 0.5f),
                new Vector3(0.26f, 0.5f, 0.04f), new Color(0.03f, 0.04f, 0.06f), 0.5f, 0.7f);
            phone.transform.localRotation = Quaternion.Euler(20f, -20f, 8f);
            Object.Destroy(phone.GetComponent<Collider>());
            _phone = phone.transform;
            var screen = BuildKit.Cube("PhoneScreen", phone.transform, new Vector3(0f, 0f, 0.6f),
                new Vector3(0.82f, 0.88f, 0.4f), default, 0, 0);
            Object.Destroy(screen.GetComponent<Collider>());
            _phoneMat = ShaderCache.MakeMat(new Color(0.1f, 0.85f, 0.4f), 0f, 1f);
            _phoneMat.EnableKeyword("_EMISSION");
            _phoneMat.SetColor("_EmissionColor", new Color(0.1f, 0.9f, 0.4f) * 1.8f);
            screen.GetComponent<Renderer>().sharedMaterial = _phoneMat;

            // floating satirical name
            BuildKit.Label(fig.gameObject, "Crypto Chad", new Color(0.3f, 0.95f, 0.9f), 24, new Vector3(0f, 2.8f, 0f));

            // floating portfolio ticker over his head
            var tickGo = new GameObject("PortfolioTicker");
            tickGo.transform.SetParent(fig.transform, false);
            tickGo.transform.localPosition = new Vector3(0f, 3.2f, 0f);
            _ticker = tickGo.transform;
            _tickerLabel = tickGo.AddComponent<WorldLabel>();
            _tickerLabel.text = "PORTFOLIO $100M"; _tickerLabel.color = new Color(0.2f, 1f, 0.4f); _tickerLabel.fontSize = 20;
            // a glowing backing slab behind the ticker
            var tickBack = BuildKit.Cube("TickerBack", tickGo.transform, new Vector3(0f, 0f, 0.1f),
                new Vector3(2.6f, 0.7f, 0.08f), new Color(0.04f, 0.06f, 0.05f), 0.3f, 0.8f);
            Object.Destroy(tickBack.GetComponent<Collider>());
            _tickerMat = tickBack.GetComponent<Renderer>().sharedMaterial;
            _tickerMat.EnableKeyword("_EMISSION");
            _tickerMat.SetColor("_EmissionColor", new Color(0.1f, 0.6f, 0.25f) * 1.2f);

            _nextPumpAt = Time.time + 3.5f;
        }

        void BuildCoinPile(Transform parent)
        {
            var goldMat = ShaderCache.MakeMat(new Color(0.95f, 0.74f, 0.18f), 1f, 0.85f);
            goldMat.EnableKeyword("_EMISSION");
            goldMat.SetColor("_EmissionColor", new Color(0.85f, 0.6f, 0.12f) * 1.0f);
            var goldDeep = ShaderCache.MakeMat(new Color(0.72f, 0.50f, 0.08f), 1f, 0.7f);

            // a low mound of coins (random tilted cylinders) under the figure
            var pile = new GameObject("Chad.CoinPile");
            pile.transform.SetParent(parent, false);
            pile.transform.localPosition = Vector3.zero;

            int n = 26;
            for (int i = 0; i < n; i++)
            {
                float a = Random.value * Mathf.PI * 2f;
                float rad = Random.Range(0f, 2.3f);
                float y = 0.18f + Mathf.Max(0f, 1.0f - rad * 0.35f) * Random.Range(0.3f, 1.0f);
                var coin = BuildKit.Cylinder("PileCoin" + i, pile.transform,
                    new Vector3(Mathf.Cos(a) * rad, y, Mathf.Sin(a) * rad),
                    new Vector3(0.9f, 0.06f, 0.9f), default, 0, 0);
                coin.GetComponent<Renderer>().sharedMaterial = (i % 2 == 0) ? goldMat : goldDeep;
                coin.transform.localRotation = Quaternion.Euler(Random.Range(-35f, 35f), Random.Range(0f, 360f), Random.Range(-35f, 35f));
                Object.Destroy(coin.GetComponent<Collider>());
            }
            // a stout supporting drum so the figure isn't floating on gaps
            var drum = BuildKit.Cylinder("PileCore", pile.transform, new Vector3(0f, 0.55f, 0f),
                new Vector3(2.0f, 0.55f, 2.0f), new Color(0.6f, 0.46f, 0.12f), 1f, 0.6f);
            drum.GetComponent<Renderer>().sharedMaterial = goldDeep;
            Object.Destroy(drum.GetComponent<Collider>());
        }

        void Sneaker(Transform parent, Vector3 pos, Material mat)
        {
            var s = BuildKit.Cube("Sneaker", parent, pos, new Vector3(0.32f, 0.22f, 0.5f), default, 0, 0);
            s.GetComponent<Renderer>().sharedMaterial = mat;
            Object.Destroy(s.GetComponent<Collider>());
            // glowing sole accent
            var sole = BuildKit.Cube("Sole", parent, pos + new Vector3(0f, -0.1f, 0f), new Vector3(0.34f, 0.06f, 0.52f), new Color(0.2f, 0.95f, 1f), 0.2f, 0.9f);
            var m = sole.GetComponent<Renderer>().sharedMaterial; m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", new Color(0.2f, 0.95f, 1f) * 1.4f);
            Object.Destroy(sole.GetComponent<Collider>());
        }

        // ── SpawnMinions — 4 'lambo' hover-drones patrolling the vault ───────
        protected override void SpawnMinions()
        {
            for (int i = 0; i < 4; i++)
            {
                float ang = i / 4f * Mathf.PI * 2f;
                Vector3 p = _center + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * 5.5f + Vector3.up * 4.2f;
                SpawnDrone(p, 32f);
            }
        }

        // ── TickGimmick — PUMP-AND-DUMP: coin rain → red crash shockwave ─────
        protected override void TickGimmick(float dt)
        {
            // keep Chad facing the player and taunt-bob
            if (_chad != null && player != null)
            {
                Vector3 look = player.position - _chad.position; look.y = 0f;
                if (look.sqrMagnitude > 0.01f)
                    _chad.rotation = Quaternion.Slerp(_chad.rotation, Quaternion.LookRotation(look), dt * 3f);
            }

            // phone idle shimmer (green = pumping)
            if (_phoneMat != null)
            {
                float s = 1.6f + Mathf.Sin(Time.time * 5f) * 0.4f;
                _phoneMat.SetColor("_EmissionColor", new Color(0.1f, 0.9f, 0.4f) * s);
            }

            // ticker tracks remaining clout (his portfolio bleeds as you bonk him)
            UpdateTicker();

            // prune dead props
            for (int i = _liveCoins.Count - 1; i >= 0; i--) if (_liveCoins[i] == null) _liveCoins.RemoveAt(i);
            for (int i = _liveWaves.Count - 1; i >= 0; i--) if (_liveWaves[i] == null) _liveWaves.RemoveAt(i);

            float now = Time.time;

            // telegraph window (1.6s): phone flares, chains flash gold, beep
            if (!_pumpTelegraphed && now >= _nextPumpAt - 1.6f && now < _nextPumpAt)
            {
                _pumpTelegraphed = true;
                _pumpFireAt = _nextPumpAt;
                if (SoundFx.Instance != null) SoundFx.Instance.Chime();
            }
            if (_pumpTelegraphed)
            {
                float blink = 0.5f + 0.5f * Mathf.Sin(now * 20f);
                if (_phoneMat != null) _phoneMat.SetColor("_EmissionColor", new Color(0.2f, 1f, 0.3f) * (1f + blink * 2.5f));
                if (_chainMat != null) _chainMat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.15f) * (1f + blink * 1.6f));
            }

            // FIRE the pump: rain a ring of giant coins around the player, then
            // schedule the crash shockwave a beat later.
            if (now >= _nextPumpAt)
            {
                PumpCoins();
                _pumpTelegraphed = false;
                if (_chainMat != null) _chainMat.SetColor("_EmissionColor", new Color(0.85f, 0.6f, 0.12f) * 1.2f);

                // the dump/crash window: Chad panics + exposed; a clean bonk now = bonus
                _crashUntil = now + 2.8f;
                _crashBonusPrimed = true;
                // crash shockwave fires shortly after the coins land
                Invoke(nameof(MarketCrash), 1.1f);

                // gets more desperate as his portfolio (clout) drops
                float gap = Mathf.Lerp(5.5f, 3.0f, 1f - CloutFrac);
                _nextPumpAt = now + gap;
            }

            // bonus-clout during the crash window: a clean facing bonk in range.
            // Base DetectBonkOnBoss() already drained the normal amount; this tops
            // it up for "shorting the top" while the market dumps.
            if (_crashBonusPrimed && now <= _crashUntil && player != null && bossBodyCollider != null)
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
                        _crashBonusPrimed = false;       // one bonus per window
                        DamageClout(bonkCloutDamage);
                    }
                }
            }
        }

        void UpdateTicker()
        {
            if (_tickerLabel == null) return;
            int val = Mathf.RoundToInt(CloutFrac * 100f);
            if (val != _portfolio)
            {
                _portfolio = val;
                bool up = val >= 50;
                _tickerLabel.text = "PORTFOLIO $" + val + "M";
                _tickerLabel.color = up ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.4f, 0.25f);
                if (_tickerMat != null)
                    _tickerMat.SetColor("_EmissionColor",
                        (up ? new Color(0.1f, 0.6f, 0.25f) : new Color(0.6f, 0.18f, 0.1f)) * 1.2f);
            }
        }

        // rain a ring of giant coins down around the player's position
        void PumpCoins()
        {
            if (SoundFx.Instance != null) { SoundFx.Instance.LevelUp(); SoundFx.Instance.Sparkle(); }
            CameraShake.Shake(0.4f);

            Vector3 focus = player != null ? player.position : _center;
            int count = 6;
            for (int i = 0; i < count; i++)
            {
                float a = i / (float)count * Mathf.PI * 2f + Random.value * 0.6f;
                float rad = Random.Range(1.2f, 4.5f);
                Vector3 ground = focus + new Vector3(Mathf.Cos(a) * rad, 0f, Mathf.Sin(a) * rad);
                Vector3 spawn = ground + Vector3.up * Random.Range(9f, 13f);
                var go = new GameObject("Chad Coin");
                go.transform.position = spawn;
                var c = go.AddComponent<ChadCoin>();
                c.Build(ground.y + 0.4f, this);
                _liveCoins.Add(c);
            }
        }

        // the dump: a red shockwave ring rips across the floor from Chad
        void MarketCrash()
        {
            if (Defeated) return;
            if (SoundFx.Instance != null) { SoundFx.Instance.Bonk(); SoundFx.Instance.Ouch(); }
            CameraShake.Shake(0.8f);

            // flash phone + ticker red (price tanking)
            if (_phoneMat != null) _phoneMat.SetColor("_EmissionColor", new Color(1f, 0.15f, 0.1f) * 2.6f);

            var go = new GameObject("Crash Wave");
            go.transform.position = new Vector3(_center.x, 0.12f, _center.z);
            var w = go.AddComponent<CrashWave>();
            w.Build(arenaRadius * 1.6f);
            _liveWaves.Add(w);
        }

        // ── DoHumiliation — portfolio to ZERO, coins rain on his head, REKT ──
        protected override void DoHumiliation()
        {
            CancelInvoke();
            if (SoundFx.Instance != null) { SoundFx.Instance.Ouch(); SoundFx.Instance.Sparkle(); }
            CameraShake.Shake(1.0f);

            // kill the green glow — everything goes red/dead
            if (_phoneMat != null)  _phoneMat.SetColor("_EmissionColor", new Color(0.6f, 0.05f, 0.05f));
            if (_chainMat != null)  _chainMat.SetColor("_EmissionColor", Color.black);
            if (_tickerMat != null) _tickerMat.SetColor("_EmissionColor", new Color(0.6f, 0.05f, 0.05f));
            if (_tickerLabel != null) { _tickerLabel.text = "PORTFOLIO $0"; _tickerLabel.color = new Color(1f, 0.2f, 0.2f); }

            Vector3 headPos = bossBodyCollider != null
                ? bossBodyCollider.bounds.center + Vector3.up * 1.4f
                : transform.position + Vector3.up * 3.6f;

            // a final crash wave + coins rain straight onto his head
            MarketCrash();
            for (int i = 0; i < 7; i++)
            {
                Vector3 ground = headPos + new Vector3(Random.Range(-0.6f, 0.6f), 0f, Random.Range(-0.6f, 0.6f));
                var go = new GameObject("REKT Coin");
                go.transform.position = headPos + Vector3.up * Random.Range(3f, 6f);
                go.AddComponent<ChadCoin>().Build(ground.y - 1.0f, null);
            }

            // confetti / stars of despair
            for (int i = 0; i < 5; i++)
                BonkBurst.Spawn(headPos + new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 1.5f), Random.Range(-1f, 1f)));

            // "REKT" floating over him
            var gag = new GameObject("Rekt");
            gag.transform.position = headPos + Vector3.up * 2.0f;
            var w = gag.AddComponent<WorldLabel>();
            w.text = "REKT"; w.color = new Color(1f, 0.15f, 0.15f); w.fontSize = 40;

            // slump the bro
            if (_chad != null) _chad.localRotation = Quaternion.Euler(20f, _chad.localEulerAngles.y, -10f);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ChadCoin — a big detailed emissive coin that plummets from above and
    //  bonks the ground. Reeded edge + raised symbol on both faces + glow.
    //  Damages the player on contact handled by the existing bonk/hazard layer?
    //  Here it's a dodge prop: it lands, bursts, and lingers briefly.
    // ─────────────────────────────────────────────────────────────────────────
    public class ChadCoin : MonoBehaviour
    {
        float _vel;
        float _restY;
        bool  _landed;
        float _killAt;
        Material _glow;

        public void Build(float restY, CryptoChadBoss owner)
        {
            _restY = restY;
            _killAt = Time.time + 4.5f;

            float r = 1.6f;   // GIANT coin

            var gold = ShaderCache.MakeMat(new Color(0.97f, 0.78f, 0.2f), 1f, 0.88f);
            gold.EnableKeyword("_EMISSION");
            gold.SetColor("_EmissionColor", new Color(0.95f, 0.68f, 0.15f) * 1.6f);
            _glow = gold;
            var edge = ShaderCache.MakeMat(new Color(0.72f, 0.5f, 0.1f), 1f, 0.7f);

            // coin body (flattened cylinder)
            var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "CoinBody"; body.transform.SetParent(transform, false);
            body.transform.localScale = new Vector3(r, 0.18f, r);
            body.GetComponent<Renderer>().sharedMaterial = gold;
            // keep its collider so it physically reads as a thrown object
            // (kinematic-ish fall; collider lets later systems detect it if needed)

            // reeded edge ring (slightly larger, deeper colour)
            var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = "CoinRim"; rim.transform.SetParent(transform, false);
            rim.transform.localScale = new Vector3(r + 0.12f, 0.16f, r + 0.12f);
            Object.Destroy(rim.GetComponent<Collider>());
            rim.GetComponent<Renderer>().sharedMaterial = edge;

            // raised symbol on both faces (a stylised Ξ / ₿ bar group)
            for (int face = 0; face < 2; face++)
            {
                float yf = (face == 0 ? 1f : -1f) * 0.20f;
                var sym = BuildKit.Cube("Sym" + face, transform, new Vector3(0f, yf, 0f),
                    new Vector3(0.18f * r, 0.06f, 1.0f * r), new Color(0.6f, 0.42f, 0.05f), 1f, 0.6f);
                Object.Destroy(sym.GetComponent<Collider>());
                var bar = BuildKit.Cube("SymBar" + face, transform, new Vector3(0f, yf, 0f),
                    new Vector3(0.7f * r, 0.06f, 0.18f * r), new Color(0.6f, 0.42f, 0.05f), 1f, 0.6f);
                Object.Destroy(bar.GetComponent<Collider>());
            }

            // tumble it for a coin-flip look
            transform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));

            // launch puff
            BonkBurst.Spawn(transform.position);
        }

        void Update()
        {
            if (Time.time >= _killAt) { Destroy(gameObject); return; }

            // glow flicker
            if (_glow != null)
            {
                float f = 1.4f + Mathf.Sin(Time.time * 18f) * 0.4f;
                _glow.SetColor("_EmissionColor", new Color(0.95f, 0.68f, 0.15f) * f);
            }

            if (_landed) { transform.Rotate(0f, 80f * Time.deltaTime, 0f, Space.World); return; }

            float dt = Time.deltaTime;
            _vel += 24f * dt;
            transform.position += Vector3.down * _vel * dt;
            transform.Rotate(220f * dt, 90f * dt, 0f, Space.Self);   // tumble while falling

            if (transform.position.y <= _restY)
            {
                var p = transform.position; p.y = _restY; transform.position = p;
                transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f); // lie flat
                _landed = true;
                _killAt = Mathf.Min(_killAt, Time.time + 1.2f);   // clear soon after landing
                if (SoundFx.Instance != null) SoundFx.Instance.Bonk();
                BonkBurst.Spawn(transform.position);
                CameraShake.Shake(0.18f);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CrashWave — an expanding blood-red shockwave ring marking the DUMP. A
    //  thin emissive torus-like ring (built from a flat cylinder shell) that
    //  grows outward across the floor and fades.
    // ─────────────────────────────────────────────────────────────────────────
    public class CrashWave : MonoBehaviour
    {
        float _maxR;
        float _r;
        float _speed = 14f;
        Material _mat;
        Transform _ring;

        public void Build(float maxR)
        {
            _maxR = Mathf.Max(2f, maxR);
            _r = 1.0f;

            _mat = ShaderCache.MakeMat(new Color(1f, 0.12f, 0.1f), 0.2f, 0.9f);
            _mat.EnableKeyword("_EMISSION");
            _mat.SetColor("_EmissionColor", new Color(1f, 0.1f, 0.06f) * 3.5f);

            // a flat ring: an outer cylinder shell. We fake a ring with a thin
            // flattened torus substitute — a low cylinder scaled wide, lifted just
            // off the floor; the growth + fade sells the shockwave.
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "WaveRing"; ring.transform.SetParent(transform, false);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localScale = new Vector3(_r * 2f, 0.04f, _r * 2f);
            Object.Destroy(ring.GetComponent<Collider>());
            ring.GetComponent<Renderer>().sharedMaterial = _mat;
            _ring = ring.transform;

            // a few orbiting red "sell" sparks riding the front
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * Mathf.PI * 2f;
                var spark = BuildKit.Cube("SellTick" + i, transform,
                    new Vector3(Mathf.Cos(a), 0.1f, Mathf.Sin(a)), new Vector3(0.5f, 0.5f, 0.15f),
                    new Color(1f, 0.15f, 0.1f), 0.2f, 0.9f);
                var m = spark.GetComponent<Renderer>().sharedMaterial;
                m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", new Color(1f, 0.12f, 0.08f) * 3f);
                Object.Destroy(spark.GetComponent<Collider>());
                spark.transform.SetParent(_ring, true);
            }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            _r += _speed * dt;
            float frac = Mathf.Clamp01(_r / _maxR);

            if (_ring != null)
                _ring.localScale = new Vector3(_r * 2f, Mathf.Lerp(0.06f, 0.01f, frac), _r * 2f);

            // fade emission as it expands
            if (_mat != null)
                _mat.SetColor("_EmissionColor", new Color(1f, 0.1f, 0.06f) * Mathf.Lerp(3.5f, 0f, frac));

            if (_r >= _maxR) Destroy(gameObject);
        }
    }
}
