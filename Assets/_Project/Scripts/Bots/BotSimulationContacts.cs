using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FartRafters
{
    public class BotSimulationContacts : MonoBehaviour
    {
        static readonly Dictionary<string, int> counts = new Dictionary<string, int>();
        int logged;

        public static string Summary()
        {
            var text = new StringBuilder();
            foreach (KeyValuePair<string, int> pair in counts)
            {
                text.Append(pair.Key).Append('=').Append(pair.Value).Append(' ');
            }
            return text.ToString();
        }

        void OnCollisionEnter(Collision collision)
        {
            Transform other = collision.collider.transform;
            string name = other.parent != null ? other.parent.name + "/" + other.name : other.name;
            float speed = collision.relativeVelocity.magnitude;
            if (speed < 0.8f)
            {
                return;
            }
            counts.TryGetValue(name, out int count);
            counts[name] = count + 1;
            if (logged++ < 25)
            {
                Vector3 p = collision.GetContact(0).point;
                Debug.Log("[BotSim] contact " + name + " at " + p.x.ToString("F1") + "," + p.z.ToString("F1") + " rel=" + speed.ToString("F1") + " obj=" + other.position.x.ToString("F1") + "," + other.position.z.ToString("F1"));
            }
        }
    }
}
