using System.IO;
using System.Linq;
using Unity.CodeEditor;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FartRafters.EditorTools
{
    public static class ProjectSetup
    {
        public const string ApplicationId = "com.fartrafters.game";
        const string ApkPath = "Builds/Android/FartRafter.apk";
        const string DevenvPath = @"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\devenv.exe";

        [MenuItem("FartRaft/Proje Ayarlarını Uygula")]
        public static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "FartRafters";
            PlayerSettings.productName = "Fart Rafter";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, ApplicationId);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.SplashScreen.backgroundColor = new Color(0.07f, 0.08f, 0.21f, 1f);
            EditorUserBuildSettings.buildAppBundle = false;
            EnsureOnlineConfig();
            ApplyIcons();
            AssetDatabase.SaveAssets();
        }

        static void ApplyIcons()
        {
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AvatarPreview.IconPath);
            Texture2D clear = AssetDatabase.LoadAssetAtPath<Texture2D>(AvatarPreview.IconClearPath);
            if (icon == null)
            {
                return;
            }
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
            foreach (PlatformIconKind kind in new PlatformIconKind[] { AndroidPlatformIconKind.Legacy, AndroidPlatformIconKind.Round })
            {
                PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
                foreach (PlatformIcon platformIcon in icons)
                {
                    platformIcon.SetTexture(icon);
                }
                PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
            }
            if (clear != null)
            {
                PlatformIcon[] adaptive = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, AndroidPlatformIconKind.Adaptive);
                foreach (PlatformIcon platformIcon in adaptive)
                {
                    platformIcon.SetTextures(icon, clear);
                }
                PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, AndroidPlatformIconKind.Adaptive, adaptive);
            }
        }

        public static void EnsureOnlineConfig()
        {
            const string path = "Assets/_Project/Resources/OnlineConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<OnlineConfig>(path) != null)
            {
                return;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<OnlineConfig>(), path);
            AssetDatabase.SaveAssets();
        }

        public static void ConfigureScriptEditor()
        {
            if (File.Exists(DevenvPath))
            {
                CodeEditor.SetExternalScriptEditor(DevenvPath);
            }
        }

        public static void SetupAll()
        {
            ConfigurePlayer();
            ConfigureScriptEditor();
            SceneBuilder.Build();
            AssetDatabase.SaveAssets();
            Debug.Log("[FartRafters] Setup complete");
        }

        [MenuItem("FartRaft/Android APK Derle")]
        public static void BuildAndroid()
        {
            ConfigurePlayer();
            Directory.CreateDirectory(Path.GetDirectoryName(ApkPath));
            EnsureOnlineConfig();
            PlayerSettings.Android.forceInternetPermission = true;
            EditorUserBuildSettings.buildAppBundle = false;

            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            Debug.Log($"[FartRafters] Build {summary.result}: {summary.outputPath} {summary.totalSize / (1024f * 1024f):0.0} MB, {summary.totalErrors} errors, {summary.totalTime}");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
            }
        }

        public static void BuildAndroidPerf()
        {
            ConfigurePlayer();
            const string path = "Builds/Android/FartRafter_Test.apk";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            EditorUserBuildSettings.buildAppBundle = false;
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = path,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None,
                extraScriptingDefines = new[] { "FR_PERF" }
            };
            BuildSummary summary = BuildPipeline.BuildPlayer(options).summary;
            Debug.Log($"[FartRafters] Perf build {summary.result}: {summary.outputPath} {summary.totalErrors} errors, {summary.totalTime}");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
            }
        }

        public static void BuildSimulation()
        {
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.SplashScreen.backgroundColor = new Color(0.07f, 0.08f, 0.21f, 1f);
            SceneBuilder.RefreshBotLevels();
            EnsureOnlineConfig();
            const string path = "Builds/Sim/FartRafters.exe";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = path,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None
            };
            BuildSummary summary = BuildPipeline.BuildPlayer(options).summary;
            Debug.Log($"[FartRafters] Sim build {summary.result}: {summary.totalErrors} errors, {summary.totalTime}");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
            }
        }
    }
}
