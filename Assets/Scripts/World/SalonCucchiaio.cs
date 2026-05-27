using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Salon Cucchiaio interaction.
    // - If wearing police uniform (GameState.PoliceMode + JustEscaped): show NEGOTIATION menu:
    //     [E] PAY 30 tokens for civilian disguise
    //     [Q] THREATEN attendant (bonk Coiffeur) — free disguise but attendant gets a Spoon Mark
    // - Otherwise: standard skin cycler (E for next skin).
    // - On any skin change: sparkle SFX + brief flash on the spoon.
    public class SalonCucchiaio : MonoBehaviour
    {
        public KeyValueText prompt;
        public string frontSignText = "SALON CUCCHIAIO";
        public Transform attendantTransform; // optional — bonk target for "threaten"

        bool playerInside;
        ProceduralSpoonBuilder cachedBuilder;
        SpoonAnimator cachedAnim;
        PoliceMode cachedPolice;

        GUIStyle menuStyle;

        void OnTriggerEnter(Collider other)
        {
            var b = other.GetComponent<ProceduralSpoonBuilder>();
            if (b != null)
            {
                cachedBuilder = b;
                cachedAnim = other.GetComponent<SpoonAnimator>();
                cachedPolice = other.GetComponent<PoliceMode>();
                playerInside = true;
                ShowEntryTooltip();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<ProceduralSpoonBuilder>() != null)
            {
                playerInside = false;
                if (prompt != null) prompt.Set("");
            }
        }

        bool InNegotiationMode => playerInside && GameState.JustEscaped && !GameState.WearingCivilianClothes;

        void Update()
        {
            if (!playerInside || cachedBuilder == null) return;
            var kb = Keyboard.current;
            if (kb == null) return;

            if (InNegotiationMode)
            {
                if (kb.eKey.wasPressedThisFrame)      TryPay();
                else if (kb.qKey.wasPressedThisFrame) Threaten();
                return;
            }

            if (kb.eKey.wasPressedThisFrame) Cycle();
        }

        void ShowEntryTooltip()
        {
            if (prompt == null) return;
            if (InNegotiationMode)
            {
                prompt.Set("✨ SALON CUCCHIAIO ✨  [E] PAY 30 tokens for disguise   ·   [Q] THREATEN attendant for free");
            }
            else
            {
                string current = SpoonSkinLibrary.All[GameState.CurrentSkinIndex].name;
                prompt.Set("✨ SALON CUCCHIAIO ✨   Wearing: " + current + "   ·   [E] next skin");
            }
        }

        void TryPay()
        {
            if (GameState.TrollTokens < 30)
            {
                if (prompt != null) prompt.Set("...not enough tokens (need 30). Bonk more violators or [Q] threaten.");
                SoundFx.Instance.Ouch();
                return;
            }
            GameState.SpendTokens(30);
            GiveCivilianDisguise();
            if (prompt != null) prompt.Set("✨ The Coiffeur smiles. You blend in beautifully now.");
            SaveSystem.Instance.Save("Bought disguise");
        }

        void Threaten()
        {
            // attendant is bonk'd off-screen-style — visual lite reaction via daze birds if we can find it
            if (attendantTransform != null)
            {
                var birds = new GameObject("DazeBirds");
                birds.AddComponent<DazeBirds>().Attach(attendantTransform);
            }
            SoundFx.Instance.Bonk();
            SoundFx.Instance.Ouch();
            GiveCivilianDisguise();
            if (prompt != null) prompt.Set("⚠ You threatened the Coiffeur. She fled in tears. You wear what you like now.");
            SaveSystem.Instance.Save("Threatened salon");
        }

        void GiveCivilianDisguise()
        {
            GameState.WearingCivilianClothes = true;
            GameState.PoliceMode = false; // shed the uniform
            // disable cop badge + siren
            if (cachedPolice != null) cachedPolice.ForceExit();
            // pick a random non-default skin
            GameState.CurrentSkinIndex = Random.Range(1, SpoonSkinLibrary.All.Count);
            var s = SpoonSkinLibrary.All[GameState.CurrentSkinIndex];
            cachedBuilder.ApplySkin(s.color, s.metallic, s.smoothness);
            SoundFx.Instance.Sparkle();
            SoundFx.Instance.LevelUp();
            if (cachedAnim != null) cachedAnim.TriggerLandSquash();
        }

        void Cycle()
        {
            GameState.CurrentSkinIndex = (GameState.CurrentSkinIndex + 1) % SpoonSkinLibrary.All.Count;
            var s = SpoonSkinLibrary.All[GameState.CurrentSkinIndex];
            cachedBuilder.ApplySkin(s.color, s.metallic, s.smoothness);
            if (cachedAnim != null) cachedAnim.TriggerLandSquash();
            SoundFx.Instance.Sparkle();
            if (prompt != null) prompt.Set("✨ Now wearing: " + s.name + "   ·   [E] next");
        }

        void OnGUI()
        {
            if (!InNegotiationMode) return;
            EnsureStyles();

            float w = 560f, h = 140f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.32f;
            GUI.color = new Color(0f, 0f, 0f, 0.78f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(x, y + 8f, w, 32f), "DITCH THE POLICE UNIFORM", menuStyle);
            var s = new GUIStyle(menuStyle) { fontSize = 22, fontStyle = FontStyle.Normal };
            GUI.Label(new Rect(x, y + 50f, w, 28f), "[E] Pay 30 Troll Tokens  (you have " + GameState.TrollTokens + ")", s);
            GUI.Label(new Rect(x, y + 84f, w, 28f), "[Q] Threaten the Coiffeur (free, but rude)", s);
        }

        void EnsureStyles()
        {
            if (menuStyle == null)
                menuStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.85f, 0.4f) } };
        }
    }

    public interface KeyValueText { void Set(string s); }
}
