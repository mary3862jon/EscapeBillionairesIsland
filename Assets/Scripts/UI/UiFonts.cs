using UnityEngine;

namespace Spoonacci
{
    // Loads a strong, bold OS font so the whole game stops using Unity's thin
    // default IMGUI font. Picks the first available from a preference list
    // (heavy/condensed sans → clean fallbacks). Cached; null-safe (falls back to
    // the built-in font if none resolve, e.g. a stripped headless build).
    public static class UiFonts
    {
        static Font _bold;
        static bool _tried;

        public static Font Bold
        {
            get
            {
                if (_tried) return _bold;
                _tried = true;
                string[] prefs =
                {
                    "Bahnschrift",            // modern condensed (Win 10/11)
                    "Arial Black", "Impact",  // heavy display
                    "Franklin Gothic Heavy",
                    "Segoe UI Black", "Segoe UI Semibold", "Segoe UI",
                    "Tahoma", "Verdana", "Arial", "Liberation Sans",
                };
                try { _bold = Font.CreateDynamicFontFromOSFont(prefs, 40); }
                catch { _bold = null; }
                return _bold;
            }
        }
    }

    // Installs the bold font as the global IMGUI font at the START of every frame's
    // OnGUI (very low execution order), so every GUIStyle that doesn't override its
    // own font inherits it — i.e. AAA text EVERYWHERE with one component.
    [DefaultExecutionOrder(-10000)]
    public class GuiFontInstaller : MonoBehaviour
    {
        void OnGUI()
        {
            var f = UiFonts.Bold;
            if (f != null) GUI.skin.font = f;
        }
    }
}
