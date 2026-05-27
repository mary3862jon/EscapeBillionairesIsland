using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spoonacci
{
    // Chained-quest framework. Missions have id, title, prerequisite, completion check.
    // The active list = missions whose prereqs are all complete + not done themselves.
    public class Mission
    {
        public string id;
        public string title;
        public Func<bool> isComplete;
        public List<string> requires = new List<string>();
        public string scene;        // optional: where it applies (filter for tracker)

        public Mission(string id, string title, Func<bool> isComplete, string scene = null, params string[] reqs)
        {
            this.id = id; this.title = title; this.isComplete = isComplete; this.scene = scene;
            if (reqs != null) requires.AddRange(reqs);
        }
    }

    public static class MissionManager
    {
        static readonly Dictionary<string, Mission> _all = new Dictionary<string, Mission>();
        static readonly HashSet<string> _completedIds = new HashSet<string>();
        public static IReadOnlyCollection<string> CompletedIds => _completedIds;

        public static void Register(Mission m) { _all[m.id] = m; }

        public static void RehydrateCompleted(List<string> ids)
        {
            _completedIds.Clear();
            if (ids != null) foreach (var i in ids) _completedIds.Add(i);
        }

        public static IEnumerable<Mission> ActiveFor(string sceneName)
        {
            foreach (var m in _all.Values)
            {
                if (m.scene != null && m.scene != sceneName) continue;
                if (_completedIds.Contains(m.id)) continue;
                bool ready = true;
                foreach (var r in m.requires)
                    if (!_completedIds.Contains(r)) { ready = false; break; }
                if (!ready) continue;
                yield return m;
            }
        }

        public static void Tick()
        {
            foreach (var m in _all.Values)
            {
                if (_completedIds.Contains(m.id)) continue;
                if (m.isComplete != null && m.isComplete())
                {
                    _completedIds.Add(m.id);
                    SoundFx.Instance.LevelUp();
                }
            }
        }

        public static bool IsComplete(string id) => _completedIds.Contains(id);

        public static void BootstrapDefaults()
        {
            // Cell scene
            Register(new Mission("cell.talk",       "Talk to all 5 Cell Crew",                () => GameState.CellCrewAllTalked, "Cutlery"));
            Register(new Mission("cell.pickaxe",    "Find a pickaxe (try the hay pile)",      () => PickaxeState.Found, "Cutlery"));
            Register(new Mission("cell.dig",        "Dig the loose stone (E)",                () => GameState.TunnelDug, "Cutlery", "cell.talk", "cell.pickaxe"));

            // Island arrival (post-escape)
            Register(new Mission("island.salon",    "Ditch the police uniform at Salon Cucchiaio (E)", () => GameState.WearingCivilianClothes, "Sample", "cell.dig"));

            // Sandbox / chaos missions
            Register(new Mission("isle.bonk5",      "Bonk 5 violators",                       () => GameState.PerfectBonks >= 5, "Sample"));
            Register(new Mission("isle.tokens100",  "Earn 100 Troll Tokens",                  () => GameState.TrollTokens >= 100, "Sample"));
            Register(new Mission("isle.bust",       "Enter Police Mode (P) and bust dealers", () => GameState.PoliceMode && GameState.PerfectBonks >= 10, "Sample"));
            Register(new Mission("isle.tusk",       "Bonk Magnus Tusk (the Billionaire)",     () => MagnusTuskState.Bonked, "Sample"));

            // Ocean / shark
            Register(new Mission("shark.fuse",      "Walk into the ocean → fuse with the Shark", () => SharkFusionState.Fused, "Shark"));
        }
    }

    // Tiny static "events" — set from gameplay code.
    public static class PickaxeState { public static bool Found; }
    public static class MagnusTuskState { public static bool Bonked; }
    public static class SharkFusionState { public static bool Fused; }
}
