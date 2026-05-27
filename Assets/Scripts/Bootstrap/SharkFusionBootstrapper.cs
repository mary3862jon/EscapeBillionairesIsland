using System.Collections;
using UnityEngine;

namespace Spoonacci
{
    // Act 3 — Beach → enter water → fusion cutscene → control shark.
    public class SharkFusionBootstrapper : MonoBehaviour
    {
        GameObject spoon;
        GameObject shark;
        ProceduralShark sharkComp;
        HudText hud;
        ObjectiveTracker tracker;
        bool fused;

        void Awake()
        {
            SoundFx.Instance.ToString();
            DimAmbient();
            BuildGround();
            BuildOcean();
            BuildPalmsAndShack();
            BuildSpoon();
            BuildShark();
            WireCamera();
            BuildHud();
            BuildObjectives();
        }

        void DimAmbient()
        {
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) l.intensity = 1.2f;
            RenderSettings.ambientLight = new Color(0.45f, 0.55f, 0.7f);
            RenderSettings.ambientIntensity = 1f;
        }

        void BuildGround()
        {
            var sand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sand.name = "Beach";
            sand.transform.position = new Vector3(0f, -0.05f, 18f);
            sand.transform.localScale = new Vector3(80f, 0.2f, 40f);
            sand.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.97f, 0.88f, 0.62f), 0f, 0.18f);
        }

        void BuildOcean()
        {
            var ocean = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ocean.name = "Ocean";
            ocean.transform.position = new Vector3(0f, -1.0f, -22f);
            ocean.transform.localScale = new Vector3(160f, 2.1f, 80f);
            ocean.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.1f, 0.4f, 0.65f), 0.2f, 0.95f);
            Destroy(ocean.GetComponent<Collider>());

            var trig = new GameObject("Water Edge Trigger");
            trig.transform.position = new Vector3(0f, 0.3f, -3f);
            var box = trig.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(120f, 5f, 6f);
            trig.AddComponent<WaterEdgeTrigger>().bootstrap = this;
        }

        void BuildPalmsAndShack()
        {
            for (int i = 0; i < 10; i++)
            {
                var pos = new Vector3(Random.Range(-30f, 30f), 0f, Random.Range(8f, 30f));
                BuildPalm(pos);
            }
            // a tiki shack
            var shack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shack.transform.position = new Vector3(-12f, 1.4f, 14f);
            shack.transform.localScale = new Vector3(3.5f, 2.8f, 3.5f);
            shack.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.5f, 0.32f, 0.18f), 0f, 0.3f);
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.transform.position = shack.transform.position + Vector3.up * 1.6f;
            roof.transform.localScale = new Vector3(4f, 0.2f, 4f);
            roof.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.55f, 0.4f, 0.2f), 0f, 0.3f);

            // a couple of beach civilians for life
            for (int i = 0; i < 4; i++)
            {
                var c = new GameObject("Beachgoer " + i);
                c.transform.position = new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(6f, 15f));
                var civ = c.AddComponent<Civilian>();
                civ.mode = Civilian.Mode.Wander;
                civ.shirtColor = new Color(Random.value, Random.value, Random.value);
                civ.pantsColor = new Color(Random.value, Random.value, Random.value);
                civ.patrolRadius = 6f;
            }
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

        void BuildSpoon()
        {
            spoon = new GameObject("Sir Spoonacci");
            spoon.transform.position = new Vector3(0f, 0.5f, 12f);
            spoon.AddComponent<Rigidbody>();
            spoon.AddComponent<CapsuleCollider>();
            spoon.AddComponent<ProceduralSpoonBuilder>();
            spoon.AddComponent<SpoonAnimator>();
            spoon.AddComponent<SpoonController>();
        }

        void BuildShark()
        {
            shark = new GameObject("Confused Shark");
            // Y=0 so dorsal fin sticks above water
            shark.transform.position = new Vector3(15f, 0f, -10f);
            shark.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            sharkComp = shark.AddComponent<ProceduralShark>();
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
            hud.Set("ACT 3 — Walk into the ocean. A shark awaits.");
            Invoke(nameof(ClearHud), 8f);
        }

        void ClearHud() { if (hud != null) hud.Set(""); }

        void BuildObjectives()
        {
            var trGo = new GameObject("[ObjectiveTracker]");
            tracker = trGo.AddComponent<ObjectiveTracker>();
        }

        public void TriggerFusion()
        {
            if (fused) return;
            fused = true;
            StartCoroutine(FusionSequence());
        }

        IEnumerator FusionSequence()
        {
            hud.Set("...A shark approaches...");
            SoundFx.Instance.Swoosh();

            var sc = spoon.GetComponent<SpoonController>();
            if (sc != null) sc.enabled = false;
            var rb = spoon.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            // pan camera dramatically
            var cam = Camera.main;
            var follow = cam != null ? cam.GetComponent<ThirdPersonCamera>() : null;
            if (follow != null) { follow.distance = 14f; follow.height = 8f; }

            yield return new WaitForSeconds(0.8f);

            // shark sprints to the spoon
            float t = 0f;
            Vector3 start = shark.transform.position;
            Vector3 end = spoon.transform.position + Vector3.down * 0.3f - shark.transform.forward * 1.2f;
            while (t < 1f)
            {
                t += Time.deltaTime * 1.4f;
                shark.transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t));
                Vector3 look = spoon.transform.position - shark.transform.position; look.y = 0f;
                if (look.sqrMagnitude > 0.01f) shark.transform.rotation = Quaternion.LookRotation(look, Vector3.up);
                yield return null;
            }

            // CHOMP — attach spoon to snout
            hud.Set("CHOMP!");
            SoundFx.Instance.Bonk();
            spoon.transform.SetParent(sharkComp.SnoutPivot, true);
            spoon.transform.localPosition = Vector3.zero;
            spoon.transform.localRotation = Quaternion.Euler(-25f, 0f, 0f); // unicorn-horn angle

            yield return new WaitForSeconds(0.6f);

            // hand control to the shark
            shark.AddComponent<SharkController>();
            if (follow != null) { follow.target = shark.transform; follow.distance = 11f; follow.height = 3.5f; }
            hud.Set("🦈 SPOON-SHARK MODE — WASD swim · SPACE dash · roam the ocean");
            SoundFx.Instance.LevelUp();
            SharkFusionState.Fused = true;
            SaveSystem.Instance.Save("Spoon-Shark formed!");
            Invoke(nameof(ClearHud), 8f);
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

    public class WaterEdgeTrigger : MonoBehaviour
    {
        public SharkFusionBootstrapper bootstrap;
        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<ProceduralSpoonBuilder>() != null) bootstrap.TriggerFusion();
        }
    }
}
