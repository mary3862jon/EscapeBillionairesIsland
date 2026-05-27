using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // P key toggles. Spoonacci → blue uniform + gold badge + red/blue flashing siren lights + LOOPED SIREN AUDIO.
    // While on: all DrugDealer civilians become auto-violators.
    public class PoliceMode : MonoBehaviour
    {
        ProceduralSpoonBuilder builder;
        Color savedColor;
        float savedMetallic, savedSmoothness;
        bool savedKnown;

        GameObject badge;
        Light flashRed, flashBlue;
        float flashTimer;
        bool prevMode;

        void Awake()
        {
            builder = GetComponent<ProceduralSpoonBuilder>();
        }

        void Start()
        {
            // honor JustEscaped: arrive in police uniform
            if (GameState.PoliceMode) EnterPolice();
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.pKey.wasPressedThisFrame) Toggle();

            if (GameState.PoliceMode != prevMode)
            {
                prevMode = GameState.PoliceMode;
                if (GameState.PoliceMode) EnterPolice(); else ExitPolice();
            }

            if (GameState.PoliceMode)
            {
                // flag any drug dealer civilians
                foreach (var c in Object.FindObjectsByType<Civilian>(FindObjectsSortMode.None))
                {
                    if (c == null) continue;
                    var d = c.GetComponent<DrugDealer>();
                    if (d != null && !c.IsViolator && !c.HasSpoonMark)
                        c.SetViolator(true, "DEALING SHADY SUBSTANCES");
                }
                // siren lights pulse
                if (flashRed != null && flashBlue != null)
                {
                    flashTimer += Time.deltaTime;
                    bool red = Mathf.Sin(flashTimer * 14f) > 0f;
                    flashRed.intensity = red ? 3f : 0f;
                    flashBlue.intensity = red ? 0f : 3f;
                }
            }
        }

        void Toggle()
        {
            GameState.PoliceMode = !GameState.PoliceMode;
            SoundFx.Instance.Chime();
            // EnterPolice/ExitPolice triggered by prevMode change in Update; do directly here too for instant feel
            if (GameState.PoliceMode) EnterPolice(); else ExitPolice();
        }

        void EnterPolice()
        {
            if (builder == null) return;
            if (!savedKnown)
            {
                savedColor = builder.skinColor;
                savedMetallic = builder.metallic;
                savedSmoothness = builder.smoothness;
                savedKnown = true;
            }
            builder.ApplySkin(new Color(0.05f, 0.15f, 0.4f), 0.8f, 0.5f);

            if (badge == null) BuildBadge();
            else badge.SetActive(true);

            SoundFx.Instance.StartSiren();
        }

        void BuildBadge()
        {
            badge = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            badge.name = "Badge";
            badge.transform.SetParent(transform, false);
            badge.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            badge.transform.localScale = new Vector3(0.5f, 0.12f, 0.5f);
            Destroy(badge.GetComponent<Collider>());
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(1f, 0.85f, 0.1f) };
            mat.SetFloat("_Metallic", 1f); mat.SetFloat("_Smoothness", 0.9f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.5f, 0.4f, 0f));
            badge.GetComponent<Renderer>().sharedMaterial = mat;

            var r = new GameObject("FlashRed");
            r.transform.SetParent(badge.transform, false);
            r.transform.localPosition = new Vector3(-0.5f, 0.5f, 0f);
            flashRed = r.AddComponent<Light>();
            flashRed.type = LightType.Point; flashRed.color = Color.red; flashRed.range = 8f;

            var b = new GameObject("FlashBlue");
            b.transform.SetParent(badge.transform, false);
            b.transform.localPosition = new Vector3(0.5f, 0.5f, 0f);
            flashBlue = b.AddComponent<Light>();
            flashBlue.type = LightType.Point; flashBlue.color = Color.blue; flashBlue.range = 8f;
        }

        void ExitPolice()
        {
            if (savedKnown && builder != null) builder.ApplySkin(savedColor, savedMetallic, savedSmoothness);
            if (badge != null) badge.SetActive(false);
            SoundFx.Instance.StopSiren();
        }

        public void ForceExit()
        {
            GameState.PoliceMode = false;
            ExitPolice();
        }

        void OnGUI()
        {
            var s = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = GameState.PoliceMode ? new Color(0.4f, 0.85f, 1f) : new Color(0.8f, 0.8f, 0.8f, 0.6f) } };
            string txt = GameState.PoliceMode ? "🚨 POLICE MODE — siren active" : "Press [P] for Police Mode";
            float w = 520f;
            GUI.Label(new Rect((Screen.width - w) * 0.5f, 8f, w, 28f), txt, s);
        }

        void OnDestroy()
        {
            SoundFx.Instance.StopSiren();
        }
    }

    public class DrugDealer : MonoBehaviour { }
}
