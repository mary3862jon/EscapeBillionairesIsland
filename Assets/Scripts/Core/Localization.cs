using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    public enum Lang { EN, RU }

    // i18n. Use Loc.T("key") everywhere. Press L to toggle EN/RU.
    public static class Loc
    {
        public static Lang Current = Lang.EN;
        public static System.Action OnLanguageChanged;

        static readonly Dictionary<string, (string en, string ru)> _table = new Dictionary<string, (string, string)>
        {
            // -------- Title --------
            { "title.subtitle",   ("Escape the Billionaire's Island", "Побег с острова миллиардеров") },
            { "title.new_game",   ("NEW GAME",  "НОВАЯ ИГРА") },
            { "title.continue",   ("CONTINUE",  "ПРОДОЛЖИТЬ") },
            { "title.quit",       ("QUIT",      "ВЫЙТИ") },
            { "ui.music_on",      ("♪ Music: ON",  "♪ Музыка: ВКЛ") },
            { "ui.music_off",     ("♪ Music: OFF", "♪ Музыка: ВЫКЛ") },
            { "title.save_found", ("Save: found ✔",   "Сохранение: найдено ✔") },
            { "title.no_save",    ("Save: no save yet", "Сохранение: пока нет") },
            { "title.tokens",     ("Tokens",    "Монеты") },
            { "title.missions",   ("Missions done", "Заданий выполнено") },

            // -------- Pause Menu --------
            { "pause.title",       ("PAUSED",            "ПАУЗА") },
            { "pause.resume",      ("RESUME",            "ПРОДОЛЖИТЬ") },
            { "pause.save",        ("SAVE GAME (F5)",    "СОХРАНИТЬ ИГРУ (F5)") },
            { "pause.settings",    ("SETTINGS",          "НАСТРОЙКИ") },
            { "pause.exit",        ("EXIT",              "ВЫЙТИ") },
            { "pause.to_title",    ("QUIT TO TITLE",     "В ГЛАВНОЕ МЕНЮ") },
            { "pause.to_quit",     ("QUIT TO DESKTOP",   "ВЫЙТИ ИЗ ИГРЫ") },
            { "pause.help",        ("[ESC] menu  ·  [F5] save  ·  [K/J] music  ·  [M] mute  ·  [TAB] FPS  ·  [L] language",
                                    "[ESC] меню · [F5] сохранить · [K/J] музыка · [M] выкл · [TAB] FPS · [L] язык") },
            { "pause.now_playing", ("♪ Now playing:", "♪ Играет:") },

            // -------- Settings --------
            { "settings.title",      ("SETTINGS",        "НАСТРОЙКИ") },
            { "settings.resolution", ("Resolution",      "Разрешение") },
            { "settings.framerate",  ("Frame Rate",      "Частота кадров") },
            { "settings.fullscreen", ("Fullscreen",      "Полный экран") },
            { "settings.master",     ("Master Volume",   "Общая громкость") },
            { "settings.music",      ("Music Volume",    "Громкость музыки") },
            { "settings.sfx",        ("SFX Volume",      "Громкость эффектов") },
            { "settings.dialogue",   ("Dialogue Volume", "Громкость диалогов") },
            { "settings.fov",        ("Field of View",   "Угол обзора") },
            { "common.on",     ("ON",   "ВКЛ") },
            { "common.off",    ("OFF",  "ВЫКЛ") },
            { "common.back",   ("BACK", "НАЗАД") },
            { "common.unlim",  ("Unlimited", "Без лимита") },

            // -------- Quest Banner --------
            { "quest.current",  ("⭐ CURRENT QUEST", "⭐ ТЕКУЩЕЕ ЗАДАНИЕ") },
            { "quest.freeroam", ("Free roam — explore, bonk, save (F5), press ESC to pause",
                                 "Свободный режим — исследуй, бей, сохраняй (F5), ESC для паузы") },

            // -------- HUD / Common --------
            { "hud.tokens",      ("TROLL TOKENS",  "ТРОЛЛЬ-МОНЕТ") },
            { "hud.violator",    ("VIOLATOR",      "НАРУШИТЕЛЬ") },
            { "hud.press_f",     ("Press F to AUTO-BONK!", "Нажми F для АВТО-УДАРА!") },
            { "hud.combo",       ("COMBO!",        "КОМБО!") },
            { "hud.controls",    ("[WASD] walk · [HOLD RMB] look · [LMB] bonk · [F] auto-bonk · [WHEEL] zoom · [SPACE] hop · [SHIFT] sprint · [E] interact · [P] police · [L] lang · [ESC] menu",
                                  "[WASD] идти · [ПКМ зажать] обзор · [ЛКМ] удар · [F] авто-удар · [КОЛЕСО] зум · [SPACE] прыжок · [SHIFT] спринт · [E] действие · [P] полиция · [L] язык · [ESC] меню") },
            { "hud.saved",       ("Saved ✔",       "Сохранено ✔") },

            // -------- Boss fights --------
            { "boss.clout",            ("CLOUT",        "ХАЙП") },
            { "boss.bonked",           ("BONKED!",      "ПРИБИТ!") },
            { "boss.defeated",         ("DEFEATED!",    "ПОВЕРЖЕН!") },
            { "boss.phase.minions",    ("Drone swarm",  "Рой дронов") },
            { "boss.phase.gimmick",    ("Bonk the boss!", "Бей босса!") },
            { "boss.phase.humiliation",("Humiliation",  "Унижение") },
            { "boss.phase.defeated",   ("Defeated",     "Повержен") },

            // -------- Police Mode --------
            { "police.on",  ("🚨 POLICE MODE — siren active", "🚨 РЕЖИМ ПОЛИЦИИ — сирена включена") },
            { "police.off", ("Press [P] for Police Mode",      "Нажми [P] для режима полиции") },

            // -------- Prison stats --------
            { "prison.stats",     ("PRISON STATS",         "СТАТИСТИКА ТЮРЬМЫ") },
            { "prison.cells",     ("🔓 Cells unlocked:",   "🔓 Открыто камер:") },
            { "prison.persuaded", ("🥄 Persuaded:",        "🥄 Уговорено:") },
            { "prison.resisting", ("💢 Still resisting:",  "💢 Сопротивляются:") },

            // -------- Mission tracker --------
            { "obj.title", ("ACTIVE MISSIONS", "АКТИВНЫЕ ЗАДАНИЯ") },

            // -------- Mission titles (lookup by id) --------
            { "mission.cell.pickaxe",  ("Find the pickaxe (hay pile in your cell)", "Найди кирку (стог сена в твоей камере)") },
            { "mission.cell.unlock",   ("Unlock the other 4 cells (E on each door)", "Открой 4 других камеры (E на каждой двери)") },
            { "mission.cell.persuade", ("Persuade all 15 inmates (E ask, Q bonk refusers)", "Уговори всех 15 заключённых (E уговорить, Q ударить отказников)") },
            { "mission.cell.dig",      ("Dig the loose stone (E, 3 stages)",        "Копай расшатанный камень (E, 3 этапа)") },
            { "mission.island.salon",  ("Ditch the police uniform at Salon Cucchiaio", "Сними полицейскую форму в Salon Cucchiaio") },
            { "mission.isle.bonk5",    ("Bonk 5 violators",                          "Ударь 5 нарушителей") },
            { "mission.isle.tokens100",("Earn 100 Troll Tokens",                     "Заработай 100 Тролль-монет") },
            { "mission.isle.bust",     ("Enter Police Mode (P) and bust dealers",    "Включи режим полиции (P) и арестуй дилеров") },
            { "mission.isle.tusk",     ("Bonk Magnus Tusk (launch pad)",             "Ударь Магнуса Таска (стартовая площадка)") },
            { "mission.isle.boss.tusk",("Humiliate Magnus Tusk at his launch pad",   "Унизь Магнуса Таска на его стартовой площадке") },
            { "mission.isle.beff",     ("Bonk Beff Jezos (yacht SW)",                "Ударь Беффа Джезоса (яхта ЮЗ)") },
            { "mission.isle.boss.beff",("Humiliate Beff Jezos on his yacht",         "Унизь Беффа Джезоса на его яхте") },
            { "mission.isle.chad",     ("Bonk Crypto Chad (vault S)",                "Ударь Крипто Чада (хранилище Ю)") },
            { "mission.isle.boss.chad",("Humiliate Crypto Chad at his vault",        "Унизь Крипто Чада в его хранилище") },
            { "mission.isle.zuck",     ("Bonk Mark Zuckersnort (lab W)",             "Ударь Марка Цукерснорта (лаба З)") },
            { "mission.isle.boss.zuck",("Humiliate Mark Zuckersnort at his lab",     "Унизь Марка Цукерснорта в его лаборатории") },
            { "mission.isle.all4",     ("Bonk all 4 billionaires",                   "Ударь всех 4 миллиардеров") },
            { "mission.shark.fuse",    ("Walk into the ocean → fuse with the Shark", "Иди в океан → слейся с Акулой") },

            // -------- Violation crimes --------
            { "crime.LITTERING",          ("LITTERING",          "МУСОРИТ") },
            { "crime.VANDALISM",          ("VANDALISM",          "ВАНДАЛИЗМ") },
            { "crime.TAX EVASION",        ("TAX EVASION",        "УКЛОНЕНИЕ ОТ НАЛОГОВ") },
            { "crime.INSIDER TRADING",    ("INSIDER TRADING",    "ИНСАЙДЕРСКАЯ ТОРГОВЛЯ") },
            { "crime.PEEING IN POOL",     ("PEEING IN POOL",     "ПИСАЕТ В БАССЕЙН") },
            { "crime.QUEUE JUMPING",      ("QUEUE JUMPING",      "ЛЕЗЕТ БЕЗ ОЧЕРЕДИ") },
            { "crime.NFT MINTING",        ("NFT MINTING",        "МИНТИТ NFT") },
            { "crime.SHARK BAITING",      ("SHARK BAITING",      "ДРАЗНИТ АКУЛ") },
            { "crime.LOUD CHEWING",       ("LOUD CHEWING",       "ГРОМКО ЖУЁТ") },
            { "crime.CRYPTO SHILLING",    ("CRYPTO SHILLING",    "ПИАРИТ КРИПТУ") },
            { "crime.PARKING VIOLATION",  ("PARKING VIOLATION",  "ПАРКОВКА В НЕПОЛОЖЕННОМ") },
            { "crime.BAD TIPPING",        ("BAD TIPPING",        "НЕ ДАЁТ ЧАЕВЫХ") },
            { "crime.BEING A BILLIONAIRE",("BEING A BILLIONAIRE","БЫТЬ МИЛЛИАРДЕРОМ") },
            { "crime.DEALING SHADY SUBSTANCES", ("DEALING SHADY SUBSTANCES", "ТОРГУЕТ ЗАПРЕЩЁНКОЙ") },

            // -------- World labels (signs) --------
            { "sign.salon",     ("✨ SALON  CUCCHIAIO ✨", "✨ САЛОН  КУККЬЯЙО ✨") },
            { "sign.pool",      ("🏊 INFINITY POOL",         "🏊 БЕСКОНЕЧНЫЙ БАССЕЙН") },
            { "sign.bar",       ("🍹 TIKI BAR",              "🍹 ТИКИ-БАР") },
            { "sign.alley",     ("⚠ SHADY ALLEY — Police Mode (P) to bust", "⚠ ТЁМНЫЙ ПЕРЕУЛОК — режим полиции (P) для арестов") },
            { "sign.mansion",   ("💰 BILLIONAIRE MANSION",   "💰 ОСОБНЯК МИЛЛИАРДЕРА") },
            { "sign.yacht",     ("🛥  BEFF JEZOS YACHT",     "🛥  ЯХТА БЕФФА ДЖЕЗОСА") },
            { "sign.vault",     ("💎  CRYPTO CHAD VAULT",    "💎  ХРАНИЛИЩЕ КРИПТО ЧАДА") },
            { "sign.lab",       ("🧪  ZUCKERSNORT AI LAB",   "🧪  ИИ-ЛАБА ЦУКЕРСНОРТА") },
            { "sign.tusk",      ("💰  MAGNUS TUSK MANSION",  "💰  ОСОБНЯК МАГНУСА ТАСКА") },
            { "sign.skinkit",   ("✨ SKIN COLLECTION ✨",     "✨ КОЛЛЕКЦИЯ СКИНОВ ✨") },
            { "sign.prison",    ("🏚 THE CUTLERY CHAMBER — Prison Wing 7", "🏚 СТОЛОВАЯ КАМЕРА — Тюремное крыло 7") },

            // -------- Prompts (E/Q triggers) --------
            { "prompt.pickaxe",      ("⛏ PRESS E TO GRAB THE PICKAXE",        "⛏ НАЖМИ E ЧТОБЫ ВЗЯТЬ КИРКУ") },
            { "prompt.dig_start",    ("⛏ PRESS E TO START DIGGING",            "⛏ НАЖМИ E ЧТОБЫ НАЧАТЬ КОПАТЬ") },
            { "prompt.dig_again",    ("⛏ PRESS E AGAIN",                       "⛏ НАЖМИ E ЕЩЁ РАЗ") },
            { "prompt.dig_stage",    ("DIG STAGE",                              "ЭТАП КОПКИ") },
            { "prompt.need_pickaxe", ("...you need a pickaxe. (Check the hay pile in your cell.)",
                                      "...нужна кирка. (Посмотри в стоге сена в твоей камере.)") },
            { "prompt.need_persuade",("...the stone won't budge alone. Persuade all 15 inmates first.",
                                      "...камень один не сдвинуть. Сначала уговори всех 15 заключённых.") },
            { "prompt.cell_unlock",  ("🔓 PRESS E TO UNLOCK",                  "🔓 НАЖМИ E ЧТОБЫ ОТКРЫТЬ") },

            // -------- Salon --------
            { "salon.menu_title",  ("DITCH THE POLICE UNIFORM",    "СНЯТЬ ПОЛИЦЕЙСКУЮ ФОРМУ") },
            { "salon.pay",         ("[E] Pay 30 Troll Tokens (you have {0})", "[E] Заплати 30 Тролль-монет (у тебя {0})") },
            { "salon.threaten",    ("[Q] Threaten the Coiffeur (free, but rude)", "[Q] Запугать парикмахера (бесплатно, но грубо)") },
            { "salon.entry_neg",   ("✨ SALON CUCCHIAIO ✨  [E] PAY 30 tokens   ·   [Q] THREATEN for free",
                                    "✨ САЛОН КУККЬЯЙО ✨  [E] ЗАПЛАТИ 30 монет · [Q] ЗАПУГАТЬ бесплатно") },
            { "salon.entry_cycle", ("✨ SALON CUCCHIAIO ✨   Wearing: {0}   ·   [E] next skin",
                                    "✨ САЛОН КУККЬЯЙО ✨   Носит: {0}   ·   [E] следующий скин") },
            { "salon.now_wearing", ("✨ Now wearing: {0}   ·   [E] next", "✨ Теперь носит: {0}   ·   [E] следующий") },
            { "salon.paid_ok",     ("✨ The Coiffeur smiles. You blend in beautifully now.",
                                    "✨ Парикмахер улыбается. Теперь ты прекрасно сливаешься с толпой.") },
            { "salon.threatened",  ("⚠ You threatened the Coiffeur. She fled in tears. You wear what you like now.",
                                    "⚠ Ты запугал парикмахера. Она убежала в слезах. Носи что хочешь.") },
            { "salon.no_tokens",   ("...not enough tokens (need 30). Bonk more violators or [Q] threaten.",
                                    "...не хватает монет (нужно 30). Бей больше нарушителей или [Q] запугай.") },

            // -------- Scene intros --------
            { "intro.island",  ("THE ISLE OF LASCIVIOUS REPOSE — 4 billionaire zones across the compass. Find them all. F to bonk violators, Q manual swing.",
                                "ОСТРОВ РАСПУЩЕННОГО ОТДЫХА — 4 зоны миллиардеров по всем сторонам света. Найди их все. F для удара нарушителей, Q ручной замах.") },
            { "intro.cell",    ("ACT 2 — Cutlery Chamber. Grab the pickaxe, unlock the 4 other cells, persuade all 15 spoons to dig.",
                                "АКТ 2 — Столовая камера. Возьми кирку, открой 4 других камеры, уговори все 15 ложек копать.") },
            { "intro.shark",   ("ACT 3 — Walk into the ocean. A shark awaits.",
                                "АКТ 3 — Иди в океан. Тебя ждёт акула.") },
            { "intro.shark2",  ("🦈 SPOON-SHARK MODE — WASD swim · SPACE dash · roam the ocean",
                                "🦈 РЕЖИМ ЛОЖКА-АКУЛА — WASD плыть · SPACE рывок · рассекай океан") },

            // -------- Loose-stone digging --------
            { "dig.tunnel",    ("Tunnel dug!", "Туннель вырыт!") },

            // -------- Language toast --------
            { "lang.toast_en", ("🌐 Language: English", "🌐 Language: English") },
            { "lang.toast_ru", ("🌐 Язык: Русский",    "🌐 Язык: Русский") },

            // -------- Warden / Alarm --------
            { "warden.name",     ("PRISON WARDEN",     "ТЮРЕМНЫЙ НАДЗИРАТЕЛЬ") },
            { "warden.choose",   ("[E] Bribe ({0} Tokens · you have {1})    ·    [Q] Bonk him out",
                                  "[E] Подкупить ({0} монет · у тебя {1})    ·    [Q] Вырубить его") },
            { "warden.knocked",  ("...he's out cold for a while.", "...он отрублен надолго.") },
            { "alarm.banner",    ("🚨  ALARM!  THE WARDEN SAW YOU DIG  🚨", "🚨  ТРЕВОГА!  НАДЗИРАТЕЛЬ ВИДЕЛ КОПКУ  🚨") },
        };

        public static string T(string key)
        {
            if (_table.TryGetValue(key, out var pair))
                return Current == Lang.EN ? pair.en : pair.ru;
            return key;
        }

        public static string Tf(string key, params object[] args)
        {
            string s = T(key);
            try { return string.Format(s, args); } catch { return s; }
        }

        public static void Toggle()
        {
            Current = Current == Lang.EN ? Lang.RU : Lang.EN;
            OnLanguageChanged?.Invoke();
        }
    }

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
            string msg = Loc.Current == Lang.EN ? Loc.T("lang.toast_en") : Loc.T("lang.toast_ru");
            var sh = new GUIStyle(style); sh.normal.textColor = Color.black;
            GUI.Label(new Rect(0, 60, Screen.width, 30), msg, sh);
            GUI.Label(new Rect(0, 58, Screen.width, 30), msg, style);
        }
    }
}
