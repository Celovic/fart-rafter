using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FartRafters.EditorTools.Sculpt;
using Object = UnityEngine.Object;

namespace FartRafters.EditorTools
{
    public static partial class SceneBuilder
    {
        const string Root = "Assets/_Project";
        public const string MenuScenePath = Root + "/Scenes/MainMenu.unity";
        public const string GameScenePath = Root + "/Scenes/Game.unity";
        const string LegacyScenePath = Root + "/Scenes/Prototype.unity";
        const string WaterShaderName = "FartRafters/RiverWater";

        [MenuItem("FartRaft/Sahneleri Yeniden Kur")]
        static void BuildFromMenu()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "FartRaft",
                "MainMenu ve Game sahneleri baştan kurulacak. Sahnelerde elle yaptığın değişiklikler silinir. Ayar, avatar, bot ve seviye dosyaları korunur.",
                "Kur",
                "Vazgeç");
            if (confirmed)
            {
                Build();
            }
        }

        public static void Build()
        {
            EnsureFolder(Root + "/Scenes");
            EnsureFolder(Root + "/Data");
            EnsureFolder(Root + "/Data/Levels");
            EnsureFolder(Root + "/Materials");
            EnsureFolder(Root + "/Textures");
            EnsureFolder(Root + "/Physics");

            ConfigureRenderer();
            RaftTuning tuning = LoadOrCreate(Root + "/Data/RaftTuning.asset", ScriptableObject.CreateInstance<RaftTuning>);
            tuning.gasMultiplier = 1.1f;
            EditorUtility.SetDirty(tuning);
            PhysicsMaterial slippery = CreateSlippery();
            CourseTheme theme = CreateTheme(slippery);
            CourseTheme[] themes = CreateBiomeThemes(theme);
            AvatarDefinition[] avatars = CreateAvatars();
            BotDifficulty[] botLevels = CreateBotLevels();
            LevelDefinition[] levels = CreateLevels();
            GameCatalog catalog = CreateCatalog(levels, avatars, botLevels, CreateDifficulties(), CreateRafts(theme));
            Material sky = CreateSky();
            VolumeProfile postFx = CreatePostFx();

            BuildGameScene(catalog, theme, themes, tuning, slippery, sky, postFx);
            BuildMenuScene(catalog, theme, themes, sky, postFx);

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(LegacyScenePath) != null)
            {
                AssetDatabase.DeleteAsset(LegacyScenePath);
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene(GameScenePath, true)
            };
            AssetDatabase.SaveAssets();
            Debug.Log("[FartRafters] Scenes built: " + MenuScenePath + ", " + GameScenePath);
        }

        static void BuildGameScene(GameCatalog catalog, CourseTheme theme, CourseTheme[] themes, RaftTuning tuning, PhysicsMaterial slippery, Material sky, VolumeProfile postFx)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            ConfigureEnvironment(sky);
            AddPostFx(postFx);
            Camera camera = Object.FindAnyObjectByType<Camera>();

            var courseObject = new GameObject("Course");
            var course = courseObject.AddComponent<CourseManager>();

            Raft raft = BuildRaft(tuning, theme, slippery);
            course.raft = raft;

            Rider leftRider = BuildRider(raft, 0, new Vector3(-0.75f, 0.2f, -0.75f), catalog.GetAvatar(0), theme);
            Rider rightRider = BuildRider(raft, 1, new Vector3(0.75f, 0.2f, -0.75f), catalog.GetAvatar(1), theme);
            Rider botA = BuildRider(raft, 2, new Vector3(-0.75f, 0.2f, 0.75f), catalog.GetAvatar(2), theme);
            Rider botB = BuildRider(raft, 3, new Vector3(0.75f, 0.2f, 0.75f), catalog.GetAvatar(0), theme);
            raft.riders = new[] { leftRider, rightRider, botA, botB };
            raft.gameObject.AddComponent<RaftBoosts>();
            raft.gameObject.AddComponent<RaftHealth>().course = course;
            raft.gameObject.AddComponent<RaftFall>().course = course;
            raft.gameObject.AddComponent<RaftVoice>().course = course;

            Rider[] seats = raft.riders;
            var playerBrains = new PlayerBrain[seats.Length];
            var botBrains = new BotBrain[seats.Length];
            for (int i = 0; i < seats.Length; i++)
            {
                playerBrains[i] = AddPlayerBrain(seats[i], i, camera);
                botBrains[i] = AddBotBrain(seats[i], course);
                seats[i].brain = i < GameSelection.MinPlayers ? (RiderBrain)playerBrains[i] : botBrains[i];
            }

            var session = new GameObject("Session").AddComponent<GameSession>();
            session.botRiders = new[] { botA, botB };
            session.botLevels = catalog.botLevels;
            session.botLevelIndex = 1;

            var bootstrap = new GameObject("Bootstrap").AddComponent<GameBootstrap>();
            bootstrap.catalog = catalog;
            bootstrap.theme = theme;
            bootstrap.themes = themes;
            bootstrap.course = course;
            bootstrap.raft = raft;
            bootstrap.seats = seats;
            bootstrap.playerBrains = playerBrains;
            bootstrap.botBrains = botBrains;
            bootstrap.session = session;

            var feedback = new GameObject("Feedback").AddComponent<GameFeedback>();
            feedback.course = course;
            feedback.raftEffects = raft.GetComponent<RaftEffects>();
            feedback.theme = theme;

            var hud = new GameObject("Hud").AddComponent<GameHud>();
            hud.bootstrap = bootstrap;
            hud.course = course;
            hud.session = session;
            hud.tuning = tuning;

            if (camera != null)
            {
                var follow = camera.gameObject.AddComponent<FollowCamera>();
                follow.target = raft.transform;
                follow.targetBody = raft.GetComponent<Rigidbody>();
                camera.transform.position = raft.transform.position + follow.offset;
                camera.transform.rotation = Quaternion.LookRotation(-follow.offset.normalized, Vector3.up);
                camera.fieldOfView = 55f;
                camera.farClipPlane = 300f;
                ConfigureCamera(camera);
            }

            EditorSceneManager.SaveScene(scene, GameScenePath);
            BakeEnvironment(scene, GameScenePath);
        }

        static void BuildMenuScene(GameCatalog catalog, CourseTheme theme, CourseTheme[] themes, Material sky, VolumeProfile postFx)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            ConfigureEnvironment(sky);
            AddPostFx(postFx);
            Camera camera = Object.FindAnyObjectByType<Camera>();

            var stage = new GameObject("Stage").AddComponent<MenuStage>();
            stage.theme = theme;
            stage.themes = themes;
            stage.catalog = catalog;
            stage.viewCamera = camera;

            if (camera != null)
            {
                camera.transform.position = stage.cameraPosition;
                camera.transform.LookAt(stage.lookTarget);
                camera.fieldOfView = 60f;
                camera.farClipPlane = 300f;
                ConfigureCamera(camera);
            }

            var menu = new GameObject("Menu").AddComponent<MainMenu>();
            menu.catalog = catalog;
            menu.stage = stage;
            EditorSceneManager.SaveScene(scene, MenuScenePath);
            BakeEnvironment(scene, MenuScenePath);
        }

        static void ConfigureEnvironment(Material sky)
        {
            ConfigureLighting(sky);
        }

        static void ConfigureCamera(Camera camera)
        {
            UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.None;
        }

        static void AddPostFx(VolumeProfile profile)
        {
            var volume = new GameObject("PostFx").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
        }

        static Raft BuildRaft(RaftTuning tuning, CourseTheme theme, PhysicsMaterial physics)
        {
            var go = new GameObject("Raft");
            go.transform.SetPositionAndRotation(Vector3.up * 0.6f, Quaternion.identity);

            var body = go.AddComponent<Rigidbody>();
            body.mass = 120f;
            body.linearDamping = 0f;
            body.angularDamping = 0.05f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var collider = go.AddComponent<BoxCollider>();
            collider.size = new Vector3(3f, 0.4f, 3f);
            collider.sharedMaterial = physics;

            var raft = go.AddComponent<Raft>();
            raft.tuning = tuning;
            RaftVisualFactory.Build(go.transform, theme);

            var floatPoints = new Transform[4];
            int index = 0;
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    var point = new GameObject("FloatPoint").transform;
                    point.SetParent(go.transform, false);
                    point.localPosition = new Vector3(x * 1.3f, -0.15f, z * 1.3f);
                    floatPoints[index++] = point;
                }
            }

            var buoyancy = go.AddComponent<Buoyancy>();
            buoyancy.tuning = tuning;
            buoyancy.floatPoints = floatPoints;

            var effects = go.AddComponent<RaftEffects>();
            effects.theme = theme;
            return raft;
        }

        static Rider BuildRider(Raft raft, int seat, Vector3 localPosition, AvatarDefinition avatar, CourseTheme theme)
        {
            var go = new GameObject("Rider_" + seat);
            go.transform.SetParent(raft.transform, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;

            var rider = go.AddComponent<Rider>();
            rider.avatar = avatar;
            rider.raft = raft;
            rider.puff = EffectFactory.CreatePuff(go.transform, theme.puff);
            rider.puff.transform.localPosition = AvatarVisualFactory.PuffPosition(avatar);

            AvatarRig rig = AvatarVisualFactory.Build(go.transform, avatar, theme);
            rig.rider = rider;
            go.AddComponent<RiderAudio>();
            return rider;
        }

        static PlayerBrain AddPlayerBrain(Rider rider, int playerIndex, Camera camera)
        {
            var brain = rider.gameObject.AddComponent<PlayerBrain>();
            brain.playerIndex = playerIndex;
            brain.viewCamera = camera;
            return brain;
        }

        static BotBrain AddBotBrain(Rider rider, CourseManager course)
        {
            var brain = rider.gameObject.AddComponent<BotBrain>();
            brain.course = course;
            return brain;
        }

        static PhysicsMaterial CreateSlippery()
        {
            PhysicsMaterial slippery = LoadOrCreate(Root + "/Physics/Slippery.asset", () => new PhysicsMaterial("Slippery"));
            slippery.dynamicFriction = 0.1f;
            slippery.staticFriction = 0.1f;
            slippery.bounciness = 0.25f;
            slippery.frictionCombine = PhysicsMaterialCombine.Minimum;
            slippery.bounceCombine = PhysicsMaterialCombine.Maximum;
            EditorUtility.SetDirty(slippery);
            return slippery;
        }

        static CourseTheme CreateTheme(PhysicsMaterial physics)
        {
            Texture2D puffTexture = CreateTexture("Puff", 64, SoftCircle);
            Texture2D squareTexture = CreateTexture("Square", 16, (x, y, size) => 1f);

            CourseTheme theme = LoadOrCreate(Root + "/Data/CourseTheme.asset", ScriptableObject.CreateInstance<CourseTheme>);
            theme.water = CreateWater();
            theme.wall = Lit("Wall", new Color(0.88f, 0.78f, 0.55f), 0.05f);
            theme.obstacle = Lit("Obstacle", new Color(0.36f, 0.62f, 0.3f), 0.25f);
            theme.checkpoint = Lit("Checkpoint", new Color(1f, 0.82f, 0.15f), 0.3f);
            theme.checkpointPassed = Lit("CheckpointPassed", new Color(0.35f, 0.85f, 0.4f), 0.3f);
            theme.finish = Lit("Finish", new Color(0.96f, 0.96f, 0.96f), 0.3f);
            theme.buoy = Lit("Buoy", new Color(0.93f, 0.3f, 0.25f), 0.4f);
            theme.bubbles = Particle("Bubbles", puffTexture);
            theme.physics = physics;
            theme.waterCellSize = 1.5f;

            theme.grass = Lit("Grass", new Color(0.47f, 0.74f, 0.32f), 0.05f);
            theme.rock = Lit("Rock", new Color(0.6f, 0.6f, 0.62f), 0.1f);
            theme.trunk = Lit("Trunk", new Color(0.47f, 0.31f, 0.19f), 0.05f);
            theme.leaves = Lit("Leaves", new Color(0.33f, 0.66f, 0.27f), 0.05f);
            theme.leavesDark = Lit("LeavesDark", new Color(0.22f, 0.5f, 0.25f), 0.05f);
            theme.pine = Lit("Pine", new Color(0.18f, 0.45f, 0.3f), 0.05f);
            theme.reed = Lit("Reed", new Color(0.52f, 0.64f, 0.3f), 0.05f);
            theme.flower = Lit("Flower", new Color(1f, 0.55f, 0.78f), 0.2f);
            theme.flowerAlt = Lit("FlowerAlt", new Color(1f, 0.9f, 0.35f), 0.2f);

            theme.raftLog = Lit("Raft", new Color(0.66f, 0.47f, 0.28f), 0.15f);
            theme.rope = Lit("Plank", new Color(0.42f, 0.3f, 0.18f), 0.1f);
            theme.flag = Lit("Flag", new Color(0.95f, 0.3f, 0.3f), 0.2f);

            theme.accent = Lit("Dark", new Color(0.08f, 0.08f, 0.1f), 0.6f);
            theme.eyeWhite = Lit("EyeWhite", new Color(0.98f, 0.98f, 0.98f), 0.6f);
            theme.blush = Lit("Blush", new Color(1f, 0.55f, 0.6f), 0.2f);
            theme.playerMarkers = new Material[PlayerLayout.Colors.Length];
            for (int i = 0; i < theme.playerMarkers.Length; i++)
            {
                theme.playerMarkers[i] = Lit("Player" + (i + 1), PlayerLayout.Colors[i], 0.3f);
            }

            theme.puff = Particle("Puff", puffTexture);
            theme.splash = Particle("Splash", puffTexture);
            theme.confetti = Particle("Confetti", squareTexture);
            ApplyEnvironment(theme);
            EditorUtility.SetDirty(theme);
            return theme;
        }

        static Material CreateWater()
        {
            Shader shader = Shader.Find(WaterShaderName);
            if (shader == null)
            {
                Debug.LogWarning("[FartRafters] Water shader not found: " + WaterShaderName);
                return Lit("Water", new Color(0.16f, 0.48f, 0.78f), 0.85f);
            }

            Material material = LoadOrCreate(Root + "/Materials/Water.mat", () => new Material(shader));
            if (material.shader != shader)
            {
                material.shader = shader;
            }
            material.SetColor("_ShallowColor", new Color(0.36f, 0.58f, 0.5f));
            material.SetColor("_DeepColor", new Color(0.06f, 0.24f, 0.25f));
            material.SetColor("_ScatterColor", new Color(0.25f, 0.6f, 0.5f));
            material.SetColor("_FoamColor", new Color(0.95f, 0.97f, 0.96f));
            material.SetTexture("_WaveNormal", Baked.waveNormal);
            material.SetTexture("_FoamTex", Baked.foam);
            material.SetFloat("_NormalScale", 0.085f);
            material.SetFloat("_NormalStrength", 0.5f);
            material.SetFloat("_FlowSpeed", 0.6f);
            material.SetFloat("_DepthDistance", 2.6f);
            material.SetFloat("_ClarityDistance", 1.5f);
            material.SetFloat("_ShoreFoam", 0.55f);
            material.SetFloat("_FoamDistance", 0.45f);
            material.SetFloat("_Smoothness", 0.9f);
            material.SetFloat("_ReflectionStrength", 0.75f);
            material.SetFloat("_SunStrength", 0.45f);
            material.renderQueue = (int)RenderQueue.Transparent - 50;
            material.SetColor("_RippleColor", new Color(0.8f, 0.95f, 1f));
            EditorUtility.SetDirty(material);
            return material;
        }

        static VolumeProfile CreatePostFx()
        {
            string path = Root + "/Data/PostFx.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile != null)
            {
                ApplyPostFx(profile);
                return profile;
            }

            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);

            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.saturation.Override(18f);
            color.contrast.Override(8f);
            color.postExposure.Override(0.05f);

            Bloom bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(1.1f);
            bloom.intensity.Override(0.25f);

            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.2f);

            foreach (VolumeComponent component in profile.components)
            {
                AssetDatabase.AddObjectToAsset(component, profile);
            }
            ApplyPostFx(profile);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        static GameCatalog CreateCatalog(LevelDefinition[] levels, AvatarDefinition[] avatars, BotDifficulty[] botLevels, AdventureDifficulty[] difficulties, RaftVariant[] rafts)
        {
            string path = Root + "/Data/GameCatalog.asset";
            bool existed = AssetDatabase.LoadAssetAtPath<GameCatalog>(path) != null;
            GameCatalog catalog = LoadOrCreate(path, ScriptableObject.CreateInstance<GameCatalog>);
            catalog.levels = levels;
            catalog.avatars = avatars;
            catalog.botLevels = botLevels;
            catalog.difficulties = difficulties;
            catalog.rafts = rafts;
            catalog.tuning = AssetDatabase.LoadAssetAtPath<RaftTuning>(Root + "/Data/RaftTuning.asset");
            if (!existed)
            {
                catalog.unlockAllLevels = true;
            }
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        static AdventureDifficulty[] CreateDifficulties()
        {
            return new[]
            {
                Hazards(Summary(Extras(Difficulty("Kolay", "kolay", "Kolay", 450f, 22f, 70f, 30f, 0.1f, 48f, 0.15f, 0.3f, 0.35f, 0.2f, 1f, 2.2f, 1, 0.6f, 50f, 3f), 0f, 0f, 0f, 0f, 0f, 0f), "Sakin nehir, az engel", 0), 0.06f, 0.06f, 0f, 0.08f, 0.05f),
                Hazards(Summary(Extras(Difficulty("Orta", "orta", "Orta", 750f, 20f, 80f, 40f, 0.2f, 27f, 0.22f, 0.3f, 0.3f, 0.2f, 2.2f, 3.9f, 3, 1.26f, 55f, 3f), 0.18f, 0f, 0f, 0f, 0f, 0.15f), "Akıntılar ve timsahlar", 0), 0.12f, 0.1f, 0f, 0.08f, 0.1f),
                Hazards(Summary(Extras(Difficulty("Zor", "zor", "Zor", 1500f, 18f, 95f, 50f, 0.25f, 15f, 0.22f, 0.26f, 0.22f, 0.16f, 3.6f, 5.1f, 3, 2.4f, 65f, 3.1f), 0.25f, 0.12f, 0.15f, 45f, 0.3f, 0.18f), "Değirmen, kaya tarlası, azgın akıntı", 0), 0.12f, 0.1f, 0.08f, 0.08f, 0.1f),
                Hazards(Summary(Extras(Difficulty("Uzman", "uzman", "Uzman", 2400f, 16.5f, 110f, 58f, 0.3f, 10f, 0.22f, 0.24f, 0.2f, 0.14f, 5f, 6.4f, 4, 5f, 75f, 3.2f), 0.3f, 0.15f, 0.2f, 75f, 0.45f, 0.2f), "Her şey, hiç acımadan", 21), 0.12f, 0.1f, 0.1f, 0.08f, 0.12f)
            };
        }

        static AdventureDifficulty Hazards(AdventureDifficulty difficulty, float drift, float geyser, float barrel, float ramp, float special)
        {
            difficulty.driftWeight = drift;
            difficulty.geyserWeight = geyser;
            difficulty.barrelWeight = barrel;
            difficulty.rampWeight = ramp;
            difficulty.specialWeight = special;
            EditorUtility.SetDirty(difficulty);
            return difficulty;
        }

        static AdventureDifficulty Summary(AdventureDifficulty difficulty, string summary, int checkpoints)
        {
            difficulty.summary = summary;
            difficulty.checkpointCount = checkpoints;
            EditorUtility.SetDirty(difficulty);
            return difficulty;
        }

        static AdventureDifficulty Extras(AdventureDifficulty difficulty, float zigzag, float mill, float rockField, float millSpeed, float raging, float fall)
        {
            difficulty.zigzagChance = zigzag;
            difficulty.millWeight = mill;
            difficulty.rockFieldWeight = rockField;
            difficulty.millSpeed = millSpeed;
            difficulty.ragingChance = raging;
            difficulty.fallChance = fall;
            EditorUtility.SetDirty(difficulty);
            return difficulty;
        }

        static AdventureDifficulty Difficulty(string file, string id, string displayName, float length, float width, float checkpointSpacing, float maxBend, float narrowChance, float featureSpacing, float crocodile, float rock, float current, float thorn, float crocodileSpeed, float currentStrength, int maxRocks, float damage, float pickupSpacing, float expectedSpeed)
        {
            AdventureDifficulty difficulty = LoadOrCreate(Root + "/Data/Adventure_" + file + ".asset", ScriptableObject.CreateInstance<AdventureDifficulty>);
            difficulty.id = id;
            difficulty.displayName = displayName;
            difficulty.length = length;
            difficulty.riverWidth = width;
            difficulty.checkpointSpacing = checkpointSpacing;
            difficulty.maxBend = maxBend;
            difficulty.narrowChance = narrowChance;
            difficulty.featureSpacing = featureSpacing;
            difficulty.crocodileWeight = crocodile;
            difficulty.rockWeight = rock;
            difficulty.currentWeight = current;
            difficulty.thornWeight = thorn;
            difficulty.crocodileSpeed = crocodileSpeed;
            difficulty.currentStrength = currentStrength;
            difficulty.maxRocksPerCluster = maxRocks;
            difficulty.damageMultiplier = damage;
            difficulty.pickupSpacing = pickupSpacing;
            difficulty.expectedSpeed = expectedSpeed;
            EditorUtility.SetDirty(difficulty);
            return difficulty;
        }

        static LevelDefinition[] CreateLevels()
        {
            var levels = new LevelDefinition[LevelPresets.Files.Length];
            for (int i = 0; i < levels.Length; i++)
            {
                string path = Root + "/Data/Levels/Level_" + LevelPresets.Files[i] + ".asset";
                LevelDefinition level = LoadOrCreate(path, ScriptableObject.CreateInstance<LevelDefinition>);
                LevelPresets.Apply(i, level);
                EditorUtility.SetDirty(level);
                levels[i] = level;
            }
            return levels;
        }

        public static AvatarDefinition[] CreateAvatars()
        {
            EnsureFolder(Root + "/Data");
            EnsureFolder(Root + "/Materials");
            Material character = CreateCharacterMaterial();
            AvatarDefinition[] avatars =
            {
                CreateAvatar("Hippo", AvatarKind.Hippo, "Su aygırı", new Color(0.94f, 0.7f, 0.78f), 0.85f, character),
                CreateAvatar("Bear", AvatarKind.Bear, "Ayı", new Color(0.88f, 0.71f, 0.5f), 1f, character),
                CreateAvatar("Tombik", AvatarKind.Tombik, "Tombik", new Color(0.98f, 0.62f, 0.18f), 1.2f, character),
                CreateAvatar("Ogre", AvatarKind.Ogre, "Bataklık Devi", new Color(0.48f, 0.62f, 0.3f), 0.75f, character),
                CreateAvatar("Donkey", AvatarKind.Donkey, "Eşek", new Color(0.84f, 0.2f, 0.2f), 1.05f, character),
                CreateAvatar("Sloth", AvatarKind.Sloth, "Tembel Hayvan", new Color(0.93f, 0.86f, 0.72f), 0.9f, character),
                CreateAvatar("Uncle", AvatarKind.Uncle, "Bıyıklı Amca", new Color(0.8f, 0.2f, 0.18f), 0.8f, character),
                CreateAvatar("Dad", AvatarKind.Dad, "Gözlüklü Baba", new Color(0.85f, 0.65f, 0.25f), 0.95f, character)
            };
            AssetDatabase.SaveAssets();
            return avatars;
        }

        static AvatarDefinition CreateAvatar(string file, AvatarKind kind, string displayName, Color accent, float pitch, Material material)
        {
            string path = Root + "/Data/Avatar_" + file + ".asset";
            AvatarDefinition avatar = LoadOrCreate(path, ScriptableObject.CreateInstance<AvatarDefinition>);
            avatar.kind = kind;
            avatar.displayName = displayName;
            avatar.accentColor = accent;
            avatar.voicePitch = pitch;
            avatar.material = material;
            AvatarStats stats = StatsFor(kind);
            avatar.thrustForce = stats.thrust;
            avatar.gasCapacitySeconds = stats.gas;
            avatar.refillPerSecond = stats.refill;

            AvatarSculpture sculpture = AvatarSculptor.Create(kind);
            avatar.bodyMesh = SaveMesh(SurfaceNets.Build(sculpture.body, 0.014f, file + "_Body"), "Avatars");
            avatar.headMesh = SaveMesh(SurfaceNets.Build(sculpture.head, 0.0095f, file + "_Head"), "Avatars");
            avatar.armLeftMesh = SaveMesh(SurfaceNets.Build(sculpture.armLeft, 0.012f, file + "_ArmLeft"), "Avatars");
            avatar.armRightMesh = SaveMesh(SurfaceNets.Build(sculpture.armRight, 0.012f, file + "_ArmRight"), "Avatars");
            avatar.eyeMesh = SaveMesh(SurfaceNets.Build(sculpture.eye, 0.008f, file + "_Eye"), "Avatars");
            avatar.headPivot = sculpture.headPivot;
            avatar.shoulder = sculpture.shoulder;
            avatar.eyePosition = sculpture.eyePosition;
            avatar.puffOffset = sculpture.puff;
            BuildCostumes(avatar, file);
            avatar.fartClips = LoadClips(Root + "/Audio/Farts/" + kind);
            string voices = Root + "/Audio/Voices/" + kind;
            avatar.voice = new VoiceSet
            {
                hit = LoadClips(voices, "hit_"),
                fall = LoadClips(voices, "fall_"),
                board = LoadClips(voices, "board_"),
                eliminated = LoadClips(voices, "eliminated_"),
                cheer = LoadClips(voices, "cheer_"),
                pickup = LoadClips(voices, "pickup_"),
                caught = LoadClips(voices, "caught_"),
                caughtPolar = LoadClips(voices, "orca_").Concat(LoadClips(voices, "caught_1")).ToArray(),
                caughtOcean = LoadClips(voices, "shark_").Concat(LoadClips(voices, "caught_1")).ToArray(),
                caughtFun = LoadClips(voices, "caught_1").Concat(LoadClips(voices, "hit_2")).ToArray()
            };
            EditorUtility.SetDirty(avatar);
            return avatar;
        }

        struct AvatarStats
        {
            public float thrust;
            public float gas;
            public float refill;
        }

        static AvatarStats StatsFor(AvatarKind kind)
        {
            switch (kind)
            {
                case AvatarKind.Hippo:
                    return new AvatarStats { thrust = 528f, gas = 2.64f, refill = 0.935f };
                case AvatarKind.Bear:
                    return new AvatarStats { thrust = 561f, gas = 2.42f, refill = 0.792f };
                case AvatarKind.Tombik:
                    return new AvatarStats { thrust = 473f, gas = 3.19f, refill = 1.045f };
                case AvatarKind.Ogre:
                    return new AvatarStats { thrust = 517f, gas = 2.97f, refill = 0.726f };
                case AvatarKind.Donkey:
                    return new AvatarStats { thrust = 462f, gas = 2.64f, refill = 1.265f };
                case AvatarKind.Sloth:
                    return new AvatarStats { thrust = 434.5f, gas = 3.52f, refill = 0.935f };
                case AvatarKind.Uncle:
                    return new AvatarStats { thrust = 522.5f, gas = 2.75f, refill = 0.858f };
                default:
                    return new AvatarStats { thrust = 484f, gas = 2.86f, refill = 1.122f };
            }
        }

        static Material CreateCharacterMaterial()
        {
            return SculptMaterial("Character", 0.34f, 9f, 0.1f, 0.22f);
        }

        static AudioClip[] LoadClips(string folder, string prefix)
        {
            return System.Array.FindAll(LoadClips(folder), clip => clip.name.StartsWith(prefix));
        }

        static AudioClip[] LoadClips(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                return new AudioClip[0];
            }
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folder });
            var clips = new AudioClip[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guids[i]));
            }
            Array.Sort(clips, (a, b) => string.CompareOrdinal(a.name, b.name));
            return clips;
        }

        public static void BuildCostumes(AvatarDefinition avatar, string file)
        {
            avatar.costumes = new Costume[WardrobeSculptor.Count];
            for (int i = 0; i < WardrobeSculptor.Count; i++)
            {
                CostumeParts parts = WardrobeSculptor.Build(avatar.kind, i);
                avatar.costumes[i] = new Costume
                {
                    name = parts.name,
                    head = parts.head != null ? SaveMesh(SurfaceNets.Build(parts.head, 0.0085f, file + "_Hat" + i), "Costumes") : null,
                    body = parts.body != null ? SaveMesh(SurfaceNets.Build(parts.body, 0.011f, file + "_Outfit" + i), "Costumes") : null,
                    armLeft = parts.armLeft != null ? SaveMesh(SurfaceNets.Build(parts.armLeft, 0.009f, file + "_SleeveL" + i), "Costumes") : null,
                    armRight = parts.armRight != null ? SaveMesh(SurfaceNets.Build(parts.armRight, 0.009f, file + "_SleeveR" + i), "Costumes") : null
                };
            }
            EditorUtility.SetDirty(avatar);
        }

        public static void RebuildRafts()
        {
            SaveMesh(SurfaceNets.Build(RaftSculptor.InflatableBoat(), 0.028f, "RaftInflatable"), "Props");
            SaveMesh(SurfaceNets.Build(RaftSculptor.Rowboat(), 0.028f, "RaftRowboat"), "Props");
            SaveMesh(SurfaceNets.Build(RaftSculptor.BigRing(), 0.028f, "RaftRing"), "Props");
            SaveMesh(SurfaceNets.Build(RaftSculptor.Bathtub(), 0.028f, "RaftBathtub"), "Props");
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(Root + "/Data/GameCatalog.asset");
            CourseTheme river = AssetDatabase.LoadAssetAtPath<CourseTheme>(Root + "/Data/CourseTheme.asset");
            if (catalog != null && river != null)
            {
                catalog.rafts = CreateRafts(river);
                foreach (AvatarDefinition avatar in catalog.avatars)
                {
                    AvatarStats stats = StatsFor(avatar.kind);
                    avatar.thrustForce = stats.thrust;
                    avatar.gasCapacitySeconds = stats.gas;
                    avatar.refillPerSecond = stats.refill;
                    EditorUtility.SetDirty(avatar);
                }
                EditorUtility.SetDirty(catalog);
            }
            AssetDatabase.SaveAssets();
        }

        public static void RebuildCostumes()
        {
            RebuildRafts();
            string only = "";
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-avatars")
                {
                    only = args[i + 1];
                }
            }
            foreach (string guid in AssetDatabase.FindAssets("t:AvatarDefinition", new[] { Root + "/Data" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (only.Length > 0 && !only.Split(',').Any(name => path.EndsWith("Avatar_" + name + ".asset")))
                {
                    continue;
                }
                AvatarDefinition avatar = AssetDatabase.LoadAssetAtPath<AvatarDefinition>(path);
                string file = System.IO.Path.GetFileNameWithoutExtension(path).Replace("Avatar_", "");
                BuildCostumes(avatar, file);
            }
            AssetDatabase.SaveAssets();
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        public static Mesh SaveMesh(Mesh mesh, string folder)
        {
            EnsureFolder(Root + "/Generated");
            EnsureFolder(Root + "/Generated/" + folder);
            string path = Root + "/Generated/" + folder + "/" + mesh.name + ".asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }
            existing.Clear();
            EditorUtility.CopySerialized(mesh, existing);
            Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        public static void RefreshBotLevels()
        {
            CreateBotLevels();
            AssetDatabase.SaveAssets();
        }

        static BotDifficulty[] CreateBotLevels()
        {
            BotDifficulty bad = CreateBotLevel("Bad", BotLevel.Bad, "Kötü", level =>
            {
                level.cruiseSpeed = 3.5f;
                level.minCruiseSpeed = 2f;
                level.decisionInterval = 0.35f;
                level.speedTolerance = 1.5f;
                level.minGasToStart = 0.1f;
                level.aimNoiseDegrees = 35f;
                level.facingTolerance = 90f;
                level.responsiveness = 1f;
                level.mistakeChancePerSecond = 0.9f;
                level.mistakeDuration = new Vector2(0.6f, 1.4f);
                level.hazardMistakeChancePerSecond = 0.9f;
                level.cornerLookAhead = 0f;
                level.planLookAhead = 0f;
                level.waitForSwimmers = false;
                level.teamwork = 0.35f;
            });
            BotDifficulty medium = CreateBotLevel("Medium", BotLevel.Medium, "Orta", level =>
            {
                level.cruiseSpeed = 4.2f;
                level.minCruiseSpeed = 2.5f;
                level.decisionInterval = 0.22f;
                level.speedTolerance = 1f;
                level.minGasToStart = 0.15f;
                level.aimNoiseDegrees = 12f;
                level.facingTolerance = 70f;
                level.responsiveness = 1.3f;
                level.mistakeChancePerSecond = 0.08f;
                level.mistakeDuration = new Vector2(0.4f, 0.9f);
                level.hazardMistakeChancePerSecond = 0.7f;
                level.cornerLookAhead = 0.3f;
                level.planLookAhead = 0f;
                level.waitForSwimmers = true;
                level.teamwork = 0.75f;
            });
            BotDifficulty good = CreateBotLevel("Good", BotLevel.Good, "İyi", level =>
            {
                level.cruiseSpeed = 5.2f;
                level.minCruiseSpeed = 2.2f;
                level.gasReserve = 0.2f;
                level.decisionInterval = 0.1f;
                level.speedTolerance = 0.6f;
                level.minGasToStart = 0.3f;
                level.aimNoiseDegrees = 2f;
                level.facingTolerance = 40f;
                level.responsiveness = 1.8f;
                level.mistakeChancePerSecond = 0.008f;
                level.mistakeDuration = new Vector2(0.15f, 0.3f);
                level.hazardMistakeChancePerSecond = 0.01f;
                level.cornerLookAhead = 0.5f;
                level.planLookAhead = 14f;
                level.planLanes = 9;
                level.planMargin = 1f;
                level.predictCrocodiles = true;
                level.readCurrents = true;
                level.collectPickups = false;
                level.dangerSlowdown = 50f;
                level.waitForSwimmers = true;
                level.timeCrocodiles = true;
                level.chargeCurrents = true;
                level.teamwork = 1f;
            });
            BotDifficulty super = CreateBotLevel("Super", BotLevel.Super, "Süper Zeka", level =>
            {
                level.cruiseSpeed = 5.6f;
                level.minCruiseSpeed = 1.8f;
                level.gasReserve = 0.35f;
                level.decisionInterval = 0.05f;
                level.speedTolerance = 0.3f;
                level.minGasToStart = 0.2f;
                level.aimNoiseDegrees = 0f;
                level.facingTolerance = 40f;
                level.responsiveness = 2.2f;
                level.mistakeChancePerSecond = 0f;
                level.hazardMistakeChancePerSecond = 0f;
                level.cornerLookAhead = 0.5f;
                level.planLookAhead = 24f;
                level.planLanes = 13;
                level.planMargin = 1.4f;
                level.predictCrocodiles = true;
                level.readCurrents = true;
                level.collectPickups = true;
                level.dangerSlowdown = 35f;
                level.waitForSwimmers = true;
                level.timeCrocodiles = true;
                level.chargeCurrents = true;
                level.teamwork = 1f;
            });
            return new[] { bad, medium, good, super };
        }

        static BotDifficulty CreateBotLevel(string file, BotLevel kind, string displayName, Action<BotDifficulty> preset)
        {
            string path = Root + "/Data/Bot_" + file + ".asset";
            BotDifficulty level = LoadOrCreate(path, ScriptableObject.CreateInstance<BotDifficulty>);
            level.level = kind;
            level.displayName = displayName;
            preset(level);
            EditorUtility.SetDirty(level);
            return level;
        }

        static Material Lit(string name, Color color, float smoothness)
        {
            Material material = LoadOrCreate(Root + "/Materials/" + name + ".mat", () => new Material(Shader.Find("Universal Render Pipeline/Lit")));
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material Particle(string name, Texture2D texture)
        {
            Material material = LoadOrCreate(Root + "/Materials/" + name + ".mat", () => new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")));
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }

        static float SoftCircle(int x, int y, int size)
        {
            float radius = size * 0.5f;
            float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius)) / radius;
            float alpha = Mathf.Clamp01(1f - distance);
            return alpha * alpha * (3f - 2f * alpha);
        }

        static Texture2D CreateTexture(string name, int size, Func<int, int, int, float> alpha)
        {
            string path = Root + "/Textures/" + name + ".png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha(x, y, size)));
                    }
                }
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);

                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static T LoadOrCreate<T>(string path, Func<T> factory) where T : Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }
            asset = factory();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
