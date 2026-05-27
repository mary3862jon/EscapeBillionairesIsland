using UnityEngine;

namespace Spoonacci
{
    // BIG island demo — sandbox phase. Beach + pool + bar + Salon + Skin Kiosk +
    // Shady Alley + Billionaire + 30+ NPCs + violators + objectives + police mode.
    public class IslandBootstrapper : MonoBehaviour
    {
        public bool spawnSalon = true;
        public bool spawnSandbox = true;

        GameObject spoon;
        HudText hud;
        ObjectiveTracker tracker;

        void Awake()
        {
            SoundFx.Instance.ToString(); // force init
            BuildGround();
            BuildOcean();
            BuildSpoon();
            WireCamera();
            BuildHud();
            BuildObjectives();
            if (spawnSalon)
            {
                BuildSalon();
                BuildSkinKiosk();
            }
            if (spawnSandbox)
            {
                BuildPalmRing();
                BuildBigPool();
                BuildBar();
                BuildDeckChairs();
                BuildShadyAlley();
                BuildBillionaireZone();
                BuildCrowd();
                BuildViolationSpawner();
            }
        }

        void BuildGround()
        {
            // huge sand
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground (Beach)";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(20f, 1f, 20f); // 200x200
            ground.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.86f, 0.62f), 0f, 0.18f);
        }

        void BuildOcean()
        {
            var ocean = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ocean.name = "Ocean";
            ocean.transform.position = new Vector3(0f, -0.18f, 0f);
            ocean.transform.localScale = new Vector3(120f, 1f, 120f);
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
            spoon.AddComponent<PoliceMode>();
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
            cam.fieldOfView = 70f;
            var follower = cam.GetComponent<ThirdPersonCamera>() ?? cam.gameObject.AddComponent<ThirdPersonCamera>();
            follower.target = spoon.transform;
            follower.distance = 9f;
            follower.height = 5.2f;
        }

        void BuildHud()
        {
            var hudGo = new GameObject("HUD");
            hud = hudGo.AddComponent<HudText>();
            hud.Set("ACT 4 — Open Island. Spank, bonk, and humiliate the rich. Press F to auto-bonk violators.");
            Invoke(nameof(ClearHud), 9f);
        }

        void ClearHud() { if (hud != null) hud.Set(""); }

        void BuildObjectives()
        {
            var trGo = new GameObject("[ObjectiveTracker]");
            tracker = trGo.AddComponent<ObjectiveTracker>();
            // tracker reads active missions from MissionManager based on scene name (matches "Sample")
        }

        void BuildSalon()
        {
            var salon = new GameObject("Salon Cucchiaio");
            salon.transform.position = new Vector3(9f, 0f, 7f);

            // building
            var bld = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bld.name = "Salon Building";
            bld.transform.SetParent(salon.transform, false);
            bld.transform.localScale = new Vector3(5f, 3.2f, 3.6f);
            bld.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            bld.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(1.0f, 0.65f, 0.8f), 0.05f, 0.5f);

            // pink roof
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.transform.SetParent(salon.transform, false);
            roof.transform.localScale = new Vector3(5.5f, 0.25f, 4f);
            roof.transform.localPosition = new Vector3(0f, 3.3f, 0f);
            roof.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.92f, 0.45f, 0.7f), 0.3f, 0.6f);

            // big sign panel
            var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.transform.SetParent(salon.transform, false);
            sign.transform.localScale = new Vector3(4.6f, 0.9f, 0.1f);
            sign.transform.localPosition = new Vector3(0f, 4.1f, -1.85f);
            sign.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.98f, 0.96f, 0.92f), 0.1f, 0.4f);
            var signLabel = sign.AddComponent<WorldLabel>();
            signLabel.text = "✨ SALON  CUCCHIAIO ✨";
            signLabel.color = new Color(0.6f, 0.15f, 0.45f);
            signLabel.fontSize = 36;
            signLabel.worldOffset = new Vector3(0f, 0f, -0.1f);

            // FRONT COUNTER
            var counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.transform.SetParent(salon.transform, false);
            counter.transform.localScale = new Vector3(4f, 1.1f, 0.8f);
            counter.transform.localPosition = new Vector3(0f, 0.55f, -2.1f);
            counter.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.88f, 0.78f), 0.0f, 0.4f);

            // gold trim
            var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.transform.SetParent(salon.transform, false);
            trim.transform.localScale = new Vector3(4.1f, 0.08f, 0.82f);
            trim.transform.localPosition = new Vector3(0f, 1.1f, -2.1f);
            trim.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.78f, 0.25f), 1f, 0.9f);

            // ATTENDANT at counter (the salon Coiffeur spoon)
            var att = new GameObject("Coiffeur Cucchiaio");
            att.transform.position = salon.transform.position + new Vector3(0f, 0.6f, -1.8f);
            var attendant = att.AddComponent<NpcSpoon>();
            attendant.crewName = "Coiffeur Cucchiaio";
            attendant.color = new Color(1f, 0.65f, 0.8f);
            attendant.metallic = 0.6f;
            attendant.smoothness = 0.85f;
            salonAttendantTransform = att.transform;

            // RECEPTIONIST (human civilian) standing behind counter
            var recep = new GameObject("Receptionist");
            recep.transform.position = salon.transform.position + new Vector3(1.5f, 0f, -1.7f);
            var rc = recep.AddComponent<Civilian>();
            rc.mode = Civilian.Mode.Idle;
            rc.shirtColor = new Color(0.95f, 0.55f, 0.75f);
            rc.pantsColor = new Color(0.4f, 0.1f, 0.3f);

            // flag posts
            for (int i = -1; i <= 1; i += 2)
            {
                var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.transform.SetParent(salon.transform, false);
                pole.transform.localPosition = new Vector3(2.8f * i, 1.8f, -2.4f);
                pole.transform.localScale = new Vector3(0.12f, 1.8f, 0.12f);
                pole.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.5f, 0.75f), 0.6f, 0.5f);

                var flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
                flag.transform.SetParent(salon.transform, false);
                flag.transform.localPosition = new Vector3(2.8f * i + i * 0.4f, 3f, -2.4f);
                flag.transform.localScale = new Vector3(0.7f, 0.5f, 0.02f);
                flag.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(1f, 0.4f, 0.7f), 0.1f, 0.6f);
            }

            // interactable trigger zone
            var interactGo = new GameObject("Salon Trigger");
            interactGo.transform.SetParent(salon.transform, false);
            interactGo.transform.localPosition = new Vector3(0f, 0.6f, -3.5f);
            var sc = interactGo.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 3f;
            var interact = interactGo.AddComponent<SalonCucchiaio>();
            interact.prompt = hud;
            interact.frontSignText = "SALON  CUCCHIAIO";
            interact.attendantTransform = salonAttendantTransform;
        }

        Transform salonAttendantTransform;

        void BuildSkinKiosk()
        {
            // big display board next to the Salon
            var kioskGo = new GameObject("Skin Kiosk");
            kioskGo.transform.position = new Vector3(14f, 0f, 6f);
            kioskGo.transform.rotation = Quaternion.Euler(0f, -25f, 0f);
            kioskGo.AddComponent<SkinKiosk>();
        }

        void BuildPalmRing()
        {
            for (int i = 0; i < 24; i++)
            {
                float ang = (i / 24f) * Mathf.PI * 2f;
                float r = 35f + Random.Range(-4f, 6f);
                var pos = new Vector3(Mathf.Cos(ang) * r, 0f, Mathf.Sin(ang) * r);
                BuildPalm(pos);
            }
            for (int i = 0; i < 14; i++)
                BuildPalm(new Vector3(Random.Range(-20f, 20f), 0f, Random.Range(-20f, 20f)));
        }

        void BuildPalm(Vector3 pos)
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.position = pos + Vector3.up * 2.2f;
            trunk.transform.localScale = new Vector3(0.32f, 2.2f, 0.32f);
            trunk.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.4f, 0.26f, 0.16f), 0f, 0.3f);
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90f * Mathf.Deg2Rad;
                var leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leaf.transform.position = pos + Vector3.up * 4.3f + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 1.1f;
                leaf.transform.localScale = new Vector3(1.6f, 0.35f, 0.7f);
                leaf.transform.rotation = Quaternion.Euler(0f, i * 90f, -18f);
                Destroy(leaf.GetComponent<Collider>());
                leaf.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.18f, 0.55f, 0.22f), 0f, 0.3f);
            }
        }

        // ---- Pool zone with swimmers in modest swimwear ----
        void BuildBigPool()
        {
            var pool = new GameObject("Pool");
            pool.transform.position = new Vector3(-12f, 0f, -2f);

            var deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.transform.SetParent(pool.transform, false);
            deck.transform.localScale = new Vector3(14f, 0.1f, 10f);
            deck.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            deck.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.97f, 0.93f, 0.85f), 0f, 0.4f);

            var water = GameObject.CreatePrimitive(PrimitiveType.Cube);
            water.transform.SetParent(pool.transform, false);
            water.transform.localScale = new Vector3(10f, 0.15f, 7f);
            water.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            water.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.22f, 0.7f, 0.88f), 0.3f, 0.95f);
            Destroy(water.GetComponent<Collider>());

            // pool label
            var lblGo = new GameObject("PoolLabel");
            lblGo.transform.SetParent(pool.transform, false);
            lblGo.transform.localPosition = new Vector3(0f, 3.5f, 4.5f);
            var lbl = lblGo.AddComponent<WorldLabel>();
            lbl.text = "🏊 INFINITY POOL";
            lbl.color = new Color(0.1f, 0.4f, 0.7f);
            lbl.fontSize = 24;

            // 3 swimmers + 3 poolside loungers — modest swimwear (one-piece colors)
            for (int i = 0; i < 3; i++)
            {
                var pos = pool.transform.position + new Vector3(-3f + i * 3f, 0.3f, 0f);
                var sw = new GameObject("Swimmer " + i);
                sw.transform.position = pos;
                var c = sw.AddComponent<Civilian>();
                c.mode = Civilian.Mode.Idle;
                c.shirtColor = new Color(Random.value, Random.value, Random.value);
                c.pantsColor = c.shirtColor; // one-piece feel
                c.skinColor = new Color(0.96f, 0.82f, 0.7f);
            }

            for (int i = 0; i < 3; i++)
            {
                var pos = pool.transform.position + new Vector3(-3f + i * 3f, 0f, 4.2f);
                // lounger
                var chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chair.transform.position = pos + Vector3.up * 0.12f;
                chair.transform.localScale = new Vector3(0.9f, 0.1f, 2f);
                chair.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.5f, 0.55f), 0f, 0.6f);
                // person
                var p = new GameObject("Lounger " + i);
                p.transform.position = pos + Vector3.up * 0.3f;
                var c = p.AddComponent<Civilian>();
                c.mode = Civilian.Mode.Sunbathe;
                c.shirtColor = new Color(Random.value, Random.value, Random.value);
                c.pantsColor = new Color(Random.value, Random.value, Random.value);
            }

            // a couple of plastic palm trees framing the pool
            BuildPalm(pool.transform.position + new Vector3(-7f, 0f, -4f));
            BuildPalm(pool.transform.position + new Vector3(7f, 0f, -4f));
        }

        void BuildBar()
        {
            var bar = new GameObject("Tiki Bar");
            bar.transform.position = new Vector3(15f, 0f, -10f);

            var counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.transform.SetParent(bar.transform, false);
            counter.transform.localScale = new Vector3(6f, 1.1f, 1.5f);
            counter.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            counter.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.45f, 0.3f, 0.18f), 0f, 0.35f);

            for (int i = -1; i <= 1; i += 2)
            {
                var p = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                p.transform.SetParent(bar.transform, false);
                p.transform.localPosition = new Vector3(2.9f * i, 1.6f, 0.4f);
                p.transform.localScale = new Vector3(0.15f, 1.6f, 0.15f);
                p.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.7f, 0.55f, 0.3f), 0f, 0.4f);
            }

            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.transform.SetParent(bar.transform, false);
            roof.transform.localScale = new Vector3(6.5f, 0.2f, 2.0f);
            roof.transform.localPosition = new Vector3(0f, 3.3f, 0.4f);
            roof.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            roof.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.55f, 0.4f, 0.2f), 0f, 0.3f);

            var lblGo = new GameObject("BarLabel");
            lblGo.transform.SetParent(bar.transform, false);
            lblGo.transform.localPosition = new Vector3(0f, 3.5f, 0f);
            var lbl = lblGo.AddComponent<WorldLabel>();
            lbl.text = "🍹 TIKI BAR";
            lbl.color = new Color(0.55f, 0.3f, 0.05f);
            lbl.fontSize = 22;

            for (int i = -3; i <= 3; i++)
            {
                var b = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                b.transform.SetParent(bar.transform, false);
                b.transform.localPosition = new Vector3(i * 0.65f, 1.3f, -0.2f);
                b.transform.localScale = new Vector3(0.13f, 0.22f, 0.13f);
                Destroy(b.GetComponent<Collider>());
                var col = i % 2 == 0 ? new Color(0.3f, 0.6f, 0.3f) : new Color(0.7f, 0.4f, 0.2f);
                b.GetComponent<Renderer>().sharedMaterial = MakeMat(col, 0.4f, 0.9f);
            }

            // bartender
            var bt = new GameObject("Bartender");
            bt.transform.position = bar.transform.position + new Vector3(0f, 0f, 0.9f);
            var c = bt.AddComponent<Civilian>();
            c.mode = Civilian.Mode.Idle;
            c.shirtColor = new Color(0.95f, 0.95f, 0.95f);
            c.pantsColor = new Color(0.1f, 0.1f, 0.15f);

            // 4 drunk guests sitting around
            for (int i = -2; i <= 2; i += 2)
            {
                var g = new GameObject("Guest");
                g.transform.position = bar.transform.position + new Vector3(i * 0.9f, 0f, -1.8f);
                var cc = g.AddComponent<Civilian>();
                cc.mode = Civilian.Mode.Idle;
                cc.shirtColor = RandomTropicalColor();
                cc.pantsColor = RandomTropicalColor();
            }
        }

        void BuildDeckChairs()
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = new Vector3(-4f + i * 2f, 0f, 9f);
                var chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chair.transform.position = pos + Vector3.up * 0.12f;
                chair.transform.localScale = new Vector3(0.8f, 0.1f, 1.8f);
                chair.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.5f, 0.55f), 0f, 0.6f);

                var sb = new GameObject("Sunbather");
                sb.transform.position = pos + Vector3.up * 0.25f;
                var c = sb.AddComponent<Civilian>();
                c.mode = Civilian.Mode.Sunbathe;
                c.shirtColor = new Color(Random.value, Random.value, Random.value, 1f);
                c.pantsColor = new Color(Random.value, Random.value, Random.value, 1f);

                // umbrella
                var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.transform.position = pos + new Vector3(0f, 1.3f, 0.8f);
                pole.transform.localScale = new Vector3(0.08f, 1.3f, 0.08f);
                pole.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.4f, 0.3f, 0.2f), 0f, 0.4f);
                var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopy.transform.position = pos + new Vector3(0f, 2.4f, 0.8f);
                canopy.transform.localScale = new Vector3(2.4f, 0.25f, 2.4f);
                Destroy(canopy.GetComponent<Collider>());
                var col = i % 2 == 0 ? new Color(0.9f, 0.4f, 0.4f) : new Color(0.9f, 0.85f, 0.4f);
                canopy.GetComponent<Renderer>().sharedMaterial = MakeMat(col, 0.05f, 0.6f);
            }
        }

        // ---- Shady alley with drug dealers ----
        void BuildShadyAlley()
        {
            var alley = new GameObject("Shady Alley");
            alley.transform.position = new Vector3(-16f, 0f, 14f);

            // two warehouse walls forming alley
            for (int side = -1; side <= 1; side += 2)
            {
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.transform.SetParent(alley.transform, false);
                wall.transform.localPosition = new Vector3(side * 3f, 2.5f, 0f);
                wall.transform.localScale = new Vector3(0.4f, 5f, 12f);
                wall.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.25f, 0.22f, 0.2f), 0f, 0.2f);
            }
            // ground patch (darker)
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(alley.transform, false);
            floor.transform.localPosition = new Vector3(0f, 0.06f, 0f);
            floor.transform.localScale = new Vector3(6f, 0.12f, 12f);
            floor.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.18f, 0.18f, 0.2f), 0f, 0.15f);

            // a flickering low light
            var lampGo = new GameObject("Alley Lamp");
            lampGo.transform.SetParent(alley.transform, false);
            lampGo.transform.localPosition = new Vector3(0f, 4.5f, 0f);
            var lamp = lampGo.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = new Color(0.7f, 0.5f, 0.35f);
            lamp.intensity = 3f;
            lamp.range = 12f;
            lampGo.AddComponent<FlickerLight>();

            // sign
            var lblGo = new GameObject("AlleyLabel");
            lblGo.transform.SetParent(alley.transform, false);
            lblGo.transform.localPosition = new Vector3(0f, 5.5f, -5.5f);
            var lbl = lblGo.AddComponent<WorldLabel>();
            lbl.text = "⚠ SHADY ALLEY — police mode (P) to bust dealers";
            lbl.color = new Color(1f, 0.4f, 0.3f);
            lbl.fontSize = 22;

            // 3 drug dealers — civilians + DrugDealer marker
            for (int i = 0; i < 3; i++)
            {
                var d = new GameObject("Shady Character " + i);
                d.transform.position = alley.transform.position + new Vector3(Random.Range(-1.5f, 1.5f), 0f, -3f + i * 3f);
                var c = d.AddComponent<Civilian>();
                c.mode = Civilian.Mode.Idle;
                c.shirtColor = new Color(0.2f, 0.2f, 0.25f);
                c.pantsColor = new Color(0.1f, 0.1f, 0.12f);
                c.skinColor = new Color(0.85f, 0.7f, 0.6f);
                d.AddComponent<DrugDealer>();
            }
        }

        void BuildBillionaireZone()
        {
            // a small mansion patio for Magnus Tusk
            var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = "Mansion Patio";
            pad.transform.position = new Vector3(-2f, 0.04f, -16f);
            pad.transform.localScale = new Vector3(10f, 0.1f, 8f);
            pad.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.92f, 0.92f, 0.95f), 0.05f, 0.5f);

            // gold pillars
            for (int side = -1; side <= 1; side += 2)
            {
                var col = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                col.transform.position = new Vector3(-2f + side * 4f, 2.4f, -19f);
                col.transform.localScale = new Vector3(0.4f, 2.4f, 0.4f);
                col.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.85f, 0.4f), 1f, 0.9f);
            }

            // Magnus Tusk himself
            var mt = new GameObject("Magnus Tusk");
            mt.transform.position = new Vector3(-2f, 0f, -17f);
            var bn = mt.AddComponent<BillionaireNPC>();
            bn.billionaireName = "Magnus Tusk";
            bn.suitColor = new Color(0.05f, 0.08f, 0.18f);

            // sign
            var lblGo = new GameObject("MansionLabel");
            lblGo.transform.SetParent(pad.transform, false);
            lblGo.transform.localPosition = new Vector3(0f, 60f, -50f);
            var lbl = lblGo.AddComponent<WorldLabel>();
            lbl.text = "💰 BILLIONAIRE MANSION";
            lbl.color = new Color(0.95f, 0.8f, 0.2f);
            lbl.fontSize = 22;
        }

        void BuildCrowd()
        {
            // 18 wanderers across the world for population
            for (int i = 0; i < 18; i++)
            {
                var npc = new GameObject("Walker " + i);
                npc.transform.position = new Vector3(Random.Range(-22f, 22f), 0f, Random.Range(-18f, 18f));
                var c = npc.AddComponent<Civilian>();
                c.mode = Civilian.Mode.Wander;
                c.shirtColor = RandomTropicalColor();
                c.pantsColor = RandomTropicalColor();
                c.patrolRadius = 10f;
                c.walkSpeed = 1.0f + Random.value * 1.0f;
            }
            // security
            for (int side = -1; side <= 1; side += 2)
            for (int i = 0; i < 2; i++)
            {
                var npc = new GameObject("Security");
                npc.transform.position = new Vector3(20f * side, 0f, -3f + i * 4f);
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
            var vs = go.AddComponent<ViolationSpawner>();
            vs.interval = 5f;
            vs.maxConcurrent = 6;
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

    // tiny helper for the alley lamp
    public class FlickerLight : MonoBehaviour
    {
        Light lt; float next; float saved;
        void Awake() { lt = GetComponent<Light>(); if (lt != null) saved = lt.intensity; }
        void Update()
        {
            if (lt == null) return;
            if (Time.time > next) { next = Time.time + Random.Range(0.05f, 0.4f); lt.intensity = saved * (Random.value > 0.15f ? 1f : 0.2f); }
        }
    }
}
