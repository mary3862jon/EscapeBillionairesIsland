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
            float h = 82f;
            float x = 20f;
            float y = 70f;

            UiTheme.Panel(new Rect(x, y, w, h));
            // a gold left tab marks this as the primary objective
            UiTheme.Accent(new Rect(x + 1f, y + 14f, 3f, h - 28f), UiTheme.Gold);
            UiTheme.Label(new Rect(x + 18f, y + 9f, w - 28f, 24f), Loc.T("quest.current"), titleStyle);
            UiTheme.Rule(x + 18f, y + 34f, w - 36f);
            UiTheme.Label(new Rect(x + 18f, y + 40f, w - 34f, 36f), task, taskStyle);
        }

        void EnsureStyles()
        {
            if (titleStyle == null)
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(15), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = UiTheme.GoldSoft } };
            if (taskStyle == null)
                taskStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(23), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = UiTheme.TextMain } };
        }
    }
}
