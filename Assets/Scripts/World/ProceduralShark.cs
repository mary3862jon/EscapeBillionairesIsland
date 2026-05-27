using UnityEngine;

namespace Spoonacci
{
    // Builds a stylized shark from primitives — elongated body, dorsal fin, tail, snout.
    // After fusion, Sir Spoonacci is parented to the snout pivot.
    public class ProceduralShark : MonoBehaviour
    {
        public Transform SnoutPivot { get; private set; }
        public Color sharkColor = new Color(0.35f, 0.45f, 0.55f);
        public Color bellyColor = new Color(0.85f, 0.88f, 0.9f);

        Transform tail;

        void Awake()
        {
            BuildBody();
        }

        void BuildBody()
        {
            var mat = MakeMat(sharkColor, 0.05f, 0.5f);
            var bellyMat = MakeMat(bellyColor, 0.05f, 0.5f);

            // body (elongated capsule)
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(transform, false);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(1.2f, 2.8f, 1.2f);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // long axis = Z
            body.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(body.GetComponent<Collider>());

            // belly
            var belly = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            belly.name = "Belly";
            belly.transform.SetParent(transform, false);
            belly.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            belly.transform.localScale = new Vector3(1.05f, 2.4f, 0.7f);
            belly.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            belly.GetComponent<Renderer>().sharedMaterial = bellyMat;
            Destroy(belly.GetComponent<Collider>());

            // snout (cone-ish — stacked sphere)
            var snout = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            snout.name = "Snout";
            snout.transform.SetParent(transform, false);
            snout.transform.localPosition = new Vector3(0f, 0f, 2.6f);
            snout.transform.localScale = new Vector3(1.0f, 0.9f, 1.4f);
            snout.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(snout.GetComponent<Collider>());

            // snout pivot for spoon attachment
            var pivot = new GameObject("SnoutPivot");
            pivot.transform.SetParent(transform, false);
            pivot.transform.localPosition = new Vector3(0f, 0.4f, 3.2f);
            SnoutPivot = pivot.transform;

            // dorsal fin
            var dorsal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dorsal.name = "Dorsal";
            dorsal.transform.SetParent(transform, false);
            dorsal.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            dorsal.transform.localScale = new Vector3(0.15f, 1.2f, 1.0f);
            dorsal.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            dorsal.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(dorsal.GetComponent<Collider>());

            // pectoral fins
            for (int side = -1; side <= 1; side += 2)
            {
                var pec = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pec.name = "Pectoral";
                pec.transform.SetParent(transform, false);
                pec.transform.localPosition = new Vector3(side * 0.9f, -0.3f, 0.8f);
                pec.transform.localScale = new Vector3(1.2f, 0.12f, 0.5f);
                pec.transform.localRotation = Quaternion.Euler(0f, 0f, side * 15f);
                pec.GetComponent<Renderer>().sharedMaterial = mat;
                Destroy(pec.GetComponent<Collider>());
            }

            // tail
            var tailRoot = new GameObject("TailPivot");
            tailRoot.transform.SetParent(transform, false);
            tailRoot.transform.localPosition = new Vector3(0f, 0f, -2.4f);
            tail = tailRoot.transform;

            var tailFin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tailFin.name = "Tail Fin";
            tailFin.transform.SetParent(tail, false);
            tailFin.transform.localPosition = new Vector3(0f, 0f, -0.5f);
            tailFin.transform.localScale = new Vector3(0.15f, 1.6f, 1.0f);
            tailFin.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(tailFin.GetComponent<Collider>());

            // eyes
            for (int side = -1; side <= 1; side += 2)
            {
                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.name = "Eye";
                eye.transform.SetParent(transform, false);
                eye.transform.localPosition = new Vector3(side * 0.55f, 0.25f, 2.1f);
                eye.transform.localScale = Vector3.one * 0.18f;
                Destroy(eye.GetComponent<Collider>());
                eye.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.02f, 0.02f, 0.02f), 0f, 0.1f);
            }

            // physics
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 30f;
            rb.useGravity = false;
            rb.linearDamping = 1.5f;
            rb.angularDamping = 4f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var box = gameObject.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0f, 0f);
            box.size = new Vector3(1.4f, 1.4f, 5f);
        }

        void Update()
        {
            // tail wag
            if (tail != null)
                tail.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * 6f) * 25f, 0f);
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
