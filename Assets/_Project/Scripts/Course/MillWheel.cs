using UnityEngine;

namespace FartRafters
{
    [RequireComponent(typeof(Rigidbody))]
    public class MillWheel : MonoBehaviour
    {
        public float speed = 40f;
        public static System.Func<float> SharedClock;

        Rigidbody body;
        float angle;
        Quaternion baseRotation;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            baseRotation = transform.rotation;
        }

        void FixedUpdate()
        {
            angle = SharedClock != null ? SharedClock() * speed : angle + speed * Time.fixedDeltaTime;
            body.MoveRotation(baseRotation * Quaternion.Euler(0f, 0f, angle));
        }
    }
}
