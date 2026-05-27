using UnityEngine;

namespace Spoonacci
{
    // Builds Sir Spoonacci as a UPRIGHT character — handle vertical (body), bowl on top (head).
    // Mesh = primitives, but stacked properly so he reads as a little dude.
    // All visuals live under a child 'Visual' transform so SpoonAnimator can rock/squash it
    // without fighting the Rigidbody's transform.
    public class ProceduralSpoonBuilder : MonoBehaviour
    {
        public Color skinColor = new Color(0.85f, 0.85f, 0.9f);
        public float metallic = 0.95f;
        public float smoothness = 0.85f;

        [Header("Proportions (set before Awake to override)")]
        public float bodyHeight = 0.9f;
        public float bodyRadius = 0.08f;
        public Vector3 bowlSize = new Vector3(0.42f, 0.16f, 0.55f); // x,y,z
        public bool addFace = true;

        Renderer[] metalParts;
        Material runtimeMat;
        public Transform Visual { get; private set; }

        void Awake()
        {
            BuildBody();
            ApplySkin(skinColor, metallic, smoothness);
        }

        void BuildBody()
        {
            // physics root
            var rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
            rb.mass = 0.25f;
            rb.linearDamping = 4f;
            rb.angularDamping = 8f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var col = GetComponent<CapsuleCollider>() ?? gameObject.AddComponent<CapsuleCollider>();
            col.direction = 1; // Y-axis (vertical)
            col.height = bodyHeight + 0.25f;
            col.radius = Mathf.Max(bodyRadius * 1.3f, 0.16f);
            col.center = new Vector3(0f, (bodyHeight + 0.25f) * 0.5f - 0.05f, 0f);

            // visual root — animator wiggles THIS
            var visualGo = new GameObject("Visual");
            visualGo.transform.SetParent(transform, false);
            Visual = visualGo.transform;

            // HANDLE = body, vertical cylinder rising from ground
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Body (Handle)";
            handle.transform.SetParent(Visual, false);
            handle.transform.localPosition = new Vector3(0f, bodyHeight * 0.5f, 0f);
            handle.transform.localScale = new Vector3(bodyRadius * 2f, bodyHeight * 0.5f, bodyRadius * 2f);
            Destroy(handle.GetComponent<Collider>());

            // HANDLE TIP — small sphere at top of handle for smooth transition to bowl
            var neck = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            neck.name = "Neck";
            neck.transform.SetParent(Visual, false);
            neck.transform.localPosition = new Vector3(0f, bodyHeight, 0f);
            neck.transform.localScale = new Vector3(bodyRadius * 2.4f, bodyRadius * 2.4f, bodyRadius * 2.4f);
            Destroy(neck.GetComponent<Collider>());

            // BOWL = head, tilted slightly forward so it reads as a face
            var bowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bowl.name = "Head (Bowl)";
            bowl.transform.SetParent(Visual, false);
            bowl.transform.localPosition = new Vector3(0f, bodyHeight + bowlSize.y * 0.55f, 0.02f);
            bowl.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            bowl.transform.localScale = bowlSize;
            Destroy(bowl.GetComponent<Collider>());

            // BOWL RIM = thin torus-ish ring (use a flattened sphere for now)
            var rim = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rim.name = "Bowl Rim";
            rim.transform.SetParent(Visual, false);
            rim.transform.localPosition = bowl.transform.localPosition + new Vector3(0f, 0.005f, 0f);
            rim.transform.localRotation = bowl.transform.localRotation;
            rim.transform.localScale = bowlSize * 1.05f;
            Destroy(rim.GetComponent<Collider>());

            // FACE — two tiny eyes (matte black) on the front of the bowl
            if (addFace)
            {
                Vector3 eyeBasePos = bowl.transform.localPosition + Quaternion.Euler(15f, 0f, 0f) * new Vector3(0f, 0.025f, bowlSize.z * 0.42f);
                var eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eyeL.name = "Eye L";
                eyeL.transform.SetParent(Visual, false);
                eyeL.transform.localPosition = eyeBasePos + new Vector3(-bowlSize.x * 0.25f, 0f, 0f);
                eyeL.transform.localScale = Vector3.one * 0.06f;
                Destroy(eyeL.GetComponent<Collider>());
                eyeL.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.02f, 0.02f, 0.02f), 0f, 0.1f);

                var eyeR = Instantiate(eyeL, Visual);
                eyeR.name = "Eye R";
                eyeR.transform.localPosition = eyeBasePos + new Vector3(bowlSize.x * 0.25f, 0f, 0f);
                eyeR.transform.localScale = Vector3.one * 0.06f;
            }

            metalParts = new Renderer[] { handle.GetComponent<Renderer>(), neck.GetComponent<Renderer>(), bowl.GetComponent<Renderer>(), rim.GetComponent<Renderer>() };
        }

        public void ApplySkin(Color color, float met = 0.95f, float smooth = 0.85f)
        {
            skinColor = color;
            metallic = met;
            smoothness = smooth;
            if (runtimeMat == null) runtimeMat = MakeMat(color, met, smooth);
            else
            {
                runtimeMat.color = color;
                runtimeMat.SetFloat("_Metallic", met);
                runtimeMat.SetFloat("_Smoothness", smooth);
            }
            if (metalParts != null) foreach (var r in metalParts) if (r != null) r.sharedMaterial = runtimeMat;
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
