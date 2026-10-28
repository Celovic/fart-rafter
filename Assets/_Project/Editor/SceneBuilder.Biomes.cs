using System.Collections.Generic;
using System.Linq;
using FartRafters.EditorTools.Sculpt;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FartRafters.EditorTools
{
    public static partial class SceneBuilder
    {
        const string WhirlpoolShaderName = "FartRafters/Whirlpool";

        public static void RebuildBiomes()
        {
            CourseTheme river = AssetDatabase.LoadAssetAtPath<CourseTheme>(Root + "/Data/CourseTheme.asset");
            CreateBiomeThemes(river);
            AssetDatabase.SaveAssets();
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        public static void RebuildPickups()
        {
            var extra = new[]
            {
                SaveMesh(SurfaceNets.Build(PropSculptor.RepairKit(), 0.012f, "PickupRepair"), "Props"),
                SaveMesh(SurfaceNets.Build(PropSculptor.Bubble(), 0.012f, "PickupBubble"), "Props"),
                SaveMesh(SurfaceNets.Build(PropSculptor.Magnet(), 0.012f, "PickupMagnet"), "Props")
            };
            foreach (string guid in AssetDatabase.FindAssets("t:CourseTheme"))
            {
                CourseTheme theme = AssetDatabase.LoadAssetAtPath<CourseTheme>(AssetDatabase.GUIDToAssetPath(guid));
                if (theme == null || theme.pickupMeshes == null || theme.pickupMeshes.Length < 6)
                {
                    continue;
                }
                var meshes = new Mesh[6 + extra.Length];
                System.Array.Copy(theme.pickupMeshes, meshes, 6);
                System.Array.Copy(extra, 0, meshes, 6, extra.Length);
                theme.pickupMeshes = meshes;
                EditorUtility.SetDirty(theme);
            }
            AssetDatabase.SaveAssets();
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        static CourseTheme[] CreateBiomeThemes(CourseTheme river)
        {
            ResetBiome(river);
            river.biome = Biome.River;
            EditorUtility.SetDirty(river);

            CourseTheme polar = CopyTheme(river, "CourseTheme_Polar");
            ApplyPolar(polar);
            EditorUtility.SetDirty(polar);

            CourseTheme ocean = CopyTheme(river, "CourseTheme_Ocean");
            ApplyOcean(ocean);
            EditorUtility.SetDirty(ocean);

            CourseTheme fun = CopyTheme(river, "CourseTheme_Fun");
            ApplyFun(fun);
            EditorUtility.SetDirty(fun);
            ApplyChase(river, Biome.River);
            ApplyChase(polar, Biome.Polar);
            ApplyChase(ocean, Biome.Ocean);
            ApplyChase(fun, Biome.Fun);
            CourseTheme[] added = CreateNewBiomeThemes(river);
            AssetDatabase.SaveAssets();
            return new[] { river, polar, ocean, fun }.Concat(added).ToArray();
        }

        static void ResetBiome(CourseTheme theme)
        {
            theme.predatorName = "Timsah";
            theme.predatorLength = 0f;
            theme.caughtMessage = "Timsaha yakalandı!";
            theme.extraCheers = new string[0];
            theme.extraJeers = new string[0];
            theme.skybox = null;
            theme.sunColor = new Color(1f, 0.93f, 0.82f);
            theme.sunIntensity = 1.6f;
            theme.sunEuler = new Vector3(42f, -40f, 0f);
            theme.fogColor = new Color(0.68f, 0.78f, 0.86f);
            theme.fogDensity = 0.0085f;
            theme.ambientTint = Color.white;
            theme.postFx = null;
            theme.waveAmplitude = 0.15f;
            theme.waveLength = 8f;
            theme.waveSpeed = 1.2f;
            theme.snow = false;
            theme.snowMaterial = null;
            theme.openSide = 0;
            theme.iceMaterial = null;
            theme.whirlpoolMaterial = null;
            theme.rockMaterial = null;
            theme.rockMeshes = new Mesh[0];
            theme.thornMaterial = null;
            theme.millMaterial = null;
            theme.shoreProps = new Mesh[0];
            theme.shorePropChance = 0.12f;
            theme.jumperMesh = null;
            theme.crowdAnimal = new AudioClip[0];
            theme.ambience = null;
            theme.ambienceVolume = 0.35f;
            theme.animalShare = 0.25f;
            theme.landHeight = 0f;
            theme.bankSpread = 1f;
            theme.elevated = false;
            theme.rockPhysics = null;
            theme.mesaMeshes = new Mesh[0];
            theme.pillarMesh = null;
            theme.gatePoleMesh = null;
            theme.spinnerMesh = null;
            theme.cloudMaterial = null;
            theme.gateLabel = "";
            theme.startLabel = "";
            theme.predatorLunge = 18f;
            theme.predatorSpin = 0f;
            theme.driftMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.Log(), 0.035f, "DriftLog"), "Props");
            theme.driftLength = BiomeSculptor.LogLength;
            theme.driftName = "Tomruk";
            theme.eelMesh = null;
            theme.eelLength = BiomeSculptor.EelLength;
            theme.barrelMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.Barrel(), 0.022f, "Barrel"), "Props");
            theme.icicleMesh = null;
            theme.rampColor = new Color(0.62f, 0.42f, 0.24f);
            theme.rampStripe = new Color(0.9f, 0.8f, 0.55f);
            theme.dropName = "Buz sarkıtı";
            theme.raftLight = false;
        }

        static void ApplyFun(CourseTheme theme)
        {
            theme.biome = Biome.Fun;
            theme.elevated = true;
            theme.predatorName = "Dikenli Silindir";
            theme.predatorLength = BiomeSculptor.RollerLength;
            theme.predatorLunge = 0f;
            theme.predatorSpin = 240f;
            theme.caughtMessage = "Dikenli silindire çarptın!";
            theme.skybox = SkyVariant("Sky_Fun", new Color(0.55f, 0.6f, 0.68f), 1.3f, 20f);
            theme.sunColor = new Color(1f, 0.95f, 0.85f);
            theme.sunIntensity = 1.8f;
            theme.sunEuler = new Vector3(50f, -35f, 0f);
            theme.fogColor = new Color(0.84f, 0.91f, 0.99f);
            theme.fogDensity = 0.0045f;
            theme.ambientTint = new Color(1.04f, 1.02f, 1.02f);
            theme.postFx = PostFxVariant("PostFx_Fun", 2f, 28f, 0.22f);
            theme.waveAmplitude = 0.07f;
            theme.waveLength = 6f;
            theme.waveSpeed = 1.6f;
            theme.water = WaterVariant("Water_Fun", new Color(0.25f, 0.72f, 0.97f), new Color(0.06f, 0.38f, 0.85f), new Color(0.2f, 0.6f, 0.9f), 0.8f, 1.8f, 0.7f, 0.11f, 0.6f);
            theme.water.SetFloat("_FlowSpeed", 1.1f);
            theme.bank = Lit("SlideFloor", new Color(0.55f, 0.86f, 0.96f), 0.5f);
            theme.grass = theme.bank;

            PhysicsMaterial bouncy = LoadOrCreate(Root + "/Physics/Bouncy.asset", () => new PhysicsMaterial("Bouncy"));
            bouncy.dynamicFriction = 0.05f;
            bouncy.staticFriction = 0.05f;
            bouncy.bounciness = 0.85f;
            bouncy.frictionCombine = PhysicsMaterialCombine.Minimum;
            bouncy.bounceCombine = PhysicsMaterialCombine.Maximum;
            EditorUtility.SetDirty(bouncy);
            theme.rockPhysics = bouncy;
            theme.rockMaterial = theme.sculptMaterial;
            theme.rockMeshes = new[]
            {
                SaveMesh(SurfaceNets.Build(BiomeSculptor.Bumper(0), 0.08f, "Bumper0"), "Fun"),
                SaveMesh(SurfaceNets.Build(BiomeSculptor.Bumper(1), 0.08f, "Bumper1"), "Fun")
            };
            theme.crocodileMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.SpikeRoller(), 0.06f, "SpikeRoller"), "Fun");
            theme.driftMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.InflatableTube(), 0.055f, "InflatableTube"), "Fun");
            theme.driftName = "Şişme silindir";
            theme.rampColor = new Color(0.95f, 0.35f, 0.62f);
            theme.rampStripe = new Color(0.98f, 0.85f, 0.25f);
            theme.spinnerMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.SpinnerArm(), 0.06f, "SpinnerArm"), "Fun");
            theme.brambleMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.SpikeStrip(), 0.04f, "SpikeStrip"), "Fun");
            theme.thornMaterial = theme.sculptMaterial;
            theme.gatePoleMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.BambooPole(), 0.06f, "BambooPole"), "Fun");
            theme.pillarMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.SlidePillar(), 0.4f, "SlidePillar"), "Fun");
            theme.mesaMeshes = new Mesh[3];
            for (int i = 0; i < theme.mesaMeshes.Length; i++)
            {
                theme.mesaMeshes[i] = SaveMesh(SurfaceNets.Build(BiomeSculptor.Mesa(i), 0.9f, "Mesa" + i), "Fun");
            }
            theme.cloudMaterial = CloudMaterial();
            theme.gateLabel = "KONTROL NOKTASI";
            theme.startLabel = "BAŞLA";
            theme.spectators = new SpectatorLook[0];
            theme.crowdAnimal = new AudioClip[0];
            theme.shoreProps = new Mesh[0];
            theme.ambience = null;
            theme.extraCheers = new string[0];
            theme.extraJeers = new string[0];
        }

        static Material CloudMaterial()
        {
            Shader shader = Shader.Find("FartRafters/Clouds");
            Material material = LoadOrCreate(Root + "/Materials/Clouds.mat", () => new Material(shader));
            material.shader = shader;
            material.SetTexture("_NoiseTex", Baked.macro);
            material.SetColor("_Color", new Color(1f, 1f, 1f));
            material.SetColor("_Shade", new Color(0.74f, 0.83f, 0.96f));
            material.SetFloat("_Scale", 0.004f);
            material.SetFloat("_Speed", 0.004f);
            material.renderQueue = (int)RenderQueue.Transparent - 60;
            EditorUtility.SetDirty(material);
            return material;
        }

        static CourseTheme CopyTheme(CourseTheme source, string file)
        {
            string path = Root + "/Data/" + file + ".asset";
            CourseTheme theme = LoadOrCreate(path, ScriptableObject.CreateInstance<CourseTheme>);
            EditorUtility.CopySerialized(source, theme);
            theme.name = file;
            return theme;
        }

        static void ApplyPolar(CourseTheme theme)
        {
            theme.biome = Biome.Polar;
            theme.predatorName = "Orka";
            theme.predatorLength = BiomeSculptor.OrcaLength;
            theme.caughtMessage = "Orkaya yakalandı!";
            theme.extraCheers = new[] { "BUZ GİBİ\nSAL!", "KAYDIR\nGİTSİN!", "PENGUENLER\nSİZİNLE", "SICAK\nFASULYE!" };
            theme.extraJeers = new[] { "DONDUNUZ!", "ORKA\nACIKTI!", "BUZDA\nKALDINIZ" };
            theme.skybox = SkyVariant("Sky_Polar", new Color(0.62f, 0.66f, 0.74f), 1.2f, 80f);
            theme.sunColor = new Color(0.94f, 0.97f, 1f);
            theme.sunIntensity = 1.45f;
            theme.sunEuler = new Vector3(30f, -60f, 0f);
            theme.fogColor = new Color(0.8f, 0.87f, 0.94f);
            theme.fogDensity = 0.011f;
            theme.ambientTint = new Color(0.98f, 1.02f, 1.12f);
            theme.postFx = PostFxVariant("PostFx_Polar", -9f, 6f, 0.2f);
            theme.waveAmplitude = 0.05f;
            theme.waveLength = 7f;
            theme.waveSpeed = 0.9f;
            theme.snow = true;
            theme.snowMaterial = Particle("Snow", CreateTexture("Puff", 64, SoftCircle));
            theme.water = WaterVariant("Water_Polar", new Color(0.45f, 0.62f, 0.68f), new Color(0.06f, 0.2f, 0.3f), new Color(0.3f, 0.55f, 0.65f), 0.5f, 1.5f, 0.85f, 0.07f, 0.4f);

            Material ground = GroundVariant("Ground_Snow",
                PolyHavenRoot + "/snow_02", PolyHavenRoot + "/snow_field_aerial", PolyHavenRoot + "/snow_03", PolyHavenRoot + "/river_small_rocks",
                new Color(0.97f, 0.99f, 1.04f), new Color(0.92f, 0.95f, 1f), new Color(0.88f, 0.92f, 0.98f), new Color(0.88f, 0.9f, 0.96f), "_col_1k.jpg");
            ground.SetFloat("_GrassScale", 0.45f);
            theme.grass = ground;
            theme.bank = ground;

            theme.iceMaterial = IceMaterial();
            theme.rockMaterial = theme.iceMaterial;
            theme.rockMeshes = new Mesh[3];
            for (int i = 0; i < theme.rockMeshes.Length; i++)
            {
                theme.rockMeshes[i] = SaveMesh(SurfaceNets.Build(BiomeSculptor.IceChunk(i), 0.12f, "IceChunk" + i), "Polar");
            }
            theme.thornMaterial = theme.iceMaterial;
            theme.brambleMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.Icicles(), 0.035f, "Icicles"), "Polar");
            theme.millWheelMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.FrozenWheel(), 0.07f, "FrozenWheel"), "Polar");
            theme.millMaterial = theme.sculptMaterial;
            theme.crocodileMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.Orca(), 0.05f, "Orca"), "Polar");
            theme.driftMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.IceFloe(), 0.06f, "IceFloe"), "Polar");
            theme.driftName = "Buz kütlesi";
            theme.icicleMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.IcicleSpike(), 0.05f, "IcicleSpike"), "Polar");
            theme.rampColor = new Color(0.72f, 0.86f, 0.96f);
            theme.rampStripe = new Color(0.95f, 0.97f, 1f);

            theme.treeMeshes = new Mesh[3];
            for (int i = 0; i < theme.treeMeshes.Length; i++)
            {
                theme.treeMeshes[i] = SaveMesh(SurfaceNets.Build(BiomeSculptor.SnowPine(i), 0.2f, "SnowPine" + i), "Polar");
            }
            theme.pineMeshes = theme.treeMeshes;
            theme.bushMeshes = new Mesh[3];
            for (int i = 0; i < theme.bushMeshes.Length; i++)
            {
                theme.bushMeshes[i] = SaveMesh(SurfaceNets.Build(BiomeSculptor.SnowMound(i), 0.12f, "SnowMound" + i), "Polar");
            }
            theme.grassModels = new GameObject[0];
            theme.reedModels = new GameObject[0];
            theme.flowerModels = new GameObject[0];
            theme.plantModels = new GameObject[0];
            theme.pebbleModels = new GameObject[0];
            theme.shoreProps = new[]
            {
                SaveMesh(SurfaceNets.Build(BiomeSculptor.Igloo(), 0.06f, "Igloo"), "Polar"),
                SaveMesh(SurfaceNets.Build(BiomeSculptor.Snowman(), 0.04f, "Snowman"), "Polar"),
                SaveMesh(SurfaceNets.Build(BiomeSculptor.Sled(), 0.025f, "Sled"), "Polar"),
                SaveMesh(SurfaceNets.Build(BiomeSculptor.IceCrystal(), 0.03f, "IceCrystal"), "Polar")
            };
            theme.shorePropChance = 0.1f;

            var looks = new List<SpectatorLook>();
            for (int i = 0; i < BiomeCrowdSculptor.WinterPeople.Length; i++)
            {
                looks.Add(BakeSpectator("Winter" + i, BiomeCrowdSculptor.Winter(BiomeCrowdSculptor.WinterPeople[i]), false));
            }
            looks.Add(BakeSpectator("Penguin", BiomeCrowdSculptor.Penguin(), true));
            looks.Add(BakeSpectator("Seal", BiomeCrowdSculptor.Seal(new Color(0.55f, 0.58f, 0.62f), new Color(0.82f, 0.84f, 0.86f)), true));
            looks.Add(BakeSpectator("ArcticFox", BiomeCrowdSculptor.FurBall(new Color(0.96f, 0.96f, 0.97f), new Color(0.86f, 0.88f, 0.92f), Ink(), true), true));
            looks.Add(BakeSpectator("PolarCub", BiomeCrowdSculptor.FurBall(new Color(0.97f, 0.95f, 0.88f), new Color(1f, 0.98f, 0.94f), Ink(), false), true));
            theme.spectators = looks.ToArray();
            theme.animalShare = 0.45f;

            string crowd = Root + "/Audio/Crowd";
            theme.crowdAnimal = LoadClips(crowd, "penguin_").Concat(LoadClips(crowd, "seal_")).ToArray();
            theme.ambience = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/Ambience/polar_wind.wav");
            theme.ambienceVolume = 0.12f;
            theme.openSide = 0;
            theme.jumperMesh = null;
        }

        static void ApplyOcean(CourseTheme theme)
        {
            theme.biome = Biome.Ocean;
            theme.predatorName = "Köpek Balığı";
            theme.predatorLength = BiomeSculptor.SharkLength;
            theme.caughtMessage = "Köpek balığına yakalandı!";
            theme.extraCheers = new[] { "DALGAYI\nYAK!", "TATİL\nGAZI!", "YÜZ\nGİTSİN!", "PLAJIN\nKRALI" };
            theme.extraJeers = new[] { "BATIYORSUNUZ!", "KÖPEK BALIĞI\nAÇ!", "KUMDA\nKALIN!" };
            theme.skybox = SkyVariant("Sky_Ocean", new Color(0.5f, 0.55f, 0.62f), 1.18f, 140f);
            theme.sunColor = new Color(1f, 0.95f, 0.86f);
            theme.sunIntensity = 1.75f;
            theme.sunEuler = new Vector3(55f, -30f, 0f);
            theme.fogColor = new Color(0.72f, 0.84f, 0.93f);
            theme.fogDensity = 0.006f;
            theme.ambientTint = new Color(1f, 1.02f, 1.05f);
            theme.postFx = PostFxVariant("PostFx_Ocean", 4f, 22f, 0.18f);
            theme.waveAmplitude = 0.26f;
            theme.waveLength = 13f;
            theme.waveSpeed = 1f;
            theme.openSide = 1;
            theme.landHeight = 0.85f;
            theme.bankSpread = 3.5f;
            theme.water = WaterVariant("Water_Ocean", new Color(0.28f, 0.7f, 0.72f), new Color(0.03f, 0.18f, 0.34f), new Color(0.15f, 0.6f, 0.62f), 0.85f, 0.8f, 1f, 0.06f, 0.55f);
            theme.whirlpoolMaterial = WhirlpoolMaterial();

            Material ground = GroundVariant("Ground_Sand",
                PolyHavenRoot + "/coast_sand_01", PolyHavenRoot + "/sand_01", PolyHavenRoot + "/coast_sand_02", PolyHavenRoot + "/coast_sand_rocks_02",
                new Color(1f, 0.97f, 0.9f), new Color(0.98f, 0.93f, 0.84f), new Color(0.85f, 0.78f, 0.68f), new Color(0.95f, 0.9f, 0.84f), "_diff_1k.jpg");
            theme.grass = ground;
            theme.bank = ground;

            theme.brambleMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.Reef(), 0.04f, "Reef"), "Ocean");
            theme.thornMaterial = theme.sculptMaterial;
            theme.crocodileMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.Shark(), 0.05f, "Shark"), "Ocean");
            theme.driftMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.Crate(), 0.05f, "Crate"), "Ocean");
            theme.driftName = "Sandık";
            theme.eelMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.Eel(), 0.045f, "Eel"), "Ocean");
            theme.rampColor = new Color(0.2f, 0.65f, 0.7f);
            theme.rampStripe = new Color(0.96f, 0.96f, 0.9f);
            theme.jumperMesh = SaveMesh(SurfaceNets.Build(BiomeSculptor.Dolphin(), 0.035f, "Dolphin"), "Ocean");

            theme.treeMeshes = new Mesh[3];
            for (int i = 0; i < theme.treeMeshes.Length; i++)
            {
                theme.treeMeshes[i] = SaveMesh(SurfaceNets.Build(BiomeSculptor.Palm(i), 0.08f, "Palm" + i), "Ocean");
            }
            theme.pineMeshes = theme.treeMeshes;
            theme.reedModels = new GameObject[0];
            theme.flowerModels = new GameObject[0];
            theme.plantModels = new GameObject[0];
            theme.grassModels = Models(NatureRoot, "Grass_Wispy_Short");
            theme.shoreProps = new[]
            {
                SaveMesh(SurfaceNets.Build(BiomeSculptor.Umbrella(0), 0.05f, "Umbrella0"), "Ocean"),
                SaveMesh(SurfaceNets.Build(BiomeSculptor.Umbrella(1), 0.05f, "Umbrella1"), "Ocean"),
                SaveMesh(SurfaceNets.Build(BiomeSculptor.LifeguardTower(), 0.06f, "LifeguardTower"), "Ocean"),
                SaveMesh(SurfaceNets.Build(BiomeSculptor.Starfish(), 0.012f, "Starfish"), "Ocean"),
                SaveMesh(SurfaceNets.Build(BiomeSculptor.SandCastle(), 0.03f, "SandCastle"), "Ocean")
            };
            theme.shorePropChance = 0.22f;

            var looks = new List<SpectatorLook>();
            for (int i = 0; i < BiomeCrowdSculptor.BeachPeople.Length; i++)
            {
                looks.Add(BakeSpectator("Beach" + i, BiomeCrowdSculptor.Beach(BiomeCrowdSculptor.BeachPeople[i]), false));
            }
            looks.Add(BakeSpectator("Crab", BiomeCrowdSculptor.Crab(), true));
            looks.Add(BakeSpectator("SeaLion", BiomeCrowdSculptor.Seal(new Color(0.52f, 0.38f, 0.26f), new Color(0.75f, 0.6f, 0.45f)), true));
            looks.Add(BakeSpectator("Gull", BiomeCrowdSculptor.Gull(), true));
            looks.Add(BakeSpectator("Octopus", BiomeCrowdSculptor.Octopus(), true));
            looks.Add(BakeSpectator("SeaTurtle", BiomeCrowdSculptor.Turtle(), true));
            theme.spectators = looks.ToArray();
            theme.animalShare = 0.4f;

            string crowd = Root + "/Audio/Crowd";
            theme.crowdAnimal = LoadClips(crowd, "gull_").Concat(LoadClips(crowd, "seal_")).ToArray();
            theme.ambience = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/Ambience/ocean_waves.wav");
            theme.ambienceVolume = 0.16f;
        }

        static RaftVariant[] CreateRafts(CourseTheme theme)
        {
            PhysicsMaterial bouncy = LoadOrCreate(Root + "/Physics/RaftBouncy.asset", () => new PhysicsMaterial("RaftBouncy"));
            bouncy.dynamicFriction = 0.1f;
            bouncy.staticFriction = 0.1f;
            bouncy.bounciness = 0.6f;
            bouncy.frictionCombine = PhysicsMaterialCombine.Minimum;
            bouncy.bounceCombine = PhysicsMaterialCombine.Maximum;
            EditorUtility.SetDirty(bouncy);
            Material wood = theme.raftMaterial != null ? theme.raftMaterial : theme.sculptMaterial;
            return new[]
            {
                new RaftVariant { name = "Klasik Sal", description = "Hafif ve çevik, çabuk hızlanır", mesh = theme.raftMesh, material = wood, props = true, durability = 1, acceleration = 5 },
                new RaftVariant { name = "Şişme Bot", description = "Çarpınca seker, az hasar alır", mesh = SaveMesh(SurfaceNets.Build(RaftSculptor.InflatableBoat(), 0.028f, "RaftInflatable"), "Props"), material = theme.sculptMaterial, props = false, thrust = 1f, drag = 1.05f, turnDrag = 0.85f, durability = 3, acceleration = 3, physics = bouncy },
                new RaftVariant { name = "Ahşap Kayık", description = "Hızlı süzülür ama zor döner", mesh = SaveMesh(SurfaceNets.Build(RaftSculptor.Rowboat(), 0.028f, "RaftRowboat"), "Props"), material = wood, props = false, thrust = 1.05f, drag = 0.85f, turnDrag = 1.35f, durability = 2, acceleration = 4 },
                new RaftVariant { name = "Dev Simit", description = "Fırıl fırıl döner, yumuşak çarpar", mesh = SaveMesh(SurfaceNets.Build(RaftSculptor.BigRing(), 0.028f, "RaftRing"), "Props"), material = theme.sculptMaterial, props = false, thrust = 0.95f, drag = 1f, turnDrag = 0.65f, durability = 4, acceleration = 2, physics = bouncy },
                new RaftVariant { name = "Küvet", description = "Ağır ve yavaş ama çok sağlam", mesh = SaveMesh(SurfaceNets.Build(RaftSculptor.Bathtub(), 0.028f, "RaftBathtub"), "Props"), material = theme.sculptMaterial, props = false, thrust = 0.9f, drag = 1.15f, turnDrag = 1.15f, durability = 5, acceleration = 1 }
            };
        }

        static Color Ink()
        {
            return new Color(0.08f, 0.06f, 0.07f);
        }

        static Material IceMaterial()
        {
            Material material = SculptMaterial("Ice", 0.82f, 0.45f, 0.1f, 0.3f);
            material.SetColor("_BaseColor", new Color(1f, 1.02f, 1.06f));
            material.SetFloat("_Translucency", 0.35f);
            material.SetFloat("_RimStrength", 0.3f);
            material.SetColor("_RimColor", new Color(0.85f, 0.95f, 1f));
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material WhirlpoolMaterial()
        {
            Shader shader = Shader.Find(WhirlpoolShaderName);
            Material material = LoadOrCreate(Root + "/Materials/Whirlpool.mat", () => new Material(shader));
            material.shader = shader;
            material.SetTexture("_FoamTex", Baked.foam);
            material.SetColor("_DeepColor", new Color(0.02f, 0.12f, 0.22f));
            material.SetColor("_MidColor", new Color(0.08f, 0.38f, 0.5f));
            material.SetColor("_FoamColor", new Color(0.92f, 0.97f, 0.98f));
            material.SetFloat("_Twist", 7f);
            material.SetFloat("_Speed", 0.5f);
            material.renderQueue = (int)RenderQueue.Transparent - 40;
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material WaterVariant(string name, Color shallow, Color deep, Color scatter, float shoreFoam, float clarity, float reflection, float normalScale, float normalStrength)
        {
            Shader shader = Shader.Find(WaterShaderName);
            Material material = LoadOrCreate(Root + "/Materials/" + name + ".mat", () => new Material(shader));
            material.shader = shader;
            material.SetColor("_ShallowColor", shallow);
            material.SetColor("_DeepColor", deep);
            material.SetColor("_ScatterColor", scatter);
            material.SetColor("_FoamColor", new Color(0.96f, 0.98f, 0.98f));
            material.SetTexture("_WaveNormal", Baked.waveNormal);
            material.SetTexture("_FoamTex", Baked.foam);
            material.SetFloat("_NormalScale", normalScale);
            material.SetFloat("_NormalStrength", normalStrength);
            material.SetFloat("_FlowSpeed", 0.45f);
            material.SetFloat("_DepthDistance", 2.6f);
            material.SetFloat("_ClarityDistance", clarity);
            material.SetFloat("_ShoreFoam", shoreFoam);
            material.SetFloat("_FoamDistance", 0.55f);
            material.SetFloat("_Smoothness", 0.9f);
            material.SetFloat("_ReflectionStrength", reflection);
            material.SetFloat("_SunStrength", 0.55f);
            material.renderQueue = (int)RenderQueue.Transparent - 50;
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material GroundVariant(string name, string grass, string dirt, string mud, string pebble, Color grassTint, Color dirtTint, Color mudTint, Color pebbleTint, string dirtSuffix)
        {
            Shader shader = Shader.Find(GroundShaderName);
            Material material = LoadOrCreate(Root + "/Materials/" + name + ".mat", () => new Material(shader));
            material.shader = shader;
            material.SetTexture("_GrassMap", Load(Diffuse(grass)));
            material.SetTexture("_GrassNormal", Load(grass + "_nor_gl_1k.jpg"));
            material.SetTexture("_DirtMap", Load(dirt + dirtSuffix));
            material.SetTexture("_DirtNormal", Load(dirt + "_nor_gl_1k.jpg"));
            material.SetTexture("_MudMap", Load(Diffuse(mud)));
            material.SetTexture("_MudNormal", Load(mud + "_nor_gl_1k.jpg"));
            material.SetTexture("_PebbleMap", Load(Diffuse(pebble)));
            material.SetTexture("_PebbleNormal", Load(pebble + "_nor_gl_1k.jpg"));
            material.SetTexture("_MacroMap", Baked.macro);
            material.SetColor("_GrassTint", grassTint);
            material.SetColor("_DirtTint", dirtTint);
            material.SetColor("_MudTint", mudTint);
            material.SetColor("_PebbleTint", pebbleTint);
            material.SetFloat("_GrassScale", 0.9f);
            material.SetFloat("_DirtScale", 0.7f);
            material.SetFloat("_MudScale", 0.9f);
            material.SetFloat("_PebbleScale", 1.2f);
            material.SetFloat("_MacroScale", 0.03f);
            material.SetFloat("_NormalStrength", 0.9f);
            material.SetFloat("_WetHeight", 0.3f);
            material.SetFloat("_DirtNoise", 0.4f);
            EditorUtility.SetDirty(material);
            return material;
        }

        static string Diffuse(string stem)
        {
            string diff = stem + "_diff_1k.jpg";
            return System.IO.File.Exists(diff) ? diff : stem + "_col_1k.jpg";
        }

        static Material SkyVariant(string name, Color tint, float exposure, float rotation)
        {
            var cubemap = AssetDatabase.LoadAssetAtPath<Cubemap>(SkyTexturePath);
            if (cubemap == null)
            {
                return null;
            }
            Shader shader = Shader.Find("Skybox/Cubemap");
            Material material = LoadOrCreate(Root + "/Materials/" + name + ".mat", () => new Material(shader));
            material.shader = shader;
            material.SetTexture("_Tex", cubemap);
            material.SetFloat("_Exposure", exposure);
            material.SetFloat("_Rotation", rotation);
            material.SetColor("_Tint", tint);
            EditorUtility.SetDirty(material);
            return material;
        }

        static VolumeProfile PostFxVariant(string name, float temperature, float saturation, float exposure)
        {
            string path = Root + "/Data/" + name + ".asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }
            ApplyPostFx(profile);
            profile.TryGet(out WhiteBalance balance);
            balance.temperature.Override(temperature);
            profile.TryGet(out ColorAdjustments color);
            color.saturation.Override(saturation);
            color.postExposure.Override(exposure);
            EditorUtility.SetDirty(profile);
            return profile;
        }
    }
}
