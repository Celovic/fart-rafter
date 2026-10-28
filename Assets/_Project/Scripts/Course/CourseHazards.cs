using System;
using System.Collections.Generic;
using UnityEngine;

namespace FartRafters
{
    public static class CourseNotices
    {
        public static event Action<string, Color> Notice;

        public static void Post(string text, Color color)
        {
            Notice?.Invoke(text, color);
        }
    }

    public class SoftHazard : MonoBehaviour
    {
        static readonly List<SoftHazard> active = new List<SoftHazard>();

        [Min(0.5f)] public float radius = 3f;
        [Range(0f, 1f)] public float weight = 1f;

        public static IReadOnlyList<SoftHazard> Active => active;
        public IcicleDrop Drop { get; private set; }
        public Geyser Spout { get; private set; }

        void OnEnable()
        {
            Drop = GetComponent<IcicleDrop>();
            Spout = GetComponent<Geyser>();
            active.Add(this);
        }

        void OnDisable()
        {
            active.Remove(this);
        }
    }

    public class Geyser : MonoBehaviour
    {
        public static Func<float> SharedClock;

        [Min(1f)] public float period = 4f;
        public float phase;
        [Min(0.5f)] public float radius = 2.4f;
        [Min(0f)] public float lift = 5.5f;
        [Min(0f)] public float sideways = 3f;
        public ParticleSystem bubbles;
        public ParticleSystem burst;
        public Transform dome;

        const float EruptLength = 0.7f;
        bool erupting;

        float Clock => SharedClock != null ? SharedClock() : Time.time;

        public float Warmup
        {
            get
            {
                float t = Mathf.Repeat(Clock + phase, period);
                return t > period - 1.2f ? (t - (period - 1.2f)) / 1.2f : 0f;
            }
        }

        public bool Erupting => Mathf.Repeat(Clock + phase, period) < EruptLength;

        public bool EruptingAt(float ahead)
        {
            return Mathf.Repeat(Clock + phase + ahead, period) < EruptLength + 0.3f;
        }

        void Update()
        {
            float t = Mathf.Repeat(Clock + phase, period);
            bool now = t < EruptLength;
            if (bubbles != null)
            {
                ParticleSystem.EmissionModule emission = bubbles.emission;
                emission.rateOverTime = now ? 160f : 25f + Warmup * 90f;
            }
            if (dome != null)
            {
                float scale = now ? Mathf.Lerp(1.6f, 0.4f, t / EruptLength) : 0.4f + Warmup * 0.5f;
                dome.localScale = new Vector3(radius * scale, 0.25f + Warmup * 0.6f, radius * scale);
            }
            if (now && !erupting)
            {
                erupting = true;
                if (burst != null)
                {
                    burst.Emit(50);
                }
                Raft raft = Raft.Current;
                if (raft != null && raft.Body != null && !raft.Body.isKinematic)
                {
                    Vector3 offset = raft.Body.position - transform.position;
                    offset.y = 0f;
                    float distance = offset.magnitude;
                    if (distance < radius + 1.4f)
                    {
                        Vector3 side = distance > 0.01f ? offset / distance : UnityEngine.Random.insideUnitSphere;
                        side.y = 0f;
                        raft.Body.AddForce(Vector3.up * lift + side.normalized * sideways, ForceMode.VelocityChange);
                        raft.Body.AddTorque(UnityEngine.Random.insideUnitSphere * 1.5f, ForceMode.VelocityChange);
                        RaftHealth health = raft.GetComponent<RaftHealth>();
                        if (health != null)
                        {
                            health.Jolt(2.4f);
                        }
                        GameAudio.Play(GameAudio.Sfx.Splash, 0.9f, 0.8f);
                        CourseNotices.Post("Gayzer!", new Color(0.7f, 0.9f, 1f));
                    }
                }
            }
            else if (!now)
            {
                erupting = false;
            }
        }
    }

    public class Barrel : MonoBehaviour
    {
        public static event Action<int> Exploded;

        public int index;
        [Min(0f)] public float damage = 16f;
        [Min(0f)] public float push = 6f;
        public ParticleSystem burst;
        public Transform visual;

        bool done;

        void OnCollisionEnter(Collision collision)
        {
            Raft raft = collision.rigidbody != null ? collision.rigidbody.GetComponent<Raft>() : null;
            if (raft == null)
            {
                return;
            }
            Explode(raft, true);
        }

        public void Explode(Raft raft, bool authoritative)
        {
            if (done)
            {
                return;
            }
            done = true;
            if (burst != null)
            {
                burst.transform.SetParent(null, true);
                burst.Emit(70);
                Destroy(burst.gameObject, 3f);
            }
            GameAudio.Play(GameAudio.Sfx.Crack, 1f, 0.6f);
            GameAudio.Play(GameAudio.Sfx.Splash, 0.8f, 0.7f);
            FollowCamera.Shake(0.5f);
            CourseNotices.Post("Varil patladı!", new Color(1f, 0.6f, 0.3f));
            if (authoritative && raft != null && raft.Body != null && !raft.Body.isKinematic)
            {
                Vector3 away = raft.Body.position - transform.position;
                away.y = 0f;
                away = away.sqrMagnitude > 0.01f ? away.normalized : raft.transform.forward;
                raft.Body.AddForce(away * push + Vector3.up * (push * 0.5f), ForceMode.VelocityChange);
                raft.Body.AddTorque(UnityEngine.Random.insideUnitSphere * 2f, ForceMode.VelocityChange);
                RaftHealth health = raft.GetComponent<RaftHealth>();
                if (health != null)
                {
                    health.Apply(damage, true);
                    health.Jolt(3.2f);
                }
                Exploded?.Invoke(index);
            }
            foreach (Collider collider in GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }
            SoftHazard hazard = GetComponent<SoftHazard>();
            if (hazard != null)
            {
                hazard.enabled = false;
            }
            if (visual != null)
            {
                visual.gameObject.SetActive(false);
            }
        }
    }

    public class LaunchRamp : MonoBehaviour
    {
        [Min(0f)] public float boost = 3.5f;
        [Min(0f)] public float lift = 3f;

        float lastLaunch = -10f;

        void OnTriggerEnter(Collider other)
        {
            Raft raft = other.attachedRigidbody != null ? other.attachedRigidbody.GetComponent<Raft>() : null;
            if (raft == null || raft.Body.isKinematic || Time.time - lastLaunch < 2f)
            {
                return;
            }
            Vector3 forward = transform.forward;
            float along = Vector3.Dot(raft.Body.linearVelocity, forward);
            if (along < 1f)
            {
                return;
            }
            lastLaunch = Time.time;
            raft.Body.AddForce(forward * boost + Vector3.up * lift, ForceMode.VelocityChange);
            GameAudio.Play(GameAudio.Sfx.Wheeze, 0.7f, 1.4f);
            CourseNotices.Post("Uçuyoruz!", new Color(1f, 0.95f, 0.5f));
        }
    }

    public class IcicleDrop : MonoBehaviour
    {
        public static Func<float> SharedClock;

        [Min(1f)] public float period = 6f;
        public float phase;
        [Min(0.5f)] public float radius = 2.2f;
        [Min(0f)] public float damage = 14f;
        [Min(0f)] public float height = 9f;
        public Transform icicle;
        public Transform shadow;
        public ParticleSystem splash;

        const float Warn = 1.1f;
        const float Fall = 0.45f;
        bool landed;

        float Clock => SharedClock != null ? SharedClock() : Time.time;

        public bool StrikingAt(float ahead)
        {
            float t = Mathf.Repeat(Clock + phase + ahead, period);
            float strike = Warn + Fall;
            return t > strike - 0.45f && t < strike + 0.35f;
        }

        void Update()
        {
            float t = Mathf.Repeat(Clock + phase, period);
            float warn = Mathf.Clamp01(t / Warn);
            float fall = Mathf.Clamp01((t - Warn) / Fall);
            bool falling = t >= Warn && t < Warn + Fall;
            bool down = t >= Warn + Fall && t < Warn + Fall + 0.6f;
            if (shadow != null)
            {
                shadow.gameObject.SetActive(t < Warn + Fall);
                float size = radius * 2f * Mathf.Lerp(0.3f, 1f, warn);
                shadow.localScale = new Vector3(size, 0.02f, size);
            }
            if (icicle != null)
            {
                icicle.gameObject.SetActive(t < Warn + Fall + 0.6f);
                float y = falling ? Mathf.Lerp(height, 0f, fall * fall) : down ? Mathf.Lerp(0f, -2.5f, (t - Warn - Fall) / 0.6f) : height;
                icicle.localPosition = new Vector3(0f, y, 0f);
            }
            if (down && !landed)
            {
                landed = true;
                if (splash != null)
                {
                    splash.Emit(35);
                }
                GameAudio.PlayAt(GameAudio.Sfx.Splash, transform.position, 0.6f, 1.1f);
                Raft raft = Raft.Current;
                if (raft != null && raft.Body != null && !raft.Body.isKinematic)
                {
                    Vector3 offset = raft.Body.position - transform.position;
                    offset.y = 0f;
                    if (offset.magnitude < radius + 1.2f)
                    {
                        RaftHealth health = raft.GetComponent<RaftHealth>();
                        if (health != null)
                        {
                            health.Apply(damage, true);
                            health.Jolt(2.6f);
                        }
                        raft.Body.AddForce(Vector3.down * 2f + (offset.sqrMagnitude > 0.01f ? offset.normalized : Vector3.forward) * 2.5f, ForceMode.VelocityChange);
                        GameAudio.Play(GameAudio.Sfx.Crack, 0.675f, 1.2f);
                        FollowCamera.Shake(0.4f);
                        CourseNotices.Post((CourseTheme.Active != null && !string.IsNullOrEmpty(CourseTheme.Active.dropName) ? CourseTheme.Active.dropName : "Buz sarkıtı") + "!", new Color(0.75f, 0.9f, 1f));
                    }
                }
            }
            else if (!down)
            {
                landed = false;
            }
        }
    }
}
