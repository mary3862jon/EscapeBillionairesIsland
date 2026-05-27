using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // F key: lunge-bonk the nearest violator. Auto-targeting + slow-mo + combo counter.
    public class BonkAttack : MonoBehaviour
    {
        public float targetRange = 18f;
        public float bonkRange = 1.8f;
        public float comboWindow = 3f;
        public float slowMoDuration = 0.35f;
        public float slowMoScale = 0.35f;

        SpoonController ctrl;
        SpoonAnimator anim;

        public Civilian CurrentLockOn { get; private set; }
        public int Combo { get; private set; }
        public float ComboBarUntil { get; private set; }

        float pendingBonkTime;
        Civilian pendingTarget;
        float slowMoUntil;

        void Awake()
        {
            ctrl = GetComponent<SpoonController>();
            anim = GetComponent<SpoonAnimator>();
        }

        void Update()
        {
            CurrentLockOn = ViolatorRegistry.NearestTo(transform.position, targetRange);

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.fKey.wasPressedThisFrame && CurrentLockOn != null)
            {
                LaunchAt(CurrentLockOn);
            }

            // pending impact check (during/after lunge)
            if (pendingTarget != null)
            {
                if (Vector3.Distance(transform.position, pendingTarget.transform.position) < bonkRange ||
                    Time.time > pendingBonkTime)
                {
                    Land(pendingTarget);
                    pendingTarget = null;
                }
            }

            // combo expire
            if (Time.time > ComboBarUntil) Combo = 0;

            // slow-mo restore
            if (Time.unscaledTime > slowMoUntil && Time.timeScale != 1f)
                Time.timeScale = 1f;
        }

        void LaunchAt(Civilian c)
        {
            if (ctrl == null) return;
            ctrl.StartLungeTo(c.transform.position, 0.8f);
            pendingTarget = c;
            pendingBonkTime = Time.time + 0.85f;
        }

        void Land(Civilian c)
        {
            if (c == null) return;
            c.ReceiveBonk(transform.position);
            if (anim != null) anim.TriggerBonkSwing();

            // combo
            Combo += 1;
            ComboBarUntil = Time.time + comboWindow;

            // slow-mo punch
            Time.timeScale = slowMoScale;
            slowMoUntil = Time.unscaledTime + slowMoDuration;

            // little camera shake via camera follower would be nice — skipping for now
        }

        void OnGUI()
        {
            // Lock-on prompt
            if (CurrentLockOn != null && !ctrl.LungeActive)
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 24, fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(1f, 0.85f, 0.3f) }
                };
                float w = 420f;
                float x = (Screen.width - w) * 0.5f;
                float y = Screen.height * 0.18f;
                var shadow = new GUIStyle(style);
                shadow.normal.textColor = Color.black;
                string txt = "⚠ VIOLATOR DETECTED — Press F to BONK!";
                GUI.Label(new Rect(x + 2, y + 2, w, 40f), txt, shadow);
                GUI.Label(new Rect(x, y, w, 40f), txt, style);
            }

            // combo counter
            if (Combo > 1)
            {
                var s = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 36, fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = new Color(1f, 0.55f, 0.15f) }
                };
                var sh = new GUIStyle(s); sh.normal.textColor = Color.black;
                string c = "x" + Combo + " COMBO!";
                GUI.Label(new Rect(22f, 22f, 300f, 50f), c, sh);
                GUI.Label(new Rect(20f, 20f, 300f, 50f), c, s);
            }
        }
    }
}
