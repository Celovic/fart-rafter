using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DebugSymbolFormat = Unity.Android.Types.DebugSymbolFormat;
using DebugSymbolLevel = Unity.Android.Types.DebugSymbolLevel;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FartRafters.EditorTools
{
    /// <summary>
    /// Google Play'e yüklenebilir, imzalı Android App Bundle (.aab) üretir.
    ///
    /// Play şartları (Kasım 2026):
    ///  - Format: AAB (APK kabul edilmiyor)
    ///  - targetSdk: API 36 (Android 16) veya üstü
    ///  - 64-bit (ARM64) native kod, IL2CPP
    ///  - Upload key ile imza (debug imzası reddedilir)
    ///  - Her yüklemede bir öncekinden büyük versionCode
    ///
    /// İmza bilgileri koda/Git'e girmez. Sırasıyla şuradan okunur:
    ///  1) Ortam değişkenleri: FR_KEYSTORE_PATH, FR_KEYSTORE_PASS, FR_KEY_ALIAS, FR_KEY_PASS
    ///  2) Proje kökünde Keystore/upload.properties (gitignore'da)
    /// </summary>
    public static class PlayStoreBuild
    {
        public const int TargetApiLevel = 36;
        const string OutputDir = "Builds/Android";
        const string PropertiesPath = "Keystore/upload.properties";
        const long BaseModuleLimitBytes = 200L * 1024 * 1024;

        [MenuItem("FartRaft/Google Play AAB Derle (imzalı)")]
        public static void BuildFromMenu()
        {
            Build(bumpVersionCode: true);
        }

        [MenuItem("FartRaft/Google Play AAB Derle (sürüm kodunu artırmadan)")]
        public static void BuildFromMenuSameVersion()
        {
            Build(bumpVersionCode: false);
        }

        /// <summary>
        /// Komut satırı / CI:
        /// Unity -batchmode -quit -projectPath . -executeMethod FartRafters.EditorTools.PlayStoreBuild.BuildFromCommandLine
        /// İsteğe bağlı: FR_VERSION_CODE, FR_VERSION_NAME ortam değişkenleri.
        /// </summary>
        public static void BuildFromCommandLine()
        {
            Build(bumpVersionCode: true);
        }

        static void Build(bool bumpVersionCode)
        {
            ProjectSetup.ConfigurePlayer();
            ProjectSetup.EnsureOnlineConfig();

            SigningConfig signing = SigningConfig.Load(out string signingError);
            if (signing == null)
            {
                Fail("İmza bilgisi bulunamadı.\n\n" + signingError + "\n\nAyrıntı: Docs/GOOGLE_PLAY.md");
                return;
            }

            ApplyVersion(bumpVersionCode);
            ApplyPlaySettings();

            string previousKeystore = PlayerSettings.Android.keystoreName;
            string previousAlias = PlayerSettings.Android.keyaliasName;
            bool previousUseCustom = PlayerSettings.Android.useCustomKeystore;

            BuildSummary summary;
            string outputPath = Path.Combine(OutputDir,
                $"FartRafter-{PlayerSettings.bundleVersion}-{PlayerSettings.Android.bundleVersionCode}.aab");
            try
            {
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = signing.KeystorePath;
                PlayerSettings.Android.keystorePass = signing.KeystorePass;
                PlayerSettings.Android.keyaliasName = signing.KeyAlias;
                PlayerSettings.Android.keyaliasPass = signing.KeyPass;

                Directory.CreateDirectory(OutputDir);
                var options = new BuildPlayerOptions
                {
                    scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                    locationPathName = outputPath,
                    target = BuildTarget.Android,
                    targetGroup = BuildTargetGroup.Android,
                    options = BuildOptions.None
                };
                summary = BuildPipeline.BuildPlayer(options).summary;
            }
            finally
            {
                // Keystore yolu ve şifreler ProjectSettings.asset'e (dolayısıyla Git'e) yazılmasın.
                PlayerSettings.Android.keystorePass = string.Empty;
                PlayerSettings.Android.keyaliasPass = string.Empty;
                PlayerSettings.Android.keystoreName = previousKeystore;
                PlayerSettings.Android.keyaliasName = previousAlias;
                PlayerSettings.Android.useCustomKeystore = previousUseCustom;
                EditorUserBuildSettings.buildAppBundle = false;
                AssetDatabase.SaveAssets();
            }

            bool ok = summary.result == BuildResult.Succeeded;
            string sizeNote = string.Empty;
            if (ok && File.Exists(outputPath))
            {
                long size = new FileInfo(outputPath).Length;
                sizeNote = $"{size / (1024f * 1024f):0.0} MB";
                if (size > BaseModuleLimitBytes)
                {
                    Debug.LogWarning($"[FartRafters] AAB {sizeNote}. Play base modül indirme sınırı 200 MB (sıkıştırılmış). " +
                                     "Play Console reddederse büyük içerik Play Asset Delivery'ye taşınmalı.");
                }
            }

            string message = ok
                ? $"Google Play paketi hazır:\n{Path.GetFullPath(outputPath)}\n\n" +
                  $"Sürüm: {PlayerSettings.bundleVersion} (kod {PlayerSettings.Android.bundleVersionCode}), {sizeNote}\n" +
                  $"targetSdk {TargetApiLevel}, ARM64, IL2CPP, sembol tablosu paketin içinde."
                : $"Build başarısız: {summary.totalErrors} hata. Console'a bakın.";
            Debug.Log("[FartRafters] Play build " + summary.result + " — " + message.Replace('\n', ' '));

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(ok ? 0 : 1);
                return;
            }
            EditorUtility.DisplayDialog("Google Play AAB", message, "Tamam");
            if (ok)
            {
                EditorUtility.RevealInFinder(outputPath);
            }
        }

        static void ApplyVersion(bool bump)
        {
            string versionName = Environment.GetEnvironmentVariable("FR_VERSION_NAME");
            if (!string.IsNullOrWhiteSpace(versionName))
            {
                PlayerSettings.bundleVersion = versionName.Trim();
            }

            string codeText = Environment.GetEnvironmentVariable("FR_VERSION_CODE");
            if (!string.IsNullOrWhiteSpace(codeText) && int.TryParse(codeText.Trim(), out int code) && code > 0)
            {
                PlayerSettings.Android.bundleVersionCode = code;
            }
            else if (bump)
            {
                PlayerSettings.Android.bundleVersionCode = Math.Max(1, PlayerSettings.Android.bundleVersionCode + 1);
            }
        }

        static void ApplyPlaySettings()
        {
            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.development = false;

            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)TargetApiLevel;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.splitApplicationBinary = false;

            // Play Console'un "yerel hata ayıklama sembolü yok" uyarısını giderir ve
            // çökme raporlarında okunabilir stack trace sağlar.
            UserBuildSettings.DebugSymbols.level = DebugSymbolLevel.SymbolTable;
            UserBuildSettings.DebugSymbols.format = DebugSymbolFormat.IncludeInBundle;
        }

        sealed class SigningConfig
        {
            public string KeystorePath;
            public string KeystorePass;
            public string KeyAlias;
            public string KeyPass;

            public static SigningConfig Load(out string error)
            {
                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (File.Exists(PropertiesPath))
                {
                    foreach (string rawLine in File.ReadAllLines(PropertiesPath))
                    {
                        string line = rawLine.Trim();
                        if (line.Length == 0 || line.StartsWith("#"))
                        {
                            continue;
                        }
                        int eq = line.IndexOf('=');
                        if (eq > 0)
                        {
                            values[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
                        }
                    }
                }

                string Get(string env, string key)
                {
                    string v = Environment.GetEnvironmentVariable(env);
                    if (!string.IsNullOrEmpty(v))
                    {
                        return v;
                    }
                    return values.TryGetValue(key, out string fromFile) ? fromFile : null;
                }

                var config = new SigningConfig
                {
                    KeystorePath = Get("FR_KEYSTORE_PATH", "storeFile"),
                    KeystorePass = Get("FR_KEYSTORE_PASS", "storePassword"),
                    KeyAlias = Get("FR_KEY_ALIAS", "keyAlias"),
                    KeyPass = Get("FR_KEY_PASS", "keyPassword")
                };

                var missing = new List<string>();
                if (string.IsNullOrEmpty(config.KeystorePath)) missing.Add("storeFile / FR_KEYSTORE_PATH");
                if (string.IsNullOrEmpty(config.KeystorePass)) missing.Add("storePassword / FR_KEYSTORE_PASS");
                if (string.IsNullOrEmpty(config.KeyAlias)) missing.Add("keyAlias / FR_KEY_ALIAS");
                if (string.IsNullOrEmpty(config.KeyPass)) config.KeyPass = config.KeystorePass;

                if (missing.Count > 0)
                {
                    error = "Eksik: " + string.Join(", ", missing) +
                            $"\n\n{PropertiesPath} dosyasını oluşturun (örnek: Keystore/upload.properties.example).";
                    return null;
                }

                if (!Path.IsPathRooted(config.KeystorePath))
                {
                    config.KeystorePath = Path.GetFullPath(config.KeystorePath);
                }
                if (!File.Exists(config.KeystorePath))
                {
                    error = "Keystore dosyası yok: " + config.KeystorePath;
                    return null;
                }

                error = null;
                return config;
            }
        }

        static void Fail(string message)
        {
            Debug.LogError("[FartRafters] " + message.Replace('\n', ' '));
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
                return;
            }
            EditorUtility.DisplayDialog("Google Play AAB", message, "Tamam");
        }
    }
}
