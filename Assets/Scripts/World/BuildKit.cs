using UnityEngine;

namespace Spoonacci
{
    // Shared primitive/material/NPC helpers so island-feature builders
    // (Pool, Alley, Paths, Vegetation, Perimeter, Cyclists, Props...) can each
    // live in their own file and build the world without duplicating boilerplate.
    //
    // Every builder should follow the convention:
    //     public static class FooBuilder { public static void Build(Transform root) { ... } }
    // and add its content under `root` (or under a child Root("Foo", pos)).
    public static class BuildKit
    {
        public static Material Mat(Color c, float metallic, float smoothness)
        {
            var m = new Material(ShaderCache.Lit) { color = c };
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Smoothness", smoothness);
            return m;
        }

        // Empty container GameObject placed in world space.
        public static GameObject Root(string name, Vector3 pos)
        {
            var g = new GameObject(name);
            g.transform.position = pos;
            return g;
        }

        public static GameObject Cube(string name, Transform parent, Vector3 pos, Vector3 scale, Color color, float metallic = 0f, float smoothness = 0.4f)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = name;
            if (parent != null) { g.transform.SetParent(parent, false); g.transform.localPosition = pos; }
            else g.transform.position = pos;
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = Mat(color, metallic, smoothness);
            return g;
        }

        public static GameObject CubeTex(string name, Transform parent, Vector3 pos, Vector3 scale, Texture2D tex, Color tint, float metallic, float smoothness, Vector2 tile)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = name;
            if (parent != null) { g.transform.SetParent(parent, false); g.transform.localPosition = pos; }
            else g.transform.position = pos;
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = ShaderCache.MakeTextured(tex, tint, metallic, smoothness, tile);
            return g;
        }

        public static GameObject Cylinder(string name, Transform parent, Vector3 pos, Vector3 scale, Color color, float metallic = 0f, float smoothness = 0.4f)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            g.name = name;
            if (parent != null) { g.transform.SetParent(parent, false); g.transform.localPosition = pos; }
            else g.transform.position = pos;
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = Mat(color, metallic, smoothness);
            return g;
        }

        // Sphere — collider removed by default (decorative).
        public static GameObject Sphere(string name, Transform parent, Vector3 pos, Vector3 scale, Color color, float metallic = 0f, float smoothness = 0.4f, bool keepCollider = false)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.name = name;
            if (parent != null) { g.transform.SetParent(parent, false); g.transform.localPosition = pos; }
            else g.transform.position = pos;
            g.transform.localScale = scale;
            if (!keepCollider) Object.Destroy(g.GetComponent<Collider>());
            g.GetComponent<Renderer>().sharedMaterial = Mat(color, metallic, smoothness);
            return g;
        }

        // Occlusion-aware floating sign. `anchor` defaults to the parent object.
        public static void Label(GameObject parent, string text, Color color, int fontSize, Vector3 offset)
        {
            var lbl = new GameObject("Label");
            lbl.transform.SetParent(parent.transform, false);
            lbl.transform.localPosition = offset;
            var w = lbl.AddComponent<WorldLabel>();
            w.text = text; w.color = color; w.fontSize = fontSize;
            w.anchor = parent.transform;
        }

        // Spawns a walking/idle human civilian.
        public static Civilian Civ(string name, Vector3 pos, Civilian.Mode mode, Color? shirt = null, Color? pants = null)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var c = go.AddComponent<Civilian>();
            c.mode = mode;
            if (shirt.HasValue) c.shirtColor = shirt.Value;
            if (pants.HasValue) c.pantsColor = pants.Value;
            return c;
        }
    }
}
