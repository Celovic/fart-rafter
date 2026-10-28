using System.Collections.Generic;
using System.Linq;
using FartRafters.EditorTools.Sculpt;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace FartRafters.EditorTools
{
    public static partial class SceneBuilder
    {
        public static void RebuildNewBiomes()
        {
            CourseTheme river = AssetDatabase.LoadAssetAtPath<CourseTheme>(Root + "/Data/CourseTheme.asset");
            CourseTheme polar = AssetDatabase.LoadAssetAtPath<CourseTheme>(Root + "/Data/CourseTheme_Polar.asset");
            CourseTheme ocean = AssetDatabase.LoadAssetAtPath<CourseTheme>(Root + "/Data/CourseTheme_Ocean.asset");
            CourseTheme fun = AssetDatabase.LoadAssetAtPath<CourseTheme>(Root + "/Data/CourseTheme_Fun.asset");
            Mesh[] pickups = PickupMeshes(river);
            foreach (CourseTheme theme in new[] { river, polar, ocean, fun })
            {
                theme.pickupMeshes = pickups;
            }
            ApplyChase(river, Biome.River);
            ApplyChase(polar, Biome.Polar);
            ApplyChase(ocean, Biome.Ocean);
            ApplyChase(fun, Biome.Fun);
            CourseTheme[] themes = new[] { river, polar, ocean, fun }.Concat(CreateNewBiomeThemes(river)).ToArray();
            foreach (CourseTheme theme in themes)
            {
                EditorUtility.SetDirty(theme);
            }
            AssetDatabase.SaveAssets();
            AssignThemes(themes);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        static Mesh[] PickupMeshes(CourseTheme river)
        {
            var meshes = new List<Mesh>();
            for (int i = 0; i < 6 && river.pickupMeshes != null && i < river.pickupMeshes.Length; i++)
            {
                meshes.Add(river.pickupMeshes[i]);
            }
            meshes.Add(SaveMesh(SurfaceNets.Build(PropSculptor.RepairKit(), 0.012f, "PickupRepair"), "Props"));
            meshes.Add(SaveMesh(SurfaceNets.Build(PropSculptor.Bubble(), 0.012f, "PickupBubble"), "Props"));
            meshes.Add(SaveMesh(SurfaceNets.Build(PropSculptor.Magnet(), 0.012f, "PickupMagnet"), "Props"));
            return meshes.ToArray();
        }

        static void AssignThemes(CourseTheme[] themes)
        {
            var game = EditorSceneManager.OpenScene(Root + "/Scenes/Game.unity", OpenSceneMode.Single);
            foreach (GameBootstrap bootstrap in Object.FindObjectsByType<GameBootstrap>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                bootstrap.themes = themes;
                EditorUtility.SetDirty(bootstrap);
            }
            EditorSceneManager.SaveScene(game);
            var menu = EditorSceneManager.OpenScene(Root + "/Scenes/MainMenu.unity", OpenSceneMode.Single);
            foreach (MenuStage stage in Object.FindObjectsByType<MenuStage>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                stage.themes = themes;
                EditorUtility.SetDirty(stage);
            }
            EditorSceneManager.SaveScene(menu);
        }

        static CourseTheme[] CreateNewBiomeThemes(CourseTheme river)
        {
            CourseTheme lava = CopyTheme(river, "CourseTheme_Lava");
            ApplyLava(lava);
            CourseTheme night = CopyTheme(river, "CourseTheme_Night");
            ApplyNight(night);
            CourseTheme desert = CopyTheme(river, "CourseTheme_Desert");
            ApplyDesert(desert);
            CourseTheme swamp = CopyTheme(river, "CourseTheme_Swamp");
            ApplySwamp(swamp);
            foreach (CourseTheme theme in new[] { lava, night, desert, swamp })
            {
                ApplyChase(theme, theme.biome);
                EditorUtility.SetDirty(theme);
            }
            return new[] { lava, night, desert, swamp };
        }

        static void ApplyChase(CourseTheme theme, Biome biome)
        {
            theme.chaseCount = 1;
            theme.chaseHeight = 0f;
            switch (biome)
            {
                case Biome.Polar:
                    theme.chaseMesh = SaveMesh(SurfaceNets.Build(ChaseSculptor.PolarBear(), 0.05f, "ChasePolarBear"), "Chase");
                    theme.chaseName = "Kutup Ayısı";
                    theme.chaseSize = 7f;
                    break;
                case Biome.Ocean:
                    theme.chaseMesh = SaveMesh(SurfaceNets.Build(ChaseSculptor.GiantOctopus(), 0.05f, "ChaseOctopus"), "Chase");
                    theme.chaseName = "Dev Ahtapot";
                    theme.chaseSize = 8f;
                    break;
                case Biome.Fun:
                    theme.chaseMesh = SaveMesh(SurfaceNets.Build(ChaseSculptor.ToyDragon(), 0.05f, "ChaseToyDragon"), "Chase");
                    theme.chaseName = "Oyuncak Ejderha";
                    theme.chaseSize = 8f;
                    break;
                case Biome.Lava:
                    theme.chaseMesh = SaveMesh(SurfaceNets.Build(ChaseSculptor.Salamander(), 0.05f, "ChaseSalamander"), "Chase");
                    theme.chaseName = "Dev Ateş Semenderi";
                    theme.chaseSize = 9f;
                    break;
                case Biome.Night:
                    theme.chaseMesh = SaveMesh(SurfaceNets.Build(ChaseSculptor.Wolf(), 0.04f, "ChaseWolf"), "Chase");
                    theme.chaseName = "Kurt Sürüsü";
                    theme.chaseSize = 3.6f;
                    theme.chaseCount = 3;
                    break;
                case Biome.Desert:
                    theme.chaseMesh = SaveMesh(SurfaceNets.Build(ChaseSculptor.Scorpion(), 0.05f, "ChaseScorpion"), "Chase");
                    theme.chaseName = "Dev Akrep";
                    theme.chaseSize = 8f;
                    break;
                case Biome.Swamp:
                    theme.chaseMesh = theme.frogMesh;
                    theme.chaseName = "Dev Kurbağa";
                    theme.chaseSize = 5f;
                    break;
                default:
                    theme.chaseMesh = theme.crocodileMesh;
                    theme.chaseName = "Dev Timsah";
                    theme.chaseSize = 8f;
                    theme.chaseHeight = -0.1f;
                    break;
            }
        }

        static void ResetNewBiome(CourseTheme theme)
        {
            theme.dropName = "Buz sarkıtı";
            theme.raftLight = false;
            theme.ambience = null;
            theme.extraCheers = new string[0];
            theme.extraJeers = new string[0];
        }

        static void ApplyLava(CourseTheme theme)
        {
            ResetNewBiome(theme);
            theme.biome = Biome.Lava;
            theme.predatorName = "Ateş Semenderi";
            theme.predatorLength = ChaseSculptor.SalamanderLength;
            theme.caughtMessage = "Ateş semenderine yakalandı!";
            theme.extraCheers = new[] { "ATEŞLİ\nSAL!", "YAN\nGİTSİN!", "LAV GİBİ\nHIZLI!" };
            theme.extraJeers = new[] { "PİŞTİNİZ!", "SEMENDER\nACIKTI!", "KÜL\nOLDUNUZ" };
            theme.skybox = SkyVariant("Sky_Lava", new Color(0.62f, 0.32f, 0.22f), 0.75f, 200f);
            theme.sunColor = new Color(1f, 0.62f, 0.38f);
            theme.sunIntensity = 1.35f;
            theme.sunEuler = new Vector3(28f, -50f, 0f);
            theme.fogColor = new Color(0.42f, 0.22f, 0.16f);
            theme.fogDensity = 0.012f;
            theme.ambientTint = new Color(1.08f, 0.95f, 0.88f);
            theme.postFx = PostFxVariant("PostFx_Lava", 14f, 18f, -0.05f);
            theme.waveAmplitude = 0.08f;
            theme.waveLength = 9f;
            theme.waveSpeed = 0.7f;
            theme.water = WaterVariant("Water_Lava", new Color(1f, 0.5f, 0.06f), new Color(0.42f, 0.05f, 0.01f), new Color(1f, 0.35f, 0.05f), 0.6f, 0.6f, 0.15f, 0.08f, 0.5f);
            theme.water.SetColor("_FoamColor", new Color(1f, 0.85f, 0.3f));
            theme.water.SetFloat("_FlowSpeed", 0.25f);
            theme.whirlpoolMaterial = LavaWhirlpoolMaterial();

            Material ground = GroundVariant("Ground_Lava",
                PolyHavenRoot + "/brown_mud_rocks_01", PolyHavenRoot + "/river_small_rocks", PolyHavenRoot + "/brown_mud_rocks_01", PolyHavenRoot + "/river_small_rocks",
                new Color(0.3f, 0.26f, 0.24f), new Color(0.24f, 0.2f, 0.19f), new Color(0.2f, 0.15f, 0.13f), new Color(0.28f, 0.24f, 0.22f), "_diff_1k.jpg");
            theme.grass = ground;
            theme.bank = ground;

            theme.rockMaterial = theme.sculptMaterial;
            theme.rockMeshes = Meshes(3, i => LavaDesertSculptor.Basalt(i), 0.12f, "Basalt", "Lava");
            theme.thornMaterial = theme.sculptMaterial;
            theme.brambleMesh = SaveMesh(SurfaceNets.Build(LavaDesertSculptor.BasaltSpikes(), 0.035f, "BasaltSpikes"), "Lava");
            theme.crocodileMesh = SaveMesh(SurfaceNets.Build(ChaseSculptor.Salamander(), 0.05f, "Salamander"), "Lava");
            theme.driftMesh = SaveMesh(SurfaceNets.Build(LavaDesertSculptor.Pumice(), 0.06f, "Pumice"), "Lava");
            theme.driftLength = LavaDesertSculptor.PumiceLength;
            theme.driftName = "Sünger taşı";
            theme.icicleMesh = SaveMesh(SurfaceNets.Build(LavaDesertSculptor.LavaBomb(), 0.05f, "LavaBomb"), "Lava");
            theme.dropName = "Lav topu";
            theme.rampColor = new Color(0.35f, 0.12f, 0.08f);
            theme.rampStripe = new Color(1f, 0.55f, 0.1f);

            theme.treeMeshes = Meshes(3, i => LavaDesertSculptor.CharredTree(i), 0.2f, "CharredTree", "Lava");
            theme.pineMeshes = theme.treeMeshes;
            theme.bushMeshes = Meshes(3, i => LavaDesertSculptor.EmberRock(i), 0.12f, "EmberRock", "Lava");
            theme.grassModels = new GameObject[0];
            theme.reedModels = new GameObject[0];
            theme.flowerModels = new GameObject[0];
            theme.plantModels = new GameObject[0];
            theme.pebbleModels = new GameObject[0];
            theme.shoreProps = new[]
            {
                SaveMesh(SurfaceNets.Build(LavaDesertSculptor.MiniVolcano(), 0.06f, "MiniVolcano"), "Lava"),
                SaveMesh(SurfaceNets.Build(LavaDesertSculptor.Obsidian(), 0.03f, "Obsidian"), "Lava"),
                SaveMesh(SurfaceNets.Build(LavaDesertSculptor.SmokingVent(), 0.04f, "SmokingVent"), "Lava")
            };
            theme.shorePropChance = 0.14f;
        }

        static void ApplyNight(CourseTheme theme)
        {
            ResetNewBiome(theme);
            theme.biome = Biome.Night;
            theme.raftLight = true;
            theme.predatorName = "Yayın Balığı";
            theme.predatorLength = ChaseSculptor.CatfishLength;
            theme.caughtMessage = "Dev yayın balığına yakalandı!";
            theme.extraCheers = new[] { "GECE\nKUŞU!", "FENER\nSİZDE!", "KARANLIKTA\nPARLA!" };
            theme.extraJeers = new[] { "KAYBOLDUNUZ!", "KURTLAR\nGELİYOR!", "UYKU\nVAKTİ!" };
            theme.skybox = SkyVariant("Sky_Night", new Color(0.16f, 0.2f, 0.36f), 0.35f, 300f);
            theme.sunColor = new Color(0.62f, 0.72f, 1f);
            theme.sunIntensity = 0.55f;
            theme.sunEuler = new Vector3(35f, 30f, 0f);
            theme.fogColor = new Color(0.07f, 0.09f, 0.17f);
            theme.fogDensity = 0.013f;
            theme.ambientTint = new Color(0.7f, 0.75f, 0.95f);
            theme.postFx = PostFxVariant("PostFx_Night", -22f, -18f, -0.9f);
            Material ground = GroundVariant("Ground_Night",
                PolyHavenRoot + "/leafy_grass", PolyHavenRoot + "/forest_ground_04", PolyHavenRoot + "/brown_mud_rocks_01", PolyHavenRoot + "/river_small_rocks",
                new Color(0.22f, 0.3f, 0.32f), new Color(0.2f, 0.22f, 0.26f), new Color(0.16f, 0.17f, 0.2f), new Color(0.24f, 0.26f, 0.3f), "_diff_1k.jpg");
            theme.grass = ground;
            theme.bank = ground;
            theme.waveAmplitude = 0.1f;
            theme.water = WaterVariant("Water_Night", new Color(0.12f, 0.22f, 0.32f), new Color(0.01f, 0.03f, 0.08f), new Color(0.08f, 0.16f, 0.28f), 0.5f, 1.2f, 0.9f, 0.08f, 0.45f);
            theme.crocodileMesh = SaveMesh(SurfaceNets.Build(ChaseSculptor.Catfish(), 0.05f, "Catfish"), "Night");
            theme.rampColor = new Color(0.3f, 0.32f, 0.5f);
            theme.rampStripe = new Color(1f, 0.85f, 0.4f);
            theme.shoreProps = new[]
            {
                SaveMesh(SurfaceNets.Build(NightSwampSculptor.LanternPost(), 0.03f, "LanternPost"), "Night"),
                SaveMesh(SurfaceNets.Build(NightSwampSculptor.Campfire(), 0.03f, "Campfire"), "Night"),
                SaveMesh(SurfaceNets.Build(NightSwampSculptor.GlowMushrooms(), 0.025f, "GlowMushrooms"), "Night"),
                SaveMesh(SurfaceNets.Build(NightSwampSculptor.OwlStump(), 0.03f, "OwlStump"), "Night"),
                SaveMesh(SurfaceNets.Build(NightSwampSculptor.Tent(), 0.05f, "Tent"), "Night")
            };
            theme.shorePropChance = 0.16f;
        }

        static void ApplyDesert(CourseTheme theme)
        {
            ResetNewBiome(theme);
            theme.biome = Biome.Desert;
            theme.extraCheers = new[] { "KUM GİBİ\nKAY!", "SICAK\nSICAK!", "VAHAYA\nKOŞ!" };
            theme.extraJeers = new[] { "SUSADINIZ!", "AKREP\nGELİYOR!", "KUMA\nGÖMÜLDÜNÜZ" };
            theme.skybox = SkyVariant("Sky_Desert", new Color(0.7f, 0.6f, 0.48f), 1.35f, 60f);
            theme.sunColor = new Color(1f, 0.88f, 0.7f);
            theme.sunIntensity = 1.95f;
            theme.sunEuler = new Vector3(58f, -20f, 0f);
            theme.fogColor = new Color(0.9f, 0.8f, 0.64f);
            theme.fogDensity = 0.007f;
            theme.ambientTint = new Color(1.06f, 1f, 0.9f);
            theme.postFx = PostFxVariant("PostFx_Desert", 16f, 14f, 0.15f);
            theme.water = WaterVariant("Water_Desert", new Color(0.32f, 0.66f, 0.6f), new Color(0.06f, 0.24f, 0.26f), new Color(0.25f, 0.55f, 0.5f), 0.7f, 1.4f, 0.8f, 0.07f, 0.5f);

            Material ground = GroundVariant("Ground_Desert",
                PolyHavenRoot + "/sand_01", PolyHavenRoot + "/coast_sand_01", PolyHavenRoot + "/coast_sand_02", PolyHavenRoot + "/coast_sand_rocks_02",
                new Color(1f, 0.8f, 0.6f), new Color(0.98f, 0.78f, 0.58f), new Color(0.82f, 0.6f, 0.44f), new Color(0.92f, 0.72f, 0.56f), "_diff_1k.jpg");
            theme.grass = ground;
            theme.bank = ground;

            theme.rockMaterial = theme.sculptMaterial;
            theme.rockMeshes = Meshes(3, i => LavaDesertSculptor.Sandstone(i), 0.12f, "Sandstone", "Desert");
            theme.thornMaterial = theme.sculptMaterial;
            theme.brambleMesh = SaveMesh(SurfaceNets.Build(LavaDesertSculptor.CactusStrip(), 0.04f, "CactusStrip"), "Desert");
            theme.icicleMesh = SaveMesh(SurfaceNets.Build(LavaDesertSculptor.Boulder(), 0.05f, "Boulder"), "Desert");
            theme.dropName = "Kaya düştü";
            theme.rampColor = new Color(0.82f, 0.5f, 0.3f);
            theme.rampStripe = new Color(0.98f, 0.9f, 0.7f);

            theme.treeMeshes = Meshes(3, i => LavaDesertSculptor.Cactus(i), 0.08f, "Cactus", "Desert");
            theme.pineMeshes = theme.treeMeshes;
            theme.bushMeshes = Meshes(3, i => LavaDesertSculptor.Tumbleweed(i), 0.04f, "Tumbleweed", "Desert");
            theme.grassModels = Models(NatureRoot, "Grass_Wispy_Short");
            theme.reedModels = new GameObject[0];
            theme.flowerModels = new GameObject[0];
            theme.plantModels = new GameObject[0];
            theme.pebbleModels = new GameObject[0];
            theme.shoreProps = new[]
            {
                SaveMesh(SurfaceNets.Build(LavaDesertSculptor.CowSkull(), 0.02f, "CowSkull"), "Desert"),
                SaveMesh(SurfaceNets.Build(LavaDesertSculptor.RockArch(), 0.08f, "RockArch"), "Desert"),
                SaveMesh(SurfaceNets.Build(LavaDesertSculptor.Pyramid(), 0.08f, "Pyramid"), "Desert"),
                SaveMesh(SurfaceNets.Build(BiomeSculptor.Palm(1), 0.08f, "Palm1"), "Ocean")
            };
            theme.shorePropChance = 0.14f;
        }

        static void ApplySwamp(CourseTheme theme)
        {
            ResetNewBiome(theme);
            theme.biome = Biome.Swamp;
            theme.predatorName = "Timsah";
            theme.caughtMessage = "Bataklık timsahına yakalandı!";
            theme.extraCheers = new[] { "VIRAK\nVIRAK!", "ÇAMURA\nBATMA!", "SAZLIK\nKRALI" };
            theme.extraJeers = new[] { "BATAKLIKTA\nKALDINIZ!", "KURBAĞA\nGÜLÜYOR!", "YOSUN\nTUTTUNUZ" };
            theme.skybox = SkyVariant("Sky_Swamp", new Color(0.46f, 0.52f, 0.42f), 0.9f, 250f);
            theme.sunColor = new Color(0.92f, 0.95f, 0.78f);
            theme.sunIntensity = 1.25f;
            theme.sunEuler = new Vector3(38f, -70f, 0f);
            theme.fogColor = new Color(0.52f, 0.6f, 0.48f);
            theme.fogDensity = 0.015f;
            theme.ambientTint = new Color(0.95f, 1.02f, 0.9f);
            theme.postFx = PostFxVariant("PostFx_Swamp", -4f, -6f, -0.05f);
            theme.waveAmplitude = 0.06f;
            theme.waveSpeed = 0.8f;
            theme.water = WaterVariant("Water_Swamp", new Color(0.36f, 0.44f, 0.2f), new Color(0.08f, 0.14f, 0.05f), new Color(0.3f, 0.4f, 0.16f), 0.4f, 0.7f, 0.6f, 0.06f, 0.35f);

            Material ground = GroundVariant("Ground_Swamp",
                PolyHavenRoot + "/forest_ground_04", PolyHavenRoot + "/brown_mud_rocks_01", PolyHavenRoot + "/brown_mud_rocks_01", PolyHavenRoot + "/mossy_rock",
                new Color(0.72f, 0.8f, 0.55f), new Color(0.6f, 0.58f, 0.45f), new Color(0.45f, 0.42f, 0.32f), new Color(0.6f, 0.7f, 0.5f), "_diff_1k.jpg");
            theme.grass = ground;
            theme.bank = ground;

            theme.rockMaterial = theme.sculptMaterial;
            theme.rockMeshes = Meshes(3, i => NightSwampSculptor.MossRock(i), 0.12f, "MossRock", "Swamp");
            theme.driftMesh = SaveMesh(SurfaceNets.Build(NightSwampSculptor.MossLog(), 0.035f, "MossLog"), "Swamp");
            theme.driftLength = NightSwampSculptor.MossLogLength;
            theme.driftName = "Yosunlu kütük";
            theme.rampColor = new Color(0.4f, 0.35f, 0.22f);
            theme.rampStripe = new Color(0.7f, 0.85f, 0.4f);

            theme.treeMeshes = Meshes(3, i => NightSwampSculptor.Willow(i), 0.2f, "Willow", "Swamp");
            theme.pineMeshes = theme.treeMeshes;
            theme.bushMeshes = Meshes(3, i => NightSwampSculptor.Cattails(i), 0.04f, "Cattails", "Swamp");
            theme.flowerModels = new GameObject[0];
            theme.shoreProps = new[]
            {
                SaveMesh(SurfaceNets.Build(NightSwampSculptor.StiltShack(), 0.06f, "StiltShack"), "Swamp"),
                SaveMesh(SurfaceNets.Build(NightSwampSculptor.MossyLog(), 0.04f, "MossyLog"), "Swamp"),
                SaveMesh(SurfaceNets.Build(NightSwampSculptor.GlowMushrooms(), 0.025f, "GlowMushrooms"), "Night"),
                SaveMesh(SurfaceNets.Build(NightSwampSculptor.LanternPost(), 0.03f, "LanternPost"), "Night")
            };
            theme.shorePropChance = 0.14f;
        }

        static Mesh[] Meshes(int count, System.Func<int, Sculpt.Sculpt> sculpt, float cell, string name, string folder)
        {
            var meshes = new Mesh[count];
            for (int i = 0; i < count; i++)
            {
                meshes[i] = SaveMesh(SurfaceNets.Build(sculpt(i), cell, name + i), folder);
            }
            return meshes;
        }

        static Material LavaWhirlpoolMaterial()
        {
            Shader shader = Shader.Find(WhirlpoolShaderName);
            Material material = LoadOrCreate(Root + "/Materials/WhirlpoolLava.mat", () => new Material(shader));
            material.shader = shader;
            material.SetTexture("_FoamTex", Baked.foam);
            material.SetColor("_DeepColor", new Color(0.3f, 0.03f, 0.0f));
            material.SetColor("_MidColor", new Color(0.95f, 0.35f, 0.04f));
            material.SetColor("_FoamColor", new Color(1f, 0.85f, 0.3f));
            material.SetFloat("_Twist", 6f);
            material.SetFloat("_Speed", 0.35f);
            material.renderQueue = (int)RenderQueue.Transparent - 40;
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
