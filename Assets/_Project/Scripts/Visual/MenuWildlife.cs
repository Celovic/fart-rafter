using UnityEngine;

namespace FartRafters
{
    public class MenuJumper : MonoBehaviour
    {
        public Vector3 center;
        [Min(1f)] public float radius = 10f;
        [Min(0.2f)] public float height = 1.4f;
        [Min(0.2f)] public float duration = 1.1f;
        [Min(0.5f)] public float length = 3f;
        public ParticleSystem splash;

        float start = -10f;
        float next;
        Vector3 from;
        Vector3 to;

        void Start()
        {
            next = Time.time + Random.Range(0.5f, 4f);
            Hide();
        }

        void Update()
        {
            if (Time.time >= next)
            {
                float angle = Random.value * Mathf.PI * 2f;
                float distance = Random.Range(radius * 0.3f, radius);
                from = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
                float heading = Random.value * Mathf.PI * 2f;
                to = from + new Vector3(Mathf.Cos(heading), 0f, Mathf.Sin(heading)) * length;
                start = Time.time;
                next = Time.time + duration + Random.Range(2.5f, 6f);
                Splash(from);
            }
            float t = (Time.time - start) / duration;
            if (t < 0f || t > 1f)
            {
                if (t > 1f && t < 1.05f)
                {
                    Splash(to);
                }
                Hide();
                return;
            }
            Vector3 position = Vector3.Lerp(from, to, t);
            position.y = Mathf.Sin(t * Mathf.PI) * height - 0.15f;
            float pitch = Mathf.Lerp(-55f, 55f, t);
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(to - from, Vector3.up) * Quaternion.Euler(pitch, 0f, Mathf.Sin(t * 20f) * 10f));
        }

        void Hide()
        {
            transform.position = new Vector3(transform.position.x, -3f, transform.position.z);
        }

        void Splash(Vector3 point)
        {
            if (splash == null)
            {
                return;
            }
            splash.transform.position = new Vector3(point.x, 0.05f, point.z);
            splash.Emit(14);
        }
    }

    public class MenuPaddler : MonoBehaviour
    {
        public Vector3 center;
        [Min(0.5f)] public float radius = 9f;
        public float speed = 0.08f;
        public float lag;
        float angle;

        void Start()
        {
            angle = -lag;
        }

        void Update()
        {
            angle += speed * Time.deltaTime;
            float wobble = Mathf.Sin(angle * 3f) * 0.6f;
            Vector3 position = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (radius + wobble);
            WaterSurface water = WaterSurface.Instance;
            position.y = (water != null ? water.GetHeight(position.x, position.z, Time.time) : 0f) - 0.04f;
            Vector3 tangent = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * Mathf.Sign(speed);
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(tangent, Vector3.up) * Quaternion.Euler(Mathf.Sin(Time.time * 3f + lag) * 4f, 0f, Mathf.Sin(Time.time * 2.2f + lag) * 5f));
        }
    }

    public class MenuFlutter : MonoBehaviour
    {
        public Vector3 center;
        [Min(0.5f)] public float radius = 3f;
        public float height = 2.5f;
        public Transform leftWing;
        public Transform rightWing;
        float seed;

        void Start()
        {
            seed = Random.value * 100f;
        }

        void Update()
        {
            float t = Time.time * 0.45f + seed;
            Vector3 position = center + new Vector3(Mathf.Sin(t * 1.3f) * radius, height + Mathf.Sin(t * 2.7f) * 0.6f, Mathf.Cos(t * 0.9f) * radius * 0.8f);
            Vector3 velocity = new Vector3(Mathf.Cos(t * 1.3f) * 1.3f * radius, Mathf.Cos(t * 2.7f) * 1.6f, -Mathf.Sin(t * 0.9f) * 0.72f * radius);
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(velocity.sqrMagnitude > 0.001f ? velocity : Vector3.forward, Vector3.up));
            float flap = Mathf.Sin(Time.time * 26f + seed) * 55f;
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
