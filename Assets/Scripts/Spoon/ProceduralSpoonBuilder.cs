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
        public bool addMonocle = true;

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

            // BOWL — elongated egg/teardrop shape (1.6x deeper than wide), tilted back so scoop is visible
            // Position offset forward so it reads as a spoon extending past the handle
            var bowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bowl.name = "Head (Bowl)";
            bowl.transform.SetParent(Visual, false);
            bowl.transform.localPosition = new Vector3(0f, bodyHeight + bowlSize.y * 0.55f, 0.18f);
            bowl.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
            bowl.transform.localScale = new Vector3(bowlSize.x, bowlSize.y, bowlSize.z * 1.6f);
            Destroy(bowl.GetComponent<Collider>());

            // BOWL RIM — thin flat ellipse around the rim (gold-ish darker = spoon edge)
            var rim = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rim.name = "Bowl Rim";
            rim.transform.SetParent(Visual, false);
            rim.transform.localPosition = bowl.transform.localPosition + new Vector3(0f, 0.01f, 0f);
            rim.transform.localRotation = bowl.transform.localRotation;
            rim.transform.localScale = new Vector3(bowlSize.x * 1.06f, bowlSize.y * 0.6f, bowlSize.z * 1.7f);
            Destroy(rim.GetComponent<Collider>());

            // SCOOP INTERIOR — darker inset sphere creates the concave "scoop" illusion
            var scoop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            scoop.name = "Scoop Interior";
            scoop.transform.SetParent(Visual, false);
            scoop.transform.localPosition = bowl.transform.localPosition + Quaternion.Euler(20f, 0f, 0f) * new Vector3(0f, 0.025f, -0.02f);
            scoop.transform.localRotation = bowl.transform.localRotation;
            scoop.transform.localScale = new Vector3(bowlSize.x * 0.85f, bowlSize.y * 1.2f, bowlSize.z * 1.4f);
            Destroy(scoop.GetComponent<Collider>());
            scoop.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.32f, 0.32f, 0.36f), 0.4f, 0.7f); // darker matte metal

            // TIP — small sphere at the very front of the bowl to round out the egg shape
            var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tip.name = "Bowl Tip";
            tip.transform.SetParent(Visual, false);
            tip.transform.localPosition = bowl.transform.localPosition + Quaternion.Euler(20f, 0f, 0f) * new Vector3(0f, 0f, bowlSize.z * 0.85f);
            tip.transform.localRotation = bowl.transform.localRotation;
            tip.transform.localScale = new Vector3(bowlSize.x * 0.6f, bowlSize.y * 0.9f, bowlSize.z * 0.6f);
            Destroy(tip.GetComponent<Collider>());

            // FACE — eyes, eyebrows, mouth, optional monocle
            if (addFace)
            {
                var blackMat = MakeMat(new Color(0.02f, 0.02f, 0.02f), 0f, 0.1f);
                var goldMat  = MakeMat(new Color(0.95f, 0.78f, 0.2f), 1f, 0.9f);

                // face is on the BACK of the bowl now (handle-facing side) so the scoop tip is forward
                Vector3 faceBase = bowl.transform.localPosition + Quaternion.Euler(20f, 0f, 0f) * new Vector3(0f, 0.02f, -bowlSize.z * 0.6f);

                // EYES — pupils face BACKWARD (toward camera since face is on back side of bowl)
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector3 ep = faceBase + new Vector3(side * bowlSize.x * 0.25f, 0.025f, 0f);

                    var sclera = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sclera.name = "Eye White";
                    sclera.transform.SetParent(Visual, false);
                    sclera.transform.localPosition = ep;
                    sclera.transform.localScale = Vector3.one * 0.085f;
                    Destroy(sclera.GetComponent<Collider>());
                    sclera.GetComponent<Renderer>().sharedMaterial = MakeMat(Color.white, 0f, 0.4f);

                    var pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    pupil.transform.SetParent(sclera.transform, false);
                    pupil.transform.localPosition = new Vector3(0f, 0f, -0.4f); // back-facing now
                    pupil.transform.localScale = Vector3.one * 0.55f;
                    Destroy(pupil.GetComponent<Collider>());
                    pupil.GetComponent<Renderer>().sharedMaterial = blackMat;
                }

                // EYEBROWS — angled cubes give him an attitude
                for (int side = -1; side <= 1; side += 2)
                {
                    var brow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    brow.name = "Brow";
                    brow.transform.SetParent(Visual, false);
                    brow.transform.localPosition = faceBase + new Vector3(side * bowlSize.x * 0.25f, 0.095f, 0f);
                    brow.transform.localScale = new Vector3(0.14f, 0.03f, 0.03f);
                    brow.transform.localRotation = Quaternion.Euler(0f, 0f, side * -18f); // angry-furrowed-inward
                    Destroy(brow.GetComponent<Collider>());
                    brow.GetComponent<Renderer>().sharedMaterial = blackMat;
                }

                // MOUTH — small dark slit
                var mouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mouth.name = "Mouth";
                mouth.transform.SetParent(Visual, false);
                mouth.transform.localPosition = faceBase + new Vector3(0f, -0.08f, 0f);
                mouth.transform.localScale = new Vector3(0.12f, 0.03f, 0.03f);
                Destroy(mouth.GetComponent<Collider>());
                mouth.GetComponent<Renderer>().sharedMaterial = blackMat;

                // MONOCLE — gold ring around right eye (Sir Spoonacci nobility flair)
                if (addMonocle)
                {
                    var ring = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    ring.name = "Monocle";
                    ring.transform.SetParent(Visual, false);
                    ring.transform.localPosition = faceBase + new Vector3(bowlSize.x * 0.25f, 0.025f, 0f);
                    ring.transform.localScale = new Vector3(0.14f, 0.14f, 0.03f);
                    Destroy(ring.GetComponent<Collider>());
                    ring.GetComponent<Renderer>().sharedMaterial = goldMat;

                    // monocle chain (a tiny gold cube hanging down)
                    var chain = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    chain.transform.SetParent(ring.transform, false);
                    chain.transform.localPosition = new Vector3(0f, -0.5f, 0f);
                    chain.transform.localScale = new Vector3(0.06f, 1.5f, 0.06f);
                    chain.transform.localRotation = Quaternion.Euler(0f, 0f, 15f);
                    Destroy(chain.GetComponent<Collider>());
                    chain.GetComponent<Renderer>().sharedMaterial = goldMat;
                }
            }

            metalParts = new Renderer[] { handle.GetComponent<Renderer>(), neck.GetComponent<Renderer>(), bowl.GetComponent<Renderer>(), rim.GetComponent<Renderer>(), tip.GetComponent<Renderer>() };
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
            var sh = ShaderCache.Lit;
            var m = new Material(sh) { color = c };
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Smoothness", smoothness);
            return m;
        }
    }
}
