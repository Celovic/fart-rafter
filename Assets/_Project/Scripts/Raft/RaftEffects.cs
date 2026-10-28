using UnityEngine;

namespace FartRafters
{
    [RequireComponent(typeof(Raft))]
    public class RaftEffects : MonoBehaviour
    {
        public CourseTheme theme;
        [Min(0f)] public float wakePerSpeed = 9f;
        [Min(0f)] public float wakeMinSpeed = 0.6f;
        [Min(0f)] public float impactThreshold = 1.2f;
        [Min(0f)] public float impactCooldown = 0.15f;

        Raft raft;
        Buoyancy buoyancy;
        ParticleSystem wake;
        ParticleSystem splash;
        float lastImpact = -10f;

        void Start()
        {
            raft = GetComponent<Raft>();
            buoyancy = GetComponent<Buoyancy>();
            if (theme == null)
            {
                return;
            }

            wake = EffectFactory.CreateWake(transform, theme.splash);
            wake.transform.localPosition = new Vector3(0f, -0.15f, 0f);

            var holder = new GameObject("RaftSplash").transform;
            splash = EffectFactory.CreateSplash(holder, theme.splash);
        }

        void Update()
        {
            if (wake == null || raft.Body == null)
            {
                return;
            }

            Vector3 velocity = raft.Body.linearVelocity;
            float speed = new Vector2(velocity.x, velocity.z).magnitude;
            bool wet = buoyancy == null || (buoyancy.IsInWater && !buoyancy.OnIce);
            ParticleSystem.EmissionModule emission = wake.emission;
            emission.rateOverTime = wet && speed > wakeMinSpeed ? (speed - wakeMinSpeed) * wakePerSpeed : 0f;
        }

        void OnCollisionEnter(Collision collision)
        {
            float impact = collision.relativeVelocity.magnitude;
            if (impact < impactThreshold || Time.time - lastImpact < impactCooldown)
            {
                return;
            }
            lastImpact = Time.time;

            float strength = Mathf.Clamp01((impact - impactThreshold) / 5f);
            Vector3 point = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            GameAudio.Play(GameAudio.Sfx.Bump, 0.4f + strength * 0.6f, Random.Range(0.9f, 1.1f));
            GameAudio.Play(GameAudio.Sfx.Splash, 0.25f + strength * 0.5f, Random.Range(0.9f, 1.15f));
            FollowCamera.Shake(0.15f + strength * 0.4f);

            if (splash != null)
            {
                splash.transform.position = new Vector3(point.x, transform.position.y, point.z);
                splash.Emit(Mathf.RoundToInt(10f + strength * 25f));
            }
        }

        public void SplashAt(Vector3 position, int count)
        {
            if (splash == null)
            {
                return;
            }
            splash.transform.position = position;
            splash.Emit(count);
        }
    }
}
