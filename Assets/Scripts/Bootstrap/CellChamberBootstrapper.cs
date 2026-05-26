using UnityEngine;

namespace Spoonacci
{
    // Stub — full implementation lives in CellChamberBootstrapper.partial.cs (Step B).
    // Kept separate so AutoBootstrap compiles in Step A even before cell content lands.
    public partial class CellChamberBootstrapper : MonoBehaviour
    {
        void Awake() => BuildIfNeeded();
        partial void BuildIfNeeded();
    }
}
