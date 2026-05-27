using UnityEngine;

namespace Spoonacci
{
    // Procedural sound effects + the looping police siren.
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
                    _instance.sirenSrc = go.AddComponent<AudioSource>();
                    _instance.sirenSrc.spatialBlend = 0f;
                    _instance.sirenSrc.loop = true;
                    _instance.sirenSrc.volume = 0.45f;
                    _instance.Bake();
                }
                return _instance;
            }
        }

        AudioSource audio;
        AudioSource sirenSrc;
        AudioClip bonkClip, swooshClip, chimeClip, levelUpClip, ouchClip, sparkleClip, savedClip, sirenLoop;

        void Bake()
        {
            bonkClip    = MakeBonk();
            swooshClip  = MakeSwoosh();
            chimeClip   = MakeChime();
            levelUpClip = MakeLevelUp();
            ouchClip    = MakeOuch();
            sparkleClip = MakeSparkle();
            savedClip   = MakeSavedDing();
            sirenLoop   = MakeSiren();
            sirenSrc.clip = sirenLoop;
        }

        float Sfx => SettingsManager.EffectiveSfx;
        float Dlg => SettingsManager.EffectiveDialogue;
        public void Bonk()    { if (audio != null && bonkClip    != null) audio.PlayOneShot(bonkClip,    Sfx); }
        public void Swoosh()  { if (audio != null && swooshClip  != null) audio.PlayOneShot(swooshClip,  Sfx * 0.6f); }
        public void Chime()   { if (audio != null && chimeClip   != null) audio.PlayOneShot(chimeClip,   Dlg); }
        public void LevelUp() { if (audio != null && levelUpClip != null) audio.PlayOneShot(levelUpClip, Dlg); }
        public void Ouch()    { if (audio != null && ouchClip    != null) audio.PlayOneShot(ouchClip,    Dlg * 0.7f); }
        public void Sparkle() { if (audio != null && sparkleClip != null) audio.PlayOneShot(sparkleClip, Sfx * 0.6f); }
        public void Saved()   { if (audio != null && savedClip   != null) audio.PlayOneShot(savedClip,   Dlg * 0.5f); }
        public void StartSiren() { if (sirenSrc != null && !sirenSrc.isPlaying) { sirenSrc.volume = 0.45f * SettingsManager.EffectiveSfx; sirenSrc.Play(); } }
        public void StopSiren()  { if (sirenSrc != null && sirenSrc.isPlaying) sirenSrc.Stop(); }
        void Update() { if (sirenSrc != null && sirenSrc.isPlaying) sirenSrc.volume = 0.45f * SettingsManager.EffectiveSfx; }

        AudioClip MakeBonk()
        {
            int sr = 44100;
            int len = sr / 4;
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
            var c = AudioClip.Create("bonk", len, 1, sr, false); c.SetData(data, 0); return c;
        }

        AudioClip MakeSwoosh()
        {
            int sr = 44100;
            int len = sr / 3;
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)sr;
                float env = Mathf.Sin(Mathf.PI * t / 0.33f);
                float noise = (Random.value * 2f - 1f);
                float body = Mathf.Sin(2f * Mathf.PI * (700f + t * 200f) * t) * 0.3f;
                data[i] = (noise * 0.4f + body) * env * 0.8f;
            }
            var c = AudioClip.Create("swoosh", len, 1, sr, false); c.SetData(data, 0); return c;
        }

        AudioClip MakeChime()
        {
            int sr = 44100; int len = sr / 2;
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
            var c = AudioClip.Create("chime", len, 1, sr, false); c.SetData(data, 0); return c;
        }

        AudioClip MakeLevelUp()
        {
            int sr = 44100; int len = sr;
            float[] data = new float[len];
            float[] notes = { 523f, 659f, 784f, 1046f };
            int slice = len / notes.Length;
            for (int i = 0; i < len; i++)
            {
                int n = Mathf.Min(notes.Length - 1, i / slice);
                float t = (i % slice) / (float)sr;
                float env = Mathf.Exp(-t * 10f);
                data[i] = Mathf.Sin(2f * Mathf.PI * notes[n] * t) * env * 0.5f;
            }
            var c = AudioClip.Create("levelup", len, 1, sr, false); c.SetData(data, 0); return c;
        }

        AudioClip MakeOuch()
        {
            // a quick yelp — pitch rising then dropping
            int sr = 44100; int len = sr / 3;
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)sr;
                float u = t / (len / (float)sr); // 0..1
                float freq = 300f + 600f * Mathf.Sin(u * Mathf.PI);
                float env = Mathf.Sin(u * Mathf.PI);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.45f;
            }
            var c = AudioClip.Create("ouch", len, 1, sr, false); c.SetData(data, 0); return c;
        }

        AudioClip MakeSparkle()
        {
            int sr = 44100; int len = sr / 2;
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)sr;
                float u = t / (len / (float)sr);
                // shimmer = many short bursts of high frequencies
                float freq = 1500f + Mathf.Sin(t * 80f) * 800f;
                float env = Mathf.Exp(-u * 5f);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.35f;
            }
            var c = AudioClip.Create("sparkle", len, 1, sr, false); c.SetData(data, 0); return c;
        }

        AudioClip MakeSavedDing()
        {
            int sr = 44100; int len = sr / 3;
            float[] data = new float[len];
            float f = 1175f; // D6
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)sr;
                float env = Mathf.Exp(-t * 6f);
                data[i] = Mathf.Sin(2f * Mathf.PI * f * t) * env * 0.5f;
            }
            var c = AudioClip.Create("saved", len, 1, sr, false); c.SetData(data, 0); return c;
        }

        // Classic two-tone alternating police siren (~1.5s loop)
        AudioClip MakeSiren()
        {
            int sr = 22050;
            int len = sr * 2; // 2s loop
            float[] data = new float[len];
            float f1 = 750f;
            float f2 = 480f;
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)sr;
                // alternate every 0.5s
                bool high = ((int)(t / 0.5f)) % 2 == 0;
                float f = high ? f1 : f2;
                // crossfade between tones for smoothness
                float local = (t % 0.5f) / 0.5f;
                float crossFade = Mathf.Sin(local * Mathf.PI); // 0 at edges, 1 in middle
                float tone = Mathf.Sin(2f * Mathf.PI * f * t);
                data[i] = tone * crossFade * 0.35f;
            }
            var c = AudioClip.Create("siren", len, 1, sr, false); c.SetData(data, 0); return c;
        }
    }
}
