using UnityEngine;

namespace FartRafters
{
    [RequireComponent(typeof(BoxCollider))]
    public class Checkpoint : MonoBehaviour
    {
        public CourseManager course;
        public bool isFinish;
        public int nextWaypointIndex;
        public Renderer[] flags;
        public Material passedMaterial;

        public Vector3 SpawnPosition => transform.position;
        public Quaternion SpawnRotation => Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        public bool Passed { get; private set; }

        void Reset()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (course != null)
            {
                course.OnCheckpointTouched(this, other);
            }
        }

        public void MarkPassed()
        {
            if (Passed)
            {
                return;
            }
            Passed = true;
            if (passedMaterial == null || flags == null)
            {
                return;
            }
            foreach (Renderer flag in flags)
            {
                if (flag != null)
                {
                    flag.sharedMaterial = passedMaterial;
                }
            }
        }
    }
}
