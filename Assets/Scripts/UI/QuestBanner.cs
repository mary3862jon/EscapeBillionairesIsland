using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spoonacci
{
    // Big always-visible top-center banner showing the CURRENT primary objective.
    // Picks the first active mission for the current scene from MissionManager.
    public class QuestBanner : MonoBehaviour
    {
        GUIStyle titleStyle, taskStyle;
        string sceneKey;

        void Awake() { sceneKey = SceneManager.GetActiveScene().name; }

        void OnGUI()
        {
            EnsureStyles();

            // get the first incomplete mission (title is now a translation key)
            string task = null;
            foreach (var m in MissionManager.ActiveFor(sceneKey))
            {
                if (m.isComplete != null && m.isComplete()) continue;
                task = Loc.T(m.title);
                break;
            }
            if (task == null) task = Loc.T("quest.freeroam");

            // Shifted LEFT and shrunk so it never overlaps the top-right mission panel (which is 460 wide)
            float w = Mathf.Min(Screen.width * 0.55f, 900f);
            float h = 76f;
            float x = 20f;
            float y = 70f;

            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.82f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = prev;

            GUI.Label(new Rect(x, y + 6f, w, 26f), Loc.T("quest.current"), titleStyle);
            GUI.Label(new Rect(x + 14f, y + 36f, w - 28f, 36f), task, taskStyle);
        }

        void EnsureStyles()
        {
            if (titleStyle == null)
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(18), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.85f, 0.4f) } };
            if (taskStyle == null)
                taskStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(24), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
        }
    }
}
