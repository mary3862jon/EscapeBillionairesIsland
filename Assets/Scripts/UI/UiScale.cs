using UnityEngine;

namespace Spoonacci
{
    // Single source of truth for IMGUI font scaling so high-res screens stay readable.
    // Reference: 1080p = 1.0x; 1440p = ~1.33x; 4K = 2.0x.
    public static class UiScale
    {
        public static float Factor => Mathf.Max(1f, Screen.height / 1080f);
        public static int Font(int baseSize) => Mathf.RoundToInt(baseSize * Factor);
    }
}
