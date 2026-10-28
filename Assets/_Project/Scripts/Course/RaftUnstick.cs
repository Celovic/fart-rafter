using UnityEngine;

namespace FartRafters
{
    public class RaftUnstick : MonoBehaviour
    {
        const float SlowSpeed = 0.35f;
        const float StuckSeconds = 3.5f;
        const float Push = 2.6f;

        CourseManager course;
        Raft raft;
        float slowTime;
        float cooldown;

        public static int Nudges { get; private set; }

        public void Setup(CourseManager course, Raft raft)
        {
            this.course = course;
            this.raft = raft;
            Nudges = 0;
        }

        void FixedUpdate()
        {
            if (course == null || raft == null || raft.Body == null || course.NetworkClient || !course.Running || course.Finished)
            {
                slowTime = 0f;
                return;
            }
            cooldown -= Time.fixedDeltaTime;
            Vector3 velocity = raft.Body.linearVelocity;
            velocity.y = 0f;
            if (velocity.magnitude > SlowSpeed || cooldown > 0f)
            {
                slowTime = 0f;
                return;
            }
            slowTime += Time.fixedDeltaTime;
            if (slowTime < StuckSeconds)
            {
                return;
            }
            Vector3 position = raft.Body.position;
            Vector3 ahead = course.NextWaypoint - position;
            Vector3 center = course.CurrentWaypoint - position;
            ahead.y = 0f;
            center.y = 0f;
            Vector3 direction = (ahead.normalized + center.normalized * 0.6f).normalized;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = raft.transform.forward;
            }
            raft.Body.AddForce(direction * Push, ForceMode.VelocityChange);
            slowTime = 0f;
            cooldown = 2f;
            Nudges++;
        }
    }
}
