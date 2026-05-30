using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spoonacci
{
    // Narrative level order + a helper to jump straight to the next playable act.
    // Used by the "nextlevel" cheat and available to any other system that wants it.
    public static class SceneFlow
    {
        // The playable acts, in story order. (TitleScreen is the menu, not a level.)
        public static readonly string[] Levels = { "CutleryChamber", "SampleScene", "SharkFusion" };

        public static int IndexOf(string sceneName)
        {
            for (int i = 0; i < Levels.Length; i++)
                if (Levels[i] == sceneName) return i;
            return -1;
        }

        // Load the next level after the current scene, wrapping back to the first.
        // From the title screen (or any unknown scene) it starts at the first level.
        public static void JumpToNextLevel()
        {
            string cur = SceneManager.GetActiveScene().name;
            int idx = IndexOf(cur);
            int next = idx < 0 ? 0 : (idx + 1) % Levels.Length;
            LoadLevel(Levels[next]);
        }

        public static void LoadLevel(string target)
        {
            // The island (SampleScene) gates some content on having escaped the cell;
            // when we teleport straight in, mark the tunnel dug so it spawns correctly.
            if (target == "SampleScene") GameState.TunnelDug = true;
            try { SceneManager.LoadScene(target); }
            catch (System.Exception e)
            {
                Debug.LogError("[SceneFlow] LoadLevel('" + target + "') failed: " + e.Message +
                               " — is the scene in Build Settings?");
            }
        }
    }
}
