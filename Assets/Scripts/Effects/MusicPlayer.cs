using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Background music player.
    // Loads real .mp3/.ogg files from Resources/Music/ if present (the 6 Kevin MacLeod CC-BY tracks),
    // otherwise falls back to 4 procedurally-generated loops.
    // Controls: [K] next  [J] prev  [M] mute  [-/=] vol
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
        readonly List<AudioClip> tracks = new List<AudioClip>();
        readonly List<string> trackNames = new List<string>();
        int current;
        float toastUntil;
        bool muted;
        float volume = 0.55f;

        void Awake()
        {
            src = gameObject.AddComponent<AudioSource>();
            src.loop = true;
            src.spatialBlend = 0f;
            src.volume = volume;
            src.bypassEffects = true;

            // 1) try Resources/Music/*.mp3 (real CC-BY tracks if present)
            var loaded = Resources.LoadAll<AudioClip>("Music");
            if (loaded != null && loaded.Length > 0)
            {
                System.Array.Sort(loaded, (a, b) => string.Compare(a.name, b.name));
                foreach (var c in loaded)
                {
                    tracks.Add(c);
                    // strip the "01_" prefix for display
                    string n = c.name;
                    int us = n.IndexOf('_');
                    if (us > 0 && us < 4) n = n.Substring(us + 1);
                    trackNames.Add(n);
                }
            }

            // 2) fallback to procedural
            if (tracks.Count == 0)
            {
                tracks.Add(BuildTropicalLounge());   trackNames.Add("Tropical Lounge (procedural)");
                tracks.Add(BuildHeistSynth());       trackNames.Add("Heist Synth (procedural)");
                tracks.Add(BuildSpoonacciTheme());   trackNames.Add("Spoonacci Theme (procedural)");
                tracks.Add(BuildNightDrive());       trackNames.Add("Night Drive (procedural)");
            }

            Play(0, announce: false);
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.jKey.wasPressedThisFrame) Play((current - 1 + tracks.Count) % tracks.Count);
            if (kb.kKey.wasPressedThisFrame) Play((current + 1) % tracks.Count);
            if (kb.mKey.wasPressedThisFrame) { muted = !muted; src.mute = muted; Toast(muted ? "♪ MUTED" : "♪ unmuted"); }
            if (kb.minusKey.wasPressedThisFrame || kb.numpadMinusKey.wasPressedThisFrame) { volume = Mathf.Clamp01(volume - 0.1f); src.volume = volume; Toast("♪ vol " + Mathf.RoundToInt(volume * 100) + "%"); }
            if (kb.equalsKey.wasPressedThisFrame || kb.numpadPlusKey.wasPressedThisFrame) { volume = Mathf.Clamp01(volume + 0.1f); src.volume = volume; Toast("♪ vol " + Mathf.RoundToInt(volume * 100) + "%"); }
        }

        void Play(int idx, bool announce = true)
        {
            if (tracks.Count == 0) return;
            current = idx;
            src.clip = tracks[idx];
            src.Play();
            if (announce) Toast("♪ " + trackNames[idx]);
        }

        public string CurrentTrack => trackNames.Count > 0 ? trackNames[current] : "";

        void Toast(string s) { _toast = s; toastUntil = Time.unscaledTime + 2.5f; }
        string _toast = "";

        GUIStyle style;
        void OnGUI()
        {
            if (Time.unscaledTime > toastUntil) return;
            if (style == null)
                style = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(20), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = new Color(0.7f, 1f, 0.8f) } };
            var sh = new GUIStyle(style); sh.normal.textColor = Color.black;
            GUI.Label(new Rect(22f, Screen.height - 142f, 600f, 28f), _toast, sh);
            GUI.Label(new Rect(20f, Screen.height - 144f, 600f, 28f), _toast, style);
        }

        // ---------- Procedural fallback ----------
        AudioClip BuildTropicalLounge() => BakeChordLoop(new[] {
            new[] { 261f, 329f, 392f }, new[] { 220f, 261f, 329f },
            new[] { 174f, 220f, 261f }, new[] { 196f, 246f, 293f } }, 60, true, 0.4f);
        AudioClip BuildHeistSynth() => BakeChordLoop(new[] {
            new[] { 220f, 261f, 329f }, new[] { 196f, 246f, 293f },
            new[] { 174f, 220f, 261f }, new[] { 165f, 207f, 246f } }, 110, false, 0.65f);
        AudioClip BuildSpoonacciTheme() => BakeChordLoop(new[] {
            new[] { 293f, 369f, 440f }, new[] { 392f, 493f, 587f },
            new[] { 440f, 554f, 659f }, new[] { 293f, 369f, 440f } }, 90, true, 0.85f);
        AudioClip BuildNightDrive() => BakeChordLoop(new[] {
            new[] { 174f, 220f, 261f }, new[] { 130f, 174f, 220f },
            new[] { 220f, 277f, 329f }, new[] { 246f, 311f, 369f } }, 80, true, 0.3f);

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

                float pad = 0f;
                foreach (var f in chord)
                {
                    pad += Mathf.Sin(2f * Mathf.PI * f * t) * 0.18f;
                    pad += Mathf.Sin(2f * Mathf.PI * f * 0.5f * t) * 0.08f;
                }
                float melT = (t / beatSec) % 1f;
                float melEnv = sustain ? (Mathf.Sin(melT * Mathf.PI * 2f) * 0.5f + 0.5f) : Mathf.Exp(-melT * 4f);
                float melFreq = chord[chord.Length - 1] * 2f * (brightness > 0.5f ? 1f : 0.75f);
                float melody = Mathf.Sin(2f * Mathf.PI * melFreq * t) * melEnv * brightness * 0.18f;

                float beatPhase = (t / beatSec) % 1f;
                float beat = (beatPhase < 0.05f) ? Mathf.Sin(2f * Mathf.PI * 80f * t) * (1f - beatPhase / 0.05f) * 0.25f : 0f;

                float loopFade = 1f;
                float fadeSec = 0.05f;
                if (t < fadeSec) loopFade = t / fadeSec;
                float endT = sampleCount / (float)sr - t;
                if (endT < fadeSec) loopFade = Mathf.Min(loopFade, endT / fadeSec);

                data[i] = Mathf.Clamp((pad + melody + beat) * 0.55f * loopFade, -0.85f, 0.85f);
            }

            var c = AudioClip.Create("track", sampleCount, 1, sr, false);
            c.SetData(data, 0);
            return c;
        }
    }
}
