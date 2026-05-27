using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Background music system. Always playing. 4 procedurally-generated tracks.
    // Controls: [J] previous · [K] next · [M] mute toggle · [-/=] volume down/up
    public class MusicPlayer : MonoBehaviour
    {
        static MusicPlayer _instance;
        public static MusicPlayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[MusicPlayer]");
                    _instance = go.AddComponent<MusicPlayer>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        AudioSource src;
        AudioClip[] tracks;
        string[] trackNames = { "Tropical Lounge", "Heist Synth", "Spoonacci Theme", "Night Drive" };
        int current;
        float toastUntil;
        bool muted;
        float volume = 0.35f;

        void Awake()
        {
            src = gameObject.AddComponent<AudioSource>();
            src.loop = true;
            src.spatialBlend = 0f;
            src.volume = volume;

            tracks = new AudioClip[]
            {
                BuildTropicalLounge(),
                BuildHeistSynth(),
                BuildSpoonacciTheme(),
                BuildNightDrive(),
            };

            Play(0, announce: false);
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.jKey.wasPressedThisFrame) Play((current - 1 + tracks.Length) % tracks.Length);
            if (kb.kKey.wasPressedThisFrame) Play((current + 1) % tracks.Length);
            if (kb.mKey.wasPressedThisFrame) { muted = !muted; src.mute = muted; Toast(muted ? "♪ MUTED" : "♪ unmuted"); }
            if (kb.minusKey.wasPressedThisFrame || kb.numpadMinusKey.wasPressedThisFrame) { volume = Mathf.Clamp01(volume - 0.1f); src.volume = volume; Toast("♪ vol " + Mathf.RoundToInt(volume * 100) + "%"); }
            if (kb.equalsKey.wasPressedThisFrame || kb.numpadPlusKey.wasPressedThisFrame) { volume = Mathf.Clamp01(volume + 0.1f); src.volume = volume; Toast("♪ vol " + Mathf.RoundToInt(volume * 100) + "%"); }
        }

        void Play(int idx, bool announce = true)
        {
            current = idx;
            src.clip = tracks[idx];
            src.Play();
            if (announce) Toast("♪ " + trackNames[idx]);
        }

        void Toast(string s) { _toast = s; toastUntil = Time.unscaledTime + 2f; }
        string _toast = "";

        GUIStyle style;
        void OnGUI()
        {
            if (Time.unscaledTime > toastUntil) return;
            if (style == null)
                style = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(20), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = new Color(0.7f, 1f, 0.8f) } };
            var sh = new GUIStyle(style); sh.normal.textColor = Color.black;
            GUI.Label(new Rect(22f, Screen.height - 102f, 500f, 28f), _toast, sh);
            GUI.Label(new Rect(20f, Screen.height - 104f, 500f, 28f), _toast, style);
        }

        // ---------- Procedural generators ----------
        // Each track = a chord progression with bass + melody + soft beat. ~16s loops.

        AudioClip BuildTropicalLounge()
        {
            // I-vi-IV-V in C major, gentle pad
            float[] chord1 = { 261f, 329f, 392f }; // C major
            float[] chord2 = { 220f, 261f, 329f }; // A minor
            float[] chord3 = { 174f, 220f, 261f }; // F major
            float[] chord4 = { 196f, 246f, 293f }; // G major
            return BakeChordLoop(new[] { chord1, chord2, chord3, chord4 }, bpm: 60, sustain: true, brightness: 0.4f);
        }

        AudioClip BuildHeistSynth()
        {
            // i-VII-VI-V in A minor — moody bouncy
            float[] chord1 = { 220f, 261f, 329f }; // Am
            float[] chord2 = { 196f, 246f, 293f }; // G
            float[] chord3 = { 174f, 220f, 261f }; // F
            float[] chord4 = { 165f, 207f, 246f }; // E
            return BakeChordLoop(new[] { chord1, chord2, chord3, chord4 }, bpm: 110, sustain: false, brightness: 0.65f);
        }

        AudioClip BuildSpoonacciTheme()
        {
            // I-IV-V-I in D — uplifting
            float[] chord1 = { 293f, 369f, 440f };
            float[] chord2 = { 392f, 493f, 587f };
            float[] chord3 = { 440f, 554f, 659f };
            float[] chord4 = { 293f, 369f, 440f };
            return BakeChordLoop(new[] { chord1, chord2, chord3, chord4 }, bpm: 90, sustain: true, brightness: 0.85f);
        }

        AudioClip BuildNightDrive()
        {
            // i-v-VI-VII in F minor — synthwave moody
            float[] chord1 = { 174f, 220f, 261f };
            float[] chord2 = { 130f, 174f, 220f };
            float[] chord3 = { 220f, 277f, 329f };
            float[] chord4 = { 246f, 311f, 369f };
            return BakeChordLoop(new[] { chord1, chord2, chord3, chord4 }, bpm: 80, sustain: true, brightness: 0.3f);
        }

        AudioClip BakeChordLoop(float[][] chords, int bpm, bool sustain, float brightness)
        {
            int sr = 22050;
            float beatSec = 60f / bpm;
            int beatsPerChord = 4;
            int totalBeats = chords.Length * beatsPerChord;
            int sampleCount = Mathf.RoundToInt(totalBeats * beatSec * sr);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sr;
                int beatIdx = Mathf.Min(totalBeats - 1, Mathf.FloorToInt(t / beatSec));
                int chordIdx = beatIdx / beatsPerChord;
                float[] chord = chords[chordIdx];
                float chordT = t - chordIdx * beatsPerChord * beatSec;

                // chord pad — sine + slight detune for warmth
                float pad = 0f;
                foreach (var f in chord)
                {
                    pad += Mathf.Sin(2f * Mathf.PI * f * t) * 0.18f;
                    pad += Mathf.Sin(2f * Mathf.PI * f * 0.5f * t) * 0.08f; // bass octave
                }

                // melody — top note of chord with rhythmic attack
                float melT = (t / beatSec) % 1f;
                float melEnv = sustain ? (Mathf.Sin(melT * Mathf.PI * 2f) * 0.5f + 0.5f) : Mathf.Exp(-melT * 4f);
                float melFreq = chord[chord.Length - 1] * 2f * (brightness > 0.5f ? 1f : 0.75f);
                float melody = Mathf.Sin(2f * Mathf.PI * melFreq * t) * melEnv * brightness * 0.18f;

                // soft beat (every beat, brief click)
                float beatPhase = (t / beatSec) % 1f;
                float beat = (beatPhase < 0.05f) ? Mathf.Sin(2f * Mathf.PI * 80f * t) * (1f - beatPhase / 0.05f) * 0.25f : 0f;

                // long fade at start + end of loop for click-free loop point
                float loopFade = 1f;
                float fadeSec = 0.05f;
                if (t < fadeSec) loopFade = t / fadeSec;
                float endT = sampleCount / (float)sr - t;
                if (endT < fadeSec) loopFade = Mathf.Min(loopFade, endT / fadeSec);

                data[i] = (pad + melody + beat) * 0.55f * loopFade;
                data[i] = Mathf.Clamp(data[i], -0.85f, 0.85f);
            }

            var c = AudioClip.Create("track", sampleCount, 1, sr, false);
            c.SetData(data, 0);
            return c;
        }
    }
}
