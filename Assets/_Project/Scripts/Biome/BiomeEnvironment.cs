using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FartRafters
{
    public class BiomeEnvironment : MonoBehaviour
    {
        public CourseTheme theme;

        AudioSource ambience;
        CourseManager course;
        float ambienceLevel;
        ParticleSystem snow;
        readonly List<MenuJumper> jumpers = new List<MenuJumper>();

        public static void ApplyLook(CourseTheme theme)
        {
            if (theme.skybox != null)
            {
                RenderSettings.skybox = theme.skybox;
            }
            RenderSettings.fogColor = theme.fogColor;
            RenderSettings.fogDensity = theme.fogDensity;
            foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type == LightType.Directional)
                {
                    light.color = theme.sunColor;
                    light.intensity = theme.sunIntensity;
                    light.transform.rotation = Quaternion.Euler(theme.sunEuler);
                }
            }
            if (theme.postFx != null)
            {
                foreach (Volume volume in FindObjectsByType<Volume>(FindObjectsSortMode.None))
                {
                    if (volume.isGlobal)
                    {
                        volume.sharedProfile = theme.postFx;
                    }
                }
            }
        }

        public static Light Sun()
        {
            foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type == LightType.Directional)
                {
                    return light;
                }
            }
            return null;
        }

        public static Volume GlobalVolume()
        {
            foreach (Volume volume in FindObjectsByType<Volume>(FindObjectsSortMode.None))
            {
                if (volume.isGlobal)
                {
                    return volume;
                }
            }
            return null;
        }

        public static BiomeEnvironment Apply(CourseTheme theme, GameObject host)
        {
            if (theme == null)
            {
                return null;
            }
            ApplyLook(theme);
            var environment = host.AddComponent<BiomeEnvironment>();
            environment.theme = theme;
            return environment;
        }

        void Start()
        {
            if (theme.ambience != null)
            {
                ambience = gameObject.AddComponent<AudioSource>();
                ambience.clip = theme.ambience;
                ambience.loop = true;
                ambience.playOnAwake = false;
                ambience.spatialBlend = 0f;
                ambience.volume = 0f;
                ambience.Play();
                course = FindAnyObjectByType<CourseManager>();
            }
            if (theme.snow && theme.snowMaterial != null && GraphicsQuality.Level != QualityLevel.Low)
            {
                snow = CreateSnow(transform, theme.snowMaterial, GraphicsQuality.Level == QualityLevel.High ? 260f : 140f);
            }
            if (theme.jumperMesh != null && theme.openSide != 0)
            {
                ParticleSystem splash = EffectFactory.CreateSplash(transform, theme.splash);
                for (int i = 0; i < 3; i++)
                {
                    GameObject dolphin = CourseBuilder.CreateSculpt("Dolphin", transform, theme.jumperMesh, theme.sculptMaterial, Vector3.zero, Vector3.one * (1f + i * 0.15f));
                    var jumper = dolphin.AddComponent<MenuJumper>();
                    jumper.radius = 8f;
                    jumper.height = 1.5f + i * 0.3f;
                    jumper.duration = 1.3f;
                    jumper.length = 4.5f;
                    jumper.splash = splash;
                    jumpers.Add(jumper);
                }
            }
        }

        void LateUpdate()
        {
            if (ambience != null)
            {
                bool paused = Time.timeScale < 0.01f;
                bool finished = course != null && course.Finished;
                ambience.mute = !GameAudio.MusicOn;
                if (paused && ambience.isPlaying)
                {
                    ambience.Pause();
                }
                else if (!paused && !ambience.isPlaying && !finished)
                {
                    ambience.UnPause();
                }
                float target = finished ? 0f : theme.ambienceVolume;
                ambienceLevel = Mathf.MoveTowards(ambienceLevel, target, Time.unscaledDeltaTime * 0.25f);
                ambience.volume = ambienceLevel;
                if (finished && ambienceLevel <= 0f && ambience.isPlaying)
                {
                    ambience.Stop();
                }
            }
            Camera view = Camera.main;
            if (snow != null && view != null)
            {
                Vector3 forward = view.transform.forward;
                forward.y = 0f;
                snow.transform.position = view.transform.position + forward.normalized * 12f + Vector3.up * 9f;
            }
            Raft raft = Raft.Current;
            if (jumpers.Count > 0 && raft != null)
            {
                Vector3 heading = raft.Body != null && raft.Body.linearVelocity.sqrMagnitude > 1f ? raft.Body.linearVelocity : raft.transform.forward;
                heading.y = 0f;
                heading.Normalize();
                Vector3 side = Vector3.Cross(Vector3.up, heading) * theme.openSide;
                for (int i = 0; i < jumpers.Count; i++)
                {
                    jumpers[i].center = raft.transform.position + side * (24f + i * 7f) + heading * (10f + i * 9f);
                }
            }
        }

        static ParticleSystem CreateSnow(Transform parent, Material material, float rate)
        {
            var go = new GameObject("Snow");
            go.transform.SetParent(parent, false);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = system.main;
            main.loop = true;
            main.startLifetime = 7f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            main.startColor = new Color(1f, 1f, 1f, 0.9f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 1200;
            main.playOnAwake = true;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = rate;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(46f, 1f, 46f);

            ParticleSystem.VelocityOverLifetimeModule motion = system.velocityOverLifetime;
            motion.enabled = true;
            motion.space = ParticleSystemSimulationSpace.World;
            motion.x = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);
            motion.y = new ParticleSystem.MinMaxCurve(-1.6f, -1.1f);
            motion.z = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            system.Play();
            return system;
        }
    }
}
