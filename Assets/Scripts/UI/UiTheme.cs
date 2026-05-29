using UnityEngine;

namespace Spoonacci
{
    // ─────────────────────────────────────────────────────────────────────────
    //  UiTheme — one cohesive look for every IMGUI panel in the game.
    //
    //  Instead of flat opaque black rectangles, panels are drawn as rounded,
    //  semi-transparent "cards": a soft drop-shadow, a translucent slate-navy
    //  body, and a thin warm-gold border baked straight into the texture so the
    //  corners stay crisp at any size (9-slice). The palette matches the warm,
    //  tropical, sunset-gold mood of the island so the HUD reads as part of the
    //  world rather than something stapled on top of it.
    //
    //  Usage:
    //      UiTheme.Panel(rect);                 // card with shadow + gold border
    //      UiTheme.Card(rect);                  // lighter card, no big shadow (world labels)
    //      UiTheme.Label(rect, txt, style);     // text with a soft drop-shadow
    //      UiTheme.Rule(x, y, w);               // a thin gold divider line
    // ─────────────────────────────────────────────────────────────────────────
    public static class UiTheme
    {
        // ---- palette ---------------------------------------------------------
        public static readonly Color Gold      = new Color(0.96f, 0.78f, 0.36f, 1f);
        public static readonly Color GoldSoft  = new Color(1.00f, 0.87f, 0.52f, 1f);
        public static readonly Color TextMain  = new Color(0.97f, 0.96f, 0.92f, 1f);
        public static readonly Color TextDim   = new Color(0.72f, 0.74f, 0.78f, 1f);
        public static readonly Color TextDone  = new Color(0.55f, 0.93f, 0.62f, 1f);
        public static readonly Color TextWarn  = new Color(1.00f, 0.62f, 0.30f, 1f);

        // body fill (translucent slate-navy) + warm gold hairline border
        static readonly Color BodyFill   = new Color(0.055f, 0.075f, 0.105f, 0.90f);
        static readonly Color BodyBorder = new Color(0.96f, 0.78f, 0.36f, 0.85f);
        // lighter card for floating world labels (a touch warmer, more opaque)
        static readonly Color CardFill   = new Color(0.07f, 0.085f, 0.11f, 0.94f);

        static Texture2D _card, _cardLite, _shadow, _solid;
        static GUIStyle _cardStyle, _cardLiteStyle, _shadowStyle;

        // ---- public draw helpers --------------------------------------------

        // Full HUD panel: soft drop-shadow + rounded translucent body + gold border.
        // NOTE: we draw via GUI.Box (not GUIStyle.Draw) so it's safe in every GUI
        // event, not just Repaint — calling .Draw() directly floods the console with
        // "control 0 ... not a repaint event" errors every frame and freezes the game.
        public static void Panel(Rect r)
        {
            Ensure();
            var prev = GUI.color;
            GUI.color = Color.white;
            // drop shadow, nudged down a touch so the card looks lifted
            var sr = new Rect(r.x - 10f, r.y - 6f, r.width + 20f, r.height + 22f);
            GUI.Box(sr, GUIContent.none, _shadowStyle);
            GUI.Box(r, GUIContent.none, _cardStyle);
            GUI.color = prev;
        }

        // Lighter card for world-space labels — no heavy shadow (cheaper, lots on screen).
        public static void Card(Rect r)
        {
            Ensure();
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.35f);
            GUI.DrawTexture(new Rect(r.x + 2f, r.y + 3f, r.width, r.height), _solid); // contact shadow
            GUI.color = Color.white;
            GUI.Box(r, GUIContent.none, _cardLiteStyle);
            GUI.color = prev;
        }

        // A thin gold divider line.
        public static void Rule(float x, float y, float w)
        {
            Ensure();
            var prev = GUI.color;
            GUI.color = new Color(Gold.r, Gold.g, Gold.b, 0.40f);
            GUI.DrawTexture(new Rect(x, y, w, 1f), _solid);
            GUI.color = prev;
        }

        // A short solid accent block (e.g. a left tab) tinted by `c`.
        public static void Accent(Rect r, Color c)
        {
            Ensure();
            var prev = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, _solid);
            GUI.color = prev;
        }

        // Text with a 1px drop-shadow for legibility over any background.
        public static void Label(Rect r, string text, GUIStyle s)
        {
            var keep = s.normal.textColor;
            s.normal.textColor = new Color(0f, 0f, 0f, 0.65f);
            GUI.Label(new Rect(r.x + 1.5f, r.y + 1.5f, r.width, r.height), text, s);
            s.normal.textColor = keep;
            GUI.Label(r, text, s);
        }

        // ---- texture generation (lazy, cached) ------------------------------

        static void Ensure()
        {
            if (_cardStyle != null) return;

            _solid = Texture2D.whiteTexture;
            _card     = MakeCard(BodyFill, BodyBorder, 14, 2f);
            _cardLite = MakeCard(CardFill, BodyBorder, 10, 1.5f);
            _shadow   = MakeShadow(14, 14, 0.42f);

            _cardStyle     = SliceStyle(_card, 16);
            _cardLiteStyle = SliceStyle(_cardLite, 12);
            _shadowStyle   = SliceStyle(_shadow, 30);
        }

        static GUIStyle SliceStyle(Texture2D tex, int border)
        {
            return new GUIStyle
            {
                normal = { background = tex },
                border = new RectOffset(border, border, border, border)
            };
        }

        // Rounded-rect card: translucent fill + crisp anti-aliased gold border.
        // Built as a 9-sliceable texture so corners never stretch.
        static Texture2D MakeCard(Color fill, Color border, int radius, float borderW)
        {
            int N = radius * 2 + 8;
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false)
            { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };

            var px = new Color[N * N];
            float c = (N - 1) * 0.5f;
            float H = N * 0.5f - 1.5f;   // half-extent of the rounded box
            float feather = 1.3f;
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float d = RoundBoxSdf(x - c, y - c, H, radius);
                Color col; float a;
                if (d > feather)        { col = border; a = 0f; }
                else if (d > 0f)        { col = border; a = border.a * (1f - d / feather); }
                else if (d > -borderW)  { col = border; a = border.a; }
                else                    { col = fill;   a = fill.a; }
                px[y * N + x] = new Color(col.r, col.g, col.b, a);
            }
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }

        // Soft feathered shadow blob (rounded, fading to transparent).
        static Texture2D MakeShadow(int radius, int blur, float maxA)
        {
            int rr = radius + blur;
            int N = rr * 2 + 8;
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false)
            { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };

            var px = new Color[N * N];
            float c = (N - 1) * 0.5f;
            float H = N * 0.5f - 1.5f;
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float d = RoundBoxSdf(x - c, y - c, H, rr);
                float a;
                if (d > 0f) a = 0f;
                else
                {
                    float t = Mathf.Clamp01(-d / blur);   // 0 at edge → 1 deep inside
                    a = maxA * (t * t);                    // ease-in for a soft falloff
                }
                px[y * N + x] = new Color(0f, 0f, 0f, a);
            }
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }

        // Signed distance to a rounded box centred at origin (px, py from centre).
        static float RoundBoxSdf(float px, float py, float half, float r)
        {
            float qx = Mathf.Abs(px) - (half - r);
            float qy = Mathf.Abs(py) - (half - r);
            float ax = Mathf.Max(qx, 0f);
            float ay = Mathf.Max(qy, 0f);
            return Mathf.Sqrt(ax * ax + ay * ay) + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
        }
    }
}
