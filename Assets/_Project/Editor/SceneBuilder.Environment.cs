using System.Collections.Generic;
using System.Linq;
using FartRafters.EditorTools.Sculpt;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FartRafters.EditorTools
{
    public static partial class SceneBuilder
    {
        const string NatureRoot = Root + "/ThirdParty/StylizedNature";
        const string PropsRoot = Root + "/ThirdParty/FantasyProps";
        const string KayKitRoot = Root + "/ThirdParty/KayKitForest";
        const string PolyHavenRoot = Root + "/ThirdParty/PolyHaven";
        const string SkyTexturePath = PolyHavenRoot + "/kloofendal_48d_partly_cloudy_puresky_2k.hdr";

        static readonly string[] CutoutMaterials = { "Leaves", "Leaves_NormalTree", "Leaves_TwistedTree", "Leaves_Pine", "Grass", "Flowers" };

        static void ApplyEnvironment(CourseTheme theme)
        {
            EnsureFolder(Root + "/Materials/Nature");
            RemapModels(NatureRoot);
            RemapModels(PropsRoot);
            RemapModels(KayKitRoot);

            theme.treeModels = Models(KayKitRoot, "Tree_1_A_Color1", "Tree_1_B_Color1", "Tree_2_A_Color1", "Tree_2_B_Color1", "Tree_2_C_Color1", "Tree_3_A_Color1", "Tree_3_B_Color1", "Tree_4_A_Color1", "Tree_4_B_Color1");
            theme.pineModels = Models(NatureRoot, "Pine_1", "Pine_2", "Pine_3", "Pine_4", "Pine_5");
            theme.bushModels = Models(KayKitRoot, "Bush_1_C_Color1", "Bush_2_A_Color1", "Bush_2_C_Color1", "Bush_3_A_Color1", "Bush_4_A_Color1");
            theme.rockModels = Models(NatureRoot, "Rock_Medium_1", "Rock_Medium_2", "Rock_Medium_3");
            theme.grassModels = Models(NatureRoot, "Grass_Common_Short", "Grass_Wispy_Short");
            theme.reedModels = Models(NatureRoot, "Grass_Wispy_Tall", "Grass_Common_Tall");
            theme.flowerModels = Models(NatureRoot, "Flower_3_Group", "Flower_4_Group", "Flower_3_Single");
            theme.plantModels = Models(NatureRoot, "Plant_1", "Plant_7", "Clover_1", "Clover_2", "Mushroom_Common");
            theme.pebbleModels = Models(NatureRoot, "Pebble_Round_1", "Pebble_Round_2", "Pebble_Round_3", "Pebble_Round_4", "Pebble_Round_5");
            theme.raftProps = Models(PropsRoot, "Barrel", "Bucket_Wooden_1");

            theme.grass = CreateGroundMaterial();
            theme.bank = theme.grass;
            theme.raftLog = Textured("RaftPole", PolyHavenRoot + "/bark_brown_02_diff_1k.jpg", PolyHavenRoot + "/bark_brown_02_nor_gl_1k.jpg", Color.white, 0.1f, new Vector2(1f, 3f));
            theme.flag = Lit("Flag", new Color(0.92f, 0.26f, 0.22f), 0.15f);
            theme.flag.SetFloat("_Cull", 0f);

            theme.sculptMaterial = CreateCharacterMaterial();
            theme.foliageMaterial = CreateFoliageMaterial();
            theme.raftMaterial = CreateRaftMaterial();
            theme.eyeMaterial = CreateEyeMaterial();
            UpgradeRockMaterials(theme);
            theme.raftMesh = SaveMesh(SurfaceNets.Build(PropSculptor.Raft(), 0.028f, "Raft"), "Props");
            theme.crocodileMesh = SaveMesh(SurfaceNets.Build(PropSculptor.Crocodile(), 0.014f, "Crocodile"), "Props");
            theme.buoyMesh = SaveMesh(SurfaceNets.Build(PropSculptor.Buoy(), 0.016f, "Buoy"), "Props");
            theme.brambleMesh = SaveMesh(SurfaceNets.Build(PropSculptor.Bramble(), 0.02f, "Bramble"), "Props");
            theme.pickupMeshes = new[]
            {
                SaveMesh(SurfaceNets.Build(PropSculptor.Beans(), 0.014f, "PickupBeans"), "Props"),
                SaveMesh(SurfaceNets.Build(PropSculptor.Pastirma(), 0.014f, "PickupPastirma"), "Props"),
                SaveMesh(SurfaceNets.Build(PropSculptor.Onion(), 0.014f, "PickupOnion"), "Props"),
                SaveMesh(SurfaceNets.Build(PropSculptor.Chickpea(), 0.012f, "PickupChickpea"), "Props"),
                SaveMesh(SurfaceNets.Build(PropSculptor.Pepper(), 0.012f, "PickupPepper"), "Props"),
                SaveMesh(SurfaceNets.Build(PropSculptor.Egg(), 0.012f, "PickupEgg"), "Props"),
                SaveMesh(SurfaceNets.Build(PropSculptor.RepairKit(), 0.012f, "PickupRepair"), "Props"),
                SaveMesh(SurfaceNets.Build(PropSculptor.Bubble(), 0.012f, "PickupBubble"), "Props"),
                SaveMesh(SurfaceNets.Build(PropSculptor.Magnet(), 0.012f, "PickupMagnet"), "Props")
            };
            theme.treeMeshes = new Mesh[4];
            theme.pineMeshes = new Mesh[3];
            theme.bushMeshes = new Mesh[4];
            for (int i = 0; i < theme.treeMeshes.Length; i++)
            {
                theme.treeMeshes[i] = SaveMesh(SurfaceNets.Build(PropSculptor.RoundTree(i), 0.23f, "Tree" + i), "Nature");
            }
            for (int i = 0; i < theme.pineMeshes.Length; i++)
            {
                theme.pineMeshes[i] = SaveMesh(SurfaceNets.Build(PropSculptor.Pine(i), 0.22f, "Pine" + i), "Nature");
            }
            for (int i = 0; i < theme.bushMeshes.Length; i++)
            {
                theme.bushMeshes[i] = SaveMesh(SurfaceNets.Build(PropSculptor.Bush(i), 0.16f, "Bush" + i), "Nature");
            }
            Debug.Log("[FartRafters] Nature verts tree " + theme.treeMeshes[0].vertexCount + " pine " + theme.pineMeshes[0].vertexCount + " bush " + theme.bushMeshes[0].vertexCount);
            theme.frogMesh = SaveMesh(SurfaceNets.Build(PropSculptor.Frog(), 0.012f, "Frog"), "Props");
            theme.lilyPadMesh = SaveMesh(SurfaceNets.Build(PropSculptor.LilyPad(), 0.015f, "LilyPad"), "Props");
            theme.birdBodyMesh = SaveMesh(SurfaceNets.Build(PropSculptor.BirdBody(), 0.012f, "BirdBody"), "Props");
            theme.birdWingMesh = SaveMesh(SurfaceNets.Build(PropSculptor.BirdWing(), 0.01f, "BirdWing"), "Props");
            ApplySpectators(theme);
            theme.millWheelMesh = SaveMesh(SurfaceNets.Build(PropSculptor.MillWheel(), 0.05f, "MillWheel"), "Props");
        }

        static GameObject[] Models(string folder, params string[] names)
        {
            var models = new List<GameObject>();
            foreach (string name in names)
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(folder + "/Models/" + name + ".fbx");
                if (model != null)
                {
                    models.Add(model);
                }
                else
                {
                    Debug.LogWarning("[FartRafters] Model missing: " + name);
                }
            }
            return models.ToArray();
        }

        static void RemapModels(string folder)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { folder + "/Models" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                {
                    continue;
                }
                Dictionary<AssetImporter.SourceAssetIdentifier, Object> existing = importer.GetExternalObjectMap();
                bool changed = false;
                foreach (Material embedded in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Material>())
                {
                    Material target = NatureMaterial(embedded, folder);
                    var id = new AssetImporter.SourceAssetIdentifier(typeof(Material), embedded.name);
                    if (existing.TryGetValue(id, out Object current) && current == target)
                    {
                        continue;
                    }
                    importer.AddRemap(id, target);
                    changed = true;
                }
                if (changed)
                {
                    importer.SaveAndReimport();
                }
            }
        }

        static Material NatureMaterial(Material source, string folder)
        {
            string path = Root + "/Materials/Nature/" + source.name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = source.name };
            Texture texture = source.HasProperty("_BaseMap") ? source.GetTexture("_BaseMap") : null;
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Smoothness", source.name.StartsWith("MI_") ? 0.25f : 0.05f);
            material.SetFloat("_EnvironmentReflections", 0f);
            material.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");

            if (texture != null)
            {
                string texturePath = AssetDatabase.GetAssetPath(texture);
                string normalPath = texturePath.Contains("BaseColor")
                    ? texturePath.Replace("BaseColor", "Normal")
                    : System.IO.Path.ChangeExtension(texturePath, null) + "_Normal.png";
                var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
                if (normal != null)
                {
                    material.SetTexture("_BumpMap", normal);
                    material.EnableKeyword("_NORMALMAP");
                }
            }

            if (CutoutMaterials.Contains(source.name))
            {
                material.SetFloat("_AlphaClip", 1f);
                material.SetFloat("_Cutoff", 0.35f);
                material.EnableKeyword("_ALPHATEST_ON");
                material.SetFloat("_Cull", 0f);
                material.SetFloat("_SpecularHighlights", 0f);
                material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.renderQueue = (int)RenderQueue.AlphaTest;
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static Material Textured(string name, string texturePath, string normalPath, Color tint, float smoothness, Vector2 tiling)
        {
            Material material = Lit(name, tint, smoothness);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            material.SetTexture("_BaseMap", texture);
            material.SetTextureScale("_BaseMap", tiling);
            if (normal != null)
            {
                material.SetTexture("_BumpMap", normal);
                material.SetFloat("_BumpScale", 0.8f);
                material.EnableKeyword("_NORMALMAP");
            }
            material.SetFloat("_EnvironmentReflections", 0f);
            material.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material CreateSky()
        {
            var cubemap = AssetDatabase.LoadAssetAtPath<Cubemap>(SkyTexturePath);
            if (cubemap == null)
            {
                Debug.LogWarning("[FartRafters] Sky HDRI missing, using procedural sky");
                Material procedural = LoadOrCreate(Root + "/Materials/Sky.mat", () => new Material(Shader.Find("Skybox/Procedural")));
                procedural.shader = Shader.Find("Skybox/Procedural");
                EditorUtility.SetDirty(procedural);
                return procedural;
            }

            Shader shader = Shader.Find("Skybox/Cubemap");
            Material material = LoadOrCreate(Root + "/Materials/Sky.mat", () => new Material(shader));
            material.shader = shader;
            material.SetTexture("_Tex", cubemap);
            material.SetFloat("_Exposure", 1.05f);
            material.SetFloat("_Rotation", 40f);
            material.SetColor("_Tint", new Color(0.52f, 0.53f, 0.56f));
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
