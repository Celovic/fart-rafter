using UnityEngine;

namespace FartRafters
{
    public class DecorStreamer : MonoBehaviour
    {
        public LevelDefinition level;
        public CourseTheme theme;
        [Min(2)] public int chunkNodes = 6;
        [Min(10f)] public float buildRange = 75f;
        [Min(10f)] public float keepRange = 120f;
        [Min(0.05f)] public float interval = 0.25f;

        Transform[] chunks;
        float nextCheck;

        void Start()
        {
            if (level == null || theme == null || level.NodeCount < 2)
            {
                enabled = false;
                return;
            }
            if (chunks == null)
            {
                chunks = new Transform[Mathf.CeilToInt((level.NodeCount - 1) / (float)chunkNodes)];
            }
            Refresh(true);
        }

        void Update()
        {
            if (Time.time < nextCheck)
            {
                return;
            }
            nextCheck = Time.time + interval;
            Refresh(false);
        }

        public void BuildNear(Vector3 focus)
        {
            if (chunks == null)
            {
                chunks = new Transform[Mathf.CeilToInt((level.NodeCount - 1) / (float)chunkNodes)];
            }
            Refresh(true, focus);
        }

        void Refresh(bool all)
        {
            Refresh(all, Focus());
        }

        void Refresh(bool all, Vector3 focus)
        {
            bool built = false;
            for (int c = 0; c < chunks.Length; c++)
            {
                int start = c * chunkNodes;
                int end = Mathf.Min(level.NodeCount - 1, start + chunkNodes);
                float distance = Distance(focus, start, end);
                if (chunks[c] == null && distance < buildRange && (all || !built))
                {
                    chunks[c] = LandscapeBuilder.BuildDecorChunk(level, theme, transform, start, end, c);
                    built = true;
                }
                else if (chunks[c] != null && distance > keepRange)
                {
                    foreach (MeshFilter filter in chunks[c].GetComponentsInChildren<MeshFilter>())
                    {
                        Destroy(filter.sharedMesh);
                    }
                    Destroy(chunks[c].gameObject);
                    chunks[c] = null;
                }
            }
        }

        Vector3 Focus()
        {
            if (Raft.Current != null)
            {
                return Raft.Current.transform.position;
            }
            Camera camera = Camera.main;
            return camera != null ? camera.transform.position : level.Center(level.startNode);
        }

        float Distance(Vector3 focus, int start, int end)
        {
            float best = float.MaxValue;
            for (int i = start; i <= end; i++)
            {
                Vector3 offset = level.Center(i) - focus;
                offset.y = 0f;
                best = Mathf.Min(best, offset.magnitude);
            }
            return best;
        }
    }
}
