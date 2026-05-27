using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Spoonacci
{
    // ESC pauses. Sub-pages: Main, Settings, Exit confirm.
    enum PausePage { Main, Settings, ExitConfirm }

    public class PauseMenu : MonoBehaviour
    {
        bool paused;
        PausePage page = PausePage.Main;
        GUIStyle titleStyle, btnStyle, helpStyle, trackStyle, labelStyle, valueStyle;
        float savedTimeScale = 1f;

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.escapeKey.wasPressedThisFrame)
            {
                if (paused && page != PausePage.Main) { page = PausePage.Main; return; }
                Toggle();
            }
        }

        public void Toggle()
        {
            paused = !paused;
            page = PausePage.Main;
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
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.78f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = prev;

            switch (page)
            {
                case PausePage.Main:        DrawMain(); break;
                case PausePage.Settings:    DrawSettings(); break;
                case PausePage.ExitConfirm: DrawExit(); break;
            }
        }

        void DrawMain()
        {
            float w = 480f, bh = 70f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.18f;

            GUI.Label(new Rect(x, y, w, 80f), Loc.T("pause.title"), titleStyle);
            y += 110f;

            if (GUI.Button(new Rect(x, y, w, bh), "▶  " + Loc.T("pause.resume"), btnStyle)) Toggle();
            y += bh + 16f;
            if (GUI.Button(new Rect(x, y, w, bh), "💾  " + Loc.T("pause.save"), btnStyle))
            {
                SaveSystem.Instance.Save("Saved from menu");
            }
            y += bh + 16f;
            if (GUI.Button(new Rect(x, y, w, bh), "⚙  " + Loc.T("pause.settings"), btnStyle))
                page = PausePage.Settings;
            y += bh + 16f;
            if (GUI.Button(new Rect(x, y, w, bh), "🚪  " + Loc.T("pause.exit"), btnStyle))
                page = PausePage.ExitConfirm;

            // help + track
            float hy = Screen.height - 90f;
            GUI.Label(new Rect(0, hy, Screen.width, 28f), Loc.T("pause.help"), helpStyle);
            GUI.Label(new Rect(0, hy + 36f, Screen.width, 28f), Loc.T("pause.now_playing") + "  " + MusicPlayer.Instance.CurrentTrack, trackStyle);
        }

        void DrawSettings()
        {
            float w = 620f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.10f;

            GUI.Label(new Rect(x, y, w, 50f), "⚙  " + Loc.T("settings.title"), titleStyle);
            y += 80f;

            // Resolution
            DrawCycleRow(x, y, w, Loc.T("settings.resolution"),
                SettingsManager.Resolutions[SettingsManager.ResolutionIndex].label,
                () => { SettingsManager.ResolutionIndex = (SettingsManager.ResolutionIndex - 1 + SettingsManager.Resolutions.Length) % SettingsManager.Resolutions.Length; SettingsManager.Save(); },
                () => { SettingsManager.ResolutionIndex = (SettingsManager.ResolutionIndex + 1) % SettingsManager.Resolutions.Length; SettingsManager.Save(); });
            y += 56f;

            // Frame Rate
            DrawCycleRow(x, y, w, Loc.T("settings.framerate"),
                SettingsManager.FrameRateLabel(SettingsManager.FrameRates[SettingsManager.FrameRateIndex]) + " FPS",
                () => { SettingsManager.FrameRateIndex = (SettingsManager.FrameRateIndex - 1 + SettingsManager.FrameRates.Length) % SettingsManager.FrameRates.Length; SettingsManager.Save(); },
                () => { SettingsManager.FrameRateIndex = (SettingsManager.FrameRateIndex + 1) % SettingsManager.FrameRates.Length; SettingsManager.Save(); });
            y += 56f;

            // Fullscreen toggle
            DrawCycleRow(x, y, w, Loc.T("settings.fullscreen"),
                SettingsManager.Fullscreen ? Loc.T("common.on") : Loc.T("common.off"),
                () => { SettingsManager.Fullscreen = !SettingsManager.Fullscreen; SettingsManager.Save(); },
                () => { SettingsManager.Fullscreen = !SettingsManager.Fullscreen; SettingsManager.Save(); });
            y += 56f;

            // Sliders
            SettingsManager.MasterVolume   = DrawSliderRow(x, y, w, Loc.T("settings.master"),   SettingsManager.MasterVolume);   y += 52f;
            SettingsManager.MusicVolume    = DrawSliderRow(x, y, w, Loc.T("settings.music"),    SettingsManager.MusicVolume);    y += 52f;
            SettingsManager.SfxVolume      = DrawSliderRow(x, y, w, Loc.T("settings.sfx"),      SettingsManager.SfxVolume);      y += 52f;
            SettingsManager.DialogueVolume = DrawSliderRow(x, y, w, Loc.T("settings.dialogue"), SettingsManager.DialogueVolume); y += 52f;

            // FOV slider 50-90
            SettingsManager.Fov = Mathf.Round(DrawSliderRow(x, y, w, Loc.T("settings.fov"), (SettingsManager.Fov - 50f) / 40f) * 40f + 50f);
            y += 52f;

            // apply live (sliders) — saves on debounce-less every change for simplicity
            SettingsManager.Apply();

            // back button
            if (GUI.Button(new Rect(x + (w - 280f) * 0.5f, y + 12f, 280f, 60f), "← " + Loc.T("common.back"), btnStyle))
            {
                SettingsManager.Save();
                page = PausePage.Main;
            }
        }

        void DrawCycleRow(float x, float y, float w, string label, string value, System.Action onLeft, System.Action onRight)
        {
            GUI.Label(new Rect(x, y + 4f, 240f, 40f), label, labelStyle);
            if (GUI.Button(new Rect(x + 240f, y, 50f, 44f), "◀", btnStyle)) onLeft();
            GUI.Label(new Rect(x + 290f, y + 4f, w - 290f - 50f, 40f), value, valueStyle);
            if (GUI.Button(new Rect(x + w - 50f, y, 50f, 44f), "▶", btnStyle)) onRight();
        }

        float DrawSliderRow(float x, float y, float w, string label, float v01)
        {
            GUI.Label(new Rect(x, y + 4f, 240f, 40f), label, labelStyle);
            float newV = GUI.HorizontalSlider(new Rect(x + 240f, y + 14f, w - 240f - 80f, 22f), v01, 0f, 1f);
            GUI.Label(new Rect(x + w - 80f, y + 4f, 80f, 40f), Mathf.RoundToInt(newV * 100f) + "%", valueStyle);
            return newV;
        }

        void DrawExit()
        {
            float w = 480f, bh = 70f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.32f;

            GUI.Label(new Rect(x, y, w, 60f), Loc.T("pause.exit"), titleStyle);
            y += 90f;

            if (GUI.Button(new Rect(x, y, w, bh), "🏠  " + Loc.T("pause.to_title"), btnStyle))
            {
                Time.timeScale = 1f;
                SettingsManager.Save();
                SoundFx.Instance.Chime();
                try { SceneManager.LoadScene("TitleScreen"); }
                catch { SceneManager.LoadScene(0); }
            }
            y += bh + 16f;
            if (GUI.Button(new Rect(x, y, w, bh), "🛑  " + Loc.T("pause.to_quit"), btnStyle))
            {
                SettingsManager.Save();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            y += bh + 30f;
            if (GUI.Button(new Rect(x + (w - 240f) * 0.5f, y, 240f, 50f), "← " + Loc.T("common.back"), btnStyle))
                page = PausePage.Main;
        }

        void EnsureStyles()
        {
            if (titleStyle == null)
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(48), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.95f, 0.7f) } };
            if (btnStyle == null)
                btnStyle = new GUIStyle(GUI.skin.button) { fontSize = UiScale.Font(22), fontStyle = FontStyle.Bold };
            if (helpStyle == null)
                helpStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(16), alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.85f, 0.85f, 0.85f) } };
            if (trackStyle == null)
                trackStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(16), alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.7f, 1f, 0.8f) } };
            if (labelStyle == null)
                labelStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(20), fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            if (valueStyle == null)
                valueStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(20), alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.85f, 0.4f) } };
        }
    }
}
