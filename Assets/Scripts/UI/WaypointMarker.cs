using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spoonacci
{
    // Floating glowing arrow above the next objective.
    // The bootstrapper assigns Target — the marker hovers above it pulsing.
    public class WaypointMarker : MonoBehaviour
    {
        public Transform target;
        public Color color = new Color(0.3f, 1f, 0.6f);

        GameObject arrow;
        Light glow;
        Material mat;

        void Awake()
        {
            BuildArrow();
        }

        void BuildArrow()
        {
            // downward-pointing cone made from a cone-shaped sphere + cube tail
            arrow = new GameObject("ArrowVisual");
            arrow.transform.SetParent(transform, false);

            mat = new Material(ShaderCache.Lit) { color = color };
            mat.SetFloat("_Metallic", 0.2f);
            mat.SetFloat("_Smoothness", 0.7f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 2f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(arrow.transform, false);
            body.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            body.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            body.transform.localRotation = Quaternion.Euler(45f, 0f, 45f);
            Destroy(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().sharedMaterial = mat;

            var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tip.transform.SetParent(arrow.transform, false);
            tip.transform.localScale = new Vector3(0.4f, 0.45f, 0.4f);
            tip.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            Destroy(tip.GetComponent<Collider>());
            tip.GetComponent<Renderer>().sharedMaterial = mat;

            // point light for emphasis
            var glowGo = new GameObject("Glow");
            glowGo.transform.SetParent(arrow.transform, false);
            glow = glowGo.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = color;
            glow.range = 8f;
            glow.intensity = 4f;
        }

        void Update()
        {
            if (target == null) { gameObject.SetActive(false); return; }
            gameObject.SetActive(true);

            // hover above target + bob + spin
            Vector3 hover = target.position + Vector3.up * (2.6f + Mathf.Sin(Time.time * 2f) * 0.35f);
            transform.position = hover;
            arrow.transform.Rotate(Vector3.up, 80f * Time.deltaTime);

            // pulse emission
            if (mat != null)
            {
                float p = 1.5f + Mathf.Sin(Time.time * 4f) * 1.5f;
                mat.SetColor("_EmissionColor", color * p);
            }
        }
    }
}
