using System.Collections.Generic;
using UnityEngine;

namespace Spoonacci
{
    // Top-down minimap (top-right corner). Shows the player plus a blip for every
    // billionaire target so none of them can ever be "impossible to find" again.
    // Blip is gold while the target is still active, dimmed once bonked.
    public class Minimap : MonoBehaviour
    {
        public float worldHalfExtent = 125f; // island spans roughly +/-125
        public float size = 200f;            // on-screen square size (pixels, pre-scale)
        public float margin = 16f;

        Transform player;
        readonly List<BillionaireNPC> targets = new List<BillionaireNPC>();
        float nextScan;
        GUIStyle blipStyle;
        Texture2D dot;

        void EnsureRefs()
        {
            if (player == null)
            {
                var sc = Object.FindFirstObjectByType<SpoonController>();
                if (sc != null) player = sc.transform;
            }
            if (Time.time > nextScan)
            {
                nextScan = Time.time + 1.5f;
                targets.Clear();
                targets.AddRange(Object.FindObjectsByType<BillionaireNPC>(FindObjectsSortMode.None));
            }
        }

        void OnGUI()
        {
            EnsureRefs();
            if (dot == null) dot = Texture2D.whiteTexture;

            float s = size * UiScale.Factor;
            float x0 = Screen.width - s - margin;
            float y0 = margin;
            var panel = new Rect(x0, y0, s, s);

            // backdrop + border
            var prev = GUI.color;
            GUI.color = new Color(0.04f, 0.07f, 0.10f, 0.72f);
            GUI.DrawTexture(panel, dot);
            GUI.color = new Color(0.85f, 0.7f, 0.25f, 1f);
            float b = 2f;
            GUI.DrawTexture(new Rect(panel.x, panel.y, panel.width, b), dot);
            GUI.DrawTexture(new Rect(panel.x, panel.yMax - b, panel.width, b), dot);
            GUI.DrawTexture(new Rect(panel.x, panel.y, b, panel.height), dot);
            GUI.DrawTexture(new Rect(panel.xMax - b, panel.y, b, panel.height), dot);
            GUI.color = prev;

            if (blipStyle == null)
                blipStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = UiScale.Font(11),
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

            // map a world XZ position into the panel rect
            Vector2 ToMap(Vector3 w)
            {
                float u = Mathf.Clamp01((w.x / worldHalfExtent) * 0.5f + 0.5f);
                float v = Mathf.Clamp01((w.z / worldHalfExtent) * 0.5f + 0.5f);
                // z up = north = top of map
                return new Vector2(panel.x + u * s, panel.yMax - v * s);
            }

            // billionaire blips
            foreach (var t in targets)
            {
                if (t == null) continue;
                bool bonked = BillionaireRegistry.IsBonked(t.billionaireName);
                Vector2 p = ToMap(t.transform.position);
                float d = 12f * UiScale.Factor;
                GUI.color = bonked ? new Color(0.45f, 0.45f, 0.5f, 0.9f) : new Color(0.95f, 0.78f, 0.2f, 1f);
                GUI.DrawTexture(new Rect(p.x - d * 0.5f, p.y - d * 0.5f, d, d), dot);
                GUI.color = Color.black;
                string initial = string.IsNullOrEmpty(t.billionaireName) ? "?" : t.billionaireName.Substring(0, 1);
                GUI.Label(new Rect(p.x - d, p.y - d * 0.5f, d * 2f, d), initial, blipStyle);
            }

            // player blip (drawn last, on top)
            if (player != null)
            {
                Vector2 p = ToMap(player.position);
                float d = 10f * UiScale.Factor;
                GUI.color = new Color(0.3f, 1f, 0.6f, 1f);
                GUI.DrawTexture(new Rect(p.x - d * 0.5f, p.y - d * 0.5f, d, d), dot);
            }

            GUI.color = prev;
        }
    }
}
