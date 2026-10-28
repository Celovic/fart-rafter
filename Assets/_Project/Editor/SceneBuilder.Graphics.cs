using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FartRafters.EditorTools
{
    public static partial class SceneBuilder
    {
        const string SculptShaderName = "FartRafters/SculptLit";
        const string GroundShaderName = "FartRafters/Ground";

        static ProceduralTextureBaker.Set baked;
        static bool texturesBaked;

        static ProceduralTextureBaker.Set Baked
        {
            get
            {
                if (!texturesBaked)
                {
                    baked = ProceduralTextureBaker.Bake();
                    texturesBaked = true;
                }
                return baked;
            }
        }

        static Material SculptMaterial(string name, float smoothness, float detailScale, float detailAlbedo, float detailNormal)
        {
            Shader shader = Shader.Find(SculptShaderName);
            Material material = LoadOrCreate(Root + "/Materials/" + name + ".mat", () => new Material(shader));
            material.shader = shader;
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Occlusion", 1f);
            material.SetTexture("_DetailMap", Baked.detail);
            material.SetTexture("_DetailNormal", Baked.detailNormal);
            material.SetFloat("_DetailScale", detailScale);
            material.SetFloat("_DetailAlbedo", detailAlbedo);
            material.SetFloat("_DetailNormalStrength", detailNormal);
            material.SetFloat("_DetailColorMix", 0f);
            material.SetFloat("_VertexColor", 1f);
            material.SetFloat("_RimStrength", 0.1f);
            material.SetFloat("_Translucency", 0f);
            material.SetFloat("_Detail", 1f);
            material.EnableKeyword("_DETAIL");
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material CreateFoliageMaterial()
        {
            Material material = SculptMaterial("Foliage", 0.12f, 2.4f, 0.45f, 1.1f);
            material.SetFloat("_Translucency", 0.45f);
            material.SetFloat("_RimStrength", 0.06f);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material CreateRaftMaterial()
        {
            Material material = SculptMaterial("RaftWood", 0.18f, 1.1f, 0.3f, 1.2f);
            material.SetTexture("_DetailMap", AssetDatabase.LoadAssetAtPath<Texture2D>(PolyHavenRoot + "/bark_brown_02_diff_1k.jpg"));
            material.SetTexture("_DetailNormal", AssetDatabase.LoadAssetAtPath<Texture2D>(PolyHavenRoot + "/bark_brown_02_nor_gl_1k.jpg"));
            material.SetFloat("_DetailColorMix", 0.45f);
            material.SetColor("_BaseColor", new Color(1.15f, 1.05f, 0.95f));
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material CreateEyeMaterial()
        {
            Material material = SculptMaterial("Eyes", 0.9f, 1f, 0f, 0f);
            material.SetFloat("_Detail", 0f);
            material.DisableKeyword("_DETAIL");
            material.SetFloat("_RimStrength", 0f);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material CreateGroundMaterial()
        {
            Shader shader = Shader.Find(GroundShaderName);
            Material material = LoadOrCreate(Root + "/Materials/Ground.mat", () => new Material(shader));
            material.shader = shader;
            material.SetTexture("_GrassMap", Load(PolyHavenRoot + "/leafy_grass_diff_1k.jpg"));
            material.SetTexture("_GrassNormal", Load(PolyHavenRoot + "/leafy_grass_nor_gl_1k.jpg"));
            material.SetTexture("_DirtMap", Load(PolyHavenRoot + "/forest_ground_04_diff_1k.jpg"));
            material.SetTexture("_DirtNormal", Load(PolyHavenRoot + "/forest_ground_04_nor_gl_1k.jpg"));
            material.SetTexture("_MudMap", Load(PolyHavenRoot + "/brown_mud_rocks_01_diff_1k.jpg"));
            material.SetTexture("_MudNormal", Load(PolyHavenRoot + "/brown_mud_rocks_01_nor_gl_1k.jpg"));
            material.SetTexture("_PebbleMap", Load(PolyHavenRoot + "/river_small_rocks_diff_1k.jpg"));
            material.SetTexture("_PebbleNormal", Load(PolyHavenRoot + "/river_small_rocks_nor_gl_1k.jpg"));
            material.SetTexture("_MacroMap", Baked.macro);
            material.SetColor("_GrassTint", new Color(0.66f, 0.84f, 0.5f));
            material.SetColor("_DirtTint", new Color(0.95f, 0.9f, 0.84f));
            material.SetColor("_MudTint", new Color(0.86f, 0.76f, 0.66f));
            material.SetColor("_PebbleTint", new Color(0.98f, 0.95f, 0.9f));
            material.SetFloat("_GrassScale", 1f);
            material.SetFloat("_DirtScale", 0.8f);
            material.SetFloat("_MudScale", 0.9f);
            material.SetFloat("_PebbleScale", 1.3f);
            material.SetFloat("_MacroScale", 0.03f);
            material.SetFloat("_NormalStrength", 1f);
            material.SetFloat("_WetHeight", 0.3f);
            material.SetFloat("_DirtNoise", 0.4f);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Texture2D Load(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static void UpgradeRockMaterials(CourseTheme theme)
        {
            Shader shader = Shader.Find(SculptShaderName);
            Texture2D rock = Load(PolyHavenRoot + "/mossy_rock_diff_1k.jpg");
            Texture2D rockNormal = Load(PolyHavenRoot + "/mossy_rock_nor_gl_1k.jpg");
            var done = new System.Collections.Generic.HashSet<Material>();
            foreach (GameObject[] models in new[] { theme.rockModels, theme.pebbleModels })
            {
                if (models == null)
                {
                    continue;
                }
                foreach (GameObject model in models)
                {
                    foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
                    {
                        foreach (Material material in renderer.sharedMaterials)
                        {
                            if (material == null || !done.Add(material))
                            {
                                continue;
                            }
                            material.shader = shader;
                            material.SetColor("_BaseColor", new Color(0.9f, 0.88f, 0.84f));
                            material.SetFloat("_Smoothness", 0.22f);
                            material.SetFloat("_Occlusion", 1f);
                            material.SetTexture("_DetailMap", rock);
                            material.SetTexture("_DetailNormal", rockNormal);
                            material.SetFloat("_DetailScale", 0.8f);
                            material.SetFloat("_DetailAlbedo", 0.3f);
                            material.SetFloat("_DetailNormalStrength", 1.1f);
                            material.SetFloat("_DetailColorMix", 0.9f);
                            material.SetFloat("_VertexColor", 0f);
                            material.SetFloat("_RimStrength", 0.04f);
                            material.SetFloat("_Translucency", 0f);
                            material.SetFloat("_Detail", 1f);
                            material.EnableKeyword("_DETAIL");
                            material.enableInstancing = true;
                            EditorUtility.SetDirty(material);
                        }
                    }
                }
            }
        }

        static void ConfigureRenderer()
        {
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
            if (asset == null)
            {
                return;
            }
            asset.supportsCameraDepthTexture = false;
            asset.supportsCameraOpaqueTexture = false;
            asset.supportsHDR = true;
            asset.msaaSampleCount = 2;
            asset.renderScale = 0.9f;
            asset.mainLightShadowmapResolution = 1024;
            asset.shadowDistance = 45f;
            asset.shadowCascadeCount = 1;
            EditorUtility.SetDirty(asset);
        }

        static void ApplyPostFx(VolumeProfile profile)
        {
            if (!profile.TryGet(out Tonemapping tonemapping))
            {
                tonemapping = profile.Add<Tonemapping>(true);
                AssetDatabase.AddObjectToAsset(tonemapping, profile);
            }
            tonemapping.mode.Override(TonemappingMode.ACES);

            if (!profile.TryGet(out ColorAdjustments color))
            {
                color = profile.Add<ColorAdjustments>(true);
                AssetDatabase.AddObjectToAsset(color, profile);
            }
            color.postExposure.Override(0.15f);
            color.contrast.Override(14f);
            color.saturation.Override(16f);

            if (!profile.TryGet(out WhiteBalance balance))
            {
                balance = profile.Add<WhiteBalance>(true);
                AssetDatabase.AddObjectToAsset(balance, profile);
            }
            balance.temperature.Override(6f);
            balance.tint.Override(2f);

            if (!profile.TryGet(out Bloom bloom))
            {
                bloom = profile.Add<Bloom>(true);
                AssetDatabase.AddObjectToAsset(bloom, profile);
            }
            bloom.threshold.Override(1.05f);
            bloom.intensity.Override(0.35f);
            bloom.scatter.Override(0.6f);
            bloom.downscale.Override(BloomDownscaleMode.Quarter);
            bloom.maxIterations.Override(4);
            bloom.highQualityFiltering.Override(false);

            if (!profile.TryGet(out Vignette vignette))
            {
                vignette = profile.Add<Vignette>(true);
                AssetDatabase.AddObjectToAsset(vignette, profile);
            }
            vignette.intensity.Override(0.18f);
            vignette.smoothness.Override(0.45f);
            EditorUtility.SetDirty(profile);
        }


        static void BakeEnvironment(UnityEngine.SceneManagement.Scene scene, string path)
        {
            Lightmapping.bakedGI = false;
            Lightmapping.realtimeGI = false;
            Lightmapping.Bake();
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, path);
        }
        static void ConfigureLighting(Material sky)
        {
            RenderSettings.skybox = sky;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.05f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
            RenderSettings.reflectionIntensity = 1f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.68f, 0.78f, 0.86f);
            RenderSettings.fogDensity = 0.0085f;
            DynamicGI.UpdateEnvironment();

            Light sun = Object.FindAnyObjectByType<Light>();
            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler(42f, -40f, 0f);
                sun.color = new Color(1f, 0.93f, 0.82f);
                sun.intensity = 1.6f;
                sun.shadows = LightShadows.Soft;
                sun.shadowStrength = 0.85f;
                sun.shadowBias = 0.05f;
                sun.shadowNormalBias = 0.4f;
            }
        }
    }
}
