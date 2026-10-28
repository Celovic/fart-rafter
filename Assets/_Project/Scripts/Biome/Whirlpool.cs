using System.Collections.Generic;
using UnityEngine;

namespace FartRafters
{
    public class Whirlpool : MonoBehaviour
    {
        static readonly List<Whirlpool> active = new List<Whirlpool>();

        [Min(1f)] public float radius = 7f;
        [Min(0.5f)] public float coreRadius = 1.8f;
        [Min(0f)] public float swirl = 3f;
        [Min(0f)] public float pull = 1.6f;
        public float direction = 1f;
        [Min(0f)] public float coreDamage = 16f;
        public Transform visual;
        public float spinSpeed = 70f;

        float lastReport = -10f;

        public static IReadOnlyList<Whirlpool> Active => active;

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
            Raft raft = Raft.Current;
            if (raft == null || raft.Body == null || raft.Body.isKinematic)
            {
                return;
            }
            Vector3 offset = raft.Body.position - transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance >= radius || distance < 0.001f)
            {
                return;
            }
            float strength = 1f - distance / radius;
            Vector3 inward = -offset / distance;
            Vector3 tangent = Vector3.Cross(Vector3.up, inward) * Mathf.Sign(direction);
            raft.Body.AddForce(tangent * (swirl * Mathf.Sqrt(strength)) + inward * (pull * strength), ForceMode.Acceleration);
            raft.Body.AddTorque(Vector3.up * (-Mathf.Sign(direction) * 1.4f * strength), ForceMode.Acceleration);
            if (distance < coreRadius)
            {
                RaftHealth health = raft.GetComponent<RaftHealth>();
                if (health != null)
                {
                    bool report = Time.time - lastReport > 0.4f;
                    if (report)
                    {
                        lastReport = Time.time;
                    }
                    health.Apply(coreDamage * Time.fixedDeltaTime, report);
                }
            }
        }

        void Update()
        {
            if (visual != null)
            {
                visual.Rotate(0f, -spinSpeed * Mathf.Sign(direction) * Time.deltaTime, 0f, Space.Self);
            }
        }
    }
}
