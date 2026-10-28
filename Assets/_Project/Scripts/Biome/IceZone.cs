using System.Collections.Generic;
using UnityEngine;

namespace FartRafters
{
    public class IceZone : MonoBehaviour
    {
        public const float Top = 0.12f;
        public const float FloatLevel = 0.57f;
        const float Ramp = 2.5f;

        static readonly List<IceZone> active = new List<IceZone>();

        public Vector2 halfSize = new Vector2(8f, 10f);

        Vector2 center;
        Vector2 axis;
        Vector2 side;

        public static IReadOnlyList<IceZone> Active => active;
        public Vector2 Center => center;
        public Vector2 Axis => axis;

        void OnEnable()
        {
            Vector3 forward = transform.forward;
            axis = new Vector2(forward.x, forward.z).normalized;
            side = new Vector2(axis.y, -axis.x);
            center = new Vector2(transform.position.x, transform.position.z);
            active.Add(this);
        }

        void OnDisable()
        {
            active.Remove(this);
        }

        public float AmountAt(Vector2 point)
        {
            Vector2 offset = point - center;
            float along = Mathf.Abs(Vector2.Dot(offset, axis));
            float lateral = Mathf.Abs(Vector2.Dot(offset, side));
            if (lateral > halfSize.x + 1f || along > halfSize.y)
            {
                return 0f;
            }
            return Mathf.Clamp01((halfSize.y - along) / Ramp);
        }

        public static float Amount(Vector3 position)
        {
            if (active.Count == 0)
            {
                return 0f;
            }
            var point = new Vector2(position.x, position.z);
            float best = 0f;
            foreach (IceZone zone in active)
            {
                best = Mathf.Max(best, zone.AmountAt(point));
            }
            return best;
        }

        public static float Ahead(Vector3 position, Vector3 direction, float distance)
        {
            if (active.Count == 0)
            {
                return float.MaxValue;
            }
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return Amount(position) > 0f ? 0f : float.MaxValue;
            }
            direction.Normalize();
            for (float d = 0f; d <= distance; d += 1.5f)
            {
                if (Amount(position + direction * d) > 0f)
                {
                    return d;
                }
            }
            return float.MaxValue;
        }
    }
}
