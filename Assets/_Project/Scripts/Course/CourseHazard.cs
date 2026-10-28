using System.Collections.Generic;
using UnityEngine;

namespace FartRafters
{
    public class CourseHazard : MonoBehaviour
    {
        static readonly List<CourseHazard> active = new List<CourseHazard>();

        [Min(0f)] public float radius = 6f;

        void OnEnable()
        {
            active.Add(this);
        }

        void OnDisable()
        {
            active.Remove(this);
        }

        public static bool Contains(Vector3 position)
        {
            foreach (CourseHazard hazard in active)
            {
                Vector3 offset = position - hazard.transform.position;
                offset.y = 0f;
                if (offset.sqrMagnitude <= hazard.radius * hazard.radius)
                {
                    return true;
                }
            }
            return false;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
