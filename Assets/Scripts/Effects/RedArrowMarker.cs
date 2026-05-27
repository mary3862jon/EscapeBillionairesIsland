using UnityEngine;

namespace Spoonacci
{
    // Floating red arrow above a violator's head. Football-style call-out.
    // Spins, bobs, with a billboard text label underneath.
    public class RedArrowMarker : MonoBehaviour
    {
        Transform target;
        string label = "";
        GUIStyle style;
        Camera cam;
        Renderer arrowRend;
        Material mat;

        public void Attach(Transform t, string lab)
        {
            target = t;
            label = lab;
            cam = Camera.main;
            BuildArrow();
        }

        public void SetLabel(string lab) { label = lab; }

        void BuildArrow()
        {
            // big red downward-pointing cone
            var arrow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arrow.name = "ArrowCone";
            arrow.transform.SetParent(transform, false);
            arrow.transform.localScale = new Vector3(0.5f, 0.4f, 0.5f); // cylinder = cone-ish
            // squash top to make a cone-like shape via two stacked primitives
            Destroy(arrow.GetComponent<Collider>());

            var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tip.name = "Tip";
            tip.transform.SetParent(transform, false);
            tip.transform.localPosition = new Vector3(0f, -0.4f, 0f);
            tip.transform.localScale = new Vector3(0.55f, 0.5f, 0.55f);
            Destroy(tip.GetComponent<Collider>());

            var sh = Shader.Find("Universal Render Pipeline/Lit");
            mat = new Material(sh) { color = new Color(1f, 0.1f, 0.1f) };
            mat.SetFloat("_Metallic", 0.1f);
            mat.SetFloat("_Smoothness", 0.6f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1.4f, 0.1f, 0.1f));
            arrow.GetComponent<Renderer>().sharedMaterial = mat;
            tip.GetComponent<Renderer>().sharedMaterial = mat;
            arrowRend = arrow.GetComponent<Renderer>();
        }

        void LateUpdate()
        {
            if (target == null) { Destroy(gameObject); return; }
            // hover above target head
            transform.position = target.position + Vector3.up * 1.4f
                + Vector3.up * Mathf.Sin(Time.time * 4f) * 0.12f;
            transform.Rotate(Vector3.up, 180f * Time.deltaTime, Space.World);

            // pulse emission
            if (mat != null)
            {
                float p = 0.7f + Mathf.Sin(Time.time * 6f) * 0.7f;
                mat.SetColor("_EmissionColor", new Color(1.4f * p, 0.1f, 0.1f));
            }
        }

        void OnGUI()
        {
            if (target == null || cam == null) return;
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(1f, 0.4f, 0.4f) }
                };
            }
            Vector3 screen = cam.WorldToScreenPoint(transform.position + Vector3.up * 0.4f);
            if (screen.z < 0f) return;
            var r = new Rect(screen.x - 90f, Screen.height - screen.y - 12f, 180f, 22f);
            // shadow
            var shadow = new GUIStyle(style);
            shadow.normal.textColor = Color.black;
            GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), label, shadow);
            GUI.Label(r, label, style);
        }
    }
}
