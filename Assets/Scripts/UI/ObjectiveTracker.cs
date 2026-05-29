using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spoonacci
{
    // Top-right active mission panel (driven by MissionManager) + Troll Tokens counter.
    public class ObjectiveTracker : MonoBehaviour
    {
        GUIStyle titleStyle, objStyle, tokenStyle, doneStyle, statStyle, hitStyle;
        string sceneKey;
        public bool expanded = true; // collapsible — click the header arrow

        void Awake()
        {
            sceneKey = SceneManager.GetActiveScene().name;
        }

        void Update()
        {
            MissionManager.Tick();
        }

        void OnGUI()
        {
            EnsureStyles();

            var active = new List<Mission>(MissionManager.ActiveFor(sceneKey));
            bool isCell = sceneKey.Contains("Cutlery") || sceneKey.Contains("Cell");

            int n = expanded ? Mathf.Min(active.Count, 6) : 0;
            int statRows = (expanded && isCell) ? 3 : 0;

            float w = 460f;
            float headerH = 46f;
            float h = headerH + (expanded ? (14f + n * 36f + (statRows > 0 ? (10f + statRows * 34f) : 0f)) : 6f);
            float x = Screen.width - w - 20f;
            float y = 70f;

            // ── Troll Tokens strip (its own little card) ──────────────────────
            float ty = 16f, tH = 48f;
            UiTheme.Panel(new Rect(x, ty, w, tH));
            UiTheme.Accent(new Rect(x + 1f, ty + 10f, 3f, tH - 20f), UiTheme.TextWarn);
            UiTheme.Label(new Rect(x + 18f, ty + 8f, w - 28f, 36f), "💰 " + GameState.TrollTokens + "  " + Loc.T("hud.tokens"), tokenStyle);

            // ── Active-missions panel ─────────────────────────────────────────
            UiTheme.Panel(new Rect(x, y, w, h));

            // header (transparent clickable region over the card)
            string arrow = expanded ? "▾" : "▸";
            int total = active.Count + statRows;
            UiTheme.Accent(new Rect(x + 1f, y + 11f, 3f, headerH - 18f), UiTheme.Gold);
            UiTheme.Label(new Rect(x + 18f, y + 7f, w - 32f, 34f), arrow + "  " + Loc.T("obj.title") + "   (" + total + ")", titleStyle);
            if (GUI.Button(new Rect(x, y, w, headerH), GUIContent.none, hitStyle))
                expanded = !expanded;

            if (expanded)
            {
                UiTheme.Rule(x + 18f, y + headerH - 2f, w - 36f);
                float oy = y + headerH + 8f;
                for (int i = 0; i < n; i++)
                {
                    var m = active[i];
                    bool done = m.isComplete != null && m.isComplete();
                    string mark = done ? "✔" : "○";
                    var s = done ? doneStyle : objStyle;
                    UiTheme.Label(new Rect(x + 22f, oy, w - 36f, 32f), mark + "   " + Loc.T(m.title), s);
                    oy += 34f;
                }

                // Prison stats INSIDE the same panel — same style
                if (isCell)
                {
                    oy += 8f;
                    UiTheme.Rule(x + 18f, oy - 4f, w - 36f);
                    int rem = Mathf.Max(0, PrisonState.Total - PrisonState.Persuaded);
                    string s1 = Loc.T("prison.cells")     + "  " + PrisonState.Unlocked + " / " + PrisonState.TotalCells;
                    string s2 = Loc.T("prison.persuaded") + "  " + PrisonState.Persuaded + " / " + PrisonState.Total;
                    string s3 = Loc.T("prison.resisting") + " " + rem;
                    UiTheme.Label(new Rect(x + 22f, oy, w - 36f, 28f), "🔓  " + s1, statStyle); oy += 34f;
                    UiTheme.Label(new Rect(x + 22f, oy, w - 36f, 28f), "🥄  " + s2, statStyle); oy += 34f;
                    UiTheme.Label(new Rect(x + 22f, oy, w - 36f, 28f), "💢  " + s3, statStyle);
                }
            }
        }

        void EnsureStyles()
        {
            if (titleStyle == null)
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(21), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = UiTheme.GoldSoft } };
            if (objStyle == null)
                objStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(18), alignment = TextAnchor.MiddleLeft, normal = { textColor = UiTheme.TextMain } };
            if (doneStyle == null)
                doneStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(18), alignment = TextAnchor.MiddleLeft, normal = { textColor = UiTheme.TextDone } };
            if (statStyle == null)
                statStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(18), alignment = TextAnchor.MiddleLeft, normal = { textColor = UiTheme.GoldSoft } };
            if (tokenStyle == null)
                tokenStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(25), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = UiTheme.TextWarn } };
            if (hitStyle == null)
                hitStyle = new GUIStyle(); // invisible click target over the card header
        }
    }
}
