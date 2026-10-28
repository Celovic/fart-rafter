using UnityEngine;

namespace FartRafters
{
    public class WaterSurface : MonoBehaviour
    {
        static readonly int WaveId = Shader.PropertyToID("_FR_Wave");
        static readonly int TimeId = Shader.PropertyToID("_FR_WaterTime");

        public float baseHeight;
        [Min(0f)] public float waveAmplitude = 0.15f;
        [Min(0.1f)] public float waveLength = 8f;
        public float waveSpeed = 1.2f;

        public static WaterSurface Instance { get; private set; }

        void OnEnable()
        {
            Instance = this;
            PushShaderGlobals();
        }

        void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        void Update()
        {
            PushShaderGlobals();
        }

        void PushShaderGlobals()
        {
            Shader.SetGlobalVector(WaveId, new Vector4(waveAmplitude, waveLength, waveSpeed, baseHeight));
            Shader.SetGlobalFloat(TimeId, Time.time);
        }

        public float GetHeight(Vector3 position)
        {
            return GetHeight(position.x, position.z, Time.time);
        }

        public float GetHeight(float x, float z, float time)
        {
            float k = 2f * Mathf.PI / waveLength;
            float a = Mathf.Sin((x * 0.7f + z) * k + time * waveSpeed);
            float b = Mathf.Sin((x - z * 0.6f) * k * 1.7f + time * waveSpeed * 1.3f);
            return baseHeight + CourseProfile.Height(x, z) + waveAmplitude * (a + 0.5f * b);
        }
    }
}
