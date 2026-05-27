using UnityEngine;

namespace Spoonacci
{
    // Slowly rotates the directional sun + tints color over a 6-min cycle.
    // Day → sunset → night → sunrise. Public GameState flag IsNight for spawn behaviors.
    public class DayNightCycle : MonoBehaviour
    {
        public float secondsPerDay = 900f; // 15 min full day (was 6 — too fast)
        public float startTimeOfDay = 0.4f; // 0..1; 0 = midnight, 0.5 = noon — start in morning
        public Light sunOverride;
        public bool affectAmbient = true;

        Light sun;
        float t;

        void Awake()
        {
            t = startTimeOfDay;
            sun = sunOverride;
            if (sun == null)
            {
                foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                    if (l.type == LightType.Directional) { sun = l; break; }
            }
            if (sun == null)
            {
                var go = new GameObject("Sun");
                sun = go.AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.shadows = LightShadows.Soft;
            }
        }

        void Update()
        {
            t = (t + Time.deltaTime / Mathf.Max(secondsPerDay, 1f)) % 1f;
            float ang = t * 360f - 90f; // dawn at t=0.25
            sun.transform.rotation = Quaternion.Euler(ang, 30f, 0f);

            // intensity & color — never let it go pitch-black
            float dayWeight = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI * 2f - Mathf.PI * 0.5f) * 1.2f + 0.1f);
            sun.intensity = Mathf.Lerp(0.4f, 1.5f, dayWeight); // moonlit floor of 0.4 (was 0.05)
            sun.color = Color.Lerp(new Color(0.7f, 0.7f, 0.9f), new Color(1f, 0.95f, 0.85f), dayWeight); // moonlight stays cool-white

            if (affectAmbient)
            {
                RenderSettings.ambientIntensity = Mathf.Lerp(0.7f, 1.2f, dayWeight); // night ambient 0.7 (was 0.2)
                RenderSettings.ambientLight = Color.Lerp(new Color(0.25f, 0.30f, 0.40f), new Color(0.55f, 0.6f, 0.7f), dayWeight); // dim blue moonlight, never black
            }

            GameState.IsNight = dayWeight < 0.25f;
        }
    }
}
