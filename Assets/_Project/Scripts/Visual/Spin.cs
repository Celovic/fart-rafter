using UnityEngine;

namespace FartRafters
{
    public class Wiggle : MonoBehaviour
    {
        public float amplitude = 14f;
        public float speed = 7f;

        void Update()
        {
            transform.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * speed) * amplitude, Mathf.Sin(Time.time * speed * 0.7f) * amplitude * 0.4f);
        }
    }

    public class Spin : MonoBehaviour
    {
        public Vector3 axis = Vector3.up;
        public float speed = 90f;

        void Update()
        {
            transform.Rotate(axis, speed * Time.deltaTime, Space.Self);
        }
    }
}
