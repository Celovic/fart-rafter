using UnityEngine;

namespace FartRafters
{
    [RequireComponent(typeof(Rigidbody))]
    public class Raft : MonoBehaviour
    {
        public RaftTuning tuning;
        public Rider[] riders;
        public Vector3 centerOfMassOffset = new Vector3(0f, -0.2f, 0f);

        public Rigidbody Body { get; private set; }
        public float ThrustScale { get; private set; } = 1f;
        public float DragScale { get; private set; } = 1f;
        public float TurnDragScale { get; private set; } = 1f;
        public float DamageScale { get; private set; } = 1f;
        public int AccelerationLevel { get; private set; } = 3;
        public float Momentum { get; private set; } = 1f;
        public float PropulsionScale => ThrustScale * Momentum;
        public float MaxMomentum => 1f + MomentumPerLevel * AccelerationLevel;

        public const float MomentumPerLevel = 0.07f;
        const float MomentumBuildPerLevel = 0.025f;
        const float MomentumDecay = 0.15f;
        const float MomentumMinSpeed = 1.5f;
        const float MomentumImpact = 1.6f;
        const float MomentumKeptOnImpact = 0.3f;
        public RaftBoosts Boosts { get; private set; }
        public static Raft Current { get; private set; }

        public float CrewThrustScale
        {
            get
            {
                int active = 0;
                foreach (Rider rider in riders)
                {
                    if (rider != null && rider.isActiveAndEnabled)
                    {
                        active++;
                    }
                }
                if (active == 0 || tuning == null)
                {
                    return 1f;
                }
                return Mathf.Lerp(1f, (float)riders.Length / active, tuning.crewCompensation);
            }
        }

        public bool IsFlipped => tuning != null && transform.up.y < tuning.flippedUpThreshold;
        public float StunUntil { get; private set; } = -1f;
        public bool Stunned => Time.time < StunUntil;

        public void Stun(float seconds)
        {
            StunUntil = Mathf.Max(StunUntil, Time.time + seconds);
        }

        void Awake()
        {
            Body = GetComponent<Rigidbody>();
            Boosts = GetComponent<RaftBoosts>();
            Current = this;
            Body.centerOfMass = centerOfMassOffset;
        }

        public void ApplyVariant(RaftVariant variant)
        {
            if (variant == null)
            {
                return;
            }
            ThrustScale = variant.thrust;
            DragScale = variant.drag;
            TurnDragScale = variant.turnDrag;
            DamageScale = variant.DamageScale;
            AccelerationLevel = Mathf.Clamp(variant.acceleration, 1, RaftVariant.MaxDurability);
            Momentum = 1f;
            if (variant.physics != null)
            {
                foreach (Collider collider in GetComponents<Collider>())
                {
                    collider.sharedMaterial = variant.physics;
                }
            }
            RaftVisualFactory.ApplyVariant(transform, variant);
        }

        void FixedUpdate()
        {
            int active = 0;
            int firing = 0;
            foreach (Rider rider in riders)
            {
                if (rider != null && rider.isActiveAndEnabled && !rider.InWater && !rider.Eliminated)
                {
                    active++;
                    if (rider.IsFiring)
                    {
                        firing++;
                    }
                }
            }
            Vector3 velocity = Body.linearVelocity;
            velocity.y = 0f;
            float dt = Time.fixedDeltaTime;
            if (firing > 0 && !Stunned && velocity.magnitude > MomentumMinSpeed)
            {
                float share = firing / (float)Mathf.Max(1, active);
                Momentum = Mathf.MoveTowards(Momentum, MaxMomentum, MomentumBuildPerLevel * AccelerationLevel * share * dt);
            }
            else
            {
                Momentum = Mathf.MoveTowards(Momentum, 1f, MomentumDecay * dt);
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.impulse.magnitude / Mathf.Max(1f, Body.mass) > MomentumImpact)
            {
                Momentum = 1f + (Momentum - 1f) * MomentumKeptOnImpact;
            }
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            Momentum = 1f;
            Body.linearVelocity = Vector3.zero;
            Body.angularVelocity = Vector3.zero;
            Body.position = position;
            Body.rotation = rotation;
            transform.SetPositionAndRotation(position, rotation);

            foreach (Rider rider in riders)
            {
                if (rider != null)
                {
                    rider.ResetFacing();
                }
            }
        }
    }
}
