using UnityEngine;

namespace Spoonacci
{
    // Bottom-center prompt text. Big fat readable.
    public class HudText : MonoBehaviour, KeyValueText
    {
        string currentText = "";
        GUIStyle style;

        public void Set(string s) { currentText = s ?? ""; }

        void OnGUI()
        {
            if (string.IsNullOrEmpty(currentText)) return;

            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = UiScale.Font(32),
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
                style.wordWrap = true;
            }

            float w = Mathf.Min(Screen.width * 0.92f, 1400f);
            float h = 90f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height - 130f;

            // dark panel
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = prev;

            var shadow = new GUIStyle(style);
            shadow.normal.textColor = Color.black;
            GUI.Label(new Rect(x + 3, y + 3, w, h), currentText, shadow);
            GUI.Label(new Rect(x, y, w, h), currentText, style);
        }
    }
}
