using UnityEngine;

namespace Spoonacci
{
    // 250x250 island sandbox. Four billionaire zones spread across compass corners.
    // Central hub: Salon + Skin Kiosk + Pool + Bar + Shady Alley. ~40 NPCs total + violator spawner.
    public class IslandBootstrapper : MonoBehaviour
    {
        const float ISLE = 250f; // edge length
        const float ZONE_R = 80f;  // distance from center for each corner zone

        GameObject spoon;
        HudText hud;
        ObjectiveTracker tracker;
        Transform salonAttendantTransform;

        void Awake()
        {
            // singletons
            _ = SoundFx.Instance;
            _ = SaveSystem.Instance;
            _ = MusicPlayer.Instance;
            MissionManager.BootstrapDefaults();

            gameObject.AddComponent<PostFxBoost>();
            gameObject.AddComponent<PauseMenu>();
            gameObject.AddComponent<QuestBanner>();
            gameObject.AddComponent<LanguageToggle>();
            BuildGround();
            BuildOcean();
            BuildSky();
            gameObject.AddComponent<DayNightCycle>();
            BuildSpoon();
            WireCamera();
            BuildHud();
            BuildObjectives();
            BuildCentralHub();
            BuildBeffYacht();
            BuildCryptoVault();
            BuildZuckLab();
            BuildMagnusMansion();
            BuildShadyAlley();
            BuildPalmEdges();
            BuildCrowd();
            BuildViolationSpawner();
        }

        // ---------- WORLD ----------
        void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Beach";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(ISLE / 10f, 1f, ISLE / 10f);
            ground.GetComponent<Renderer>().sharedMaterial = ShaderCache.MakeTextured(
                ProceduralTextures.Sand, new Color(0.98f, 0.94f, 0.78f), 0f, 0.18f, new Vector2(40f, 40f));
        }

        void BuildOcean()
        {
            var ocean = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ocean.name = "Ocean";
            ocean.transform.position = new Vector3(0f, -0.18f, 0f);
            ocean.transform.localScale = new Vector3(80f, 1f, 80f);
            ocean.GetComponent<Renderer>().sharedMaterial = ShaderCache.MakeTextured(
                ProceduralTextures.Water, new Color(0.85f, 0.95f, 1f), 0.2f, 0.95f, new Vector2(30f, 30f));
            Destroy(ocean.GetComponent<Collider>());
        }

        void BuildSky()
        {
            // a giant low-hemisphere "skydome" with a soft gradient cube around the player area
            // (Unity's default skybox already handles this — just bump ambient)
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.ambientLight = new Color(0.6f, 0.65f, 0.75f);

            // distant cloud spheres for parallax flavor
            for (int i = 0; i < 30; i++)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                c.name = "Cloud";
                float ang = Random.value * Mathf.PI * 2f;
                float r = 180f + Random.value * 60f;
                c.transform.position = new Vector3(Mathf.Cos(ang) * r, 35f + Random.value * 15f, Mathf.Sin(ang) * r);
                c.transform.localScale = new Vector3(20f + Random.value * 20f, 6f, 14f + Random.value * 10f);
                Destroy(c.GetComponent<Collider>());
                c.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.97f, 0.97f, 1f), 0.05f, 0.6f);
            }
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
            cam.fieldOfView = 65f;
            cam.farClipPlane = 600f;
            var follower = cam.GetComponent<ThirdPersonCamera>() ?? cam.gameObject.AddComponent<ThirdPersonCamera>();
            follower.target = spoon.transform;
            follower.distance = 5.5f;
            follower.height = 1.8f;
        }

        void BuildHud()
        {
            var hudGo = new GameObject("HUD");
            hud = hudGo.AddComponent<HudText>();
            hud.Set("THE ISLE OF LASCIVIOUS REPOSE — 4 billionaire zones across the compass. Find them all. F to bonk violators, Q manual swing.");
            Invoke(nameof(ClearHud), 12f);
        }

        void ClearHud() { if (hud != null) hud.Set(""); }

        void BuildObjectives()
        {
            var trGo = new GameObject("[ObjectiveTracker]");
            tracker = trGo.AddComponent<ObjectiveTracker>();
        }

        // ---------- CENTRAL HUB ----------
        void BuildCentralHub()
        {
            BuildSalon();
            BuildSkinKiosk();
            BuildPool();
            BuildBar();
            BuildDeckChairs();
        }

        void BuildSalon()
        {
            var salon = new GameObject("Salon Cucchiaio");
            salon.transform.position = new Vector3(12f, 0f, 8f);
            // building
            var bld = MakePrimCube("Salon Body", salon.transform, new Vector3(0f, 1.6f, 0f), new Vector3(5f, 3.2f, 3.6f), new Color(1.0f, 0.65f, 0.8f), 0.05f, 0.5f);
            // roof
            MakePrimCube("Roof", salon.transform, new Vector3(0f, 3.3f, 0f), new Vector3(5.5f, 0.25f, 4f), new Color(0.92f, 0.45f, 0.7f), 0.3f, 0.6f);
            // sign
            var sign = MakePrimCube("Sign", salon.transform, new Vector3(0f, 4.1f, -1.85f), new Vector3(4.6f, 0.9f, 0.1f), new Color(0.98f, 0.96f, 0.92f), 0.1f, 0.4f);
            AddLabel(sign, "✨ SALON  CUCCHIAIO ✨", new Color(0.6f, 0.15f, 0.45f), 36, new Vector3(0f, 0f, -0.1f));
            // counter
            MakePrimCube("Counter", salon.transform, new Vector3(0f, 0.55f, -2.1f), new Vector3(4f, 1.1f, 0.8f), new Color(0.95f, 0.88f, 0.78f), 0f, 0.4f);
            MakePrimCube("Trim",    salon.transform, new Vector3(0f, 1.1f, -2.1f), new Vector3(4.1f, 0.08f, 0.82f), new Color(0.95f, 0.78f, 0.25f), 1f, 0.9f);

            // attendant spoon
            var att = new GameObject("Coiffeur Cucchiaio");
            att.transform.position = salon.transform.position + new Vector3(0f, 0.6f, -1.8f);
            var a = att.AddComponent<NpcSpoon>();
            a.crewName = "Coiffeur Cucchiaio"; a.color = new Color(1f, 0.65f, 0.8f); a.metallic = 0.6f; a.smoothness = 0.85f;
            salonAttendantTransform = att.transform;

            // human receptionist behind counter
            var recep = new GameObject("Receptionist");
            recep.transform.position = salon.transform.position + new Vector3(1.5f, 0f, -1.7f);
            var rc = recep.AddComponent<Civilian>();
            rc.mode = Civilian.Mode.Idle;
            rc.shirtColor = new Color(0.95f, 0.55f, 0.75f);
            rc.pantsColor = new Color(0.4f, 0.1f, 0.3f);

            // flag posts
            for (int i = -1; i <= 1; i += 2)
            {
                MakePrimCylinder("Pole", salon.transform, new Vector3(2.8f * i, 1.8f, -2.4f), new Vector3(0.12f, 1.8f, 0.12f), new Color(0.95f, 0.5f, 0.75f), 0.6f, 0.5f);
                MakePrimCube("Flag", salon.transform, new Vector3(2.8f * i + i * 0.4f, 3f, -2.4f), new Vector3(0.7f, 0.5f, 0.02f), new Color(1f, 0.4f, 0.7f), 0.1f, 0.6f);
            }

            // interactable trigger zone
            var trig = new GameObject("Salon Trigger");
            trig.transform.SetParent(salon.transform, false);
            trig.transform.localPosition = new Vector3(0f, 0.6f, -3.5f);
            var sc = trig.AddComponent<SphereCollider>();
            sc.isTrigger = true; sc.radius = 3f;
            var interact = trig.AddComponent<SalonCucchiaio>();
            interact.prompt = hud;
            interact.attendantTransform = salonAttendantTransform;
        }

        void BuildSkinKiosk()
        {
            var k = new GameObject("Skin Kiosk");
            k.transform.position = new Vector3(20f, 0f, 6f);
            k.transform.rotation = Quaternion.Euler(0f, -25f, 0f);
            k.AddComponent<SkinKiosk>();
        }

        void BuildPool()
        {
            var pool = new GameObject("Pool");
            pool.transform.position = new Vector3(-15f, 0f, -2f);
            MakePrimCube("Deck", pool.transform, new Vector3(0f, 0.05f, 0f), new Vector3(16f, 0.1f, 12f), new Color(0.97f, 0.93f, 0.85f), 0f, 0.4f);
            var water = MakePrimCube("Water", pool.transform, new Vector3(0f, 0.12f, 0f), new Vector3(12f, 0.15f, 8f), new Color(0.22f, 0.7f, 0.88f), 0.3f, 0.95f);
            Destroy(water.GetComponent<Collider>());
            // label
            var lbl = new GameObject("PoolLabel");
            lbl.transform.SetParent(pool.transform, false);
            lbl.transform.localPosition = new Vector3(0f, 4f, 6f);
            var l = lbl.AddComponent<WorldLabel>();
            l.text = "🏊 INFINITY POOL"; l.color = new Color(0.1f, 0.4f, 0.7f); l.fontSize = 28;
            // 3 swimmers + 3 loungers
            for (int i = 0; i < 3; i++)
            {
                var sw = SpawnCiv("Swimmer " + i, pool.transform.position + new Vector3(-3f + i * 3f, 0.3f, 0f), Civilian.Mode.Idle);
                sw.shirtColor = new Color(Random.value, Random.value, Random.value); sw.pantsColor = sw.shirtColor;
                var lng = SpawnCiv("Lounger " + i, pool.transform.position + new Vector3(-3f + i * 3f, 0.3f, 5f), Civilian.Mode.Sunbathe);
                lng.shirtColor = new Color(Random.value, Random.value, Random.value); lng.pantsColor = new Color(Random.value, Random.value, Random.value);
                MakePrimCube("Lounger Chair", pool.transform, new Vector3(-3f + i * 3f, 0.12f, 5f), new Vector3(0.9f, 0.1f, 2f), new Color(0.95f, 0.5f, 0.55f), 0f, 0.6f);
            }
        }

        void BuildBar()
        {
            var bar = new GameObject("Tiki Bar");
            bar.transform.position = new Vector3(22f, 0f, -10f);
            MakePrimCube("Counter", bar.transform, new Vector3(0f, 0.55f, 0f), new Vector3(6f, 1.1f, 1.5f), new Color(0.45f, 0.3f, 0.18f), 0f, 0.35f);
            for (int i = -1; i <= 1; i += 2)
                MakePrimCylinder("Pole", bar.transform, new Vector3(2.9f * i, 1.6f, 0.4f), new Vector3(0.15f, 1.6f, 0.15f), new Color(0.7f, 0.55f, 0.3f), 0f, 0.4f);
            var roof = MakePrimCube("Roof", bar.transform, new Vector3(0f, 3.3f, 0.4f), new Vector3(6.5f, 0.2f, 2.0f), new Color(0.55f, 0.4f, 0.2f), 0f, 0.3f);
            roof.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            var lbl = new GameObject("BarLabel"); lbl.transform.SetParent(bar.transform, false); lbl.transform.localPosition = new Vector3(0f, 4f, 0f);
            var l = lbl.AddComponent<WorldLabel>(); l.text = "🍹 TIKI BAR"; l.color = new Color(0.55f, 0.3f, 0.05f); l.fontSize = 26;
            // bottles
            for (int i = -3; i <= 3; i++)
                MakePrimCylinder("Bottle", bar.transform, new Vector3(i * 0.65f, 1.3f, -0.2f), new Vector3(0.13f, 0.22f, 0.13f), i % 2 == 0 ? new Color(0.3f, 0.6f, 0.3f) : new Color(0.7f, 0.4f, 0.2f), 0.4f, 0.9f);
            // bartender + guests
            SpawnCiv("Bartender", bar.transform.position + new Vector3(0f, 0f, 0.9f), Civilian.Mode.Idle, new Color(0.95f, 0.95f, 0.95f), new Color(0.1f, 0.1f, 0.15f));
            for (int i = -2; i <= 2; i += 2)
                SpawnCiv("Guest " + i, bar.transform.position + new Vector3(i * 0.9f, 0f, -1.8f), Civilian.Mode.Idle);
        }

        void BuildDeckChairs()
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3 pos = new Vector3(-5f + i * 2f, 0f, 14f);
                MakePrimCube("Deck Chair", null, pos + Vector3.up * 0.12f, new Vector3(0.8f, 0.1f, 1.8f), new Color(0.95f, 0.5f, 0.55f), 0f, 0.6f);
                SpawnCiv("Sunbather " + i, pos + Vector3.up * 0.25f, Civilian.Mode.Sunbathe);
                // umbrella
                MakePrimCylinder("Pole", null, pos + new Vector3(0f, 1.3f, 0.8f), new Vector3(0.08f, 1.3f, 0.08f), new Color(0.4f, 0.3f, 0.2f), 0f, 0.4f);
                var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopy.transform.position = pos + new Vector3(0f, 2.4f, 0.8f);
                canopy.transform.localScale = new Vector3(2.4f, 0.25f, 2.4f);
                Destroy(canopy.GetComponent<Collider>());
                canopy.GetComponent<Renderer>().sharedMaterial = MakeMat(i % 2 == 0 ? new Color(0.9f, 0.4f, 0.4f) : new Color(0.9f, 0.85f, 0.4f), 0.05f, 0.6f);
            }
        }

        // ---------- BILLIONAIRE ZONES ----------
        void BuildBeffYacht()
        {
            var z = new GameObject("Beff Jezos Yacht Zone");
            z.transform.position = new Vector3(-ZONE_R, 0f, -ZONE_R);
            AddLabel(z, "🛥  BEFF JEZOS YACHT", new Color(0.95f, 0.85f, 0.3f), 32, new Vector3(0f, 12f, 0f));

            // dock
            MakePrimCube("Dock", z.transform, new Vector3(0f, 0.1f, 8f), new Vector3(6f, 0.3f, 16f), new Color(0.5f, 0.35f, 0.18f), 0f, 0.35f);
            // yacht (giant capsule)
            var yacht = MakePrimCube("Hull", z.transform, new Vector3(0f, 1.2f, -14f), new Vector3(8f, 2f, 24f), new Color(0.95f, 0.95f, 1f), 0.05f, 0.6f);
            yacht.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            // upper deck
            MakePrimCube("Upper Deck", z.transform, new Vector3(0f, 3.5f, -18f), new Vector3(7f, 1.6f, 10f), new Color(0.95f, 0.95f, 1f), 0.05f, 0.6f);
            // bridge tower
            MakePrimCube("Bridge", z.transform, new Vector3(0f, 5.5f, -22f), new Vector3(4f, 2f, 4f), new Color(0.85f, 0.85f, 0.9f), 0.3f, 0.85f);
            // helipad ring
            for (int i = 0; i < 12; i++)
            {
                float a = i * 30f * Mathf.Deg2Rad;
                MakePrimCube("Helipad Trim", z.transform, new Vector3(Mathf.Cos(a) * 3f, 2.3f, -10f + Mathf.Sin(a) * 3f), new Vector3(0.25f, 0.1f, 0.25f), new Color(0.95f, 0.85f, 0.2f), 1f, 0.9f);
            }

            // Beff Jezos NPC
            var beff = new GameObject("Beff Jezos");
            beff.transform.position = z.transform.position + new Vector3(0f, 0f, -10f);
            var bn = beff.AddComponent<BillionaireNPC>();
            bn.billionaireName = "Beff Jezos";
            bn.suitColor = new Color(0.15f, 0.18f, 0.25f);

            // 4 yacht staff civilians
            for (int i = 0; i < 4; i++)
                SpawnCiv("Yacht Staff " + i, z.transform.position + new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-22f, -8f)), Civilian.Mode.Wander, new Color(0.95f, 0.95f, 0.95f), new Color(0.05f, 0.05f, 0.1f));
        }

        void BuildCryptoVault()
        {
            var z = new GameObject("Crypto Chad Vault Zone");
            z.transform.position = new Vector3(ZONE_R, 0f, -ZONE_R);
            AddLabel(z, "💎  CRYPTO CHAD VAULT", new Color(0.4f, 0.95f, 0.9f), 32, new Vector3(0f, 12f, 0f));

            // glass cube building (semi-transparent feel via white emission)
            var vault = MakePrimCube("Vault", z.transform, new Vector3(0f, 4f, 0f), new Vector3(10f, 8f, 10f), new Color(0.6f, 0.9f, 1f), 0.4f, 0.95f);
            // neon ring base
            MakePrimCube("Neon Base", z.transform, new Vector3(0f, 0.1f, 0f), new Vector3(12f, 0.2f, 12f), new Color(0.3f, 0.9f, 1f), 0.3f, 0.85f);
            // 4 NFT pillars
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90f * Mathf.Deg2Rad;
                MakePrimCube("NFT Pedestal", z.transform, new Vector3(Mathf.Cos(a) * 3.5f, 0.7f, Mathf.Sin(a) * 3.5f), new Vector3(0.8f, 1.4f, 0.8f), new Color(0.95f, 0.85f, 0.2f), 1f, 0.9f);
                MakePrimCube("NFT Sphere", z.transform, new Vector3(Mathf.Cos(a) * 3.5f, 1.8f, Mathf.Sin(a) * 3.5f), new Vector3(0.6f, 0.6f, 0.6f), new Color(0.4f, 0.95f, 0.85f), 0.95f, 0.95f);
            }
            // Chad
            var chad = new GameObject("Crypto Chad");
            chad.transform.position = z.transform.position + new Vector3(0f, 0f, -7f);
            var cn = chad.AddComponent<BillionaireNPC>();
            cn.billionaireName = "Crypto Chad";
            cn.suitColor = new Color(0.05f, 0.4f, 0.45f);

            for (int i = 0; i < 3; i++)
                SpawnCiv("Hodler " + i, z.transform.position + new Vector3(Random.Range(-6f, 6f), 0f, Random.Range(-6f, 6f)), Civilian.Mode.Wander, new Color(0.4f, 0.95f, 0.85f), new Color(0.1f, 0.1f, 0.15f));
        }

        void BuildZuckLab()
        {
            var z = new GameObject("Zuckersnort Lab Zone");
            z.transform.position = new Vector3(-ZONE_R, 0f, ZONE_R);
            AddLabel(z, "🧪  ZUCKERSNORT AI LAB", new Color(0.6f, 1f, 0.4f), 32, new Vector3(0f, 12f, 0f));

            // sterile white box
            MakePrimCube("Lab Building", z.transform, new Vector3(0f, 3f, 0f), new Vector3(14f, 6f, 10f), new Color(0.95f, 0.97f, 0.97f), 0.1f, 0.7f);
            // green roof (lizard tribute)
            MakePrimCube("Lab Roof", z.transform, new Vector3(0f, 6.1f, 0f), new Vector3(14.5f, 0.3f, 10.5f), new Color(0.2f, 0.7f, 0.4f), 0.3f, 0.6f);
            // big antenna
            MakePrimCylinder("Antenna", z.transform, new Vector3(0f, 9f, 0f), new Vector3(0.2f, 3f, 0.2f), new Color(0.4f, 0.4f, 0.45f), 0.9f, 0.5f);
            MakePrimCube("Antenna Dish", z.transform, new Vector3(0f, 11.5f, 0f), new Vector3(2.5f, 0.3f, 2.5f), new Color(0.85f, 0.85f, 0.9f), 0.3f, 0.7f);
            // glass front
            MakePrimCube("Glass Front", z.transform, new Vector3(0f, 2.5f, -5.1f), new Vector3(8f, 4f, 0.1f), new Color(0.5f, 0.85f, 1f), 0.5f, 0.95f);

            // Zuck
            var zuck = new GameObject("Mark Zuckersnort");
            zuck.transform.position = z.transform.position + new Vector3(0f, 0f, -7f);
            var zn = zuck.AddComponent<BillionaireNPC>();
            zn.billionaireName = "Mark Zuckersnort";
            zn.suitColor = new Color(0.4f, 0.4f, 0.45f); // hoodie

            // 4 interns wandering
            for (int i = 0; i < 4; i++)
                SpawnCiv("Intern " + i, z.transform.position + new Vector3(Random.Range(-6f, 6f), 0f, Random.Range(-6f, 6f)), Civilian.Mode.Wander, new Color(0.95f, 0.95f, 0.95f), new Color(0.2f, 0.2f, 0.25f));
        }

        void BuildMagnusMansion()
        {
            var z = new GameObject("Magnus Tusk Mansion Zone");
            z.transform.position = new Vector3(ZONE_R, 0f, ZONE_R);
            AddLabel(z, "💰  MAGNUS TUSK MANSION", new Color(0.95f, 0.8f, 0.2f), 32, new Vector3(0f, 12f, 0f));

            // marble patio
            MakePrimCube("Patio", z.transform, new Vector3(0f, 0.05f, 0f), new Vector3(20f, 0.2f, 14f), new Color(0.92f, 0.92f, 0.95f), 0.1f, 0.7f);
            // mansion body
            MakePrimCube("Mansion", z.transform, new Vector3(0f, 3f, 5f), new Vector3(18f, 6f, 8f), new Color(0.97f, 0.94f, 0.86f), 0.1f, 0.6f);
            // gold columns
            for (int side = -1; side <= 1; side += 2)
                for (int row = 0; row < 3; row++)
                {
                    MakePrimCylinder("Column", z.transform, new Vector3(side * 7f, 2.5f, -3f + row * 3f), new Vector3(0.5f, 2.5f, 0.5f), new Color(0.95f, 0.85f, 0.4f), 1f, 0.9f);
                }
            // little rocket on the lawn
            MakePrimCylinder("Rocket Body", z.transform, new Vector3(7f, 3f, -6f), new Vector3(0.7f, 3f, 0.7f), new Color(0.95f, 0.95f, 0.98f), 0.3f, 0.85f);
            var nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nose.transform.SetParent(z.transform, false);
            nose.transform.localPosition = new Vector3(7f, 6.2f, -6f);
            nose.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            Destroy(nose.GetComponent<Collider>());
            nose.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(1f, 0.3f, 0.3f), 0.4f, 0.85f);

            // Magnus
            var mt = new GameObject("Magnus Tusk");
            mt.transform.position = z.transform.position + new Vector3(-2f, 0f, -3f);
            var bn = mt.AddComponent<BillionaireNPC>();
            bn.billionaireName = "Magnus Tusk";
            bn.suitColor = new Color(0.05f, 0.08f, 0.18f);

            // mansion guards
            for (int i = 0; i < 4; i++)
                SpawnCiv("Mansion Guard " + i, z.transform.position + new Vector3(Random.Range(-9f, 9f), 0f, Random.Range(-6f, 4f)), Civilian.Mode.Wander, new Color(0.1f, 0.1f, 0.15f), new Color(0.05f, 0.05f, 0.08f));
        }

        // ---------- ALLEY / EVERYTHING ELSE ----------
        void BuildShadyAlley()
        {
            var alley = new GameObject("Shady Alley");
            alley.transform.position = new Vector3(-22f, 0f, 22f);
            // walls
            for (int side = -1; side <= 1; side += 2)
                MakePrimCube("Alley Wall", alley.transform, new Vector3(side * 3f, 2.5f, 0f), new Vector3(0.4f, 5f, 14f), new Color(0.25f, 0.22f, 0.2f), 0f, 0.2f);
            MakePrimCube("Alley Floor", alley.transform, new Vector3(0f, 0.06f, 0f), new Vector3(6f, 0.12f, 14f), new Color(0.18f, 0.18f, 0.2f), 0f, 0.15f);

            // lamp
            var lampGo = new GameObject("Alley Lamp");
            lampGo.transform.SetParent(alley.transform, false);
            lampGo.transform.localPosition = new Vector3(0f, 4.5f, 0f);
            var lamp = lampGo.AddComponent<Light>();
            lamp.type = LightType.Point; lamp.color = new Color(0.7f, 0.5f, 0.35f); lamp.intensity = 3.5f; lamp.range = 13f;
            lampGo.AddComponent<FlickerLight>();

            // big sign
            var lbl = new GameObject("AlleyLabel");
            lbl.transform.SetParent(alley.transform, false);
            lbl.transform.localPosition = new Vector3(0f, 6f, -6.5f);
            var l = lbl.AddComponent<WorldLabel>(); l.text = "⚠ SHADY ALLEY — Police Mode (P) to bust"; l.color = new Color(1f, 0.4f, 0.3f); l.fontSize = 24;

            // 3 drug dealers
            for (int i = 0; i < 3; i++)
            {
                var d = new GameObject("Shady Character " + i);
                d.transform.position = alley.transform.position + new Vector3(Random.Range(-1.5f, 1.5f), 0f, -3f + i * 3f);
                var c = d.AddComponent<Civilian>();
                c.mode = Civilian.Mode.Idle;
                c.shirtColor = new Color(0.2f, 0.2f, 0.25f); c.pantsColor = new Color(0.1f, 0.1f, 0.12f); c.skinColor = new Color(0.85f, 0.7f, 0.6f);
                d.AddComponent<DrugDealer>();
            }
        }

        void BuildPalmEdges()
        {
            // dense palm ring far from center
            for (int i = 0; i < 50; i++)
            {
                float ang = (i / 50f) * Mathf.PI * 2f + Random.Range(-0.05f, 0.05f);
                float r = 110f + Random.Range(-5f, 8f);
                BuildPalm(new Vector3(Mathf.Cos(ang) * r, 0f, Mathf.Sin(ang) * r));
            }
            // scattered closer palms
            for (int i = 0; i < 25; i++)
                BuildPalm(new Vector3(Random.Range(-90f, 90f), 0f, Random.Range(-90f, 90f)));
        }

        void BuildPalm(Vector3 pos)
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.position = pos + Vector3.up * 2.2f;
            trunk.transform.localScale = new Vector3(0.32f, 2.2f, 0.32f);
            trunk.GetComponent<Renderer>().sharedMaterial = ShaderCache.MakeTextured(
                ProceduralTextures.Wood, new Color(0.7f, 0.5f, 0.32f), 0f, 0.3f, new Vector2(1f, 3f));
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90f * Mathf.Deg2Rad;
                var leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leaf.transform.position = pos + Vector3.up * 4.3f + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 1.1f;
                leaf.transform.localScale = new Vector3(1.6f, 0.35f, 0.7f);
                leaf.transform.rotation = Quaternion.Euler(0f, i * 90f, -18f);
                Destroy(leaf.GetComponent<Collider>());
                leaf.GetComponent<Renderer>().sharedMaterial = ShaderCache.MakeTextured(
                    ProceduralTextures.Leaves, new Color(0.5f, 0.95f, 0.5f), 0f, 0.3f, new Vector2(2f, 1f));
            }
        }

        void BuildCrowd()
        {
            // 28 wandering NPCs spread across the island
            for (int i = 0; i < 28; i++)
            {
                var npc = SpawnCiv("Walker " + i, new Vector3(Random.Range(-60f, 60f), 0f, Random.Range(-60f, 60f)), Civilian.Mode.Wander);
                npc.shirtColor = RandomTropicalColor();
                npc.pantsColor = RandomTropicalColor();
                npc.patrolRadius = 12f;
                npc.walkSpeed = 1.0f + Random.value * 1.0f;
            }
        }

        void BuildViolationSpawner()
        {
            var go = new GameObject("[Violation Spawner]");
            var vs = go.AddComponent<ViolationSpawner>();
            vs.interval = 4f;
            vs.maxConcurrent = 8;
        }

        // ---------- HELPERS ----------
        Civilian SpawnCiv(string name, Vector3 pos, Civilian.Mode mode, Color? shirt = null, Color? pants = null)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var c = go.AddComponent<Civilian>();
            c.mode = mode;
            if (shirt.HasValue) c.shirtColor = shirt.Value;
            if (pants.HasValue) c.pantsColor = pants.Value;
            return c;
        }

        GameObject MakePrimCube(string name, Transform parent, Vector3 pos, Vector3 scale, Color color, float metallic, float smoothness)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = name;
            if (parent != null) { g.transform.SetParent(parent, false); g.transform.localPosition = pos; }
            else                  g.transform.position = pos;
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = MakeMat(color, metallic, smoothness);
            return g;
        }

        GameObject MakePrimCylinder(string name, Transform parent, Vector3 pos, Vector3 scale, Color color, float metallic, float smoothness)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            g.name = name;
            if (parent != null) { g.transform.SetParent(parent, false); g.transform.localPosition = pos; }
            else                  g.transform.position = pos;
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = MakeMat(color, metallic, smoothness);
            return g;
        }

        void AddLabel(GameObject parent, string text, Color color, int fontSize, Vector3 offset)
        {
            var lbl = new GameObject("Label");
            lbl.transform.SetParent(parent.transform, false);
            lbl.transform.localPosition = offset;
            var w = lbl.AddComponent<WorldLabel>();
            w.text = text; w.color = color; w.fontSize = fontSize;
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
            var sh = ShaderCache.Lit;
            var m = new Material(sh) { color = c };
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Smoothness", smoothness);
            return m;
        }
    }

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
