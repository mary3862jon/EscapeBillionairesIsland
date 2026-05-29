using UnityEngine;
using System.Collections.Generic;

namespace Spoonacci
{
    // Wild rooftop-style pool party at WorldLayout.Pool (-15, 0, -2):
    // bikini dancers + DJ booth + speaker stacks + strobing party lights +
    // fresh procedural party music every time the player approaches.
    public static class PoolPartyBuilder
    {
        public static void Build(Transform root)
        {
            // World center of the pool party zone (this builder is EXEMPT from
            // the Blocked() scatter rule for its OWN pool zone).
            Vector3 C = new Vector3(WorldLayout.Pool.x, 0f, WorldLayout.Pool.y); // (-15, 0, -2)

            var hub = BuildKit.Root("PoolParty", C);
            hub.transform.SetParent(root, true);
            Transform h = hub.transform;

            // ---------- palette ----------
            Color tan = new Color(0.82f, 0.58f, 0.40f);
            Color tanDark = new Color(0.74f, 0.50f, 0.34f);
            Color boothBody = new Color(0.08f, 0.08f, 0.11f);
            Color boothTrim = new Color(0.15f, 0.85f, 0.95f);
            Color speakerCol = new Color(0.06f, 0.06f, 0.07f);
            Color chrome = new Color(0.85f, 0.86f, 0.88f);

            // ============== DANCEFLOOR DECAL (flat, no collider) ==============
            var floor = BuildKit.Cube("DanceFloorPaint", h, new Vector3(2f, 0.02f, 6f),
                new Vector3(9f, 0.04f, 7f), new Color(0.18f, 0.12f, 0.28f), 0.1f, 0.85f);
            Object.Destroy(floor.GetComponent<Collider>());
            // checker accent strips on the floor
            for (int i = -1; i <= 1; i++)
            {
                var strip = BuildKit.Cube("FloorStrip", h, new Vector3(2f, 0.025f, 6f + i * 2.2f),
                    new Vector3(9f, 0.04f, 0.5f), new Color(0.95f, 0.25f, 0.75f), 0.1f, 0.9f);
                Object.Destroy(strip.GetComponent<Collider>());
            }

            // ============== DJ BOOTH (solid cubes, keep colliders) ==============
            Vector3 boothL = new Vector3(2f, 0f, 11.5f); // behind dancefloor, on the deck
            // booth base / counter
            BuildKit.Cube("DJCounter", h, boothL + new Vector3(0f, 0.75f, 0f),
                new Vector3(5.0f, 1.5f, 1.6f), boothBody, 0.25f, 0.6f);
            // glowing front trim panel
            var trim = BuildKit.Cube("DJTrim", h, boothL + new Vector3(0f, 0.85f, -0.82f),
                new Vector3(4.8f, 1.0f, 0.08f), boothTrim, 0.1f, 0.95f);
            Object.Destroy(trim.GetComponent<Collider>());
            // angled top deck
            var top = BuildKit.Cube("DJTop", h, boothL + new Vector3(0f, 1.62f, 0.1f),
                new Vector3(5.0f, 0.12f, 1.7f), new Color(0.12f, 0.12f, 0.15f), 0.3f, 0.6f);
            top.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
            // two turntables
            for (int t = -1; t <= 1; t += 2)
            {
                var plate = BuildKit.Cylinder("Turntable", h, boothL + new Vector3(t * 1.3f, 1.78f, 0.1f),
                    new Vector3(0.8f, 0.05f, 0.8f), new Color(0.05f, 0.05f, 0.06f), 0.4f, 0.7f);
                Object.Destroy(plate.GetComponent<Collider>());
                var disc = BuildKit.Cylinder("Vinyl", h, boothL + new Vector3(t * 1.3f, 1.82f, 0.1f),
                    new Vector3(0.62f, 0.04f, 0.62f), new Color(0.02f, 0.02f, 0.02f), 0.1f, 0.4f);
                Object.Destroy(disc.GetComponent<Collider>());
                // spinning record label
                var lbl = BuildKit.Cylinder("VinylLabel", h, boothL + new Vector3(t * 1.3f, 1.85f, 0.1f),
                    new Vector3(0.22f, 0.04f, 0.22f), t < 0 ? new Color(0.95f, 0.3f, 0.2f) : new Color(0.2f, 0.6f, 0.95f), 0.1f, 0.6f);
                Object.Destroy(lbl.GetComponent<Collider>());
                lbl.AddComponent<SpinY>().speed = t < 0 ? 220f : -180f;
            }
            // mixer knobs in the middle
            for (int k = 0; k < 4; k++)
            {
                var knob = BuildKit.Cylinder("MixerKnob", h, boothL + new Vector3(-0.3f + k * 0.2f, 1.74f, 0.55f),
                    new Vector3(0.07f, 0.05f, 0.07f), new Color(0.9f, 0.9f, 0.2f), 0.3f, 0.7f);
                Object.Destroy(knob.GetComponent<Collider>());
            }

            // ============== TWO BIG SPEAKER STACKS (solid, keep colliders) ==============
            BuildSpeakerStack(h, boothL + new Vector3(-3.6f, 0f, 0.2f), speakerCol);
            BuildSpeakerStack(h, boothL + new Vector3(3.6f, 0f, 0.2f), speakerCol);

            // ============== STROBING / SPINNING PARTY LIGHTS overhead ==============
            // a light rig truss above the dancefloor
            BuildKit.Cube("LightTruss", h, new Vector3(2f, 4.6f, 6f), new Vector3(9f, 0.18f, 0.18f), chrome, 0.7f, 0.7f);
            BuildKit.Cube("LightTrussZ", h, new Vector3(2f, 4.6f, 6f), new Vector3(0.18f, 0.18f, 7f), chrome, 0.7f, 0.7f);
            Color[] lightCols = {
                new Color(1f, 0.2f, 0.5f), new Color(0.2f, 0.7f, 1f),
                new Color(0.5f, 1f, 0.3f), new Color(1f, 0.8f, 0.2f)
            };
            Vector3[] lightSpots = {
                new Vector3(-1.5f, 4.4f, 4f), new Vector3(5.5f, 4.4f, 4f),
                new Vector3(-1.5f, 4.4f, 8f), new Vector3(5.5f, 4.4f, 8f)
            };
            for (int i = 0; i < lightSpots.Length; i++)
            {
                var lgo = new GameObject("PartyLight");
                lgo.transform.SetParent(h, false);
                lgo.transform.localPosition = lightSpots[i];
                var l = lgo.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = lightCols[i];
                l.intensity = 3.5f;
                l.range = 14f;
                var pl = lgo.AddComponent<PartyLight>();
                pl.baseColor = lightCols[i];
                pl.cyclePhase = i * 1.7f;
                // little fixture housing
                var fix = BuildKit.Sphere("LightHousing", h, lightSpots[i] + new Vector3(0f, 0.15f, 0f),
                    new Vector3(0.5f, 0.5f, 0.5f), new Color(0.1f, 0.1f, 0.1f), 0.5f, 0.6f);
            }
            // a slow-sweeping spotlight beam pair on the truss
            for (int s = -1; s <= 1; s += 2)
            {
                var sgo = new GameObject("SweepLight");
                sgo.transform.SetParent(h, false);
                sgo.transform.localPosition = new Vector3(2f + s * 3f, 4.5f, 6f);
                var sl = sgo.AddComponent<Light>();
                sl.type = LightType.Spot;
                sl.color = s < 0 ? new Color(0.4f, 0.9f, 1f) : new Color(1f, 0.4f, 0.9f);
                sl.intensity = 6f;
                sl.range = 18f;
                sl.spotAngle = 32f;
                sgo.transform.localRotation = Quaternion.Euler(60f, s * 30f, 0f);
                sgo.AddComponent<SweepLight>().dir = s;
            }

            // ============== PARTY AUDIO (tracks player, fresh clip each visit) ==============
            var audioGo = new GameObject("PoolPartyAudio");
            audioGo.transform.SetParent(h, false);
            audioGo.transform.localPosition = new Vector3(2f, 1.5f, 6f); // dancefloor center
            audioGo.AddComponent<PoolPartyAudio>();

            // ============== BIKINI DANCERS (6-8) ==============
            // Bright bikini color pairs (top, bottom).
            (Color top, Color bottom)[] bikinis = {
                (new Color(1f, 0.15f, 0.55f), new Color(1f, 0.35f, 0.65f)),     // hot pink
                (new Color(0.15f, 0.85f, 1f), new Color(0.1f, 0.6f, 0.95f)),    // cyan/blue
                (new Color(0.95f, 0.85f, 0.1f), new Color(0.95f, 0.55f, 0.1f)), // yellow/orange
                (new Color(0.5f, 1f, 0.3f), new Color(0.2f, 0.85f, 0.4f)),      // lime/green
                (new Color(0.9f, 0.2f, 0.2f), new Color(1f, 0.5f, 0.3f)),       // red/coral
                (new Color(0.75f, 0.3f, 1f), new Color(0.55f, 0.2f, 0.95f)),    // violet
                (new Color(1f, 0.4f, 0.75f), new Color(0.95f, 0.2f, 0.5f)),     // rose
                (new Color(0.2f, 1f, 0.85f), new Color(0.1f, 0.8f, 0.7f)),      // teal
            };
            Color[] hairCols = {
                new Color(0.12f, 0.08f, 0.05f), new Color(0.35f, 0.22f, 0.10f),
                new Color(0.85f, 0.72f, 0.40f), new Color(0.55f, 0.30f, 0.12f),
                new Color(0.05f, 0.05f, 0.06f), new Color(0.90f, 0.45f, 0.55f),
            };

            // 7 dancers: a ring around the dancefloor + a couple at the pool edge.
            // local positions relative to hub C; kept on the deck (not in roads).
            Vector3[] dancerLocals = {
                new Vector3(-0.5f, 0f, 5f),
                new Vector3(4.5f, 0f, 5f),
                new Vector3(0.5f, 0f, 7.5f),
                new Vector3(3.5f, 0f, 7.5f),
                new Vector3(2f, 0f, 4f),
                new Vector3(-2f, 0f, 7f),    // pool-edge left
                new Vector3(6f, 0f, 7f),     // pool-edge right
            };

            for (int i = 0; i < dancerLocals.Length; i++)
            {
                Vector3 world = C + dancerLocals[i];
                var bk = bikinis[i % bikinis.Length];

                // Build INACTIVE first so we can set the bikini/hair colours BEFORE
                // Awake() runs BuildBody() — otherwise the figure builds with defaults.
                var go = new GameObject("BikiniDancer" + i);
                go.SetActive(false);
                go.transform.position = world;
                var w = go.AddComponent<BikiniWoman>();
                w.mode = Civilian.Mode.Idle;
                w.bikiniTop = bk.top;
                w.bikiniBottom = bk.bottom;
                w.hairColor = hairCols[i % hairCols.Length];
                go.SetActive(true);   // now BuildBody() runs with the right colours

                // Take full control of the transform for dancing.
                var rb = w.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;
                go.AddComponent<DanceMove>();
            }
        }

        // ---------- helpers ----------

        static void BuildSpeakerStack(Transform h, Vector3 local, Color speakerCol)
        {
            Color coneCol = new Color(0.18f, 0.18f, 0.20f);
            // tall cabinet (solid, keep collider)
            BuildKit.Cube("SpeakerCab", h, local + new Vector3(0f, 1.4f, 0f),
                new Vector3(1.6f, 2.8f, 1.4f), speakerCol, 0.2f, 0.4f);
            // woofers + tweeter (flat discs, no collider)
            float[] coneY = { 0.7f, 1.7f, 2.4f };
            float[] coneR = { 0.55f, 0.55f, 0.28f };
            for (int i = 0; i < coneY.Length; i++)
            {
                var cone = BuildKit.Cylinder("SpeakerCone", h, local + new Vector3(0f, coneY[i], -0.72f),
                    new Vector3(coneR[i] * 2f, 0.06f, coneR[i] * 2f), coneCol, 0.2f, 0.5f);
                cone.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Object.Destroy(cone.GetComponent<Collider>());
                var ring = BuildKit.Cylinder("SpeakerRing", h, local + new Vector3(0f, coneY[i], -0.70f),
                    new Vector3(coneR[i] * 2.2f, 0.05f, coneR[i] * 2.2f), new Color(0.05f, 0.05f, 0.06f), 0.3f, 0.6f);
                ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Object.Destroy(ring.GetComponent<Collider>());
            }
            // pulsing glow strip on top
            var glow = BuildKit.Cube("SpeakerGlow", h, local + new Vector3(0f, 2.85f, 0f),
                new Vector3(1.5f, 0.08f, 1.3f), new Color(0.2f, 0.9f, 1f), 0.1f, 0.95f);
            Object.Destroy(glow.GetComponent<Collider>());
        }
    }

    // =========================================================================
    // DanceMove: animates the civ's own transform — bob, sway, hip wiggle.
    // =========================================================================
    public class DanceMove : MonoBehaviour
    {
        float phase;
        float bobSpeed;
        float bobAmp;
        float swaySpeed;
        float swayAmp;
        float wiggleSpeed;
        float wiggleAmp;
        Vector3 basePos;
        float baseYaw;

        void Awake()
        {
            // Per-instance deterministic-ish offset so dancers aren't in lockstep.
            int id = GetInstanceID();
            System.Random rng = new System.Random(id);
            phase = (float)rng.NextDouble() * Mathf.PI * 2f;
            bobSpeed = 4.5f + (float)rng.NextDouble() * 2.5f;   // ~4.5-7
            bobAmp = 0.10f + (float)rng.NextDouble() * 0.06f;   // gentle bounce
            swaySpeed = 2.0f + (float)rng.NextDouble() * 1.5f;
            swayAmp = 16f + (float)rng.NextDouble() * 14f;      // degrees of body sway
            wiggleSpeed = 3.0f + (float)rng.NextDouble() * 2.0f;
            wiggleAmp = 0.12f + (float)rng.NextDouble() * 0.08f; // lateral hip wiggle
            basePos = transform.position;
            baseYaw = transform.eulerAngles.y;
        }

        void Update()
        {
            float t = Time.time * bobSpeed + phase;

            // vertical bob (|sin| gives a bouncy on-beat feel)
            float bob = Mathf.Abs(Mathf.Sin(t)) * bobAmp;

            // lateral hip wiggle (perpendicular to facing)
            float wig = Mathf.Sin(Time.time * wiggleSpeed + phase) * wiggleAmp;
            Vector3 right = Quaternion.Euler(0f, baseYaw, 0f) * Vector3.right;

            transform.position = basePos + Vector3.up * bob + right * wig;

            // body sway rotation around Y + a little roll for groove
            float sway = Mathf.Sin(Time.time * swaySpeed + phase) * swayAmp;
            float roll = Mathf.Sin(Time.time * swaySpeed * 0.5f + phase) * (swayAmp * 0.25f);
            transform.rotation = Quaternion.Euler(0f, baseYaw + sway, roll);
        }
    }

    // =========================================================================
    // PartyLight: rotates fixture + cycles color/intensity (strobe-ish).
    // =========================================================================
    public class PartyLight : MonoBehaviour
    {
        public Color baseColor = Color.magenta;
        public float cyclePhase = 0f;
        Light l;
        float spin;

        void Awake()
        {
            l = GetComponent<Light>();
            spin = Random.Range(60f, 140f) * (Random.value > 0.5f ? 1f : -1f);
        }

        void Update()
        {
            transform.Rotate(Vector3.up, spin * Time.deltaTime, Space.Self);

            float t = Time.time * 2.2f + cyclePhase;
            // hue rotate around the base color by mixing with a sweeping accent
            float r = 0.5f + 0.5f * Mathf.Sin(t);
            float g = 0.5f + 0.5f * Mathf.Sin(t + 2.094f);
            float b = 0.5f + 0.5f * Mathf.Sin(t + 4.188f);
            Color cycle = new Color(r, g, b);
            if (l != null)
            {
                l.color = Color.Lerp(baseColor, cycle, 0.6f);
                // strobe pulse
                float pulse = 0.5f + 0.5f * Mathf.Abs(Mathf.Sin(Time.time * 8f + cyclePhase));
                l.intensity = 2.0f + pulse * 4.0f;
            }
        }
    }

    // =========================================================================
    // SweepLight: slowly sweeps a spotlight back and forth.
    // =========================================================================
    public class SweepLight : MonoBehaviour
    {
        public int dir = 1;
        float baseYaw;

        void Awake() { baseYaw = transform.localEulerAngles.y; }

        void Update()
        {
            float sweep = Mathf.Sin(Time.time * 0.8f) * 35f * dir;
            transform.localRotation = Quaternion.Euler(60f, baseYaw + sweep, 0f);
        }
    }

    // =========================================================================
    // SpinY: simple constant Y spin for turntable records.
    // =========================================================================
    public class SpinY : MonoBehaviour
    {
        public float speed = 200f;
        void Update() { transform.Rotate(Vector3.up, speed * Time.deltaTime, Space.Self); }
    }

    // =========================================================================
    // PoolPartyAudio: bakes 4 upbeat party loops at Awake. Tracks the player;
    // crossing INSIDE ~24u picks a RANDOM clip different from the last and Plays;
    // leaving Stops. Fresh track every visit.
    // =========================================================================
    public class PoolPartyAudio : MonoBehaviour
    {
        AudioSource src;
        AudioClip[] clips;
        int lastClip = -1;
        bool playerInside = false;
        Transform player;
        float repollAt;

        const float EnterRadius = 24f;
        const float ExitRadius = 27f; // hysteresis so it doesn't flicker at the edge

        void Awake()
        {
            src = gameObject.AddComponent<AudioSource>();
            src.spatialBlend = 1f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.minDistance = 4f;
            src.maxDistance = 36f;
            src.loop = true;
            src.volume = 0.6f;
            src.playOnAwake = false;

            clips = new AudioClip[4];
            clips[0] = BakeLoop("PartyLoop0", 124f, 0);
            clips[1] = BakeLoop("PartyLoop1", 128f, 1);
            clips[2] = BakeLoop("PartyLoop2", 122f, 2);
            clips[3] = BakeLoop("PartyLoop3", 130f, 3);
        }

        void Update()
        {
            if (player == null && Time.time >= repollAt)
            {
                repollAt = Time.time + 1.5f;
                ResolvePlayer();
            }
            if (player == null) return;

            float d = Vector3.Distance(transform.position, player.position);

            if (!playerInside && d < EnterRadius)
            {
                playerInside = true;
                PlayFreshTrack();
            }
            else if (playerInside && d > ExitRadius)
            {
                playerInside = false;
                src.Stop();
            }
        }

        void ResolvePlayer()
        {
            var spoon = Object.FindFirstObjectByType<SpoonController>();
            if (spoon != null) { player = spoon.transform; return; }
            if (Camera.main != null) player = Camera.main.transform;
        }

        void PlayFreshTrack()
        {
            if (clips == null || clips.Length == 0) return;
            int idx = Random.Range(0, clips.Length);
            if (clips.Length > 1)
            {
                int guard = 0;
                while (idx == lastClip && guard++ < 8) idx = Random.Range(0, clips.Length);
            }
            lastClip = idx;
            src.clip = clips[idx];
            src.Play();
        }

        // Bake a ~10s upbeat loop: 4-on-the-floor kick + bright sine arpeggio chords.
        AudioClip BakeLoop(string name, float bpm, int variant)
        {
            int sampleRate = 22050;
            float beat = 60f / bpm;            // seconds per beat
            int bars = 8;                      // 8 bars of 4/4
            float lengthSec = beat * 4f * bars;
            int n = Mathf.RoundToInt(lengthSec * sampleRate);
            float[] data = new float[n];

            // arpeggio note tables per variant (semitone offsets from a root)
            float rootHz = 220f + variant * 18f; // A3-ish, slight variant detune
            int[][] arps = {
                new[] { 0, 4, 7, 11, 7, 4 },     // maj7 shimmer
                new[] { 0, 3, 7, 10, 12, 7 },    // min7 groove
                new[] { 0, 5, 7, 12, 7, 5 },     // sus / open
                new[] { 0, 4, 7, 9, 12, 9 },     // 6th add bright
            };
            int[] arp = arps[variant % arps.Length];

            float arpStep = beat * 0.5f; // eighth notes
            int arpSamples = Mathf.RoundToInt(arpStep * sampleRate);
            int kickSamples = Mathf.RoundToInt(beat * sampleRate);

            for (int i = 0; i < n; i++)
            {
                float tSec = (float)i / sampleRate;

                // ---- 4-on-the-floor kick: a decaying low sine pitch-drop each beat ----
                int posInBeat = i % kickSamples;
                float kEnv = Mathf.Exp(-(float)posInBeat / sampleRate * 22f);
                float kPhase = (float)posInBeat / sampleRate;
                float kFreq = 120f - 80f * Mathf.Clamp01(kPhase * 12f); // 120 -> 40 Hz drop
                float kick = Mathf.Sin(2f * Mathf.PI * kFreq * kPhase) * kEnv * 0.55f;

                // ---- bright arpeggio sine chord ----
                int arpIndex = (i / arpSamples) % arp.Length;
                int posInArp = i % arpSamples;
                float aEnv = Mathf.Exp(-(float)posInArp / sampleRate * 8f) * 0.9f + 0.1f;
                float noteHz = rootHz * Mathf.Pow(2f, arp[arpIndex] / 12f);
                // octave-up sparkle layer
                float arpVoice = Mathf.Sin(2f * Mathf.PI * noteHz * tSec) * 0.22f
                               + Mathf.Sin(2f * Mathf.PI * noteHz * 2f * tSec) * 0.10f;
                arpVoice *= aEnv;

                // ---- offbeat hat: short noise burst on the "and" ----
                float hat = 0f;
                int posInHalf = i % Mathf.RoundToInt(beat * 0.5f * sampleRate);
                int halfIndex = (i / Mathf.RoundToInt(beat * 0.5f * sampleRate));
                if ((halfIndex & 1) == 1)
                {
                    float hEnv = Mathf.Exp(-(float)posInHalf / sampleRate * 60f);
                    hat = (Random.value * 2f - 1f) * hEnv * 0.12f;
                }

                // ---- sub bass pulse following the kick beat (sawish) ----
                float bassHz = rootHz * 0.5f;
                float bass = Mathf.Sin(2f * Mathf.PI * bassHz * tSec) * 0.14f;

                float s = kick + arpVoice + hat + bass;
                // soft clip
                if (s > 1f) s = 1f; else if (s < -1f) s = -1f;
                data[i] = s * 0.85f;
            }

            // smooth the loop seam (short crossfade of the tail into the head)
            int fade = Mathf.Min(sampleRate / 20, n / 4);
            for (int i = 0; i < fade; i++)
            {
                float g = (float)i / fade;
                int tail = n - fade + i;
                data[tail] = data[tail] * (1f - g) + data[i] * g;
            }

            var clip = AudioClip.Create(name, n, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
