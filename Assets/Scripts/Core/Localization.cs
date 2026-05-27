using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    public enum Lang { EN, RU }

    // Tiny i18n. Use Loc.T("key") everywhere. Press L to toggle EN/RU.
    public static class Loc
    {
        public static Lang Current = Lang.EN;
        public static System.Action OnLanguageChanged;

        static readonly Dictionary<string, (string en, string ru)> _table = new Dictionary<string, (string, string)>
        {
            // title screen
            { "title.subtitle",   ("Escape the Billionaire's Island", "Побег с острова миллиардеров") },
            { "title.new_game",   ("NEW GAME",  "НОВАЯ ИГРА") },
            { "title.continue",   ("CONTINUE",  "ПРОДОЛЖИТЬ") },
            { "title.quit",       ("QUIT",      "ВЫЙТИ") },
            { "title.save_found", ("Save: found ✔",   "Сохранение: найдено ✔") },
            { "title.no_save",    ("Save: no save yet", "Сохранение: пока нет") },
            { "title.tokens",     ("Tokens",    "Монеты") },
            { "title.missions",   ("Missions done", "Заданий выполнено") },

            // pause menu
            { "pause.title",    ("PAUSED",            "ПАУЗА") },
            { "pause.resume",   ("RESUME",            "ПРОДОЛЖИТЬ") },
            { "pause.save",     ("SAVE GAME (F5)",    "СОХРАНИТЬ ИГРУ (F5)") },
            { "pause.to_title", ("QUIT TO TITLE",     "В ГЛАВНОЕ МЕНЮ") },
            { "pause.to_quit",  ("QUIT TO DESKTOP",   "ВЫЙТИ ИЗ ИГРЫ") },
            { "pause.help",     ("[ESC] resume  ·  [F5] save  ·  [K/J] music  ·  [M] mute  ·  [TAB] FPS mode  ·  [L] language",
                                 "[ESC] продолжить · [F5] сохранить · [K/J] музыка · [M] выкл · [TAB] FPS режим · [L] язык") },
            { "pause.now_playing", ("♪ Now playing:", "♪ Играет:") },

            // quest banner
            { "quest.current", ("⭐ CURRENT QUEST", "⭐ ТЕКУЩЕЕ ЗАДАНИЕ") },
            { "quest.freeroam", ("Free roam — explore, bonk, save (F5), press ESC to pause",
                                 "Свободный режим — исследуй, бей, сохраняй (F5), ESC для паузы") },

            // hud / common
            { "hud.tokens",    ("TROLL TOKENS",        "ТРОЛЛЬ-МОНЕТ") },
            { "hud.violator",  ("VIOLATOR",            "НАРУШИТЕЛЬ") },
            { "hud.press_f",   ("Press F to AUTO-BONK!", "Нажми F для АВТО-УДАРА!") },
            { "hud.combo",     ("COMBO!",              "КОМБО!") },
            { "hud.controls",  ("[WASD] walk · [SPACE] hop · [Q] manual BONK · [F] auto-lock BONK · [E] interact · [P] Police · [TAB] FPS · [L] lang · [ESC] menu",
                                 "[WASD] идти · [SPACE] прыжок · [Q] ручной УДАР · [F] авто-УДАР · [E] действие · [P] полиция · [TAB] FPS · [L] язык · [ESC] меню") },

            // police mode
            { "police.on",  ("🚨 POLICE MODE — siren active", "🚨 РЕЖИМ ПОЛИЦИИ — сирена включена") },
            { "police.off", ("Press [P] for Police Mode",     "Нажми [P] для режима полиции") },

            // prison stats
            { "prison.stats",    ("PRISON STATS",         "СТАТИСТИКА ТЮРЬМЫ") },
            { "prison.cells",    ("🔓 Cells unlocked:",   "🔓 Открыто камер:") },
            { "prison.persuaded",("🥄 Persuaded:",        "🥄 Уговорено:") },
            { "prison.resisting",("💢 Still resisting:",  "💢 Сопротивляются:") },

            // missions panel
            { "obj.title", ("ACTIVE MISSIONS", "АКТИВНЫЕ ЗАДАНИЯ") },
        };

        public static string T(string key)
        {
            if (_table.TryGetValue(key, out var pair))
                return Current == Lang.EN ? pair.en : pair.ru;
            return key;
        }

        public static void Toggle()
        {
            Current = Current == Lang.EN ? Lang.RU : Lang.EN;
            OnLanguageChanged?.Invoke();
        }
    }

    // Drop this on any GameObject in the scene to enable the L-toggle.
    public class LanguageToggle : MonoBehaviour
    {
        float toastUntil;
        GUIStyle style;

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.lKey.wasPressedThisFrame)
            {
                Loc.Toggle();
                toastUntil = Time.unscaledTime + 2f;
                SoundFx.Instance.Chime();
            }
        }

        void OnGUI()
        {
            if (Time.unscaledTime > toastUntil) return;
            if (style == null)
                style = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(20), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.6f, 1f, 0.9f) } };
            string msg = "🌐 " + (Loc.Current == Lang.EN ? "Language: English" : "Язык: Русский");
            var sh = new GUIStyle(style); sh.normal.textColor = Color.black;
            GUI.Label(new Rect(0, 60, Screen.width, 30), msg, sh);
            GUI.Label(new Rect(0, 58, Screen.width, 30), msg, style);
        }
    }
}
