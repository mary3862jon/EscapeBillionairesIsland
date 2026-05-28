using UnityEngine;

namespace Spoonacci
{
    // Add this to the Main Camera. Call CameraShake.Shake(amp, dur) anywhere for impact feel.
    public class CameraShake : MonoBehaviour
    {
        static CameraShake _i;
        float trauma;
        float decay = 1.5f;

        void Awake() { _i = this; }
        void OnDestroy() { if (_i == this) _i = null; }

        public static void Shake(float amplitude = 0.6f)
        {
            if (_i == null && Camera.main != null) _i = Camera.main.gameObject.AddComponent<CameraShake>();
            if (_i == null) return;
            _i.trauma = Mathf.Max(_i.trauma, Mathf.Clamp01(amplitude));
        }

        void LateUpdate()
        {
            if (trauma <= 0f) return;
            float t2 = trauma * trauma;
            float ox = (Mathf.PerlinNoise(Time.unscaledTime * 35f, 0f) * 2f - 1f) * t2 * 0.4f;
            float oy = (Mathf.PerlinNoise(0f, Time.unscaledTime * 35f) * 2f - 1f) * t2 * 0.4f;
            float oroll = (Mathf.PerlinNoise(Time.unscaledTime * 25f, 99f) * 2f - 1f) * t2 * 6f;
            transform.position += new Vector3(ox, oy, 0f);
            transform.rotation = transform.rotation * Quaternion.Euler(0f, 0f, oroll);
            trauma = Mathf.Max(0f, trauma - decay * Time.unscaledDeltaTime);
        }
    }
}
