using UnityEditor;
using UnityEngine;

namespace FartRafters.EditorTools
{
    public class ThirdPartyImport : AssetPostprocessor
    {
        const string Root = "Assets/_Project/ThirdParty/";
        const string Farts = "Assets/_Project/Audio/Farts/";

        public override uint GetVersion()
        {
            return 4;
        }

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Root))
            {
                return;
            }
            var importer = (TextureImporter)assetImporter;
            string name = System.IO.Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
            if (assetPath.EndsWith(".hdr"))
            {
                importer.textureShape = TextureImporterShape.TextureCube;
                importer.generateCubemap = TextureImporterGenerateCubemap.AutoCubemap;
                importer.maxTextureSize = 1024;
                importer.mipmapEnabled = true;
                return;
            }

            importer.maxTextureSize = 1024;
            importer.mipmapEnabled = true;
            importer.anisoLevel = assetPath.Contains("/PolyHaven/") ? 8 : 2;
            if (name.Contains("normal") || name.Contains("nor_gl"))
            {
                importer.textureType = TextureImporterType.NormalMap;
            }
            else if (name.Contains("orm") || name.EndsWith("_arm_1k"))
            {
                importer.sRGBTexture = false;
            }
            bool cutout = name.Contains("leaf") || name.Contains("leaves") || name == "grass" || name == "flowers";
            importer.alphaIsTransparency = cutout;
            importer.mipMapsPreserveCoverage = cutout;
            importer.alphaTestReferenceValue = 0.35f;
        }

        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(Root))
            {
                return;
            }
            var importer = (ModelImporter)assetImporter;
            importer.isReadable = true;
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
        }

        void OnPreprocessAudio()
        {
            if (assetPath.StartsWith("Assets/_Project/Audio/Ambience/"))
            {
                var ambience = (AudioImporter)assetImporter;
                ambience.forceToMono = true;
                ambience.loadInBackground = true;
                AudioImporterSampleSettings loop = ambience.defaultSampleSettings;
                loop.loadType = AudioClipLoadType.CompressedInMemory;
                loop.compressionFormat = AudioCompressionFormat.Vorbis;
                loop.quality = 0.4f;
                loop.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
                loop.sampleRateOverride = 22050;
                ambience.defaultSampleSettings = loop;
                return;
            }
            if (!assetPath.StartsWith(Farts) && !assetPath.StartsWith("Assets/_Project/Audio/Voices/") && !assetPath.StartsWith("Assets/_Project/Audio/Crowd/"))
            {
                return;
            }
            var importer = (AudioImporter)assetImporter;
            importer.forceToMono = true;
            importer.loadInBackground = false;
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.6f;
            settings.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
            settings.sampleRateOverride = 32000;
            importer.defaultSampleSettings = settings;
        }
    }
}
