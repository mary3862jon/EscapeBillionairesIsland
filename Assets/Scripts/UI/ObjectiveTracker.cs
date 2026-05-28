using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spoonacci
{
    // Top-right active mission panel (driven by MissionManager) + Troll Tokens counter.
    public class ObjectiveTracker : MonoBehaviour
    {
        GUIStyle titleStyle, objStyle, tokenStyle, doneStyle;
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
            float headerH = 44f;
            float h = headerH + (expanded ? (16f + n * 36f + (statRows > 0 ? (8f + statRows * 34f) : 0f)) : 4f);
            float x = Screen.width - w - 20f;
            float y = 70f;

            // panel
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.82f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = prev;

            // header
            string arrow = expanded ? "▼" : "▶";
            int total = active.Count + statRows;
            if (GUI.Button(new Rect(x + 6f, y + 4f, w - 12f, 36f), arrow + "  " + Loc.T("obj.title") + "  (" + total + ")", titleStyle))
                expanded = !expanded;

            if (expanded)
            {
                float oy = y + headerH + 6f;
                for (int i = 0; i < n; i++)
                {
                    var m = active[i];
                    bool done = m.isComplete != null && m.isComplete();
                    string mark = done ? "✔" : "▢";
                    var s = done ? doneStyle : objStyle;
                    GUI.Label(new Rect(x + 14f, oy, w - 28f, 32f), mark + "  " + Loc.T(m.title), s);
                    oy += 34f;
                }

                // Prison stats INSIDE the same panel — same style
                if (isCell)
                {
                    oy += 6f;
                    int rem = Mathf.Max(0, PrisonState.Total - PrisonState.Persuaded);
                    string s1 = Loc.T("prison.cells")     + "  " + PrisonState.Unlocked + " / " + PrisonState.TotalCells;
                    string s2 = Loc.T("prison.persuaded") + "  " + PrisonState.Persuaded + " / " + PrisonState.Total;
                    string s3 = Loc.T("prison.resisting") + " " + rem;
                    var stat = new GUIStyle(objStyle); stat.normal.textColor = new Color(1f, 0.85f, 0.5f);
                    GUI.Label(new Rect(x + 14f, oy, w - 28f, 28f), "🔓  " + s1, stat); oy += 34f;
                    GUI.Label(new Rect(x + 14f, oy, w - 28f, 28f), "🥄  " + s2, stat); oy += 34f;
                    GUI.Label(new Rect(x + 14f, oy, w - 28f, 28f), "💢  " + s3, stat);
                }
            }

            // Troll Tokens strip above
            float ty = 16f;
            GUI.color = new Color(0f, 0f, 0f, 0.82f);
            GUI.DrawTexture(new Rect(x, ty, w, 48f), Texture2D.whiteTexture);
            GUI.color = prev;
            GUI.Label(new Rect(x + 14f, ty + 8f, w - 28f, 36f), "💰 " + GameState.TrollTokens + " " + Loc.T("hud.tokens"), tokenStyle);
        }

        void EnsureStyles()
        {
            if (titleStyle == null)
                titleStyle = new GUIStyle(GUI.skin.button) { fontSize = UiScale.Font(22), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = new Color(1f, 0.95f, 0.6f) }, hover = { textColor = new Color(1f, 1f, 0.8f) } };
            if (objStyle == null)
                objStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(18), normal = { textColor = Color.white } };
            if (doneStyle == null)
                doneStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(18), normal = { textColor = new Color(0.6f, 1f, 0.6f) } };
            if (tokenStyle == null)
                tokenStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(26), fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.55f, 0.1f) } };
        }
    }
}
