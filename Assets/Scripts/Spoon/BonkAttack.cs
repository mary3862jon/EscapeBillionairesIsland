using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // F = auto-lock onto nearest violator and lunge-bonk.
    // Q = manual bonk swing (hits anyone in front, no lock-on).
    // Adds: combo counter, slow-mo on landing, Spoon Mark + Daze Birds VFX, procedural SFX.
    public class BonkAttack : MonoBehaviour
    {
        public float lockOnRange = 22f;
        public float bonkRange = 2.2f;
        public float manualSwingRange = 2.2f;
        public float comboWindow = 3.5f;
        public float slowMoDuration = 0.32f;
        public float slowMoScale = 0.35f;

        SpoonController ctrl;
        SpoonAnimator anim;

        public Civilian CurrentLockOn { get; private set; }
        public int Combo { get; private set; }
        public float ComboBarUntil { get; private set; }

        float pendingBonkTime;
        Civilian pendingTarget;
        float slowMoUntil;
        float manualCooldown;

        GUIStyle promptStyle, comboStyle, controlsStyle;

        void Awake()
        {
            ctrl = GetComponent<SpoonController>();
            anim = GetComponent<SpoonAnimator>();
        }

        void Update()
        {
            CurrentLockOn = ViolatorRegistry.NearestTo(transform.position, lockOnRange);

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.fKey.wasPressedThisFrame && CurrentLockOn != null && !ctrl.LungeActive)
                LaunchAt(CurrentLockOn);

            if (kb.qKey.wasPressedThisFrame && Time.time > manualCooldown && !ctrl.LungeActive)
                ManualSwing();

            if (pendingTarget != null)
            {
                if (Vector3.Distance(transform.position, pendingTarget.transform.position) < bonkRange ||
                    Time.time > pendingBonkTime)
                {
                    Land(pendingTarget, true);
                    pendingTarget = null;
                }
            }

            if (Time.time > ComboBarUntil) Combo = 0;
            if (Time.unscaledTime > slowMoUntil && Time.timeScale != 1f) Time.timeScale = 1f;
        }

        void LaunchAt(Civilian c)
        {
            ctrl.StartLungeTo(c.transform.position, 0.85f);
            if (anim != null) anim.TriggerBonkSwing();
            SoundFx.Instance.Swoosh();
            pendingTarget = c;
            pendingBonkTime = Time.time + 0.9f;
        }

        void ManualSwing()
        {
            manualCooldown = Time.time + 0.55f;
            if (anim != null) anim.TriggerBonkSwing();
            SoundFx.Instance.Swoosh();

            // hit anything within manualSwingRange in front
            Civilian best = null;
            float bestDot = 0.5f;
            foreach (var c in Object.FindObjectsByType<Civilian>(FindObjectsSortMode.None))
            {
                if (c == null) continue;
                Vector3 to = c.transform.position - transform.position;
                float d = to.magnitude;
                if (d > manualSwingRange) continue;
                Vector3 dir = to / Mathf.Max(d, 0.01f);
                float dot = Vector3.Dot(transform.forward, dir);
                if (dot > bestDot) { bestDot = dot; best = c; }
            }
            if (best != null) Land(best, best.IsViolator);
        }

        void Land(Civilian c, bool wasViolator)
        {
            if (c == null) return;
            c.ReceiveBonk(transform.position);
            if (anim != null) anim.TriggerBonkSwing();
            SoundFx.Instance.Bonk();
            // Two staggered bonk sounds = THWACK + thud
            Invoke(nameof(SecondaryBonk), 0.06f);

            // SCREEN SHAKE — heavier for violators
            CameraShake.Shake(wasViolator ? 0.85f : 0.5f);

            // particle burst at head height
            Vector3 burstPos = c.HeadTransform != null ? c.HeadTransform.position : c.transform.position + Vector3.up * 1.4f;
            BonkBurst.Spawn(burstPos);
            // double burst at slight offset = bigger impact
            BonkBurst.Spawn(burstPos + Vector3.up * 0.15f);

            // attach daze birds to head
            if (c.HeadTransform != null)
            {
                var go = new GameObject("DazeBirds");
                go.AddComponent<DazeBirds>().Attach(c.HeadTransform);
            }

            Combo += 1;
            ComboBarUntil = Time.time + comboWindow;
            GameState.RegisterBonk(wasViolator);

            // level-up jingle on combo milestones
            if (Combo == 3 || Combo == 5 || Combo == 10) SoundFx.Instance.LevelUp();

            // slow-mo punch — stronger and longer
            Time.timeScale = slowMoScale * 0.7f; // even slower
            slowMoUntil = Time.unscaledTime + slowMoDuration * 1.4f;
        }

        void SecondaryBonk() { SoundFx.Instance.Bonk(); }

        void OnGUI()
        {
            EnsureStyles();

            // Bottom bar: controls always visible
            string controls = Loc.T("hud.controls");
            float cw = Mathf.Min(Screen.width * 0.95f, 1500f);
            float cx = (Screen.width - cw) * 0.5f;
            float cy = Screen.height - 30f;
            var s = new GUIStyle(controlsStyle); s.normal.textColor = Color.black;
            GUI.Label(new Rect(cx + 2, cy + 2, cw, 26f), controls, s);
            GUI.Label(new Rect(cx, cy, cw, 26f), controls, controlsStyle);

            if (CurrentLockOn != null && (ctrl == null || !ctrl.LungeActive))
            {
                float w = 700f;
                float x = (Screen.width - w) * 0.5f;
                float y = Screen.height * 0.14f;
                var sh = new GUIStyle(promptStyle); sh.normal.textColor = Color.black;
                string crime = Loc.T("crime." + CurrentLockOn.ViolationLabel);
                string txt = "⚠ " + Loc.T("hud.violator") + ": " + crime + " — " + Loc.T("hud.press_f");
                GUI.Label(new Rect(x + 3, y + 3, w, 50f), txt, sh);
                GUI.Label(new Rect(x, y, w, 50f), txt, promptStyle);
            }

            if (Combo > 1)
            {
                string c = "x" + Combo + " " + Loc.T("hud.combo");
                var sh = new GUIStyle(comboStyle); sh.normal.textColor = Color.black;
                GUI.Label(new Rect(24f, 24f, 360f, 60f), c, sh);
                GUI.Label(new Rect(20f, 20f, 360f, 60f), c, comboStyle);
            }
        }

        void EnsureStyles()
        {
            if (promptStyle == null)
                promptStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(30), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.85f, 0.3f) } };
            if (comboStyle == null)
                comboStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(44), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = new Color(1f, 0.55f, 0.15f) } };
            if (controlsStyle == null)
                controlsStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(16), alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
        }
    }
}
