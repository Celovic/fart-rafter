using UnityEngine;

namespace FartRafters
{
    public static class EffectFactory
    {
        public static ParticleSystem CreatePuff(Transform parent, Material material)
        {
            ParticleSystem system = Create("Puff", parent, material, ParticleSystemSimulationSpace.World);
            system.transform.localPosition = new Vector3(0f, 0.35f, -0.45f);
            system.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            ParticleSystem.MainModule main = system.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 4.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.8f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.72f, 0.78f, 0.3f, 0.85f), new Color(0.55f, 0.62f, 0.22f, 0.85f));
            main.gravityModifier = -0.05f;
            main.maxParticles = 220;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 18f;
            shape.radius = 0.08f;

            ParticleSystem.SizeOverLifetimeModule size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.5f, 1f, 2f));

            ParticleSystem.RotationOverLifetimeModule rotation = system.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-2f, 2f);

            ParticleSystem.LimitVelocityOverLifetimeModule drag = system.limitVelocityOverLifetime;
            drag.enabled = true;
            drag.limit = 0.6f;
            drag.dampen = 0.15f;

            SetFade(system, 0.9f);
            return system;
        }

        public static ParticleSystem CreateSplash(Transform parent, Material material)
        {
            ParticleSystem system = Create("Splash", parent, material, ParticleSystemSimulationSpace.World);
            ParticleSystem.MainModule main = system.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.55f);
            main.startColor = new Color(0.92f, 0.97f, 1f, 0.9f);
            main.gravityModifier = 1.4f;
            main.maxParticles = 200;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.4f;
            system.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

            ParticleSystem.SizeOverLifetimeModule size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.4f));
            SetFade(system, 1f);
            return system;
        }

        public static ParticleSystem CreateWake(Transform parent, Material material)
        {
            ParticleSystem system = Create("Wake", parent, material, ParticleSystemSimulationSpace.World);
            ParticleSystem.MainModule main = system.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 1.4f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            main.startColor = new Color(1f, 1f, 1f, 0.65f);
            main.maxParticles = 300;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(3.2f, 0.05f, 3.2f);

            ParticleSystem.SizeOverLifetimeModule size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 1.8f));
            SetFade(system, 0.7f);
            return system;
        }

        public static ParticleSystem CreateConfetti(Transform parent, Material material)
        {
            ParticleSystem system = Create("Confetti", parent, material, ParticleSystemSimulationSpace.World);
            ParticleSystem.MainModule main = system.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.4f, 2.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 11f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            var colors = new Gradient();
            colors.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.3f, 0.35f), 0f), new GradientColorKey(new Color(1f, 0.82f, 0.2f), 0.25f),
                    new GradientColorKey(new Color(0.35f, 0.85f, 0.4f), 0.5f), new GradientColorKey(new Color(0.3f, 0.6f, 1f), 0.75f),
                    new GradientColorKey(new Color(0.85f, 0.45f, 1f), 1f)
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            main.startColor = new ParticleSystem.MinMaxGradient(colors) { mode = ParticleSystemGradientMode.RandomColor };
            main.gravityModifier = 0.6f;
            main.maxParticles = 400;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 35f;
            shape.radius = 0.5f;
            system.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

            ParticleSystem.RotationOverLifetimeModule rotation = system.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-6f, 6f);

            ParticleSystem.LimitVelocityOverLifetimeModule drag = system.limitVelocityOverLifetime;
            drag.enabled = true;
            drag.limit = 1.5f;
            drag.dampen = 0.08f;
            SetFade(system, 1f);
            return system;
        }

        public static void AddCurrentStreaks(Transform parent, Vector3 boxSize, Vector3 velocity, Material material)
        {
            ParticleSystem system = Create("Streaks", parent, material, ParticleSystemSimulationSpace.World);
            ParticleSystem.MainModule main = system.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = 1.6f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.2f);
            main.startColor = new Color(1f, 1f, 1f, 0.75f);
            main.maxParticles = 200;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = Mathf.Clamp(boxSize.x * boxSize.z * 0.08f, 8f, 40f);

            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(boxSize.x, 0.05f, boxSize.z);

            ParticleSystem.VelocityOverLifetimeModule motion = system.velocityOverLifetime;
            motion.enabled = true;
            motion.space = ParticleSystemSimulationSpace.World;
            motion.x = new ParticleSystem.MinMaxCurve(velocity.x * 1.5f);
            motion.y = new ParticleSystem.MinMaxCurve(0f);
            motion.z = new ParticleSystem.MinMaxCurve(velocity.z * 1.5f);

            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.25f;
            renderer.lengthScale = 2f;

            SetFade(system, 1f, true);
            system.Play();
        }

        static ParticleSystem Create(string name, Transform parent, Material material, ParticleSystemSimulationSpace space)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = system.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = space;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            system.Play();
            return system;
        }

        static void SetFade(ParticleSystem system, float startAlpha, bool fadeIn = false)
        {
            ParticleSystem.ColorOverLifetimeModule color = system.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                fadeIn
                    ? new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(startAlpha, 0.2f), new GradientAlphaKey(startAlpha, 0.8f), new GradientAlphaKey(0f, 1f) }
                    : new[] { new GradientAlphaKey(startAlpha, 0f), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;
        }
    }
}
