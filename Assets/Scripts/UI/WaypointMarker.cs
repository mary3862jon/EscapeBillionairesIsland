using UnityEngine;

namespace Spoonacci
{
    // Subtle floating chevron above the next objective.
    // SMALL, no light, mild emission — should NOT dominate the view.
    public class WaypointMarker : MonoBehaviour
    {
        public Transform target;
        public Color color = new Color(0.3f, 1f, 0.6f);
        public float maxRange = 40f;
        public float hoverHeight = 3.2f;

        GameObject arrow;
        Material mat;

        void Awake() { /* visual disabled by user request — keeping component as a stub for future re-enable */ }

        void BuildArrow()
        {
            arrow = new GameObject("ArrowVisual");
            arrow.transform.SetParent(transform, false);

            mat = new Material(ShaderCache.Lit) { color = color };
            mat.SetFloat("_Metallic", 0.0f);
            mat.SetFloat("_Smoothness", 0.5f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.5f);

            // small downward arrow — tilted cube + cone tip
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(arrow.transform, false);
            body.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            body.transform.localPosition = new Vector3(0f, 0.18f, 0f);
            body.transform.localRotation = Quaternion.Euler(45f, 0f, 45f);
            Destroy(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().sharedMaterial = mat;
        }

        void Update() { /* no-op while visual is disabled */ }
    }
}
