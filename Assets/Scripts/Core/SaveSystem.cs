using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Persistent save to Application.persistentDataPath/save.json.
    // Auto-saves every 30s + on scene load + F5 manual save.
    // Loads on Awake at game start.
    [System.Serializable]
    public class SaveData
    {
        public int trollTokens;
        public int bonks;
        public int perfectBonks;
        public bool policeMode;
        public bool cellCrewAllTalked;
        public bool tunnelDug;
        public bool justEscaped;
        public bool wearingCivilianClothes;
        public int currentSkinIndex;
        public List<string> talked = new List<string>();
        public List<string> completedMissions = new List<string>();
    }

    public class SaveSystem : MonoBehaviour
    {
        static SaveSystem _instance;
        public static SaveSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[SaveSystem]");
                    _instance = go.AddComponent<SaveSystem>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        float nextAuto;
        float toastEndsAt;
        string toast = "";
        GUIStyle toastStyle;

        public static string Path => System.IO.Path.Combine(Application.persistentDataPath, "save.json");

        void Awake()
        {
            Load();
        }

        void Update()
        {
            if (Time.time > nextAuto)
            {
                nextAuto = Time.time + 30f;
                Save("Auto-saved");
            }
            var kb = Keyboard.current;
            if (kb != null && kb.f5Key.wasPressedThisFrame)
                Save("Saved (F5)");
        }

        public void Save(string message)
        {
            var d = new SaveData
            {
                trollTokens = GameState.TrollTokens,
                bonks = GameState.Bonks,
                perfectBonks = GameState.PerfectBonks,
                policeMode = GameState.PoliceMode,
                cellCrewAllTalked = GameState.CellCrewAllTalked,
                tunnelDug = GameState.TunnelDug,
                justEscaped = GameState.JustEscaped,
                wearingCivilianClothes = GameState.WearingCivilianClothes,
                currentSkinIndex = GameState.CurrentSkinIndex,
                talked = new List<string>(GameState.TalkedSet),
                completedMissions = new List<string>(MissionManager.CompletedIds),
            };
            try
            {
                File.WriteAllText(Path, JsonUtility.ToJson(d, true));
                ShowToast(message);
                SoundFx.Instance.Saved();
            }
            catch (System.Exception e) { Debug.LogError("[Save] " + e.Message); }
        }

        public void Load()
        {
            if (!File.Exists(Path)) return;
            try
            {
                var json = File.ReadAllText(Path);
                var d = JsonUtility.FromJson<SaveData>(json);
                if (d == null) return;
                GameState.TrollTokens = d.trollTokens;
                GameState.Bonks = d.bonks;
                GameState.PerfectBonks = d.perfectBonks;
                GameState.PoliceMode = d.policeMode;
                GameState.CellCrewAllTalked = d.cellCrewAllTalked;
                GameState.TunnelDug = d.tunnelDug;
                GameState.JustEscaped = d.justEscaped;
                GameState.WearingCivilianClothes = d.wearingCivilianClothes;
                GameState.CurrentSkinIndex = d.currentSkinIndex;
                if (d.talked != null) foreach (var n in d.talked) GameState.OnNpcTalkedTo(n);
                MissionManager.RehydrateCompleted(d.completedMissions);
                Debug.Log("[Save] Loaded " + Path);
            }
            catch (System.Exception e) { Debug.LogError("[Save] Load failed: " + e.Message); }
        }

        void ShowToast(string s)
        {
            toast = s + " ✔";
            toastEndsAt = Time.time + 2f;
        }

        void OnGUI()
        {
            if (Time.time > toastEndsAt) return;
            if (toastStyle == null)
                toastStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(22), fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.5f, 1f, 0.6f) }, alignment = TextAnchor.MiddleCenter };
            float w = 320f;
            var sh = new GUIStyle(toastStyle); sh.normal.textColor = Color.black;
            float x = (Screen.width - w) * 0.5f;
            GUI.Label(new Rect(x + 2, 92f, w, 28f), toast, sh);
            GUI.Label(new Rect(x, 90f, w, 28f), toast, toastStyle);
        }
    }
}
