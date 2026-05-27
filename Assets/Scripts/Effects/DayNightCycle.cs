using UnityEngine;

namespace Spoonacci
{
    // Slowly rotates the directional sun + tints color over a 6-min cycle.
    // Day → sunset → night → sunrise. Public GameState flag IsNight for spawn behaviors.
    public class DayNightCycle : MonoBehaviour
    {
        public float secondsPerDay = 360f; // 6 min full day
        public float startTimeOfDay = 0.35f; // 0..1; 0 = midnight, 0.5 = noon
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

            // intensity & color
            float dayWeight = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI * 2f - Mathf.PI * 0.5f) * 1.2f + 0.1f);
            sun.intensity = Mathf.Lerp(0.05f, 1.5f, dayWeight);
            sun.color = Color.Lerp(new Color(0.5f, 0.3f, 0.6f), new Color(1f, 0.95f, 0.85f), dayWeight);

            if (affectAmbient)
            {
                RenderSettings.ambientIntensity = Mathf.Lerp(0.2f, 1.1f, dayWeight);
                RenderSettings.ambientLight = Color.Lerp(new Color(0.05f, 0.05f, 0.1f), new Color(0.5f, 0.55f, 0.65f), dayWeight);
            }

            GameState.IsNight = dayWeight < 0.25f;
        }
    }
}
