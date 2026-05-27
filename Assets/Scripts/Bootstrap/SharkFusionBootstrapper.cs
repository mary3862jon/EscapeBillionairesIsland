using System.Collections;
using UnityEngine;

namespace Spoonacci
{
    // Act 3 scene. Beach → enter water → fusion cutscene → control shark.
    public class SharkFusionBootstrapper : MonoBehaviour
    {
        GameObject spoon;
        GameObject shark;
        ProceduralShark sharkComp;
        HudText hud;
        bool fused;

        void Awake()
        {
            DimAmbient();
            BuildGround();
            BuildOcean();
            BuildPalmsAndShack();
            BuildSpoon();
            BuildShark();
            WireCamera();
            BuildHud();
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
            // tropical beach narrows to shoreline at z=0
            var sand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sand.name = "Beach";
            sand.transform.position = new Vector3(0f, -0.05f, 15f);
            sand.transform.localScale = new Vector3(60f, 0.2f, 30f);
            sand.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.97f, 0.88f, 0.62f), 0f, 0.18f);
        }

        void BuildOcean()
        {
            var ocean = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ocean.name = "Ocean";
            ocean.transform.position = new Vector3(0f, -1.0f, -20f);
            ocean.transform.localScale = new Vector3(120f, 2.1f, 60f);
            ocean.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.1f, 0.4f, 0.65f), 0.2f, 0.95f);
            Destroy(ocean.GetComponent<Collider>());

            // surface trigger to detect spoon entering water
            var trig = new GameObject("Water Edge Trigger");
            trig.transform.position = new Vector3(0f, 0.2f, -2f);
            var box = trig.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(80f, 4f, 4f);
            trig.AddComponent<WaterEdgeTrigger>().bootstrap = this;
        }

        void BuildPalmsAndShack()
        {
            // a few palms behind for scene depth
            for (int i = 0; i < 5; i++)
            {
                var pos = new Vector3(Random.Range(-22f, 22f), 0f, Random.Range(8f, 22f));
                BuildPalm(pos);
            }
            // a tiki shack
            var shack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shack.name = "Beach Shack";
            shack.transform.position = new Vector3(-12f, 1.4f, 12f);
            shack.transform.localScale = new Vector3(3f, 2.8f, 3f);
            shack.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.5f, 0.32f, 0.18f), 0f, 0.3f);
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.transform.position = shack.transform.position + Vector3.up * 1.6f;
            roof.transform.localScale = new Vector3(3.6f, 0.2f, 3.6f);
            roof.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.55f, 0.4f, 0.2f), 0f, 0.3f);
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
            spoon.transform.position = new Vector3(0f, 0.5f, 10f);
            spoon.AddComponent<Rigidbody>();
            spoon.AddComponent<CapsuleCollider>();
            spoon.AddComponent<ProceduralSpoonBuilder>();
            spoon.AddComponent<SpoonAnimator>();
            spoon.AddComponent<SpoonController>();
        }

        void BuildShark()
        {
            shark = new GameObject("Confused Shark");
            shark.transform.position = new Vector3(8f, -0.4f, -8f);
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
            var follower = cam.GetComponent<ThirdPersonCamera>() ?? cam.gameObject.AddComponent<ThirdPersonCamera>();
            follower.target = spoon.transform;
        }

        void BuildHud()
        {
            var hudGo = new GameObject("HUD");
            hud = hudGo.AddComponent<HudText>();
            hud.Set("ACT 3 — Walk into the ocean to trigger Shark Fusion.");
            Invoke(nameof(ClearHud), 8f);
        }

        void ClearHud() { if (hud != null) hud.Set(""); }

        // Called by trigger
        public void TriggerFusion()
        {
            if (fused) return;
            fused = true;
            StartCoroutine(FusionSequence());
        }

        IEnumerator FusionSequence()
        {
            hud.Set("...A shark approaches...");

            // disable player control during cinematic
            var sc = spoon.GetComponent<SpoonController>();
            if (sc != null) sc.enabled = false;
            var rb = spoon.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            // dramatic camera swap to shark
            var cam = Camera.main;
            var follow = cam != null ? cam.GetComponent<ThirdPersonCamera>() : null;
            if (follow != null)
            {
                follow.target = shark.transform;
                follow.distance = 8f;
            }
            yield return new WaitForSeconds(1.2f);

            // shark approaches the spoon
            float t = 0f;
            Vector3 start = shark.transform.position;
            Vector3 end = spoon.transform.position + Vector3.up * 0.2f;
            while (t < 1f)
            {
                t += Time.deltaTime * 0.9f;
                shark.transform.position = Vector3.Lerp(start, end, t);
                shark.transform.LookAt(spoon.transform.position);
                yield return null;
            }

            // CHOMP — parent the spoon to shark's snout
            hud.Set("CHOMP!");
            spoon.transform.SetParent(sharkComp.SnoutPivot, true);
            spoon.transform.localPosition = Vector3.zero;
            spoon.transform.localRotation = Quaternion.Euler(-25f, 0f, 0f); // lodged in like a horn

            yield return new WaitForSeconds(0.6f);

            // hand control to the shark
            shark.AddComponent<SharkController>();
            if (follow != null) follow.target = shark.transform;
            hud.Set("SPOON-SHARK MODE — WASD swim · Space dash · ride toward the island for chaos");
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
