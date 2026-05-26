using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Stationary spoon NPC. Builds itself from primitives like the player, with custom proportions/color.
    // Step inside trigger, press E, dialogue lines play in sequence.
    public class CellCrewNPC : MonoBehaviour
    {
        public string crewName = "Spoon";
        public string[] lines = new[] { "..." };
        public Color color = Color.white;
        public Vector3 bowlScale = new Vector3(0.45f, 0.18f, 0.6f);
        public Vector3 handleScale = new Vector3(0.12f, 0.6f, 0.12f);
        public float metallic = 0.95f;
        public float smoothness = 0.85f;

        int idx;
        bool playerInside;

        void Awake()
        {
            BuildBody();
            var trigger = gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.8f;
        }

        void BuildBody()
        {
            var mat = MakeMat(color, metallic, smoothness);

            var bowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bowl.name = "Bowl";
            bowl.transform.SetParent(transform, false);
            bowl.transform.localPosition = new Vector3(0f, 0.1f, 0.35f);
            bowl.transform.localScale = bowlScale;
            bowl.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(bowl.GetComponent<Collider>());

            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Handle";
            handle.transform.SetParent(transform, false);
            handle.transform.localPosition = new Vector3(0f, 0.1f, -0.25f);
            handle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            handle.transform.localScale = handleScale;
            handle.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(handle.GetComponent<Collider>());

            var floorBlocker = gameObject.AddComponent<BoxCollider>();
            floorBlocker.center = new Vector3(0f, 0.1f, 0.05f);
            floorBlocker.size = new Vector3(0.4f, 0.3f, 1.1f);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<ProceduralSpoonBuilder>() != null) playerInside = true;
        }
        void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<ProceduralSpoonBuilder>() != null)
            {
                playerInside = false;
                DialogueBox.Instance.Hide();
                idx = 0;
            }
        }

        void Update()
        {
            if (!playerInside) return;
            var kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame)
            {
                if (lines == null || lines.Length == 0) return;
                DialogueBox.Instance.Say(crewName, lines[idx]);
                idx = (idx + 1) % lines.Length;
            }
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
