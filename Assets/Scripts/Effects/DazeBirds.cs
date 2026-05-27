using UnityEngine;

namespace Spoonacci
{
    // Three little cartoon birds circling a stunned NPC's head for a few seconds.
    // Each bird = a small ellipsoid body + two tiny wing cubes that flap.
    public class DazeBirds : MonoBehaviour
    {
        public float lifetime = 5f;
        public float radius = 0.7f;
        public int birdCount = 3;

        float endAt;
        Transform[] birds;
        Transform[] wingsL;
        Transform[] wingsR;

        public void Attach(Transform head)
        {
            transform.SetParent(head, false);
            transform.localPosition = new Vector3(0f, 0.45f, 0f);
            endAt = Time.time + lifetime;
            BuildBirds();
        }

        void BuildBirds()
        {
            birds = new Transform[birdCount];
            wingsL = new Transform[birdCount];
            wingsR = new Transform[birdCount];

            var body = MakeMat(new Color(1f, 0.95f, 0.4f), 0f, 0.3f);
            var beak = MakeMat(new Color(1f, 0.55f, 0.1f), 0f, 0.3f);

            for (int i = 0; i < birdCount; i++)
            {
                var b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                b.transform.SetParent(transform, false);
                b.transform.localScale = new Vector3(0.18f, 0.14f, 0.22f);
                Destroy(b.GetComponent<Collider>());
                b.GetComponent<Renderer>().sharedMaterial = body;
                birds[i] = b.transform;

                // beak
                var bk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bk.transform.SetParent(b.transform, false);
                bk.transform.localPosition = new Vector3(0f, -0.1f, 0.6f);
                bk.transform.localScale = new Vector3(0.4f, 0.2f, 0.5f);
                Destroy(bk.GetComponent<Collider>());
                bk.GetComponent<Renderer>().sharedMaterial = beak;

                // wings
                var wl = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wl.transform.SetParent(b.transform, false);
                wl.transform.localPosition = new Vector3(-0.5f, 0.15f, 0f);
                wl.transform.localScale = new Vector3(0.7f, 0.1f, 0.6f);
                Destroy(wl.GetComponent<Collider>());
                wl.GetComponent<Renderer>().sharedMaterial = body;
                wingsL[i] = wl.transform;

                var wr = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wr.transform.SetParent(b.transform, false);
                wr.transform.localPosition = new Vector3(0.5f, 0.15f, 0f);
                wr.transform.localScale = new Vector3(0.7f, 0.1f, 0.6f);
                Destroy(wr.GetComponent<Collider>());
                wr.GetComponent<Renderer>().sharedMaterial = body;
                wingsR[i] = wr.transform;
            }
        }

        void Update()
        {
            if (Time.time > endAt) { Destroy(gameObject); return; }

            float t = Time.time * 4f;
            for (int i = 0; i < birds.Length; i++)
            {
                float ang = t + (i / (float)birds.Length) * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(ang) * radius, Mathf.Sin(t * 1.7f + i) * 0.07f, Mathf.Sin(ang) * radius);
                birds[i].localPosition = p;
                birds[i].localRotation = Quaternion.LookRotation(new Vector3(-Mathf.Sin(ang), 0f, Mathf.Cos(ang)));

                float flap = Mathf.Sin(Time.time * 22f + i) * 35f;
                wingsL[i].localRotation = Quaternion.Euler(0f, 0f, flap);
                wingsR[i].localRotation = Quaternion.Euler(0f, 0f, -flap);
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
