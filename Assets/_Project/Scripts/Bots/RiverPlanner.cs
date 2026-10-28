using System.Collections.Generic;
using UnityEngine;

namespace FartRafters
{
    public class RiverPlanner
    {
        struct Zone
        {
            public Vector2 position;
            public Vector2 halfSize;
            public float cos;
            public float sin;
            public Vector2 flow;
            public float along;
            public float reach;
        }

        struct Goodie
        {
            public Pickup pickup;
            public PickupKind kind;
            public Vector2 position;
            public float along;
        }

        public struct Settings
        {
            public float lookAhead;
            public float step;
            public int lanes;
            public float margin;
            public float travelSpeed;
            public bool collectPickups;
            public bool readCurrents;
            public bool predictCrocodiles;
            public bool timedHazards;
            public bool invincible;
            public float health;
        }

        public struct Result
        {
            public Vector3 target;
            public float danger;
            public float lateralChange;
        }

        public const float RaftRadius = 1.9f;
        const float Blocked = 400f;

        static RiverPlanner cached;
        static LevelDefinition cachedLevel;

        readonly LevelDefinition level;
        readonly Vector2[] points;
        readonly float[] distance;
        readonly Vector2[] right;
        readonly float[] halfWidth;
        readonly List<Zone> zones = new List<Zone>();
        readonly List<Goodie> goodies = new List<Goodie>();
        readonly List<Zone> nearZones = new List<Zone>();
        readonly List<Goodie> nearGoodies = new List<Goodie>();
        readonly List<MovingObstacle> nearCrocodiles = new List<MovingObstacle>();

        float[,] cost = new float[1, 1];
        float[,] best = new float[1, 1];
        int[,] from = new int[1, 1];
        float[,] lateralOf = new float[1, 1];
        Vector2[,] pointOf = new Vector2[1, 1];
        int[] path = new int[1];
        float[] midCost = new float[1];

        public float Length => distance[distance.Length - 1];

        public static RiverPlanner For(LevelDefinition level)
        {
            if (level == null || level.NodeCount < 2)
            {
                return null;
            }
            if (cached == null || cachedLevel != level)
            {
                cached = new RiverPlanner(level);
                cachedLevel = level;
            }
            return cached;
        }

        RiverPlanner(LevelDefinition level)
        {
            this.level = level;
            int count = level.NodeCount;
            points = new Vector2[count];
            distance = new float[count];
            right = new Vector2[count];
            halfWidth = new float[count];
            for (int i = 0; i < count; i++)
            {
                points[i] = level.nodes[i].position;
                halfWidth[i] = level.nodes[i].width * 0.5f;
                distance[i] = i == 0 ? 0f : distance[i - 1] + Vector2.Distance(points[i - 1], points[i]);
                Vector3 direction = level.Direction(i);
                right[i] = new Vector2(direction.z, -direction.x).normalized;
            }

            foreach (CurrentSpec current in level.currents)
            {
                float yaw = current.yaw * Mathf.Deg2Rad;
                Vector2 position = Flat(current.position);
                zones.Add(new Zone
                {
                    position = position,
                    halfSize = new Vector2(current.size.x, current.size.z) * 0.5f,
                    cos = Mathf.Cos(yaw),
                    sin = Mathf.Sin(yaw),
                    flow = Flat(current.direction).normalized * current.strength,
                    along = Project(position, out _, out _),
                    reach = new Vector2(current.size.x, current.size.z).magnitude * 0.5f
                });
            }
            foreach (Pickup pickup in Object.FindObjectsByType<Pickup>(FindObjectsSortMode.None))
            {
                Vector2 position = Flat(pickup.transform.position);
                goodies.Add(new Goodie { pickup = pickup, kind = pickup.kind, position = position, along = Project(position, out _, out _) });
            }
        }

        static Vector2 Flat(Vector3 point)
        {
            return new Vector2(point.x, point.z);
        }

        public float Project(Vector2 point, out float lateral, out int segment)
        {
            float bestDistance = float.MaxValue;
            float along = 0f;
            lateral = 0f;
            segment = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector2 a = points[i];
                Vector2 ab = points[i + 1] - a;
                float lengthSquared = ab.sqrMagnitude;
                float t = lengthSquared > 0.0001f ? Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSquared) : 0f;
                Vector2 closest = a + ab * t;
                float d = (point - closest).sqrMagnitude;
                if (d < bestDistance)
                {
                    bestDistance = d;
                    along = distance[i] + Mathf.Sqrt(lengthSquared) * t;
                    Vector2 side = new Vector2(ab.y, -ab.x).normalized;
                    lateral = Vector2.Dot(point - closest, side);
                    segment = i;
                }
            }
            return along;
        }

        float ProjectNear(Vector2 point, int hint, out float lateral, out int segment)
        {
            float bestDistance = float.MaxValue;
            float along = 0f;
            lateral = 0f;
            segment = hint;
            int first = Mathf.Max(0, hint - 3);
            int last = Mathf.Min(points.Length - 2, hint + 3);
            for (int i = first; i <= last; i++)
            {
                Vector2 a = points[i];
                Vector2 ab = points[i + 1] - a;
                float lengthSquared = ab.sqrMagnitude;
                float t = lengthSquared > 0.0001f ? Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSquared) : 0f;
                Vector2 closest = a + ab * t;
                float d = (point - closest).sqrMagnitude;
                if (d < bestDistance)
                {
                    bestDistance = d;
                    along = distance[i] + Mathf.Sqrt(lengthSquared) * t;
                    Vector2 side = new Vector2(ab.y, -ab.x).normalized;
                    lateral = Vector2.Dot(point - closest, side);
                    segment = i;
                }
            }
            return along;
        }

        int SegmentAt(float along)
        {
            int low = 0;
            int high = distance.Length - 2;
            while (low < high)
            {
                int mid = (low + high + 1) / 2;
                if (distance[mid] <= along)
                {
                    low = mid;
                }
                else
                {
                    high = mid - 1;
                }
            }
            return Mathf.Clamp(low, 0, points.Length - 2);
        }

        public Vector2 PointAt(float along, float lateral, out float half, out int segment)
        {
            along = Mathf.Clamp(along, 0f, Length);
            segment = SegmentAt(along);
            float span = distance[segment + 1] - distance[segment];
            float t = span > 0.0001f ? (along - distance[segment]) / span : 0f;
            Vector2 center = Vector2.Lerp(points[segment], points[segment + 1], t);
            Vector2 side = Vector2.Lerp(right[segment], right[segment + 1], t).normalized;
            half = Mathf.Lerp(halfWidth[segment], halfWidth[segment + 1], t);
            return center + side * lateral;
        }

        public Vector3 Repulsion(Vector3 position, float margin)
        {
            Vector3 probe = new Vector3(position.x, ProbeHeight + CourseProfile.Height(position.x, position.z), position.z);
            Vector3 push = Vector3.zero;
            int count = Physics.OverlapSphereNonAlloc(probe, RaftRadius + margin + 1.5f, hits, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (!IsSolid(hits[i], out float weight))
                {
                    continue;
                }
                Vector3 closest = hits[i].ClosestPoint(probe);
                Vector3 offset = probe - closest;
                offset.y = 0f;
                float distance = offset.magnitude;
                float reach = margin + (weight > 1.5f ? 1.6f : 0.3f);
                float gap = distance - RaftRadius;
                if (gap < reach && distance > 0.001f)
                {
                    push += offset / distance * Mathf.Clamp01(1f - gap / reach) * 1.5f;
                }
            }
            foreach (Whirlpool pool in Whirlpool.Active)
            {
                Vector2 pushed = Push(Flat(position), Flat(pool.transform.position), pool.coreRadius, pool.radius * 0.6f + margin);
                push += new Vector3(pushed.x, 0f, pushed.y);
            }
            foreach (MovingObstacle crocodile in MovingObstacle.Active)
            {
                Vector2 point = Flat(position);
                Vector2 center = Flat(crocodile.transform.position);
                Vector2 axis = Flat(crocodile.TravelAxis).normalized;
                float half = Mathf.Max(0f, crocodile.Radius - crocodile.HalfWidth);
                Vector2 closest = center + axis * Mathf.Clamp(Vector2.Dot(point - center, axis), -half, half);
                Vector2 pushed = Push(point, closest, crocodile.HalfWidth, margin + 0.6f);
                push += new Vector3(pushed.x, 0f, pushed.y);
            }
            return push;
        }

        const float ProbeHeight = 0.3f;
        readonly Collider[] hits = new Collider[24];

        static bool IsSolid(Collider collider, out float weight)
        {
            weight = 1f;
            Rigidbody body = collider.attachedRigidbody;
            if (body != null && (!body.isKinematic || body.GetComponent<MovingObstacle>() != null))
            {
                return false;
            }
            if (collider.GetComponentInParent<LaunchRamp>() != null)
            {
                return false;
            }
            if (collider.GetComponent<ThornWall>() != null)
            {
                weight = 2.5f;
                return true;
            }
            DamageSource source = collider.GetComponentInParent<DamageSource>();
            if (source != null)
            {
                weight = Mathf.Max(1f, source.multiplier);
            }
            return true;
        }

        float SolidCost(Vector2 point, float margin)
        {
            return SolidCost(point, margin, -1f, false);
        }

        float SolidCost(Vector2 point, float margin, float time, bool invincible)
        {
            Vector3 probe = new Vector3(point.x, ProbeHeight + CourseProfile.Height(point.x, point.y), point.y);
            float total = 0f;
            int count = Physics.OverlapSphereNonAlloc(probe, RaftRadius + margin + 2.2f, hits, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (!IsSolid(hits[i], out float weight))
                {
                    continue;
                }
                Vector3 closest = hits[i].ClosestPoint(probe);
                Vector3 offset = probe - closest;
                offset.y = 0f;
                float gap = offset.magnitude - RaftRadius;
                float reach = margin + (weight > 2f ? 2.2f : 0.6f);
                if (gap < 0f)
                {
                    total += Blocked * weight * (1f + Mathf.Min(2f, -gap));
                }
                else if (gap < reach)
                {
                    float closeness = 1f - gap / reach;
                    total += closeness * closeness * 45f * weight;
                }
            }
            foreach (Whirlpool pool in Whirlpool.Active)
            {
                float gap = (point - Flat(pool.transform.position)).magnitude - RaftRadius;
                if (gap < pool.coreRadius)
                {
                    total += Blocked * 1.5f;
                }
                else if (gap < pool.radius * 0.85f + margin)
                {
                    float closeness = 1f - (gap - pool.coreRadius) / Mathf.Max(0.5f, pool.radius * 0.85f + margin - pool.coreRadius);
                    total += closeness * closeness * 70f;
                }
            }
            foreach (SoftHazard hazard in SoftHazard.Active)
            {
                float gap = (point - Flat(hazard.transform.position)).magnitude - RaftRadius;
                float reach = hazard.radius + margin;
                if (gap >= reach)
                {
                    continue;
                }
                float closeness = 1f - Mathf.Max(0f, gap) / Mathf.Max(0.5f, reach);
                bool timed = time >= 0f && (hazard.Drop != null || hazard.Spout != null);
                if (!timed)
                {
                    total += closeness * closeness * 90f * hazard.weight;
                    continue;
                }
                if (invincible && hazard.Drop != null)
                {
                    continue;
                }
                bool live = hazard.Drop != null ? hazard.Drop.StrikingAt(time) : hazard.Spout.EruptingAt(time);
                total += live ? closeness * closeness * 260f * hazard.weight : closeness * closeness * 6f;
            }
            return total;
        }

        static Vector2 Push(Vector2 point, Vector2 center, float radius, float margin)
        {
            Vector2 offset = point - center;
            float distance = offset.magnitude;
            float gap = distance - radius - RaftRadius;
            if (gap >= margin || distance < 0.001f)
            {
                return Vector2.zero;
            }
            return offset / distance * Mathf.Clamp01(1f - gap / margin) * 1.5f;
        }

        public float OpposingFlowAhead(Vector3 position, float distance, out bool inside, out float gap)
        {
            Vector2 point = Flat(position);
            float along = Project(point, out float lateral, out _);
            inside = false;
            gap = float.MaxValue;
            float worst = 0f;
            for (float d = 0f; d <= distance; d += 1.5f)
            {
                Vector2 probe = PointAt(along + d, lateral, out _, out int segment);
                Vector2 forward = DirectionAt(segment);
                foreach (Zone zone in zones)
                {
                    if (Mathf.Abs(zone.along - along) > zone.reach + distance || !Inside(zone, probe))
                    {
                        continue;
                    }
                    float with = Vector2.Dot(zone.flow, forward);
                    if (with < -1.5f)
                    {
                        worst = Mathf.Max(worst, -with);
                        gap = Mathf.Min(gap, d);
                        if (d < 0.01f)
                        {
                            inside = true;
                        }
                    }
                }
            }
            return worst;
        }

        public bool CrocodileBlocks(Vector3 position, float speed, float acceleration, float topSpeed, out bool inBand, out float gap, out float lane)
        {
            return CrocodileBlocks(position, speed, acceleration, topSpeed, 1f, out inBand, out gap, out lane);
        }

        public bool CrocodileBlocks(Vector3 position, float speed, float acceleration, float topSpeed, float clearance, out bool inBand, out float gap, out float lane)
        {
            Vector2 point = Flat(position);
            float along = Project(point, out float lateral, out int segment);
            Vector2 forward = DirectionAt(segment);
            Vector2 side = new Vector2(forward.y, -forward.x);
            inBand = false;
            gap = float.MaxValue;
            lane = lateral;
            float usable = Mathf.Max(0f, HalfWidthAt(along) - RaftRadius - 0.6f);
            bool any = false;
            float bestShift = float.MaxValue;
            for (int i = 0; i < 9; i++)
            {
                float candidate = Mathf.Lerp(-usable, usable, i / 8f);
                bool safe = true;
                foreach (MovingObstacle crocodile in MovingObstacle.Active)
                {
                    float crocodileAlong = Project(Flat(crocodile.Origin), out _, out _);
                    float band = crocodile.HalfWidth + RaftRadius + 1f;
                    float ahead = crocodileAlong - along;
                    if (ahead < -band || ahead > band + 12f)
                    {
                        continue;
                    }
                    any = true;
                    if (ahead - band <= 0f)
                    {
                        inBand = true;
                        candidate = lateral;
                    }
                    else
                    {
                        gap = Mathf.Min(gap, ahead - band);
                    }
                    float v = speed;
                    float s = 0f;
                    for (float t = 0f; t <= 5f && s <= ahead + band + 0.5f; t += 0.05f)
                    {
                        if (s >= ahead - band - 0.5f)
                        {
                            float shiftTime = 0.5f + Mathf.Abs(candidate - lateral) * 0.38f;
                            float shift = Mathf.Lerp(0f, candidate - lateral, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / shiftTime)));
                            Vector2 raft = point + forward * s + side * shift;
                            if (CrocodileGap(raft, crocodile, Flat(crocodile.PredictPosition(t))) < clearance)
                            {
                                safe = false;
                                break;
                            }
                        }
                        v = Mathf.Min(topSpeed, v + acceleration * 0.05f);
                        s += v * 0.05f;
                    }
                    if (!safe)
                    {
                        break;
                    }
                }
                if (!any)
                {
                    return false;
                }
                if (safe && Mathf.Abs(candidate - lateral) < bestShift)
                {
                    bestShift = Mathf.Abs(candidate - lateral);
                    lane = candidate;
                }
                if (inBand)
                {
                    break;
                }
            }
            return bestShift == float.MaxValue;
        }

        public float SpeedLimit(Vector3 position, Vector3 velocity, float cruise, float slowest)
        {
            Vector2 point = Flat(position);
            float along = Project(point, out _, out int segment);
            Vector2 heading = velocity.sqrMagnitude > 1f ? Flat(velocity).normalized : DirectionAt(segment);
            float limit = cruise;
            for (float d = 2f; d <= 20f; d += 2f)
            {
                float angle = Vector2.Angle(heading, DirectionAt(SegmentAt(along + d)));
                float turn = Mathf.Lerp(cruise, slowest, Mathf.Clamp01((angle - 15f) / 70f));
                limit = Mathf.Min(limit, Mathf.Sqrt(turn * turn + 2f * 1.2f * Mathf.Max(0f, d - 1.5f)));
            }
            Vector2 ahead = point + Flat(velocity) * 0.8f;
            if (SolidCost(ahead, 0.6f) > 30f)
            {
                limit = Mathf.Min(limit, slowest);
            }
            if (IceZone.Active.Count > 0)
            {
                float ice = IceZone.Ahead(position, new Vector3(heading.x, 0f, heading.y), 14f);
                if (ice < 14f)
                {
                    limit = Mathf.Min(limit, Mathf.Lerp(cruise * 0.7f, cruise, ice / 14f));
                }
            }
            return limit;
        }

        public float HalfWidthAt(float along)
        {
            PointAt(along, 0f, out float half, out _);
            return half;
        }

        public Vector3 ForwardAt(Vector3 position)
        {
            Project(Flat(position), out _, out int segment);
            Vector2 direction = DirectionAt(segment);
            return new Vector3(direction.x, 0f, direction.y);
        }

        public Vector3 CurrentAt(Vector3 position)
        {
            Vector2 point = Flat(position);
            Vector2 total = Vector2.zero;
            foreach (Zone zone in zones)
            {
                if ((zone.position - point).sqrMagnitude <= zone.reach * zone.reach && Inside(zone, point))
                {
                    total += zone.flow;
                }
            }
            return new Vector3(total.x, 0f, total.y);
        }

        static bool Inside(Zone zone, Vector2 point)
        {
            Vector2 offset = point - zone.position;
            float x = offset.x * zone.cos - offset.y * zone.sin;
            float z = offset.x * zone.sin + offset.y * zone.cos;
            return Mathf.Abs(x) <= zone.halfSize.x && Mathf.Abs(z) <= zone.halfSize.y;
        }

        public Result Plan(Vector3 position, Settings settings)
        {
            Vector2 start = Flat(position);
            float startAlong = Project(start, out float startLateral, out int startSegment);
            int stages = Mathf.Max(1, Mathf.CeilToInt(settings.lookAhead / settings.step));
            int lanes = Mathf.Max(3, settings.lanes | 1);
            float speed = Mathf.Max(1.5f, settings.travelSpeed);

            Gather(startAlong - 4f, startAlong + settings.lookAhead + 6f, start, settings);

            if (cost.GetLength(0) < stages + 1 || cost.GetLength(1) < lanes)
            {
                cost = new float[stages + 1, lanes];
                best = new float[stages + 1, lanes];
                from = new int[stages + 1, lanes];
                lateralOf = new float[stages + 1, lanes];
                pointOf = new Vector2[stages + 1, lanes];
                path = new int[stages + 1];
            }
            if (midCost.Length < lanes)
            {
                midCost = new float[lanes];
            }

            int hint = startSegment;
            for (int k = 1; k <= stages; k++)
            {
                float along = startAlong + k * settings.step;
                PointAt(along, 0f, out float half, out hint);
                float usable = Mathf.Max(0f, half - RaftRadius - settings.margin * 0.4f);
                float time = k * settings.step / speed;
                for (int j = 0; j < lanes; j++)
                {
                    float lateral = Mathf.Lerp(-usable, usable, j / (float)(lanes - 1));
                    lateralOf[k, j] = lateral;
                    pointOf[k, j] = PointAt(along, lateral, out _, out _);
                    cost[k, j] = Danger(pointOf[k, j], time, settings, hint);
                }
            }

            for (int j = 0; j < lanes; j++)
            {
                Vector2 middle = Vector2.Lerp(start, pointOf[1, j], 0.5f);
                float sweep = Danger(middle, 0.5f * settings.step / speed, settings, startSegment);
                float turn = lateralOf[1, j] - startLateral;
                best[1, j] = cost[1, j] + sweep + turn * turn * 0.05f;
                from[1, j] = -1;
            }
            for (int k = 2; k <= stages; k++)
            {
                float time = (k - 0.5f) * settings.step / speed;
                float weight = Mathf.Lerp(1f, 0.6f, k / (float)stages);
                PointAt(startAlong + (k - 0.5f) * settings.step, 0f, out _, out int middleSegment);
                for (int m = 0; m < lanes; m++)
                {
                    midCost[m] = Danger(Vector2.Lerp(pointOf[k - 1, m], pointOf[k, m], 0.5f), time, settings, middleSegment);
                }
                for (int j = 0; j < lanes; j++)
                {
                    float bestValue = float.MaxValue;
                    int bestFrom = 0;
                    for (int i = 0; i < lanes; i++)
                    {
                        float turn = lateralOf[k, j] - lateralOf[k - 1, i];
                        float value = best[k - 1, i] + turn * turn * 0.05f;
                        if (value >= bestValue)
                        {
                            continue;
                        }
                        value += Mathf.Max(midCost[(i + j) / 2], midCost[(i + j + 1) / 2]);
                        if (value < bestValue)
                        {
                            bestValue = value;
                            bestFrom = i;
                        }
                    }
                    best[k, j] = bestValue + cost[k, j] * weight;
                    from[k, j] = bestFrom;
                }
            }

            int lane = 0;
            float total = float.MaxValue;
            for (int j = 0; j < lanes; j++)
            {
                if (best[stages, j] < total)
                {
                    total = best[stages, j];
                    lane = j;
                }
            }
            for (int k = stages; k >= 1; k--)
            {
                path[k] = lane;
                lane = from[k, lane];
            }

            int aim = Mathf.Min(stages, 2);
            Vector2 target = Vector2.Lerp(pointOf[1, path[1]], pointOf[aim, path[aim]], 0.5f);
            float danger = 0f;
            for (int k = 1; k <= Mathf.Min(stages, 3); k++)
            {
                danger = Mathf.Max(danger, cost[k, path[k]]);
            }

            return new Result
            {
                target = new Vector3(target.x, position.y, target.y),
                danger = danger,
                lateralChange = Mathf.Abs(lateralOf[1, path[1]] - startLateral)
            };
        }

        void Gather(float minAlong, float maxAlong, Vector2 start, Settings settings)
        {
            nearZones.Clear();
            nearGoodies.Clear();
            nearCrocodiles.Clear();
            if (settings.readCurrents)
            {
                foreach (Zone zone in zones)
                {
                    if (zone.along >= minAlong - zone.reach && zone.along <= maxAlong + zone.reach)
                    {
                        nearZones.Add(zone);
                    }
                }
            }
            if (settings.collectPickups)
            {
                foreach (Goodie goodie in goodies)
                {
                    if (goodie.pickup != null && goodie.pickup.gameObject.activeInHierarchy && goodie.along >= minAlong && goodie.along <= maxAlong)
                    {
                        nearGoodies.Add(goodie);
                    }
                }
            }
            float range = maxAlong - minAlong + 20f;
            foreach (MovingObstacle crocodile in MovingObstacle.Active)
            {
                Vector2 origin = Flat(crocodile.Origin);
                if ((origin - start).sqrMagnitude <= range * range)
                {
                    nearCrocodiles.Add(crocodile);
                }
            }
        }

        float Danger(Vector2 point, float time, Settings settings, int hint)
        {
            float along = ProjectNear(point, hint, out float lateral, out int segment);
            PointAt(along, 0f, out float half, out _);
            float total = lateral * lateral * 0.004f;
            if (Mathf.Abs(lateral) > half + 1f)
            {
                total += Blocked;
            }
            total += SolidCost(point, settings.margin, settings.timedHazards ? time : -1f, settings.invincible);

            foreach (MovingObstacle crocodile in nearCrocodiles)
            {
                if (settings.invincible)
                {
                    break;
                }
                if (settings.predictCrocodiles)
                {
                    total += GapCost(CrocodileGap(point, crocodile, Flat(crocodile.PredictPosition(time))), settings.margin + 0.8f, 2.5f);
                    total += GapCost(CrocodileGap(point, crocodile, Flat(crocodile.PredictPosition(time + 0.6f))), settings.margin * 0.5f, 1.2f);
                }
                else
                {
                    Vector2 origin = Flat(crocodile.Origin);
                    Vector2 axis = Flat(crocodile.TravelAxis).normalized;
                    float t = Mathf.Clamp(Vector2.Dot(point - origin, axis), -crocodile.Reach, crocodile.Reach);
                    total += GapCost(CrocodileGap(point, crocodile, origin + axis * t), settings.margin, 1f) * 0.5f;
                }
            }

            foreach (Zone zone in nearZones)
            {
                if (!Inside(zone, point))
                {
                    continue;
                }
                Vector2 forward = DirectionAt(segment);
                float with = Vector2.Dot(zone.flow, forward);
                float sideways = Vector2.Dot(zone.flow, new Vector2(forward.y, -forward.x));
                total += Mathf.Max(0f, -with) * 2.2f - Mathf.Max(0f, with) * 1.2f + Mathf.Abs(sideways) * 0.6f;
                total += Mathf.Max(0f, Mathf.Sign(lateral) * sideways) * Mathf.Clamp01(Mathf.Abs(lateral) / Mathf.Max(1f, half)) * 2f;
            }

            foreach (Goodie goodie in nearGoodies)
            {
                float reward = 9f;
                float reach = 3f;
                if (settings.timedHazards && goodie.kind == PickupKind.Repair)
                {
                    float missing = 1f - Mathf.Clamp01(settings.health);
                    reward = missing < 0.08f ? 1f : Mathf.Lerp(12f, 45f, Mathf.Clamp01(missing / 0.6f));
                    reach = missing < 0.08f ? 3f : 5.5f;
                }
                float d = Vector2.Distance(point, goodie.position);
                if (d < reach)
                {
                    total -= (1f - d / reach) * reward;
                }
            }
            return total;
        }

        Vector2 DirectionAt(int segment)
        {
            Vector2 direction = points[Mathf.Min(segment + 1, points.Length - 1)] - points[segment];
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
        }

        static float CrocodileGap(Vector2 point, MovingObstacle crocodile, Vector2 center)
        {
            Vector2 axis = Flat(crocodile.TravelAxis).normalized;
            float half = Mathf.Max(0f, crocodile.Radius - crocodile.HalfWidth);
            float t = Mathf.Clamp(Vector2.Dot(point - center, axis), -half, half);
            return Vector2.Distance(point, center + axis * t) - crocodile.HalfWidth - RaftRadius;
        }

        static float GapCost(float gap, float margin, float weight)
        {
            if (gap < 0f)
            {
                return Blocked * weight * (1f + Mathf.Min(2f, -gap));
            }
            if (gap < margin)
            {
                float closeness = 1f - gap / margin;
                return closeness * closeness * 45f * weight;
            }
            return 0f;
        }

        static float Overlap(Vector2 point, Vector2 center, float radius, float margin, float weight)
        {
            float gap = Vector2.Distance(point, center) - radius - RaftRadius;
            if (gap < 0f)
            {
                return Blocked * weight * (1f + Mathf.Min(2f, -gap));
            }
            if (gap < margin)
            {
                float closeness = 1f - gap / margin;
                return closeness * closeness * 45f * weight;
            }
            return 0f;
        }
    }
}
