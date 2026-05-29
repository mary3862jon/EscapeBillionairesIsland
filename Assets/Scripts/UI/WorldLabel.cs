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
        // The thing this label describes. Occlusion is tested against THIS, so a name
        // floating above a head doesn't peek over the wall its owner is hidden behind.
        // Defaults to the parent (the object the label is attached to).
        public Transform anchor;

        GUIStyle style;
        Camera cam;
        static readonly RaycastHit[] hits = new RaycastHit[16];

        // True if solid WORLD geometry (a wall/building) sits between camera and point p.
        // Ignores characters (Civilians/NPCs/the player) and the label's own object so a
        // body standing in the open isn't reported as "blocked" by its own collider.
        bool Blocked(Vector3 from, Vector3 p, Transform self)
        {
            Vector3 dir = p - from;
            float len = dir.magnitude;
            if (len <= 0.25f) return false;
            int n = Physics.RaycastNonAlloc(from, dir.normalized, hits, len - 0.25f, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                var t = hits[i].collider.transform;
                if (self != null && (t == self || t.IsChildOf(self))) continue;     // the labelled object
                if (t.GetComponentInParent<Civilian>() != null) continue;            // any human/billionaire
                if (t.GetComponentInParent<SpoonController>() != null) continue;      // the player spoon
                return true; // a real wall/building/prop is in the way
            }
            return false;
        }

        void OnGUI()
        {
            if (string.IsNullOrEmpty(text)) return;
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            Vector3 wp = transform.position + transform.TransformVector(worldOffset);

            // distance cutoff
            float dist = Vector3.Distance(cam.transform.position, wp);
            if (dist > maxVisibleDistance) return;

            // Occlusion — hide if the OBJECT (anchor body) is behind geometry, even if the
            // elevated text point itself has clear line of sight over the wall. This is what
            // kills the "names readable through walls" show-through.
            Vector3 from = cam.transform.position;
            Transform a = anchor != null ? anchor : (transform.parent != null ? transform.parent : transform);
            Vector3 bodyPoint = a.position + Vector3.up * 1.0f;
            // Hide if the object's BODY is behind a wall — even when the elevated text point
            // itself peeks over the top. This is what stops names reading through walls.
            if (Blocked(from, bodyPoint, a))
                return;

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
