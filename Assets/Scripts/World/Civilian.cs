using UnityEngine;

namespace Spoonacci
{
    // Generic human NPC built from primitives. Wanders or idles.
    // Optionally has a "occupation" mesh tweak (sunbather, bartender, drunk).
    // Can be flagged as a Violator at runtime — RedArrowMarker auto-spawns above head.
    public class Civilian : MonoBehaviour
    {
        public enum Mode { Wander, Idle, Sunbathe, Patrol, Yoga }
        public Mode mode = Mode.Wander;
        public Color shirtColor = new Color(0.8f, 0.8f, 0.9f);
        public Color pantsColor = new Color(0.2f, 0.2f, 0.3f);
        public Color skinColor = new Color(0.95f, 0.78f, 0.65f);
        public float walkSpeed = 1.5f;
        public Vector3 patrolCenter;
        public float patrolRadius = 6f;

        public bool IsViolator { get; private set; }
        public bool HasSpoonMark { get; private set; }
        public float OutrageEndsAt { get; private set; }
        public Transform HeadTransform { get; private set; }
        public Vector3 KnockbackTarget; // populated by bonk

        Vector3 targetPoint;
        float nextRePath;
        RedArrowMarker arrow;
        SpoonMarkDecal mark;
        float stunTimer;

        void Awake()
        {
            patrolCenter = transform.position;
            BuildBody();
            PickNewPoint();
        }

        void BuildBody()
        {
            // legs (cylinder)
            var legs = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            legs.name = "Legs";
            legs.transform.SetParent(transform, false);
            legs.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            legs.transform.localScale = new Vector3(0.4f, 0.5f, 0.4f);
            Destroy(legs.GetComponent<Collider>());
            legs.GetComponent<Renderer>().sharedMaterial = MakeMat(pantsColor, 0f, 0.3f);

            // torso
            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso";
            torso.transform.SetParent(transform, false);
            torso.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            torso.transform.localScale = new Vector3(0.55f, 0.5f, 0.4f);
            Destroy(torso.GetComponent<Collider>());
            torso.GetComponent<Renderer>().sharedMaterial = MakeMat(shirtColor, 0f, 0.4f);

            // head
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(transform, false);
            head.transform.localPosition = new Vector3(0f, 1.85f, 0f);
            head.transform.localScale = new Vector3(0.32f, 0.36f, 0.32f);
            Destroy(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().sharedMaterial = MakeMat(skinColor, 0f, 0.35f);
            HeadTransform = head.transform;

            // tiny eyes
            for (int side = -1; side <= 1; side += 2)
            {
                var e = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                e.name = "Eye";
                e.transform.SetParent(head.transform, false);
                e.transform.localPosition = new Vector3(side * 0.25f, 0.05f, 0.45f);
                e.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
                Destroy(e.GetComponent<Collider>());
                e.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.05f, 0.05f, 0.05f), 0f, 0.2f);
            }

            // physics
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 1.5f;
            rb.linearDamping = 4f;
            rb.angularDamping = 6f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var cap = gameObject.AddComponent<CapsuleCollider>();
            cap.direction = 1;
            cap.height = 2f;
            cap.radius = 0.3f;
            cap.center = new Vector3(0f, 1f, 0f);

            if (mode == Mode.Sunbathe)
            {
                // lying flat on a deck chair
                transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                legs.transform.localPosition = new Vector3(0f, 0.18f, 0.4f);
                legs.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                torso.transform.localPosition = new Vector3(0f, 0.25f, -0.4f);
                torso.transform.localRotation = Quaternion.Euler(85f, 0f, 0f);
                head.transform.localPosition = new Vector3(0f, 0.32f, -0.9f);
                rb.constraints |= RigidbodyConstraints.FreezePositionY;
                rb.isKinematic = true;
            }
        }

        void Update()
        {
            if (IsViolator && Time.time > OutrageEndsAt)
            {
                SetViolator(false, "");
            }

            if (stunTimer > 0f) { stunTimer -= Time.deltaTime; return; }
            if (mode == Mode.Sunbathe || mode == Mode.Idle || mode == Mode.Yoga) return;

            Vector3 to = targetPoint - transform.position;
            to.y = 0f;
            if (to.magnitude < 0.6f || Time.time > nextRePath) { PickNewPoint(); return; }
            transform.position += to.normalized * walkSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to.normalized, Vector3.up), 4f * Time.deltaTime);
        }

        void PickNewPoint()
        {
            Vector2 r = Random.insideUnitCircle * patrolRadius;
            targetPoint = patrolCenter + new Vector3(r.x, 0f, r.y);
            nextRePath = Time.time + 6f + Random.value * 4f;
        }

        // ------- Violator API -------
        public string ViolationLabel { get; private set; } = "";
        public void SetViolator(bool on, string label)
        {
            IsViolator = on;
            ViolationLabel = on ? label : "";
            if (on)
            {
                OutrageEndsAt = Time.time + 12f;
                if (arrow == null)
                {
                    var go = new GameObject("RedArrow");
                    arrow = go.AddComponent<RedArrowMarker>();
                    arrow.Attach(transform, label);
                }
                arrow.SetLabel(label);
                arrow.gameObject.SetActive(true);
                ViolatorRegistry.Register(this);
            }
            else
            {
                if (arrow != null) arrow.gameObject.SetActive(false);
                ViolatorRegistry.Unregister(this);
            }
        }

        public void ReceiveBonk(Vector3 fromPos)
        {
            HasSpoonMark = true;
            stunTimer = 2.2f;
            // knockback
            var rb = GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                Vector3 push = (transform.position - fromPos);
                push.y = 0f;
                push = push.normalized * 4.5f + Vector3.up * 3f;
                rb.AddForce(push, ForceMode.VelocityChange);
            }
            // mark visual
            if (mark == null && HeadTransform != null)
            {
                var go = new GameObject("SpoonMark");
                mark = go.AddComponent<SpoonMarkDecal>();
                mark.Attach(HeadTransform);
            }
            SetViolator(false, "");
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
