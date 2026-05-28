using UnityEngine;

namespace Spoonacci
{
    // Fixed-size world label rendered as an opaque sign panel.
    // - Hides itself when blocked by geometry (Linecast occlusion)
    // - Hides when too far away
    // - Black background plate so the text reads cleanly (no more "holographic show-through")
    public class WorldLabel : MonoBehaviour
    {
        public string text = "";
        public int fontSize = 24;
        public Color color = Color.black;
        public Vector3 worldOffset = Vector3.zero;
        public float maxVisibleDistance = 35f;

        GUIStyle style;
        Camera cam;

        void OnGUI()
        {
            if (string.IsNullOrEmpty(text)) return;
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            Vector3 wp = transform.position + transform.TransformVector(worldOffset);

            // distance cutoff
            float dist = Vector3.Distance(cam.transform.position, wp);
            if (dist > maxVisibleDistance) return;

            // occlusion test — Linecast from camera to label
            Vector3 from = cam.transform.position;
            Vector3 dir = wp - from;
            float len = dir.magnitude;
            if (len > 0.2f && Physics.Raycast(from, dir.normalized, out RaycastHit hit, len - 0.2f, ~0, QueryTriggerInteraction.Ignore))
            {
                // something is between camera and the label — don't show
                return;
            }

            // project to screen
            Vector3 sp = cam.WorldToScreenPoint(wp);
            if (sp.z < 0f) return;

            string display = Loc.T(text);
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

            // measure text width to pick a snug background plate
            GUIContent content = new GUIContent(display);
            Vector2 sz = style.CalcSize(content);
            float pad = 18f;
            float w = sz.x + pad * 2f;
            float h = sz.y + pad;
            var rect = new Rect(sp.x - w * 0.5f, Screen.height - sp.y - h * 0.5f, w, h);

            // opaque sign-like background
            var prev = GUI.color;
            GUI.color = new Color(0.05f, 0.05f, 0.08f, 1f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            // gold border
            GUI.color = new Color(0.85f, 0.7f, 0.25f, 1f);
            float b = 2f;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, b), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - b, rect.width, b), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, b, rect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - b, rect.y, b, rect.height), Texture2D.whiteTexture);
            GUI.color = prev;

            GUI.Label(rect, display, style);
        }
    }
}
