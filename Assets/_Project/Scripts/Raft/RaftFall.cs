using UnityEngine;

namespace FartRafters
{
    [RequireComponent(typeof(Raft))]
    public class RaftFall : MonoBehaviour
    {
        public CourseManager course;
        [Range(0f, 1f)] public float chance;
        [Min(0f)] public float impactThreshold = 4.5f;
        [Range(0f, 90f)] public float tiltAngle = 35f;
        [Min(0f)] public float cooldown = 3f;
        [Min(1)] public int maxPerEvent = 1;

        Raft raft;
        RaftHealth health;
        float lastFall = -10f;

        void Start()
        {
            raft = GetComponent<Raft>();
            health = GetComponent<RaftHealth>();
            if (health != null)
            {
                health.HardImpact += OnImpact;
            }
            if (course != null)
            {
                course.Respawned += ReturnAll;
                course.CheckpointReached += OnCheckpoint;
            }
        }

        void OnDestroy()
        {
            if (health != null)
            {
                health.HardImpact -= OnImpact;
            }
            if (course != null)
            {
                course.Respawned -= ReturnAll;
                course.CheckpointReached -= OnCheckpoint;
            }
        }

        void FixedUpdate()
        {
            if (Vector3.Angle(transform.up, Vector3.up) > tiltAngle)
            {
                Trigger();
            }
        }

        void OnImpact(float deltaV)
        {
            if (deltaV >= impactThreshold)
            {
                Trigger();
            }
        }

        void Trigger()
        {
            if (chance <= 0f || Time.time - lastFall < cooldown || course == null || course.Finished || raft.riders == null || IceZone.Amount(transform.position) > 0f)
            {
                return;
            }
            lastFall = Time.time;
            int fallen = 0;
            int start = Random.Range(0, raft.riders.Length);
            for (int k = 0; k < raft.riders.Length && fallen < maxPerEvent; k++)
            {
                Rider rider = raft.riders[(start + k) % raft.riders.Length];
                if (rider == null || !rider.isActiveAndEnabled || rider.InWater || Random.value >= chance)
                {
                    continue;
                }
                rider.Fall(rider.transform.position - transform.position);
                fallen++;
            }
        }

        void OnCheckpoint(Checkpoint checkpoint)
        {
            ReturnAll();
        }

        void ReturnAll()
        {
            if (raft == null || raft.riders == null)
            {
                return;
            }
            foreach (Rider rider in raft.riders)
            {
                if (rider != null && (rider.Eliminated || rider.InWater) && rider.brain != null)
                {
                    rider.Return();
                }
            }
        }
    }
}
