using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spoonacci
{
    // Title screen with NEW GAME / CONTINUE / QUIT. Shows giant rotating spoon.
    public class TitleScreenBootstrapper : MonoBehaviour
    {
        GameObject bigSpoon;
        Camera cam;
        GUIStyle titleStyle, btnStyle, footerStyle;

        void Awake()
        {
            // make sure cursor is usable in built games (some setups hide it by default)
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            _ = SoundFx.Instance;
            _ = SaveSystem.Instance;
            MissionManager.BootstrapDefaults();
            SettingsManager.Load();
            _ = MusicPlayer.Instance;
            gameObject.AddComponent<PostFxBoost>();
            gameObject.AddComponent<LanguageToggle>();
            // No PauseMenu on the title screen — title already has its own buttons
            BuildScene();
        }

        void BuildScene()
        {
            // sky
            RenderSettings.ambientLight = new Color(0.75f, 0.7f, 0.85f);
            RenderSettings.ambientIntensity = 1.5f;

            // sun (key)
            var sunGo = new GameObject("Sun");
            sunGo.transform.rotation = Quaternion.Euler(45f, 30f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.8f;
            sun.color = new Color(1f, 0.95f, 0.85f);
            sun.shadows = LightShadows.Soft;

            // fill (warm pink from below for the spoon)
            var fillGo = new GameObject("Fill");
            fillGo.transform.rotation = Quaternion.Euler(-25f, 200f, 0f);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.8f;
            fill.color = new Color(1f, 0.7f, 0.85f);

            // a rim/back light
            var rimGo = new GameObject("Rim");
            rimGo.transform.rotation = Quaternion.Euler(35f, 180f, 0f);
            var rim = rimGo.AddComponent<Light>();
            rim.type = LightType.Directional;
            rim.intensity = 1.0f;
            rim.color = new Color(0.6f, 0.85f, 1f);

            // point light right at the spoon for sparkle
            var pGo = new GameObject("Spoon Spot");
            pGo.transform.position = new Vector3(0f, 3.5f, -2f);
            var p = pGo.AddComponent<Light>();
            p.type = LightType.Point;
            p.color = new Color(1f, 0.95f, 0.7f);
            p.intensity = 4f;
            p.range = 10f;

            // floor — pink marble
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(6f, 1f, 6f);
            floor.GetComponent<Renderer>().sharedMaterial = ShaderCache.MakeTextured(ProceduralTextures.Marble, new Color(1f, 0.7f, 0.85f), 0.1f, 0.6f, new Vector2(8f, 8f));

            // big rotating spoon
            bigSpoon = new GameObject("Title Spoon");
            bigSpoon.transform.position = new Vector3(0f, 1.5f, 0f);
            bigSpoon.transform.localScale = Vector3.one * 4f;
            bigSpoon.AddComponent<ProceduralSpoonBuilder>();

            // camera
            var camGo = new GameObject("Main Camera");
            cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();
            cam.fieldOfView = 50f;
            cam.transform.position = new Vector3(0f, 4f, -8f);
            cam.transform.LookAt(bigSpoon.transform.position + Vector3.up * 1.5f);
            cam.backgroundColor = new Color(0.55f, 0.4f, 0.7f);

            // some floating coins for flair
            for (int i = 0; i < 14; i++)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                c.transform.position = new Vector3(Random.Range(-6f, 6f), Random.Range(1f, 5f), Random.Range(-3f, 3f));
                c.transform.localScale = new Vector3(0.3f, 0.04f, 0.3f);
                Destroy(c.GetComponent<Collider>());
                c.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.78f, 0.25f), 1f, 0.9f);
                c.AddComponent<TitleCoinFloat>();
            }
        }

        void Update()
        {
            if (bigSpoon != null)
                bigSpoon.transform.Rotate(Vector3.up, 28f * Time.deltaTime);
        }

        void OnGUI()
        {
            EnsureStyles();

            // title
            string title = "SIR  SPOONACCI";
            float tw = 900f; float th = 100f;
            float tx = (Screen.width - tw) * 0.5f; float ty = 40f;
            var sh = new GUIStyle(titleStyle); sh.normal.textColor = Color.black;
            GUI.Label(new Rect(tx + 4, ty + 4, tw, th), title, sh);
            GUI.Label(new Rect(tx, ty, tw, th), title, titleStyle);

            var sub = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(26), fontStyle = FontStyle.Italic, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.85f, 0.4f) } };
            GUI.Label(new Rect(tx, ty + 90f, tw, 40f), Loc.T("title.subtitle"), sub);

            // buttons
            float bw = 380f, bh = 70f;
            float bx = (Screen.width - bw) * 0.5f;
            float by = Screen.height * 0.45f;
            if (GUI.Button(new Rect(bx, by, bw, bh), Loc.T("title.new_game"), btnStyle))
                StartNew();
            if (GUI.Button(new Rect(bx, by + bh + 18f, bw, bh), Loc.T("title.continue"), btnStyle))
                Continue();
            if (GUI.Button(new Rect(bx, by + (bh + 18f) * 2f, bw, bh), Loc.T("title.quit"), btnStyle))
                Quit();

            // footer with save info
            string footer = System.IO.File.Exists(SaveSystem.Path)
                ? Loc.T("title.save_found") + "  ·  " + Loc.T("title.tokens") + ": " + GameState.TrollTokens + "  ·  " + Loc.T("title.missions") + ": " + MissionManager.CompletedIds.Count
                : Loc.T("title.no_save");
            GUI.Label(new Rect(20f, Screen.height - 40f, Screen.width - 40f, 24f), footer, footerStyle);

            // build/version
            var v = new GUIStyle(footerStyle); v.alignment = TextAnchor.MiddleRight;
            GUI.Label(new Rect(20f, Screen.height - 40f, Screen.width - 40f, 24f), "v0.6 dev build", v);
        }

        void EnsureStyles()
        {
            if (titleStyle == null)
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(80), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.95f, 0.7f) } };
            if (btnStyle == null)
                btnStyle = new GUIStyle(GUI.skin.button) { fontSize = UiScale.Font(30), fontStyle = FontStyle.Bold };
            if (footerStyle == null)
                footerStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(16), normal = { textColor = new Color(0.95f, 0.95f, 0.95f, 0.7f) } };
        }

        void StartNew()
        {
            try
            {
                GameState.Reset();
                MissionManager.RehydrateCompleted(null);
                SoundFx.Instance.LevelUp();
                Debug.Log("[Title] Loading CutleryChamber...");
                SceneManager.LoadScene("CutleryChamber");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Title] StartNew failed: " + e.Message + " — scene 'CutleryChamber' likely not in Build Settings.");
                LoadIndexFallback(2); // CutleryChamber is index 2 per build settings
            }
        }

        void Continue()
        {
            try
            {
                SoundFx.Instance.Chime();
                string next = GameState.TunnelDug ? "SampleScene" : "CutleryChamber";
                Debug.Log("[Title] Continue → " + next);
                SceneManager.LoadScene(next);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Title] Continue failed: " + e.Message);
                LoadIndexFallback(GameState.TunnelDug ? 1 : 2);
            }
        }

        void LoadIndexFallback(int idx)
        {
            // Try by build index as a last resort
            try { SceneManager.LoadScene(idx); }
            catch (System.Exception e) { Debug.LogError("[Title] Even index load failed: " + e.Message + " — check Build Profiles → Scenes In Build."); }
        }

        void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        static Material MakeMat(Color c, float metallic, float smoothness)
        {
            var sh = ShaderCache.Lit;
            var m = new Material(sh) { color = c };
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Smoothness", smoothness);
            return m;
        }
    }

    public class TitleCoinFloat : MonoBehaviour
    {
        float phase;
        Vector3 origin;
        void Awake() { origin = transform.position; phase = Random.value * 10f; transform.localRotation = Quaternion.Euler(Random.value * 90f, Random.value * 360f, Random.value * 90f); }
        void Update()
        {
            transform.position = origin + Vector3.up * Mathf.Sin(Time.time + phase) * 0.3f;
            transform.Rotate(Vector3.up, 60f * Time.deltaTime, Space.Self);
        }
    }
}
