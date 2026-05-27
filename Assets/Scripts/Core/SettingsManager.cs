using UnityEngine;

namespace Spoonacci
{
    // Global settings — resolution, framerate, audio volumes, FOV.
    // Persisted via PlayerPrefs (independent of save slots).
    public static class SettingsManager
    {
        public struct Resolution { public int w, h; public string label;
            public Resolution(int w, int h, string l) { this.w = w; this.h = h; this.label = l; } }

        public static readonly Resolution[] Resolutions =
        {
            new Resolution(1280, 720,  "HD 720p"),
            new Resolution(1920, 1080, "FHD 1080p"),
            new Resolution(2560, 1440, "2K 1440p"),
            new Resolution(3840, 2160, "4K 2160p"),
        };

        public static readonly int[] FrameRates = { 30, 60, 120, 144, -1 };
        public static string FrameRateLabel(int fr) => fr <= 0 ? "Unlimited" : fr.ToString();

        public static int ResolutionIndex = 1; // FHD default
        public static int FrameRateIndex  = 1; // 60 default
        public static bool Fullscreen     = true;
        public static float MasterVolume  = 0.85f;
        public static float MusicVolume   = 0.7f;
        public static float SfxVolume     = 0.85f;
        public static float DialogueVolume = 0.9f;
        public static float Fov           = 65f;

        public static System.Action OnChanged;

        public static void Load()
        {
            ResolutionIndex = PlayerPrefs.GetInt("settings.res", ResolutionIndex);
            FrameRateIndex  = PlayerPrefs.GetInt("settings.fr",  FrameRateIndex);
            Fullscreen      = PlayerPrefs.GetInt("settings.fs",  Fullscreen ? 1 : 0) == 1;
            MasterVolume    = PlayerPrefs.GetFloat("settings.vol.master", MasterVolume);
            MusicVolume     = PlayerPrefs.GetFloat("settings.vol.music",  MusicVolume);
            SfxVolume       = PlayerPrefs.GetFloat("settings.vol.sfx",    SfxVolume);
            DialogueVolume  = PlayerPrefs.GetFloat("settings.vol.dlg",    DialogueVolume);
            Fov             = PlayerPrefs.GetFloat("settings.fov",        Fov);
            Apply();
        }

        public static void Save()
        {
            PlayerPrefs.SetInt("settings.res", ResolutionIndex);
            PlayerPrefs.SetInt("settings.fr",  FrameRateIndex);
            PlayerPrefs.SetInt("settings.fs",  Fullscreen ? 1 : 0);
            PlayerPrefs.SetFloat("settings.vol.master", MasterVolume);
            PlayerPrefs.SetFloat("settings.vol.music",  MusicVolume);
            PlayerPrefs.SetFloat("settings.vol.sfx",    SfxVolume);
            PlayerPrefs.SetFloat("settings.vol.dlg",    DialogueVolume);
            PlayerPrefs.SetFloat("settings.fov",        Fov);
            PlayerPrefs.Save();
            Apply();
            OnChanged?.Invoke();
        }

        public static void Apply()
        {
            ResolutionIndex = Mathf.Clamp(ResolutionIndex, 0, Resolutions.Length - 1);
            var r = Resolutions[ResolutionIndex];
            Screen.SetResolution(r.w, r.h, Fullscreen);

            FrameRateIndex = Mathf.Clamp(FrameRateIndex, 0, FrameRates.Length - 1);
            Application.targetFrameRate = FrameRates[FrameRateIndex];

            // FOV: applied via Camera.main + ThirdPersonCamera.fov each LateUpdate-ish
            if (Camera.main != null)
            {
                Camera.main.fieldOfView = Fov;
                var follower = Camera.main.GetComponent<ThirdPersonCamera>();
                if (follower != null) follower.fov = Fov;
            }
        }

        // multiplied volumes (clamp 0..1)
        public static float EffectiveMusic    => Mathf.Clamp01(MusicVolume * MasterVolume);
        public static float EffectiveSfx      => Mathf.Clamp01(SfxVolume * MasterVolume);
        public static float EffectiveDialogue => Mathf.Clamp01(DialogueVolume * MasterVolume);
    }
}
