using UnityEngine;

namespace Spoonacci
{
    // High-detail procedural surface textures for the boss arenas.
    // Mirrors ProceduralTextures.cs's style: per-pixel Color[] built in a Make*()
    // then baked via a shared Bake(). Lazily generated and cached.
    //
    // All textures tile (wrapMode = Repeat) and are authored to be seamless-ish:
    // noise is sampled with frequencies that wrap reasonably across the 256px span,
    // and bands/grids use the pixel index modulo so the seam edges line up.
    public static class BossTextures
    {
        static Texture2D _brushed, _carbon, _concrete, _rust, _hazard, _tarmac;

        public static Texture2D BrushedMetal => _brushed  ??= MakeBrushedMetal();
        public static Texture2D CarbonFiber  => _carbon   ??= MakeCarbonFiber();
        public static Texture2D Concrete     => _concrete ??= MakeConcrete();
        public static Texture2D RustedSteel  => _rust     ??= MakeRustedSteel();
        public static Texture2D HazardStripe => _hazard   ??= MakeHazardStripe();
        public static Texture2D Tarmac       => _tarmac   ??= MakeTarmac();

        // Cool grey steel with strong horizontal directional streaks (brushing) +
        // fine vertical grain so it catches light anisotropically.
        static Texture2D MakeBrushedMetal()
        {
            int N = 256;
            var px = new Color[N * N];
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                // long horizontal streaks: low-freq in X, high-freq in Y
                float streak = Mathf.PerlinNoise(x * 0.012f, y * 0.55f);
                // fine per-row jitter so the brushing isn't a clean sine
                float grain = (Mathf.PerlinNoise(x * 0.9f, y * 0.05f) - 0.5f) * 0.10f;
                float micro = (Random.value - 0.5f) * 0.05f;
                float v = Mathf.Lerp(0.46f, 0.74f, streak) + grain + micro;
                v = Mathf.Clamp01(v);
                // very slight cool tint
                px[y * N + x] = new Color(v * 0.97f, v * 0.99f, v * 1.0f);
            }
            return Bake(px, N, "BrushedMetalTex");
        }

        // Woven carbon-fibre twill: a 2x2 over/under checker of dark glossy cells
        // with a subtle diagonal weave highlight.
        static Texture2D MakeCarbonFiber()
        {
            int N = 256;
            var px = new Color[N * N];
            int cell = 16;          // weave cell size
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                int cx = (x / cell) & 1;
                int cy = (y / cell) & 1;
                bool warp = (cx ^ cy) == 0;   // which thread is on top

                // position inside the cell → rounded thread highlight
                float fx = (x % cell) / (float)cell - 0.5f;
                float fy = (y % cell) / (float)cell - 0.5f;
                // highlight runs along the thread direction (diagonal twill look)
                float along = warp ? fx : fy;
                float across = warp ? fy : fx;
                float hi = Mathf.Cos(across * Mathf.PI) * 0.5f + 0.5f;   // bright at thread centre
                hi *= 1f - Mathf.Abs(along) * 0.4f;

                float baseV = warp ? 0.10f : 0.07f;
                float v = baseV + hi * 0.14f;
                // faint blue sheen on the highlight
                px[y * N + x] = new Color(v, v, v + hi * 0.04f);
            }
            return Bake(px, N, "CarbonFiberTex");
        }

        // Pale grey concrete: speckled aggregate + occasional dark hairline cracks +
        // faint stain blotches.
        static Texture2D MakeConcrete()
        {
            int N = 256;
            var px = new Color[N * N];
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float blotch = Mathf.PerlinNoise(x * 0.03f, y * 0.03f);
                float mid    = Mathf.PerlinNoise(x * 0.12f + 30f, y * 0.12f + 30f);
                float v = Mathf.Lerp(0.55f, 0.72f, blotch) - mid * 0.06f;
                // aggregate speckle
                float r = Random.value;
                if (r < 0.04f) v -= 0.12f;
                else if (r > 0.97f) v += 0.10f;
                // cracks: thin dark filaments from a high-freq ridged noise
                float crack = Mathf.PerlinNoise(x * 0.07f + 200f, y * 0.07f + 200f);
                if (crack > 0.80f && crack < 0.83f) v *= 0.45f;
                v = Mathf.Clamp01(v);
                px[y * N + x] = new Color(v, v * 0.99f, v * 0.96f);
            }
            return Bake(px, N, "ConcreteTex");
        }

        // Mottled orange-brown rust over a dark steel base; patchy, with darker pits.
        static Texture2D MakeRustedSteel()
        {
            int N = 256;
            var px = new Color[N * N];
            Color steel = new Color(0.28f, 0.29f, 0.31f);
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float patch = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                float fine  = Mathf.PerlinNoise(x * 0.25f + 70f, y * 0.25f + 70f);
                float rustAmt = Mathf.Clamp01((patch * 0.7f + fine * 0.5f) - 0.35f) * 1.6f;
                rustAmt = Mathf.Clamp01(rustAmt);
                // rust colour varies orange→brown with fine noise
                Color rust = Color.Lerp(new Color(0.55f, 0.28f, 0.11f),
                                        new Color(0.72f, 0.42f, 0.18f), fine);
                Color c = Color.Lerp(steel, rust, rustAmt);
                // dark pits
                if (Random.value < 0.03f) c *= 0.6f;
                px[y * N + x] = c;
            }
            return Bake(px, N, "RustedSteelTex");
        }

        // Crisp diagonal yellow/black warning stripes with a touch of edge grime.
        static Texture2D MakeHazardStripe()
        {
            int N = 256;
            var px = new Color[N * N];
            Color yellow = new Color(0.92f, 0.74f, 0.06f);
            Color black  = new Color(0.06f, 0.06f, 0.06f);
            int band = 32;                 // stripe period along the diagonal
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                // diagonal coordinate; +N keeps it positive so the seam wraps cleanly
                int diag = (x + y) % band;
                bool isYellow = diag < band / 2;
                Color c = isYellow ? yellow : black;
                // grime/wear: slight darkening noise + scuffs
                float wear = Mathf.PerlinNoise(x * 0.15f, y * 0.15f);
                c *= Mathf.Lerp(0.82f, 1f, wear);
                if (Random.value < 0.02f) c *= 0.7f;        // scuff specks
                px[y * N + x] = c;
            }
            return Bake(px, N, "HazardStripeTex");
        }

        // Dark asphalt tarmac: fine gravel speckle over a near-black base, with
        // faint lighter patches where it's worn.
        static Texture2D MakeTarmac()
        {
            int N = 256;
            var px = new Color[N * N];
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float wear = Mathf.PerlinNoise(x * 0.04f, y * 0.04f);
                float v = Mathf.Lerp(0.085f, 0.16f, wear);
                // gravel grain
                float g = Random.value;
                if (g < 0.06f) v += 0.10f;          // light aggregate stones
                else if (g > 0.96f) v -= 0.04f;     // tar pools
                v = Mathf.Clamp01(v);
                px[y * N + x] = new Color(v, v, v * 1.02f);
            }
            return Bake(px, N, "TarmacTex");
        }

        // Shared bake — same settings ProceduralTextures uses (mip + repeat + aniso).
        static Texture2D Bake(Color[] px, int n, string name)
        {
            var t = new Texture2D(n, n, TextureFormat.RGB24, true)
            {
                name = name,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Trilinear,
                anisoLevel = 4,
            };
            t.SetPixels(px);
            t.Apply(true, false);
            return t;
        }
    }
}
