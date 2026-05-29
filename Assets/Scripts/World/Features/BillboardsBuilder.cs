using UnityEngine;
using System.Collections.Generic;

namespace Spoonacci
{
    // Places satirical billboards / signage along the main roads.
    // Each billboard = two posts (solid cylinders) holding a bright panel (cube)
    // that faces the road, with a literal satirical Label slapped on it.
    public static class BillboardsBuilder
    {
        private static readonly string[] _texts = new string[]
        {
            "LIVE LAUGH LAUNDER",
            "BILLIONAIRES ONLY",
            "TAX IS FOR THE LITTLE PEOPLE",
            "BUY THE DIP (of caviar)",
            "NEW: yacht insurance",
            "VOTE: nobody",
            "TRICKLE-DOWN: still trickling",
            "YOUR RENT, OUR ROCKET",
        };

        private static readonly Color[] _panelColors = new Color[]
        {
            new Color(0.93f, 0.20f, 0.28f),  // hot red
            new Color(0.10f, 0.55f, 0.95f),  // electric blue
            new Color(0.98f, 0.78f, 0.10f),  // gold
            new Color(0.20f, 0.78f, 0.45f),  // money green
            new Color(0.78f, 0.20f, 0.85f),  // magenta
            new Color(0.98f, 0.45f, 0.05f),  // orange
            new Color(0.05f, 0.75f, 0.80f),  // teal
            new Color(0.55f, 0.30f, 0.90f),  // violet
        };

        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("Billboards", Vector3.zero).transform;
            hub.SetParent(root, true);

            // Sidewalk slots already sit off the road, with yaw facing the road.
            List<WorldLayout.Slot> slots = WorldLayout.SidewalkSlots(22f, 8.5f);
            if (slots == null || slots.Count == 0) return;

            // Spread requested ~8 billboards evenly across the available slots so
            // they don't all bunch up on one road.
            int want = Mathf.Min(_texts.Length, slots.Count);
            float step = (float)slots.Count / want;

            int built = 0;
            for (int i = 0; i < want; i++)
            {
                int idx = Mathf.Clamp(Mathf.RoundToInt(i * step), 0, slots.Count - 1);
                WorldLayout.Slot slot = slots[idx];

                string text = _texts[built % _texts.Length];
                Color panelCol = _panelColors[built % _panelColors.Length];

                BuildOneBillboard(hub, slot, text, panelCol, built);
                built++;
            }
        }

        private static void BuildOneBillboard(Transform parent, WorldLayout.Slot slot, string text, Color panelCol, int index)
        {
            // Container at the slot, rotated so +Z (panel front) faces the road.
            var go = BuildKit.Root("Billboard_" + index, slot.pos);
            go.transform.SetParent(parent, true);
            go.transform.rotation = Quaternion.Euler(0f, slot.yaw, 0f);
            Transform t = go.transform;

            Color postCol = new Color(0.18f, 0.18f, 0.20f); // dark metal posts

            // ---- Two posts (cylinders, KEEP collider — solid obstacles) ----
            float postH = 3.6f;
            float postHalfSpan = 2.2f; // posts spaced left/right along local X
            var postL = BuildKit.Cylinder(
                "PostL", t,
                new Vector3(-postHalfSpan, postH * 0.5f, 0f),
                new Vector3(0.28f, postH * 0.5f, 0.28f),
                postCol, 0.85f, 0.45f);
            var postR = BuildKit.Cylinder(
                "PostR", t,
                new Vector3(postHalfSpan, postH * 0.5f, 0f),
                new Vector3(0.28f, postH * 0.5f, 0.28f),
                postCol, 0.85f, 0.45f);

            // ---- Panel (cube) held aloft, facing the road (+Z local) ----
            float panelW = 5.4f;
            float panelH = 2.6f;
            float panelY = postH + panelH * 0.5f - 0.5f; // top of posts, overlapping a touch
            float panelThick = 0.18f;

            // Frame behind the panel (slightly larger, dark) — KEEP collider, solid sign.
            BuildKit.Cube(
                "Frame", t,
                new Vector3(0f, panelY, -0.04f),
                new Vector3(panelW * 0.5f + 0.18f, panelH * 0.5f + 0.18f, panelThick * 0.5f),
                new Color(0.10f, 0.10f, 0.12f), 0.6f, 0.3f);

            var panel = BuildKit.Cube(
                "Panel", t,
                new Vector3(0f, panelY, 0f),
                new Vector3(panelW * 0.5f, panelH * 0.5f, panelThick * 0.5f),
                panelCol, 0.1f, 0.65f);

            // ---- Label on the panel front (local +Z), childed to the panel ----
            // Label is occlusion-aware; offset it just in front of the panel face
            // so the text reads from the road side.
            BuildKit.Label(panel, text, Color.white, 60, new Vector3(0f, 0f, panelThick + 0.06f));
        }
    }
}
