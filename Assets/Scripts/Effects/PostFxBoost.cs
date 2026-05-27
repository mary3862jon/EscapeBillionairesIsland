using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Spoonacci
{
    // Crank URP post-processing at runtime so the scene looks less plastic.
    // Adds a high-priority Global Volume with Bloom, Vignette, ColorAdjustments, Tonemapping (ACES), FilmGrain.
    // We add this from each bootstrapper. Existing scene volume stays intact (this layers on top).
    public class PostFxBoost : MonoBehaviour
    {
        public float bloomIntensity = 1.1f;
        public float vignetteIntensity = 0.32f;
        public float saturation = 18f;
        public float contrast = 14f;
        public float postExposure = 0.35f;
        public Color vignetteColor = new Color(0.05f, 0.02f, 0.12f);
        public bool addGrain = true;

        void Awake()
        {
            // Per-scene tuning so the prison isn't smothered by vignette/film grain.
            string scn = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (scn.Contains("Cutlery") || scn.Contains("Cell"))
            {
                bloomIntensity = 0.6f;
                vignetteIntensity = 0.12f;
                saturation = -5f;
                contrast = 8f;
                postExposure = 0.15f;
                vignetteColor = new Color(0.02f, 0.01f, 0.04f);
                addGrain = false;
            }
            else if (scn.Contains("Title"))
            {
                bloomIntensity = 1.7f;
                vignetteIntensity = 0.2f;
                saturation = 25f;
                contrast = 18f;
                postExposure = 0.5f;
                vignetteColor = new Color(0.3f, 0.1f, 0.4f);
                addGrain = true;
            }

            var go = new GameObject("[PostFx Volume]");
            go.transform.SetParent(transform, false);
            var v = go.AddComponent<Volume>();
            v.isGlobal = true;
            v.priority = 100;
            v.weight = 1f;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            v.sharedProfile = profile;

            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(bloomIntensity);
            bloom.threshold.Override(0.85f);
            bloom.scatter.Override(0.72f);
            bloom.tint.Override(new Color(1f, 0.95f, 0.88f));
            bloom.highQualityFiltering.Override(true);

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(vignetteIntensity);
            vignette.smoothness.Override(0.45f);
            vignette.color.Override(vignetteColor);

            var color = profile.Add<ColorAdjustments>(true);
            color.saturation.Override(saturation);
            color.contrast.Override(contrast);
            color.postExposure.Override(postExposure);

            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);

            if (addGrain)
            {
                var grain = profile.Add<FilmGrain>(true);
                grain.intensity.Override(0.25f);
                grain.type.Override(FilmGrainLookup.Thin1);
            }
        }
    }
}
