using System.Collections.Generic;
using UnityEngine;

namespace FartRafters
{
    [RequireComponent(typeof(Rigidbody))]
    public class MovingObstacle : MonoBehaviour
    {
        static readonly List<MovingObstacle> active = new List<MovingObstacle>();

        public ObstacleKind kind;
        public Vector3 travel = new Vector3(4f, 0f, 0f);
        [Min(0f)] public float speed = 3f;
        public Transform visual;
        [Min(0f)] public float turnSpeed = 360f;
        [Min(0f)] public float lungeDistance = 6f;
        [Range(0f, 45f)] public float lungeAngle = 18f;

        float lunge;
        Vector3 visualBase;
        bool visualBaseSet;

        Rigidbody body;
        Vector3 origin;
        float phase;

        public static IReadOnlyList<MovingObstacle> Active => active;
        public static System.Func<float> SharedClock;
        public Vector3 Origin => origin;
        public Vector3 TravelAxis => travel.normalized;
        public float Reach => travel.magnitude;
        public float Radius { get; private set; } = 1.5f;
        public float HalfWidth { get; private set; } = 0.6f;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            origin = body.position;
            BoxCollider box = GetComponent<BoxCollider>();
            if (box != null)
            {
                Radius = Mathf.Max(box.size.x, box.size.z) * 0.5f;
                HalfWidth = Mathf.Min(box.size.x, box.size.z) * 0.5f;
            }
        }

        void OnEnable()
        {
            active.Add(this);
        }

        void OnDisable()
        {
            active.Remove(this);
        }

        void FixedUpdate()
        {
            float length = travel.magnitude;
            if (length < 0.001f)
            {
                return;
            }
            phase = SharedClock != null ? SharedClock() * speed / length : phase + Time.fixedDeltaTime * speed / length;
            body.MovePosition(origin + travel * Mathf.Sin(phase));
        }

        void Update()
        {
            if (visual == null || travel.sqrMagnitude < 0.000001f)
            {
                return;
            }
            if (!visualBaseSet)
            {
                visualBase = visual.localPosition;
                visualBaseSet = true;
            }
            Raft raft = Raft.Current;
            float near = 0f;
            if (raft != null)
            {
                Vector3 offset = raft.transform.position - transform.position;
                offset.y = 0f;
                near = Mathf.Clamp01(1f - offset.magnitude / lungeDistance);
            }
            lunge = Mathf.MoveTowards(lunge, near, Time.deltaTime * 3f);
            float snap = lunge * (0.6f + 0.4f * Mathf.Abs(Mathf.Sin(Time.time * 9f)));

            float heading = Mathf.Cos(phase) >= 0f ? 1f : -1f;
            Quaternion target = Quaternion.LookRotation(travel.normalized * heading, Vector3.up) * Quaternion.Euler(-lungeAngle * snap, 0f, 0f);
            visual.rotation = Quaternion.RotateTowards(visual.rotation, target, turnSpeed * Time.deltaTime);
            visual.localPosition = visualBase + Vector3.up * (0.25f * snap);
        }

        public Vector3 PredictPosition(float seconds)
        {
            float length = travel.magnitude;
            if (length < 0.001f)
            {
                return origin;
            }
            return origin + travel * Mathf.Sin(phase + seconds * speed / length);
        }
    }
}
