using UnityEngine;

namespace Spoonacci
{
    // Global prison alarm state. Triggered when warden sees a dig in progress.
    // Adds: looping siren (separate from PoliceMode siren), 2 flashing red point lights, screen-edge tint, big banner.
    public class PrisonAlarm : MonoBehaviour
    {
        public static bool PlayerIsDigging;       // LooseStone sets this true while a dig stage is active
        public static bool WardenSeesDig;          // PrisonWarden sets this each frame
        public static bool Active;                  // alarm currently on
        public static float AlarmEndsAt;

        Light flash1, flash2;
        AudioSource alarmSrc;
        GUIStyle bannerStyle;

        void Awake()
        {
            BuildFlashers();
            BuildAlarmAudio();
        }

        void BuildFlashers()
        {
            var f1 = new GameObject("AlarmRed1");
            f1.transform.SetParent(transform, false);
            f1.transform.localPosition = new Vector3(-15f, 5.5f, 0f);
            flash1 = f1.AddComponent<Light>();
            flash1.type = LightType.Point; flash1.color = Color.red; flash1.range = 25f; flash1.intensity = 0f;

            var f2 = new GameObject("AlarmRed2");
            f2.transform.SetParent(transform, false);
            f2.transform.localPosition = new Vector3(15f, 5.5f, 0f);
            flash2 = f2.AddComponent<Light>();
            flash2.type = LightType.Point; flash2.color = Color.red; flash2.range = 25f; flash2.intensity = 0f;
        }

        void BuildAlarmAudio()
        {
            alarmSrc = gameObject.AddComponent<AudioSource>();
            alarmSrc.loop = true;
            alarmSrc.spatialBlend = 0f;
            alarmSrc.volume = 0.7f;
            alarmSrc.clip = BakeAlarmClip();
        }

        static AudioClip BakeAlarmClip()
        {
            int sr = 22050;
            int len = sr * 2;
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)sr;
                // pulsing 3-tone alarm: 800Hz blips every 0.25s with a triangle envelope
                float beat = (t * 4f) % 1f;
                float freq = beat < 0.5f ? 850f : 600f;
                float env = 1f - Mathf.Abs((beat * 2f) - 1f);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.6f;
            }
            var c = AudioClip.Create("alarm_loop", len, 1, sr, false); c.SetData(data, 0); return c;
        }

        public static void SetWardenSeesDig(bool sees)
        {
            WardenSeesDig = sees;
            if (sees && PlayerIsDigging) Trigger();
        }

        public static void OnWardenKnocked()
        {
            WardenSeesDig = false;
            Stop();
        }

        public static void Trigger()
        {
            if (!Active) AlarmEndsAt = Time.time + 4f;
            else AlarmEndsAt = Mathf.Max(AlarmEndsAt, Time.time + 4f);
            Active = true;
        }

        public static void Stop()
        {
            Active = false;
            AlarmEndsAt = 0f;
        }

        void Update()
        {
            if (Active && Time.time > AlarmEndsAt) Stop();

            if (Active)
            {
                float p = Mathf.Sin(Time.time * 14f) > 0f ? 6f : 0f;
                if (flash1 != null) flash1.intensity = p;
                if (flash2 != null) flash2.intensity = 6f - p;
                if (alarmSrc != null && !alarmSrc.isPlaying)
                {
                    alarmSrc.volume = 0.7f * SettingsManager.EffectiveSfx;
                    alarmSrc.Play();
                }
                else if (alarmSrc != null) alarmSrc.volume = 0.7f * SettingsManager.EffectiveSfx;
            }
            else
            {
                if (flash1 != null) flash1.intensity = 0f;
                if (flash2 != null) flash2.intensity = 0f;
                if (alarmSrc != null && alarmSrc.isPlaying) alarmSrc.Stop();
            }
        }

        void OnGUI()
        {
            if (!Active) return;
            if (bannerStyle == null)
                bannerStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(36), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.2f, 0.2f) } };
            var prev = GUI.color;
            // pulsing red edge
            float alpha = 0.25f + Mathf.Sin(Time.unscaledTime * 14f) * 0.2f;
            GUI.color = new Color(1f, 0f, 0f, alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, 18f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, Screen.height - 18f, Screen.width, 18f), Texture2D.whiteTexture);
            GUI.color = prev;
            var sh = new GUIStyle(bannerStyle); sh.normal.textColor = Color.black;
            GUI.Label(new Rect(0, 120, Screen.width, 60), Loc.T("alarm.banner"), sh);
            GUI.Label(new Rect(0, 118, Screen.width, 60), Loc.T("alarm.banner"), bannerStyle);
        }
    }
}
