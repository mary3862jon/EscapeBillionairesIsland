using UnityEngine;

namespace Spoonacci
{
    // Spawn dust + spark particle burst at bonk impact. Self-destructs after lifetime.
    public class BonkBurst : MonoBehaviour
    {
        public static void Spawn(Vector3 pos)
        {
            var go = new GameObject("BonkBurst");
            go.transform.position = pos;
            go.AddComponent<BonkBurst>().Build();
        }

        void Build()
        {
            // dust cloud
            var dust = new GameObject("Dust");
            dust.transform.SetParent(transform, false);
            var pDust = dust.AddComponent<ParticleSystem>();
            pDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var mainD = pDust.main;
            mainD.startLifetime = 0.7f;
            mainD.startSpeed = 2.5f;
            mainD.startSize = 0.5f;
            mainD.startColor = new Color(0.7f, 0.55f, 0.35f, 0.7f);
            mainD.maxParticles = 60;
            mainD.duration = 0.15f;
            mainD.loop = false;
            var emit = pDust.emission;
            emit.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });
            var shape = pDust.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.3f;
            var rend = pDust.GetComponent<ParticleSystemRenderer>();
            rend.material = new Material(ShaderCache.SpritesDefault) { color = new Color(0.7f, 0.55f, 0.35f, 0.7f) };
            var coCol = pDust.colorOverLifetime;
            coCol.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(0.7f, 0.55f, 0.35f), 0f), new GradientColorKey(new Color(0.5f, 0.4f, 0.25f), 1f) },
                new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            coCol.color = grad;

            // stars/sparks
            pDust.Play();

            var stars = new GameObject("Stars");
            stars.transform.SetParent(transform, false);
            var pStars = stars.AddComponent<ParticleSystem>();
            pStars.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var mainS = pStars.main;
            mainS.startLifetime = 0.6f;
            mainS.startSpeed = 5f;
            mainS.startSize = 0.18f;
            mainS.startColor = new Color(1f, 0.95f, 0.3f, 1f);
            mainS.maxParticles = 30;
            mainS.duration = 0.1f;
            mainS.loop = false;
            mainS.gravityModifier = 1.5f;
            var emitS = pStars.emission;
            emitS.SetBursts(new[] { new ParticleSystem.Burst(0f, 18) });
            var shapeS = pStars.shape;
            shapeS.shapeType = ParticleSystemShapeType.Sphere;
            shapeS.radius = 0.15f;
            var rendS = pStars.GetComponent<ParticleSystemRenderer>();
            rendS.material = new Material(ShaderCache.SpritesDefault) { color = new Color(1f, 0.9f, 0.3f, 1f) };
            pStars.Play();

            Destroy(gameObject, 1.6f);
        }
    }
}
