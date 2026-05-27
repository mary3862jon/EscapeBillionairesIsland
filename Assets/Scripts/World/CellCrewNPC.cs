using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Stationary UPRIGHT spoon NPC. Handle = body, bowl = head, eyes, optional accessory (monocle/bow).
    // Each Cell Crew member has a personality scale tweak and an accessory.
    public class CellCrewNPC : MonoBehaviour
    {
        public string crewName = "Spoon";
        public string[] lines = new[] { "..." };
        public Color color = Color.white;
        public float metallic = 0.95f;
        public float smoothness = 0.85f;
        public float bodyHeight = 0.9f;
        public float bodyRadius = 0.08f;
        public Vector3 bowlSize = new Vector3(0.42f, 0.16f, 0.55f);
        public enum Accessory { None, Monocle, Bow, Hardhat, Crown, Toothpick }
        public Accessory accessory = Accessory.None;

        int idx;
        bool playerInside;
        Transform headTransform;
        Vector3 baseLocalPosBody;
        Transform visualRoot;

        void Awake()
        {
            BuildUpright();
            var trigger = gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 2.2f;
        }

        void BuildUpright()
        {
            var mat = MakeMat(color, metallic, smoothness);

            var vis = new GameObject("Visual");
            vis.transform.SetParent(transform, false);
            visualRoot = vis.transform;

            // body (handle) cylinder vertical
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Body";
            handle.transform.SetParent(vis.transform, false);
            handle.transform.localPosition = new Vector3(0f, bodyHeight * 0.5f, 0f);
            handle.transform.localScale = new Vector3(bodyRadius * 2f, bodyHeight * 0.5f, bodyRadius * 2f);
            Destroy(handle.GetComponent<Collider>());
            handle.GetComponent<Renderer>().sharedMaterial = mat;

            // neck
            var neck = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            neck.transform.SetParent(vis.transform, false);
            neck.transform.localPosition = new Vector3(0f, bodyHeight, 0f);
            neck.transform.localScale = new Vector3(bodyRadius * 2.3f, bodyRadius * 2.3f, bodyRadius * 2.3f);
            Destroy(neck.GetComponent<Collider>());
            neck.GetComponent<Renderer>().sharedMaterial = mat;

            // head (bowl) tilted forward
            var bowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bowl.name = "Head";
            bowl.transform.SetParent(vis.transform, false);
            bowl.transform.localPosition = new Vector3(0f, bodyHeight + bowlSize.y * 0.55f, 0.02f);
            bowl.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            bowl.transform.localScale = bowlSize;
            Destroy(bowl.GetComponent<Collider>());
            bowl.GetComponent<Renderer>().sharedMaterial = mat;
            headTransform = bowl.transform;

            // eyes
            for (int side = -1; side <= 1; side += 2)
            {
                var e = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                e.name = "Eye";
                e.transform.SetParent(bowl.transform, false);
                e.transform.localPosition = new Vector3(side * 0.25f, 0.1f, 0.4f);
                e.transform.localScale = new Vector3(0.18f, 0.2f, 0.18f);
                Destroy(e.GetComponent<Collider>());
                e.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.02f, 0.02f, 0.02f), 0f, 0.1f);
            }

            // mouth (small black sliver)
            var mouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mouth.transform.SetParent(bowl.transform, false);
            mouth.transform.localPosition = new Vector3(0f, -0.15f, 0.42f);
            mouth.transform.localScale = new Vector3(0.3f, 0.04f, 0.06f);
            Destroy(mouth.GetComponent<Collider>());
            mouth.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.05f, 0.05f, 0.05f), 0f, 0.1f);

            BuildAccessory(bowl.transform);

            // physics — kinematic, just blocks the player
            var col = gameObject.AddComponent<CapsuleCollider>();
            col.direction = 1;
            col.height = bodyHeight + bowlSize.y;
            col.radius = Mathf.Max(bodyRadius * 1.3f, 0.18f);
            col.center = new Vector3(0f, (bodyHeight + bowlSize.y) * 0.5f, 0f);
        }

        void BuildAccessory(Transform parent)
        {
            switch (accessory)
            {
                case Accessory.Monocle:
                    var ring = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    ring.transform.SetParent(parent, false);
                    ring.transform.localPosition = new Vector3(0.25f, 0.1f, 0.45f);
                    ring.transform.localScale = new Vector3(0.22f, 0.22f, 0.05f);
                    Destroy(ring.GetComponent<Collider>());
                    ring.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.85f, 0.7f, 0.2f), 1f, 0.9f);
                    break;
                case Accessory.Bow:
                    var bow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bow.transform.SetParent(parent, false);
                    bow.transform.localPosition = new Vector3(0f, 0.35f, -0.1f);
                    bow.transform.localScale = new Vector3(0.4f, 0.12f, 0.18f);
                    Destroy(bow.GetComponent<Collider>());
                    bow.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.2f, 0.5f), 0.2f, 0.7f);
                    break;
                case Accessory.Hardhat:
                    var hat = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    hat.transform.SetParent(parent, false);
                    hat.transform.localPosition = new Vector3(0f, 0.3f, -0.05f);
                    hat.transform.localScale = new Vector3(0.7f, 0.35f, 0.7f);
                    Destroy(hat.GetComponent<Collider>());
                    hat.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(1f, 0.75f, 0.1f), 0.2f, 0.6f);
                    break;
                case Accessory.Crown:
                    for (int i = 0; i < 5; i++)
                    {
                        var spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        spike.transform.SetParent(parent, false);
                        spike.transform.localPosition = new Vector3((i - 2) * 0.12f, 0.4f, -0.05f);
                        spike.transform.localScale = new Vector3(0.08f, 0.2f, 0.08f);
                        Destroy(spike.GetComponent<Collider>());
                        spike.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(1f, 0.85f, 0.2f), 1f, 0.9f);
                    }
                    break;
                case Accessory.Toothpick:
                    var pick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    pick.transform.SetParent(parent, false);
                    pick.transform.localPosition = new Vector3(0.2f, -0.13f, 0.4f);
                    pick.transform.localRotation = Quaternion.Euler(0f, 0f, 60f);
                    pick.transform.localScale = new Vector3(0.02f, 0.15f, 0.02f);
                    Destroy(pick.GetComponent<Collider>());
                    pick.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.6f, 0.45f, 0.25f), 0f, 0.4f);
                    break;
            }
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
            // little idle sway
            if (visualRoot != null)
            {
                visualRoot.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * 0.8f + crewName.GetHashCode() * 0.001f) * 4f, 0f);
            }
            if (!playerInside) return;
            var kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame)
            {
                if (lines == null || lines.Length == 0) return;
                DialogueBox.Instance.Say(crewName, lines[idx]);
                GameState.OnNpcTalkedTo(crewName);
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
