using System.Collections.Generic;
using UnityEngine;

namespace FartRafters
{
    [RequireComponent(typeof(BoxCollider))]
    public class CurrentZone : MonoBehaviour
    {
        public Vector3 direction = Vector3.left;
        [Min(0f)] public float strength = 3.5f;

        readonly Dictionary<Rigidbody, int> overlaps = new Dictionary<Rigidbody, int>();
        readonly List<Rigidbody> stale = new List<Rigidbody>();

        void Reset()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (body == null)
            {
                return;
            }
            overlaps.TryGetValue(body, out int count);
            overlaps[body] = count + 1;
        }

        void OnTriggerExit(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (body == null || !overlaps.TryGetValue(body, out int count))
            {
                return;
            }
            if (count <= 1)
            {
                overlaps.Remove(body);
            }
            else
            {
                overlaps[body] = count - 1;
            }
        }

        void FixedUpdate()
        {
            Vector3 acceleration = direction.normalized * strength;
            foreach (KeyValuePair<Rigidbody, int> pair in overlaps)
            {
                if (pair.Key == null)
                {
                    stale.Add(pair.Key);
                    continue;
                }
                if (!pair.Key.isKinematic)
                {
                    pair.Key.AddForce(acceleration, ForceMode.Acceleration);
                }
            }

            foreach (Rigidbody body in stale)
            {
                overlaps.Remove(body);
            }
            stale.Clear();
        }
    }
}
