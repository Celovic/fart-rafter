using System.Collections.Generic;
using UnityEngine;

namespace FartRafters
{
    public class ChaseAnimal : MonoBehaviour
    {
        const float StartGap = 26f;
        const float MaxGap = 20f;
        const float RespawnDelay = 2.5f;
        const float Wobble = 1.4f;

        static readonly Vector3[] PackOffsets = { Vector3.zero, new Vector3(-2.4f, 0f, -2.8f), new Vector3(2.4f, 0f, -2.8f) };

        CourseManager course;
        RaftHealth health;
        CourseTheme theme;
        float baseSpeed;
        float ratio;
        Vector3[] points;
        float[] distance;
        float triggerAlong;
        bool active;
        bool done;
        float along;
        float raftAlong;
        float raftSpeed;
        float resumeAt;
        readonly List<Transform> bodies = new List<Transform>();

        public static ChaseAnimal Current { get; private set; }
        public bool Hunting => active && !done && Time.time >= resumeAt;
        public float Gap => raftAlong - along;

        public static ChaseAnimal Create(CourseManager course, CourseTheme theme, int difficulty)
        {
            if (course == null || theme == null || theme.chaseMesh == null || course.waypoints == null || course.waypoints.Length < 2 || course.checkpoints == null || course.checkpoints.Length < 2)
            {
                return null;
            }
            var go = new GameObject("Chase");
            var chase = go.AddComponent<ChaseAnimal>();
            chase.course = course;
            chase.theme = theme;
            chase.health = course.raft != null ? course.raft.GetComponent<RaftHealth>() : null;
            bool expert = difficulty >= 3;
            chase.baseSpeed = expert ? 2.8f : 2.2f;
            chase.ratio = expert ? 0.95f : 0.85f;
            chase.Build();
            Current = chase;
            return chase;
        }

        void Build()
        {
            points = new Vector3[course.waypoints.Length];
            distance = new float[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = course.waypoints[i].position;
                if (i > 0)
                {
                    distance[i] = distance[i - 1] + Vector3.Distance(Flat(points[i - 1]), Flat(points[i]));
                }
            }
            Checkpoint last = course.checkpoints[course.checkpoints.Length - 2];
            triggerAlong = Project(last.transform.position);

            float length = Mathf.Max(0.1f, theme.chaseMesh.bounds.size.z);
            Vector3 scale = Vector3.one * (theme.chaseSize / length);
            int count = Mathf.Clamp(theme.chaseCount, 1, PackOffsets.Length);
            for (int i = 0; i < count; i++)
            {
                var holder = new GameObject("Animal" + i).transform;
                holder.SetParent(transform, false);
                holder.localPosition = PackOffsets[i] * (theme.chaseSize / 3f);
                CourseBuilder.CreateSculpt("Body", holder, theme.chaseMesh, theme.sculptMaterial, Vector3.zero, scale);
                bodies.Add(holder);
            }
            gameObject.SetActive(true);
            SetVisible(false);
            course.Respawned += OnRespawned;
        }

        void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
            if (course != null)
            {
                course.Respawned -= OnRespawned;
            }
        }

        void OnRespawned()
        {
            resumeAt = Time.time + RespawnDelay;
        }

        void SetVisible(bool visible)
        {
            foreach (Transform body in bodies)
            {
                body.gameObject.SetActive(visible);
            }
        }

        void Update()
        {
            Raft raft = course.raft;
            if (raft == null || done)
            {
                return;
            }
            float dt = Time.deltaTime;
            float previous = raftAlong;
            raftAlong = Project(raft.transform.position);
            if (dt > 0f)
            {
                float speed = Mathf.Max(0f, (raftAlong - previous) / dt);
                raftSpeed = Mathf.Lerp(raftSpeed, Mathf.Min(speed, 20f), dt * 1.5f);
            }

            if (!active)
            {
                if (course.Running && !course.Finished && raftAlong >= triggerAlong)
                {
                    Begin();
                }
                return;
            }
            if (course.Finished)
            {
                done = true;
                SetVisible(false);
                return;
            }

            if (Time.time < resumeAt)
            {
                along = raftAlong - StartGap;
            }
            else
            {
                along += Mathf.Max(baseSpeed, raftSpeed * ratio) * dt;
                along = Mathf.Max(along, raftAlong - MaxGap);
                if (!OnlineGame.IsClient && health != null && raftAlong - along < theme.chaseSize * 0.5f)
                {
                    if (health.TryCatch())
                    {
                        resumeAt = Time.time + RespawnDelay;
                    }
                    else
                    {
                        along = raftAlong - theme.chaseSize;
                    }
                }
            }
            Place();
        }

        void Begin()
        {
            active = true;
            along = raftAlong - StartGap;
            SetVisible(true);
            Place();
            CourseNotices.Post((string.IsNullOrEmpty(theme.chaseName) ? "Bir şey" : theme.chaseName) + " peşinizde! Kaçın!", new Color(1f, 0.55f, 0.35f));
            Haptics.Heavy();
            GameAudio.Play(GameAudio.Sfx.Chomp, 0.8f, 0.7f);
        }

        void Place()
        {
            Vector3 position = PointAt(along, out Vector3 forward);
            Vector3 side = Vector3.Cross(Vector3.up, forward);
            float t = Time.time;
            position += side * Mathf.Sin(t * 0.7f) * Wobble;
            WaterSurface water = WaterSurface.Instance;
            float surface = water != null ? water.GetHeight(position.x, position.z, t) : position.y;
            transform.position = new Vector3(position.x, surface + theme.chaseHeight, position.z);
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            for (int i = 0; i < bodies.Count; i++)
            {
                float phase = t * 5f + i * 1.7f;
                bodies[i].localRotation = Quaternion.Euler(Mathf.Sin(phase) * 4f, Mathf.Sin(phase * 0.5f) * 6f, Mathf.Sin(phase * 0.5f) * 5f);
                Vector3 offset = PackOffsets[Mathf.Min(i, PackOffsets.Length - 1)] * (theme.chaseSize / 3f);
                bodies[i].localPosition = offset + Vector3.up * (Mathf.Abs(Mathf.Sin(phase)) * 0.18f);
            }
        }

        static Vector3 Flat(Vector3 point)
        {
            return new Vector3(point.x, 0f, point.z);
        }

        float Project(Vector3 position)
        {
            Vector3 p = Flat(position);
            float best = float.MaxValue;
            float result = 0f;
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector3 a = Flat(points[i]);
                Vector3 b = Flat(points[i + 1]);
                Vector3 ab = b - a;
                float t = ab.sqrMagnitude > 0.0001f ? Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
                float d = (a + ab * t - p).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    result = distance[i] + (distance[i + 1] - distance[i]) * t;
                }
            }
            return result;
        }

        Vector3 PointAt(float s, out Vector3 forward)
        {
            s = Mathf.Clamp(s, 0f, distance[distance.Length - 1]);
            for (int i = 0; i < points.Length - 1; i++)
            {
                if (s <= distance[i + 1] || i == points.Length - 2)
                {
                    float span = distance[i + 1] - distance[i];
                    float t = span > 0.0001f ? (s - distance[i]) / span : 0f;
                    forward = Flat(points[i + 1] - points[i]).normalized;
                    if (forward.sqrMagnitude < 0.5f)
                    {
                        forward = Vector3.forward;
                    }
                    return Vector3.Lerp(points[i], points[i + 1], t);
                }
            }
            forward = Vector3.forward;
            return points[points.Length - 1];
        }
    }
}
