using UnityEngine;

namespace FartRafters
{
    public class FollowCamera : MonoBehaviour
    {
        public Transform target;
        public Rigidbody targetBody;
        public Vector3 offset = new Vector3(0f, 13f, -11f);
        [Min(0f)] public float lookAhead = 0.6f;
        [Min(0.01f)] public float smoothTime = 0.25f;
        [Min(0f)] public float maxShake = 0.6f;
        [Min(0f)] public float shakeDecay = 1.6f;

        static FollowCamera active;

        Vector3 velocity;
        Vector3 basePosition;
        float trauma;

        public static void Shake(float amount)
        {
            if (active != null)
            {
                active.trauma = Mathf.Clamp01(active.trauma + amount);
            }
        }

        void OnEnable()
        {
            active = this;
        }

        void OnDisable()
        {
            if (active == this)
            {
                active = null;
            }
        }

        void Start()
        {
            basePosition = transform.position;
            if (target != null)
            {
                basePosition = target.position + offset;
                transform.position = basePosition;
                transform.rotation = Quaternion.LookRotation(-offset.normalized, Vector3.up);
            }
        }

        void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 ahead = targetBody != null ? Vector3.ProjectOnPlane(targetBody.linearVelocity, Vector3.up) * lookAhead : Vector3.zero;
            Vector3 focus = target.position + ahead;
            basePosition = Vector3.SmoothDamp(basePosition, focus + offset, ref velocity, smoothTime);

            trauma = Mathf.MoveTowards(trauma, 0f, shakeDecay * Time.deltaTime);
            float shake = trauma * trauma * maxShake;
            float t = Time.time * 25f;
            Vector3 jitter = new Vector3(Mathf.PerlinNoise(t, 0.3f) - 0.5f, Mathf.PerlinNoise(0.7f, t) - 0.5f, Mathf.PerlinNoise(t, t * 0.5f) - 0.5f) * (2f * shake);

            transform.position = basePosition + jitter;
            transform.rotation = Quaternion.LookRotation(-offset.normalized, Vector3.up);
        }
    }
}
