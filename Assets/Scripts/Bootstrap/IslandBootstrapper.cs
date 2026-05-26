using UnityEngine;

namespace Spoonacci
{
    // Builds the whole demo scene at runtime.
    // Boba opens SampleScene → hits Play → this script spawns ground, spoon, camera, salon, NPCs.
    // Nothing for him to wire in the Inspector.
    public class IslandBootstrapper : MonoBehaviour
    {
        public bool spawnSalon = true;
        public bool spawnSandbox = true; // open island vs. cell — overridden by CellChamberBootstrapper

        GameObject spoon;
        HudText hud;

        void Awake()
        {
            BuildGround();
            BuildSpoon();
            WireCamera();
            BuildHud();
            if (spawnSalon) BuildSalon();
            if (spawnSandbox) BuildSandbox();
        }

        void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground (Island Beach)";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(8f, 1f, 8f); // 80x80 tropical beach
            var rend = ground.GetComponent<Renderer>();
            rend.sharedMaterial = MakeMat(new Color(0.92f, 0.84f, 0.6f), 0f, 0.2f); // sandy

            // ocean ring (simple blue ring around sand)
            var ocean = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ocean.name = "Ocean";
            ocean.transform.position = new Vector3(0f, -0.1f, 0f);
            ocean.transform.localScale = new Vector3(50f, 1f, 50f);
            ocean.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.15f, 0.45f, 0.7f), 0.1f, 0.85f);
            Destroy(ocean.GetComponent<Collider>());
        }

        void BuildSpoon()
        {
            spoon = new GameObject("Sir Spoonacci");
            spoon.transform.position = new Vector3(0f, 1f, 0f);
            spoon.AddComponent<ProceduralSpoonBuilder>(); // (auto-adds Rigidbody+CapsuleCollider)
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
            var follower = cam.GetComponent<ThirdPersonCamera>() ?? cam.gameObject.AddComponent<ThirdPersonCamera>();
            follower.target = spoon.transform;
        }

        void BuildHud()
        {
            var hudGo = new GameObject("HUD");
            hud = hudGo.AddComponent<HudText>();
            hud.Set("WASD = walk · Space = hop · Right-mouse = look · E = interact");
            Invoke(nameof(ClearHud), 6f);
        }

        void ClearHud() { if (hud != null) hud.Set(""); }

        void BuildSalon()
        {
            // Posh pink kiosk on the beach. Visible from spawn.
            var salon = new GameObject("Salon Cucchiaio");
            salon.transform.position = new Vector3(6f, 0f, 4f);

            var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = "Salon Building";
            building.transform.SetParent(salon.transform, false);
            building.transform.localScale = new Vector3(3f, 2.4f, 2.5f);
            building.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            building.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(1.0f, 0.65f, 0.8f), 0.05f, 0.5f);

            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(salon.transform, false);
            roof.transform.localScale = new Vector3(3.4f, 0.2f, 2.9f);
            roof.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            roof.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.55f, 0.75f), 0.3f, 0.6f);

            var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "Sign — Salon Cucchiaio";
            sign.transform.SetParent(salon.transform, false);
            sign.transform.localScale = new Vector3(2.6f, 0.6f, 0.05f);
            sign.transform.localPosition = new Vector3(0f, 2.8f, -1.3f);
            sign.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.95f, 0.95f), 0.1f, 0.4f);

            var interact = salon.AddComponent<SalonCucchiaio>();
            interact.prompt = hud;
        }

        void BuildSandbox()
        {
            // a few palm-trunk stand-ins so the beach doesn't look empty
            for (int i = 0; i < 6; i++)
            {
                float ang = (i / 6f) * Mathf.PI * 2f;
                float r = 12f + (i % 2) * 3f;
                var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.name = "Palm " + i;
                trunk.transform.position = new Vector3(Mathf.Cos(ang) * r, 2f, Mathf.Sin(ang) * r);
                trunk.transform.localScale = new Vector3(0.3f, 2f, 0.3f);
                trunk.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.4f, 0.25f, 0.15f), 0f, 0.3f);

                var leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leaves.name = "Palm Leaves " + i;
                leaves.transform.position = trunk.transform.position + Vector3.up * 2.2f;
                leaves.transform.localScale = new Vector3(2.2f, 0.9f, 2.2f);
                leaves.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.2f, 0.55f, 0.25f), 0f, 0.3f);
                Destroy(leaves.GetComponent<Collider>());
            }
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
