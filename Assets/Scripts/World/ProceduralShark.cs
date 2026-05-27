using UnityEngine;

namespace Spoonacci
{
    // Big, bright, very visibly SHARK-shaped. Floats with dorsal fin above water.
    // After fusion, Sir Spoonacci is parented to SnoutPivot.
    public class ProceduralShark : MonoBehaviour
    {
        public Transform SnoutPivot { get; private set; }
        public Color sharkColor = new Color(0.45f, 0.55f, 0.7f);
        public Color bellyColor = new Color(0.95f, 0.95f, 0.98f);
        Transform tail;

        void Awake() => BuildBody();

        void BuildBody()
        {
            var mat = MakeMat(sharkColor, 0.05f, 0.5f);
            var bellyMat = MakeMat(bellyColor, 0.05f, 0.5f);
            var black = MakeMat(new Color(0.03f, 0.03f, 0.03f), 0f, 0.1f);

            // body — big elongated capsule, long axis Z
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(transform, false);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(1.6f, 4.0f, 1.6f);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(body.GetComponent<Collider>());

            // belly stripe (lighter)
            var belly = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            belly.name = "Belly";
            belly.transform.SetParent(transform, false);
            belly.transform.localPosition = new Vector3(0f, -0.6f, 0f);
            belly.transform.localScale = new Vector3(1.45f, 3.6f, 0.9f);
            belly.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            belly.GetComponent<Renderer>().sharedMaterial = bellyMat;
            Destroy(belly.GetComponent<Collider>());

            // snout (pointed front)
            var snout = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            snout.name = "Snout";
            snout.transform.SetParent(transform, false);
            snout.transform.localPosition = new Vector3(0f, 0f, 3.6f);
            snout.transform.localScale = new Vector3(1.2f, 1.0f, 1.6f);
            snout.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(snout.GetComponent<Collider>());

            // snout pivot for spoon attachment
            var pivot = new GameObject("SnoutPivot");
            pivot.transform.SetParent(transform, false);
            pivot.transform.localPosition = new Vector3(0f, 0.5f, 4.2f);
            SnoutPivot = pivot.transform;

            // mouth (big black slit on underside of snout)
            var mouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mouth.transform.SetParent(transform, false);
            mouth.transform.localPosition = new Vector3(0f, -0.35f, 3.9f);
            mouth.transform.localScale = new Vector3(0.9f, 0.08f, 0.7f);
            mouth.GetComponent<Renderer>().sharedMaterial = black;
            Destroy(mouth.GetComponent<Collider>());

            // teeth (4 small white triangles in the mouth)
            for (int i = 0; i < 5; i++)
            {
                var tooth = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tooth.transform.SetParent(transform, false);
                tooth.transform.localPosition = new Vector3((i - 2) * 0.18f, -0.32f, 3.95f);
                tooth.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);
                Destroy(tooth.GetComponent<Collider>());
                tooth.GetComponent<Renderer>().sharedMaterial = MakeMat(Color.white, 0.1f, 0.7f);
            }

            // BIG dorsal fin (very visible above water)
            var dorsal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dorsal.name = "Dorsal Fin";
            dorsal.transform.SetParent(transform, false);
            dorsal.transform.localPosition = new Vector3(0f, 1.6f, 0.4f);
            dorsal.transform.localScale = new Vector3(0.2f, 1.8f, 1.5f);
            dorsal.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            // make it triangular-ish by adding a topper
            var dorsalTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dorsalTop.transform.SetParent(dorsal.transform, false);
            dorsalTop.transform.localScale = new Vector3(1.0f, 0.5f, 0.6f);
            dorsalTop.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            Destroy(dorsal.GetComponent<Collider>());
            Destroy(dorsalTop.GetComponent<Collider>());
            dorsal.GetComponent<Renderer>().sharedMaterial = mat;
            dorsalTop.GetComponent<Renderer>().sharedMaterial = mat;

            // pectoral fins
            for (int side = -1; side <= 1; side += 2)
            {
                var pec = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pec.name = "Pectoral";
                pec.transform.SetParent(transform, false);
                pec.transform.localPosition = new Vector3(side * 1.1f, -0.35f, 1.2f);
                pec.transform.localScale = new Vector3(1.5f, 0.18f, 0.6f);
                pec.transform.localRotation = Quaternion.Euler(0f, 0f, side * 22f);
                pec.GetComponent<Renderer>().sharedMaterial = mat;
                Destroy(pec.GetComponent<Collider>());
            }

            // tail pivot for wagging
            var tailRoot = new GameObject("TailPivot");
            tailRoot.transform.SetParent(transform, false);
            tailRoot.transform.localPosition = new Vector3(0f, 0f, -3.2f);
            tail = tailRoot.transform;

            // tail fin
            var tailFin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tailFin.transform.SetParent(tail, false);
            tailFin.transform.localPosition = new Vector3(0f, 0.5f, -0.6f);
            tailFin.transform.localScale = new Vector3(0.2f, 2.4f, 1.2f);
            tailFin.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(tailFin.GetComponent<Collider>());

            var tailFin2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tailFin2.transform.SetParent(tail, false);
            tailFin2.transform.localPosition = new Vector3(0f, -0.4f, -0.4f);
            tailFin2.transform.localScale = new Vector3(0.2f, 1.0f, 0.8f);
            tailFin2.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(tailFin2.GetComponent<Collider>());

            // BIG eyes
            for (int side = -1; side <= 1; side += 2)
            {
                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.transform.SetParent(transform, false);
                eye.transform.localPosition = new Vector3(side * 0.7f, 0.3f, 2.6f);
                eye.transform.localScale = Vector3.one * 0.32f;
                Destroy(eye.GetComponent<Collider>());
                eye.GetComponent<Renderer>().sharedMaterial = MakeMat(Color.white, 0.05f, 0.4f);

                var pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pupil.transform.SetParent(eye.transform, false);
                pupil.transform.localPosition = new Vector3(0f, 0f, 0.4f);
                pupil.transform.localScale = Vector3.one * 0.5f;
                Destroy(pupil.GetComponent<Collider>());
                pupil.GetComponent<Renderer>().sharedMaterial = black;
            }

            // gill stripes (3 dark slashes per side)
            for (int side = -1; side <= 1; side += 2)
            {
                for (int g = 0; g < 3; g++)
                {
                    var gill = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    gill.transform.SetParent(transform, false);
                    gill.transform.localPosition = new Vector3(side * 0.75f, 0.05f, 1.4f - g * 0.3f);
                    gill.transform.localScale = new Vector3(0.05f, 0.5f, 0.06f);
                    gill.transform.localRotation = Quaternion.Euler(0f, 0f, side * 25f);
                    Destroy(gill.GetComponent<Collider>());
                    gill.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.2f, 0.25f, 0.35f), 0f, 0.3f);
                }
            }

            // physics — buoyant
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 30f;
            rb.useGravity = false;
            rb.linearDamping = 2.5f;
            rb.angularDamping = 5f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var box = gameObject.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0f, 0f);
            box.size = new Vector3(2f, 1.8f, 7f);
        }

        void Update()
        {
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
