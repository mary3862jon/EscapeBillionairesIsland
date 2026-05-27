using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Trigger zone in front of the salon counter.
    // - Spoon enters → tooltip auto-pops (no key needed)
    // - Press E → cycle skin
    // - Front-sign text rendered by parent WorldLabel
    public class SalonCucchiaio : MonoBehaviour
    {
        public KeyValueText prompt;       // HUD ref
        public string frontSignText = "SALON CUCCHIAIO";

        int currentSkin = 0;
        bool playerInside;
        ProceduralSpoonBuilder cachedBuilder;
        SpoonAnimator cachedAnim;

        void OnTriggerEnter(Collider other)
        {
            var b = other.GetComponent<ProceduralSpoonBuilder>();
            if (b != null)
            {
                cachedBuilder = b;
                cachedAnim = other.GetComponent<SpoonAnimator>();
                playerInside = true;
                ShowEntryTooltip();
            }
        }

        void OnTriggerExit(Collider other)
        {
            var b = other.GetComponent<ProceduralSpoonBuilder>();
            if (b != null) { playerInside = false; if (prompt != null) prompt.Set(""); }
        }

        void Update()
        {
            if (!playerInside || cachedBuilder == null) return;
            var kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame) Cycle();
        }

        void ShowEntryTooltip()
        {
            if (prompt == null) return;
            string current = SpoonSkinLibrary.All[currentSkin].name;
            prompt.Set("✨ SALON CUCCHIAIO ✨   Now wearing: " + current + "   ·   Press E for next skin");
        }

        void Cycle()
        {
            currentSkin = (currentSkin + 1) % SpoonSkinLibrary.All.Count;
            var s = SpoonSkinLibrary.All[currentSkin];
            cachedBuilder.ApplySkin(s.color, s.metallic, s.smoothness);
            if (cachedAnim != null) cachedAnim.TriggerLandSquash();
            Debug.Log("[Salon Cucchiaio] Madame, you look fabulous as: " + s.name);
            if (prompt != null) prompt.Set("✨ Now wearing: " + s.name + "   ·   Press E for next");
        }
    }

    public interface KeyValueText { void Set(string s); }
}
