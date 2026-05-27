using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Spoonacci
{
    // Multi-stage dig. Requires GameState.CellCrewAllTalked AND PickaxeState.Found to activate.
    // Each E press advances digProgress by 0.34 (3 stages). Audio + visual feedback per stage.
    // On reaching 1.0 → sets GameState.TunnelDug + JustEscaped → loads SampleScene.
    public class LooseStone : MonoBehaviour
    {
        Material mat;
        Light glowLight;
        bool playerInside;
        float digProgress;
        bool digging;
        int stages = 3;
        int stagesDone;
        float currentStageEndsAt;
        GUIStyle promptStyle;

        void Awake()
        {
            var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.transform.SetParent(transform, false);
            stone.transform.localPosition = Vector3.zero;
            stone.transform.localScale = new Vector3(1.2f, 0.7f, 1.2f);
            var sh = Shader.Find("Universal Render Pipeline/Lit");
            mat = new Material(sh) { color = new Color(0.3f, 0.3f, 0.32f) };
            mat.SetFloat("_Metallic", 0.0f);
            mat.SetFloat("_Smoothness", 0.25f);
            mat.EnableKeyword("_EMISSION");
            stone.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(stone.GetComponent<Collider>());

            var trig = gameObject.AddComponent<SphereCollider>();
            trig.isTrigger = true;
            trig.radius = 1.8f;

            var glow = new GameObject("StoneGlow");
            glow.transform.SetParent(transform, false);
            glow.transform.localPosition = Vector3.up * 0.7f;
            glowLight = glow.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.color = new Color(1f, 0.5f, 0.2f);
            glowLight.range = 5f;
            glowLight.intensity = 0f;
        }

        bool Active() => GameState.CellCrewAllTalked && PickaxeState.Found && !GameState.TunnelDug;

        void Update()
        {
            bool active = Active();
            if (mat != null)
            {
                Color emit = active
                    ? new Color(0.95f, 0.4f, 0.12f) * (0.5f + Mathf.Sin(Time.time * 4f) * 0.5f)
                    : Color.black;
                mat.SetColor("_EmissionColor", emit);
            }
            if (glowLight != null) glowLight.intensity = active ? 2f + Mathf.Sin(Time.time * 4f) * 0.7f : 0f;

            if (digging)
            {
                if (Time.time > currentStageEndsAt)
                {
                    stagesDone++;
                    SoundFx.Instance.Bonk();
                    if (stagesDone >= stages)
                    {
                        digging = false;
                        Finish();
                    }
                    else
                    {
                        // wait for next E press
                        digging = false;
                    }
                }
                return;
            }

            if (!playerInside || !active) return;
            var kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame) StartStage();
        }

        void StartStage()
        {
            digging = true;
            currentStageEndsAt = Time.time + 1.2f;
            SoundFx.Instance.Swoosh();
        }

        void Finish()
        {
            GameState.TunnelDug = true;
            GameState.JustEscaped = true;
            GameState.PoliceMode = true; // arrive on island in police uniform
            SaveSystem.Instance.Save("Tunnel dug!");
            SoundFx.Instance.LevelUp();
            SceneManager.LoadScene("SampleScene");
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
            float w = 700f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.55f;

            if (digging || stagesDone > 0)
            {
                // progress bar
                float total = (stagesDone + (digging ? Mathf.Clamp01(1f - (currentStageEndsAt - Time.time) / 1.2f) : 0f)) / stages;
                float barW = 600f, barH = 36f;
                float bx = (Screen.width - barW) * 0.5f;
                float by = Screen.height * 0.45f;
                GUI.color = new Color(0f, 0f, 0f, 0.7f);
                GUI.DrawTexture(new Rect(bx - 10, by - 10, barW + 20, barH + 30), Texture2D.whiteTexture);
                GUI.color = new Color(0.95f, 0.5f, 0.15f);
                GUI.DrawTexture(new Rect(bx, by, barW * total, barH), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(bx, by + barH + 4f, barW, 24f), "DIG STAGE " + (stagesDone + (digging ? 1 : 0)) + "/" + stages, promptStyle);
            }

            if (playerInside && Active() && !digging)
            {
                var s = new GUIStyle(promptStyle); s.normal.textColor = new Color(1f, 0.85f, 0.3f);
                var sh = new GUIStyle(s); sh.normal.textColor = Color.black;
                string txt = stagesDone == 0 ? "⛏ PRESS E TO START DIGGING" : "⛏ PRESS E AGAIN (" + stagesDone + "/" + stages + ")";
                GUI.Label(new Rect(x + 2, y + 2, w, 40f), txt, sh);
                GUI.Label(new Rect(x, y, w, 40f), txt, s);
            }
            else if (playerInside && !Active())
            {
                var s = new GUIStyle(promptStyle); s.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
                string txt;
                if (!GameState.CellCrewAllTalked) txt = "...the stone won't budge. (Talk to all 5 Cell Crew first.)";
                else if (!PickaxeState.Found)     txt = "...you need a pickaxe. (Check the hay pile.)";
                else                               txt = "...something is missing.";
                GUI.Label(new Rect((Screen.width - w) * 0.5f, y, w, 40f), txt, s);
            }
        }

        void EnsureStyles()
        {
            if (promptStyle == null)
                promptStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
        }
    }
}
