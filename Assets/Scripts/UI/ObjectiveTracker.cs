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
            int n = Mathf.Min(active.Count, 6);

            float w = 460f;
            float h = 60f + n * 36f;
            float x = Screen.width - w - 20f;
            float y = 70f;

            // panel
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.65f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = prev;

            GUI.Label(new Rect(x + 14f, y + 8f, w - 28f, 32f), "ACTIVE MISSIONS", titleStyle);
            float oy = y + 46f;
            for (int i = 0; i < n; i++)
            {
                var m = active[i];
                bool done = m.isComplete != null && m.isComplete();
                string mark = done ? "✔" : "▢";
                var s = done ? doneStyle : objStyle;
                GUI.Label(new Rect(x + 14f, oy, w - 28f, 32f), mark + "  " + m.title, s);
                oy += 34f;
            }

            // Troll Tokens — separate strip above
            float ty = 16f;
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(new Rect(x, ty, w, 48f), Texture2D.whiteTexture);
            GUI.color = prev;
            GUI.Label(new Rect(x + 14f, ty + 8f, w - 28f, 36f), "💰 " + GameState.TrollTokens + " TROLL TOKENS", tokenStyle);
        }

        void EnsureStyles()
        {
            if (titleStyle == null)
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.95f, 0.6f) } };
            if (objStyle == null)
                objStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, normal = { textColor = Color.white } };
            if (doneStyle == null)
                doneStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, normal = { textColor = new Color(0.6f, 1f, 0.6f) } };
            if (tokenStyle == null)
                tokenStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.55f, 0.1f) } };
        }
    }
}
