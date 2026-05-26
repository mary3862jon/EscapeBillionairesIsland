using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Trigger zone. Step inside, press E, spoon cycles to the next skin.
    // The salon is themed as a posh Italian day spa for cutlery.
    [RequireComponent(typeof(SphereCollider))]
    public class SalonCucchiaio : MonoBehaviour
    {
        public float interactRadius = 2.5f;
        public KeyValueText prompt; // optional UI hook
        int currentSkin = 0;
        bool playerInside;
        ProceduralSpoonBuilder cachedBuilder;

        void Awake()
        {
            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = interactRadius;
        }

        void OnTriggerEnter(Collider other)
        {
            var b = other.GetComponent<ProceduralSpoonBuilder>();
            if (b != null) { cachedBuilder = b; playerInside = true; ShowPrompt(true); }
        }

        void OnTriggerExit(Collider other)
        {
            var b = other.GetComponent<ProceduralSpoonBuilder>();
            if (b != null) { playerInside = false; ShowPrompt(false); }
        }

        void Update()
        {
            if (!playerInside || cachedBuilder == null) return;
            var kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame) Cycle();
        }

        void Cycle()
        {
            currentSkin = (currentSkin + 1) % SpoonSkinLibrary.All.Count;
            var s = SpoonSkinLibrary.All[currentSkin];
            cachedBuilder.ApplySkin(s.color, s.metallic, s.smoothness);
            Debug.Log("[Salon Cucchiaio] Madame, you look fabulous as: " + s.name);
            if (prompt != null) prompt.Set("Skin: " + s.name + "  (press E for next)");
        }

        void ShowPrompt(bool show)
        {
            if (prompt == null) return;
            prompt.Set(show ? "Salon Cucchiaio — press E to change skin" : "");
        }
    }

    // Tiny abstraction so script doesn't depend on a specific UI lib.
    public interface KeyValueText { void Set(string s); }
}
