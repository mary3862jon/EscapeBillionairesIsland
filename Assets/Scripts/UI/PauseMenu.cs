using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Spoonacci
{
    // Press ESC to pause. Shows: Resume / Save / Quit to Title / Quit to Desktop.
    // Spawned by every gameplay bootstrapper.
    public class PauseMenu : MonoBehaviour
    {
        bool paused;
        GUIStyle titleStyle, btnStyle, helpStyle, trackStyle;
        float savedTimeScale = 1f;

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.escapeKey.wasPressedThisFrame) Toggle();
        }

        public void Toggle()
        {
            paused = !paused;
            if (paused)
            {
                savedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Time.timeScale = savedTimeScale > 0f ? savedTimeScale : 1f;
            }
        }

        void OnGUI()
        {
            if (!paused) return;
            EnsureStyles();

            // dim background
            GUI.color = new Color(0f, 0f, 0f, 0.7f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // title
            float w = 480f, bh = 70f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.22f;

            GUI.Label(new Rect(x, y, w, 80f), "PAUSED", titleStyle);
            y += 100f;

            if (GUI.Button(new Rect(x, y, w, bh), "RESUME", btnStyle)) Toggle();
            y += bh + 14f;
            if (GUI.Button(new Rect(x, y, w, bh), "SAVE GAME (F5)", btnStyle))
            {
                SaveSystem.Instance.Save("Saved from menu");
            }
            y += bh + 14f;
            if (GUI.Button(new Rect(x, y, w, bh), "QUIT TO TITLE", btnStyle))
            {
                Time.timeScale = 1f;
                SoundFx.Instance.Chime();
                try { SceneManager.LoadScene("TitleScreen"); }
                catch { SceneManager.LoadScene(0); }
            }
            y += bh + 14f;
            if (GUI.Button(new Rect(x, y, w, bh), "QUIT TO DESKTOP", btnStyle))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }

            // help text + track info bottom
            float hy = Screen.height - 120f;
            GUI.Label(new Rect(0, hy, Screen.width, 28f), "[ESC] resume  ·  [F5] save  ·  [K/J] next/prev music  ·  [M] mute  ·  [+/-] volume", helpStyle);
            GUI.Label(new Rect(0, hy + 36f, Screen.width, 28f), "♪ Now playing:  " + MusicPlayer.Instance.CurrentTrack, trackStyle);
        }

        void EnsureStyles()
        {
            if (titleStyle == null)
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(60), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.95f, 0.7f) } };
            if (btnStyle == null)
                btnStyle = new GUIStyle(GUI.skin.button) { fontSize = UiScale.Font(26), fontStyle = FontStyle.Bold };
            if (helpStyle == null)
                helpStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(18), alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.85f, 0.85f, 0.85f) } };
            if (trackStyle == null)
                trackStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(18), alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.7f, 1f, 0.8f) } };
        }
    }
}
