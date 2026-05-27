using UnityEngine;

namespace Spoonacci
{
    // Builds the open-island demo at runtime. Beach + pool + bar + Salon + ~14 NPCs (mix of sunbathers, walkers, bartenders, security) + the ViolationSpawner.
    public class IslandBootstrapper : MonoBehaviour
    {
        public bool spawnSalon = true;
        public bool spawnSandbox = true;

        GameObject spoon;
        HudText hud;

        void Awake()
        {
            BuildGround();
            BuildOcean();
            BuildSpoon();
            WireCamera();
            BuildHud();
            if (spawnSalon) BuildSalon();
            if (spawnSandbox)
            {
                BuildPalmRing();
                BuildPool();
                BuildBar();
                BuildDeckChairs();
                BuildNpcs();
                BuildViolationSpawner();
            }
        }

        void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground (Beach)";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10f, 1f, 10f); // 100x100
            ground.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.86f, 0.62f), 0f, 0.18f);
        }

        void BuildOcean()
        {
            var ocean = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ocean.name = "Ocean";
            ocean.transform.position = new Vector3(0f, -0.15f, 0f);
            ocean.transform.localScale = new Vector3(80f, 1f, 80f);
            ocean.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.12f, 0.42f, 0.7f), 0.15f, 0.92f);
            Destroy(ocean.GetComponent<Collider>());
        }

        void BuildSpoon()
        {
            spoon = new GameObject("Sir Spoonacci");
            spoon.transform.position = new Vector3(0f, 0.5f, 0f);
            spoon.AddComponent<Rigidbody>();
            spoon.AddComponent<CapsuleCollider>();
            spoon.AddComponent<ProceduralSpoonBuilder>();
            spoon.AddComponent<SpoonAnimator>();
            spoon.AddComponent<SpoonController>();
            spoon.AddComponent<BonkAttack>();
        }

        void WireCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
                camGo.AddComponent<AudioListener>();
            }
            cam.backgroundColor = new Color(0.55f, 0.78f, 0.95f);
            var follower = cam.GetComponent<ThirdPersonCamera>() ?? cam.gameObject.AddComponent<ThirdPersonCamera>();
            follower.target = spoon.transform;
        }

        void BuildHud()
        {
            var hudGo = new GameObject("HUD");
            hud = hudGo.AddComponent<HudText>();
            hud.Set("WASD walk · Space hop · Right-mouse look · E interact · F BONK violators!");
            Invoke(nameof(ClearHud), 8f);
        }

        void ClearHud() { if (hud != null) hud.Set(""); }

        void BuildSalon()
        {
            var salon = new GameObject("Salon Cucchiaio");
            salon.transform.position = new Vector3(7f, 0f, 5f);

            // building
            var bld = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bld.name = "Salon Building";
            bld.transform.SetParent(salon.transform, false);
            bld.transform.localScale = new Vector3(4f, 2.6f, 3f);
            bld.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            bld.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(1.0f, 0.65f, 0.8f), 0.05f, 0.5f);

            // pink roof
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(salon.transform, false);
            roof.transform.localScale = new Vector3(4.4f, 0.25f, 3.4f);
            roof.transform.localPosition = new Vector3(0f, 2.7f, 0f);
            roof.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.92f, 0.45f, 0.7f), 0.3f, 0.6f);

            // BIG sign on roof — uppercase
            var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "Sign Plate";
            sign.transform.SetParent(salon.transform, false);
            sign.transform.localScale = new Vector3(3.8f, 0.7f, 0.08f);
            sign.transform.localPosition = new Vector3(0f, 3.3f, -1.55f);
            sign.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.98f, 0.96f, 0.92f), 0.1f, 0.4f);

            // Front counter (the user explicitly asked for this)
            var counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "Counter";
            counter.transform.SetParent(salon.transform, false);
            counter.transform.localScale = new Vector3(3.4f, 1f, 0.7f);
            counter.transform.localPosition = new Vector3(0f, 0.5f, -1.8f);
            counter.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.88f, 0.78f), 0.0f, 0.4f);

            // counter-top trim (gold)
            var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = "Counter Trim";
            trim.transform.SetParent(salon.transform, false);
            trim.transform.localScale = new Vector3(3.5f, 0.06f, 0.72f);
            trim.transform.localPosition = new Vector3(0f, 1.0f, -1.8f);
            trim.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.78f, 0.25f), 1f, 0.9f);

            // attendant — Salon Spoon NPC
            var att = new GameObject("Salon Attendant");
            att.transform.position = salon.transform.position + new Vector3(0f, 0.6f, -1.2f);
            var attendant = att.AddComponent<NpcSpoon>();
            attendant.crewName = "Coiffeur Cucchiaio";
            attendant.color = new Color(1f, 0.65f, 0.8f);
            attendant.metallic = 0.6f;
            attendant.smoothness = 0.85f;

            // pink towel/flag posts
            for (int i = -1; i <= 1; i += 2)
            {
                var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.name = "Salon Pole";
                pole.transform.SetParent(salon.transform, false);
                pole.transform.localPosition = new Vector3(2.3f * i, 1.5f, -2f);
                pole.transform.localScale = new Vector3(0.1f, 1.5f, 0.1f);
                pole.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.5f, 0.75f), 0.6f, 0.5f);
            }

            // interactable trigger zone in front of the counter
            var interactGo = new GameObject("Salon Trigger");
            interactGo.transform.SetParent(salon.transform, false);
            interactGo.transform.localPosition = new Vector3(0f, 0.6f, -2.8f);
            var sc = interactGo.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 2.5f;
            var interact = interactGo.AddComponent<SalonCucchiaio>();
            interact.prompt = hud;
            interact.frontSignText = "SALON  CUCCHIAIO";

            // big floating signboard text via SignText
            var signText = sign.AddComponent<WorldLabel>();
            signText.text = "SALON CUCCHIAIO";
            signText.color = new Color(0.6f, 0.15f, 0.45f);
            signText.fontSize = 28;
            signText.worldOffset = new Vector3(0f, 0f, -0.1f);
        }

        void BuildPalmRing()
        {
            for (int i = 0; i < 10; i++)
            {
                float ang = (i / 10f) * Mathf.PI * 2f;
                float r = 18f + ((i % 3) * 2f);
                var pos = new Vector3(Mathf.Cos(ang) * r, 0f, Mathf.Sin(ang) * r);
                BuildPalm(pos);
            }
            // some scattered closer
            for (int i = 0; i < 6; i++)
            {
                BuildPalm(new Vector3(Random.Range(-12f, 12f), 0f, Random.Range(-12f, 12f)));
            }
        }

        void BuildPalm(Vector3 pos)
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Palm Trunk";
            trunk.transform.position = pos + Vector3.up * 2.2f;
            trunk.transform.localScale = new Vector3(0.32f, 2.2f, 0.32f);
            trunk.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.4f, 0.26f, 0.16f), 0f, 0.3f);

            // 4 leaf clusters around top
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90f * Mathf.Deg2Rad;
                var leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leaf.name = "Palm Leaf";
                leaf.transform.position = pos + Vector3.up * 4.3f + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 1.1f;
                leaf.transform.localScale = new Vector3(1.6f, 0.35f, 0.7f);
                leaf.transform.rotation = Quaternion.Euler(0f, i * 90f, -18f);
                Destroy(leaf.GetComponent<Collider>());
                leaf.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.18f, 0.55f, 0.22f), 0f, 0.3f);
            }
            // coconut
            var nut = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nut.name = "Coconut";
            nut.transform.position = pos + Vector3.up * 4.1f + new Vector3(0.3f, 0f, 0.3f);
            nut.transform.localScale = Vector3.one * 0.25f;
            Destroy(nut.GetComponent<Collider>());
            nut.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.25f, 0.15f, 0.08f), 0.1f, 0.4f);
        }

        void BuildPool()
        {
            var poolRoot = new GameObject("Pool");
            poolRoot.transform.position = new Vector3(-8f, 0f, -2f);

            // pool deck (cream)
            var deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.name = "Pool Deck";
            deck.transform.SetParent(poolRoot.transform, false);
            deck.transform.localScale = new Vector3(10f, 0.1f, 7f);
            deck.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            deck.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.97f, 0.93f, 0.85f), 0f, 0.4f);

            // pool water (cyan, slightly sunken visually with thin slab)
            var water = GameObject.CreatePrimitive(PrimitiveType.Cube);
            water.name = "Pool Water";
            water.transform.SetParent(poolRoot.transform, false);
            water.transform.localScale = new Vector3(7f, 0.1f, 5f);
            water.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            water.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.25f, 0.7f, 0.85f), 0.3f, 0.95f);
            Destroy(water.GetComponent<Collider>());

            // a flamingo float
            var fl = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            fl.name = "Flamingo Float";
            fl.transform.SetParent(poolRoot.transform, false);
            fl.transform.localPosition = new Vector3(-2f, 0.3f, -1f);
            fl.transform.localScale = new Vector3(0.8f, 0.4f, 1.3f);
            Destroy(fl.GetComponent<Collider>());
            fl.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(1f, 0.4f, 0.7f), 0.05f, 0.7f);
            // flamingo neck
            var neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neck.transform.SetParent(fl.transform, false);
            neck.transform.localPosition = new Vector3(0f, 0.6f, 1.0f);
            neck.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
            Destroy(neck.GetComponent<Collider>());
            neck.GetComponent<Renderer>().sharedMaterial = fl.GetComponent<Renderer>().sharedMaterial;
        }

        void BuildBar()
        {
            var bar = new GameObject("Tiki Bar");
            bar.transform.position = new Vector3(10f, 0f, -7f);

            var counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "Bar Counter";
            counter.transform.SetParent(bar.transform, false);
            counter.transform.localScale = new Vector3(5f, 1f, 1.3f);
            counter.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            counter.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.45f, 0.3f, 0.18f), 0f, 0.35f);

            // bamboo posts
            for (int i = -1; i <= 1; i += 2)
            {
                var p = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                p.transform.SetParent(bar.transform, false);
                p.transform.localPosition = new Vector3(2.4f * i, 1.5f, 0.4f);
                p.transform.localScale = new Vector3(0.15f, 1.5f, 0.15f);
                p.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.7f, 0.55f, 0.3f), 0f, 0.4f);
            }
            // thatch roof
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.transform.SetParent(bar.transform, false);
            roof.transform.localScale = new Vector3(5.5f, 0.2f, 1.8f);
            roof.transform.localPosition = new Vector3(0f, 3f, 0.4f);
            roof.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            roof.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.55f, 0.4f, 0.2f), 0f, 0.3f);

            // bottles on counter
            for (int i = -2; i <= 2; i++)
            {
                var b = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                b.name = "Bottle";
                b.transform.SetParent(bar.transform, false);
                b.transform.localPosition = new Vector3(i * 0.7f, 1.15f, -0.2f);
                b.transform.localScale = new Vector3(0.12f, 0.2f, 0.12f);
                Destroy(b.GetComponent<Collider>());
                var col = i % 2 == 0 ? new Color(0.3f, 0.6f, 0.3f) : new Color(0.7f, 0.4f, 0.2f);
                b.GetComponent<Renderer>().sharedMaterial = MakeMat(col, 0.4f, 0.9f);
            }

            // bartender NPC
            var bt = new GameObject("Bartender");
            bt.transform.position = bar.transform.position + new Vector3(0f, 0f, 0.9f);
            var c = bt.AddComponent<Civilian>();
            c.mode = Civilian.Mode.Idle;
            c.shirtColor = new Color(0.95f, 0.95f, 0.95f);
            c.pantsColor = new Color(0.1f, 0.1f, 0.15f);
        }

        void BuildDeckChairs()
        {
            // a row of deck chairs near the pool with sunbathers
            for (int i = 0; i < 4; i++)
            {
                Vector3 pos = new Vector3(-3.5f + i * 1.8f, 0f, 3.5f);
                var chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chair.name = "Deck Chair";
                chair.transform.position = pos + Vector3.up * 0.12f;
                chair.transform.localScale = new Vector3(0.8f, 0.1f, 1.8f);
                chair.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.5f, 0.55f), 0f, 0.6f);

                // sunbather
                var sb = new GameObject("Sunbather");
                sb.transform.position = pos + Vector3.up * 0.25f;
                var c = sb.AddComponent<Civilian>();
                c.mode = Civilian.Mode.Sunbathe;
                c.shirtColor = new Color(Random.value, Random.value, Random.value, 1f);
                c.pantsColor = new Color(Random.value, Random.value, Random.value, 1f);
            }

            // umbrellas
            for (int i = 0; i < 4; i++)
            {
                Vector3 pos = new Vector3(-3.5f + i * 1.8f, 0f, 4.6f);
                var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.transform.position = pos + Vector3.up * 1.3f;
                pole.transform.localScale = new Vector3(0.08f, 1.3f, 0.08f);
                pole.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.4f, 0.3f, 0.2f), 0f, 0.4f);

                var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopy.transform.position = pos + Vector3.up * 2.4f;
                canopy.transform.localScale = new Vector3(2.4f, 0.25f, 2.4f);
                Destroy(canopy.GetComponent<Collider>());
                var color = i % 2 == 0 ? new Color(0.9f, 0.4f, 0.4f) : new Color(0.9f, 0.85f, 0.4f);
                canopy.GetComponent<Renderer>().sharedMaterial = MakeMat(color, 0.05f, 0.6f);
            }
        }

        void BuildNpcs()
        {
            // wandering walkers
            for (int i = 0; i < 6; i++)
            {
                var npc = new GameObject("Walker " + i);
                npc.transform.position = new Vector3(Random.Range(-12f, 12f), 0f, Random.Range(-8f, 8f));
                var c = npc.AddComponent<Civilian>();
                c.mode = Civilian.Mode.Wander;
                c.shirtColor = RandomTropicalColor();
                c.pantsColor = RandomTropicalColor();
                c.patrolRadius = 8f;
                c.walkSpeed = 1.2f + Random.value * 0.8f;
            }

            // a couple of "security guards" in matching colors
            for (int i = -1; i <= 1; i += 2)
            {
                var npc = new GameObject("Security");
                npc.transform.position = new Vector3(15f * i, 0f, -3f);
                var c = npc.AddComponent<Civilian>();
                c.mode = Civilian.Mode.Wander;
                c.shirtColor = new Color(0.1f, 0.1f, 0.15f);
                c.pantsColor = new Color(0.05f, 0.05f, 0.08f);
                c.patrolRadius = 4f;
                c.walkSpeed = 0.9f;
            }
        }

        void BuildViolationSpawner()
        {
            var go = new GameObject("[Violation Spawner]");
            go.AddComponent<ViolationSpawner>();
        }

        Color RandomTropicalColor()
        {
            Color[] palette = {
                new Color(1f, 0.6f, 0.3f),
                new Color(0.95f, 0.4f, 0.6f),
                new Color(0.4f, 0.8f, 0.95f),
                new Color(0.8f, 0.95f, 0.4f),
                new Color(0.95f, 0.95f, 0.5f),
                new Color(0.6f, 0.4f, 0.85f),
            };
            return palette[Random.Range(0, palette.Length)];
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
