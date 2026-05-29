using UnityEngine;

namespace Spoonacci
{
    // A display rack near Salon Cucchiaio that shows ALL 12 spoon skins as tiny upright spoons on shelves.
    // Pure visual — Salon trigger handles the actual cycling.
    public class SkinKiosk : MonoBehaviour
    {
        void Awake() => BuildShelves();

        void BuildShelves()
        {
            // base sign
            var bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.name = "Kiosk Back";
            bg.transform.SetParent(transform, false);
            bg.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            bg.transform.localScale = new Vector3(6f, 2.6f, 0.1f);
            bg.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.98f, 0.95f, 0.9f), 0.05f, 0.4f);

            // top banner
            var banner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            banner.transform.SetParent(transform, false);
            banner.transform.localPosition = new Vector3(0f, 2.8f, 0f);
            banner.transform.localScale = new Vector3(6.4f, 0.4f, 0.12f);
            banner.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.95f, 0.5f, 0.75f), 0.3f, 0.6f);
            var label = banner.AddComponent<WorldLabel>();
            label.text = "sign.skinkit";
            label.color = new Color(1f, 0.5f, 0.82f);
            label.fontSize = 22;

            // 12 mini-spoon icons in a 4x3 grid
            int cols = 4, rows = 3;
            int idx = 0;
            for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                if (idx >= SpoonSkinLibrary.All.Count) break;
                var skin = SpoonSkinLibrary.All[idx];
                float x = -2.1f + c * 1.4f;
                float y = 1.95f - r * 0.75f;
                BuildMiniSpoon(new Vector3(x, y, 0.1f), skin);

                // tiny shelf line
                var shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shelf.transform.SetParent(transform, false);
                shelf.transform.localPosition = new Vector3(x, y - 0.32f, 0.1f);
                shelf.transform.localScale = new Vector3(1.2f, 0.03f, 0.15f);
                shelf.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.7f, 0.55f, 0.3f), 0.2f, 0.5f);
                Destroy(shelf.GetComponent<Collider>());
                idx++;
            }
        }

        void BuildMiniSpoon(Vector3 pos, SpoonSkinLibrary.Skin skin)
        {
            var root = new GameObject("MiniSpoon " + skin.name);
            root.transform.SetParent(transform, false);
            root.transform.localPosition = pos;
            root.transform.localScale = Vector3.one * 0.5f;

            var mat = MakeMat(skin.color, skin.metallic, skin.smoothness);

            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.transform.SetParent(root.transform, false);
            handle.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            handle.transform.localScale = new Vector3(0.16f, 0.3f, 0.16f);
            Destroy(handle.GetComponent<Collider>());
            handle.GetComponent<Renderer>().sharedMaterial = mat;

            var bowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bowl.transform.SetParent(root.transform, false);
            bowl.transform.localPosition = new Vector3(0f, 0.65f, 0.02f);
            bowl.transform.localScale = new Vector3(0.36f, 0.14f, 0.45f);
            Destroy(bowl.GetComponent<Collider>());
            bowl.GetComponent<Renderer>().sharedMaterial = mat;
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
