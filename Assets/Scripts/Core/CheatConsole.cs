using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // ─────────────────────────────────────────────────────────────────────────
    //  CheatConsole — GTA-style typed cheat codes.
    //
    //  Just start typing the code anywhere during gameplay (no menu, no key to
    //  open). Letters accumulate in a rolling buffer; when the buffer ends with a
    //  known code it fires, clears, plays a chime and shows a big toast. A small
    //  faint input line at the bottom shows what you're typing so it stays readable.
    //
    //  Codes:
    //    tokens      — max Troll Tokens (99999)
    //    goldspoon   — unlock/equip every spoon (switches you to the Gold Spoon)
    //    heat        — toggle Police Mode on/off
    //    humble      — instantly defeat the nearest boss
    //    nextlevel   — jump to the next level/scene
    //    bossquiet   — silence ALL bosses (they go passive)
    //    bossrage    — attack mode: ALL bosses fight on sight (even in police mode)
    //    bossally    — bosses fight FOR you (each lends a friendly drone escort)
    //    bossnormal  — restore normal boss behaviour
    //
    //  Spawned once (DontDestroyOnLoad) by AutoBootstrap, so codes work in every
    //  gameplay scene.
    // ─────────────────────────────────────────────────────────────────────────
    public class CheatConsole : MonoBehaviour
    {
        public static CheatConsole Instance { get; private set; }

        const int MaxBuffer = 24;
        string _buffer = "";

        // typing display
        float _typingShownUntil;
        // activation toast
        string _toast = "";
        float _toastUntil;

        Keyboard _kb;
        GUIStyle _toastStyle, _toastShadow, _typeStyle;

        // code → human label shown in the toast
        static readonly List<string> Codes = new List<string>
        {
            "tokens", "goldspoon", "heat", "humble", "nextlevel",
            "bossquiet", "bossrage", "bossally", "bossnormal",
        };

        public static void EnsureExists()
        {
            if (Instance != null) return;
            var go = new GameObject("[CheatConsole]");
            go.AddComponent<CheatConsole>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDisable() { Unsubscribe(); }
        void OnDestroy() { Unsubscribe(); if (Instance == this) Instance = null; }

        void Subscribe(Keyboard kb)
        {
            if (kb == null) return;
            kb.onTextInput += OnText;
            _kb = kb;
        }

        void Unsubscribe()
        {
            if (_kb != null) _kb.onTextInput -= OnText;
            _kb = null;
        }

        void Update()
        {
            // (re)bind to the active keyboard if it changed (device hot-swap / first frame)
            var cur = Keyboard.current;
            if (cur != _kb)
            {
                Unsubscribe();
                Subscribe(cur);
            }
        }

        void OnText(char c)
        {
            // only letters matter for our codes
            if (c >= 'A' && c <= 'Z') c = (char)(c + 32);
            if (c < 'a' || c > 'z') return;

            _buffer += c;
            if (_buffer.Length > MaxBuffer)
                _buffer = _buffer.Substring(_buffer.Length - MaxBuffer);
            _typingShownUntil = Time.unscaledTime + 2f;

            foreach (var code in Codes)
            {
                if (_buffer.EndsWith(code))
                {
                    Activate(code);
                    _buffer = "";
                    return;
                }
            }
        }

        // ── effects ──────────────────────────────────────────────────────────
        void Activate(string code)
        {
            string msg;
            switch (code)
            {
                case "tokens":
                    GameState.TrollTokens = 99999;
                    GameState.SpendTokens(0);            // nudge OnChanged so the HUD refreshes
                    msg = "💰 TROLL TOKENS MAXED";
                    break;

                case "goldspoon":
                    EquipBestSpoon();
                    msg = "🥄 ALL SPOONS UNLOCKED — GOLD SPOON EQUIPPED";
                    break;

                case "heat":
                    GameState.PoliceMode = !GameState.PoliceMode; // PoliceMode component reacts next frame
                    msg = GameState.PoliceMode ? "🚓 POLICE MODE ON" : "🚓 POLICE MODE OFF";
                    break;

                case "humble":
                    msg = HumbleNearestBoss();
                    break;

                case "nextlevel":
                    Toast("⏭ JUMPING TO NEXT LEVEL…");
                    if (SoundFx.Instance != null) SoundFx.Instance.LevelUp();
                    SceneFlow.JumpToNextLevel();
                    return; // scene is loading; skip the chime below

                case "bossquiet":
                    GameState.BossMode = BossBehaviour.Silenced;
                    msg = "🤫 ALL BOSSES SILENCED";
                    break;

                case "bossrage":
                    GameState.BossMode = BossBehaviour.Attack;
                    msg = "😡 BOSS ATTACK MODE — THEY FIGHT ON SIGHT";
                    break;

                case "bossally":
                    GameState.BossMode = BossBehaviour.Ally;
                    msg = "🤝 BOSSES ARE YOUR ALLIES NOW";
                    break;

                case "bossnormal":
                    GameState.BossMode = BossBehaviour.Normal;
                    msg = "↩ BOSS BEHAVIOUR: NORMAL";
                    break;

                default:
                    return;
            }

            Toast(msg);
            if (SoundFx.Instance != null) { SoundFx.Instance.Chime(); SoundFx.Instance.Sparkle(); }
        }

        void EquipBestSpoon()
        {
            // find the Gold Spoon by name (fall back to the last type)
            int idx = SpoonTypeLibrary.All.Count - 1;
            for (int i = 0; i < SpoonTypeLibrary.All.Count; i++)
                if (SpoonTypeLibrary.All[i].name.ToLowerInvariant().Contains("gold")) { idx = i; break; }
            GameState.CurrentSpoonType = idx;

            var builder = Object.FindFirstObjectByType<ProceduralSpoonBuilder>();
            if (builder != null) builder.RebuildFromCurrentType();
        }

        string HumbleNearestBoss()
        {
            var spoon = Object.FindFirstObjectByType<SpoonController>();
            Vector3 p = spoon != null ? spoon.transform.position : Vector3.zero;

            var bosses = Object.FindObjectsByType<BossFight>(FindObjectsSortMode.None);
            BossFight best = null;
            float bestSqr = float.MaxValue;
            foreach (var b in bosses)
            {
                if (b == null || b.Defeated) continue;
                float d = (b.transform.position - p).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = b; }
            }
            if (best == null) return "…no boss left to humble here";
            string name = string.IsNullOrEmpty(best.bossName) ? "Boss" : best.bossName;
            best.ForceWin();
            return "👊 " + name.ToUpperInvariant() + " HUMBLED";
        }

        void Toast(string msg)
        {
            _toast = msg;
            _toastUntil = Time.unscaledTime + 2.6f;
        }

        // ── on-screen feedback ────────────────────────────────────────────────
        void OnGUI()
        {
            EnsureStyles();

            // small input line while typing
            if (Time.unscaledTime <= _typingShownUntil && _buffer.Length > 0)
            {
                string shown = "cheat: " + _buffer;
                float w = 360f, h = 26f;
                GUI.Label(new Rect(10f, Screen.height - h - 8f, w, h), shown, _typeStyle);
            }

            // big activation toast
            if (Time.unscaledTime <= _toastUntil && !string.IsNullOrEmpty(_toast))
            {
                float w = Mathf.Min(Screen.width - 40f, 720f);
                float x = (Screen.width - w) * 0.5f;
                float y = Screen.height * 0.22f;
                GUI.Label(new Rect(x + 2f, y + 2f, w, 44f), _toast, _toastShadow);
                GUI.Label(new Rect(x, y, w, 44f), _toast, _toastStyle);
            }
        }

        void EnsureStyles()
        {
            if (_toastStyle != null) return;
            _toastStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UiScale.Font(28),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.95f, 0.5f) },
            };
            _toastShadow = new GUIStyle(_toastStyle);
            _toastShadow.normal.textColor = new Color(0f, 0f, 0f, 0.85f);
            _typeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UiScale.Font(16),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.85f, 0.9f, 1f, 0.7f) },
            };
        }
    }
}
