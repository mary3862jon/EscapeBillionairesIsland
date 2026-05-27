using UnityEngine;

namespace Spoonacci
{
    // Taller, fancier civilian — gold trim, sunglasses, satirical name floating above head.
    // Inherits Civilian for the violator/bonk/Spoon-Mark behavior.
    public class BillionaireNPC : Civilian
    {
        public string billionaireName = "Magnus Tusk";
        public Color suitColor = new Color(0.05f, 0.08f, 0.15f);

        protected override void BuildBody()
        {
            var skin = MakeMat(new Color(0.95f, 0.78f, 0.65f), 0f, 0.35f);
            var suit = MakeMat(suitColor, 0.1f, 0.5f);
            var gold = MakeMat(new Color(0.95f, 0.78f, 0.2f), 1f, 0.9f);
            var black = MakeMat(new Color(0.02f, 0.02f, 0.02f), 0f, 0.1f);

            // legs
            var legs = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            legs.transform.SetParent(transform, false);
            legs.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            legs.transform.localScale = new Vector3(0.45f, 0.6f, 0.45f);
            Destroy(legs.GetComponent<Collider>());
            legs.GetComponent<Renderer>().sharedMaterial = suit;

            // torso (suit)
            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.transform.SetParent(transform, false);
            torso.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            torso.transform.localScale = new Vector3(0.65f, 0.55f, 0.5f);
            Destroy(torso.GetComponent<Collider>());
            torso.GetComponent<Renderer>().sharedMaterial = suit;

            // gold tie
            var tie = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tie.transform.SetParent(transform, false);
            tie.transform.localPosition = new Vector3(0f, 1.55f, 0.32f);
            tie.transform.localScale = new Vector3(0.12f, 0.5f, 0.04f);
            Destroy(tie.GetComponent<Collider>());
            tie.GetComponent<Renderer>().sharedMaterial = gold;

            // head
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.transform.SetParent(transform, false);
            head.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            head.transform.localScale = new Vector3(0.42f, 0.46f, 0.42f);
            Destroy(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().sharedMaterial = skin;
            HeadTransform = head.transform;

            // sunglasses
            var glasses = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glasses.transform.SetParent(head.transform, false);
            glasses.transform.localPosition = new Vector3(0f, 0.1f, 0.45f);
            glasses.transform.localScale = new Vector3(0.9f, 0.18f, 0.08f);
            Destroy(glasses.GetComponent<Collider>());
            glasses.GetComponent<Renderer>().sharedMaterial = black;

            // gold watch
            var watch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            watch.transform.SetParent(transform, false);
            watch.transform.localPosition = new Vector3(-0.4f, 1.2f, 0.1f);
            watch.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            Destroy(watch.GetComponent<Collider>());
            watch.GetComponent<Renderer>().sharedMaterial = gold;

            // physics
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 3f;
            rb.linearDamping = 4f;
            rb.angularDamping = 6f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var col = gameObject.AddComponent<CapsuleCollider>();
            col.direction = 1;
            col.height = 2.4f;
            col.radius = 0.35f;
            col.center = new Vector3(0f, 1.2f, 0f);

            // name above head
            var label = new GameObject("Name");
            label.transform.SetParent(transform, false);
            label.transform.localPosition = new Vector3(0f, 2.8f, 0f);
            var w = label.AddComponent<WorldLabel>();
            w.text = billionaireName;
            w.color = new Color(1f, 0.85f, 0.2f);
            w.fontSize = 22;
        }

        void Start()
        {
            // always a violator — "BEING A BILLIONAIRE"
            SetViolator(true, "BEING A BILLIONAIRE");
        }

        protected override void Update()
        {
            base.Update();
            // re-arm violator if expired & not yet bonked
            if (!IsViolator && !HasSpoonMark)
                SetViolator(true, "BEING A BILLIONAIRE");

            // mission hook — register named billionaire as bonked
            if (HasSpoonMark)
            {
                if (billionaireName == "Magnus Tusk") MagnusTuskState.Bonked = true;
                BillionaireRegistry.MarkBonked(billionaireName);
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
