using System.Collections.Generic;
using UnityEngine;

namespace FartRafters
{
    [RequireComponent(typeof(Raft))]
    public class BotCrew : MonoBehaviour
    {
        Raft raft;
        CourseManager course;
        RiverPlanner planner;
        bool searched;

        readonly List<Rider> bots = new List<Rider>();
        readonly List<Rider> ready = new List<Rider>();
        readonly HashSet<Rider> firing = new HashSet<Rider>();
        readonly HashSet<Rider> chosen = new HashSet<Rider>();
        readonly HashSet<Rider> resting = new HashSet<Rider>();

        int computedFrame = -1;
        float nextPlanTime;
        Vector3 target;
        float danger;
        Vector3 aim;
        Mode mode;
        float holdGap;
        float holdSince;
        bool approaching;
        bool sprint;
        float approachCap = float.MaxValue;
        float holdBack = 4.5f;
        int firingLimit = int.MaxValue;
        bool crossingCrocodile;
        float crocodileCommitUntil;
        float committedLane;

        enum Mode
        {
            Normal,
            Hold,
            Charge
        }
        float stuckTime;
        float escapeUntil;
        Vector3 escape;

        public Vector3 Target => target;
        public bool Demand { get; private set; }
        public float Danger => danger;
        public Vector3 Aim => aim;
        public int FiringCount => chosen.Count;
        public string ModeName => mode.ToString();
        public RiverPlanner Planner => planner;

        public static BotCrew For(Raft raft)
        {
            if (raft == null)
            {
                return null;
            }
            BotCrew crew = raft.GetComponent<BotCrew>();
            return crew != null ? crew : raft.gameObject.AddComponent<BotCrew>();
        }

        void Awake()
        {
            raft = GetComponent<Raft>();
        }

        void Locate()
        {
            if (searched)
            {
                return;
            }
            searched = true;
            course = FindFirstObjectByType<CourseManager>();
            GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
            planner = bootstrap != null ? RiverPlanner.For(bootstrap.Level) : null;
        }

        public RiderCommand CommandFor(Rider rider, BotDifficulty settings)
        {
            if (computedFrame != Time.frameCount)
            {
                computedFrame = Time.frameCount;
                Compute(settings);
            }
            bool fire = chosen.Contains(rider);
            return new RiderCommand(aim, fire);
        }

        void Compute(BotDifficulty settings)
        {
            Locate();
            chosen.Clear();
            Demand = false;
            if (course == null || raft == null || raft.Body == null || course.Finished)
            {
                aim = Vector3.zero;
                return;
            }

            Rigidbody body = raft.Body;
            Vector3 position = body.position;
            Vector3 velocity = body.linearVelocity;
            velocity.y = 0f;

            if (Time.time >= nextPlanTime)
            {
                nextPlanTime = Time.time + Mathf.Max(settings.decisionInterval, 0.13f);
                ChooseTarget(settings, position, velocity);
            }

            Vector3 desired = DesiredVelocity(settings, position, velocity);
            if (planner != null && settings.planLookAhead > 0f && mode != Mode.Hold)
            {
                Vector3 push = planner.Repulsion(position, settings.planMargin);
                if (desired.sqrMagnitude > 0.01f)
                {
                    Vector3 heading = desired.normalized;
                    float against = Vector3.Dot(push, heading);
                    if (against < 0f)
                    {
                        push -= heading * (against * 0.85f);
                    }
                }
                desired += push * (settings.cruiseSpeed * 0.6f);
                desired = Unstick(desired, velocity, position);
            }
            Vector3 acceleration = (desired - velocity) * settings.responsiveness;
            acceleration += desired * Drag();
            if (planner != null)
            {
                acceleration -= planner.CurrentAt(position);
            }
            Vector3 force = acceleration * body.mass;
            force.y = 0f;

            bots.Clear();
            Vector3 human = Vector3.zero;
            foreach (Rider rider in raft.riders)
            {
                if (rider == null || !rider.isActiveAndEnabled || rider.InWater || rider.Eliminated)
                {
                    continue;
                }
                if (rider.IsBot)
                {
                    bots.Add(rider);
                }
                else if (rider.IsFiring)
                {
                    human += Flat(rider.transform.forward) * Thrust(rider);
                }
            }

            Vector3 remaining = force - human;
            float nominal = bots.Count > 0 ? Thrust(bots[0]) : 450f;
            Demand = remaining.magnitude > nominal * (0.25f + settings.speedTolerance * 0.15f);
            Vector3 wanted = Demand ? remaining.normalized : (desired.sqrMagnitude > 0.01f ? desired.normalized : Flat(raft.transform.forward));
            aim = aim.sqrMagnitude < 0.01f ? wanted : Vector3.RotateTowards(aim, wanted, 540f * Mathf.Deg2Rad * Time.deltaTime, 0f).normalized;
            Allocate(settings, remaining);
        }

        void ChooseTarget(BotDifficulty settings, Vector3 position, Vector3 velocity)
        {
            if (planner != null && settings.planLookAhead > 0f)
            {
                var plan = planner.Plan(position, new RiverPlanner.Settings
                {
                    lookAhead = settings.planLookAhead,
                    step = 3.5f,
                    lanes = GraphicsQuality.PlannerLanes(settings.planLanes),
                    margin = settings.planMargin,
                    travelSpeed = Mathf.Max(velocity.magnitude, settings.minCruiseSpeed),
                    collectPickups = settings.collectPickups,
                    readCurrents = settings.readCurrents,
                    predictCrocodiles = settings.predictCrocodiles,
                    timedHazards = Expert(settings),
                    invincible = Expert(settings) && raft.Boosts != null && raft.Boosts.Invincible,
                    health = Health
                });
                target = plan.target;
                if (Expert(settings) && WeatherSystem.Current != null)
                {
                    float gust = WeatherSystem.Current.Gust;
                    if (gust > 0f)
                    {
                        target -= WeatherSystem.Current.WindDirection * (gust * 2.4f);
                    }
                }
                danger = plan.danger;
                EvaluateStrategy(settings, position, velocity);
                return;
            }

            danger = 0f;
            target = course.CurrentWaypoint;
            if (settings.cornerLookAhead > 0f && settings.cornerLookAheadDistance > 0f)
            {
                Vector3 offset = target - position;
                offset.y = 0f;
                float closeness = 1f - Mathf.Clamp01(offset.magnitude / settings.cornerLookAheadDistance);
                target = Vector3.Lerp(target, course.NextWaypoint, settings.cornerLookAhead * closeness);
            }
        }

        public static bool ExpertDisabled;

        static bool Expert(BotDifficulty settings)
        {
            return settings.level == BotLevel.Super && !ExpertDisabled;
        }

        float Health
        {
            get
            {
                RaftHealth health = raft != null ? raft.GetComponent<RaftHealth>() : null;
                return health != null ? health.Normalized : 1f;
            }
        }

        float rememberedCurrent = float.MinValue;
        const float StuckSeconds = 25f;
        const float StuckDistance = 5f;
        float progressAlong = float.MinValue;
        float progressSince;

        bool Stuck(BotDifficulty settings, Vector3 position)
        {
            if (!Expert(settings) || planner == null)
            {
                return false;
            }
            float along = planner.Project(new Vector2(position.x, position.z), out _, out _);
            if (along > progressAlong + StuckDistance || along < progressAlong - 30f)
            {
                progressAlong = along;
                progressSince = Time.time;
            }
            return Time.time - progressSince > StuckSeconds;
        }

        bool Fleeing(BotDifficulty settings)
        {
            ChaseAnimal chase = ChaseAnimal.Current;
            return Expert(settings) && chase != null && chase.Hunting && chase.Gap < 16f;
        }

        void EvaluateStrategy(BotDifficulty settings, Vector3 position, Vector3 velocity)
        {
            Mode previous = mode;
            if (previous != Mode.Hold)
            {
                holdSince = Time.time;
            }
            mode = Mode.Normal;
            sprint = false;
            approachCap = float.MaxValue;
            holdBack = 4.5f;
            firingLimit = int.MaxValue;
            crossingCrocodile = false;
            holdGap = float.MaxValue;
            bool infinite = raft.Boosts != null && raft.Boosts.InfiniteGas;
            bool stuck = Stuck(settings, position);
            if (settings.chargeCurrents)
            {
                float flow = planner.OpposingFlowAhead(position, Expert(settings) ? 20f : 12f, out bool inside, out float gap);
                if (Expert(settings))
                {
                    float here = planner.Project(new Vector2(position.x, position.z), out _, out _);
                    if (flow > 0f)
                    {
                        rememberedCurrent = here + (inside ? 0f : gap);
                    }
                    else if (rememberedCurrent > float.MinValue && here > rememberedCurrent - 30f && here < rememberedCurrent + 3f)
                    {
                        flow = 1f;
                        inside = here >= rememberedCurrent;
                        gap = Mathf.Max(0f, rememberedCurrent - here);
                    }
                    else if (here >= rememberedCurrent + 3f || here < rememberedCurrent - 30f)
                    {
                        rememberedCurrent = float.MinValue;
                    }
                }
                if (flow <= 0f)
                {
                    approaching = false;
                }
                else if (inside)
                {
                    if (Expert(settings) && !HasFuel(infinite, previous == Mode.Charge ? 0.04f : 0.9f, true))
                    {
                        approaching = false;
                        mode = Mode.Hold;
                        holdGap = float.MaxValue;
                        holdBack = 0f;
                    }
                    else
                    {
                        mode = Mode.Charge;
                        sprint = true;
                    }
                }
                else if (Expert(settings))
                {
                    if ((previous == Mode.Charge && gap > 16f) || (approaching && !HasFuel(infinite, 0.25f)))
                    {
                        approaching = false;
                    }
                    if (!approaching && HasFuel(infinite, 0.95f, true))
                    {
                        approaching = true;
                    }
                    if (approaching)
                    {
                        mode = Mode.Charge;
                        sprint = true;
                    }
                    else
                    {
                        mode = Mode.Hold;
                        holdGap = gap;
                        holdBack = 0f;
                    }
                }
                else
                {
                    if ((previous == Mode.Charge && gap > 5f) || (approaching && !HasFuel(infinite, 0.3f)))
                    {
                        approaching = false;
                    }
                    if (!approaching && gap >= 7.5f && (HasFuel(infinite, 0.95f, true) || Time.time - holdSince > 30f))
                    {
                        approaching = true;
                    }
                    if (approaching && !infinite && gap > 4f)
                    {
                        firingLimit = 1;
                    }
                    if (approaching && gap <= 4f)
                    {
                        mode = Mode.Charge;
                    }
                    if (approaching)
                    {
                        sprint = true;
                    }
                    if (!approaching)
                    {
                        mode = Mode.Hold;
                        holdGap = gap;
                        holdBack = 9f;
                    }
                }
            }
            if (Fleeing(settings) && mode != Mode.Hold)
            {
                sprint = true;
            }
            if (!settings.timeCrocodiles)
            {
                return;
            }
            float thrustTotal = 0f;
            foreach (Rider rider in raft.riders)
            {
                if (rider != null && rider.isActiveAndEnabled && rider.IsBot && !rider.InWater && !rider.Eliminated && (infinite || rider.Gas.Amount > 0.3f))
                {
                    thrustTotal += Thrust(rider);
                }
            }
            float acceleration = Mathf.Max(2f, 0.55f * thrustTotal / raft.Body.mass);
            float top = settings.cruiseSpeed * 1.3f;
            float crossing = Mathf.Max(top * 0.7f, 3f);
            float speedNow = Mathf.Max(0f, Vector3.Dot(velocity, planner.ForwardAt(position)));
            bool blocked = planner.CrocodileBlocks(position, speedNow, acceleration, top, Expert(settings) ? 2f : 1f, out bool inBand, out float crocodileGap, out float lane);
            bool near = inBand || crocodileGap < float.MaxValue;
            if (!near)
            {
                crocodileCommitUntil = 0f;
                return;
            }
            bool committed = Time.time < crocodileCommitUntil;
            if (!committed && !inBand)
            {
                if (crocodileGap > 5.5f)
                {
                    approachCap = Mathf.Sqrt(2f * 3f * Mathf.Max(0f, crocodileGap - 4.5f)) + 1f;
                    return;
                }
                bool wasWaiting = previous == Mode.Hold;
                if (blocked || (!stuck && !HasFuel(infinite, wasWaiting ? 0.85f : 0.6f)))
                {
                    mode = Mode.Hold;
                    holdGap = Mathf.Min(holdGap, crocodileGap);
                    return;
                }
                crocodileCommitUntil = Time.time + (crocodileGap + 8f) / crossing + 0.6f;
                committedLane = lane;
            }
            sprint = true;
            mode = Mode.Normal;
            crossingCrocodile = true;
            float along = planner.Project(new Vector2(position.x, position.z), out _, out _);
            Vector2 aimPoint = planner.PointAt(along + 5f, committed ? committedLane : lane, out _, out _);
            target = new Vector3(aimPoint.x, position.y, aimPoint.y);
        }

        bool HasFuel(bool infinite, float share, bool full = false)
        {
            if (infinite)
            {
                return true;
            }
            float total = 0f;
            float capacity = 0f;
            int count = 0;
            foreach (Rider rider in raft.riders)
            {
                if (rider != null && rider.isActiveAndEnabled && rider.IsBot && !rider.InWater && !rider.Eliminated)
                {
                    total += rider.Gas.Amount;
                    capacity += rider.Gas.Capacity;
                    count++;
                }
            }
            return count > 0 && total >= (full ? capacity * share : Mathf.Min(capacity * share, share > 0.7f ? 4.5f : 3f));
        }

        float LowestGas()
        {
            float lowest = 1f;
            foreach (Rider rider in raft.riders)
            {
                if (rider != null && rider.isActiveAndEnabled && rider.IsBot && !rider.InWater && !rider.Eliminated)
                {
                    lowest = Mathf.Min(lowest, rider.Gas.Normalized);
                }
            }
            return lowest;
        }

        Vector3 DesiredVelocity(BotDifficulty settings, Vector3 position, Vector3 velocity)
        {
            if (mode == Mode.Hold && planner != null)
            {
                return holdGap < holdBack ? -planner.ForwardAt(position) * 1.4f : Vector3.zero;
            }
            if (settings.waitForSwimmers)
            {
                foreach (Rider rider in raft.riders)
                {
                    if (rider != null && rider.InWater && !rider.Eliminated)
                    {
                        Vector3 toSwimmer = Flat(rider.transform.position - position);
                        float distance = toSwimmer.magnitude;
                        return distance > 0.5f ? toSwimmer / distance * Mathf.Min(2.2f, distance * 0.7f) : Vector3.zero;
                    }
                }
            }

            Vector3 toTarget = Flat(target - position);
            if (toTarget.sqrMagnitude < 0.01f)
            {
                return Vector3.zero;
            }
            float slow = Mathf.Clamp01(danger / settings.dangerSlowdown);
            float speed = Mathf.Lerp(settings.cruiseSpeed, settings.minCruiseSpeed, slow);
            Vector3 direction = toTarget.normalized;
            if (planner != null && settings.planLookAhead > 0f)
            {
                float alignment = Vector3.Dot(direction, planner.ForwardAt(position));
                speed *= Mathf.Lerp(0.6f, 1f, Mathf.Clamp01(alignment));
                float limit = planner.SpeedLimit(position, velocity, settings.cruiseSpeed, settings.minCruiseSpeed * 1.1f);
                speed = Mathf.Min(speed, limit);
                if (sprint || mode == Mode.Charge)
                {
                    float sprintLimit = crossingCrocodile || mode == Mode.Charge ? Mathf.Max(limit, settings.minCruiseSpeed * 1.5f) : limit;
                    speed = Mathf.Max(speed, Mathf.Min(settings.cruiseSpeed * 1.3f, sprintLimit));
                }
            }
            speed = Mathf.Min(speed, approachCap);
            return direction * Mathf.Max(Mathf.Min(settings.minCruiseSpeed * 0.6f, approachCap), speed);
        }

        Vector3 Unstick(Vector3 desired, Vector3 velocity, Vector3 position)
        {
            float now = Time.time;
            if (mode == Mode.Hold)
            {
                stuckTime = 0f;
                return desired;
            }
            if (now < escapeUntil)
            {
                return escape;
            }
            bool pushing = desired.magnitude > 1f && Vector3.Dot(velocity, desired.normalized) < 0.25f && firing.Count > 0;
            if (pushing && now > escapeUntil + 3f && !AnyoneSwimming())
            {
                stuckTime += Time.deltaTime;
            }
            else
            {
                stuckTime = Mathf.Max(0f, stuckTime - Time.deltaTime * 2f);
            }
            if (stuckTime < 1.2f)
            {
                return desired;
            }
            stuckTime = 0f;
            Vector3 away = planner.Repulsion(position, 2.5f);
            Vector3 back = -desired.normalized;
            Vector3 side = Vector3.Cross(Vector3.up, planner.ForwardAt(position)) * (Random.value < 0.5f ? 1f : -1f);
            Vector3 direction = away.sqrMagnitude > 0.01f ? (away.normalized + back * 0.5f) : (back + side * 0.8f);
            escape = direction.normalized * 2.2f;
            escapeUntil = now + 1.1f;
            return escape;
        }

        bool AnyoneSwimming()
        {
            foreach (Rider rider in raft.riders)
            {
                if (rider != null && rider.InWater && !rider.Eliminated)
                {
                    return true;
                }
            }
            return false;
        }

        void Allocate(BotDifficulty settings, Vector3 remaining)
        {
            ready.Clear();
            bool infinite = raft.Boosts != null && raft.Boosts.InfiniteGas;
            float thrust = 0f;
            foreach (Rider rider in bots)
            {
                if (!infinite && (rider.Gas.Exhausted || rider.Gas.Normalized <= 0.02f))
                {
                    firing.Remove(rider);
                    continue;
                }
                bool backingOff = mode == Mode.Hold && holdGap < holdBack;
                if (!infinite && mode != Mode.Charge && !sprint && !backingOff && danger < 15f && rider.Gas.Normalized < settings.gasReserve)
                {
                    resting.Add(rider);
                }
                if (resting.Contains(rider))
                {
                    if (infinite || danger >= 15f || mode == Mode.Charge || sprint || backingOff || rider.Gas.Normalized >= 0.85f)
                    {
                        resting.Remove(rider);
                    }
                    else
                    {
                        firing.Remove(rider);
                        continue;
                    }
                }
                if (!firing.Contains(rider) && !infinite && rider.Gas.Normalized < settings.minGasToStart && rider.Gas.Normalized < 0.95f)
                {
                    continue;
                }
                float facing = Vector3.Angle(Flat(rider.transform.forward), aim);
                if (facing > settings.facingTolerance)
                {
                    continue;
                }
                ready.Add(rider);
                thrust += Thrust(rider);
            }

            if (ready.Count == 0)
            {
                firing.Clear();
                return;
            }

            float perRider = thrust / ready.Count;
            float magnitude = remaining.magnitude;
            float threshold = perRider * (0.25f + settings.speedTolerance * 0.15f);
            int needed = mode == Mode.Charge ? ready.Count : magnitude < threshold ? 0 : Mathf.Clamp(Mathf.FloorToInt(magnitude / perRider + 0.55f), 1, ready.Count);
            needed = Mathf.Min(needed, firingLimit);
            if (mode == Mode.Hold && holdGap >= holdBack && planner != null && Vector3.Dot(raft.Body.linearVelocity, planner.ForwardAt(raft.Body.position)) < 0.8f)
            {
                needed = 0;
            }

            if (needed == 0)
            {
                firing.Clear();
                return;
            }

            Vector3 center = raft.Body.worldCenterOfMass;
            int bestMask = 0;
            float bestScore = float.MaxValue;
            int combinations = 1 << ready.Count;
            for (int mask = 1; mask < combinations; mask++)
            {
                if (CountBits(mask) != needed)
                {
                    continue;
                }
                float torque = 0f;
                float score = 0f;
                for (int i = 0; i < ready.Count; i++)
                {
                    if ((mask & (1 << i)) == 0)
                    {
                        continue;
                    }
                    Rider rider = ready[i];
                    Vector3 arm = Flat(rider.transform.position - center);
                    torque += Vector3.Cross(arm, aim).y;
                    float gas = infinite ? 1f : rider.Gas.Normalized;
                    score += (1f - gas) * 1.6f;
                    if (!firing.Contains(rider))
                    {
                        score += 0.35f;
                    }
                }
                score += Mathf.Abs(torque) * 0.6f;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestMask = mask;
                }
            }

            firing.Clear();
            for (int i = 0; i < ready.Count; i++)
            {
                if ((bestMask & (1 << i)) != 0)
                {
                    firing.Add(ready[i]);
                    chosen.Add(ready[i]);
                }
            }
        }

        static int CountBits(int value)
        {
            int count = 0;
            while (value != 0)
            {
                count += value & 1;
                value >>= 1;
            }
            return count;
        }

        float Drag()
        {
            return raft.tuning != null ? raft.tuning.waterLinearDrag : 0.8f;
        }

        float Thrust(Rider rider)
        {
            float multiplier = (raft.tuning != null ? raft.tuning.thrustMultiplier : 1f) * raft.CrewThrustScale * raft.PropulsionScale * (raft.Boosts != null ? raft.Boosts.ThrustMultiplier : 1f);
            return rider.avatar != null ? rider.avatar.thrustForce * multiplier : 450f;
        }

        static Vector3 Flat(Vector3 vector)
        {
            vector.y = 0f;
            return vector;
        }
    }
}
