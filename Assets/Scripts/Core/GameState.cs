using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spoonacci
{
    // Global progression state — currency, bonk count, mission flags, etc.
    public static class GameState
    {
        public static int TrollTokens;
        public static int Bonks;
        public static int PerfectBonks;
        public static bool PoliceMode;
        public static bool CellCrewAllTalked;
        public static bool TunnelDug;
        public static bool JustEscaped;            // set on cell dig, consumed on first island spawn
        public static bool WearingCivilianClothes; // set after Salon Cucchiaio negotiation
        public static int  CurrentSkinIndex;
        public static bool IsNight;

        static readonly HashSet<string> _talked = new HashSet<string>();
        public static IReadOnlyCollection<string> TalkedSet => _talked;
        public static event Action OnChanged;

        public static void Reset()
        {
            TrollTokens = 0; Bonks = 0; PerfectBonks = 0;
            PoliceMode = false; CellCrewAllTalked = false; TunnelDug = false;
            JustEscaped = false; WearingCivilianClothes = false; CurrentSkinIndex = 0;
            _talked.Clear();
            OnChanged?.Invoke();
        }

        public static void OnNpcTalkedTo(string name)
        {
            if (_talked.Add(name))
            {
                if (_talked.Count >= 5) CellCrewAllTalked = true;
                OnChanged?.Invoke();
            }
        }

        public static int TalkedCount => _talked.Count;
        public static bool HasTalkedTo(string name) => _talked.Contains(name);

        public static void RegisterBonk(bool wasViolator)
        {
            Bonks++;
            if (wasViolator) { PerfectBonks++; TrollTokens += 10; }
            else             { TrollTokens += 1; }
            OnChanged?.Invoke();
        }

        public static void SpendTokens(int n)
        {
            TrollTokens = Mathf.Max(0, TrollTokens - n);
            OnChanged?.Invoke();
        }
    }
}
