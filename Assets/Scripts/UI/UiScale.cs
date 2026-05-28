using UnityEngine;

namespace Spoonacci
{
    // Single source of truth for IMGUI font scaling so high-res screens stay readable.
    // Reference: 1080p = 1.0x; 1440p = ~1.33x; 4K = 2.0x.
    public static class UiScale
    {
        // 720p → ~0.85x ; 1080p → 1.0x ; 1440p → 1.22x ; 4K → 1.55x
        public static float Factor => Mathf.Clamp(0.5f + Screen.height / 2160f, 0.85f, 1.6f);
        public static int Font(int baseSize) => Mathf.RoundToInt(baseSize * Factor);
    }
}
