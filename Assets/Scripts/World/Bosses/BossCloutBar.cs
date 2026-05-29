using UnityEngine;

namespace Spoonacci
{
    // ─────────────────────────────────────────────────────────────────────────
    //  BossCloutBar — bottom-center HUD for the currently engaged boss.
    //
    //  Shows the boss name, a "CLOUT" bar that drains gold→red as the player
    //  bonks the boss, and the current phase label. Themed via UiTheme/UiScale.
    //  Fills are drawn with GUI.DrawTexture(Texture2D.whiteTexture) tinted via
    //  GUI.color (never GUIStyle.Draw — that throws outside Repaint).
    //
    //  Hidden when no boss is engaged or the boss is Defeated (after a brief
    //  victory flash).
    // ─────────────────────────────────────────────────────────────────────────
    public class BossCloutBar : MonoBehaviour
    {
        BossFight _boss;
        float _shownFrac = 1f;       // eased fill so it drains smoothly
        float _victoryUntil;         // > now while the victory flash plays

        GUIStyle _nameStyle, _phaseStyle, _cloutStyle, _hintStyle;

        public void Track(BossFight b)
        {
            _boss = b;
            _shownFrac = b != null ? b.CloutFrac : 1f;
            _victoryUntil = 0f;
        }

        // Called by BossFight when the boss is defeated → brief celebratory flash.
        public void FlashVictory()
        {
            _victoryUntil = Time.time + 2.2f;
        }

        void Update()
        {
            if (_boss == null) return;
            _shownFrac = Mathf.Lerp(_shownFrac, _boss.CloutFrac, Time.deltaTime * 8f);
        }

        bool ShouldShow()
        {
            if (_boss == null) return false;
            if (_boss.Defeated) return Time.time < _victoryUntil;  // linger for the flash
            return true;
        }

        void OnGUI()
        {
            if (!ShouldShow()) return;
            EnsureStyles();

            bool win = _boss.Defeated;

            float w = Mathf.Min(Screen.width * 0.5f, 760f);
            float h = 92f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height - h - 64f;

            var panel = new Rect(x, y, w, h);
            UiTheme.Panel(panel);

            // BIG action-plan banner just above the bar — tells the player exactly what to
            // do this phase (smash drones / dodge + bonk the boss). Pulses to draw the eye.
            if (!win)
            {
                string hint = _boss.ActionHint;
                if (!string.IsNullOrEmpty(hint))
                {
                    float hw = Mathf.Min(Screen.width * 0.72f, 1040f);
                    float hh = 50f;
                    var hintRect = new Rect((Screen.width - hw) * 0.5f, y - hh - 12f, hw, hh);
                    UiTheme.Card(hintRect);
                    UiTheme.Accent(new Rect(hintRect.x + 1f, hintRect.y + 8f, 3f, hh - 16f), UiTheme.TextWarn);
                    float pulse = 0.78f + 0.22f * Mathf.Sin(Time.unscaledTime * 5f);
                    _hintStyle.normal.textColor = new Color(1f, 0.86f * pulse, 0.4f * pulse, 1f);
                    UiTheme.Label(hintRect, hint, _hintStyle);
                }
            }

            float pad = 18f;
            // boss name (top-left of panel)
            string name = win ? (_boss.bossName + "  —  " + Loc.T("boss.bonked")) : _boss.bossName;
            _nameStyle.normal.textColor = win ? UiTheme.TextDone : UiTheme.GoldSoft;
            UiTheme.Label(new Rect(x + pad, y + 8f, w - pad * 2f, 28f), name, _nameStyle);

            // phase label (top-right)
            _phaseStyle.normal.textColor = UiTheme.TextDim;
            UiTheme.Label(new Rect(x + pad, y + 8f, w - pad * 2f, 28f), _boss.PhaseLabel, _phaseStyle);

            // CLOUT bar
            float barX = x + pad;
            float barY = y + 48f;
            float barW = w - pad * 2f;
            float barH = 22f;

            // track (dark)
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(barX, barY, barW, barH), Texture2D.whiteTexture);

            // fill — gold when full, lerp to red as it drains; bright flash on win
            float frac = win ? 0f : Mathf.Clamp01(_shownFrac);
            Color full = new Color(0.96f, 0.78f, 0.30f);   // gold
            Color low  = new Color(0.92f, 0.18f, 0.14f);   // red
            Color fill = Color.Lerp(low, full, frac);
            if (win) fill = UiTheme.TextDone;
            GUI.color = fill;
            float fillW = Mathf.Max(0f, barW * frac);
            if (fillW > 1f)
                GUI.DrawTexture(new Rect(barX, barY, fillW, barH), Texture2D.whiteTexture);

            // gold hairline border on the track
            GUI.color = new Color(UiTheme.Gold.r, UiTheme.Gold.g, UiTheme.Gold.b, 0.6f);
            GUI.DrawTexture(new Rect(barX, barY, barW, 1f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(barX, barY + barH - 1f, barW, 1f), Texture2D.whiteTexture);
            GUI.color = prev;

            // "CLOUT" caption centered over the bar
            _cloutStyle.normal.textColor = frac > 0.45f ? new Color(0f, 0f, 0f, 0.85f) : UiTheme.TextMain;
            string cap = win ? Loc.T("boss.defeated") : Loc.T("boss.clout") + "  " + Mathf.RoundToInt(frac * 100f) + "%";
            UiTheme.Label(new Rect(barX, barY - 1f, barW, barH), cap, _cloutStyle);
        }

        void EnsureStyles()
        {
            if (_nameStyle == null)
                _nameStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(22), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            if (_phaseStyle == null)
                _phaseStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(15), fontStyle = FontStyle.Italic, alignment = TextAnchor.MiddleRight };
            if (_cloutStyle == null)
                _cloutStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(14), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            if (_hintStyle == null)
                _hintStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(21), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        }
    }
}
