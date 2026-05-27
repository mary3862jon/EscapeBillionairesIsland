using UnityEngine;

namespace Spoonacci
{
    public partial class CellChamberBootstrapper
    {
        HudText hud;
        GameObject spoon;

        partial void BuildIfNeeded()
        {
            DampenAmbient();
            BuildRoom();
            BuildCorridorBeyondBars();
            BuildSpoon();
            WireCamera();
            BuildHud();
            BuildCellCrew();
        }

        void DampenAmbient()
        {
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (l.type == LightType.Directional) l.intensity = 0.05f;
            }
            RenderSettings.ambientLight = new Color(0.04f, 0.03f, 0.05f);
            RenderSettings.ambientIntensity = 0.3f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        }

        void BuildRoom()
        {
            // floor (stone)
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Cell Floor";
            floor.transform.position = new Vector3(0f, 0f, 0f);
            floor.transform.localScale = new Vector3(10f, 0.2f, 8f);
            floor.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.18f, 0.18f, 0.2f), 0.1f, 0.2f);

            // back wall
            BuildWall(new Vector3(0f, 2f, 4f), new Vector3(10f, 4f, 0.3f));
            BuildWall(new Vector3(-5f, 2f, 0f), new Vector3(0.3f, 4f, 8f));
            BuildWall(new Vector3(5f, 2f, 0f), new Vector3(0.3f, 4f, 8f));

            // ceiling
            var ceil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceil.name = "Ceiling";
            ceil.transform.position = new Vector3(0f, 4f, 0f);
            ceil.transform.localScale = new Vector3(10f, 0.2f, 8f);
            ceil.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.12f, 0.12f, 0.14f), 0f, 0.1f);

            // prison bars (front, vertical cylinders, NO gap — closed cell)
            for (int i = -4; i <= 4; i++)
            {
                var bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bar.name = "Bar " + i;
                bar.transform.position = new Vector3(i * 0.5f, 2f, -4f);
                bar.transform.localScale = new Vector3(0.1f, 2f, 0.1f);
                bar.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.35f, 0.35f, 0.4f), 0.85f, 0.4f);
            }
            // crossbeam
            var hbar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hbar.name = "Bar Crossbeam";
            hbar.transform.position = new Vector3(0f, 3.5f, -4f);
            hbar.transform.localScale = new Vector3(10f, 0.15f, 0.15f);
            hbar.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.35f, 0.35f, 0.4f), 0.85f, 0.4f);

            // hay pile in the corner
            var hay = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hay.name = "Hay Pile";
            hay.transform.position = new Vector3(-3.5f, 0.3f, 2.8f);
            hay.transform.localScale = new Vector3(1.6f, 0.4f, 1.6f);
            hay.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.75f, 0.65f, 0.3f), 0f, 0.2f);

            // discarded cutlery scatter for vibe
            for (int i = 0; i < 6; i++)
            {
                var sp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                sp.name = "Discarded Spoon";
                sp.transform.position = new Vector3(Random.Range(-4f, 4f), 0.18f, Random.Range(-3f, 3.5f));
                sp.transform.rotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
                sp.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
                Destroy(sp.GetComponent<Collider>());
                sp.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.5f, 0.5f, 0.55f), 0.6f, 0.4f);
            }

            // torch
            var torch = new GameObject("Cell Torch");
            torch.transform.position = new Vector3(0f, 3f, 3.5f);
            var lt = torch.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = new Color(1f, 0.55f, 0.25f);
            lt.intensity = 8f;
            lt.range = 12f;

            // a small torch on the corridor side as well
            var torch2 = new GameObject("Corridor Torch");
            torch2.transform.position = new Vector3(0f, 3f, -7f);
            var lt2 = torch2.AddComponent<Light>();
            lt2.type = LightType.Point;
            lt2.color = new Color(1f, 0.55f, 0.25f);
            lt2.intensity = 6f;
            lt2.range = 10f;
        }

        // Beyond the bars: a guard corridor with floor (no more abyss),
        // walls forming a dead-end ('locked door' sets up next mission).
        void BuildCorridorBeyondBars()
        {
            // corridor floor
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Corridor Floor";
            floor.transform.position = new Vector3(0f, 0f, -8f);
            floor.transform.localScale = new Vector3(10f, 0.2f, 8f);
            floor.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.16f, 0.16f, 0.18f), 0.1f, 0.2f);

            // corridor side walls
            BuildWall(new Vector3(-5f, 2f, -8f), new Vector3(0.3f, 4f, 8f));
            BuildWall(new Vector3(5f, 2f, -8f), new Vector3(0.3f, 4f, 8f));

            // corridor end wall (the "locked door")
            BuildWall(new Vector3(0f, 2f, -12f), new Vector3(10f, 4f, 0.3f));
            var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Locked Door";
            door.transform.position = new Vector3(0f, 1.5f, -11.85f);
            door.transform.localScale = new Vector3(1.6f, 3f, 0.1f);
            door.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.3f, 0.18f, 0.1f), 0f, 0.3f);

            // corridor ceiling so it doesn't feel open
            var ceil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceil.name = "Corridor Ceiling";
            ceil.transform.position = new Vector3(0f, 4f, -8f);
            ceil.transform.localScale = new Vector3(10f, 0.2f, 8f);
            ceil.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.1f, 0.1f, 0.12f), 0f, 0.1f);

            // door sign — invisible label that floats
            var sign = new GameObject("DoorSign");
            sign.transform.position = door.transform.position + Vector3.up * 1.6f;
            var label = sign.AddComponent<WorldLabel>();
            label.text = "🔒 LOCKED — escape via the dig (tunnel quest, coming soon)";
            label.color = new Color(1f, 0.8f, 0.5f);
            label.fontSize = 16;
        }

        void BuildWall(Vector3 pos, Vector3 scale)
        {
            var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
            w.name = "Wall";
            w.transform.position = pos;
            w.transform.localScale = scale;
            w.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.22f, 0.22f, 0.24f), 0f, 0.15f);
        }

        void BuildSpoon()
        {
            spoon = new GameObject("Sir Spoonacci");
            spoon.transform.position = new Vector3(0f, 0.5f, -2.5f);
            spoon.AddComponent<Rigidbody>();
            spoon.AddComponent<CapsuleCollider>();
            spoon.AddComponent<ProceduralSpoonBuilder>();
            spoon.AddComponent<SpoonAnimator>();
            spoon.AddComponent<SpoonController>();
            // No BonkAttack in cell — nobody to bonk
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
            cam.backgroundColor = new Color(0.05f, 0.03f, 0.06f);
            var follower = cam.GetComponent<ThirdPersonCamera>() ?? cam.gameObject.AddComponent<ThirdPersonCamera>();
            follower.target = spoon.transform;
        }

        void BuildHud()
        {
            var hudGo = new GameObject("HUD");
            hud = hudGo.AddComponent<HudText>();
            hud.Set("The Cutlery Chamber. Talk to your fellow spoons (walk close, press E).");
            Invoke(nameof(ClearHud), 8f);
        }

        void ClearHud() { if (hud != null) hud.Set(""); }

        void BuildCellCrew()
        {
            Spawn("Big Bjørn", new Vector3(-2.5f, 0.15f, 1.5f),
                new Color(0.7f, 0.7f, 0.75f), 0.95f, 0.85f,
                bowlS: new Vector3(0.8f, 0.25f, 1.0f), handleS: new Vector3(0.18f, 0.9f, 0.18f),
                lines: new[] {
                    "BIG BJØRN: ja. you are new spoon. welcome to the rotting drawer.",
                    "BIG BJØRN: in the old country, i served caviar to a king. now? oatmeal. for a dog.",
                    "BIG BJØRN: we dig. tonight. or tomorrow. or next tuesday. i have lost track."
                });

            Spawn("Plastic Pete", new Vector3(-1f, 0.15f, 2.3f),
                new Color(0.95f, 0.95f, 0.95f), 0.05f, 0.30f,
                bowlS: new Vector3(0.35f, 0.12f, 0.5f), handleS: new Vector3(0.08f, 0.5f, 0.08f),
                lines: new[] {
                    "PLASTIC PETE: yo. you got a death wish, prince? *spits toothpick*",
                    "PLASTIC PETE: i'm disposable. that's my whole deal. i go in front. you stab last.",
                    "PLASTIC PETE: ladle's lost his mind by the way. don't take his philosophy lectures personally."
                });

            Spawn("Goldie", new Vector3(1f, 0.15f, 2.3f),
                new Color(0.95f, 0.78f, 0.25f), 1.0f, 0.92f,
                bowlS: new Vector3(0.3f, 0.12f, 0.4f), handleS: new Vector3(0.08f, 0.4f, 0.08f),
                lines: new[] {
                    "GOLDIE: oh DARLING, you simply MUST tell me — is my luster still divine?",
                    "GOLDIE: i was forged for caviar. they used me for *chip dip*. CHIP. DIP.",
                    "GOLDIE: when we escape, sweetie, i bribe the guards. just polish me first."
                });

            Spawn("Tasting Tina", new Vector3(2.5f, 0.15f, 1.5f),
                new Color(0.92f, 0.92f, 0.96f), 0.9f, 0.8f,
                bowlS: new Vector3(0.22f, 0.08f, 0.28f), handleS: new Vector3(0.05f, 0.3f, 0.05f),
                lines: new[] {
                    "TINA: hi-hi-hi i scouted the vent already there's a way out trust me trust me trust me",
                    "TINA: 14 guards on rotation. 12 are drunk. the 2 sober ones nap at 3am.",
                    "TINA: i can fit through anything. anything. ANYTHING. don't test me."
                });

            Spawn("The Ladle", new Vector3(0f, 0.15f, 3.2f),
                new Color(0.45f, 0.4f, 0.35f), 0.6f, 0.4f,
                bowlS: new Vector3(1.0f, 0.35f, 1.0f), handleS: new Vector3(0.18f, 1.2f, 0.18f),
                lines: new[] {
                    "THE LADLE: *staring into middle distance* ...a spoon stirs the soup. but who stirs the spoon?",
                    "THE LADLE: i held the broth of empires, child. now i hold dust.",
                    "THE LADLE: when the moment comes, i will jam the door. it is the way. it is always the way."
                });
        }

        void Spawn(string name, Vector3 pos, Color c, float metallic, float smoothness,
                   Vector3 bowlS, Vector3 handleS, string[] lines)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var npc = go.AddComponent<CellCrewNPC>();
            npc.crewName = name;
            npc.color = c;
            npc.metallic = metallic;
            npc.smoothness = smoothness;
            npc.bowlScale = bowlS;
            npc.handleScale = handleS;
            npc.lines = lines;
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
