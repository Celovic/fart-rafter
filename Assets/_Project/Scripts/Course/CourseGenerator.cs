using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace FartRafters
{
    public static class CourseGenerator
    {
        const float Step = 9f;
        const float MaxHeading = 62f;
        const float CheckpointClearance = 14f;
        const float MaxCurrent = 5f;
        const float MaxSideCurrent = 5.5f;
        const float MaxBackCurrent = 4f;

        class Path
        {
            public readonly List<CourseNode> nodes = new List<CourseNode>();
            public readonly List<float> distances = new List<float>();
            public Vector2 position;
            public float heading;
            public float distance;

            public void Add(float width)
            {
                nodes.Add(new CourseNode(position.x, position.y, width));
                distances.Add(distance);
            }

            public int Count => nodes.Count;

            public void Truncate(int count, Vector2 savedPosition, float savedHeading, float savedDistance)
            {
                nodes.RemoveRange(count, nodes.Count - count);
                distances.RemoveRange(count, distances.Count - count);
                position = savedPosition;
                heading = savedHeading;
                distance = savedDistance;
            }

            public bool Clear(int recent, float clearance)
            {
                for (int i = 0; i < nodes.Count - recent; i++)
                {
                    if ((nodes[i].position - position).magnitude < clearance)
                    {
                        return false;
                    }
                }
                return true;
            }

            public void Advance(float length, float width)
            {
                float radians = heading * Mathf.Deg2Rad;
                position += new Vector2(Mathf.Sin(radians), Mathf.Cos(radians)) * length;
                distance += length;
                Add(width);
            }
        }

        public static LevelDefinition Generate(AdventureDifficulty difficulty, int seed, float intensity = 1f)
        {
            var random = new Random(seed);
            var level = ScriptableObject.CreateInstance<LevelDefinition>();
            level.id = "macera_" + difficulty.id;
            level.displayName = "Macera · " + difficulty.displayName;
            level.damageMultiplier = difficulty.damageMultiplier;

            float width = difficulty.riverWidth;
            var path = new Path { position = new Vector2(0f, -6f), distance = -6f };
            path.Add(width);
            path.position = Vector2.zero;
            path.distance = 0f;
            path.Add(width);
            for (int i = 0; i < 3; i++)
            {
                path.Advance(Step, width);
            }

            while (path.distance < difficulty.length - 25f)
            {
                if (random.NextDouble() < difficulty.zigzagChance && TryZigzag(path, random, width))
                {
                    continue;
                }
                double roll = random.NextDouble();
                if (roll < difficulty.narrowChance)
                {
                    float narrow = Mathf.Max(8f, width * Range(random, 0.5f, 0.62f));
                    path.Advance(Step, Mathf.Lerp(width, narrow, 0.5f));
                    path.Advance(Step, narrow);
                    path.Advance(Step, narrow);
                    path.Advance(Step, Mathf.Lerp(width, narrow, 0.5f));
                    path.Advance(Step, width);
                }
                else if (roll < difficulty.narrowChance + 0.6f)
                {
                    float bend = Range(random, difficulty.maxBend * 0.4f, difficulty.maxBend) * (random.NextDouble() < 0.5 ? -1f : 1f);
                    float target = Mathf.Clamp(path.heading + bend, -MaxHeading, MaxHeading);
                    int steps = random.Next(3, 6);
                    float start = path.heading;
                    for (int i = 1; i <= steps; i++)
                    {
                        path.heading = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, i / (float)steps));
                        path.Advance(Step, width * Range(random, 0.94f, 1.06f));
                    }
                }
                else
                {
                    int steps = random.Next(2, 5);
                    for (int i = 0; i < steps; i++)
                    {
                        path.heading = Mathf.Lerp(path.heading, 0f, 0.25f);
                        path.Advance(Step, width);
                    }
                }
            }

            path.heading = Mathf.Lerp(path.heading, 0f, 0.5f);
            path.Advance(Step, width);
            path.Advance(Step, width);
            int finish = path.nodes.Count - 1;
            path.Advance(7f, width);

            level.nodes = path.nodes.ToArray();
            level.startNode = 1;
            level.checkpointNodes = Checkpoints(path, difficulty, finish);

            var currents = new List<CurrentSpec>();
            var obstacles = new List<ObstacleSpec>();
            var hazards = new List<HazardSpec>();
            var rocks = new List<RockSpec>();
            var thorns = new List<ThornSpec>();
            var mills = new List<MillSpec>();
            var geysers = new List<GeyserSpec>();
            var barrels = new List<BarrelSpec>();
            var ramps = new List<RampSpec>();
            var specials = new List<SpecialSpec>();
            var checkpointDistances = new List<float>();
            foreach (int node in level.checkpointNodes)
            {
                checkpointDistances.Add(path.distances[node]);
            }

            float limit = path.distances[finish] - 25f;
            for (float s = 40f; s < limit; s += difficulty.featureSpacing * Range(random, 0.75f, 1.25f))
            {
                if (NearCheckpoint(checkpointDistances, s))
                {
                    continue;
                }
                int node = NodeAt(path, s);
                Vector3 center = level.Center(node);
                Vector3 forward = level.Direction(node);
                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
                float riverWidth = level.Width(node);
                float tightest = MinWidth(level, path, s, NarrowWindow);
                bool open = tightest >= SweeperClearance;
                bool roomy = tightest >= ClusterClearance;
                float yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
                bool curved = Vector3.Angle(level.Direction(Mathf.Max(0, node - 3)), level.Direction(Mathf.Min(level.NodeCount - 1, node + 3))) > 30f;

                float total = difficulty.crocodileWeight + difficulty.rockWeight + difficulty.currentWeight + difficulty.thornWeight + difficulty.millWeight + difficulty.rockFieldWeight
                    + difficulty.driftWeight + difficulty.geyserWeight + difficulty.barrelWeight + difficulty.rampWeight + difficulty.specialWeight;
                float pick = Range(random, 0f, Mathf.Max(0.0001f, total));
                if ((pick -= difficulty.driftWeight) < 0f && open && SweepLane(riverWidth * 0.5f, 3f, random, out float driftOffset, out float driftReach))
                {
                    obstacles.Add(new ObstacleSpec
                    {
                        position = center + Vector3.up * 0.3f + right * driftOffset + forward * Range(random, -2f, 2f),
                        size = new Vector3(6f, 1.1f, 1.3f),
                        travel = right * driftReach,
                        speed = difficulty.crocodileSpeed * Range(random, 0.6f, 0.85f),
                        kind = ObstacleKind.Drift
                    });
                    hazards.Add(new HazardSpec { position = center, radius = riverWidth * 0.4f });
                }
                else if ((pick -= difficulty.geyserWeight) < 0f)
                {
                    float lateral = Range(random, -riverWidth * 0.3f, riverWidth * 0.3f);
                    Vector3 position = center + right * lateral;
                    geysers.Add(new GeyserSpec { position = position, radius = 2.4f, period = Range(random, 3.5f, 5.5f), phase = Range(random, 0f, 5f) });
                    hazards.Add(new HazardSpec { position = position, radius = 4.5f });
                }
                else if ((pick -= difficulty.barrelWeight) < 0f && roomy)
                {
                    float gap = Range(random, -riverWidth * 0.5f + 4f, riverWidth * 0.5f - 4f);
                    int count = random.Next(2, 4);
                    for (int b = 0; b < count; b++)
                    {
                        for (int attempt = 0; attempt < 6; attempt++)
                        {
                            float lateral = Range(random, -riverWidth * 0.5f + 1.8f, riverWidth * 0.5f - 1.8f);
                            if (Mathf.Abs(lateral - gap) < 4f)
                            {
                                continue;
                            }
                            float shift = Range(random, -4f, 4f);
                            Vector3 position = center + right * lateral + forward * shift;
                            if (TooClose(rocks, position, 3f) || TooClose(barrels, position, 3f) || TightAt(level, path, position) < ClusterClearance)
                            {
                                continue;
                            }
                            barrels.Add(new BarrelSpec { position = position });
                            hazards.Add(new HazardSpec { position = position, radius = 3.5f });
                            break;
                        }
                    }
                }
                else if ((pick -= difficulty.rampWeight) < 0f && !curved)
                {
                    float rampWidth = Mathf.Clamp(riverWidth * 0.42f, 5f, 9f);
                    float lateral = Range(random, -1f, 1f) * (riverWidth * 0.5f - rampWidth * 0.5f - 1.5f);
                    ramps.Add(new RampSpec { position = center + right * lateral, yaw = yaw, width = rampWidth });
                }
                else if ((pick -= difficulty.specialWeight) < 0f && open)
                {
                    specials.Add(new SpecialSpec { position = center, yaw = yaw, reach = riverWidth * 0.5f - 3f });
                    hazards.Add(new HazardSpec { position = center, radius = riverWidth * 0.4f });
                }
                else if ((pick -= difficulty.millWeight) < 0f && riverWidth > 12f && roomy)
                {
                    float radius = Mathf.Min(riverWidth * 0.32f, 5.5f);
                    float lateral = (riverWidth * 0.5f - radius * 0.55f) * (random.NextDouble() < 0.5 ? -1f : 1f);
                    Vector3 position = center + right * lateral;
                    mills.Add(new MillSpec
                    {
                        position = position,
                        yaw = yaw,
                        radius = radius,
                        speed = difficulty.millSpeed * Range(random, 0.85f, 1.15f) * (random.NextDouble() < 0.5 ? -1f : 1f)
                    });
                    hazards.Add(new HazardSpec { position = position, radius = radius + 2f });
                }
                else if ((pick -= difficulty.rockFieldWeight) < 0f && roomy)
                {
                    int count = random.Next(5, 10);
                    float lane = riverWidth * 0.5f - 4f;
                    float laneStart = Range(random, -lane, lane) * 0.5f;
                    float laneSwing = Range(random, 0.15f, 0.3f) * riverWidth * (random.NextDouble() < 0.5 ? -1f : 1f);
                    for (int r = 0; r < count; r++)
                    {
                        float along = (r - (count - 1) * 0.5f) * Range(random, 2.6f, 3.6f);
                        float lateral = Range(random, -riverWidth * 0.5f + 1.8f, riverWidth * 0.5f - 1.8f);
                        if (r % 2 == 0)
                        {
                            lateral = Mathf.Lerp(lateral, (r % 4 == 0 ? -1f : 1f) * riverWidth * 0.22f, 0.6f);
                        }
                        float radius = Range(random, 0.8f, 1.35f);
                        float corridor = Mathf.Clamp(laneStart + laneSwing * Mathf.Sin(along * 0.3f), -lane, lane);
                        if (Mathf.Abs(lateral - corridor) < 2.9f + radius)
                        {
                            continue;
                        }
                        Vector3 position = center + right * lateral + forward * along;
                        if (TooClose(rocks, position, 2.6f) || TightAt(level, path, position) < ClusterClearance)
                        {
                            continue;
                        }
                        rocks.Add(new RockSpec { position = position, radius = radius });
                        hazards.Add(new HazardSpec { position = position, radius = radius + 1.8f });
                    }
                }
                else if ((pick -= difficulty.crocodileWeight) < 0f && open && SweepLane(riverWidth * 0.5f, 2.5f, random, out float crocodileOffset, out float crocodileReach))
                {
                    obstacles.Add(new ObstacleSpec
                    {
                        position = center + Vector3.up * 0.4f + right * crocodileOffset,
                        size = new Vector3(5f, 1.5f, 1.2f),
                        travel = right * crocodileReach,
                        speed = difficulty.crocodileSpeed * Range(random, 0.85f, 1.15f)
                    });
                    hazards.Add(new HazardSpec { position = center, radius = riverWidth * 0.45f });
                }
                else if ((pick -= difficulty.rockWeight) < 0f && roomy)
                {
                    float gap = Range(random, -riverWidth * 0.5f + 4f, riverWidth * 0.5f - 4f);
                    int count = random.Next(1, difficulty.maxRocksPerCluster + 1);
                    for (int r = 0; r < count; r++)
                    {
                        for (int attempt = 0; attempt < 6; attempt++)
                        {
                            float lateral = Range(random, -riverWidth * 0.5f + 1.8f, riverWidth * 0.5f - 1.8f);
                            if (Mathf.Abs(lateral - gap) < 4f)
                            {
                                continue;
                            }
                            float radius = Range(random, 0.9f, 1.5f);
                            float shift = Range(random, -3f, 3f);
                            Vector3 position = center + right * lateral + forward * shift;
                            if (TightAt(level, path, position) < ClusterClearance)
                            {
                                continue;
                            }
                            rocks.Add(new RockSpec { position = position, radius = radius });
                            hazards.Add(new HazardSpec { position = position, radius = radius + 2.2f });
                            break;
                        }
                    }
                }
                else if ((pick -= difficulty.currentWeight) < 0f && !curved)
                {
                    double kind = random.NextDouble();
                    Vector3 direction;
                    float strength = difficulty.currentStrength * (random.NextDouble() < difficulty.ragingChance ? 1.6f : 1f);
                    if (kind < 0.4)
                    {
                        direction = right * (random.NextDouble() < 0.5 ? -1f : 1f) + forward * 0.15f;
                    }
                    else if (kind < 0.7)
                    {
                        direction = forward;
                        strength *= 1.1f;
                    }
                    else
                    {
                        direction = -forward;
                        strength *= 0.75f;
                    }
                    float length = Range(random, 16f, 26f);
                    if (kind >= 0.7)
                    {
                        strength = Mathf.Min(strength, MaxBackCurrent * intensity);
                        length = Mathf.Min(length, 8f + 36f / Mathf.Max(1f, strength));
                    }
                    else if (kind < 0.4)
                    {
                        strength = Mathf.Min(strength, MaxSideCurrent * intensity);
                        length = Mathf.Min(length, 8f + 60f / Mathf.Max(1f, strength));
                    }
                    else
                    {
                        strength = Mathf.Min(strength, MaxCurrent * intensity);
                    }
                    currents.Add(new CurrentSpec
                    {
                        position = center + Vector3.up * 0.5f,
                        size = new Vector3(riverWidth - 1f, 4f, length),
                        direction = direction.normalized,
                        strength = strength,
                        yaw = yaw
                    });
                    if (kind < 0.4 || kind >= 0.7)
                    {
                        hazards.Add(new HazardSpec { position = center, radius = length * 0.5f });
                    }
                }
                else if (roomy)
                {
                    int span = random.Next(2, 5);
                    thorns.Add(new ThornSpec
                    {
                        startNode = Mathf.Max(1, node - span / 2),
                        endNode = Mathf.Min(finish - 1, node + span - span / 2),
                        side = random.NextDouble() < 0.5 ? -1f : 1f
                    });
                }
            }

            var pickups = new List<PickupSpec>();
            for (float s = 30f; s < limit; s += difficulty.pickupSpacing * Range(random, 0.8f, 1.2f))
            {
                int node = NodeAt(path, s);
                Vector3 center = level.Center(node);
                Vector3 right = Vector3.Cross(Vector3.up, level.Direction(node)).normalized;
                Vector3 position = center + right * Range(random, -level.Width(node) * 0.5f + 3f, level.Width(node) * 0.5f - 3f) + Vector3.up * 0.35f;
                if (TooClose(rocks, position, 3.5f) || TooClose(barrels, position, 3.5f))
                {
                    continue;
                }
                pickups.Add(new PickupSpec { position = position, kind = RandomPickup(random) });
            }

            level.currents = currents.ToArray();
            level.obstacles = obstacles.ToArray();
            level.hazards = hazards.ToArray();
            level.rocks = rocks.ToArray();
            level.thorns = thorns.ToArray();
            level.pickups = pickups.ToArray();
            level.mills = mills.ToArray();
            level.geysers = geysers.ToArray();
            level.barrels = barrels.ToArray();
            level.ramps = ramps.ToArray();
            level.specials = specials.ToArray();
            level.fallChance = difficulty.fallChance;

            float length2 = path.distances[finish];
            level.parTime = Mathf.Round(length2 / difficulty.expectedSpeed);
            return level;
        }

        static bool TryZigzag(Path path, Random random, float width)
        {
            int count = path.Count;
            Vector2 position = path.position;
            float heading = path.heading;
            float distance = path.distance;
            float side = random.NextDouble() < 0.5 ? -1f : 1f;
            var headings = new List<float>();
            if (random.NextDouble() < 0.5)
            {
                float sideways = Range(random, 85f, 115f) * side;
                headings.AddRange(new[] { sideways * 0.35f, sideways * 0.7f, sideways, sideways, sideways });
                if (random.NextDouble() < 0.5)
                {
                    headings.Add(sideways);
                }
                headings.AddRange(new[] { sideways * 0.6f, sideways * 0.25f, 0f, 0f });
            }
            else
            {
                float swing = Range(random, 65f, 85f) * side;
                headings.AddRange(new[] { swing * 0.4f, swing * 0.8f, swing, swing * 0.5f, 0f, -swing * 0.5f, -swing, -swing * 0.8f, -swing * 0.4f, 0f });
            }

            float clearance = width * 1.3f + 10f;
            foreach (float target in headings)
            {
                path.heading = target;
                path.Advance(Step, width);
                if (!path.Clear(6, clearance))
                {
                    path.Truncate(count, position, heading, distance);
                    return false;
                }
            }
            return true;
        }

        static PickupKind RandomPickup(Random random)
        {
            double roll = random.NextDouble();
            if (roll < 0.16) return PickupKind.Beans;
            if (roll < 0.32) return PickupKind.Pastirma;
            if (roll < 0.42) return PickupKind.Onion;
            if (roll < 0.54) return PickupKind.Chickpea;
            if (roll < 0.64) return PickupKind.Pepper;
            if (roll < 0.72) return PickupKind.Egg;
            if (roll < 0.86) return PickupKind.Repair;
            if (roll < 0.93) return PickupKind.Bubble;
            return PickupKind.Magnet;
        }

        static int[] Checkpoints(Path path, AdventureDifficulty difficulty, int finish)
        {
            var result = new List<int> { 1 };
            if (difficulty.checkpointCount > 2)
            {
                int inner = difficulty.checkpointCount - 2;
                float total = path.distances[finish] - path.distances[1];
                int node = 2;
                for (int k = 1; k <= inner; k++)
                {
                    float target = path.distances[1] + total * k / (inner + 1);
                    while (node < finish - 2 && path.distances[node] < target)
                    {
                        node++;
                    }
                    if (node >= finish - 2)
                    {
                        break;
                    }
                    result.Add(node);
                    node++;
                }
                result.Add(finish);
                return result.ToArray();
            }
            float next = difficulty.checkpointSpacing;
            for (int i = 2; i < finish - 2; i++)
            {
                if (path.distances[i] >= next)
                {
                    result.Add(i);
                    next = path.distances[i] + difficulty.checkpointSpacing;
                }
            }
            result.Add(finish);
            return result.ToArray();
        }

        static bool NearCheckpoint(List<float> checkpoints, float distance)
        {
            foreach (float checkpoint in checkpoints)
            {
                if (Mathf.Abs(checkpoint - distance) < CheckpointClearance)
                {
                    return true;
                }
            }
            return false;
        }

        static bool TooClose(List<RockSpec> rocks, Vector3 position, float distance)
        {
            foreach (RockSpec rock in rocks)
            {
                Vector3 offset = rock.position - position;
                offset.y = 0f;
                if (offset.magnitude < distance + rock.radius)
                {
                    return true;
                }
            }
            return false;
        }

        static bool TooClose(List<BarrelSpec> barrels, Vector3 position, float distance)
        {
            foreach (BarrelSpec barrel in barrels)
            {
                Vector3 offset = barrel.position - position;
                offset.y = 0f;
                if (offset.magnitude < distance)
                {
                    return true;
                }
            }
            return false;
        }

        public const float NarrowWindow = 14f;
        public const float SweeperLane = 6f;
        const float BankMargin = 0.5f;
        const float MinSweep = 1f;

        public static bool SweepLane(float half, float bodyHalf, Random random, out float offset, out float reach)
        {
            float span = 2f * half - SweeperLane - BankMargin;
            reach = (span - 2f * bodyHalf) * 0.5f;
            if (reach < MinSweep)
            {
                offset = 0f;
                return false;
            }
            float side = random.NextDouble() < 0.5 ? -1f : 1f;
            offset = side * (SweeperLane - BankMargin) * 0.5f;
            return true;
        }
        public const float SweeperClearance = 15f;
        public const float ClusterClearance = 12f;

        static float MinWidth(LevelDefinition level, Path path, float distance, float window)
        {
            float min = float.MaxValue;
            for (int i = 0; i < path.distances.Count; i++)
            {
                if (Mathf.Abs(path.distances[i] - distance) <= window)
                {
                    min = Mathf.Min(min, level.Width(i));
                }
            }
            return min == float.MaxValue ? level.Width(NodeAt(path, distance)) : min;
        }

        static float TightAt(LevelDefinition level, Path path, Vector3 position)
        {
            int nearest = 0;
            float best = float.MaxValue;
            for (int i = 0; i < path.distances.Count; i++)
            {
                Vector3 delta = level.Center(i) - position;
                delta.y = 0f;
                if (delta.sqrMagnitude < best)
                {
                    best = delta.sqrMagnitude;
                    nearest = i;
                }
            }
            return MinWidth(level, path, path.distances[nearest], NarrowWindow);
        }

        static int NodeAt(Path path, float distance)
        {
            int best = 0;
            float bestDelta = float.MaxValue;
            for (int i = 0; i < path.distances.Count; i++)
            {
                float delta = Mathf.Abs(path.distances[i] - distance);
                if (delta < bestDelta)
                {
                    bestDelta = delta;
                    best = i;
                }
            }
            return best;
        }

        static float Range(Random random, float min, float max)
        {
            return min + (max - min) * (float)random.NextDouble();
        }
    }
}
