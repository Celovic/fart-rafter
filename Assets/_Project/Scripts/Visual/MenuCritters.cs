using UnityEngine;

namespace FartRafters
{
    public class MenuSwimmer : MonoBehaviour
    {
        public Vector3 center;
        [Min(0.5f)] public float radius = 8f;
        public float speed = 0.12f;
        public float depth = -0.25f;
        float angle;

        void Start()
        {
            angle = Random.value * Mathf.PI * 2f;
        }

        void Update()
        {
            angle += speed * Time.deltaTime;
            Vector3 position = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            WaterSurface water = WaterSurface.Instance;
            position.y = (water != null ? water.GetHeight(position.x, position.z, Time.time) : 0f) + depth + Mathf.Sin(Time.time * 1.3f + angle) * 0.04f;
            Vector3 tangent = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * Mathf.Sign(speed);
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(tangent, Vector3.up));
        }
    }

    public class MenuHopper : MonoBehaviour
    {
        public Transform body;
        [Min(0.1f)] public float height = 0.55f;
        [Min(0.1f)] public float duration = 0.45f;
        float nextHop;
        float hopStart = -10f;

        void Start()
        {
            nextHop = Time.time + Random.Range(0.5f, 3f);
        }

        void Update()
        {
            if (body == null)
            {
                return;
            }
            if (Time.time >= nextHop)
            {
                hopStart = Time.time;
                nextHop = Time.time + Random.Range(2f, 5f);
                body.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }
            float t = (Time.time - hopStart) / duration;
            float lift = t >= 0f && t <= 1f ? Mathf.Sin(t * Mathf.PI) * height : 0f;
            float squash = t >= 0f && t <= 1f ? 1f + Mathf.Sin(t * Mathf.PI) * 0.15f : 1f + Mathf.Sin(Time.time * 3f) * 0.02f;
            body.localPosition = new Vector3(0f, 0.04f + lift, 0f);
            body.localScale = new Vector3(1f / Mathf.Sqrt(squash), squash, 1f / Mathf.Sqrt(squash));
        }
    }

    public class MenuFlyer : MonoBehaviour
    {
        public Vector3 center;
        [Min(1f)] public float radius = 20f;
        public float height = 12f;
        public float speed = 0.25f;
        public Transform leftWing;
        public Transform rightWing;
        float angle;

        void Start()
        {
            angle = Random.value * Mathf.PI * 2f;
        }

        void Update()
        {
            angle += speed * Time.deltaTime;
            Vector3 position = center + new Vector3(Mathf.Cos(angle) * radius, height + Mathf.Sin(angle * 2f) * 1.5f, Mathf.Sin(angle) * radius);
            Vector3 tangent = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * Mathf.Sign(speed);
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(tangent, Vector3.up) * Quaternion.Euler(0f, 0f, -Mathf.Sign(speed) * 18f));
            float flap = Mathf.Sin(Time.time * 9f + angle * 10f) * 35f;
            if (leftWing != null)
            {
                leftWing.localRotation = Quaternion.Euler(0f, 0f, 180f - flap);
            }
            if (rightWing != null)
            {
                rightWing.localRotation = Quaternion.Euler(0f, 0f, flap);
            }
        }
    }
}
