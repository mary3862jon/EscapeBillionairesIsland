using UnityEngine;

namespace Spoonacci
{
    // Builds a crude spoon from primitives at runtime.
    // Bowl = squashed sphere, handle = cylinder. Replace with real model later.
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class ProceduralSpoonBuilder : MonoBehaviour
    {
        public Color skinColor = new Color(0.85f, 0.85f, 0.9f); // default silver
        public float metallic = 0.9f;
        public float smoothness = 0.85f;

        Renderer bowlRenderer;
        Renderer handleRenderer;
        Material runtimeMat;

        void Awake()
        {
            BuildMesh();
            ApplySkin(skinColor, metallic, smoothness);
        }

        void BuildMesh()
        {
            // bowl
            var bowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bowl.name = "Bowl";
            bowl.transform.SetParent(transform, false);
            bowl.transform.localPosition = new Vector3(0f, 0.05f, 0.35f);
            bowl.transform.localScale = new Vector3(0.45f, 0.18f, 0.6f);
            Destroy(bowl.GetComponent<Collider>());
            bowlRenderer = bowl.GetComponent<Renderer>();

            // handle
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Handle";
            handle.transform.SetParent(transform, false);
            handle.transform.localPosition = new Vector3(0f, 0.05f, -0.25f);
            handle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            handle.transform.localScale = new Vector3(0.12f, 0.6f, 0.12f);
            Destroy(handle.GetComponent<Collider>());
            handleRenderer = handle.GetComponent<Renderer>();

            // physics body (so it can move/collide)
            var col = GetComponent<CapsuleCollider>();
            col.direction = 2; // Z-axis
            col.height = 1.4f;
            col.radius = 0.22f;
            col.center = new Vector3(0f, 0.05f, 0.05f);

            var rb = GetComponent<Rigidbody>();
            rb.mass = 0.2f;
            rb.linearDamping = 4f;
            rb.angularDamping = 6f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void ApplySkin(Color color, float met = 0.9f, float smooth = 0.85f)
        {
            skinColor = color;
            metallic = met;
            smoothness = smooth;

            // URP/Lit shader
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (runtimeMat == null) runtimeMat = new Material(shader);
            runtimeMat.color = color;
            runtimeMat.SetFloat("_Metallic", met);
            runtimeMat.SetFloat("_Smoothness", smooth);

            if (bowlRenderer != null) bowlRenderer.sharedMaterial = runtimeMat;
            if (handleRenderer != null) handleRenderer.sharedMaterial = runtimeMat;
        }
    }
}
