using UnityEngine;

namespace Spoonacci
{
    // ─────────────────────────────────────────────────────────────────────────
    //  BossDrone — a detailed quad-style attack drone built entirely from
    //  primitives. Phase-1 minion for any BossFight.
    //
    //  Build (in Awake): brushed-metal central body, carbon-fibre top shell,
    //  4 arms ending in motor pods, 4 spinning rotor discs, a gimbal camera ball
    //  with a glowing red lens, 4 status LEDs. The whole thing bobs + strafes
    //  around the player and periodically fires a harmless emissive tracer.
    //
    //  Death: hp<=0 OR a player bonk → sparks, drops, and is destroyed; reports
    //  to owner.OnDroneDown(this).
    // ─────────────────────────────────────────────────────────────────────────
    public class BossDrone : MonoBehaviour
    {
        public bool Alive { get; private set; } = true;

        Transform _target;
        BossFight _owner;
        float _hp;
        bool _down;

        readonly Transform[] _rotors = new Transform[4];
        Transform _gimbal, _lens;
        float _orbitAngle;
        float _orbitRadius = 6.5f;
        float _bobPhase;
        float _nextFire;
        float _fallVel;

        // bonk detection (drone isn't a Civilian → mirror BonkAttack manual swing test)
        float _bonkCooldown;

        public void Init(Transform target, BossFight owner, float hp)
        {
            _target = target;
            _owner = owner;
            _hp = Mathf.Max(1f, hp);
            _orbitAngle = Random.value * Mathf.PI * 2f;
            _orbitRadius = Random.Range(5.5f, 8.5f);
            _bobPhase = Random.value * Mathf.PI * 2f;
            _nextFire = Time.time + Random.Range(1.5f, 3f);
        }

        void Awake() { Build(); }

        // ---- detailed primitive assembly ------------------------------------
        void Build()
        {
            var metal  = ShaderCache.MakeTextured(BossTextures.BrushedMetal, new Color(0.8f, 0.82f, 0.85f), 0.85f, 0.65f, new Vector2(2, 2));
            var carbon = ShaderCache.MakeTextured(BossTextures.CarbonFiber, Color.white, 0.4f, 0.55f, new Vector2(3, 3));
            var dark   = ShaderCache.MakeMat(new Color(0.10f, 0.10f, 0.12f), 0.6f, 0.5f);

            // central body (rounded — capsule core + carbon shell box)
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(transform, false);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            body.transform.localScale = new Vector3(0.9f, 0.45f, 0.9f);
            body.GetComponent<Renderer>().sharedMaterial = metal;
            // keep a collider on the body for bonk hit-testing
            var bodyCol = body.GetComponent<Collider>();
            if (bodyCol != null) bodyCol.isTrigger = false;

            var shell = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shell.name = "Shell";
            shell.transform.SetParent(transform, false);
            shell.transform.localPosition = new Vector3(0f, 0.18f, 0f);
            shell.transform.localScale = new Vector3(1.1f, 0.22f, 1.1f);
            Destroy(shell.GetComponent<Collider>());
            shell.GetComponent<Renderer>().sharedMaterial = carbon;

            // 4 arms + motor pods + rotors at the corners
            float a = 0.95f;
            Vector3[] corners =
            {
                new Vector3( a, 0f,  a), new Vector3(-a, 0f,  a),
                new Vector3( a, 0f, -a), new Vector3(-a, 0f, -a),
            };
            for (int i = 0; i < 4; i++)
            {
                Vector3 c = corners[i];
                // arm (thin cylinder from centre to pod)
                var arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                arm.name = "Arm" + i;
                arm.transform.SetParent(transform, false);
                Vector3 mid = c * 0.5f;
                arm.transform.localPosition = new Vector3(mid.x, 0.02f, mid.z);
                arm.transform.localRotation = Quaternion.FromToRotation(Vector3.up, c.normalized) * Quaternion.Euler(0, 0, 90f);
                // length along the arm
                float len = c.magnitude * 0.5f;
                arm.transform.localScale = new Vector3(0.10f, len, 0.10f);
                Destroy(arm.GetComponent<Collider>());
                arm.GetComponent<Renderer>().sharedMaterial = metal;

                // motor pod
                var pod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pod.name = "Pod" + i;
                pod.transform.SetParent(transform, false);
                pod.transform.localPosition = new Vector3(c.x, 0.05f, c.z);
                pod.transform.localScale = new Vector3(0.26f, 0.10f, 0.26f);
                Destroy(pod.GetComponent<Collider>());
                pod.GetComponent<Renderer>().sharedMaterial = dark;

                // rotor disc (thin, spins every frame)
                var rotor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rotor.name = "Rotor" + i;
                rotor.transform.SetParent(transform, false);
                rotor.transform.localPosition = new Vector3(c.x, 0.16f, c.z);
                rotor.transform.localScale = new Vector3(0.62f, 0.012f, 0.62f);
                Destroy(rotor.GetComponent<Collider>());
                var rm = ShaderCache.MakeMat(new Color(0.05f, 0.05f, 0.06f, 1f), 0.3f, 0.7f);
                rotor.GetComponent<Renderer>().sharedMaterial = rm;
                // blade bar across the disc so the spin reads
                var blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.transform.SetParent(rotor.transform, false);
                blade.transform.localPosition = Vector3.zero;
                blade.transform.localScale = new Vector3(1.6f, 1.2f, 0.12f);
                Destroy(blade.GetComponent<Collider>());
                blade.GetComponent<Renderer>().sharedMaterial = ShaderCache.MakeMat(new Color(0.15f, 0.15f, 0.17f), 0.2f, 0.3f);
                _rotors[i] = rotor.transform;

                // status LED on each pod
                var led = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                led.transform.SetParent(transform, false);
                led.transform.localPosition = new Vector3(c.x, 0.12f, c.z);
                led.transform.localScale = Vector3.one * 0.09f;
                Destroy(led.GetComponent<Collider>());
                var lm = ShaderCache.MakeMat(new Color(0.1f, 1f, 0.3f), 0f, 1f);
                lm.EnableKeyword("_EMISSION");
                lm.SetColor("_EmissionColor", new Color(0.1f, 1f, 0.3f) * 2.2f);
                led.GetComponent<Renderer>().sharedMaterial = lm;
            }

            // gimbal camera ball under the body
            var gimbal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gimbal.name = "Gimbal";
            gimbal.transform.SetParent(transform, false);
            gimbal.transform.localPosition = new Vector3(0f, -0.22f, 0.18f);
            gimbal.transform.localScale = Vector3.one * 0.42f;
            Destroy(gimbal.GetComponent<Collider>());
            gimbal.GetComponent<Renderer>().sharedMaterial = dark;
            _gimbal = gimbal.transform;

            // glowing red lens on the gimbal
            var lens = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lens.name = "Lens";
            lens.transform.SetParent(gimbal.transform, false);
            lens.transform.localPosition = new Vector3(0f, 0f, 0.42f);
            lens.transform.localScale = Vector3.one * 0.55f;
            Destroy(lens.GetComponent<Collider>());
            var lensMat = ShaderCache.MakeMat(new Color(0.9f, 0.05f, 0.05f), 0f, 1f);
            lensMat.EnableKeyword("_EMISSION");
            lensMat.SetColor("_EmissionColor", new Color(1f, 0.1f, 0.1f) * 3f);
            lens.GetComponent<Renderer>().sharedMaterial = lensMat;
            _lens = lens.transform;
        }

        // ---- behaviour -------------------------------------------------------
        void Update()
        {
            if (_down) { Fall(); return; }
            float dt = Time.deltaTime;

            // spin rotors fast
            for (int i = 0; i < 4; i++)
                if (_rotors[i] != null)
                    _rotors[i].Rotate(0f, (i % 2 == 0 ? 1f : -1f) * 1800f * dt, 0f, Space.Self);

            if (_target == null) { Hover(dt); return; }

            // strafe orbit around the player
            _orbitAngle += dt * 0.9f;
            Vector3 center = _target.position + Vector3.up * 2.6f;
            Vector3 want = center + new Vector3(Mathf.Cos(_orbitAngle), 0f, Mathf.Sin(_orbitAngle)) * _orbitRadius;
            _bobPhase += dt * 2.2f;
            want.y += Mathf.Sin(_bobPhase) * 0.35f;
            transform.position = Vector3.Lerp(transform.position, want, dt * 1.8f);

            // face the player; tilt gimbal toward them
            Vector3 look = _target.position - transform.position; look.y = 0f;
            if (look.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), dt * 3f);
            if (_gimbal != null)
                _gimbal.LookAt(_target.position + Vector3.up * 1.4f);

            // periodic harmless tracer
            if (Time.time >= _nextFire)
            {
                Fire();
                _nextFire = Time.time + Random.Range(2.2f, 3.6f);
            }

            DetectBonk();
        }

        void Hover(float dt)
        {
            _bobPhase += dt * 2.2f;
            transform.position += Vector3.up * Mathf.Sin(_bobPhase) * 0.01f;
        }

        void Fire()
        {
            if (_lens == null || _target == null) return;
            if (SoundFx.Instance != null) SoundFx.Instance.Swoosh();
            // spawn a small emissive tracer that flies toward (not through) the player
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Tracer";
            go.transform.position = _lens.position;
            go.transform.localScale = Vector3.one * 0.18f;
            Destroy(go.GetComponent<Collider>());
            var m = ShaderCache.MakeMat(new Color(1f, 0.4f, 0.1f), 0f, 1f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", new Color(1f, 0.45f, 0.1f) * 4f);
            go.GetComponent<Renderer>().sharedMaterial = m;
            Vector3 dir = (_target.position + Vector3.up * 1.2f - _lens.position).normalized;
            go.AddComponent<DroneTracer>().Launch(dir);
        }

        // mirror BonkAttack.ManualSwing gating, but for this non-Civilian target
        void DetectBonk()
        {
            if (_target == null || Time.time < _bonkCooldown) return;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            var mouse = UnityEngine.InputSystem.Mouse.current;
            bool swung = (kb != null && kb.qKey.wasPressedThisFrame)
                      || (mouse != null && mouse.leftButton.wasPressedThisFrame);
            if (!swung) return;

            Vector3 to = transform.position - _target.position;
            float d = to.magnitude;
            if (d > 2.8f) return;
            Vector3 flat = to; flat.y = 0f;
            float dot = Vector3.Dot(_target.forward, flat.normalized);
            if (dot < 0.35f) return;
            _bonkCooldown = Time.time + 0.3f;
            TakeHit(20f, _target.position);
        }

        public void TakeHit(float dmg, Vector3 fromPos)
        {
            if (_down) return;
            _hp -= dmg;
            if (SoundFx.Instance != null) SoundFx.Instance.Bonk();
            CameraShake.Shake(0.45f);
            BonkBurst.Spawn(transform.position);
            if (_hp <= 0f) GoDown();
        }

        void GoDown()
        {
            if (_down) return;
            _down = true;
            Alive = false;
            if (SoundFx.Instance != null) SoundFx.Instance.Ouch();
            // sparks
            BonkBurst.Spawn(transform.position);
            BonkBurst.Spawn(transform.position + Vector3.up * 0.3f);
            // kill the LEDs/lens glow → goes dark as it dies
            if (_lens != null)
            {
                var r = _lens.GetComponent<Renderer>();
                if (r != null) r.sharedMaterial = ShaderCache.MakeMat(new Color(0.15f, 0.02f, 0.02f), 0.4f, 0.3f);
            }
            _fallVel = 0f;
            if (_owner != null) _owner.OnDroneDown(this);
            Destroy(gameObject, 2.5f);
        }

        void Fall()
        {
            _fallVel += 9.8f * Time.deltaTime;
            transform.position += Vector3.down * _fallVel * Time.deltaTime;
            // tumble as it drops
            transform.Rotate(180f * Time.deltaTime, 90f * Time.deltaTime, 220f * Time.deltaTime, Space.Self);
            // still spin a couple rotors weakly
            for (int i = 0; i < 4; i++)
                if (_rotors[i] != null) _rotors[i].Rotate(0f, 300f * Time.deltaTime, 0f, Space.Self);
            if (transform.position.y < 0.1f)
            {
                if (SoundFx.Instance != null) SoundFx.Instance.Bonk();
                Destroy(gameObject);
            }
        }
    }

    // Tiny self-destructing emissive tracer fired by drones (does nothing lethal).
    public class DroneTracer : MonoBehaviour
    {
        Vector3 _dir;
        float _die;
        public void Launch(Vector3 dir) { _dir = dir; _die = Time.time + 1.6f; }
        void Update()
        {
            transform.position += _dir * 26f * Time.deltaTime;
            if (Time.time >= _die) Destroy(gameObject);
        }
    }
}
