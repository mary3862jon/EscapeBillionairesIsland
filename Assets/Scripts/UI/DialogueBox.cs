using System.Collections;
using UnityEngine;

namespace Spoonacci
{
    // Bottom-of-screen dialogue panel with typewriter reveal. OnGUI = zero asset wiring.
    public class DialogueBox : MonoBehaviour
    {
        public float charsPerSecond = 40f;
        string speaker = "";
        string fullLine = "";
        string shown = "";
        Coroutine typing;
        GUIStyle nameStyle;
        GUIStyle bodyStyle;
        bool visible;

        static DialogueBox _instance;
        public static DialogueBox Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[DialogueBox]");
                    _instance = go.AddComponent<DialogueBox>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public void Say(string who, string line)
        {
            speaker = who;
            fullLine = line;
            shown = "";
            visible = true;
            if (typing != null) StopCoroutine(typing);
            typing = StartCoroutine(Reveal());
        }

        public void Hide() { visible = false; shown = ""; fullLine = ""; speaker = ""; }

        IEnumerator Reveal()
        {
            float t = 0f;
            while (shown.Length < fullLine.Length)
            {
                t += Time.unscaledDeltaTime * charsPerSecond;
                int n = Mathf.Min(fullLine.Length, Mathf.FloorToInt(t));
                shown = fullLine.Substring(0, n);
                yield return null;
            }
        }

        void OnGUI()
        {
            if (!visible) return;

            if (nameStyle == null)
            {
                nameStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.85f, 0.4f) } };
                bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true, normal = { textColor = Color.white } };
            }

            float w = Mathf.Min(Screen.width * 0.8f, 900f);
            float h = 140f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height - h - 24f;

            GUI.color = new Color(0f, 0f, 0f, 0.78f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(x + 16f, y + 8f, w - 32f, 30f), speaker, nameStyle);
            GUI.Label(new Rect(x + 16f, y + 40f, w - 32f, h - 50f), shown, bodyStyle);
        }
    }
}
