using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Spoonacci
{
    // After the player has talked to all 5 Cell Crew, the loose stone glows red-orange.
    // Walk close + press E → "digging…" progress bar → fades out → loads SharkFusion scene.
    public class LooseStone : MonoBehaviour
    {
        Material mat;
        bool playerInside;
        float digProgress; // 0..1
        bool digging;
        GUIStyle promptStyle, progressStyle;

        void Awake()
        {
            var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.transform.SetParent(transform, false);
            stone.transform.localPosition = Vector3.zero;
            stone.transform.localScale = new Vector3(1f, 0.6f, 1f);
            var sh = Shader.Find("Universal Render Pipeline/Lit");
            mat = new Material(sh) { color = new Color(0.3f, 0.3f, 0.32f) };
            mat.SetFloat("_Metallic", 0.0f);
            mat.SetFloat("_Smoothness", 0.25f);
            mat.EnableKeyword("_EMISSION");
            stone.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(stone.GetComponent<Collider>());

            var trig = gameObject.AddComponent<SphereCollider>();
            trig.isTrigger = true;
            trig.radius = 1.6f;

            // a faint warm light when activated
            var glow = new GameObject("StoneGlow");
            glow.transform.SetParent(transform, false);
            glow.transform.localPosition = Vector3.up * 0.6f;
            var lt = glow.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = new Color(1f, 0.5f, 0.2f);
            lt.range = 4f;
            lt.intensity = 0f;
            _glowLight = lt;
        }

        Light _glowLight;

        void Update()
        {
            // pulse glow once tutorial unlocked
            bool active = GameState.CellCrewAllTalked && !GameState.TunnelDug;
            if (mat != null)
            {
                Color emit = active
                    ? new Color(0.9f, 0.35f, 0.1f) * (0.5f + Mathf.Sin(Time.time * 4f) * 0.5f)
                    : Color.black;
                mat.SetColor("_EmissionColor", emit);
            }
            if (_glowLight != null) _glowLight.intensity = active ? 1.5f + Mathf.Sin(Time.time * 4f) * 0.5f : 0f;

            if (digging)
            {
                digProgress += Time.deltaTime / 3f;
                if (digProgress >= 1f)
                {
                    digging = false;
                    GameState.TunnelDug = true;
                    SoundFx.Instance.LevelUp();
                    LoadShark();
                }
                return;
            }

            if (!playerInside || !active) return;
            var kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame)
            {
                digging = true;
                SoundFx.Instance.Swoosh();
            }
        }

        void LoadShark()
        {
            // switch scene — assumes SharkFusion is in Build Settings as a fallback path; try by name
            SceneManager.LoadScene("SharkFusion");
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<ProceduralSpoonBuilder>() != null) playerInside = true;
        }
        void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<ProceduralSpoonBuilder>() != null) playerInside = false;
        }

        void OnGUI()
        {
            EnsureStyles();
            bool active = GameState.CellCrewAllTalked && !GameState.TunnelDug;
            if (digging)
            {
                float w = 600f, h = 40f;
                float x = (Screen.width - w) * 0.5f;
                float y = Screen.height * 0.5f - 60f;
                GUI.color = new Color(0f, 0f, 0f, 0.7f);
                GUI.DrawTexture(new Rect(x - 10, y - 10, w + 20, h + 80), Texture2D.whiteTexture);
                GUI.color = new Color(0.9f, 0.5f, 0.15f);
                GUI.DrawTexture(new Rect(x, y, w * digProgress, h), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(x, y + h + 8f, w, 30f), "...DIGGING THE LOOSE STONE...", promptStyle);
            }
            else if (playerInside && active)
            {
                float w = 700f;
                var s = new GUIStyle(promptStyle); s.normal.textColor = new Color(1f, 0.85f, 0.3f);
                GUI.Label(new Rect((Screen.width - w) * 0.5f, Screen.height * 0.55f, w, 40f),
                    "✨ PRESS E TO DIG THROUGH THE LOOSE STONE ✨", s);
            }
            else if (playerInside && !active)
            {
                float w = 700f;
                var s = new GUIStyle(promptStyle); s.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
                GUI.Label(new Rect((Screen.width - w) * 0.5f, Screen.height * 0.55f, w, 40f),
                    "...nothing happens. Talk to all 5 Cell Crew first.", s);
            }
        }

        void EnsureStyles()
        {
            if (promptStyle == null)
                promptStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
            if (progressStyle == null)
                progressStyle = new GUIStyle(GUI.skin.label) { fontSize = 18 };
        }
    }
}
