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
            sceneName = sceneName ?? "";
            foreach (var m in _all.Values)
            {
                // accept partial matches so "Cutlery" matches "CutleryChamber", "Sample" matches "SampleScene", etc.
                if (m.scene != null && !sceneName.Contains(m.scene)) continue;
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
            // mission.title is now a localization KEY, looked up at render time via Loc.T()
            Register(new Mission("cell.pickaxe",  "mission.cell.pickaxe",  () => PickaxeState.Found, "Cutlery"));
            Register(new Mission("cell.unlock",   "mission.cell.unlock",   () => PrisonState.Unlocked >= 4, "Cutlery"));
            Register(new Mission("cell.persuade", "mission.cell.persuade", () => PrisonState.Persuaded >= 15, "Cutlery", "cell.unlock"));
            Register(new Mission("cell.dig",      "mission.cell.dig",      () => GameState.TunnelDug, "Cutlery", "cell.pickaxe", "cell.persuade"));

            Register(new Mission("island.salon",  "mission.island.salon",  () => GameState.WearingCivilianClothes, "Sample", "cell.dig"));

            Register(new Mission("isle.bonk5",    "mission.isle.bonk5",    () => GameState.PerfectBonks >= 5, "Sample"));
            Register(new Mission("isle.tokens100","mission.isle.tokens100",() => GameState.TrollTokens >= 100, "Sample"));
            Register(new Mission("isle.bust",     "mission.isle.bust",     () => GameState.PoliceMode && GameState.PerfectBonks >= 10, "Sample"));
            Register(new Mission("isle.tusk",     "mission.isle.tusk",     () => BillionaireRegistry.IsBonked("Magnus Tusk"), "Sample"));
            Register(new Mission("isle.boss.tusk","mission.isle.boss.tusk",() => BillionaireRegistry.IsBonked("Magnus Tusk"), "Sample"));
            Register(new Mission("isle.beff",     "mission.isle.beff",     () => BillionaireRegistry.IsBonked("Beff Jezos"), "Sample"));
            Register(new Mission("isle.boss.beff","mission.isle.boss.beff",() => BillionaireRegistry.IsBonked("Beff Jezos"), "Sample"));
            Register(new Mission("isle.chad",     "mission.isle.chad",     () => BillionaireRegistry.IsBonked("Crypto Chad"), "Sample"));
            Register(new Mission("isle.boss.chad","mission.isle.boss.chad",() => BillionaireRegistry.IsBonked("Crypto Chad"), "Sample"));
            Register(new Mission("isle.zuck",     "mission.isle.zuck",     () => BillionaireRegistry.IsBonked("Mark Zuckersnort"), "Sample"));
            Register(new Mission("isle.boss.zuck","mission.isle.boss.zuck",() => BillionaireRegistry.IsBonked("Mark Zuckersnort"), "Sample"));
            Register(new Mission("isle.all4",     "mission.isle.all4",     () => BillionaireRegistry.BonkedCount >= 4, "Sample", "isle.tusk", "isle.beff", "isle.chad", "isle.zuck"));

            Register(new Mission("shark.fuse",    "mission.shark.fuse",    () => SharkFusionState.Fused, "Shark"));
        }
    }

    // Tiny static "events" — set from gameplay code.
    public static class PickaxeState { public static bool Found; }
    public static class MagnusTuskState { public static bool Bonked; }
    public static class SharkFusionState { public static bool Fused; }

    public static class BillionaireRegistry
    {
        static readonly System.Collections.Generic.HashSet<string> _bonked = new System.Collections.Generic.HashSet<string>();
        public static void MarkBonked(string name) => _bonked.Add(name);
        public static bool IsBonked(string name) => _bonked.Contains(name);
        public static int BonkedCount => _bonked.Count;
    }
}
