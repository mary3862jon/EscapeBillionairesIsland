using System.Collections.Generic;
using UnityEngine;

namespace Spoonacci
{
    public partial class CellChamberBootstrapper
    {
        HudText hud;
        GameObject spoon;
        ObjectiveTracker tracker;
        readonly List<PrisonInmate> _allInmates = new List<PrisonInmate>();
        readonly List<PrisonCellDoor> _allDoors = new List<PrisonCellDoor>();

        partial void BuildIfNeeded()
        {
            _ = MusicPlayer.Instance;
            PrisonState.Reset();
            gameObject.AddComponent<PostFxBoost>();
            gameObject.AddComponent<PauseMenu>();
            gameObject.AddComponent<QuestBanner>();
            gameObject.AddComponent<LanguageToggle>();
            DampenAmbient();
            BuildOuterStructure();
            BuildCells();
            BuildLooseStone();
            BuildPickaxe();
            BuildSpoon();
            WireCamera();
            BuildHud();
            BuildObjectives();
        }

        void DampenAmbient()
        {
            // moody but readable — was pitch-black before
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) l.intensity = 0.5f;

            // a soft warm directional "skylight" through the ceiling for fill
            var fillGo = new GameObject("Skylight Fill");
            fillGo.transform.rotation = Quaternion.Euler(70f, 30f, 0f);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.55f;
            fill.color = new Color(0.9f, 0.8f, 0.65f);

            RenderSettings.ambientLight = new Color(0.4f, 0.38f, 0.45f);
            RenderSettings.ambientIntensity = 1.0f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        }

        // BIG prison: 50m wide × 30m deep, 6m high, 5 cells along north wall
        const float PRISON_W = 50f;
        const float PRISON_D = 30f;
        const float PRISON_H = 6f;
        const int   CELL_COUNT = 5;
        const float CELL_W = 8f;       // cell width (less than PRISON_W/5 for walls)
        const float CELL_D = 7f;       // cell depth (north side)
        const int   INMATES_PER_CELL = 3;
        const float CORRIDOR_Z = -2f;  // player corridor

        void BuildOuterStructure()
        {
            // Floor (corridor area in front of cells)
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Prison Floor";
            floor.transform.position = new Vector3(0f, 0f, 0f);
            floor.transform.localScale = new Vector3(PRISON_W, 0.2f, PRISON_D);
            floor.GetComponent<Renderer>().sharedMaterial = ShaderCache.MakeTextured(
                ProceduralTextures.Stone, new Color(0.65f, 0.62f, 0.58f), 0.1f, 0.35f, new Vector2(12f, 8f));

            // Ceiling
            var ceil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceil.transform.position = new Vector3(0f, PRISON_H, 0f);
            ceil.transform.localScale = new Vector3(PRISON_W, 0.2f, PRISON_D);
            ceil.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.35f, 0.33f, 0.35f), 0f, 0.2f);

            // Outer walls
            BuildWall(new Vector3(0f, PRISON_H * 0.5f, PRISON_D * 0.5f),  new Vector3(PRISON_W, PRISON_H, 0.3f));  // north
            BuildWall(new Vector3(0f, PRISON_H * 0.5f, -PRISON_D * 0.5f), new Vector3(PRISON_W, PRISON_H, 0.3f));  // south
            BuildWall(new Vector3(-PRISON_W * 0.5f, PRISON_H * 0.5f, 0f), new Vector3(0.3f, PRISON_H, PRISON_D));  // west
            BuildWall(new Vector3(PRISON_W * 0.5f,  PRISON_H * 0.5f, 0f), new Vector3(0.3f, PRISON_H, PRISON_D));  // east

            // Cell separator walls (between cells, north half)
            // Cells span from z=-(PRISON_D/2 - CELL_D - 1) to north wall.
            // Easier: cells occupy north portion z ≥ 4 down to z = PRISON_D/2.
            // Cell row centers: x = -16, -8, 0, 8, 16 (5 cells, ~8m apart)
            for (int i = 0; i < CELL_COUNT + 1; i++)
            {
                float x = -PRISON_W * 0.5f + (i + 0.5f) * (PRISON_W / (CELL_COUNT + 0.5f));
                // adjusted layout — separator walls between cells
                if (i == 0 || i == CELL_COUNT) continue; // outer walls handled above
            }

            // Wall torches (5) along south wall
            for (int i = -2; i <= 2; i++)
            {
                BuildTorch(new Vector3(i * 10f, PRISON_H - 0.5f, -PRISON_D * 0.5f + 0.4f), 12f);
            }
            // Corridor ceiling strip lights (overhead fluorescents)
            for (int i = -2; i <= 2; i++)
            {
                BuildCeilingStrip(new Vector3(i * 10f, PRISON_H - 0.2f, -PRISON_D * 0.25f), 18f);
            }
            // Per-cell ceiling lamps
            float spacing = PRISON_W / (float)CELL_COUNT;
            for (int c = 0; c < CELL_COUNT; c++)
            {
                float cx = -PRISON_W * 0.5f + spacing * (c + 0.5f);
                BuildCeilingStrip(new Vector3(cx, PRISON_H - 0.5f, PRISON_D * 0.25f), 14f);
            }

            // Sign on south wall
            var signGo = new GameObject("PrisonSign");
            signGo.transform.position = new Vector3(0f, PRISON_H + 0.8f, -PRISON_D * 0.5f - 0.5f);
            var lbl = signGo.AddComponent<WorldLabel>();
            lbl.text = "🏚 THE CUTLERY CHAMBER — Prison Wing 7";
            lbl.color = new Color(0.95f, 0.85f, 0.3f);
            lbl.fontSize = 30;
        }

        void BuildCells()
        {
            // 5 cells along the north half: each cell is enclosed by 3 walls + barred south side (the door)
            float cellRowZ = PRISON_D * 0.25f; // cells centered north
            float spacing = PRISON_W / (float)CELL_COUNT;

            string[] cellNames = { "Cell A", "Cell B", "Cell C", "Cell D", "Cell E" };

            for (int c = 0; c < CELL_COUNT; c++)
            {
                float cx = -PRISON_W * 0.5f + spacing * (c + 0.5f);
                BuildSingleCell(cellNames[c], new Vector3(cx, 0f, cellRowZ), c == 0);
            }
        }

        void BuildSingleCell(string label, Vector3 center, bool isHeroCell)
        {
            float w = CELL_W, d = CELL_D, h = PRISON_H - 0.3f;
            // back wall (north)
            BuildWall(center + new Vector3(0f, h * 0.5f, d * 0.5f), new Vector3(w, h, 0.25f));
            // side walls
            BuildWall(center + new Vector3(-w * 0.5f, h * 0.5f, 0f), new Vector3(0.25f, h, d));
            BuildWall(center + new Vector3( w * 0.5f, h * 0.5f, 0f), new Vector3(0.25f, h, d));
            // floor differentiation (slightly lighter)
            var f = GameObject.CreatePrimitive(PrimitiveType.Cube);
            f.transform.position = center + Vector3.up * 0.11f;
            f.transform.localScale = new Vector3(w - 0.4f, 0.04f, d - 0.4f);
            f.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.55f, 0.50f, 0.45f), 0f, 0.3f);

            // hay pile in corner
            var hay = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hay.transform.position = center + new Vector3(-w * 0.35f, 0.25f, d * 0.3f);
            hay.transform.localScale = new Vector3(1.0f, 0.3f, 1.0f);
            hay.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.75f, 0.65f, 0.3f), 0f, 0.2f);

            // door — south side of cell, faces corridor
            var doorGO = new GameObject("Door " + label);
            doorGO.transform.position = center + new Vector3(0f, 0f, -d * 0.5f);
            var door = doorGO.AddComponent<PrisonCellDoor>();
            door.cellLabel = label;
            _allDoors.Add(door);

            // inmates inside
            var inmates = new List<PrisonInmate>();
            for (int i = 0; i < INMATES_PER_CELL; i++)
            {
                var pos = center + new Vector3((i - 1) * 1.8f, 0f, 1.5f);
                var inmate = SpawnInmate(label + " Inmate " + (i + 1), pos);
                inmates.Add(inmate);
                _allInmates.Add(inmate);
            }
            door.inmatesInside = inmates.ToArray();

            // hero's cell — pre-unlocked
            if (isHeroCell)
            {
                door.unlocked = true;
                door.GetComponent<Collider>().enabled = false;
                // hide door visual by rotating it open
                if (door.doorVisual != null) door.doorVisual.localRotation = Quaternion.Euler(0f, -85f, 0f);
                foreach (var i in inmates) i.Unlock();
            }

            // label sign above the cell
            var sg = new GameObject("CellSign " + label);
            sg.transform.position = center + new Vector3(0f, h + 0.4f, -d * 0.5f);
            var lbl = sg.AddComponent<WorldLabel>();
            lbl.text = label;
            lbl.color = new Color(0.95f, 0.85f, 0.4f);
            lbl.fontSize = 22;
        }

        PrisonInmate SpawnInmate(string name, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var pi = go.AddComponent<PrisonInmate>();
            pi.inmateName = name;
            // randomized look
            float v = Random.value;
            if (v < 0.2f)      { pi.color = new Color(0.95f, 0.78f, 0.25f); pi.metallic = 1.0f; pi.smoothness = 0.92f; pi.bowlSize = new Vector3(0.35f, 0.13f, 0.45f); pi.bodyHeight = 0.7f; }
            else if (v < 0.4f) { pi.color = new Color(0.95f, 0.95f, 0.95f); pi.metallic = 0.05f; pi.smoothness = 0.3f; pi.bowlSize = new Vector3(0.32f, 0.12f, 0.42f); pi.bodyHeight = 0.6f; }
            else if (v < 0.55f){ pi.color = new Color(0.55f, 0.35f, 0.25f); pi.metallic = 0.0f; pi.smoothness = 0.25f; pi.bowlSize = new Vector3(0.4f, 0.18f, 0.5f); pi.bodyHeight = 0.85f; }
            else               { pi.color = new Color(0.8f, 0.82f, 0.85f); pi.metallic = 0.95f; pi.smoothness = 0.85f; pi.bowlSize = new Vector3(0.42f, 0.16f, 0.55f); pi.bodyHeight = 0.9f; }
            return pi;
        }

        void BuildWall(Vector3 pos, Vector3 scale)
        {
            var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
            w.transform.position = pos;
            w.transform.localScale = scale;
            w.GetComponent<Renderer>().sharedMaterial = ShaderCache.MakeTextured(
                ProceduralTextures.Stone, new Color(0.7f, 0.65f, 0.6f), 0f, 0.3f, new Vector2(scale.x * 0.5f, scale.y * 0.5f));
        }

        void BuildCeilingStrip(Vector3 pos, float intensity)
        {
            var go = new GameObject("Ceiling Lamp");
            go.transform.position = pos;
            var lt = go.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = new Color(1f, 0.92f, 0.78f);
            lt.intensity = intensity;
            lt.range = 16f;

            // visible bar
            var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.transform.SetParent(go.transform, false);
            bar.transform.localScale = new Vector3(2.6f, 0.12f, 0.3f);
            Destroy(bar.GetComponent<Collider>());
            var mat = new Material(ShaderCache.Lit) { color = new Color(1f, 0.97f, 0.85f) };
            mat.SetFloat("_Metallic", 0.2f); mat.SetFloat("_Smoothness", 0.3f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(2.5f, 2.2f, 1.5f));
            bar.GetComponent<Renderer>().sharedMaterial = mat;
        }

        void BuildTorch(Vector3 pos, float intensity)
        {
            var torch = new GameObject("Torch");
            torch.transform.position = pos;
            var lt = torch.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = new Color(1f, 0.55f, 0.25f);
            lt.intensity = intensity;
            lt.range = 14f;

            var holder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            holder.transform.SetParent(torch.transform, false);
            holder.transform.localScale = new Vector3(0.18f, 0.4f, 0.18f);
            holder.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.3f, 0.2f, 0.1f), 0f, 0.3f);
            var flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flame.transform.SetParent(torch.transform, false);
            flame.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            flame.transform.localScale = new Vector3(0.3f, 0.55f, 0.3f);
            Destroy(flame.GetComponent<Collider>());
            var fm = new Material(ShaderCache.Lit) { color = new Color(1f, 0.6f, 0.2f) };
            fm.EnableKeyword("_EMISSION");
            fm.SetColor("_EmissionColor", new Color(2.5f, 1.2f, 0.4f));
            flame.GetComponent<Renderer>().sharedMaterial = fm;
        }

        void BuildLooseStone()
        {
            var go = new GameObject("Loose Stone");
            // place in the corridor at the south-west, far from hero start
            go.transform.position = new Vector3(-PRISON_W * 0.4f, 0.4f, -PRISON_D * 0.35f);
            go.AddComponent<LooseStone>();
        }

        void BuildPickaxe()
        {
            // sits in hero's cell on the hay pile (cell A, leftmost)
            float spacing = PRISON_W / (float)CELL_COUNT;
            float heroCellX = -PRISON_W * 0.5f + spacing * 0.5f;
            var go = new GameObject("Pickaxe");
            go.transform.position = new Vector3(heroCellX - 1.4f, 0.55f, PRISON_D * 0.25f + 1.8f);
            go.AddComponent<Pickaxe>();
        }

        void BuildSpoon()
        {
            spoon = new GameObject("Sir Spoonacci");
            // spawn inside hero's cell (cell A, leftmost), near the door
            float spacing = PRISON_W / (float)CELL_COUNT;
            float heroCellX = -PRISON_W * 0.5f + spacing * 0.5f;
            spoon.transform.position = new Vector3(heroCellX, 0.5f, PRISON_D * 0.25f - 2.5f);
            spoon.AddComponent<Rigidbody>();
            spoon.AddComponent<CapsuleCollider>();
            spoon.AddComponent<ProceduralSpoonBuilder>();
            spoon.AddComponent<SpoonAnimator>();
            spoon.AddComponent<SpoonController>();
            spoon.AddComponent<BonkAttack>(); // can manual-bonk other things in prison too
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
            cam.fieldOfView = 65f;
            var follower = cam.GetComponent<ThirdPersonCamera>() ?? cam.gameObject.AddComponent<ThirdPersonCamera>();
            follower.target = spoon.transform;
            follower.distance = 6f;
            follower.height = 2.6f;
        }

        void BuildHud()
        {
            var hudGo = new GameObject("HUD");
            hud = hudGo.AddComponent<HudText>();
            hud.Set("ACT 2 — Cutlery Chamber. Grab the pickaxe, unlock the 4 other cells, persuade all 15 spoons to dig.");
            Invoke(nameof(ClearHud), 12f);
        }

        void ClearHud() { if (hud != null) hud.Set(""); }

        void BuildObjectives()
        {
            var trGo = new GameObject("[ObjectiveTracker]");
            tracker = trGo.AddComponent<ObjectiveTracker>();
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
