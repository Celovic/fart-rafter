using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace FartRafters.EditorTools
{
    public static class AvatarPreview
    {
        const string ThemePath = "Assets/_Project/Data/CourseTheme.asset";

        public static void RenderAvatars()
        {
            AvatarDefinition[] avatars = SceneBuilder.CreateAvatars();
            var paths = new string[avatars.Length];
            for (int i = 0; i < avatars.Length; i++)
            {
                paths[i] = AssetDatabase.GetAssetPath(avatars[i]);
            }
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CourseTheme theme = AssetDatabase.LoadAssetAtPath<CourseTheme>(ThemePath);
            for (int i = 0; i < avatars.Length; i++)
            {
                avatars[i] = AssetDatabase.LoadAssetAtPath<AvatarDefinition>(paths[i]);
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.64f, 0.76f, 0.9f);
            RenderSettings.ambientEquatorColor = new Color(0.56f, 0.64f, 0.62f);
            RenderSettings.ambientGroundColor = new Color(0.36f, 0.4f, 0.32f);
            DynamicGI.UpdateEnvironment();

            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(40f, 150f, 0f);
            sun.color = new Color(1f, 0.95f, 0.86f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.transform.localScale = Vector3.one * 3f;
            var groundMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            groundMaterial.SetColor("_BaseColor", new Color(0.55f, 0.62f, 0.5f));
            ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;

            var holders = new Transform[avatars.Length];
            for (int i = 0; i < avatars.Length; i++)
            {
                holders[i] = new GameObject(avatars[i].name).transform;
                holders[i].position = new Vector3((i - (avatars.Length - 1) * 0.5f) * 1.45f, 0f, 0f);
                AvatarVisualFactory.Build(holders[i], avatars[i], theme);
            }

            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.7f, 0.82f, 0.92f);
            camera.fieldOfView = 24f;

            Save(camera, "Logs/warmup.png", 64, 64);
            Capture(camera, holders, 20f, new Vector3(0f, 1.6f, 14f), new Vector3(0f, 0.75f, 0f), "Logs/avatars_front.png", 2400, 700);
            Capture(camera, holders, 200f, new Vector3(0f, 1.6f, 14f), new Vector3(0f, 0.75f, 0f), "Logs/avatars_back.png", 2400, 700);
            Capture(camera, holders, 90f, new Vector3(0f, 1.6f, 14f), new Vector3(0f, 0.75f, 0f), "Logs/avatars_side.png", 2400, 700);
            for (int i = 0; i < holders.Length; i++)
            {
                holders[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < holders.Length; i++)
            {
                holders[i].gameObject.SetActive(true);
                Vector3 position = holders[i].position;
                holders[i].position = Vector3.zero;
                holders[i].rotation = Quaternion.Euler(0f, 15f, 0f);
                camera.fieldOfView = 22f;
                camera.transform.position = new Vector3(0f, 1.35f, 4.2f);
                camera.transform.LookAt(new Vector3(0f, 1.15f, 0f));
                Save(camera, "Logs/avatar_" + avatars[i].kind + ".png", 900, 900);
                holders[i].position = position;
                holders[i].gameObject.SetActive(false);
            }
            Debug.Log("[FartRafters] Avatar previews written");
        }

        public const string IconPath = "Assets/_Project/Art/AppIcon.png";
        public const string IconClearPath = "Assets/_Project/Art/AppIconClear.png";

        public static void RenderAppIcon()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CourseTheme theme = AssetDatabase.LoadAssetAtPath<CourseTheme>(ThemePath);
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/_Project/Data/GameCatalog.asset");
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.75f, 0.85f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.62f, 0.7f, 0.72f);
            RenderSettings.ambientGroundColor = new Color(0.35f, 0.45f, 0.5f);
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(45f, 160f, 0f);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;

            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            Material Mat(Color color, float smooth)
            {
                var material = new Material(lit);
                material.SetColor("_BaseColor", color);
                material.SetFloat("_Smoothness", smooth);
                return material;
            }

            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.transform.position = new Vector3(0f, -0.08f, 0f);
            water.transform.localScale = new Vector3(5.2f, 0.05f, 5.2f);
            water.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.16f, 0.62f, 0.86f), 0.85f);
            for (int i = 0; i < 9; i++)
            {
                float angle = i * 0.7f;
                var ripple = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ripple.transform.position = new Vector3(Mathf.Cos(angle) * (1.8f + i * 0.04f), -0.03f, Mathf.Sin(angle) * 1.9f);
                ripple.transform.localScale = new Vector3(0.5f, 0.06f, 0.22f);
                ripple.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.85f, 0.95f, 1f), 0.6f);
            }

            var raftRoot = new GameObject("Raft").transform;
            raftRoot.rotation = Quaternion.Euler(0f, 20f, 0f);
            Transform raftVisual = RaftVisualFactory.Build(raftRoot, theme);
            foreach (Transform part in raftVisual)
            {
                if (part.name == "Pole" || part.name == "FlagPivot")
                {
                    part.gameObject.SetActive(false);
                }
            }
            var holder = new GameObject("Rider").transform;
            holder.SetParent(raftRoot, false);
            holder.localPosition = new Vector3(0f, 0.26f, 0.05f);
            holder.localRotation = Quaternion.Euler(0f, 160f, 0f);
            AvatarVisualFactory.Build(holder, catalog.avatars[0], theme, 0);

            Material cloud = Mat(new Color(0.62f, 0.8f, 0.28f), 0.2f);
            Vector3 puff = new Vector3(0.75f, 1.05f, 0.9f);
            float[] sizes = { 0.75f, 0.6f, 0.55f, 0.45f, 0.38f, 0.3f };
            Vector3[] offsets = { Vector3.zero, new Vector3(0.45f, 0.25f, 0.2f), new Vector3(-0.35f, 0.3f, 0.25f), new Vector3(0.2f, 0.62f, 0.3f), new Vector3(0.75f, 0.6f, 0.35f), new Vector3(1.05f, 0.95f, 0.4f) };
            for (int i = 0; i < sizes.Length; i++)
            {
                var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ball.transform.position = puff + offsets[i];
                ball.transform.localScale = Vector3.one * sizes[i];
                ball.GetComponent<Renderer>().sharedMaterial = cloud;
            }

            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(1f, 0.78f, 0.25f);
            camera.fieldOfView = 30f;
            camera.transform.position = new Vector3(-0.3f, 5.4f, -7.8f);
            camera.transform.LookAt(new Vector3(0.2f, 0.55f, 0.35f));
            Directory.CreateDirectory("Assets/_Project/Art");
            Save(camera, "Logs/warmup.png", 64, 64);
            Save(camera, IconPath, 1024, 1024);
            var clear = new Texture2D(432, 432, TextureFormat.RGBA32, false);
            clear.SetPixels32(new Color32[432 * 432]);
            clear.Apply();
            File.WriteAllBytes(IconClearPath, clear.EncodeToPNG());
            Object.DestroyImmediate(clear);
            AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(IconClearPath, ImportAssetOptions.ForceUpdate);
            foreach (string path in new[] { IconPath, IconClearPath })
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            File.Copy(IconPath, "Logs/app_icon.png", true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        public static void RenderCostumeCloseups()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CourseTheme theme = AssetDatabase.LoadAssetAtPath<CourseTheme>(ThemePath);
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/_Project/Data/GameCatalog.asset");
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.64f, 0.76f, 0.9f);
            RenderSettings.ambientEquatorColor = new Color(0.56f, 0.64f, 0.62f);
            RenderSettings.ambientGroundColor = new Color(0.36f, 0.4f, 0.32f);
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(40f, 150f, 0f);
            sun.intensity = 1.15f;
            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.7f, 0.82f, 0.92f);
            camera.fieldOfView = 30f;
            Directory.CreateDirectory("Logs/costumes");
            Save(camera, "Logs/costumes/warmup.png", 64, 64);
            for (int a = 0; a < catalog.avatars.Length; a++)
            {
                AvatarDefinition avatar = catalog.avatars[a];
                for (int c = 0; c < avatar.CostumeCount; c++)
                {
                    var holder = new GameObject(avatar.name + c).transform;
                    AvatarVisualFactory.Build(holder, avatar, theme, c);
                    Bounds bounds = new Bounds(holder.position + Vector3.up * 0.6f, Vector3.one * 0.2f);
                    foreach (Renderer renderer in holder.GetComponentsInChildren<Renderer>())
                    {
                        if (!(renderer is ParticleSystemRenderer))
                        {
                            bounds.Encapsulate(renderer.bounds);
                        }
                    }
                    float distance = bounds.extents.magnitude / Mathf.Tan(15f * Mathf.Deg2Rad) * 1.1f;
                    float[] yaws = { 20f, 200f };
                    string[] tags = { "f", "b" };
                    for (int v = 0; v < 2; v++)
                    {
                        Vector3 dir = Quaternion.Euler(12f, yaws[v], 0f) * Vector3.forward;
                        camera.transform.position = bounds.center + dir * distance;
                        camera.transform.LookAt(bounds.center);
                        Save(camera, "Logs/costumes/" + avatar.name + "_" + c + "_" + tags[v] + ".png", 600, 600);
                    }
                    Object.DestroyImmediate(holder.gameObject);
                }
            }
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        public static void RenderWardrobe()
        {
            SceneBuilder.Build();
            AssetDatabase.SaveAssets();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CourseTheme theme = AssetDatabase.LoadAssetAtPath<CourseTheme>(ThemePath);
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/_Project/Data/GameCatalog.asset");
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.64f, 0.76f, 0.9f);
            RenderSettings.ambientEquatorColor = new Color(0.56f, 0.64f, 0.62f);
            RenderSettings.ambientGroundColor = new Color(0.36f, 0.4f, 0.32f);
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(40f, 150f, 0f);
            sun.color = new Color(1f, 0.95f, 0.86f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.7f, 0.82f, 0.92f);
            camera.fieldOfView = 24f;
            var root = new GameObject("Wardrobe").transform;
            for (int a = 0; a < catalog.avatars.Length; a++)
            {
                AvatarDefinition avatar = catalog.avatars[a];
                for (int c = 0; c < avatar.CostumeCount; c++)
                {
                    var holder = new GameObject(avatar.name + c).transform;
                    holder.SetParent(root, false);
                    holder.position = new Vector3((a - (catalog.avatars.Length - 1) * 0.5f) * 1.5f, 0f, -c * 2.2f);
                    holder.rotation = Quaternion.Euler(0f, 15f, 0f);
                    AvatarVisualFactory.Build(holder, avatar, theme, c);
                }
            }
            camera.transform.position = new Vector3(0f, 6.5f, 13f);
            camera.transform.LookAt(new Vector3(0f, 0.6f, -2.2f));
            camera.fieldOfView = 38f;
            Save(camera, "Logs/warmup.png", 64, 64);
            Save(camera, "Logs/wardrobe.png", 2400, 1400);
            Object.DestroyImmediate(root.gameObject);
            var rafts = new GameObject("Rafts").transform;
            for (int i = 0; i < catalog.rafts.Length; i++)
            {
                RaftVariant variant = catalog.rafts[i];
                GameObject raft = CourseBuilder.CreateSculpt(variant.name, rafts, variant.mesh, variant.material, new Vector3((i - (catalog.rafts.Length - 1) * 0.5f) * 3.8f, 0.4f, 0f), Vector3.one);
                raft.transform.rotation = Quaternion.Euler(0f, 30f, 0f);
            }
            camera.transform.position = new Vector3(0f, 6f, 11f);
            camera.transform.LookAt(new Vector3(0f, 0f, 0f));
            camera.fieldOfView = 45f;
            Save(camera, "Logs/rafts.png", 2400, 1000);
            Debug.Log("[FartRafters] Wardrobe previews written");
        }

        public static void RenderScenes()
        {
            SceneBuilder.Build();
            AssetDatabase.SaveAssets();

            EditorSceneManager.OpenScene(SceneBuilder.GameScenePath, OpenSceneMode.Single);
            DynamicGI.UpdateEnvironment();
            var bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            typeof(GameBootstrap).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(bootstrap, null);
            Camera camera = Camera.main;
            var follow = camera.GetComponent<FollowCamera>();
            Transform raft = bootstrap.raft.transform;
            Object.FindAnyObjectByType<DecorStreamer>().BuildNear(raft.position);
            camera.transform.position = raft.position + follow.offset;
            camera.transform.rotation = Quaternion.LookRotation(-follow.offset.normalized, Vector3.up);
            Save(camera, "Logs/warmup.png", 64, 64);
            Save(camera, "Logs/scene_game.png", 1600, 900);
            Debug.Log("[FartRafters] Water meshes: " + Object.FindObjectsByType<WaterMesh>(FindObjectsSortMode.None).Length + " time " + Shader.GetGlobalFloat("_FR_WaterTime") + " wave " + Shader.GetGlobalVector("_FR_Wave"));

            camera.transform.position = raft.position + raft.rotation * new Vector3(2.8f, 2.4f, 4.2f);
            camera.transform.LookAt(raft.position + Vector3.up * 0.6f);
            Save(camera, "Logs/scene_raft.png", 1600, 900);

            camera.transform.position = raft.position + raft.rotation * new Vector3(0f, 22f, -18f);
            camera.transform.LookAt(raft.position + raft.forward * 14f);
            Save(camera, "Logs/scene_overview.png", 1600, 900);

            RenderCrowd(camera, bootstrap.theme, raft.position + new Vector3(0f, 0f, 40f));

            RenderAdventure();

            EditorSceneManager.OpenScene(SceneBuilder.MenuScenePath, OpenSceneMode.Single);
            DynamicGI.UpdateEnvironment();
            var stage = Object.FindAnyObjectByType<MenuStage>();
            typeof(MenuStage).GetMethod("EnsureBuilt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(stage, null);
            Camera menuCamera = Camera.main;
            menuCamera.transform.position = stage.cameraPosition;
            menuCamera.transform.LookAt(stage.lookTarget);
            Save(menuCamera, "Logs/warmup.png", 64, 64);
            Save(menuCamera, "Logs/scene_menu.png", 1600, 900);
            Debug.Log("[FartRafters] Scene previews written");
        }

        static Biome[] PreviewBiomes()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            int index = System.Array.IndexOf(args, "-biomes");
            if (index >= 0 && index + 1 < args.Length)
            {
                var list = new System.Collections.Generic.List<Biome>();
                foreach (string name in args[index + 1].Split(","))
                {
                    if (System.Enum.TryParse(name, true, out Biome biome))
                    {
                        list.Add(biome);
                    }
                }
                return list.ToArray();
            }
            return (Biome[])System.Enum.GetValues(typeof(Biome));
        }

        public static void RenderBiomes()
        {
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-norebuild") < 0)
            {
                SceneBuilder.Build();
                AssetDatabase.SaveAssets();
            }
            GameSelection.Mode previousMode = GameSelection.CurrentMode;
            int previousDifficulty = GameSelection.DifficultyIndex;
            int previousBiome = GameSelection.BiomeChoice;
            GameSelection.CurrentMode = GameSelection.Mode.Adventure;
            GameSelection.DifficultyIndex = 2;
            foreach (Biome biome in PreviewBiomes())
            {
                GameSelection.BiomeChoice = (int)biome;
                string tag = "Logs/biome_" + biome.ToString().ToLowerInvariant();
                EditorSceneManager.OpenScene(SceneBuilder.GameScenePath, OpenSceneMode.Single);
                var bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
                typeof(GameBootstrap).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(bootstrap, null);
                CourseTheme theme = bootstrap.theme;
                Shader.SetGlobalVector("_FR_Wave", new Vector4(theme.waveAmplitude, theme.waveLength, theme.waveSpeed, 0f));
                Shader.SetGlobalFloat("_FR_WaterTime", 3.7f);
                foreach (IceZone zone in Object.FindObjectsByType<IceZone>(FindObjectsSortMode.None))
                {
                    typeof(IceZone).GetMethod("OnEnable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(zone, null);
                }
                LevelDefinition level = bootstrap.Level;
                Debug.Log("[FartRafters] Biome " + biome + " theme " + theme.name + " ice " + level.ice.Length + " pools " + level.whirlpools.Length + " mills " + level.mills.Length + " crocs " + level.obstacles.Length + " thorns " + level.thorns.Length);
                Camera camera = Camera.main;
                var follow = camera.GetComponent<FollowCamera>();
                var streamer = Object.FindAnyObjectByType<DecorStreamer>();
                Transform raft = bootstrap.raft.transform;
                streamer.BuildNear(raft.position);
                camera.transform.position = raft.position + follow.offset;
                camera.transform.rotation = Quaternion.LookRotation(-follow.offset.normalized, Vector3.up);
                Save(camera, "Logs/warmup.png", 64, 64);
                Save(camera, tag + "_game.png", 1600, 900);
                camera.transform.position = raft.position + raft.rotation * new Vector3(0f, 26f, -20f);
                camera.transform.LookAt(raft.position + raft.forward * 18f);
                Save(camera, tag + "_overview.png", 1600, 900);
                if (theme.chaseMesh != null)
                {
                    float chaseScale = theme.chaseSize / Mathf.Max(0.1f, theme.chaseMesh.bounds.size.z);
                    Vector3 spot = raft.position + raft.forward * 14f;
                    streamer.BuildNear(spot);
                    for (int i = 0; i < theme.chaseCount; i++)
                    {
                        GameObject chase = CourseBuilder.CreateSculpt("ChasePreview", bootstrap.transform, theme.chaseMesh, theme.sculptMaterial, Vector3.zero, Vector3.one * chaseScale);
                        chase.transform.position = spot + raft.right * ((i - (theme.chaseCount - 1) * 0.5f) * theme.chaseSize * 0.9f) + Vector3.up * theme.chaseHeight;
                        chase.transform.rotation = Quaternion.LookRotation(raft.right, Vector3.up);
                    }
                    camera.transform.position = spot - raft.right * (theme.chaseSize * 1.6f) + Vector3.up * (theme.chaseSize * 0.45f) - raft.forward * 2f;
                    camera.transform.LookAt(spot + Vector3.up * 0.8f);
                    Save(camera, tag + "_chase.png", 1200, 800);
                }
                foreach (PickupKind kind in new[] { PickupKind.Repair, PickupKind.Bubble, PickupKind.Magnet })
                {
                    foreach (Pickup pickup in Object.FindObjectsByType<Pickup>(FindObjectsSortMode.None))
                    {
                        if (pickup.kind == kind)
                        {
                            Shot(camera, streamer, pickup, new Vector3(-2.5f, 1.6f, -3f), Vector3.up * 0.4f, tag + "_pickup_" + kind.ToString().ToLowerInvariant() + ".png");
                            break;
                        }
                    }
                }
                Shot(camera, streamer, Object.FindAnyObjectByType<MovingObstacle>(), new Vector3(-6f, 3.5f, -7f), Vector3.zero, tag + "_predator.png");
                Shot(camera, streamer, Object.FindAnyObjectByType<IceZone>(), new Vector3(-9f, 7f, -14f), Vector3.zero, tag + "_ice.png");
                Shot(camera, streamer, Object.FindAnyObjectByType<Whirlpool>(), new Vector3(-8f, 9f, -11f), Vector3.zero, tag + "_whirlpool.png");
                Shot(camera, streamer, Object.FindAnyObjectByType<MillWheel>(), new Vector3(-9f, 5f, -10f), Vector3.down * 1.5f, tag + "_mill.png");
                Shot(camera, streamer, Object.FindAnyObjectByType<Geyser>(), new Vector3(-6f, 4f, -7f), Vector3.zero, tag + "_geyser.png");
                Shot(camera, streamer, Object.FindAnyObjectByType<Barrel>(), new Vector3(-4f, 2.5f, -5f), Vector3.up * 0.3f, tag + "_barrel.png");
                Shot(camera, streamer, Object.FindAnyObjectByType<LaunchRamp>(), new Vector3(-7f, 4.5f, -9f), Vector3.zero, tag + "_ramp.png");
                Shot(camera, streamer, Object.FindAnyObjectByType<IcicleDrop>(), new Vector3(-6f, 5f, -8f), Vector3.up * 3f, tag + "_icicle.png");
                foreach (MovingObstacle mover in Object.FindObjectsByType<MovingObstacle>(FindObjectsSortMode.None))
                {
                    if (mover.kind == ObstacleKind.Drift)
                    {
                        Shot(camera, streamer, mover, new Vector3(-6f, 3.5f, -7f), Vector3.zero, tag + "_drift.png");
                        break;
                    }
                }
                foreach (MovingObstacle mover in Object.FindObjectsByType<MovingObstacle>(FindObjectsSortMode.None))
                {
                    if (mover.kind == ObstacleKind.Eel)
                    {
                        Shot(camera, streamer, mover, new Vector3(-5f, 3f, -6f), Vector3.zero, tag + "_eel.png");
                        break;
                    }
                }
                foreach (Transform child in bootstrap.GetComponentsInChildren<Transform>())
                {
                    if (child.name.StartsWith("Bramble"))
                    {
                        Bounds bounds = child.GetComponent<Renderer>().bounds;
                        streamer.BuildNear(bounds.center);
                        camera.transform.position = bounds.center + new Vector3(-4f, 3f, -5f);
                        camera.transform.LookAt(bounds.center);
                        Save(camera, tag + "_thorns.png", 1000, 700);
                        break;
                    }
                }
                foreach (Transform child in bootstrap.GetComponentsInChildren<Transform>())
                {
                    if (child.name == "RockVisual")
                    {
                        Bounds bounds = child.GetComponent<Renderer>().bounds;
                        Vector3 center = bounds.center;
                        streamer.BuildNear(center);
                        camera.transform.position = center + new Vector3(-6f, 4f, -8f);
                        camera.transform.LookAt(center);
                        Save(camera, tag + "_rocks.png", 1000, 700);
                        break;
                    }
                }
                Vector3 bank = level.Edge(Mathf.Min(level.NodeCount - 2, 20), theme.openSide != 0 ? -theme.openSide : -1f);
                streamer.BuildNear(bank);
                camera.transform.position = bank + Vector3.Cross(Vector3.up, level.Direction(20)) * (theme.openSide != 0 ? theme.openSide * 9f : 9f) + Vector3.up * 5f;
                camera.transform.LookAt(bank + Vector3.up * 1.5f);
                Save(camera, tag + "_shore.png", 1600, 900);
                RenderCrowd(camera, theme, raft.position + new Vector3(0f, 0f, 40f));
                System.IO.File.Copy("Logs/scene_crowd.png", tag + "_crowd.png", true);
            }
            GameSelection.CurrentMode = previousMode;
            GameSelection.DifficultyIndex = previousDifficulty;
            GameSelection.BiomeChoice = previousBiome;
            Debug.Log("[FartRafters] Biome previews written");
        }

        static void Shot(Camera camera, DecorStreamer streamer, Component target, Vector3 offset, Vector3 look, string path)
        {
            if (target == null)
            {
                return;
            }
            Vector3 position = target.transform.position;
            streamer.BuildNear(position);
            camera.transform.position = position + offset;
            camera.transform.LookAt(position + look);
            Save(camera, path, 1200, 800);
        }

        static void RenderCrowd(Camera camera, CourseTheme theme, Vector3 origin)
        {
            if (theme.spectators == null || theme.spectators.Length == 0)
            {
                return;
            }
            var root = new GameObject("CrowdPreview").transform;
            root.position = origin + Vector3.up * 2f;
            var spectators = new System.Collections.Generic.List<Spectator>();
            var random = new System.Random(4);
            for (int row = 0; row < 2; row++)
            {
                for (int i = 0; i < theme.spectators.Length; i++)
                {
                    SpectatorAct act = row == 0 ? (SpectatorAct)(i % 5) : (SpectatorAct)((i + 3) % 5);
                    Spectator spectator = SpectatorFactory.Build(root, theme.spectators[i], theme, act, SpectatorFactory.RandomSign(i % 2 == 0, random, theme), i);
                    spectator.transform.localPosition = new Vector3((i - theme.spectators.Length * 0.5f) * 1.25f, 0f, row * -2.2f);
                    spectator.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    spectators.Add(spectator);
                }
            }
            foreach (TextMesh text in root.GetComponentsInChildren<TextMesh>())
            {
                text.transform.localScale = Vector3.one * 0.6f;
            }
            for (int i = 0; i < spectators.Count; i++)
            {
                spectators[i].Pose(0.6f + i * 0.37f, 1f);
            }
            camera.transform.position = root.position + new Vector3(0f, 2.2f, -9.5f);
            camera.transform.LookAt(root.position + new Vector3(0f, 0.9f, -1f));
            Save(camera, "Logs/scene_crowd.png", 2000, 900);
            Object.DestroyImmediate(root.gameObject);
        }

        static void RenderAdventure()
        {
            GameSelection.Mode previousMode = GameSelection.CurrentMode;
            int previousDifficulty = GameSelection.DifficultyIndex;
            GameSelection.CurrentMode = GameSelection.Mode.Adventure;
            GameSelection.DifficultyIndex = 3;

            EditorSceneManager.OpenScene(SceneBuilder.GameScenePath, OpenSceneMode.Single);
            DynamicGI.UpdateEnvironment();
            var bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            typeof(GameBootstrap).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(bootstrap, null);
            LevelDefinition level = bootstrap.Level;
            Debug.Log("[FartRafters] Adventure nodes " + level.NodeCount + " checkpoints " + level.checkpointNodes.Length + " rocks " + level.rocks.Length + " crocs " + level.obstacles.Length + " currents " + level.currents.Length + " thorns " + level.thorns.Length + " pickups " + level.pickups.Length + " stars " + level.parTime);

            Camera camera = Camera.main;
            var streamer = Object.FindAnyObjectByType<DecorStreamer>();
            int node = Mathf.Min(level.NodeCount - 1, 14);
            Vector3 focus = level.Center(node);
            streamer.BuildNear(focus);
            camera.transform.position = focus + new Vector3(0f, 90f, -50f);
            camera.transform.LookAt(focus + Vector3.forward * 30f);
            Save(camera, "Logs/warmup.png", 64, 64);
            Save(camera, "Logs/scene_adventure.png", 1600, 900);

            MillWheel wheel = Object.FindAnyObjectByType<MillWheel>();
            if (wheel != null)
            {
                streamer.BuildNear(wheel.transform.position);
                camera.transform.position = wheel.transform.position + new Vector3(-9f, 5f, -10f);
                camera.transform.LookAt(wheel.transform.position + Vector3.down * 1.5f);
                Save(camera, "Logs/scene_mill.png", 1200, 800);
            }

            foreach (Pickup pickup in Object.FindObjectsByType<Pickup>(FindObjectsSortMode.None))
            {
                camera.transform.position = pickup.transform.position + new Vector3(2.2f, 1.6f, -2.6f);
                camera.transform.LookAt(pickup.transform.position + Vector3.up * 0.3f);
                Save(camera, "Logs/scene_pickup_" + pickup.kind + ".png", 800, 600);
            }
            foreach (Transform child in Object.FindAnyObjectByType<GameBootstrap>().GetComponentsInChildren<Transform>())
            {
                if (child.name.StartsWith("Bramble"))
                {
                    Bounds bounds = child.GetComponent<Renderer>().bounds;
                    camera.transform.position = bounds.center + new Vector3(-4f, 3f, -5f);
                    camera.transform.LookAt(bounds.center);
                    Save(camera, "Logs/scene_thorns.png", 800, 600);
                    break;
                }
            }

            GameSelection.CurrentMode = previousMode;
            GameSelection.DifficultyIndex = previousDifficulty;
        }

        static void Capture(Camera camera, Transform[] holders, float yaw, Vector3 position, Vector3 target, string path, int width, int height)
        {
            foreach (Transform holder in holders)
            {
                holder.gameObject.SetActive(true);
                holder.rotation = Quaternion.Euler(0f, yaw, 0f);
            }
            camera.transform.position = position;
            camera.transform.LookAt(target);
            Save(camera, path, width, height);
        }

        static void Save(Camera camera, string path, int width, int height)
        {
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            camera.targetTexture = target;
            camera.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            RenderTexture.active = previous;
            camera.targetTexture = null;
            File.WriteAllBytes(path, image.EncodeToPNG());
            Object.DestroyImmediate(image);
            target.Release();
            Object.DestroyImmediate(target);
        }
    }
}
