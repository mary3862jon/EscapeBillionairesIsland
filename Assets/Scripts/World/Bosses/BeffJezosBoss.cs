using System.Collections.Generic;
using UnityEngine;

namespace Spoonacci
{
    // ─────────────────────────────────────────────────────────────────────────
    //  BeffJezosBoss — the mega-yacht mogul.  A NORMAL boss (NOT a god-boss), so
    //  he goes passive in police mode (CanEngage() inherited from BossFight).
    //
    //  Phase 1 (Minions):     3 BossDrones styled as champagne-serving 'butler
    //                         drones' (tinted via SpawnDrone HP only — they keep
    //                         the detailed quad mesh; we hang a tray + glass off
    //                         each so they read as butlers).
    //  Phase 2 (Gimmick):     Beff fires a CHAMPAGNE-CORK + bubble barrage at the
    //                         player AND periodically TOPPLES a tier of his own
    //                         champagne tower — a cascade of emissive glass the
    //                         player must dodge. While a tier is mid-collapse he
    //                         is leaning over it, exposed: bonk him in that gap
    //                         (Q / LMB in range, facing) to drain CLOUT.
    //  Phase 3 (Humiliation): Beff face-plants into the pyramid; the whole stack
    //                         shatters in an emissive-glass cascade with pearls
    //                         everywhere and a "PRIME-VAL HUMILIATION" gag.
    //
    //  bossName == "Beff Jezos" EXACTLY so the registry mission completes.
    // ─────────────────────────────────────────────────────────────────────────
    public class BeffJezosBoss : BossFight
    {
        Transform _beff;            // the mogul figure (turns to face / taunt)
        Transform _flute;           // champagne flute he toasts with
        Material  _fluteMat;
        Transform _podium;          // captain's podium
        Vector3   _towerBase;       // where his topple-able tower stands
        readonly List<BeffTowerTier> _tiers = new List<BeffTowerTier>();

        float _nextCorkAt;
        float _nextToppleAt;
        float _exposedUntil;        // window where a bonk lands bonus clout
        bool  _bonusPrimed;

        public BeffJezosBoss()
        {
            bossName        = "Beff Jezos";   // MUST match registry exactly
            // isGodBoss left FALSE — normal boss, passive in police mode.
            tokenReward     = 90;
            arenaRadius     = 19f;
            maxClout        = 100f;
            bonkRange       = 3.3f;
            bonkCloutDamage = 11f;
        }

        // ── BuildBoss — smug bald mogul in a tiny vest on a captain's podium ─
        protected override void BuildBoss()
        {
            var skin   = ShaderCache.MakeMat(new Color(0.90f, 0.74f, 0.62f), 0f, 0.30f);
            var vest   = ShaderCache.MakeTextured(BossTextures.CarbonFiber, new Color(0.10f, 0.11f, 0.13f), 0.3f, 0.55f, new Vector2(1, 1));
            var vestTrim = ShaderCache.MakeMat(new Color(0.85f, 0.68f, 0.24f), 0.9f, 0.85f);
            var shorts = ShaderCache.MakeMat(new Color(0.92f, 0.92f, 0.95f), 0.1f, 0.4f);   // little white shorts
            var shoes  = ShaderCache.MakeMat(new Color(0.08f, 0.07f, 0.06f), 0.3f, 0.6f);

            // captain's podium (chrome drum on a textured deck plinth) — at deck level
            var podiumRoot = new GameObject("Beff.Podium");
            podiumRoot.transform.SetParent(transform, false);
            podiumRoot.transform.localPosition = new Vector3(0f, 0.65f, 3.5f);   // forward of the tower, facing aft
            _podium = podiumRoot.transform;

            BuildKit.Cylinder("PodiumBase", podiumRoot.transform, new Vector3(0f, 0.2f, 0f),
                new Vector3(2.8f, 0.2f, 2.8f), new Color(0.85f, 0.86f, 0.9f), 0.85f, 0.85f);
            BuildKit.Cylinder("PodiumDrum", podiumRoot.transform, new Vector3(0f, 0.55f, 0f),
                new Vector3(1.8f, 0.4f, 1.8f), new Color(0.14f, 0.15f, 0.18f), 0.75f, 0.55f);
            var collar = BuildKit.Cylinder("PodiumCollar", podiumRoot.transform, new Vector3(0f, 0.95f, 0f),
                new Vector3(2.0f, 0.06f, 2.0f), new Color(0.85f, 0.68f, 0.24f), 0.95f, 0.85f);
            Emit(collar, new Color(0.85f, 0.68f, 0.24f) * 0.4f);
            Object.Destroy(collar.GetComponent<Collider>());
            // ship's wheel on a pedestal in front of the podium
            BuildShipWheel(podiumRoot.transform, new Vector3(0f, 1.0f, 1.0f));

            // figure root on top of the podium (~y=1.05 local)
            var fig = new GameObject("Beff.Figure");
            fig.transform.SetParent(podiumRoot.transform, false);
            fig.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            _beff = fig.transform;

            // shoes + legs (bare muscly legs — billionaire-on-vacation energy)
            Shoe(fig.transform, new Vector3(-0.20f, 0.1f, 0.05f), shoes);
            Shoe(fig.transform, new Vector3( 0.20f, 0.1f, 0.05f), shoes);
            var legL = BuildKit.Cylinder("LegL", fig.transform, new Vector3(-0.20f, 0.42f, 0f), new Vector3(0.24f, 0.32f, 0.24f), default, 0, 0);
            legL.GetComponent<Renderer>().sharedMaterial = skin; Object.Destroy(legL.GetComponent<Collider>());
            var legR = BuildKit.Cylinder("LegR", fig.transform, new Vector3( 0.20f, 0.42f, 0f), new Vector3(0.24f, 0.32f, 0.24f), default, 0, 0);
            legR.GetComponent<Renderer>().sharedMaterial = skin; Object.Destroy(legR.GetComponent<Collider>());
            // tiny white shorts
            var sh = BuildKit.Cube("Shorts", fig.transform, new Vector3(0f, 0.72f, 0f), new Vector3(0.62f, 0.32f, 0.42f), default, 0, 0);
            sh.GetComponent<Renderer>().sharedMaterial = shorts; Object.Destroy(sh.GetComponent<Collider>());

            // bare muscly torso — give it the bonk hit-collider
            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso"; torso.transform.SetParent(fig.transform, false);
            torso.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            torso.transform.localScale = new Vector3(0.82f, 0.58f, 0.66f);
            torso.GetComponent<Renderer>().sharedMaterial = skin;
            bossBodyCollider = torso.GetComponent<Collider>();
            if (bossBodyCollider != null) bossBodyCollider.isTrigger = false;

            // tiny open vest over the torso (carbon fibre + gold zip)
            var vestBack = BuildKit.Cube("VestBack", fig.transform, new Vector3(0f, 1.3f, -0.30f), new Vector3(0.78f, 0.78f, 0.10f), default, 0, 0);
            vestBack.GetComponent<Renderer>().sharedMaterial = vest; Object.Destroy(vestBack.GetComponent<Collider>());
            var lapL = BuildKit.Cube("VestLapelL", fig.transform, new Vector3(-0.28f, 1.3f, 0.28f), new Vector3(0.20f, 0.78f, 0.12f), default, 0, 0);
            lapL.transform.localRotation = Quaternion.Euler(0, 0, 8f);
            lapL.GetComponent<Renderer>().sharedMaterial = vest; Object.Destroy(lapL.GetComponent<Collider>());
            var lapR = BuildKit.Cube("VestLapelR", fig.transform, new Vector3(0.28f, 1.3f, 0.28f), new Vector3(0.20f, 0.78f, 0.12f), default, 0, 0);
            lapR.transform.localRotation = Quaternion.Euler(0, 0, -8f);
            lapR.GetComponent<Renderer>().sharedMaterial = vest; Object.Destroy(lapR.GetComponent<Collider>());
            var zip = BuildKit.Cube("VestZip", fig.transform, new Vector3(0f, 1.3f, 0.34f), new Vector3(0.05f, 0.7f, 0.05f), default, 0, 0);
            zip.GetComponent<Renderer>().sharedMaterial = vestTrim; Object.Destroy(zip.GetComponent<Collider>());

            // arms — one toasting a flute high, one cocked on hip
            var armL = BuildKit.Cylinder("ArmL", fig.transform, new Vector3(-0.52f, 1.3f, 0f), new Vector3(0.19f, 0.36f, 0.19f), default, 0, 0);
            armL.transform.localRotation = Quaternion.Euler(0, 0, 22f);
            armL.GetComponent<Renderer>().sharedMaterial = skin; Object.Destroy(armL.GetComponent<Collider>());
            var armR = BuildKit.Cylinder("ArmR", fig.transform, new Vector3(0.46f, 1.45f, 0.10f), new Vector3(0.19f, 0.40f, 0.19f), default, 0, 0);
            armR.transform.localRotation = Quaternion.Euler(-58f, 0, -18f);
            armR.GetComponent<Renderer>().sharedMaterial = skin; Object.Destroy(armR.GetComponent<Collider>());
            BuildKit.Sphere("HandL", fig.transform, new Vector3(-0.68f, 1.04f, 0.02f), Vector3.one * 0.18f, new Color(0.90f, 0.74f, 0.62f), 0f, 0.3f);

            // toasting flute in the raised right hand (glowing champagne)
            var hand = BuildKit.Sphere("HandR", fig.transform, new Vector3(0.66f, 1.86f, 0.34f), Vector3.one * 0.17f, new Color(0.90f, 0.74f, 0.62f), 0f, 0.3f);
            var fluteRoot = new GameObject("Flute"); fluteRoot.transform.SetParent(fig.transform, false);
            fluteRoot.transform.localPosition = new Vector3(0.70f, 2.05f, 0.36f);
            _flute = fluteRoot.transform;
            var glassMat = ShaderCache.MakeMat(new Color(0.88f, 0.92f, 0.82f), 0.1f, 0.96f);
            var stem = BuildKit.Cylinder("FluteStem", fluteRoot.transform, new Vector3(0f, 0f, 0f), new Vector3(0.05f, 0.18f, 0.05f), default, 0, 0);
            stem.GetComponent<Renderer>().sharedMaterial = glassMat; Object.Destroy(stem.GetComponent<Collider>());
            var bowl = BuildKit.Cylinder("FluteBowl", fluteRoot.transform, new Vector3(0f, 0.26f, 0f), new Vector3(0.16f, 0.20f, 0.16f), default, 0, 0);
            bowl.GetComponent<Renderer>().sharedMaterial = glassMat; Object.Destroy(bowl.GetComponent<Collider>());
            _fluteMat = ShaderCache.MakeMat(new Color(0.98f, 0.88f, 0.5f), 0f, 0.95f);
            _fluteMat.EnableKeyword("_EMISSION");
            _fluteMat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.45f) * 1.6f);
            var champ = BuildKit.Cylinder("Champagne", fluteRoot.transform, new Vector3(0f, 0.24f, 0f), new Vector3(0.13f, 0.15f, 0.13f), default, 0, 0);
            champ.GetComponent<Renderer>().sharedMaterial = _fluteMat; Object.Destroy(champ.GetComponent<Collider>());

            // smug bald head — shiny dome, no hair
            var head = BuildKit.Sphere("Head", fig.transform, new Vector3(0f, 1.92f, 0f), new Vector3(0.46f, 0.50f, 0.46f), default, 0, 0.3f);
            head.GetComponent<Renderer>().sharedMaterial = skin;
            // gleam-dome highlight cap (slightly more metallic so it glints)
            var dome = BuildKit.Sphere("Dome", fig.transform, new Vector3(0f, 2.06f, -0.02f), new Vector3(0.44f, 0.30f, 0.44f),
                new Color(0.93f, 0.78f, 0.66f), 0.25f, 0.7f);
            Object.Destroy(dome.GetComponent<Collider>());
            // wraparound shades (emissive dark visor)
            var shadeMat = ShaderCache.MakeMat(new Color(0.05f, 0.06f, 0.08f), 0.4f, 0.9f);
            shadeMat.EnableKeyword("_EMISSION");
            shadeMat.SetColor("_EmissionColor", new Color(0.1f, 0.2f, 0.3f) * 0.6f);
            var shades = BuildKit.Cube("Shades", fig.transform, new Vector3(0f, 1.95f, 0.22f), new Vector3(0.46f, 0.12f, 0.10f), default, 0, 0);
            shades.GetComponent<Renderer>().sharedMaterial = shadeMat; Object.Destroy(shades.GetComponent<Collider>());
            // smug little goatee
            var goatee = BuildKit.Cube("Goatee", fig.transform, new Vector3(0f, 1.74f, 0.20f), new Vector3(0.14f, 0.12f, 0.08f),
                new Color(0.18f, 0.14f, 0.10f), 0f, 0.3f);
            Object.Destroy(goatee.GetComponent<Collider>());

            // floating satirical name
            BuildKit.Label(fig.gameObject, "Beff Jezos", new Color(0.95f, 0.8f, 0.3f), 24, new Vector3(0f, 2.6f, 0f));

            // his topple-able champagne tower stands just aft of the podium,
            // between him and the player's likely approach (player must dodge it)
            _towerBase = transform.position + new Vector3(0f, 0.65f, -1.0f);
            BuildToppleTower();

            _nextCorkAt   = Time.time + 2.0f;
            _nextToppleAt = Time.time + 6.0f;
        }

        void Shoe(Transform parent, Vector3 pos, Material mat)
        {
            var s = BuildKit.Cube("Shoe", parent, pos, new Vector3(0.26f, 0.16f, 0.42f), default, 0, 0);
            s.GetComponent<Renderer>().sharedMaterial = mat;
            Object.Destroy(s.GetComponent<Collider>());
        }

        void BuildShipWheel(Transform parent, Vector3 pos)
        {
            var ped = BuildKit.Cylinder("WheelPedestal", parent, pos + new Vector3(0f, -0.3f, 0f),
                new Vector3(0.18f, 0.3f, 0.18f), new Color(0.7f, 0.55f, 0.2f), 0.8f, 0.6f);
            Object.Destroy(ped.GetComponent<Collider>());
            var hub = BuildKit.Cylinder("WheelHub", parent, pos, new Vector3(0.5f, 0.06f, 0.5f),
                new Color(0.7f, 0.55f, 0.2f), 0.85f, 0.7f);
            hub.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Object.Destroy(hub.GetComponent<Collider>());
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * 360f;
                var spoke = BuildKit.Cube("WheelSpoke" + i, parent, pos, new Vector3(0.05f, 0.7f, 0.05f),
                    new Color(0.7f, 0.55f, 0.2f), 0.8f, 0.6f);
                spoke.transform.localRotation = Quaternion.Euler(0f, 0f, a);
                Object.Destroy(spoke.GetComponent<Collider>());
            }
        }

        // ── topple-able champagne tower (3 stacked tiers of coupe glasses) ──
        void BuildToppleTower()
        {
            var glassMat = ShaderCache.MakeMat(new Color(0.85f, 0.90f, 0.78f), 0.1f, 0.96f);
            glassMat.EnableKeyword("_EMISSION");
            glassMat.SetColor("_EmissionColor", new Color(0.95f, 0.82f, 0.4f) * 0.85f);
            var bubbleMat = ShaderCache.MakeMat(new Color(0.98f, 0.9f, 0.5f), 0f, 0.95f);
            bubbleMat.EnableKeyword("_EMISSION");
            bubbleMat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.45f) * 1.5f);

            // plinth
            var plinth = new GameObject("Beff.ToppleTower");
            plinth.transform.SetParent(transform, false);
            plinth.transform.position = _towerBase;
            var baseDisc = BuildKit.Cylinder("ToppleBase", plinth.transform, new Vector3(0f, 0.05f, 0f),
                new Vector3(3.6f, 0.16f, 3.6f), new Color(0.82f, 0.85f, 0.9f), 0.95f, 0.9f);
            Object.Destroy(baseDisc.GetComponent<Collider>());

            int tiers = 3;
            float tierH = 0.9f;
            float spacing = 0.95f;
            for (int t = 0; t < tiers; t++)
            {
                int g = tiers - t + 1;     // 4,3,2 glasses per side (top-heavy showpiece)
                float y = 0.16f + t * tierH;
                float off = (g - 1) * 0.5f * spacing;

                var tierGo = new GameObject("Tier" + t);
                tierGo.transform.SetParent(plinth.transform, false);
                tierGo.transform.localPosition = new Vector3(0f, y, 0f);
                var tier = tierGo.AddComponent<BeffTowerTier>();

                for (int ix = 0; ix < g; ix++)
                for (int iz = 0; iz < g; iz++)
                {
                    Vector3 p = new Vector3(ix * spacing - off, 0f, iz * spacing - off);
                    Coupe(tierGo.transform, p, glassMat, bubbleMat);
                }
                _tiers.Add(tier);
            }
        }

        void Coupe(Transform parent, Vector3 pos, Material glass, Material bubble)
        {
            var foot = BuildKit.Cylinder("CoupeFoot", parent, pos + new Vector3(0f, 0.04f, 0f),
                new Vector3(0.32f, 0.03f, 0.32f), Color.white, 0.1f, 0.95f);
            foot.GetComponent<Renderer>().sharedMaterial = glass; Object.Destroy(foot.GetComponent<Collider>());
            var stem = BuildKit.Cylinder("CoupeStem", parent, pos + new Vector3(0f, 0.24f, 0f),
                new Vector3(0.06f, 0.20f, 0.06f), Color.white, 0.1f, 0.95f);
            stem.GetComponent<Renderer>().sharedMaterial = glass; Object.Destroy(stem.GetComponent<Collider>());
            var bowl = BuildKit.Cylinder("CoupeBowl", parent, pos + new Vector3(0f, 0.52f, 0f),
                new Vector3(0.48f, 0.15f, 0.48f), Color.white, 0.1f, 0.96f);
            bowl.GetComponent<Renderer>().sharedMaterial = glass; Object.Destroy(bowl.GetComponent<Collider>());
            var fizz = BuildKit.Sphere("Fizz", parent, pos + new Vector3(0f, 0.55f, 0f),
                new Vector3(0.30f, 0.10f, 0.30f), Color.white, 0f, 0.9f);
            fizz.GetComponent<Renderer>().sharedMaterial = bubble; Object.Destroy(fizz.GetComponent<Collider>());
        }

        // ── SpawnMinions — 3 champagne-serving 'butler drones' ──────────────
        protected override void SpawnMinions()
        {
            for (int i = 0; i < 3; i++)
            {
                float ang = i / 3f * Mathf.PI * 2f + 0.4f;
                Vector3 p = transform.position + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * 5.5f + Vector3.up * 4.0f;
                var d = SpawnDrone(p, 30f);
                if (d != null) DressButlerDrone(d);
            }
        }

        // hang a silver serving tray + a glowing flute under each drone so the
        // reused quad-drone reads as a champagne butler.
        void DressButlerDrone(BossDrone d)
        {
            if (d == null) return;
            var t = d.transform;
            var trayMat = ShaderCache.MakeMat(new Color(0.85f, 0.87f, 0.9f), 0.95f, 0.9f);
            var tray = BuildKit.Cylinder("ButlerTray", t, new Vector3(0f, -0.6f, 0f),
                new Vector3(0.9f, 0.04f, 0.9f), Color.white, 0.9f, 0.9f);
            tray.GetComponent<Renderer>().sharedMaterial = trayMat;
            Object.Destroy(tray.GetComponent<Collider>());
            var fizzMat = ShaderCache.MakeMat(new Color(0.98f, 0.88f, 0.5f), 0f, 0.95f);
            fizzMat.EnableKeyword("_EMISSION");
            fizzMat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.45f) * 1.6f);
            for (int i = 0; i < 3; i++)
            {
                float a = i / 3f * Mathf.PI * 2f;
                Vector3 gp = new Vector3(Mathf.Cos(a) * 0.28f, -0.45f, Mathf.Sin(a) * 0.28f);
                var stem = BuildKit.Cylinder("ButlerStem", t, gp + new Vector3(0f, 0.08f, 0f),
                    new Vector3(0.04f, 0.1f, 0.04f), new Color(0.85f, 0.9f, 0.8f), 0.1f, 0.95f);
                Object.Destroy(stem.GetComponent<Collider>());
                var fizz = BuildKit.Sphere("ButlerFizz", t, gp + new Vector3(0f, 0.2f, 0f),
                    new Vector3(0.12f, 0.16f, 0.12f), Color.white, 0f, 0.9f);
                fizz.GetComponent<Renderer>().sharedMaterial = fizzMat;
                Object.Destroy(fizz.GetComponent<Collider>());
            }
        }

        // ── TickGimmick — cork/bubble barrage + tier topples + bonk window ──
        protected override void TickGimmick(float dt)
        {
            float now = Time.time;

            // keep Beff facing the player + smug toast-bob
            if (_beff != null && player != null)
            {
                Vector3 look = player.position - _beff.position; look.y = 0f;
                if (look.sqrMagnitude > 0.01f)
                    _beff.rotation = Quaternion.Slerp(_beff.rotation, Quaternion.LookRotation(look), dt * 3f);
            }
            if (_fluteMat != null)
            {
                float s = 1.4f + Mathf.Sin(now * 5f) * 0.3f;
                _fluteMat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.45f) * s);
            }

            // ── champagne-cork / bubble barrage ──
            if (now >= _nextCorkAt)
            {
                FireCork();
                float gap = Mathf.Lerp(1.9f, 0.9f, 1f - CloutFrac);   // faster as clout drops
                _nextCorkAt = now + gap;
            }

            // ── periodic tier topple (cascading emissive glass) ──
            if (now >= _nextToppleAt)
            {
                ToppleNextTier();
                _nextToppleAt = now + Mathf.Lerp(8f, 4.5f, 1f - CloutFrac);
            }

            // ── bonus-clout window: while a tier is collapsing Beff leans over
            //    it, exposed. A clean facing bonk in range tops up his clout drain.
            if (_bonusPrimed && now <= _exposedUntil && player != null && bossBodyCollider != null)
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

        // a champagne cork that arcs at the player + a fizzy bubble spray
        void FireCork()
        {
            if (SoundFx.Instance != null) SoundFx.Instance.Swoosh();

            Vector3 spawn = (_flute != null) ? _flute.position + Vector3.up * 0.2f
                                             : transform.position + new Vector3(0f, 2.6f, 0f);
            var go = new GameObject("Champagne Cork");
            go.transform.position = spawn;
            var c = go.AddComponent<ChampagneCork>();
            Vector3 aim = (player != null) ? player.position + Vector3.up * 0.8f : spawn + Vector3.forward * 8f;
            c.Launch(aim);

            // little pop puff at the flute
            BonkBurst.Spawn(spawn);
        }

        void ToppleNextTier()
        {
            // find the highest still-standing tier and topple it
            BeffTowerTier target = null;
            for (int i = _tiers.Count - 1; i >= 0; i--)
            {
                if (_tiers[i] != null && !_tiers[i].Toppled) { target = _tiers[i]; break; }
            }
            if (target == null) return;   // tower exhausted — barrage only

            if (SoundFx.Instance != null) { SoundFx.Instance.Chime(); SoundFx.Instance.Bonk(); }
            CameraShake.Shake(0.5f);

            // shove direction biased toward the player so the cascade is a dodge threat
            Vector3 dir = Vector3.forward;
            if (player != null)
            {
                Vector3 d = player.position - target.transform.position; d.y = 0f;
                if (d.sqrMagnitude > 0.01f) dir = d.normalized;
            }
            target.Topple(dir);

            // Beff leans over the tower to gloat → exposed bonk window
            _exposedUntil = Time.time + 2.4f;
            _bonusPrimed  = true;
            if (_beff != null)
                _beff.localRotation = Quaternion.Euler(14f, _beff.localEulerAngles.y, 0f);
        }

        // ── DoHumiliation — Beff face-plants into the pyramid, glass + pearls ─
        protected override void DoHumiliation()
        {
            if (SoundFx.Instance != null) { SoundFx.Instance.Ouch(); SoundFx.Instance.Sparkle(); }
            CameraShake.Shake(1.1f);

            // kill the flute glow (toast's over)
            if (_fluteMat != null) _fluteMat.SetColor("_EmissionColor", Color.black);

            // topple every remaining tier in one grand cascade
            foreach (var tier in _tiers)
                if (tier != null && !tier.Toppled)
                    tier.Topple(Random.insideUnitSphere.normalized + Vector3.up * 0.4f);

            Vector3 headPos = bossBodyCollider != null
                ? bossBodyCollider.bounds.center + Vector3.up * 0.8f
                : transform.position + Vector3.up * 3f;

            // PEARLS EVERYWHERE — a burst of bouncing pearl spheres
            for (int i = 0; i < 22; i++)
            {
                var p = new GameObject("Pearl");
                p.transform.position = _towerBase + new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(1f, 3f), Random.Range(-1.5f, 1.5f));
                p.AddComponent<BeffPearl>().Init(Random.insideUnitSphere * 3.5f + Vector3.up * 4f);
            }

            // glass-shatter sparkle bursts over the tower
            for (int i = 0; i < 6; i++)
                BonkBurst.Spawn(_towerBase + new Vector3(Random.Range(-1.4f, 1.4f), Random.Range(0.5f, 2.5f), Random.Range(-1.4f, 1.4f)));

            // face-plant the figure forward into the wreckage
            if (_beff != null) _beff.localRotation = Quaternion.Euler(78f, _beff.localEulerAngles.y, Random.Range(-12f, 12f));

            // gag floating over him
            var gag = new GameObject("BeffGag");
            gag.transform.position = headPos + Vector3.up * 2.0f;
            var w = gag.AddComponent<WorldLabel>();
            w.text = "ORDER CANCELLED"; w.color = new Color(1f, 0.3f, 0.3f); w.fontSize = 30;
        }

        static void Emit(GameObject g, Color emission)
        {
            var m = g.GetComponent<Renderer>().sharedMaterial;
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  BeffTowerTier — one ring/grid of coupe glasses that can be TOPPLED: it
    //  tumbles (gravity + tumble spin), the glasses scatter, and it sparkles as
    //  it falls. Pure visual dodge-threat; never throws per-frame.
    // ─────────────────────────────────────────────────────────────────────────
    public class BeffTowerTier : MonoBehaviour
    {
        public bool Toppled { get; private set; }
        Vector3 _vel;
        Vector3 _spin;
        float   _killAt;
        float   _sparkleNext;

        public void Topple(Vector3 dir)
        {
            if (Toppled) return;
            Toppled = true;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) dir = Vector3.forward;
            dir.Normalize();
            _vel  = dir * Random.Range(2.5f, 4.0f) + Vector3.up * Random.Range(1.0f, 2.0f);
            _spin = new Vector3(Random.Range(120f, 260f) * (Random.value < 0.5f ? -1f : 1f),
                                Random.Range(-90f, 90f),
                                Random.Range(120f, 260f) * (Random.value < 0.5f ? -1f : 1f));
            _killAt = Time.time + 4.5f;
            _sparkleNext = 0f;
            BonkBurst.Spawn(transform.position + Vector3.up * 0.3f);
        }

        void Update()
        {
            if (!Toppled) return;
            float dt = Time.deltaTime;
            _vel += Vector3.down * 14f * dt;
            transform.position += _vel * dt;
            transform.Rotate(_spin * dt, Space.World);

            if (Time.time >= _sparkleNext)
            {
                _sparkleNext = Time.time + 0.12f;
                BonkBurst.Spawn(transform.position + Random.insideUnitSphere * 0.5f);
            }

            // rest on the deck, then fade out
            if (transform.position.y <= 0.55f && _vel.y < 0f)
            {
                var p = transform.position; p.y = 0.55f; transform.position = p;
                _vel = new Vector3(_vel.x * 0.3f, 0f, _vel.z * 0.3f);
                _spin *= 0.3f;
            }
            if (Time.time >= _killAt) Destroy(gameObject);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ChampagneCork — a corked projectile that arcs at the player, trailing a
    //  fizzy bubble stream, and pops on impact/timeout. Cork = wood top + wire
    //  cage + foil collar; the whole thing tumbles in flight.
    // ─────────────────────────────────────────────────────────────────────────
    public class ChampagneCork : MonoBehaviour
    {
        Vector3 _vel;
        const float G = 9.0f;
        float _die;
        float _bubbleNext;
        bool  _spent;
        Transform _model;

        public void Launch(Vector3 target)
        {
            _model = new GameObject("CorkModel").transform;
            _model.SetParent(transform, false);

            var corkMat = ShaderCache.MakeTextured(ProceduralTextures.Wood, new Color(0.78f, 0.62f, 0.36f), 0.05f, 0.3f, new Vector2(1, 1));
            var foilMat = ShaderCache.MakeMat(new Color(0.85f, 0.7f, 0.2f), 0.95f, 0.9f);
            foilMat.EnableKeyword("_EMISSION");
            foilMat.SetColor("_EmissionColor", new Color(0.85f, 0.7f, 0.2f) * 0.4f);
            var wireMat = ShaderCache.MakeMat(new Color(0.7f, 0.72f, 0.75f), 0.9f, 0.7f);

            // cork body (mushroom: fat cap + narrower base)
            var cap = BuildKit.Cylinder("CorkCap", _model, new Vector3(0f, 0.18f, 0f), new Vector3(0.34f, 0.16f, 0.34f), Color.white, 0, 0);
            cap.GetComponent<Renderer>().sharedMaterial = corkMat; Object.Destroy(cap.GetComponent<Collider>());
            var baseC = BuildKit.Cylinder("CorkBase", _model, new Vector3(0f, -0.04f, 0f), new Vector3(0.24f, 0.2f, 0.24f), Color.white, 0, 0);
            baseC.GetComponent<Renderer>().sharedMaterial = corkMat; Object.Destroy(baseC.GetComponent<Collider>());
            // foil collar
            var collar = BuildKit.Cylinder("CorkFoil", _model, new Vector3(0f, 0.02f, 0f), new Vector3(0.3f, 0.06f, 0.3f), Color.white, 0, 0);
            collar.GetComponent<Renderer>().sharedMaterial = foilMat; Object.Destroy(collar.GetComponent<Collider>());
            // wire cage hoop
            var hoop = BuildKit.Cylinder("CorkCage", _model, new Vector3(0f, 0.12f, 0f), new Vector3(0.36f, 0.02f, 0.36f), Color.white, 0, 0);
            hoop.GetComponent<Renderer>().sharedMaterial = wireMat; Object.Destroy(hoop.GetComponent<Collider>());

            // ballistic arc toward the target (lofted)
            Vector3 d = target - transform.position;
            Vector3 dFlat = new Vector3(d.x, 0f, d.z);
            float range = Mathf.Max(2f, dFlat.magnitude);
            float t = Mathf.Clamp(range / 12f, 0.8f, 2.0f);
            Vector3 vFlat = dFlat / t;
            float vy = (d.y + 0.5f * G * t * t) / t;
            _vel = vFlat + Vector3.up * vy;
            _die = Time.time + t + 0.4f;
        }

        void Update()
        {
            if (_spent) return;
            float dt = Time.deltaTime;
            _vel += Vector3.down * G * dt;
            transform.position += _vel * dt;
            if (_model != null) _model.Rotate(420f * dt, 260f * dt, 0f, Space.Self);

            if (Time.time >= _bubbleNext)
            {
                _bubbleNext = Time.time + 0.06f;
                BonkBurst.Spawn(transform.position - _vel.normalized * 0.4f);
            }

            if (transform.position.y <= 0.4f || Time.time >= _die) Pop();
        }

        void Pop()
        {
            if (_spent) return;
            _spent = true;
            if (SoundFx.Instance != null) SoundFx.Instance.Bonk();
            CameraShake.Shake(0.35f);
            Vector3 p = transform.position; p.y = Mathf.Max(0.4f, p.y);
            for (int i = 0; i < 3; i++)
                BonkBurst.Spawn(p + Random.insideUnitSphere * 0.6f);
            Destroy(gameObject);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  BeffPearl — a humiliation pearl: a glossy pearlescent sphere that arcs
    //  out, bounces on the deck a couple of times, then fades. Decorative only.
    // ─────────────────────────────────────────────────────────────────────────
    public class BeffPearl : MonoBehaviour
    {
        Vector3 _vel;
        float _killAt;
        int _bounces;

        public void Init(Vector3 vel)
        {
            _vel = vel;
            _killAt = Time.time + 5f;
            var mat = ShaderCache.MakeMat(new Color(0.96f, 0.95f, 0.93f), 0.35f, 0.95f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.9f, 0.88f, 0.95f) * 0.3f);
            var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.transform.SetParent(transform, false);
            s.transform.localScale = Vector3.one * Random.Range(0.16f, 0.26f);
            Object.Destroy(s.GetComponent<Collider>());
            s.GetComponent<Renderer>().sharedMaterial = mat;
        }

        void Update()
        {
            if (Time.time >= _killAt) { Destroy(gameObject); return; }
            float dt = Time.deltaTime;
            _vel += Vector3.down * 16f * dt;
            transform.position += _vel * dt;
            if (transform.position.y <= 0.65f && _vel.y < 0f)
            {
                var p = transform.position; p.y = 0.65f; transform.position = p;
                _vel = new Vector3(_vel.x * 0.6f, -_vel.y * 0.45f, _vel.z * 0.6f);
                if (++_bounces >= 3) _vel.y = 0f;
            }
        }
    }
}
