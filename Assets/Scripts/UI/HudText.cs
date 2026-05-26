using UnityEngine;

namespace Spoonacci
{
    // Bottom-center prompt text via legacy OnGUI (no TMP setup needed).
    // Keeps things noob-proof — no asset wiring required.
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
                    fontSize = 22,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
            }

            float w = Screen.width * 0.8f;
            float h = 40f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height - 80f;

            // shadow
            var shadow = new GUIStyle(style);
            shadow.normal.textColor = Color.black;
            GUI.Label(new Rect(x + 2, y + 2, w, h), currentText, shadow);
            GUI.Label(new Rect(x, y, w, h), currentText, style);
        }
    }
}
