using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace FartRafters
{
    public enum QualityLevel
    {
        High,
        Medium,
        Low
    }

    public static class GraphicsQuality
    {
        const string ModeKey = "gfx.mode";
        const string AutoKey = "gfx.auto";
        const string BenchKey = "gfx.bench";
        const string FpsKey = "gfx.fps";
        const string LowKeyword = "FR_LOW_DETAIL";

        static readonly float[] BaseScales = { 0.9f, 0.75f, 0.65f };
        static readonly string[] Names = { "Yüksek", "Orta", "Düşük" };

        static bool booted;

        public static event Action Changed;

        public static bool Frozen { get; set; }

        public static int Mode { get; private set; } = -1;
        public static QualityLevel Level { get; private set; } = QualityLevel.Medium;
        public static float Scale { get; private set; } = 0.75f;
        public static int TargetFps { get; private set; } = 60;
        public static bool Benchmarked => PlayerPrefs.GetInt(BenchKey, 0) == 1;
        public static float BaseScale => BaseScales[(int)Level];
        public static string LevelName => Names[(int)Level];
        public static string ModeName => Mode < 0 ? "Otomatik (" + LevelName + ")" : Names[Mode];

        public static int CrowdPerGroup => Level == QualityLevel.Low ? 2 : Level == QualityLevel.Medium ? 3 : 5;
        public static int PlannerLanes(int requested) => Level == QualityLevel.Low ? Mathf.Min(requested, 9) : Level == QualityLevel.Medium ? Mathf.Min(requested, 11) : requested;

        static UniversalRenderPipelineAsset Asset => GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (booted)
            {
                return;
            }
            booted = true;
            Mode = PlayerPrefs.GetInt(ModeKey, -1);
            TargetFps = PlayerPrefs.GetInt(FpsKey, 60) == 30 ? 30 : 60;
            int auto = PlayerPrefs.GetInt(AutoKey, -1);
            if (auto < 0)
            {
                auto = (int)Detect();
                PlayerPrefs.SetInt(AutoKey, auto);
            }
            Level = Mode >= 0 ? (QualityLevel)Mode : (QualityLevel)auto;
            Scale = BaseScale;
            SceneManager.sceneLoaded += (scene, mode) => ApplyScene();
            var runner = new GameObject("GraphicsQuality");
            UnityEngine.Object.DontDestroyOnLoad(runner);
            runner.AddComponent<GraphicsQualityRunner>();
            Apply();
        }

        public static QualityLevel Detect()
        {
            string gpu = (SystemInfo.graphicsDeviceName ?? "").ToLowerInvariant();
            int memory = SystemInfo.systemMemorySize;
            QualityLevel level = Application.isMobilePlatform ? QualityLevel.Medium : QualityLevel.High;
            if (gpu.Contains("adreno"))
            {
                int number = Number(gpu, "adreno");
                level = number >= 700 ? QualityLevel.High : number >= 610 ? QualityLevel.Medium : QualityLevel.Low;
            }
            else if (gpu.Contains("immortalis") || gpu.Contains("xclipse"))
            {
                level = QualityLevel.High;
            }
            else if (gpu.Contains("mali"))
            {
                if (gpu.Contains("g7"))
                {
                    level = QualityLevel.High;
                }
                else if (gpu.Contains("g6") || gpu.Contains("g57") || gpu.Contains("g68") || gpu.Contains("g78"))
                {
                    level = QualityLevel.Medium;
                }
                else
                {
                    level = QualityLevel.Low;
                }
            }
            else if (gpu.Contains("powervr"))
            {
                level = QualityLevel.Low;
            }
            if (Application.isMobilePlatform)
            {
                if (memory > 0 && memory < 3000)
                {
                    level = QualityLevel.Low;
                }
                else if (memory > 0 && memory < 4500 && level == QualityLevel.High)
                {
                    level = QualityLevel.Medium;
                }
                if (SystemInfo.processorCount > 0 && SystemInfo.processorCount < 6 && level == QualityLevel.High)
                {
                    level = QualityLevel.Medium;
                }
            }
            return level;
        }

        static int Number(string text, string after)
        {
            int start = text.IndexOf(after, StringComparison.Ordinal) + after.Length;
            int value = 0;
            bool found = false;
            for (int i = start; i < text.Length; i++)
            {
                if (char.IsDigit(text[i]))
                {
                    value = value * 10 + (text[i] - '0');
                    found = true;
                }
                else if (found)
                {
                    break;
                }
            }
            return value;
        }

        public static void SetMode(int mode)
        {
            Mode = Mathf.Clamp(mode, -1, 2);
            PlayerPrefs.SetInt(ModeKey, Mode);
            PlayerPrefs.Save();
            Level = Mode >= 0 ? (QualityLevel)Mode : (QualityLevel)PlayerPrefs.GetInt(AutoKey, (int)QualityLevel.Medium);
            Scale = BaseScale;
            Apply();
        }

        public static void CycleMode()
        {
            SetMode(Mode >= 2 ? -1 : Mode + 1);
        }

        public static void SetTargetFps(int fps)
        {
            TargetFps = fps == 30 ? 30 : 60;
            PlayerPrefs.SetInt(FpsKey, TargetFps);
            PlayerPrefs.Save();
            Application.targetFrameRate = TargetFps;
        }

        public static void ReportBenchmark(float fps)
        {
            PlayerPrefs.SetInt(BenchKey, 1);
            if (Mode < 0 && fps < TargetFps * 0.8f && Level < QualityLevel.Low)
            {
                StepDown();
            }
            PlayerPrefs.Save();
        }

        public static void StepDown()
        {
            if (Mode >= 0 || Level >= QualityLevel.Low)
            {
                return;
            }
            Level++;
            PlayerPrefs.SetInt(AutoKey, (int)Level);
            Scale = BaseScale;
            Apply();
        }

        public static void SetScale(float scale)
        {
            Scale = Mathf.Clamp(scale, 0.55f, 1f);
            UniversalRenderPipelineAsset asset = Asset;
            if (asset != null)
            {
                asset.renderScale = Scale;
            }
        }

        public static void Apply()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFps;
            UniversalRenderPipelineAsset asset = Asset;
            if (asset != null)
            {
                asset.renderScale = Scale;
                asset.msaaSampleCount = Level == QualityLevel.High ? 2 : 1;
                asset.supportsCameraDepthTexture = false;
                asset.supportsCameraOpaqueTexture = false;
                asset.shadowDistance = Level == QualityLevel.High ? 45f : Level == QualityLevel.Medium ? 35f : 25f;
                asset.shadowCascadeCount = 1;
                asset.mainLightShadowmapResolution = Level == QualityLevel.Low ? 512 : 1024;
            }
            if (Level == QualityLevel.High)
            {
                Shader.DisableKeyword(LowKeyword);
            }
            else
            {
                Shader.EnableKeyword(LowKeyword);
            }
            ApplyScene();
            Changed?.Invoke();
        }

        public static void ApplyScene()
        {
            foreach (Camera camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
                if (data == null)
                {
                    continue;
                }
                data.antialiasing = Level == QualityLevel.High ? AntialiasingMode.None : AntialiasingMode.FastApproximateAntialiasing;
                data.renderPostProcessing = Level != QualityLevel.Low;
            }
            foreach (Volume volume in UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsSortMode.None))
            {
                if (volume.profile != null && volume.profile.TryGet(out Bloom bloom))
                {
                    bloom.active = Level == QualityLevel.High;
                }
            }
            foreach (Light light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type == LightType.Directional && light.shadows != LightShadows.None)
                {
                    light.shadows = Level == QualityLevel.High ? LightShadows.Soft : LightShadows.Hard;
                }
            }
        }
    }

    public class GraphicsQualityRunner : MonoBehaviour
    {
        float elapsed;
        int frames;
        int slowWindows;
        float benchStart = -1f;
        float benchTime;
        int benchFrames;

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnLoaded;
        }

        void OnLoaded(Scene scene, LoadSceneMode mode)
        {
            elapsed = 0f;
            frames = 0;
            slowWindows = 0;
            if (!GraphicsQuality.Benchmarked && scene.name == GameSelection.MenuScene)
            {
                benchStart = Time.unscaledTime + 2f;
                benchTime = 0f;
                benchFrames = 0;
            }
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (dt > 0.5f || GraphicsQuality.Frozen)
            {
                return;
            }
            if (benchStart > 0f && Time.unscaledTime >= benchStart)
            {
                benchTime += dt;
                benchFrames++;
                if (benchTime >= 3f)
                {
                    GraphicsQuality.ReportBenchmark(benchFrames / benchTime);
                    benchStart = -1f;
                }
                return;
            }

            elapsed += dt;
            frames++;
            if (elapsed < 2f)
            {
                return;
            }
            float fps = frames / elapsed;
            elapsed = 0f;
            frames = 0;
            float target = GraphicsQuality.TargetFps;
            if (fps < target * 0.85f)
            {
                if (GraphicsQuality.Scale > 0.56f)
                {
                    GraphicsQuality.SetScale(GraphicsQuality.Scale - 0.05f);
                }
                else if (fps < target * 0.75f && ++slowWindows >= 3)
                {
                    slowWindows = 0;
                    GraphicsQuality.StepDown();
                }
            }
            else
            {
                slowWindows = 0;
                if (fps > target * 0.97f && GraphicsQuality.Scale < GraphicsQuality.BaseScale - 0.01f)
                {
                    GraphicsQuality.SetScale(GraphicsQuality.Scale + 0.05f);
                }
            }
        }
    }
}
