using UnityEngine;

namespace Spoonacci
{
    // Minimal stationary spoon NPC (no dialogue, just visual presence).
    // Used by Salon Cucchiaio for its attendant. Larger / more decorative variant of the player.
    public class NpcSpoon : MonoBehaviour
    {
        public string crewName = "Spoon";
        public Color color = Color.white;
        public float metallic = 0.95f;
        public float smoothness = 0.85f;

        void Awake()
        {
            var mat = MakeMat(color, metallic, smoothness);

            // body (handle vertical)
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.transform.SetParent(transform, false);
            handle.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            handle.transform.localScale = new Vector3(0.18f, 0.5f, 0.18f);
            Destroy(handle.GetComponent<Collider>());
            handle.GetComponent<Renderer>().sharedMaterial = mat;

            // head (bowl)
            var bowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bowl.transform.SetParent(transform, false);
            bowl.transform.localPosition = new Vector3(0f, 1.0f, 0.04f);
            bowl.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            bowl.transform.localScale = new Vector3(0.55f, 0.22f, 0.7f);
            Destroy(bowl.GetComponent<Collider>());
            bowl.GetComponent<Renderer>().sharedMaterial = mat;

            // eyes
            for (int side = -1; side <= 1; side += 2)
            {
                var e = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                e.transform.SetParent(bowl.transform, false);
                e.transform.localPosition = new Vector3(0.18f * side, 0.1f, 0.4f);
                e.transform.localScale = Vector3.one * 0.13f;
                Destroy(e.GetComponent<Collider>());
                e.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.02f, 0.02f, 0.02f), 0f, 0.1f);
            }

            // tiny bow on top (cute)
            var bow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bow.transform.SetParent(transform, false);
            bow.transform.localPosition = new Vector3(0f, 1.25f, -0.05f);
            bow.transform.localScale = new Vector3(0.25f, 0.08f, 0.12f);
            Destroy(bow.GetComponent<Collider>());
            bow.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.3f, 0.6f), 0.2f, 0.7f);

            // floor anchor
            var col = gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.5f, 0f);
            col.size = new Vector3(0.4f, 1.2f, 0.4f);
        }

        static Material MakeMat(Color c, float metallic, float smoothness)
        {
            var sh = Shader.Find("Universal Render Pipeline/Lit");
            var m = new Material(sh) { color = c };
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Smoothness", smoothness);
            return m;
        }
    }
}
