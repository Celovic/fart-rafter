using System;
using UnityEngine;

namespace FartRafters
{
    public enum RiderEvent
    {
        Fell,
        Boarded,
        Eliminated,
        Returned
    }

    public class Rider : MonoBehaviour
    {
        public AvatarDefinition avatar;
        public Raft raft;
        public RiderBrain brain;
        public ParticleSystem puff;
        [Min(0f)] public float puffRate = 45f;
        [Min(0)] public int puffBurst = 10;
        [Min(0.1f)] public float swimSpeed = 2.4f;
        [Min(0.1f)] public float boardDistance = 1.4f;
        [Min(0.5f)] public float boardTimeout = 7f;
        public float swimDepth = 0.4f;

        readonly GasTank gas = new GasTank();
        Quaternion restLocalRotation;
        RiderCommand command;
        int refillVersion;
        Transform seatParent;
        Vector3 seatPosition;
        float swimTime;
        bool networkFiring;
        Vector3 predictedAim;

        public static event Action<Rider, RiderEvent> StateChanged;

        public bool InWater { get; private set; }
        public bool Eliminated { get; private set; }
        public float SwimTimeLeft => Mathf.Max(0f, boardTimeout - swimTime);

        public GasTank Gas => gas;
        public bool IsFiring { get; private set; }
        public Vector3 LastThrust { get; private set; }
        public bool NetworkDriven { get; set; }
        public bool IsBot => brain is BotBrain;
        public bool Autopilot { get; private set; }
        public float AutopilotSeconds { get; private set; }

        RiderBrain manualBrain;

        public void SetAutopilot(bool on, BotDifficulty level, bool visualOnly = false)
        {
            if (on == Autopilot)
            {
                return;
            }
            Autopilot = on;
            if (visualOnly)
            {
                return;
            }
            BotBrain bot = GetComponent<BotBrain>();
            if (on)
            {
                if (bot == null)
                {
                    Autopilot = false;
                    return;
                }
                manualBrain = brain;
                if (level != null)
                {
                    bot.difficulty = level;
                }
                bot.enabled = true;
                brain = bot;
            }
            else
            {
                if (bot != null && manualBrain != null)
                {
                    bot.enabled = false;
                }
                if (manualBrain != null)
                {
                    brain = manualBrain;
                }
                manualBrain = null;
            }
        }

        float Capacity
        {
            get
            {
                float multiplier = raft != null && raft.tuning != null ? raft.tuning.gasMultiplier : 1f;
                float boost = raft != null && raft.Boosts != null ? raft.Boosts.GasMultiplier : 1f;
                return avatar.gasCapacitySeconds * multiplier * boost;
            }
        }

        void Awake()
        {
            restLocalRotation = transform.localRotation;
            seatParent = transform.parent;
            seatPosition = transform.localPosition;
            gas.Reset(Capacity);
        }

        void OnDisable()
        {
            IsFiring = false;
            SetPuff(false);
        }

        void Update()
        {
            if (Autopilot)
            {
                AutopilotSeconds += Time.deltaTime;
            }
            if (NetworkDriven)
            {
                bool was = IsFiring;
                IsFiring = networkFiring && !InWater;
                if (IsFiring && !was && puff != null)
                {
                    puff.Emit(puffBurst);
                }
                SetPuff(IsFiring);
                if (predictedAim.sqrMagnitude > 0.0001f && !InWater)
                {
                    Face(predictedAim);
                }
                return;
            }
            if (InWater)
            {
                IsFiring = false;
                SetPuff(false);
                Swim(Time.deltaTime);
                return;
            }
            command = brain != null && brain.isActiveAndEnabled ? brain.GetCommand(this) : default;

            if (command.HasAim)
            {
                Face(command.aim);
            }

            bool wasFiring = IsFiring;
            RaftBoosts boosts = raft != null ? raft.Boosts : null;
            if (boosts != null && boosts.RefillVersion != refillVersion)
            {
                refillVersion = boosts.RefillVersion;
                gas.Fill();
            }
            float refill = avatar.refillPerSecond * (boosts != null ? boosts.RefillMultiplier : 1f);
            bool infinite = boosts != null && boosts.InfiniteGas;
            bool wantsGas = command.gas && !(raft != null && raft.Stunned);
            IsFiring = gas.Tick(Time.deltaTime, wantsGas, Capacity, refill, avatar.refillDelay, avatar.recoverThreshold, infinite);
            if (IsFiring && !wasFiring && brain is PlayerBrain && !Autopilot)
            {
                Haptics.Light();
            }
            if (IsFiring && !wasFiring && puff != null)
            {
                puff.Emit(puffBurst);
            }
            SetPuff(IsFiring);
        }

        void FixedUpdate()
        {
            LastThrust = Vector3.zero;
            if (NetworkDriven)
            {
                return;
            }
            if (!IsFiring || raft == null || InWater)
            {
                return;
            }

            Vector3 direction = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            float multiplier = (raft.tuning != null ? raft.tuning.thrustMultiplier : 1f) * raft.CrewThrustScale * raft.PropulsionScale * (raft.Boosts != null ? raft.Boosts.ThrustMultiplier : 1f);
            LastThrust = direction.normalized * (avatar.thrustForce * multiplier);
            raft.Body.AddForceAtPosition(LastThrust, transform.position, ForceMode.Force);
        }

        public int Costume { get; private set; }

        public void SetAvatar(AvatarDefinition definition, CourseTheme theme, int costume = 0)
        {
            if (definition == null || theme == null)
            {
                return;
            }

            avatar = definition;
            Transform old = transform.Find(AvatarVisualFactory.VisualName);
            if (old != null)
            {
                DestroyImmediate(old.gameObject);
            }
            Costume = costume;
            AvatarRig rig = AvatarVisualFactory.Build(transform, definition, theme, costume);
            rig.rider = this;
            if (puff != null)
            {
                puff.transform.localPosition = AvatarVisualFactory.PuffPosition(definition);
            }
            gas.Reset(Capacity);
        }

        public void ApplyNetwork(bool firing, float gasNormalized, float localYaw, bool ownYaw, Vector3 swimPosition)
        {
            networkFiring = firing;
            gas.SetNormalized(gasNormalized, Capacity);
            if (InWater)
            {
                transform.position = Vector3.Lerp(transform.position, swimPosition, 0.35f);
                return;
            }
            if (!ownYaw)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0f, localYaw, 0f), 0.5f);
            }
        }

        public void PredictAim(Vector3 aim)
        {
            predictedAim = aim;
        }

        public float LocalYaw => transform.localEulerAngles.y;

        public void ApplyNetworkEvent(RiderEvent change, Vector3 outward)
        {
            switch (change)
            {
                case RiderEvent.Fell:
                    Fall(outward);
                    break;
                case RiderEvent.Boarded:
                    if (InWater)
                    {
                        Seat();
                        StateChanged?.Invoke(this, RiderEvent.Boarded);
                    }
                    break;
                case RiderEvent.Eliminated:
                    if (!Eliminated)
                    {
                        Seat();
                        Eliminated = true;
                        StateChanged?.Invoke(this, RiderEvent.Eliminated);
                        gameObject.SetActive(false);
                    }
                    break;
                case RiderEvent.Returned:
                    Return();
                    break;
            }
        }

        public void Fall(Vector3 outward)
        {
            if (InWater || Eliminated || seatParent == null)
            {
                return;
            }
            InWater = true;
            swimTime = 0f;
            IsFiring = false;
            SetPuff(false);
            transform.SetParent(null, true);
            Vector3 flat = Vector3.ProjectOnPlane(outward, Vector3.up);
            Vector3 position = transform.position + (flat.sqrMagnitude > 0.001f ? flat.normalized : Vector3.right) * 1.8f;
            position.y = WaterHeight(position) - swimDepth;
            transform.position = position;
            StateChanged?.Invoke(this, RiderEvent.Fell);
        }

        public void Return()
        {
            if (!InWater && !Eliminated)
            {
                return;
            }
            bool wasEliminated = Eliminated;
            Seat();
            Eliminated = false;
            gameObject.SetActive(true);
            if (wasEliminated)
            {
                StateChanged?.Invoke(this, RiderEvent.Returned);
            }
        }

        void Swim(float deltaTime)
        {
            swimTime += deltaTime;
            Vector3 target = seatParent.TransformPoint(seatPosition);
            Vector3 flat = target - transform.position;
            flat.y = 0f;
            if (flat.magnitude < boardDistance)
            {
                Seat();
                StateChanged?.Invoke(this, RiderEvent.Boarded);
                return;
            }
            if (swimTime > boardTimeout)
            {
                Seat();
                Eliminated = true;
                StateChanged?.Invoke(this, RiderEvent.Eliminated);
                gameObject.SetActive(false);
                return;
            }
            Vector3 position = transform.position + flat.normalized * (swimSpeed * deltaTime);
            position.y = WaterHeight(position) - swimDepth + Mathf.Sin(Time.time * 5f) * 0.04f;
            float ice = IceZone.Amount(position);
            if (ice > 0f)
            {
                position.y = Mathf.Lerp(position.y, IceZone.Top, ice);
            }
            transform.position = position;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(flat.normalized, Vector3.up), 360f * deltaTime);
        }

        void Seat()
        {
            InWater = false;
            transform.SetParent(seatParent, false);
            transform.localPosition = seatPosition;
            transform.localRotation = restLocalRotation;
        }

        static float WaterHeight(Vector3 position)
        {
            WaterSurface water = WaterSurface.Instance;
            return water != null ? water.GetHeight(position.x, position.z, Time.time) : 0f;
        }

        public void ResetFacing()
        {
            transform.localRotation = restLocalRotation;
        }

        void Face(Vector3 worldDirection)
        {
            Vector3 up = raft != null ? raft.transform.up : Vector3.up;
            Vector3 flat = Vector3.ProjectOnPlane(worldDirection, up);
            if (flat.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion target = Quaternion.LookRotation(flat.normalized, up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, avatar.turnSpeed * Time.deltaTime);
        }

        void SetPuff(bool on)
        {
            if (puff == null)
            {
                return;
            }

            ParticleSystem.EmissionModule emission = puff.emission;
            emission.rateOverTime = on ? puffRate : 0f;
        }
    }
}
