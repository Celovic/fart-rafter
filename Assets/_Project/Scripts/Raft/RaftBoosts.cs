using System;
using UnityEngine;

namespace FartRafters
{
    public class RaftBoosts : MonoBehaviour
    {
        [Min(0f)] public float beansDuration = 11f;
        [Min(0f)] public float pastirmaDuration = 9f;
        [Min(0f)] public float onionDuration = 5f;
        [Min(0f)] public float pepperDuration = 4f;
        [Min(0f)] public float eggDuration = 9f;
        [Min(0f)] public float bubbleDuration = 6f;
        [Min(0f)] public float magnetDuration = 8f;
        [Min(1f)] public float magnetRadius = 14f;
        [Min(0.1f)] public float magnetSpeed = 9f;
        [Min(1f)] public float pepperBoost = 2f;
        [Min(1f)] public float shieldHealth = 40f;
        [Min(1f)] public float thrustBoost = 1.5f;
        [Min(1f)] public float gasBoost = 1.5f;
        [Min(1f)] public float refillBoost = 1.6f;

        public static readonly int KindCount = System.Enum.GetValues(typeof(PickupKind)).Length;

        readonly float[] remaining = new float[KindCount];
        float shield;

        public int RefillVersion { get; private set; }
        public float Shield => IsActive(PickupKind.Egg) ? shield : 0f;
        public float ShieldNormalized => shieldHealth > 0f ? Shield / shieldHealth : 0f;

        public event Action<PickupKind> Collected;

        public float ThrustMultiplier => (IsActive(PickupKind.Pastirma) ? thrustBoost : 1f) * (IsActive(PickupKind.Pepper) ? pepperBoost : 1f);
        public float GasMultiplier => IsActive(PickupKind.Beans) ? gasBoost : 1f;
        public float RefillMultiplier => IsActive(PickupKind.Beans) ? refillBoost : 1f;
        public bool InfiniteGas => IsActive(PickupKind.Onion);
        public bool Invincible => IsActive(PickupKind.Bubble);

        public bool IsActive(PickupKind kind)
        {
            return remaining[(int)kind] > 0f;
        }

        public float Remaining(PickupKind kind)
        {
            return remaining[(int)kind];
        }

        public float Duration(PickupKind kind)
        {
            switch (kind)
            {
                case PickupKind.Beans:
                    return beansDuration;
                case PickupKind.Pastirma:
                    return pastirmaDuration;
                case PickupKind.Onion:
                    return onionDuration;
                case PickupKind.Pepper:
                    return pepperDuration;
                case PickupKind.Egg:
                    return eggDuration;
                case PickupKind.Bubble:
                    return bubbleDuration;
                case PickupKind.Magnet:
                    return magnetDuration;
                default:
                    return 0f;
            }
        }

        public void Apply(PickupKind kind)
        {
            remaining[(int)kind] = Duration(kind);
            if (kind == PickupKind.Chickpea)
            {
                RefillVersion++;
            }
            if (kind == PickupKind.Egg)
            {
                shield = shieldHealth;
            }
            if (kind == PickupKind.Repair)
            {
                RaftHealth health = GetComponent<RaftHealth>();
                if (health != null)
                {
                    health.Repair();
                }
            }
            Collected?.Invoke(kind);
        }

        public float Absorb(float damage)
        {
            if (!IsActive(PickupKind.Egg))
            {
                return damage;
            }
            float absorbed = Mathf.Min(shield, damage);
            shield -= absorbed;
            if (shield <= 0f)
            {
                remaining[(int)PickupKind.Egg] = 0f;
            }
            return damage - absorbed;
        }

        public bool BlockCatch()
        {
            if (!IsActive(PickupKind.Egg))
            {
                return false;
            }
            shield = 0f;
            remaining[(int)PickupKind.Egg] = 0f;
            return true;
        }

        void Update()
        {
            for (int i = 0; i < remaining.Length; i++)
            {
                remaining[i] = Mathf.Max(0f, remaining[i] - Time.deltaTime);
            }
            if (IsActive(PickupKind.Magnet))
            {
                Attract();
            }
        }

        void Attract()
        {
            Vector3 center = transform.position;
            float step = magnetSpeed * Time.deltaTime;
            foreach (Pickup pickup in Pickup.Active)
            {
                Vector3 offset = center - pickup.transform.position;
                offset.y = 0f;
                float distance = offset.magnitude;
                if (distance > magnetRadius || distance < 0.1f)
                {
                    continue;
                }
                pickup.Pull(offset / distance * Mathf.Min(distance, step));
            }
        }
    }
}
