using System;
using System.Collections.Generic;
using UnityEngine;

namespace FartRafters
{
    public class ScoreTracker : MonoBehaviour
    {
        class Sample
        {
            public float time;
            public Vector3 thrust;
        }

        class Entry
        {
            public Rider rider;
            public PlayerScore score;
            public readonly Queue<Sample> history = new Queue<Sample>();
            public float usefulThrust;
            public float totalThrust;
            public float gasSeconds;
            public float penalties;
            public float bonus;
            public bool cleanSegment = true;
            public float swimRescue;
            public float swimBlame;
            public Dictionary<Rider, float> fallBlame;
        }

        GameBootstrap bootstrap;
        CourseManager course;
        Raft raft;
        RaftHealth health;
        RaftBoosts boosts;
        RiverPlanner planner;
        readonly List<Entry> entries = new List<Entry>();
        Vector3 forward = Vector3.forward;
        float nextForward;
        float stuckTime;
        Vector3 lastVelocity;
        int catches;
        int iceHits;
        bool finished;

        public ScoreReport Report { get; private set; }
        public static ScoreTracker Current { get; private set; }
        public static Func<int, string> SeatNames;
        public event Action<ScoreReport> Completed;

        public void Setup(GameBootstrap owner)
        {
            bootstrap = owner;
            course = owner.course;
            raft = owner.raft;
            health = raft != null ? raft.GetComponent<RaftHealth>() : null;
            boosts = raft != null ? raft.Boosts : null;
            planner = RiverPlanner.For(owner.Level);
            Current = this;
        }

        void Start()
        {
            if (raft == null)
            {
                enabled = false;
                return;
            }
            Rider main = bootstrap.PlayerRiders != null && bootstrap.PlayerRiders.Length > 0 ? bootstrap.PlayerRiders[0] : null;
            for (int i = 0; i < raft.riders.Length; i++)
            {
                Rider rider = raft.riders[i];
                if (rider == null || !rider.gameObject.activeSelf)
                {
                    continue;
                }
                string name = rider.IsBot ? (rider.avatar != null ? rider.avatar.displayName : "Bot") : PlayerName(i, rider);
                entries.Add(new Entry { rider = rider, score = new PlayerScore { name = name, bot = rider.IsBot, main = rider == main, seat = i } });
            }
            if (health != null)
            {
                health.Damaged += OnDamaged;
                health.Broken += OnBroken;
                health.Caught += OnCaught;
            }
            if (boosts != null)
            {
                boosts.Collected += OnCollected;
            }
            if (course != null)
            {
                course.CheckpointReached += OnCheckpoint;
                course.RaceFinished += OnFinished;
            }
            Rider.StateChanged += OnRider;
        }

        void OnDestroy()
        {
            if (health != null)
            {
                health.Damaged -= OnDamaged;
                health.Broken -= OnBroken;
                health.Caught -= OnCaught;
            }
            if (boosts != null)
            {
                boosts.Collected -= OnCollected;
            }
            if (course != null)
            {
                course.CheckpointReached -= OnCheckpoint;
                course.RaceFinished -= OnFinished;
            }
            Rider.StateChanged -= OnRider;
            if (Current == this)
            {
                Current = null;
            }
        }

        string PlayerName(int seat, Rider rider)
        {
            string avatar = rider.avatar != null ? rider.avatar.displayName : "Oyuncu";
            string assigned = SeatNames != null ? SeatNames(seat) : null;
            if (!string.IsNullOrEmpty(assigned))
            {
                return assigned;
            }
            if (seat == 0 && PlayerProfile.HasName)
            {
                return PlayerProfile.DisplayName;
            }
            return "Oyuncu " + (seat + 1) + " · " + avatar;
        }

        void FixedUpdate()
        {
            if (finished || course == null || !course.Running)
            {
                return;
            }
            float dt = Time.fixedDeltaTime;
            float now = Time.time;
            if (planner != null && now >= nextForward)
            {
                nextForward = now + 0.25f;
                forward = planner.ForwardAt(raft.Body.position);
            }

            Vector3 net = Vector3.zero;
            float positive = 0f;
            foreach (Entry entry in entries)
            {
                Vector3 thrust = entry.rider != null ? entry.rider.LastThrust : Vector3.zero;
                net += thrust;
                positive += Mathf.Max(0f, Vector3.Dot(thrust, forward));
            }
            float share = positive > 0.001f ? Mathf.Clamp01(Mathf.Max(0f, Vector3.Dot(net, forward)) / positive) : 0f;

            Rider swimmer = Swimmer();
            foreach (Entry entry in entries)
            {
                Vector3 thrust = entry.rider != null ? entry.rider.LastThrust : Vector3.zero;
                float magnitude = thrust.magnitude;
                if (magnitude > 0.01f)
                {
                    entry.totalThrust += magnitude * dt;
                    entry.usefulThrust += Mathf.Max(0f, Vector3.Dot(thrust, forward)) * share * dt;
                    entry.gasSeconds += dt;
                    if (swimmer != null && swimmer != entry.rider)
                    {
                        Vector3 toSwimmer = swimmer.transform.position - raft.Body.position;
                        toSwimmer.y = 0f;
                        if (toSwimmer.sqrMagnitude > 0.01f)
                        {
                            entry.swimRescue += Mathf.Max(0f, Vector3.Dot(thrust, toSwimmer.normalized)) * dt;
                        }
                    }
                }
                entry.history.Enqueue(new Sample { time = now, thrust = thrust });
                while (entry.history.Count > 0 && now - entry.history.Peek().time > ScoreRules.BlameWindow)
                {
                    entry.history.Dequeue();
                }
            }

            Vector3 velocity = raft.Body.linearVelocity;
            velocity.y = 0f;
            lastVelocity = velocity;
            if (velocity.magnitude < ScoreRules.StuckSpeed && swimmer == null && !course.Finished)
            {
                stuckTime += dt;
                if (stuckTime > ScoreRules.StuckDelay)
                {
                    ShareEqually(ScoreRules.StuckPerSecond * dt, null, false);
                }
            }
            else
            {
                stuckTime = 0f;
            }
        }

        Rider Swimmer()
        {
            foreach (Entry entry in entries)
            {
                if (entry.rider != null && entry.rider.InWater && !entry.rider.Eliminated)
                {
                    return entry.rider;
                }
            }
            return null;
        }

        Entry Find(Rider rider)
        {
            foreach (Entry entry in entries)
            {
                if (entry.rider == rider)
                {
                    return entry;
                }
            }
            return null;
        }

        Dictionary<Rider, float> Blame(float window, Rider exclude)
        {
            var weights = new Dictionary<Rider, float>();
            float now = Time.time;
            Vector3 direction = lastVelocity.sqrMagnitude > 0.04f ? lastVelocity.normalized : Vector3.zero;
            foreach (Entry entry in entries)
            {
                if (entry.rider == exclude)
                {
                    continue;
                }
                float weight = 0f;
                foreach (Sample sample in entry.history)
                {
                    if (now - sample.time > window)
                    {
                        continue;
                    }
                    float magnitude = sample.thrust.magnitude;
                    if (magnitude < 0.01f)
                    {
                        continue;
                    }
                    float along = direction == Vector3.zero ? 1f : 0.35f + 0.65f * Mathf.Max(0f, Vector3.Dot(sample.thrust / magnitude, direction));
                    weight += magnitude * along * Time.fixedDeltaTime;
                }
                if (weight > 0.0001f)
                {
                    weights[entry.rider] = weight;
                }
            }
            return weights;
        }

        void Charge(Entry entry, float amount, bool personal)
        {
            if (entry == null || amount <= 0f)
            {
                return;
            }
            entry.penalties += amount;
            if (personal)
            {
                entry.cleanSegment = false;
            }
        }

        void ShareEqually(float amount, Action<Entry, float> track, bool personal)
        {
            if (entries.Count == 0)
            {
                return;
            }
            float part = amount / entries.Count;
            foreach (Entry entry in entries)
            {
                Charge(entry, part, personal);
                track?.Invoke(entry, part);
            }
        }

        void ShareByBlame(float amount, Dictionary<Rider, float> weights, Action<Entry, float> track)
        {
            float total = 0f;
            foreach (float weight in weights.Values)
            {
                total += weight;
            }
            if (total <= 0.0001f)
            {
                ShareEqually(amount, track, true);
                return;
            }
            foreach (KeyValuePair<Rider, float> pair in weights)
            {
                Entry entry = Find(pair.Key);
                float part = amount * pair.Value / total;
                Charge(entry, part, true);
                if (entry != null)
                {
                    track?.Invoke(entry, part);
                }
            }
        }

        void OnDamaged(float amount)
        {
            if (finished)
            {
                return;
            }
            if (raft != null && IceZone.Amount(raft.transform.position) > 0f)
            {
                iceHits++;
            }
            ShareByBlame(amount * ScoreRules.DamagePerHealth, Blame(ScoreRules.BlameWindow, null), (e, p) => e.score.damagePenalty += p);
        }

        void OnBroken()
        {
            if (finished)
            {
                return;
            }
            ShareEqually(ScoreRules.Broken * 0.5f, null, true);
            ShareByBlame(ScoreRules.Broken * 0.5f, Blame(ScoreRules.BlameWindow, null), null);
        }

        void OnCaught()
        {
            if (finished)
            {
                return;
            }
            catches++;
            ShareEqually(ScoreRules.Crocodile * 0.5f, (e, p) => e.score.crocodilePenalty += p, true);
            ShareByBlame(ScoreRules.Crocodile * 0.5f, Blame(ScoreRules.BlameWindow, null), (e, p) => e.score.crocodilePenalty += p);
        }

        void OnCollected(PickupKind kind)
        {
            if (finished)
            {
                return;
            }
            Entry pilot = null;
            float best = 0f;
            foreach (Entry entry in entries)
            {
                entry.bonus += ScoreRules.PickupBonus;
                float recent = 0f;
                foreach (Sample sample in entry.history)
                {
                    recent += sample.thrust.magnitude;
                }
                if (recent > best)
                {
                    best = recent;
                    pilot = entry;
                }
            }
            if (pilot != null)
            {
                pilot.score.pickups++;
            }
        }

        void OnCheckpoint(Checkpoint checkpoint)
        {
            foreach (Entry entry in entries)
            {
                if (entry.cleanSegment)
                {
                    entry.bonus += ScoreRules.CleanCheckpointBonus;
                }
                entry.cleanSegment = true;
            }
        }

        void OnRider(Rider rider, RiderEvent change)
        {
            Entry entry = Find(rider);
            if (entry == null || finished)
            {
                return;
            }
            switch (change)
            {
                case RiderEvent.Fell:
                    entry.score.falls++;
                    Charge(entry, ScoreRules.Fall, true);
                    entry.fallBlame = Blame(ScoreRules.FallBlameWindow, rider);
                    ShareByBlameOnly(ScoreRules.Fall * ScoreRules.CulpritShare, entry.fallBlame);
                    foreach (Entry other in entries)
                    {
                        other.swimRescue = 0f;
                    }
                    break;
                case RiderEvent.Eliminated:
                    Charge(entry, ScoreRules.Elimination, true);
                    if (entry.fallBlame != null)
                    {
                        ShareByBlameOnly(ScoreRules.Elimination * ScoreRules.CulpritShare, entry.fallBlame);
                    }
                    break;
                case RiderEvent.Boarded:
                    float total = 0f;
                    foreach (Entry other in entries)
                    {
                        if (other != entry)
                        {
                            total += other.swimRescue;
                        }
                    }
                    if (total > 1f)
                    {
                        foreach (Entry other in entries)
                        {
                            if (other == entry || other.swimRescue <= 0f)
                            {
                                continue;
                            }
                            other.bonus += ScoreRules.RescueBonus * other.swimRescue / total;
                            if (other.swimRescue / total > 0.3f)
                            {
                                other.score.rescues++;
                            }
                        }
                    }
                    break;
            }
        }

        void ShareByBlameOnly(float amount, Dictionary<Rider, float> weights)
        {
            float total = 0f;
            foreach (float weight in weights.Values)
            {
                total += weight;
            }
            if (total <= 0.0001f)
            {
                return;
            }
            foreach (KeyValuePair<Rider, float> pair in weights)
            {
                Entry culprit = Find(pair.Key);
                if (culprit == null)
                {
                    continue;
                }
                float part = amount * pair.Value / total;
                Charge(culprit, part, true);
                culprit.score.causedFalls += part;
            }
        }

        void OnFinished()
        {
            if (finished)
            {
                return;
            }
            finished = true;
            Report = Build();
            Completed?.Invoke(Report);
        }

        public ScoreReport Build()
        {
            LevelDefinition level = bootstrap.Level;
            entries.RemoveAll(entry => entry.rider == null || (!entry.rider.gameObject.activeInHierarchy && entry.totalThrust < 0.01f && entry.score.falls == 0));
            int humans = 0;
            int bots = 0;
            foreach (Entry entry in entries)
            {
                if (entry.score.bot)
                {
                    bots++;
                }
                else
                {
                    humans++;
                }
            }
            BotDifficulty botLevel = bootstrap.session != null ? bootstrap.session.CurrentBotLevel : null;
            var report = new ScoreReport
            {
                levelId = ScoreId(bootstrap),
                levelName = level != null ? level.displayName : "",
                adventure = bootstrap.IsAdventure,
                difficultyIndex = bootstrap.IsAdventure ? GameSelection.DifficultyIndex : bootstrap.LevelIndex,
                elapsed = course.ElapsedTime,
                respawns = course.RespawnCount,
                humans = Mathf.Max(1, humans),
                bots = bots
            };
            report.par = Par(level, bootstrap, entries.Count);
            report.difficultyFactor = ScoreRules.DifficultyFactor(report.adventure, report.difficultyIndex);
            report.crewFactor = ScoreRules.CrewFactor(report.humans);
            report.botFactor = bots > 0 && botLevel != null ? ScoreRules.BotFactor(bots, botLevel.level) : 1f;

            float length = Mathf.Max(10f, LevelLength(level));
            float reference = ReferenceGas(entries.Count);
            float time = ScoreRules.TimeScore(report.par, report.elapsed);
            foreach (Entry entry in entries)
            {
                PlayerScore score = entry.score;
                score.time = time;
                score.penalties = entry.penalties;
                score.safety = Mathf.Max(0f, ScoreRules.SafetyMax - entry.penalties);
                score.gasSeconds = entry.gasSeconds;
                score.usefulThrust = entry.usefulThrust;
                score.wastedThrust = Mathf.Max(0f, entry.totalThrust - entry.usefulThrust);
                score.efficiency = entry.totalThrust > 0.01f ? entry.usefulThrust / entry.totalThrust : 0f;
                if (entry.totalThrust > 50f)
                {
                    float efficiency = Mathf.Clamp01(score.efficiency / ScoreRules.EfficiencyTarget);
                    float used = entry.gasSeconds / length * 100f;
                    float economy = used > 0.001f ? Mathf.Min(1f, reference / used) : 0f;
                    score.gas = ScoreRules.GasMax * (0.65f * efficiency + 0.35f * economy);
                }
                float bonus = entry.bonus + (report.respawns == 0 ? ScoreRules.NoRespawnBonus : 0f);
                score.bonus = Mathf.Min(ScoreRules.BonusMax, bonus);
                float autopilot = entry.rider != null && course != null ? Mathf.Clamp01(entry.rider.AutopilotSeconds / Mathf.Max(1f, course.ElapsedTime)) : 0f;
                score.total = Mathf.RoundToInt(score.Base * report.difficultyFactor * report.crewFactor * report.botFactor * Mathf.Lerp(1f, 0.5f, autopilot));
                report.players.Add(score);
            }
            bool crocodiles = level != null && level.obstacles != null && level.obstacles.Length > 0;
            bool ice = level != null && level.ice != null && level.ice.Length > 0;
            Badges.Assign(report, crocodiles, catches, bootstrap != null ? bootstrap.Biome : Biome.River, ice, iceHits);
            return report;
        }

        public static string ScoreId(GameBootstrap bootstrap)
        {
            if (bootstrap.IsAdventure && bootstrap.Difficulty != null)
            {
                return "macera_" + bootstrap.Difficulty.id;
            }
            return bootstrap.Level != null ? bootstrap.Level.id : "level";
        }

        static float Par(LevelDefinition level, GameBootstrap bootstrap, int crew)
        {
            if (level == null)
            {
                return 60f;
            }
            float baseline = bootstrap.IsAdventure && bootstrap.Difficulty != null
                ? LevelLength(level) / Mathf.Max(0.5f, bootstrap.Difficulty.expectedSpeed)
                : level.parTime;
            return Mathf.Max(10f, baseline * ScoreRules.ParSpeedFactor(crew));
        }

        public static float LevelLength(LevelDefinition level)
        {
            if (level == null || level.NodeCount < 2)
            {
                return 0f;
            }
            float total = 0f;
            int finish = Mathf.Clamp(level.FinishNode, 1, level.NodeCount - 1);
            for (int i = Mathf.Max(0, level.startNode); i < finish; i++)
            {
                total += Vector3.Distance(level.Center(i), level.Center(i + 1));
            }
            return total;
        }

        float ReferenceGas(int crew)
        {
            RaftTuning tuning = raft.tuning;
            float drag = tuning != null ? tuning.waterLinearDrag : 0.8f;
            float thrust = 450f;
            foreach (Entry entry in entries)
            {
                if (entry.rider != null && entry.rider.avatar != null)
                {
                    thrust = entry.rider.avatar.thrustForce;
                    break;
                }
            }
            float multiplier = tuning != null ? tuning.thrustMultiplier : 1f;
            return ScoreRules.ReferenceGasPer100(raft.Body.mass, drag, thrust * multiplier, raft.CrewThrustScale, crew);
        }
    }
}
