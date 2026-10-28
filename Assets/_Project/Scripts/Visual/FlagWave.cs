using UnityEngine;

namespace FartRafters
{
    public class FlagWave : MonoBehaviour
    {
        [Min(0f)] public float amplitude = 14f;
        [Min(0f)] public float frequency = 2.2f;

        Quaternion baseRotation;
        float offset;

        void Start()
        {
            baseRotation = transform.localRotation;
            offset = Random.value * 10f;
        }

        void Update()
        {
            float t = Time.time * frequency + offset;
            float angle = Mathf.Sin(t) * amplitude + Mathf.Sin(t * 2.3f) * amplitude * 0.3f;
            transform.localRotation = baseRotation * Quaternion.Euler(0f, angle, 0f);
        }
    }
}
