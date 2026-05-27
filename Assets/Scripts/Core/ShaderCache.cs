using UnityEngine;

namespace Spoonacci
{
    // Bullet-proof shader resolver. URP/Lit gets stripped in builds when only used via Shader.Find at runtime.
    // We pin it via Assets/Resources/UrpLitTemplate.mat → forces inclusion → cache the shader the first time.
    // Used by every "MakeMat" helper in the project.
    public static class ShaderCache
    {
        static Shader _lit;
        static Shader _spritesDefault;

        public static Shader Lit
        {
            get
            {
                if (_lit != null) return _lit;
                // 1) Resources material — guaranteed to ship if Resources/UrpLitTemplate.mat exists
                var template = Resources.Load<Material>("UrpLitTemplate");
                if (template != null && template.shader != null)
                {
                    _lit = template.shader;
                    return _lit;
                }
                // 2) Direct Shader.Find — only works if the shader is in the build
                _lit = Shader.Find("Universal Render Pipeline/Lit");
                if (_lit != null) return _lit;
                // 3) Legacy fallback
                _lit = Shader.Find("Standard");
                if (_lit != null)
                {
                    Debug.LogWarning("[ShaderCache] URP/Lit not found — using Standard fallback.");
                    return _lit;
                }
                // 4) Last resort — anything to avoid null
                _lit = Shader.Find("Sprites/Default");
                Debug.LogError("[ShaderCache] URP/Lit + Standard both missing — using Sprites/Default. Scene will look magenta.");
                return _lit;
            }
        }

        public static Shader SpritesDefault
        {
            get
            {
                if (_spritesDefault != null) return _spritesDefault;
                _spritesDefault = Shader.Find("Sprites/Default");
                return _spritesDefault;
            }
        }

        public static Material MakeMat(Color color, float metallic, float smoothness)
        {
            var m = new Material(Lit) { color = color };
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Smoothness", smoothness);
            m.SetColor("_BaseColor", color);
            return m;
        }

        public static Material MakeTextured(Texture2D tex, Color tint, float metallic, float smoothness, Vector2? tile = null)
        {
            var m = new Material(Lit) { color = tint };
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Smoothness", smoothness);
            m.SetColor("_BaseColor", tint);
            if (tex != null)
            {
                m.mainTexture = tex;
                m.SetTexture("_BaseMap", tex);
                if (tile.HasValue)
                {
                    // URP/Lit reads _BaseMap_ST — must set scale on _BaseMap specifically
                    m.SetTextureScale("_BaseMap", tile.Value);
                    m.mainTextureScale = tile.Value;
                }
                // make sure URP doesn't strip the texture sampler
                m.EnableKeyword("_BASEMAP");
            }
            return m;
        }
    }
}
