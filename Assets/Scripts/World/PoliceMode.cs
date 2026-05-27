using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // P key: toggles Police Mode. Spoonacci gets a blue/black skin + flashing badge above head.
    // All "DrugDealer"-flagged civilians become auto-violators (red-arrowed permanently while mode on).
    public class PoliceMode : MonoBehaviour
    {
        ProceduralSpoonBuilder builder;
        Color savedColor;
        float savedMetallic, savedSmoothness;
        GameObject badge;
        Light flashRed, flashBlue;
        float flashTimer;

        void Awake() { builder = GetComponent<ProceduralSpoonBuilder>(); }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.pKey.wasPressedThisFrame) Toggle();

            if (GameState.PoliceMode)
            {
                // flag any drug dealer civilians as violators
                foreach (var c in Object.FindObjectsByType<Civilian>(FindObjectsSortMode.None))
                {
                    if (c == null) continue;
                    var d = c.GetComponent<DrugDealer>();
                    if (d != null && !c.IsViolator && !c.HasSpoonMark)
                        c.SetViolator(true, "DEALING SHADY SUBSTANCES");
                }

                // flash the badge lights
                if (flashRed != null && flashBlue != null)
                {
                    flashTimer += Time.deltaTime;
                    bool red = (Mathf.Sin(flashTimer * 14f) > 0f);
                    flashRed.intensity = red ? 3f : 0f;
                    flashBlue.intensity = red ? 0f : 3f;
                }
            }
        }

        void Toggle()
        {
            GameState.PoliceMode = !GameState.PoliceMode;
            SoundFx.Instance.Chime();

            if (GameState.PoliceMode) EnterPolice();
            else ExitPolice();
        }

        void EnterPolice()
        {
            if (builder == null) return;
            savedColor = builder.skinColor;
            savedMetallic = builder.metallic;
            savedSmoothness = builder.smoothness;
            builder.ApplySkin(new Color(0.05f, 0.15f, 0.4f), 0.8f, 0.5f); // dark blue uniform

            // badge on head
            if (badge == null)
            {
                badge = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                badge.transform.SetParent(transform, false);
                badge.transform.localPosition = new Vector3(0f, 1.5f, 0f);
                badge.transform.localScale = new Vector3(0.45f, 0.1f, 0.45f);
                Destroy(badge.GetComponent<Collider>());
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(1f, 0.85f, 0.1f) };
                mat.SetFloat("_Metallic", 1f);
                mat.SetFloat("_Smoothness", 0.9f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.4f, 0.3f, 0f));
                badge.GetComponent<Renderer>().sharedMaterial = mat;

                // siren lights
                var r = new GameObject("FlashRed");
                r.transform.SetParent(badge.transform, false);
                r.transform.localPosition = new Vector3(-0.3f, 0.5f, 0f);
                flashRed = r.AddComponent<Light>();
                flashRed.type = LightType.Point; flashRed.color = Color.red; flashRed.range = 6f;

                var b = new GameObject("FlashBlue");
                b.transform.SetParent(badge.transform, false);
                b.transform.localPosition = new Vector3(0.3f, 0.5f, 0f);
                flashBlue = b.AddComponent<Light>();
                flashBlue.type = LightType.Point; flashBlue.color = Color.blue; flashBlue.range = 6f;
            }
            else badge.SetActive(true);
        }

        void ExitPolice()
        {
            if (builder != null) builder.ApplySkin(savedColor, savedMetallic, savedSmoothness);
            if (badge != null) badge.SetActive(false);
        }

        void OnGUI()
        {
            // mode indicator top-center
            var s = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = GameState.PoliceMode ? new Color(0.3f, 0.7f, 1f) : new Color(0.8f, 0.8f, 0.8f, 0.55f) } };
            string txt = GameState.PoliceMode ? "🚨 POLICE MODE — drug dealers auto-flagged" : "Press [P] to enter Police Mode";
            float w = 540f;
            GUI.Label(new Rect((Screen.width - w) * 0.5f, 6f, w, 28f), txt, s);
        }
    }

    // Marker component on civilians who are "drug dealers" — Police Mode auto-violates them.
    public class DrugDealer : MonoBehaviour { }
}
