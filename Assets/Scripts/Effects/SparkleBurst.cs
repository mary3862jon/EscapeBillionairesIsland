using UnityEngine;

namespace Spoonacci
{
    // Cheerful skin-change/level-up sparkle.
    public class SparkleBurst : MonoBehaviour
    {
        public static void Spawn(Vector3 pos, Color tint)
        {
            var go = new GameObject("Sparkle");
            go.transform.position = pos;
            go.AddComponent<SparkleBurst>().Build(tint);
        }

        void Build(Color tint)
        {
            var p = gameObject.AddComponent<ParticleSystem>();
            p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = p.main;
            main.startLifetime = 1.2f;
            main.startSpeed = 1.5f;
            main.startSize = 0.18f;
            main.startColor = tint;
            main.maxParticles = 80;
            main.duration = 0.4f;
            main.loop = false;
            main.gravityModifier = -0.3f; // rise up
            var emit = p.emission;
            emit.SetBursts(new[] { new ParticleSystem.Burst(0f, 50) });
            var shape = p.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.6f;
            var rend = p.GetComponent<ParticleSystemRenderer>();
            rend.material = new Material(Shader.Find("Sprites/Default")) { color = tint };
            var fade = p.colorOverLifetime;
            fade.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(tint, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            fade.color = grad;
            p.Play();

            Destroy(gameObject, 1.8f);
        }
    }
}
