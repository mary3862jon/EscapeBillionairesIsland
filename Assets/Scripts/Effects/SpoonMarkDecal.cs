using UnityEngine;

namespace Spoonacci
{
    // Persistent forehead bruise: small flattened red spoon-bowl-shape on the victim's head.
    // Pulses slowly so it's visible.
    public class SpoonMarkDecal : MonoBehaviour
    {
        Material mat;

        public void Attach(Transform head)
        {
            transform.SetParent(head, false);
            transform.localPosition = new Vector3(0f, 0.04f, 0.42f); // front-of-forehead
            transform.localScale = new Vector3(0.22f, 0.04f, 0.18f);

            var bowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bowl.transform.SetParent(transform, false);
            bowl.transform.localScale = Vector3.one;
            Destroy(bowl.GetComponent<Collider>());

            var sh = ShaderCache.Lit;
            mat = new Material(sh) { color = new Color(0.85f, 0.1f, 0.1f) };
            mat.SetFloat("_Metallic", 0.05f);
            mat.SetFloat("_Smoothness", 0.7f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.6f, 0.0f, 0.0f));
            bowl.GetComponent<Renderer>().sharedMaterial = mat;
        }

        void Update()
        {
            if (mat == null) return;
            float p = 0.4f + Mathf.Sin(Time.time * 2.4f) * 0.4f;
            mat.SetColor("_EmissionColor", new Color(0.55f * p, 0f, 0f));
        }
    }
}
