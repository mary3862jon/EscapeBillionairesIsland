using System.Collections.Generic;
using UnityEngine;

namespace Spoonacci
{
    // Top-right panel with active objectives + currency. Updates from GameState.
    public class ObjectiveTracker : MonoBehaviour
    {
        public List<Objective> objectives = new List<Objective>();
        GUIStyle titleStyle;
        GUIStyle objStyle;
        GUIStyle tokenStyle;

        public class Objective
        {
            public string text;
            public System.Func<bool> done;
            public Objective(string t, System.Func<bool> d) { text = t; done = d; }
        }

        public void Add(string text, System.Func<bool> done) => objectives.Add(new Objective(text, done));

        void OnGUI()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.95f, 0.6f) } };
                objStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, normal = { textColor = Color.white } };
                tokenStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.55f, 0.1f) } };
            }

            float w = 420f;
            float h = 80f + objectives.Count * 32f;
            float x = Screen.width - w - 20f;
            float y = 20f;

            // panel bg
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.65f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = prev;

            GUI.Label(new Rect(x + 14f, y + 8f, w - 28f, 32f), "OBJECTIVES", titleStyle);
            float oy = y + 44f;
            foreach (var o in objectives)
            {
                bool done = o.done != null && o.done();
                string mark = done ? "✔" : "▢";
                var s = new GUIStyle(objStyle);
                if (done) s.normal.textColor = new Color(0.6f, 1f, 0.6f);
                GUI.Label(new Rect(x + 14f, oy, w - 28f, 28f), mark + "  " + o.text, s);
                oy += 30f;
            }

            // Troll Tokens counter — separate small panel above objectives
            float ty = y - 56f;
            if (ty < 4f) ty = 4f;
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(new Rect(x, ty, w, 50f), Texture2D.whiteTexture);
            GUI.color = prev;
            string tokens = "💰 " + GameState.TrollTokens + " TROLL TOKENS";
            GUI.Label(new Rect(x + 14f, ty + 8f, w - 28f, 32f), tokens, tokenStyle);
        }
    }
}
