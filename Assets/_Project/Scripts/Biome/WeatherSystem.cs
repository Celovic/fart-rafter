using UnityEngine;
using UnityEngine.Rendering;

namespace FartRafters
{
    public class WeatherSystem : MonoBehaviour
    {
        const float GustPeriod = 9f;
        const float GustLength = 3f;
        const float GustAcceleration = 2.6f;
        const float FogDensity = 0.03f;
        const float RainFogDensity = 0.016f;
        const float StormFogDensity = 0.021f;

        Weather weather;
        CourseManager course;
        ParticleSystem rain;
        Light sun;
        float baseSun;
        float flash;
        float nextFlash;
        float thunderAt = -1f;
        bool gusting;

        public static WeatherSystem Current { get; private set; }
        public Weather Kind => weather;

        public static WeatherSystem Apply(GameObject host, Weather weather, CourseTheme theme, CourseManager course)
        {
            if (weather == Weather.Clear)
            {
                return null;
            }
            var system = host.AddComponent<WeatherSystem>();
            system.weather = weather;
            system.course = course;
            system.Setup(theme);
            return system;
        }

        void OnEnable()
        {
            Current = this;
        }

        void OnDisable()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        float Clock => MovingObstacle.SharedClock != null ? MovingObstacle.SharedClock() : Time.time;

        void Setup(CourseTheme theme)
        {
            sun = BiomeEnvironment.Sun();
            baseSun = sun != null ? sun.intensity : 1f;
            RenderSettings.fog = true;
            switch (weather)
            {
                case Weather.Rain:
                    Darken(0.75f, RainFogDensity, new Color(0.62f, 0.68f, 0.74f), 0.45f);
                    rain = CreateRain(transform, theme != null ? theme.splash : null, GraphicsQuality.Level == QualityLevel.Low ? 260f : 520f);
                    break;
                case Weather.Fog:
                    Darken(0.85f, FogDensity, new Color(0.8f, 0.83f, 0.86f), 0.8f);
                    break;
                case Weather.Storm:
                    Darken(0.5f, StormFogDensity, new Color(0.36f, 0.4f, 0.48f), 0.6f);
                    rain = CreateRain(transform, theme != null ? theme.splash : null, GraphicsQuality.Level == QualityLevel.Low ? 400f : 900f);
                    nextFlash = Time.time + Random.Range(4f, 8f);
                    break;
            }
        }

        void Darken(float sunScale, float density, Color fog, float blend)
        {
            if (sun != null)
            {
                baseSun *= sunScale;
                sun.intensity = baseSun;
            }
            RenderSettings.fogDensity = Mathf.Max(RenderSettings.fogDensity, density);
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, fog, blend);
        }

        public float Gust
        {
            get
            {
                if (weather != Weather.Storm)
                {
                    return 0f;
                }
                float t = Mathf.Repeat(Clock, GustPeriod);
                return t < GustLength ? Mathf.Sin(t / GustLength * Mathf.PI) : 0f;
            }
        }

        public Vector3 WindDirection
        {
            get
            {
                float angle = Clock * 0.11f;
                return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            }
        }

        void FixedUpdate()
        {
            if (weather != Weather.Storm || OnlineGame.IsClient || course == null || !course.Running || course.Finished)
            {
                return;
            }
            Raft raft = Raft.Current;
            if (raft == null || raft.Body == null || raft.Body.isKinematic)
            {
                return;
            }
            float gust = Gust;
            if (gust > 0f)
            {
                raft.Body.AddForce(WindDirection * (GustAcceleration * gust), ForceMode.Acceleration);
            }
        }

        void Update()
        {
            if (weather == Weather.Storm)
            {
                bool gustNow = Gust > 0.05f;
                if (gustNow && !gusting && course != null && course.Running && !course.Finished)
                {
                    CourseNotices.Post("Rüzgar!", new Color(0.8f, 0.9f, 1f));
                }
                gusting = gustNow;

                if (Time.time >= nextFlash)
                {
                    flash = 1f;
                    nextFlash = Time.time + Random.Range(7f, 14f);
                    thunderAt = Time.time + Random.Range(0.4f, 1.2f);
                }
                if (thunderAt > 0f && Time.time >= thunderAt)
                {
                    thunderAt = -1f;
                    GameAudio.Play(GameAudio.Sfx.Crack, 0.3f, 0.35f);
                }
                flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime * 3.5f);
                if (sun != null)
                {
                    sun.intensity = baseSun + flash * 2.4f;
                }
            }
        }

        void LateUpdate()
        {
            Camera view = Camera.main;
            if (rain != null && view != null)
            {
                Vector3 forward = view.transform.forward;
                forward.y = 0f;
                rain.transform.position = view.transform.position + forward.normalized * 10f + Vector3.up * 12f;
            }
        }

        static ParticleSystem CreateRain(Transform parent, Material material, float rate)
        {
            var go = new GameObject("Rain");
            go.transform.SetParent(parent, false);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = system.main;
            main.loop = true;
            main.startLifetime = 1.3f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.05f);
            main.startColor = new Color(0.8f, 0.88f, 1f, 0.55f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 2000;
            main.playOnAwake = true;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = rate;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(40f, 1f, 40f);

            ParticleSystem.VelocityOverLifetimeModule motion = system.velocityOverLifetime;
            motion.enabled = true;
            motion.space = ParticleSystemSimulationSpace.World;
            motion.x = new ParticleSystem.MinMaxCurve(-0.6f, 0.6f);
            motion.y = new ParticleSystem.MinMaxCurve(-15f, -12f);
            motion.z = new ParticleSystem.MinMaxCurve(-0.6f, 0.6f);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.05f;
            renderer.lengthScale = 1f;
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            system.Play();
            return system;
        }
    }
}
