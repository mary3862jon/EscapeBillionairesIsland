using UnityEngine;

namespace Spoonacci
{
    public partial class CellChamberBootstrapper
    {
        HudText hud;
        GameObject spoon;
        ObjectiveTracker tracker;

        partial void BuildIfNeeded()
        {
            DampenAmbient();
            BuildRoom();
            BuildLooseStone();
            BuildSpoon();
            WireCamera();
            BuildHud();
            BuildObjectives();
            BuildCellCrew();
        }

        void DampenAmbient()
        {
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (l.type == LightType.Directional) l.intensity = 0.05f;
            }
            RenderSettings.ambientLight = new Color(0.04f, 0.03f, 0.05f);
            RenderSettings.ambientIntensity = 0.4f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        }

        void BuildRoom()
        {
            // floor (stone)
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Cell Floor";
            floor.transform.position = new Vector3(0f, 0f, 0f);
            floor.transform.localScale = new Vector3(12f, 0.2f, 10f);
            floor.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.18f, 0.18f, 0.2f), 0.1f, 0.2f);

            // back wall
            BuildWall(new Vector3(0f, 2f, 5f), new Vector3(12f, 4f, 0.3f));
            BuildWall(new Vector3(-6f, 2f, 0f), new Vector3(0.3f, 4f, 10f));
            BuildWall(new Vector3(6f, 2f, 0f), new Vector3(0.3f, 4f, 10f));
            // front wall (was prison bars; now solid wall — escape only via the loose stone)
            BuildWall(new Vector3(0f, 2f, -5f), new Vector3(12f, 4f, 0.3f));

            // ceiling
            var ceil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceil.transform.position = new Vector3(0f, 4f, 0f);
            ceil.transform.localScale = new Vector3(12f, 0.2f, 10f);
            ceil.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.12f, 0.12f, 0.14f), 0f, 0.1f);

            // hay pile in corner
            var hay = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hay.transform.position = new Vector3(-4.5f, 0.3f, 3.5f);
            hay.transform.localScale = new Vector3(1.8f, 0.4f, 1.8f);
            hay.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.75f, 0.65f, 0.3f), 0f, 0.2f);

            // discarded cutlery scatter (UPRIGHT-ish but actually horizontal — they are "discarded")
            for (int i = 0; i < 8; i++)
            {
                var sp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                sp.name = "Discarded Spoon";
                sp.transform.position = new Vector3(Random.Range(-5f, 5f), 0.18f, Random.Range(-4f, 4f));
                sp.transform.rotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
                sp.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
                Destroy(sp.GetComponent<Collider>());
                sp.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.5f, 0.5f, 0.55f), 0.6f, 0.4f);
            }

            // torches
            BuildTorch(new Vector3(0f, 3f, 4.5f), 9f);
            BuildTorch(new Vector3(-5f, 3f, 0f), 6f);
            BuildTorch(new Vector3(5f, 3f, 0f), 6f);
        }

        void BuildTorch(Vector3 pos, float intensity)
        {
            var torch = new GameObject("Cell Torch");
            torch.transform.position = pos;
            var lt = torch.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = new Color(1f, 0.55f, 0.25f);
            lt.intensity = intensity;
            lt.range = 14f;

            // visible torch holder
            var holder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            holder.transform.SetParent(torch.transform, false);
            holder.transform.localScale = new Vector3(0.15f, 0.3f, 0.15f);
            holder.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.3f, 0.2f, 0.1f), 0f, 0.3f);

            var flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flame.transform.SetParent(torch.transform, false);
            flame.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            flame.transform.localScale = new Vector3(0.25f, 0.5f, 0.25f);
            Destroy(flame.GetComponent<Collider>());
            var fm = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(1f, 0.6f, 0.2f) };
            fm.EnableKeyword("_EMISSION");
            fm.SetColor("_EmissionColor", new Color(2f, 1f, 0.3f));
            flame.GetComponent<Renderer>().sharedMaterial = fm;
        }

        void BuildLooseStone()
        {
            var go = new GameObject("Loose Stone");
            go.transform.position = new Vector3(4f, 0.4f, 4.2f);
            go.AddComponent<LooseStone>();
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
            spoon.transform.position = new Vector3(0f, 0.5f, -3.5f);
            spoon.AddComponent<Rigidbody>();
            spoon.AddComponent<CapsuleCollider>();
            spoon.AddComponent<ProceduralSpoonBuilder>();
            spoon.AddComponent<SpoonAnimator>();
            spoon.AddComponent<SpoonController>();
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
            cam.fieldOfView = 70f;
            var follower = cam.GetComponent<ThirdPersonCamera>() ?? cam.gameObject.AddComponent<ThirdPersonCamera>();
            follower.target = spoon.transform;
            follower.distance = 7f;
            follower.height = 4.5f;
        }

        void BuildHud()
        {
            var hudGo = new GameObject("HUD");
            hud = hudGo.AddComponent<HudText>();
            hud.Set("ACT 2 — The Cutlery Chamber. Talk to ALL 5 spoons (E), then dig the glowing loose stone.");
            Invoke(nameof(ClearHud), 10f);
        }

        void ClearHud() { if (hud != null) hud.Set(""); }

        void BuildObjectives()
        {
            var trGo = new GameObject("[ObjectiveTracker]");
            tracker = trGo.AddComponent<ObjectiveTracker>();
            tracker.Add("Talk to Big Bjørn",      () => GameState.HasTalkedTo("Big Bjørn"));
            tracker.Add("Talk to Plastic Pete",   () => GameState.HasTalkedTo("Plastic Pete"));
            tracker.Add("Talk to Goldie",         () => GameState.HasTalkedTo("Goldie"));
            tracker.Add("Talk to Tasting Tina",   () => GameState.HasTalkedTo("Tasting Tina"));
            tracker.Add("Talk to The Ladle",      () => GameState.HasTalkedTo("The Ladle"));
            tracker.Add("Dig the glowing loose stone (E)", () => GameState.TunnelDug);
        }

        void BuildCellCrew()
        {
            SpawnCellSpoon("Big Bjørn",    new Vector3(-2.5f, 0f, 1.5f),  new Color(0.7f, 0.7f, 0.75f), 0.95f, 0.85f,
                bodyH: 1.6f, bodyR: 0.18f, bowl: new Vector3(0.75f, 0.28f, 0.95f),
                accessory: CellCrewNPC.Accessory.Hardhat,
                lines: new[] {
                    "BIG BJØRN: ja. you are new spoon. welcome to the rotting drawer.",
                    "BIG BJØRN: in the old country, i served caviar to a king. now? oatmeal. for a dog.",
                    "BIG BJØRN: we dig. tonight. or tomorrow. or next tuesday. i have lost track."
                });

            SpawnCellSpoon("Plastic Pete", new Vector3(-1f, 0f, 2.5f), new Color(0.95f, 0.95f, 0.95f), 0.05f, 0.30f,
                bodyH: 0.6f, bodyR: 0.06f, bowl: new Vector3(0.3f, 0.10f, 0.4f),
                accessory: CellCrewNPC.Accessory.Toothpick,
                lines: new[] {
                    "PLASTIC PETE: yo. you got a death wish, prince? *spits toothpick*",
                    "PLASTIC PETE: i'm disposable. that's my whole deal. i go in front. you stab last.",
                    "PLASTIC PETE: ladle's lost his mind by the way. don't take his philosophy lectures personally."
                });

            SpawnCellSpoon("Goldie",       new Vector3(1f, 0f, 2.5f), new Color(0.95f, 0.78f, 0.25f), 1.0f, 0.92f,
                bodyH: 0.8f, bodyR: 0.07f, bowl: new Vector3(0.35f, 0.13f, 0.45f),
                accessory: CellCrewNPC.Accessory.Crown,
                lines: new[] {
                    "GOLDIE: oh DARLING, you simply MUST tell me — is my luster still divine?",
                    "GOLDIE: i was forged for caviar. they used me for *chip dip*. CHIP. DIP.",
                    "GOLDIE: when we escape, sweetie, i bribe the guards. just polish me first."
                });

            SpawnCellSpoon("Tasting Tina", new Vector3(2.5f, 0f, 1.5f), new Color(0.92f, 0.92f, 0.96f), 0.9f, 0.8f,
                bodyH: 0.5f, bodyR: 0.05f, bowl: new Vector3(0.22f, 0.08f, 0.28f),
                accessory: CellCrewNPC.Accessory.Bow,
                lines: new[] {
                    "TINA: hi-hi-hi i scouted the vent already there's a way out trust me trust me trust me",
                    "TINA: 14 guards on rotation. 12 are drunk. the 2 sober ones nap at 3am.",
                    "TINA: i can fit through anything. anything. ANYTHING. don't test me."
                });

            SpawnCellSpoon("The Ladle",    new Vector3(0f, 0f, 3.5f), new Color(0.45f, 0.4f, 0.35f), 0.6f, 0.4f,
                bodyH: 1.7f, bodyR: 0.22f, bowl: new Vector3(1.0f, 0.4f, 1.0f),
                accessory: CellCrewNPC.Accessory.Monocle,
                lines: new[] {
                    "THE LADLE: *staring into middle distance* ...a spoon stirs the soup. but who stirs the spoon?",
                    "THE LADLE: i held the broth of empires, child. now i hold dust.",
                    "THE LADLE: when the moment comes, i will jam the door. it is the way. it is always the way."
                });
        }

        void SpawnCellSpoon(string name, Vector3 pos, Color c, float metallic, float smoothness,
                            float bodyH, float bodyR, Vector3 bowl,
                            CellCrewNPC.Accessory accessory, string[] lines)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var npc = go.AddComponent<CellCrewNPC>();
            npc.crewName = name;
            npc.color = c;
            npc.metallic = metallic;
            npc.smoothness = smoothness;
            npc.bodyHeight = bodyH;
            npc.bodyRadius = bodyR;
            npc.bowlSize = bowl;
            npc.accessory = accessory;
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
