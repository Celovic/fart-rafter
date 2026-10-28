using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace FartRafters
{
    public static class BiomeFeatures
    {
        const float CheckpointClearance = 10f;
        const float MaxBend = 12f;

        public static void Apply(LevelDefinition level, Biome biome, float severity, float intensity, int seed)
        {
            level.ice = Array.Empty<IceSpec>();
            level.whirlpools = Array.Empty<WhirlpoolSpec>();
            level.heights = Array.Empty<float>();
            if (biome == Biome.Fun)
            {
                AddSlopes(level, Mathf.Clamp01(severity), intensity, new Random(seed * 5 + 17));
            }
            else if (biome == Biome.Polar)
            {
                AddIce(level, Mathf.Clamp01(severity), intensity, new Random(seed * 7 + 11));
            }
            else if (biome == Biome.Ocean || biome == Biome.Lava)
            {
                ConvertMills(level);
            }
            ConvertSpecials(level, biome, new Random(seed * 3 + 29));
            EnsureSweepLanes(level);
        }

        const float LaneMargin = 0.5f;
        const float MinSweep = 1f;

        public static void EnsureSweepLanes(LevelDefinition level)
        {
            if (level.obstacles == null || level.obstacles.Length == 0 || level.NodeCount < 2)
            {
                return;
            }
            var kept = new List<ObstacleSpec>(level.obstacles.Length);
            foreach (ObstacleSpec obstacle in level.obstacles)
            {
                ObstacleSpec spec = obstacle;
                if (FitLane(level, ref spec))
                {
                    kept.Add(spec);
                }
            }
            level.obstacles = kept.ToArray();
        }

        static bool FitLane(LevelDefinition level, ref ObstacleSpec spec)
        {
            int node = NearestNode(level, spec.position);
            Vector3 center = level.Center(node);
            Vector3 right = Vector3.Cross(Vector3.up, level.Direction(node)).normalized;
            float half = level.Width(node) * 0.5f;
            float bodyHalf = Mathf.Max(spec.size.x, spec.size.z) * 0.5f;
            Vector3 flat = spec.position - center;
            flat.y = 0f;
            float offset = Vector3.Dot(flat, right);
            Vector3 travel = spec.travel;
            float reach = Mathf.Abs(Vector3.Dot(travel, right));
            float left = offset - reach - bodyHalf + half;
            float rightGap = half - (offset + reach + bodyHalf);
            if (Mathf.Max(left, rightGap) >= CourseGenerator.SweeperLane)
            {
                return true;
            }
            float free = rightGap >= left ? 1f : -1f;
            float span = 2f * half - CourseGenerator.SweeperLane - LaneMargin;
            float newReach = (span - 2f * bodyHalf) * 0.5f;
            float newOffset = -free * (CourseGenerator.SweeperLane - LaneMargin) * 0.5f;
            if (newReach < 0f)
            {
                return false;
            }
            spec.position += right * (newOffset - offset);
            if (newReach < MinSweep)
            {
                spec.travel = Vector3.zero;
            }
            else if (travel.sqrMagnitude > 0.0001f)
            {
                spec.travel = travel.normalized * Mathf.Min(travel.magnitude, newReach / Mathf.Max(0.2f, Mathf.Abs(Vector3.Dot(travel.normalized, right))));
            }
            return true;
        }

        static void ConvertSpecials(LevelDefinition level, Biome biome, Random random)
        {
            if (level.specials == null || level.specials.Length == 0)
            {
                return;
            }
            var obstacles = new List<ObstacleSpec>(level.obstacles);
            var geysers = new List<GeyserSpec>(level.geysers);
            var icicles = new List<IcicleSpec>(level.icicles);
            foreach (SpecialSpec special in level.specials)
            {
                Vector3 forward = Quaternion.Euler(0f, special.yaw, 0f) * Vector3.forward;
                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
                if (biome == Biome.Polar || biome == Biome.Lava || biome == Biome.Desert)
                {
                    int count = 2 + random.Next(2);
                    for (int i = 0; i < count; i++)
                    {
                        float lateral = Range(random, -special.reach, special.reach);
                        float along = (i - (count - 1) * 0.5f) * 5f;
                        icicles.Add(new IcicleSpec { position = special.position + right * lateral + forward * along, period = Range(random, 5f, 7f), phase = Range(random, 0f, 7f) });
                    }
                }
                else if (biome == Biome.Ocean)
                {
                    obstacles.Add(new ObstacleSpec
                    {
                        position = special.position + Vector3.up * 0.3f,
                        size = new Vector3(5f, 1f, 1f),
                        travel = right * special.reach,
                        speed = Range(random, 2f, 2.8f),
                        kind = ObstacleKind.Eel
                    });
                }
                else
                {
                    float lateral = Range(random, -special.reach * 0.6f, special.reach * 0.6f);
                    geysers.Add(new GeyserSpec { position = special.position + right * lateral, radius = 2.4f, period = Range(random, 3.5f, 5.5f), phase = Range(random, 0f, 5f) });
                }
            }
            level.obstacles = obstacles.ToArray();
            level.geysers = geysers.ToArray();
            level.icicles = icicles.ToArray();
            level.specials = Array.Empty<SpecialSpec>();
        }

        static void ConvertMills(LevelDefinition level)
        {
            if (level.mills == null || level.mills.Length == 0)
            {
                return;
            }
            var pools = new List<WhirlpoolSpec>();
            foreach (MillSpec mill in level.mills)
            {
                int node = NearestNode(level, mill.position);
                Vector3 center = level.Center(node);
                Vector3 right = Vector3.Cross(Vector3.up, level.Direction(node)).normalized;
                float lateral = Vector3.Dot(mill.position - center, right);
                float half = level.Width(node) * 0.5f;
                float radius = Mathf.Clamp(mill.radius * 1.5f, 5f, half * 1.1f);
                float offset = (lateral < 0f ? -1f : 1f) * Mathf.Max(0f, half - radius * 0.55f);
                pools.Add(new WhirlpoolSpec
                {
                    position = center + right * offset,
                    radius = radius,
                    swirl = Mathf.Clamp(Mathf.Abs(mill.speed) / 15f, 1.8f, 4.5f),
                    direction = mill.speed < 0f ? -1f : 1f
                });
            }
            level.whirlpools = pools.ToArray();
            level.mills = Array.Empty<MillSpec>();
        }

        static void AddIce(LevelDefinition level, float severity, float intensity, Random random)
        {
            int count = level.NodeCount;
            if (count < 6)
            {
                return;
            }
            var distance = new float[count];
            for (int i = 1; i < count; i++)
            {
                distance[i] = distance[i - 1] + Vector3.Distance(level.Center(i - 1), level.Center(i));
            }
            float start = distance[Mathf.Min(level.startNode + 2, count - 1)] + 10f;
            float end = distance[Mathf.Clamp(level.FinishNode, 0, count - 1)] - 20f;
            if (end - start < 30f)
            {
                return;
            }
            int wanted = Mathf.Clamp(Mathf.RoundToInt((end - start) / Mathf.Lerp(260f, 140f, severity) * intensity), 1, 40);
            var zones = new List<IceSpec>();
            var spans = new List<Vector2>();
            for (int attempt = 0; attempt < wanted * 14 && zones.Count < wanted; attempt++)
            {
                float span = Mathf.Lerp(14f, 30f, severity) * Range(random, 0.8f, 1.2f);
                float s = Range(random, start + span * 0.5f, end - span * 0.5f);
                if (Overlaps(spans, s, span) || NearCheckpoint(level, distance, s, span))
                {
                    continue;
                }
                if (!Straight(level, distance, s, span, out float width))
                {
                    continue;
                }
                Vector3 center = PointAt(level, distance, s, out Vector3 forward);
                if (Blocked(level, center, forward, span))
                {
                    continue;
                }
                zones.Add(new IceSpec
                {
                    position = center,
                    yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg,
                    size = new Vector2(Mathf.Max(4f, width - 0.4f), span)
                });
                spans.Add(new Vector2(s, span));
            }
            level.ice = zones.ToArray();
        }

        static void AddSlopes(LevelDefinition level, float severity, float intensity, Random random)
        {
            int count = level.NodeCount;
            if (count < 8)
            {
                return;
            }
            var segment = new float[count];
            for (int i = 1; i < count; i++)
            {
                segment[i] = Vector3.Distance(level.Center(i - 1), level.Center(i));
            }
            var drop = new float[count];
            int first = level.startNode + 3;
            int last = Mathf.Max(first + 1, level.FinishNode - 4);
            float length = 0f;
            for (int i = first; i < last; i++)
            {
                length += segment[i];
            }
            int wanted = Mathf.Clamp(Mathf.RoundToInt(length / Mathf.Lerp(150f, 80f, severity) * intensity), 1, 30);
            int placed = 0;
            for (int attempt = 0; attempt < wanted * 12 && placed < wanted; attempt++)
            {
                int node = random.Next(first, last);
                if (!FreeForDrop(level, drop, node))
                {
                    continue;
                }
                float amount = Mathf.Lerp(2f, 4f, severity) * Range(random, 0.8f, 1.2f);
                drop[node] = amount * 0.5f;
                drop[node + 1] = amount * 0.5f;
                placed++;
            }
            var heights = new float[count];
            for (int i = 1; i < count; i++)
            {
                heights[i] = heights[i - 1] - segment[i] * 0.012f - drop[i];
            }
            level.heights = heights;
        }

        static bool FreeForDrop(LevelDefinition level, float[] drop, int node)
        {
            for (int k = node - 4; k <= node + 5; k++)
            {
                if (k < 1 || k >= level.NodeCount || drop[k] > 0f)
                {
                    return false;
                }
            }
            Vector3 forward = level.Direction(node);
            for (int k = node - 2; k <= node + 4; k++)
            {
                if (Vector3.Angle(level.Direction(k), forward) > 10f)
                {
                    return false;
                }
            }
            foreach (int checkpoint in level.checkpointNodes)
            {
                if (checkpoint >= node - 2 && checkpoint <= node + 3)
                {
                    return false;
                }
            }
            Vector3 center = Vector3.Lerp(level.Center(node - 1), level.Center(node + 1), 0.5f);
            foreach (CurrentSpec current in level.currents)
            {
                if (Along(current.position, center, forward, out float lateral) < 12f + current.size.z * 0.5f && lateral < 30f)
                {
                    return false;
                }
            }
            foreach (ObstacleSpec obstacle in level.obstacles)
            {
                if (Along(obstacle.position, center, forward, out float lateral) < 16f && lateral < 30f)
                {
                    return false;
                }
            }
            foreach (MillSpec mill in level.mills)
            {
                if (Along(mill.position, center, forward, out float lateral) < 14f + mill.radius && lateral < 30f)
                {
                    return false;
                }
            }
            foreach (RockSpec rock in level.rocks)
            {
                if (Along(rock.position, center, forward, out float lateral) < 12f && lateral < 30f)
                {
                    return false;
                }
            }
            return true;
        }

        static bool Overlaps(List<Vector2> spans, float s, float span)
        {
            foreach (Vector2 other in spans)
            {
                if (Mathf.Abs(other.x - s) < (other.y + span) * 0.5f + 25f)
                {
                    return true;
                }
            }
            return false;
        }

        static bool NearCheckpoint(LevelDefinition level, float[] distance, float s, float span)
        {
            foreach (int node in level.checkpointNodes)
            {
                if (node >= 0 && node < distance.Length && Mathf.Abs(distance[node] - s) < span * 0.5f + CheckpointClearance)
                {
                    return true;
                }
            }
            return false;
        }

        static bool Straight(LevelDefinition level, float[] distance, float s, float span, out float width)
        {
            width = float.MaxValue;
            Vector3 center = PointAt(level, distance, s, out Vector3 forward);
            for (int i = 0; i < distance.Length; i++)
            {
                if (distance[i] < s - span * 0.5f - 4f || distance[i] > s + span * 0.5f + 4f)
                {
                    continue;
                }
                if (Vector3.Angle(level.Direction(i), forward) > MaxBend)
                {
                    return false;
                }
                width = Mathf.Min(width, level.Width(i));
            }
            if (width == float.MaxValue)
            {
                width = level.Width(NearestNode(level, center));
            }
            return true;
        }

        static bool Blocked(LevelDefinition level, Vector3 center, Vector3 forward, float span)
        {
            float half = span * 0.5f;
            foreach (CurrentSpec current in level.currents)
            {
                if (Along(current.position, center, forward, out float lateral) < half + current.size.z * 0.5f + 3f && lateral < 30f)
                {
                    return true;
                }
            }
            foreach (ObstacleSpec obstacle in level.obstacles)
            {
                if (Along(obstacle.position, center, forward, out float lateral) < half + 6f && lateral < 30f)
                {
                    return true;
                }
            }
            foreach (MillSpec mill in level.mills)
            {
                if (Along(mill.position, center, forward, out float lateral) < half + mill.radius + 3f && lateral < 30f)
                {
                    return true;
                }
            }
            return false;
        }

        static float Along(Vector3 point, Vector3 center, Vector3 forward, out float lateral)
        {
            Vector3 offset = point - center;
            offset.y = 0f;
            float along = Vector3.Dot(offset, forward);
            lateral = (offset - forward * along).magnitude;
            return Mathf.Abs(along);
        }

        static Vector3 PointAt(LevelDefinition level, float[] distance, float s, out Vector3 forward)
        {
            for (int i = 0; i < distance.Length - 1; i++)
            {
                if (s <= distance[i + 1])
                {
                    float t = Mathf.InverseLerp(distance[i], distance[i + 1], s);
                    forward = Vector3.Slerp(level.Direction(i), level.Direction(i + 1), t).normalized;
                    return Vector3.Lerp(level.Center(i), level.Center(i + 1), t);
                }
            }
            forward = level.Direction(level.NodeCount - 1);
            return level.Center(level.NodeCount - 1);
        }

        static int NearestNode(LevelDefinition level, Vector3 position)
        {
            int best = 0;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < level.NodeCount; i++)
            {
                Vector3 offset = level.Center(i) - position;
                offset.y = 0f;
                if (offset.sqrMagnitude < bestDistance)
                {
                    bestDistance = offset.sqrMagnitude;
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
