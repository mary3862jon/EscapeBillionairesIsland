using UnityEngine;

namespace Spoonacci
{
    // Sir Spoonacci — upright character built from primitives.
    // Bowl uses a flattened Capsule rotated horizontally (proper oval scoop silhouette)
    // rather than a sphere or sphere+inset (which read as bolt/toilet seat).
    public class ProceduralSpoonBuilder : MonoBehaviour
    {
        public Color skinColor = new Color(0.85f, 0.85f, 0.9f);
        public float metallic = 0.85f;
        public float smoothness = 0.7f;

        [Header("Proportions (set before Awake to override)")]
        public float bodyHeight = 0.9f;
        public float bodyRadius = 0.08f;
        public Vector3 bowlSize = new Vector3(0.45f, 0.12f, 0.85f); // x,y,z — flat oval
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
            col.direction = 1;
            col.height = bodyHeight + 0.25f;
            col.radius = Mathf.Max(bodyRadius * 1.3f, 0.16f);
            col.center = new Vector3(0f, (bodyHeight + 0.25f) * 0.5f - 0.05f, 0f);

            var visualGo = new GameObject("Visual");
            visualGo.transform.SetParent(transform, false);
            Visual = visualGo.transform;

            // 1) Try Kenney Food Kit utensil-spoon (CC0). Bounds: X=±0.24 (length), Y=0..0.03 (thickness), Z=±0.06 (width).
            //    Laid flat on XZ plane. Rotate -90° around Z so length becomes vertical (handle down, bowl up).
            var kenneyPrefab = Resources.Load<GameObject>("Models/kenney_spoon");
            if (kenneyPrefab != null)
            {
                var pivot = new GameObject("SpoonPivot");
                pivot.transform.SetParent(Visual, false);

                var instance = Instantiate(kenneyPrefab, pivot.transform);
                instance.name = "Kenney Spoon";
                instance.transform.localPosition = Vector3.zero;
                // Rotate -90° Z → flips long axis from X to Y (vertical). Then 180° Y to ensure bowl points up
                instance.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
                // Original X length = 0.48 → after rotation, vertical height = 0.48 * scale.
                // Want spoon ~1.2m tall → scale = 2.5
                instance.transform.localScale = Vector3.one * 2.5f;
                // Lift so handle bottom sits at world ground. After scale: handle bottom at local Y = 0 (since original X=-0.24 * scale 2.5 = -0.6 in world Y).
                pivot.transform.localPosition = new Vector3(0f, 0.6f, 0f);

                metalParts = instance.GetComponentsInChildren<Renderer>();
                if (addFace) AddSimpleEyes(Visual, new Vector3(0f, 1.05f, 0.12f));
                return;
            }

            // 2) Fall back to drummyfish CC0 model (the previous one)
            var realPrefab = Resources.Load<GameObject>("Models/spoon");
            if (realPrefab != null)
            {
                var pivot = new GameObject("SpoonPivot");
                pivot.transform.SetParent(Visual, false);
                var instance = Instantiate(realPrefab, pivot.transform);
                instance.name = "Real Spoon Model";
                instance.transform.localScale = Vector3.one * 0.3f;
                pivot.transform.localPosition = new Vector3(0f, 0.9f, 0f);
                metalParts = instance.GetComponentsInChildren<Renderer>();
                if (addFace) AddSimpleEyes(Visual, new Vector3(0f, 1.1f, 0.12f));
                return;
            }
            // 3) Procedural fallback (capsule-based bowl)

            // HANDLE — thin vertical cylinder
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Handle";
            handle.transform.SetParent(Visual, false);
            handle.transform.localPosition = new Vector3(0f, bodyHeight * 0.5f, 0f);
            handle.transform.localScale = new Vector3(bodyRadius * 2f, bodyHeight * 0.5f, bodyRadius * 2f);
            Destroy(handle.GetComponent<Collider>());

            // HANDLE TOP TAPER — slightly wider sphere transitioning into the bowl
            var neck = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            neck.name = "Neck";
            neck.transform.SetParent(Visual, false);
            neck.transform.localPosition = new Vector3(0f, bodyHeight + 0.02f, 0.02f);
            neck.transform.localScale = new Vector3(bodyRadius * 2.6f, bodyRadius * 2.0f, bodyRadius * 2.6f);
            Destroy(neck.GetComponent<Collider>());

            // BOWL — Capsule rotated to lie HORIZONTALLY, scaled to flat oval pill
            // = the silhouette of a real spoon scoop (longer than wide, very thin)
            var bowl = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bowl.name = "Bowl";
            bowl.transform.SetParent(Visual, false);
            // tilt forward 15° so we see the scoop curve from camera
            bowl.transform.localRotation = Quaternion.Euler(105f, 0f, 0f); // 90° lay-flat + 15° forward tilt
            bowl.transform.localPosition = new Vector3(0f, bodyHeight + bowlSize.y * 0.5f, bowlSize.z * 0.35f);
            bowl.transform.localScale = new Vector3(bowlSize.x, bowlSize.z, bowlSize.y);
            Destroy(bowl.GetComponent<Collider>());

            // BOWL EDGE — a thin gold-ish ring around the rim to emphasize the spoon edge
            var rim = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rim.name = "Bowl Rim";
            rim.transform.SetParent(Visual, false);
            rim.transform.localRotation = bowl.transform.localRotation;
            rim.transform.localPosition = bowl.transform.localPosition + new Vector3(0f, 0.005f, 0f);
            rim.transform.localScale = new Vector3(bowlSize.x * 1.04f, bowlSize.z * 1.02f, bowlSize.y * 0.4f);
            Destroy(rim.GetComponent<Collider>());

            // FACE — eyes/brows/mouth/monocle on the side of the bowl facing the camera
            if (addFace)
            {
                var blackMat = MakeMat(new Color(0.02f, 0.02f, 0.02f), 0f, 0.1f);
                var goldMat  = MakeMat(new Color(0.95f, 0.78f, 0.2f), 1f, 0.9f);

                // face plane = on the underside of the tilted bowl, slightly back from center
                Vector3 faceCenter = bowl.transform.localPosition + new Vector3(0f, -bowlSize.y * 0.45f, -bowlSize.z * 0.25f);

                // EYES
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector3 ep = faceCenter + new Vector3(side * bowlSize.x * 0.25f, 0f, 0f);
                    var sclera = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sclera.name = "Eye White";
                    sclera.transform.SetParent(Visual, false);
                    sclera.transform.localPosition = ep;
                    sclera.transform.localScale = Vector3.one * 0.095f;
                    Destroy(sclera.GetComponent<Collider>());
                    sclera.GetComponent<Renderer>().sharedMaterial = MakeMat(Color.white, 0f, 0.4f);

                    var pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    pupil.transform.SetParent(sclera.transform, false);
                    pupil.transform.localPosition = new Vector3(0f, -0.45f, 0f);
                    pupil.transform.localScale = Vector3.one * 0.55f;
                    Destroy(pupil.GetComponent<Collider>());
                    pupil.GetComponent<Renderer>().sharedMaterial = blackMat;
                }

                // EYEBROWS
                for (int side = -1; side <= 1; side += 2)
                {
                    var brow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    brow.name = "Brow";
                    brow.transform.SetParent(Visual, false);
                    brow.transform.localPosition = faceCenter + new Vector3(side * bowlSize.x * 0.25f, 0.07f, 0f);
                    brow.transform.localScale = new Vector3(0.14f, 0.03f, 0.03f);
                    brow.transform.localRotation = Quaternion.Euler(0f, 0f, side * -18f);
                    Destroy(brow.GetComponent<Collider>());
                    brow.GetComponent<Renderer>().sharedMaterial = blackMat;
                }

                // MOUTH
                var mouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mouth.name = "Mouth";
                mouth.transform.SetParent(Visual, false);
                mouth.transform.localPosition = faceCenter + new Vector3(0f, -0.07f, 0f);
                mouth.transform.localScale = new Vector3(0.12f, 0.025f, 0.025f);
                Destroy(mouth.GetComponent<Collider>());
                mouth.GetComponent<Renderer>().sharedMaterial = blackMat;

                // MONOCLE — gold ring + chain
                if (addMonocle)
                {
                    var ring = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    ring.name = "Monocle";
                    ring.transform.SetParent(Visual, false);
                    ring.transform.localPosition = faceCenter + new Vector3(bowlSize.x * 0.25f, 0f, 0f);
                    ring.transform.localScale = new Vector3(0.15f, 0.15f, 0.03f);
                    Destroy(ring.GetComponent<Collider>());
                    ring.GetComponent<Renderer>().sharedMaterial = goldMat;

                    var chain = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    chain.transform.SetParent(ring.transform, false);
                    chain.transform.localPosition = new Vector3(0f, -0.5f, 0f);
                    chain.transform.localScale = new Vector3(0.06f, 1.5f, 0.06f);
                    chain.transform.localRotation = Quaternion.Euler(0f, 0f, 15f);
                    Destroy(chain.GetComponent<Collider>());
                    chain.GetComponent<Renderer>().sharedMaterial = goldMat;
                }
            }

            metalParts = new Renderer[] { handle.GetComponent<Renderer>(), neck.GetComponent<Renderer>(), bowl.GetComponent<Renderer>(), rim.GetComponent<Renderer>() };
        }

        public void ApplySkin(Color color, float met = 0.85f, float smooth = 0.7f)
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

        void AddSimpleEyes(Transform parent, Vector3 center)
        {
            var black = MakeMat(new Color(0.02f, 0.02f, 0.02f), 0f, 0.1f);
            for (int side = -1; side <= 1; side += 2)
            {
                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.transform.SetParent(parent, false);
                eye.transform.localPosition = center + new Vector3(side * 0.07f, 0.0f, 0.0f);
                eye.transform.localScale = Vector3.one * 0.07f;
                Destroy(eye.GetComponent<Collider>());
                eye.GetComponent<Renderer>().sharedMaterial = black;
            }
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
