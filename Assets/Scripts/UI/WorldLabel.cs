using UnityEngine;

namespace Spoonacci
{
    // Floating world-space text drawn via OnGUI projection.
    // Used for signage on buildings (Salon, Bar, etc).
    public class WorldLabel : MonoBehaviour
    {
        public string text = "";
        public int fontSize = 24;
        public Color color = Color.black;
        public Vector3 worldOffset = Vector3.zero;

        GUIStyle style;
        Camera cam;

        void OnGUI()
        {
            if (string.IsNullOrEmpty(text)) return;
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = UiScale.Font(fontSize),
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = color }
                };
            }

            Vector3 wp = transform.position + transform.TransformVector(worldOffset);
            Vector3 sp = cam.WorldToScreenPoint(wp);
            if (sp.z < 0f) return;

            // Loc.T returns the key as-is when not found, so literal labels still render unchanged.
            string display = Loc.T(text);

            float w = 360f, h = 40f;
            var r = new Rect(sp.x - w * 0.5f, Screen.height - sp.y - h * 0.5f, w, h);
            var shadow = new GUIStyle(style);
            shadow.normal.textColor = new Color(1f, 1f, 1f, 0.85f);
            GUI.Label(new Rect(r.x + 2, r.y + 2, w, h), display, shadow);
            GUI.Label(r, display, style);
        }
    }
}
