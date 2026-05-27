using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spoonacci
{
    // Runs automatically after every scene load.
    // Looks at the active scene name and spawns the matching bootstrapper.
    // Saves us from hand-editing scene YAML and ensures every scene "just works"
    // when Boba hits Play, with no Inspector wiring required.
    public static class AutoBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnSceneReady()
        {
            SceneManager.sceneLoaded -= HandleLoaded; // safety
            SceneManager.sceneLoaded += HandleLoaded;
            Spawn(SceneManager.GetActiveScene());
        }

        static void HandleLoaded(Scene s, LoadSceneMode mode) => Spawn(s);

        static void Spawn(Scene s)
        {
            // already spawned? skip.
            if (Object.FindFirstObjectByType<IslandBootstrapper>() != null) return;
            if (Object.FindFirstObjectByType<CellChamberBootstrapper>() != null) return;
            if (Object.FindFirstObjectByType<SharkFusionBootstrapper>() != null) return;

            var go = new GameObject("[Auto Bootstrap]");
            // intentionally NOT DontDestroyOnLoad — we want each scene load to spawn the right bootstrapper fresh
            string name = s.name ?? "";
            if (name.Contains("Cutlery") || name.Contains("Cell"))
                go.AddComponent<CellChamberBootstrapper>();
            else if (name.Contains("Shark") || name.Contains("Fusion"))
                go.AddComponent<SharkFusionBootstrapper>();
            else
                go.AddComponent<IslandBootstrapper>();
        }
    }
}
