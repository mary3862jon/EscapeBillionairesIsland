using UnityEngine;

namespace Spoonacci
{
    // Procedural noise textures, generated once and cached.
    // Used to dress up materials so the world isn't all flat colors.
    public static class ProceduralTextures
    {
        static Texture2D _grass, _sand, _stone, _wood, _leaves, _water, _marble;

        public static Texture2D Grass  => _grass  ??= MakeGrass();
        public static Texture2D Sand   => _sand   ??= MakeSand();
        public static Texture2D Stone  => _stone  ??= MakeStone();
        public static Texture2D Wood   => _wood   ??= MakeWood();
        public static Texture2D Leaves => _leaves ??= MakeLeaves();
        public static Texture2D Water  => _water  ??= MakeWater();
        public static Texture2D Marble => _marble ??= MakeMarble();

        static Texture2D MakeGrass()
        {
            int N = 256;
            var px = new Color[N * N];
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float n  = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                float n2 = Mathf.PerlinNoise(x * 0.4f + 100f, y * 0.4f + 100f);
                float g = Mathf.Lerp(0.22f, 0.52f, n);
                float r = Mathf.Lerp(0.10f, 0.30f, n2 * 0.5f);
                float b = Mathf.Lerp(0.12f, 0.20f, n);
                // sparse darker blades
                if (Random.value < 0.005f) { r *= 0.5f; g *= 0.5f; b *= 0.5f; }
                px[y * N + x] = new Color(r, g, b);
            }
            return Bake(px, N, "GrassTex");
        }

        static Texture2D MakeSand()
        {
            int N = 256;
            var px = new Color[N * N];
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float n  = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                float grain = (Random.value - 0.5f) * 0.08f;
                float baseV = Mathf.Lerp(0.78f, 0.95f, n) + grain;
                float r = Mathf.Clamp01(baseV);
                float g = Mathf.Clamp01(baseV * 0.92f);
                float b = Mathf.Clamp01(baseV * 0.65f);
                px[y * N + x] = new Color(r, g, b);
            }
            return Bake(px, N, "SandTex");
        }

        static Texture2D MakeStone()
        {
            int N = 256;
            var px = new Color[N * N];
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float n  = Mathf.PerlinNoise(x * 0.06f, y * 0.06f);
                float n2 = Mathf.PerlinNoise(x * 0.2f + 50f, y * 0.2f + 50f);
                float v = Mathf.Lerp(0.35f, 0.65f, n) - n2 * 0.1f;
                // crack-like dark streaks via threshold
                if (n2 > 0.78f) v *= 0.55f;
                px[y * N + x] = new Color(v, v * 0.96f, v * 0.92f);
            }
            return Bake(px, N, "StoneTex");
        }

        static Texture2D MakeWood()
        {
            int N = 256;
            var px = new Color[N * N];
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float band = Mathf.Sin(y * 0.18f + Mathf.PerlinNoise(x * 0.03f, y * 0.06f) * 6f) * 0.5f + 0.5f;
                float v = Mathf.Lerp(0.35f, 0.6f, band);
                float r = v;
                float g = v * 0.6f;
                float b = v * 0.35f;
                if (Random.value < 0.01f) { r *= 0.6f; g *= 0.6f; b *= 0.6f; }
                px[y * N + x] = new Color(r, g, b);
            }
            return Bake(px, N, "WoodTex");
        }

        static Texture2D MakeLeaves()
        {
            int N = 128;
            var px = new Color[N * N];
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float n  = Mathf.PerlinNoise(x * 0.18f, y * 0.18f);
                float n2 = Mathf.PerlinNoise(x * 0.6f + 9f, y * 0.6f + 9f);
                float g = Mathf.Lerp(0.25f, 0.55f, n);
                float r = Mathf.Lerp(0.1f, 0.2f, n2);
                float b = Mathf.Lerp(0.1f, 0.18f, n);
                px[y * N + x] = new Color(r, g, b);
            }
            return Bake(px, N, "LeavesTex");
        }

        static Texture2D MakeWater()
        {
            int N = 128;
            var px = new Color[N * N];
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float w = Mathf.Sin(x * 0.4f) * 0.5f + Mathf.Sin(y * 0.35f + 1.3f) * 0.5f;
                float n = Mathf.PerlinNoise(x * 0.15f, y * 0.15f);
                float blue = Mathf.Lerp(0.42f, 0.78f, n + w * 0.05f);
                float green = blue * 0.78f;
                float red = blue * 0.18f;
                px[y * N + x] = new Color(red, green, blue);
            }
            return Bake(px, N, "WaterTex");
        }

        static Texture2D MakeMarble()
        {
            int N = 256;
            var px = new Color[N * N];
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.04f, y * 0.04f);
                float vein = Mathf.Abs(Mathf.Sin(n * 12f + y * 0.05f));
                float v = Mathf.Lerp(0.88f, 1.0f, n);
                if (vein > 0.85f) v *= 0.85f;
                px[y * N + x] = new Color(v, v * 0.98f, v * 0.96f);
            }
            return Bake(px, N, "MarbleTex");
        }

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
