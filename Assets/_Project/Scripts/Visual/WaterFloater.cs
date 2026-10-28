using UnityEngine;

namespace FartRafters
{
    public class WaterFloater : MonoBehaviour
    {
        public float heightOffset;
        [Min(0f)] public float tiltSampleDistance = 1f;
        [Range(0f, 1f)] public float tiltAmount = 1f;

        Quaternion baseRotation;
        Vector3 basePosition;

        bool started;

        void Start()
        {
            baseRotation = transform.rotation;
            if (!started)
            {
                basePosition = transform.position;
            }
            started = true;
        }

        public void Shift(Vector3 delta)
        {
            if (!started)
            {
                basePosition = transform.position;
                started = true;
            }
            basePosition += delta;
        }

        void Update()
        {
            WaterSurface water = WaterSurface.Instance;
            if (water == null)
            {
                return;
            }

            float time = Time.time;
            Vector3 p = basePosition;
            float center = water.GetHeight(p.x, p.z, time);
            transform.position = new Vector3(p.x, center + heightOffset, p.z);

            if (tiltAmount <= 0f || tiltSampleDistance <= 0f)
            {
                return;
            }
            float d = tiltSampleDistance;
            float dx = water.GetHeight(p.x + d, p.z, time) - water.GetHeight(p.x - d, p.z, time);
            float dz = water.GetHeight(p.x, p.z + d, time) - water.GetHeight(p.x, p.z - d, time);
            Vector3 normal = new Vector3(-dx, 2f * d, -dz).normalized;
            Quaternion tilt = Quaternion.FromToRotation(Vector3.up, Vector3.Slerp(Vector3.up, normal, tiltAmount));
            transform.rotation = tilt * baseRotation;
        }
    }
}
