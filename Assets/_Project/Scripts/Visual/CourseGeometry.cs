using UnityEngine;

namespace FartRafters
{
    public static class CourseGeometry
    {
        public static float WallDistance(LevelDefinition level, Vector3 point)
        {
            float best = float.NegativeInfinity;
            Vector2 p = new Vector2(point.x, point.z);
            for (int i = 0; i < level.NodeCount - 1; i++)
            {
                Vector2 a = level.nodes[i].position;
                Vector2 b = level.nodes[i + 1].position;
                Vector2 ab = b - a;
                float lengthSquared = ab.sqrMagnitude;
                float t = lengthSquared > 0.0001f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / lengthSquared) : 0f;
                float distance = Vector2.Distance(p, a + ab * t);
                float half = Mathf.Lerp(level.nodes[i].width, level.nodes[i + 1].width, t) * 0.5f;
                best = Mathf.Max(best, half - distance);
            }
            return best;
        }

        public static int StableHash(string text)
        {
            unchecked
            {
                int hash = 23;
                if (text != null)
                {
                    foreach (char c in text)
                    {
                        hash = hash * 31 + c;
                    }
                }
                return hash;
            }
        }
    }
}
