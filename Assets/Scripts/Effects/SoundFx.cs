using UnityEngine;

namespace Spoonacci
{
    // Procedural sound effects — no external files. Generates AudioClips at runtime.
    // - Bonk: short low thunk + small noise burst
    // - Swoosh: white noise burst with attack/decay
    // - Chime: rising sine tone
    public class SoundFx : MonoBehaviour
    {
        static SoundFx _instance;
        public static SoundFx Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[SoundFx]");
                    _instance = go.AddComponent<SoundFx>();
                    DontDestroyOnLoad(go);
                    _instance.audio = go.AddComponent<AudioSource>();
                    _instance.audio.spatialBlend = 0f;
                    _instance.Bake();
                }
                return _instance;
            }
        }

        AudioSource audio;
        AudioClip bonkClip, swooshClip, chimeClip, levelUpClip;

        void Bake()
        {
            bonkClip   = MakeBonk();
            swooshClip = MakeSwoosh();
            chimeClip  = MakeChime();
            levelUpClip = MakeLevelUp();
        }

        public void Bonk()    { if (audio != null && bonkClip != null) audio.PlayOneShot(bonkClip); }
        public void Swoosh()  { if (audio != null && swooshClip != null) audio.PlayOneShot(swooshClip, 0.6f); }
        public void Chime()   { if (audio != null && chimeClip != null) audio.PlayOneShot(chimeClip); }
        public void LevelUp() { if (audio != null && levelUpClip != null) audio.PlayOneShot(levelUpClip); }

        AudioClip MakeBonk()
        {
            int sr = 44100;
            int len = sr / 4; // 0.25s
            float[] data = new float[len];
            float baseFreq = 180f;
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)sr;
                float env = Mathf.Exp(-t * 18f);
                float tone = Mathf.Sin(2f * Mathf.PI * baseFreq * Mathf.Pow(0.96f, t * 30f) * t);
                float noise = (Random.value * 2f - 1f) * 0.5f * Mathf.Exp(-t * 60f);
                data[i] = (tone * 0.7f + noise) * env;
            }
            var c = AudioClip.Create("bonk", len, 1, sr, false);
            c.SetData(data, 0);
            return c;
        }

        AudioClip MakeSwoosh()
        {
            int sr = 44100;
            int len = sr / 3;
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)sr;
                float env = Mathf.Sin(Mathf.PI * t / 0.33f); // attack/release
                float noise = (Random.value * 2f - 1f);
                // simple high-pass-ish by adding sin to whittle to body
                float body = Mathf.Sin(2f * Mathf.PI * (700f + t * 200f) * t) * 0.3f;
                data[i] = (noise * 0.4f + body) * env * 0.8f;
            }
            var c = AudioClip.Create("swoosh", len, 1, sr, false);
            c.SetData(data, 0);
            return c;
        }

        AudioClip MakeChime()
        {
            int sr = 44100;
            int len = sr / 2;
            float[] data = new float[len];
            float f1 = 880f, f2 = 1320f, f3 = 1760f;
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)sr;
                float env = Mathf.Exp(-t * 6f);
                float s = Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.5f
                        + Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.3f
                        + Mathf.Sin(2f * Mathf.PI * f3 * t) * 0.2f;
                data[i] = s * env * 0.6f;
            }
            var c = AudioClip.Create("chime", len, 1, sr, false);
            c.SetData(data, 0);
            return c;
        }

        AudioClip MakeLevelUp()
        {
            int sr = 44100;
            int len = sr;
            float[] data = new float[len];
            float[] notes = { 523f, 659f, 784f, 1046f }; // C-E-G-C
            int slice = len / notes.Length;
            for (int i = 0; i < len; i++)
            {
                int n = Mathf.Min(notes.Length - 1, i / slice);
                float t = (i % slice) / (float)sr;
                float env = Mathf.Exp(-t * 10f);
                data[i] = Mathf.Sin(2f * Mathf.PI * notes[n] * t) * env * 0.5f;
            }
            var c = AudioClip.Create("levelup", len, 1, sr, false);
            c.SetData(data, 0);
            return c;
        }
    }
}
