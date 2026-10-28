using System;
using UnityEngine;

namespace FartRafters
{
    [RequireComponent(typeof(Raft))]
    public class RaftHealth : MonoBehaviour
    {
        public CourseManager course;
        [Min(1f)] public float maxHealth = 100f;
        [Min(0f)] public float damageMultiplier = 1f;

        public const float CollisionDamageScale = 0.9f;
        [Min(0f)] public float impactThreshold = 1.6f;
        [Min(0f)] public float damagePerImpact = 7f;
        [Min(0f)] public float graceAfterRespawn = 1.5f;
        [Min(0f)] public float thornReportInterval = 0.4f;

        Raft raft;
        float lastDamage = -10f;
        float graceUntil;
        float lastThornReport = -10f;

        public float Health { get; private set; }
        public float Normalized => Health / maxHealth;
        public bool Invulnerable => Time.time < graceUntil;

        public event Action<float> Damaged;
        public event Action Broken;
        public event Action Caught;
        public event Action ShieldSaved;
        public event Action<float> HardImpact;

        void Awake()
        {
            raft = GetComponent<Raft>();
            Health = maxHealth;
        }

        void OnEnable()
        {
            if (course != null)
            {
                course.Respawned += OnRespawned;
            }
        }

        void OnDisable()
        {
            if (course != null)
            {
                course.Respawned -= OnRespawned;
            }
        }

        public event Action Repaired;

        public void Repair()
        {
            Health = maxHealth;
            Repaired?.Invoke();
        }

        public event Action Shocked;

        public void Jolt(float deltaV)
        {
            HardImpact?.Invoke(deltaV);
        }

        public void Shock(float seconds)
        {
            if (course == null || course.Finished || Invulnerable)
            {
                return;
            }
            raft.Stun(seconds);
            Shocked?.Invoke();
        }

        void OnCollisionEnter(Collision collision)
        {
            MovingObstacle mover = collision.collider.GetComponentInParent<MovingObstacle>();
            if (mover != null && mover.kind == ObstacleKind.Eel)
            {
                Shock(2f);
                HardImpact?.Invoke(0.5f);
                return;
            }
            if (mover != null && mover.kind == ObstacleKind.Predator)
            {
                TryCatch();
                return;
            }

            float deltaV = collision.impulse.magnitude / Mathf.Max(1f, raft.Body.mass);
            HardImpact?.Invoke(deltaV);
            if (deltaV < impactThreshold)
            {
                return;
            }
            var source = collision.collider.GetComponentInParent<DamageSource>();
            float multiplier = source != null ? source.multiplier : 1f;
            Apply((deltaV - impactThreshold) * damagePerImpact * multiplier, true);
        }

        void OnCollisionStay(Collision collision)
        {
            var thorn = collision.collider.GetComponentInParent<ThornWall>();
            if (thorn == null)
            {
                return;
            }
            bool report = Time.time - lastThornReport >= thornReportInterval;
            if (report)
            {
                lastThornReport = Time.time;
            }
            Apply(thorn.damagePerSecond * Time.fixedDeltaTime, report);
        }

        public bool TryCatch()
        {
            if (raft.Boosts != null && raft.Boosts.Invincible)
            {
                return false;
            }
            if (Invulnerable || course == null || course.Finished)
            {
                return false;
            }
            if (raft.Boosts != null && raft.Boosts.BlockCatch())
            {
                graceUntil = Time.time + graceAfterRespawn;
                ShieldSaved?.Invoke();
                return false;
            }
            Caught?.Invoke();
            course.Respawn();
            return true;
        }

        public void SetNetworkHealth(float value)
        {
            Health = Mathf.Clamp(value, 0f, maxHealth);
            lastDamage = Time.time;
        }

        public void RaiseNetwork(NetEvent kind, float amount)
        {
            switch (kind)
            {
                case NetEvent.Damaged:
                    Damaged?.Invoke(amount);
                    break;
                case NetEvent.Broken:
                    Broken?.Invoke();
                    break;
                case NetEvent.Caught:
                    Caught?.Invoke();
                    break;
                case NetEvent.ShieldSaved:
                    ShieldSaved?.Invoke();
                    break;
                case NetEvent.Shocked:
                    Shocked?.Invoke();
                    break;
            }
        }

        public void Apply(float amount, bool report)
        {
            if (amount <= 0f || Invulnerable || course == null || course.Finished)
            {
                return;
            }
            float damage = amount * damageMultiplier * CollisionDamageScale;
            if (raft.Boosts != null && raft.Boosts.Invincible)
            {
                return;
            }
            if (raft.Boosts != null)
            {
                damage = raft.Boosts.Absorb(damage);
                if (damage <= 0f)
                {
                    return;
                }
            }
            Health -= damage;
            lastDamage = Time.time;
            if (report)
            {
                Damaged?.Invoke(damage);
            }
            if (Health <= 0f)
            {
                Health = 0f;
                Broken?.Invoke();
                course.Respawn();
            }
        }

        void OnRespawned()
        {
            Health = maxHealth;
            graceUntil = Time.time + graceAfterRespawn;
        }
    }
}
