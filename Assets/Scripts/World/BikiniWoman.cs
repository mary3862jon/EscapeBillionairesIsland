using UnityEngine;

namespace Spoonacci
{
    // A clearly-feminine beach figure built from primitives — an hourglass
    // silhouette (wide hips, narrow waist, bust), bare tan legs/arms, a bikini
    // top + bottom, and flowing hair with a ponytail. This is what the pool /
    // beach should be populated with, NOT the generic blocky Civilian (which
    // just recolours a man's torso and reads as a clothed dude).
    //
    // Inherits all the Civilian behaviour (wander/idle/sunbathe, bonk, violator
    // markers) — only the BODY mesh is overridden.
    public class BikiniWoman : Civilian
    {
        public Color bikiniTop    = new Color(0.95f, 0.15f, 0.45f);
        public Color bikiniBottom = new Color(0.95f, 0.15f, 0.45f);
        public Color hairColor    = new Color(0.32f, 0.18f, 0.07f);

        static readonly Color[] BikiniPalette =
        {
            new Color(1f, 0.15f, 0.55f),   new Color(0.15f, 0.85f, 1f),
            new Color(0.95f, 0.85f, 0.1f), new Color(0.5f, 1f, 0.3f),
            new Color(0.9f, 0.2f, 0.2f),   new Color(0.75f, 0.3f, 1f),
            new Color(1f, 0.4f, 0.75f),    new Color(0.1f, 0.1f, 0.12f),
        };
        static readonly Color[] HairPalette =
        {
            new Color(0.12f, 0.08f, 0.05f), new Color(0.35f, 0.22f, 0.10f),
            new Color(0.85f, 0.72f, 0.40f), new Color(0.55f, 0.30f, 0.12f),
            new Color(0.05f, 0.05f, 0.06f), new Color(0.90f, 0.45f, 0.55f),
        };
        public static Color RandomBikini() => BikiniPalette[Random.Range(0, BikiniPalette.Length)];
        public static Color RandomHair()   => HairPalette[Random.Range(0, HairPalette.Length)];

        // One correct way to spawn a bikini woman: builds INACTIVE so the colour
        // fields are set before Awake() runs BuildBody(), then activates. For a
        // sunbather, applies a single-owner recline at `faceYaw`; otherwise just faces yaw.
        public static BikiniWoman Spawn(string name, Vector3 worldPos, Civilian.Mode mode,
                                        float faceYaw = 0f, Color? top = null, Color? hair = null)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            go.transform.position = worldPos;
            var w = go.AddComponent<BikiniWoman>();
            w.mode = mode;
            Color t = top ?? RandomBikini();
            w.bikiniTop = t; w.bikiniBottom = t;
            w.hairColor = hair ?? RandomHair();
            go.SetActive(true);
            if (mode == Civilian.Mode.Sunbathe)
                go.transform.rotation = Quaternion.Euler(-74f, faceYaw, 0f); // recline on lounger
            else
                go.transform.rotation = Quaternion.Euler(0f, faceYaw, 0f);
            return w;
        }

        protected override void BuildBody()
        {
            // a touch of variety so a crowd isn't clones
            float tan = 0.62f + Random.value * 0.18f;
            skinColor = new Color(tan + 0.32f, tan + 0.08f, tan - 0.10f);

            // ---- pelvis / hips (wide) -------------------------------------
            var hips = Prim(PrimitiveType.Sphere, "Hips", new Vector3(0f, 0.98f, 0f),
                            new Vector3(0.46f, 0.34f, 0.40f), skinColor, 0.30f);

            // bikini bottom — a snug colored band over the hips
            Prim(PrimitiveType.Sphere, "BikiniBottom", new Vector3(0f, 0.94f, 0.02f),
                 new Vector3(0.50f, 0.30f, 0.42f), bikiniBottom, 0.55f).transform.SetParent(hips.transform, true);

            // ---- legs (long, bare, tan, slightly apart) -------------------
            for (int s = -1; s <= 1; s += 2)
            {
                Prim(PrimitiveType.Capsule, "Leg", new Vector3(s * 0.13f, 0.46f, 0f),
                     new Vector3(0.16f, 0.50f, 0.16f), skinColor, 0.30f);
            }

            // ---- waist (narrow — the hourglass pinch) ---------------------
            Prim(PrimitiveType.Capsule, "Waist", new Vector3(0f, 1.22f, 0f),
                 new Vector3(0.30f, 0.26f, 0.26f), skinColor, 0.30f);

            // ---- chest + bust ---------------------------------------------
            var chest = Prim(PrimitiveType.Capsule, "Chest", new Vector3(0f, 1.46f, 0f),
                             new Vector3(0.40f, 0.24f, 0.30f), skinColor, 0.30f);
            for (int s = -1; s <= 1; s += 2)
            {
                // bust + bikini cup on top of it
                Prim(PrimitiveType.Sphere, "Bust", new Vector3(s * 0.13f, 1.50f, 0.16f),
                     new Vector3(0.20f, 0.18f, 0.18f), skinColor, 0.35f).transform.SetParent(chest.transform, true);
                Prim(PrimitiveType.Sphere, "BikiniCup", new Vector3(s * 0.13f, 1.51f, 0.20f),
                     new Vector3(0.22f, 0.20f, 0.16f), bikiniTop, 0.55f).transform.SetParent(chest.transform, true);
            }
            // bikini top string across the back/neck
            Prim(PrimitiveType.Cube, "Strap", new Vector3(0f, 1.60f, -0.02f),
                 new Vector3(0.34f, 0.04f, 0.04f), bikiniTop, 0.5f).transform.SetParent(chest.transform, true);

            // ---- arms (slim, tan, hanging) --------------------------------
            for (int s = -1; s <= 1; s += 2)
            {
                Prim(PrimitiveType.Capsule, "Arm", new Vector3(s * 0.34f, 1.36f, 0f),
                     new Vector3(0.12f, 0.34f, 0.12f), skinColor, 0.30f);
            }

            // ---- head + face ----------------------------------------------
            var head = Prim(PrimitiveType.Sphere, "Head", new Vector3(0f, 1.84f, 0f),
                            new Vector3(0.30f, 0.34f, 0.30f), skinColor, 0.32f);
            HeadTransform = head.transform;
            for (int s = -1; s <= 1; s += 2)
            {
                var e = Prim(PrimitiveType.Sphere, "Eye", new Vector3(s * 0.24f, 0.06f, 0.44f),
                             new Vector3(0.16f, 0.18f, 0.16f), new Color(0.06f, 0.05f, 0.05f), 0.2f);
                e.transform.SetParent(head.transform, false);
            }

            // ---- hair (cap + long fall + ponytail) ------------------------
            var cap = Prim(PrimitiveType.Sphere, "HairCap", new Vector3(0f, 1.90f, -0.04f),
                           new Vector3(0.36f, 0.40f, 0.38f), hairColor, 0.45f);
            cap.transform.SetParent(head.transform, false);
            // long hair falling down the back
            Prim(PrimitiveType.Capsule, "HairFall", new Vector3(0f, 1.62f, -0.20f),
                 new Vector3(0.30f, 0.30f, 0.16f), hairColor, 0.45f);
            // ponytail
            Prim(PrimitiveType.Capsule, "Ponytail", new Vector3(0f, 1.50f, -0.30f),
                 new Vector3(0.14f, 0.26f, 0.14f), hairColor, 0.45f);

            // (No root-transform sway: the Rigidbody's FreezeRotationX|Z would snap
            //  it upright anyway. Pose/orientation is owned by the spawner / DanceMove.)

            // ---- physics (same envelope as a Civilian) --------------------
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 1.3f;
            rb.linearDamping = 4f;
            rb.angularDamping = 6f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var col = gameObject.AddComponent<CapsuleCollider>();
            col.direction = 1;
            col.height = 2f;
            col.radius = 0.3f;
            col.center = new Vector3(0f, 1f, 0f);

            if (mode == Mode.Sunbathe)
            {
                // Pinned in place; the spawner owns the recline ORIENTATION so there's a
                // single source of truth for the pose (no BuildBody-vs-caller conflict).
                rb.isKinematic = true;
            }
        }

        // small primitive helper: spawns a collider-less, lit primitive parented to this body
        GameObject Prim(PrimitiveType type, string name, Vector3 localPos, Vector3 scale, Color color, float smooth)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            var c = go.GetComponent<Collider>();
            if (c != null) Destroy(c);
            go.GetComponent<Renderer>().sharedMaterial = MakeMat(color, 0f, smooth);
            return go;
        }
    }
}
