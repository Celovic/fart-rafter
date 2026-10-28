using UnityEngine;

namespace FartRafters
{
    public static class CourseProfile
    {
        const int Window = 6;
        const float FarDistance = 25f;

        static Vector2[] points = new Vector2[0];
        static float[] heights = new float[0];
        static int hint;

        public static bool Active { get; private set; }

        public static void Setup(LevelDefinition level)
        {
            Clear();
            if (level == null || level.heights == null || level.heights.Length != level.NodeCount || level.NodeCount < 2)
            {
                return;
            }
            points = new Vector2[level.NodeCount];
            heights = new float[level.NodeCount];
            bool any = false;
            for (int i = 0; i < level.NodeCount; i++)
            {
                points[i] = level.nodes[i].position;
                heights[i] = level.heights[i];
                any |= Mathf.Abs(heights[i]) > 0.001f;
            }
            Active = any;
        }

        public static void Clear()
        {
            Active = false;
            points = new Vector2[0];
            heights = new float[0];
            hint = 0;
        }

        public static float Height(float x, float z)
        {
            if (!Active)
            {
                return 0f;
            }
            var p = new Vector2(x, z);
            int count = points.Length - 1;
            int from = Mathf.Clamp(hint - Window, 0, count - 1);
            int to = Mathf.Clamp(hint + Window, 0, count - 1);
            float best = Search(p, from, to, out int segment, out float t);
            if (best > FarDistance * FarDistance)
            {
                Search(p, 0, count - 1, out segment, out t);
            }
            hint = segment;
            return Mathf.Lerp(heights[segment], heights[segment + 1], t);
        }

        public static Vector2 Gradient(float x, float z)
        {
            if (!Active)
            {
                return Vector2.zero;
            }
            const float d = 0.75f;
            float dx = Height(x + d, z) - Height(x - d, z);
            float dz = Height(x, z + d) - Height(x, z - d);
            return new Vector2(dx, dz) / (2f * d);
        }

        static float Search(Vector2 p, int from, int to, out int segment, out float t)
        {
            float best = float.MaxValue;
            segment = from;
            t = 0f;
            for (int i = from; i <= to; i++)
            {
                Vector2 a = points[i];
                Vector2 ab = points[i + 1] - a;
                float length = ab.sqrMagnitude;
                float u = length > 0.0001f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / length) : 0f;
                float distance = (p - (a + ab * u)).sqrMagnitude;
                if (distance < best)
                {
                    best = distance;
                    segment = i;
                    t = u;
                }
            }
            return best;
        }
    }
}
