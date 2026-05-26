using System.Collections.Generic;
using UnityEngine;

namespace Spoonacci
{
    // Catalog of every spoon skin available in Salon Cucchiaio.
    // Add freely — Salon cycles through them in order.
    public static class SpoonSkinLibrary
    {
        public struct Skin
        {
            public string name;
            public Color color;
            public float metallic;
            public float smoothness;
            public Skin(string n, Color c, float m, float s) { name = n; color = c; metallic = m; smoothness = s; }
        }

        public static readonly List<Skin> All = new List<Skin>
        {
            new Skin("Sir Spoonacci (Default Silver)", new Color(0.85f, 0.85f, 0.9f),  0.95f, 0.85f),
            new Skin("Gold of the Czar",               new Color(0.95f, 0.78f, 0.25f), 1.00f, 0.90f),
            new Skin("Polished Copper",                new Color(0.85f, 0.45f, 0.20f), 0.90f, 0.75f),
            new Skin("Obsidian Edition",               new Color(0.05f, 0.05f, 0.08f), 0.80f, 0.65f),
            new Skin("Rosé Goldè",                     new Color(0.95f, 0.65f, 0.55f), 0.95f, 0.85f),
            new Skin("Plastic Pete Tribute",           new Color(0.95f, 0.95f, 0.95f), 0.05f, 0.30f),
            new Skin("Wooden Heirloom",                new Color(0.55f, 0.35f, 0.20f), 0.00f, 0.20f),
            new Skin("Caviar Pearl",                   new Color(0.92f, 0.95f, 1.00f), 0.20f, 0.95f),
            new Skin("Crypto Chrome",                  new Color(0.40f, 0.95f, 0.85f), 0.95f, 0.95f),
            new Skin("Bonk Iron (combat)",             new Color(0.35f, 0.35f, 0.40f), 0.85f, 0.30f),
            new Skin("Salon Day-Spa Pink",             new Color(1.00f, 0.65f, 0.80f), 0.60f, 0.80f),
            new Skin("Tsarist Jewel-Encrusted",        new Color(0.60f, 0.20f, 0.85f), 0.95f, 0.95f),
        };
    }
}
