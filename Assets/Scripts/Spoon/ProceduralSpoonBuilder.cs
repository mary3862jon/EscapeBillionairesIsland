using UnityEngine;

namespace Spoonacci
{
    // Sir Spoonacci — upright character built from primitives.
    // Bowl uses a flattened Capsule rotated horizontally (proper oval scoop silhouette)
    // rather than a sphere or sphere+inset (which read as bolt/toilet seat).
    public class ProceduralSpoonBuilder : MonoBehaviour
    {
        public Color skinColor = new Color(0.78f, 0.78f, 0.82f);
        public float metallic = 0.4f;  // matte-ish so the silhouette reads clearly (was mirror-shiny)
        public float smoothness = 0.35f;

        [Header("Proportions (set before Awake to override)")]
        public float bodyHeight = 0.95f;
        public float bodyRadius = 0.035f;            // very thin handle
        public Vector3 bowlSize = new Vector3(0.35f, 0.10f, 0.65f); // width × thickness × length
        public bool addFace = true;
        public bool addMonocle = true;

        Renderer[] metalParts;
        Material runtimeMat;
        public Transform Visual { get; private set; }

        void Awake()
        {
            ApplyType(GameState.CurrentSpoonType);
            BuildBody();
            ApplySkin(skinColor, metallic, smoothness);
        }

        public void ApplyType(int idx)
        {
            if (SpoonTypeLibrary.All == null || SpoonTypeLibrary.All.Count == 0) return;
            idx = Mathf.Clamp(idx, 0, SpoonTypeLibrary.All.Count - 1);
            var t = SpoonTypeLibrary.All[idx];
            bodyHeight = t.handleHeight;
            bodyRadius = t.handleRadius;
            bowlSize = new Vector3(t.bowlWidth, t.bowlThickness, t.bowlLength);
            skinColor = t.color;
            metallic = t.metallic;
            smoothness = t.smoothness;
        }

        // Destroy current visuals and rebuild — used by SpoonTypeSwitcher when keys 1-9 are pressed
        public void RebuildFromCurrentType()
        {
            if (Visual != null) Destroy(Visual.gameObject);
            ApplyType(GameState.CurrentSpoonType);
            runtimeMat = null;
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

            // Procedural OBVIOUS spoon — primitive approach (skipping the import models which weren't rendering reliably)

            // HANDLE — vertical Cube (thin tall rectangular pole). Cube reads cleaner than cylinder at this thinness.
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "Handle";
            handle.transform.SetParent(Visual, false);
            handle.transform.localPosition = new Vector3(0f, bodyHeight * 0.5f, 0f);
            handle.transform.localScale = new Vector3(bodyRadius * 2.4f, bodyHeight, bodyRadius * 2.4f);
            Destroy(handle.GetComponent<Collider>());

            // NECK — small connector at top of handle where it meets the bowl
            var neck = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            neck.name = "Neck";
            neck.transform.SetParent(Visual, false);
            neck.transform.localPosition = new Vector3(0f, bodyHeight, 0f);
            neck.transform.localScale = Vector3.one * (bodyRadius * 2.5f);
            Destroy(neck.GetComponent<Collider>());

            // BOWL — Capsule oriented horizontally along +Z, tilted 25° back so the scoop opens upward.
            // Capsule's rounded ends = the rounded scoop tip + the rounded neck-connection.
            // Position so the BACK end of the capsule meets the neck and the FRONT end extends forward.
            var bowl = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bowl.name = "Bowl";
            bowl.transform.SetParent(Visual, false);
            // Capsule default long axis = Y. 90° around X = lays along world +Z. +25° more = back-tilt so scoop visible
            bowl.transform.localRotation = Quaternion.Euler(115f, 0f, 0f);
            // Position: shift forward so the front of the capsule extends out from the handle tip
            bowl.transform.localPosition = new Vector3(0f, bodyHeight + 0.02f, bowlSize.z * 0.4f);
            // After 90°X rotation: localScale.x = width, localScale.y = length (was Y), localScale.z = vertical thickness
            bowl.transform.localScale = new Vector3(bowlSize.x, bowlSize.z, bowlSize.y);
            Destroy(bowl.GetComponent<Collider>());

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

            metalParts = new Renderer[] { handle.GetComponent<Renderer>(), neck.GetComponent<Renderer>(), bowl.GetComponent<Renderer>() };
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
