using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Listens for keys 1-9. Swaps to that spoon type and rebuilds the player's visual.
    // Shows a 2-sec toast with the new type name.
    public class SpoonTypeSwitcher : MonoBehaviour
    {
        ProceduralSpoonBuilder builder;
        float toastUntil;
        string toast = "";
        GUIStyle style;

        void Awake() { builder = GetComponent<ProceduralSpoonBuilder>(); }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || builder == null) return;
            int idx = -1;
            if (kb.digit1Key.wasPressedThisFrame) idx = 0;
            else if (kb.digit2Key.wasPressedThisFrame) idx = 1;
            else if (kb.digit3Key.wasPressedThisFrame) idx = 2;
            else if (kb.digit4Key.wasPressedThisFrame) idx = 3;
            else if (kb.digit5Key.wasPressedThisFrame) idx = 4;
            else if (kb.digit6Key.wasPressedThisFrame) idx = 5;
            else if (kb.digit7Key.wasPressedThisFrame) idx = 6;
            else if (kb.digit8Key.wasPressedThisFrame) idx = 7;
            else if (kb.digit9Key.wasPressedThisFrame) idx = 8;
            if (idx < 0) return;

            if (idx >= SpoonTypeLibrary.All.Count) return;
            GameState.CurrentSpoonType = idx;
            builder.RebuildFromCurrentType();
            toast = "🥄 " + SpoonTypeLibrary.All[idx].name + "  (" + (idx + 1) + "/9)";
            toastUntil = Time.unscaledTime + 2f;
            SoundFx.Instance.Chime();
        }

        void OnGUI()
        {
            if (Time.unscaledTime > toastUntil) return;
            if (style == null)
                style = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(26), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.95f, 0.5f) } };
            var sh = new GUIStyle(style); sh.normal.textColor = Color.black;
            float w = 460f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.30f;
            GUI.Label(new Rect(x + 2, y + 2, w, 36f), toast, sh);
            GUI.Label(new Rect(x, y, w, 36f), toast, style);
        }
    }
}
