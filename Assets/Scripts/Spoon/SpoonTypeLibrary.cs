using System.Collections.Generic;
using UnityEngine;

namespace Spoonacci
{
    // 9 kitchen-spoon presets. Press 1–9 in-game to switch.
    // Each Type defines the proportions + material that ProceduralSpoonBuilder reads at build time.
    public struct SpoonType
    {
        public string name;
        public float handleHeight;
        public float handleRadius;
        public float bowlWidth;
        public float bowlThickness;
        public float bowlLength;
        public Color color;
        public float metallic;
        public float smoothness;
        public SpoonType(string n, float hh, float hr, float bw, float bt, float bl, Color c, float m, float s)
        {
            name = n; handleHeight = hh; handleRadius = hr;
            bowlWidth = bw; bowlThickness = bt; bowlLength = bl;
            color = c; metallic = m; smoothness = s;
        }
    }

    public static class SpoonTypeLibrary
    {
        // Proportions are now: handleHeight, handleRadius, bowlWidth (X side-to-side),
        // bowlDepth (Z thickness front-to-back), bowlVerticalLen (Y — bowl is IN LINE with the handle, taller than wide).
        public static readonly List<SpoonType> All = new List<SpoonType>
        {
            // 1 — Teaspoon
            new SpoonType("Teaspoon",      0.90f, 0.030f, 0.32f, 0.14f, 0.40f, new Color(0.80f, 0.80f, 0.85f), 0.55f, 0.55f),
            // 2 — Tablespoon
            new SpoonType("Tablespoon",    1.05f, 0.035f, 0.42f, 0.18f, 0.55f, new Color(0.80f, 0.80f, 0.85f), 0.55f, 0.55f),
            // 3 — Soup spoon (deeper rounder)
            new SpoonType("Soup Spoon",    1.00f, 0.035f, 0.48f, 0.24f, 0.50f, new Color(0.80f, 0.80f, 0.85f), 0.50f, 0.50f),
            // 4 — Dessert spoon (pointier oval)
            new SpoonType("Dessert Spoon", 1.00f, 0.032f, 0.38f, 0.16f, 0.55f, new Color(0.80f, 0.80f, 0.85f), 0.55f, 0.60f),
            // 5 — Coffee spoon (tiny)
            new SpoonType("Coffee Spoon",  0.75f, 0.024f, 0.24f, 0.10f, 0.30f, new Color(0.75f, 0.75f, 0.80f), 0.55f, 0.55f),
            // 6 — Ladle (long handle, deep round bowl)
            new SpoonType("Ladle",         1.40f, 0.045f, 0.55f, 0.40f, 0.55f, new Color(0.70f, 0.70f, 0.75f), 0.65f, 0.45f),
            // 7 — Wooden spoon
            new SpoonType("Wooden Spoon",  1.15f, 0.045f, 0.42f, 0.22f, 0.50f, new Color(0.55f, 0.35f, 0.18f), 0.00f, 0.20f),
            // 8 — Gold spoon
            new SpoonType("Gold Spoon",    1.05f, 0.035f, 0.42f, 0.18f, 0.55f, new Color(0.95f, 0.78f, 0.22f), 1.00f, 0.85f),
            // 9 — Plastic spoon
            new SpoonType("Plastic Spoon", 0.90f, 0.030f, 0.34f, 0.14f, 0.42f, new Color(0.96f, 0.96f, 0.98f), 0.05f, 0.30f),
        };
    }
}
