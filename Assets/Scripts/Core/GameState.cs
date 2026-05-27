using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spoonacci
{
    // Global progression state — currency, bonk count, mission flags, talked-to spoons.
    // Static so anyone can read/write without wiring references.
    public static class GameState
    {
        public static int TrollTokens;
        public static int Bonks;
        public static int PerfectBonks;
        public static bool PoliceMode;
        public static bool CellCrewAllTalked;
        public static bool TunnelDug;

        static readonly HashSet<string> _talked = new HashSet<string>();
        public static event Action OnChanged;

        public static void Reset()
        {
            TrollTokens = 0; Bonks = 0; PerfectBonks = 0;
            PoliceMode = false; CellCrewAllTalked = false; TunnelDug = false;
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
            if (wasViolator)
            {
                PerfectBonks++;
                TrollTokens += 10;
            }
            else
            {
                TrollTokens += 1;
            }
            OnChanged?.Invoke();
        }
    }
}
