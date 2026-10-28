using UnityEngine;
using UnityEngine.Rendering;

namespace FartRafters
{
    public class BuiltCourse
    {
        public WaterSurface surface;
        public Transform[] waypoints;
        public Checkpoint[] checkpoints;
        public Vector3 spawnPosition;
        public Quaternion spawnRotation;
    }

    public static partial class CourseBuilder
    {
        const float WallThickness = 1f;
        const float WallHeight = 3f;
        const float WallCenterY = 0.25f;
        const float ShoreFadeDistance = 2.5f;

        public static BuiltCourse Build(LevelDefinition level, CourseTheme theme, Transform root)
        {
            var built = new BuiltCourse();
            var environment = new GameObject("Environment").transform;
            environment.SetParent(root, false);

            built.surface = BuildRiverWater(level, theme, environment);
            BuildWalls(level, theme, environment);
            LandscapeBuilder.Build(level, theme, environment);
            built.waypoints = BuildWaypoints(level, root);
            built.checkpoints = BuildCheckpoints(level, theme, root, built.waypoints.Length);

            foreach (CurrentSpec current in level.currents)
            {
                BuildCurrent(current, theme, environment);
            }
            foreach (ObstacleSpec obstacle in level.obstacles)
            {
                BuildObstacle(obstacle, theme, environment);
            }
            BuildHazards(level, environment);
            BuildRocks(level, theme, environment);
            BuildThorns(level, theme, environment);
            BuildPickups(level, theme, environment);
            BuildMills(level, theme, environment);
            BuildIce(level, theme, environment);
            BuildWhirlpools(level, theme, environment);
            BuildGeysers(level, theme, environment);
            BuildBarrels(level, theme, environment);
            BuildRamps(level, theme, environment);
            BuildIcicles(level, theme, environment);
            if (theme.openSide != 0)
            {
                BuildOpenSea(level, theme, environment);
            }

            built.spawnPosition = level.Surface(level.startNode);
            built.spawnRotation = Quaternion.LookRotation(level.Direction(level.startNode));
            return built;
        }

        public static WaterMesh CreateWaterMesh(Transform parent, Vector3 center, Vector2 size, CourseTheme theme, System.Func<Vector3, float> shoreFactor)
        {
            var meshObject = new GameObject("WaterMesh", typeof(MeshFilter), typeof(MeshRenderer));
            meshObject.SetActive(false);
            meshObject.transform.SetParent(parent, false);
            meshObject.transform.position = center;
            var renderer = meshObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = theme.water;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            var waterMesh = meshObject.AddComponent<WaterMesh>();
            waterMesh.size = size;
            waterMesh.cellSize = theme.waterCellSize;
            waterMesh.shoreFactor = shoreFactor;
            meshObject.SetActive(true);
            return waterMesh;
        }

        static void BuildWalls(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            var walls = new GameObject("Walls").transform;
            walls.SetParent(parent, false);

            for (int i = 0; i < level.NodeCount - 1; i++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    GameObject wall = CreateWall(walls, level.Edge(i, s) + Vector3.up * level.Height(i), level.Edge(i + 1, s) + Vector3.up * level.Height(i + 1), s, theme);
                    if (wall != null && level.IsThorn(i, s))
                    {
                        wall.AddComponent<ThornWall>();
                    }
                }
            }

            CreateCap(walls, level, 0, -1f, theme);
            CreateCap(walls, level, level.NodeCount - 1, 1f, theme);
            foreach (MeshRenderer renderer in walls.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.enabled = false;
            }
        }

        static GameObject CreateWall(Transform parent, Vector3 from, Vector3 to, float side, CourseTheme theme)
        {
            Vector3 delta = to - from;
            float length = delta.magnitude;
            if (length < 0.01f)
            {
                return null;
            }
            Vector3 direction = delta / length;
            Vector3 perpendicular = Vector3.Cross(Vector3.up, direction).normalized;
            Vector3 center = (from + to) * 0.5f + perpendicular * (side * WallThickness * 0.5f) + Vector3.up * WallCenterY;
            return CreateBox("Wall", parent, center, Quaternion.LookRotation(direction), new Vector3(WallThickness, WallHeight, length + WallThickness), theme.wall, theme.physics);
        }

        static void CreateCap(Transform parent, LevelDefinition level, int index, float outward, CourseTheme theme)
        {
            Vector3 direction = level.Direction(index);
            Vector3 center = level.Surface(index) + direction * (outward * WallThickness * 0.5f) + Vector3.up * WallCenterY;
            CreateBox("Cap", parent, center, Quaternion.LookRotation(direction), new Vector3(level.Width(index) + WallThickness * 2f, WallHeight, WallThickness), theme.wall, theme.physics);
        }

        static Transform[] BuildWaypoints(LevelDefinition level, Transform parent)
        {
            var root = new GameObject("Waypoints").transform;
            root.SetParent(parent, false);
            int first = level.FirstWaypointNode;
            int count = Mathf.Max(1, level.FinishNode - first + 1);
            var waypoints = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                var point = new GameObject("Waypoint_" + i).transform;
                point.SetParent(root, false);
                point.position = level.Surface(first + i);
                waypoints[i] = point;
            }
            return waypoints;
        }

        static Checkpoint[] BuildCheckpoints(LevelDefinition level, CourseTheme theme, Transform parent, int waypointCount)
        {
            var root = new GameObject("Checkpoints").transform;
            root.SetParent(parent, false);
            int[] nodes = level.checkpointNodes;
            var checkpoints = new Checkpoint[nodes.Length];

            for (int i = 0; i < nodes.Length; i++)
            {
                int node = nodes[i];
                bool isStart = i == 0;
                bool isFinish = i == nodes.Length - 1;
                float width = level.Width(node);

                var go = new GameObject(isStart ? "Start" : isFinish ? "Finish" : "Checkpoint_" + i);
                go.transform.SetParent(root, false);
                go.transform.SetPositionAndRotation(level.Surface(node), Quaternion.LookRotation(level.Direction(node)));

                var trigger = go.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.center = new Vector3(0f, 0.5f, 0f);
                trigger.size = new Vector3(width, 4f, 2f);

                var checkpoint = go.AddComponent<Checkpoint>();
                checkpoint.isFinish = isFinish;
                checkpoint.nextWaypointIndex = Mathf.Clamp(node + 1 - level.FirstWaypointNode, 0, waypointCount - 1);
                checkpoint.passedMaterial = isFinish ? null : theme.checkpointPassed;
                checkpoint.flags = isFinish ? BuildFinishGate(go.transform, width, theme) : theme.gatePoleMesh != null ? BuildBambooGate(go.transform, width, theme, isStart ? theme.startLabel : theme.gateLabel) : BuildBuoyGate(go.transform, width, theme);
                checkpoints[i] = checkpoint;
            }
            return checkpoints;
        }

        static Renderer[] BuildBuoyGate(Transform gate, float width, CourseTheme theme)
        {
            var flags = new Renderer[2];
            for (int s = -1; s <= 1; s += 2)
            {
                var buoy = new GameObject("Buoy").transform;
                buoy.SetParent(gate, false);
                buoy.localPosition = new Vector3(s * (width * 0.5f - 0.7f), 0f, 0f);
                var floater = buoy.gameObject.AddComponent<WaterFloater>();
                floater.heightOffset = 0.05f;
                floater.tiltSampleDistance = 0.6f;

                CreateSculpt("Ball", buoy, theme.buoyMesh, theme.sculptMaterial, Vector3.zero, Vector3.one);

                GameObject pole = CreateVisual(PrimitiveType.Cylinder, "Pole", buoy, theme.finish);
                pole.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                pole.transform.localScale = new Vector3(0.1f, 0.95f, 0.1f);

                GameObject flag = CreateVisual(PrimitiveType.Cube, "Flag", buoy, theme.checkpoint);
                flag.transform.localPosition = new Vector3(-s * 0.42f, 1.9f, 0f);
                flag.transform.localScale = new Vector3(0.8f, 0.5f, 0.04f);
                flags[s < 0 ? 0 : 1] = flag.GetComponent<Renderer>();
            }
            return flags;
        }

        static Renderer[] BuildFinishGate(Transform gate, float width, CourseTheme theme)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                GameObject post = CreateVisual(PrimitiveType.Cylinder, "Post", gate, theme.finish);
                post.transform.localPosition = new Vector3(s * (width * 0.5f - 0.4f), 1.4f, 0f);
                post.transform.localScale = new Vector3(0.3f, 1.6f, 0.3f);

                GameObject cap = CreateVisual(PrimitiveType.Sphere, "Cap", gate, theme.checkpoint);
                cap.transform.localPosition = new Vector3(s * (width * 0.5f - 0.4f), 3.05f, 0f);
                cap.transform.localScale = Vector3.one * 0.5f;
            }

            float bannerWidth = width - 0.6f;
            GameObject banner = CreateVisual(PrimitiveType.Cube, "Banner", gate, theme.finish);
            banner.transform.localPosition = new Vector3(0f, 2.6f, 0f);
            banner.transform.localScale = new Vector3(bannerWidth, 0.7f, 0.12f);
            var checker = new Material(theme.finish) { name = "FinishChecker" };
            checker.SetTexture("_BaseMap", ProceduralTextures.Checker);
            checker.SetTextureScale("_BaseMap", new Vector2(bannerWidth / 0.7f, 1f));
            banner.GetComponent<Renderer>().sharedMaterial = checker;
            return new[] { banner.GetComponent<Renderer>() };
        }

        static void BuildCurrent(CurrentSpec spec, CourseTheme theme, Transform parent)
        {
            var go = new GameObject("CurrentZone");
            go.SetActive(false);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(Lift(spec.position), Quaternion.Euler(0f, spec.yaw, 0f));

            var trigger = go.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = spec.size;

            var zone = go.AddComponent<CurrentZone>();
            zone.direction = spec.direction;
            zone.strength = spec.strength;

            Vector3 flow = spec.direction.normalized * 4f;
            CreateBubbles(go.transform, new Vector3(0f, -0.2f, 0f), new Vector3(spec.size.x, 0.1f, spec.size.z), flow, theme.bubbles);
            var streaks = new GameObject("StreakRoot").transform;
            streaks.SetParent(go.transform, false);
            streaks.localPosition = new Vector3(0f, -0.25f, 0f);
            EffectFactory.AddCurrentStreaks(streaks, spec.size, flow, theme.bubbles);
            go.SetActive(true);
        }

        static void BuildObstacle(ObstacleSpec spec, CourseTheme theme, Transform parent)
        {
            var obstacle = new GameObject("MovingObstacle");
            obstacle.SetActive(false);
            obstacle.transform.SetParent(parent, false);
            Vector3 axis = spec.travel.sqrMagnitude > 0.0001f ? spec.travel.normalized : Vector3.right;
            obstacle.transform.SetPositionAndRotation(Lift(spec.position), Quaternion.LookRotation(axis, Vector3.up));

            var collider = obstacle.AddComponent<BoxCollider>();
            collider.size = new Vector3(Mathf.Min(spec.size.x, spec.size.z), spec.size.y, Mathf.Max(spec.size.x, spec.size.z));
            collider.sharedMaterial = theme.physics;

            var body = obstacle.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            var mover = obstacle.AddComponent<MovingObstacle>();
            mover.travel = spec.travel;
            mover.speed = spec.speed;
            mover.kind = spec.kind;
            mover.lungeAngle = spec.kind == ObstacleKind.Predator ? theme.predatorLunge : 0f;
            if (spec.kind == ObstacleKind.Drift)
            {
                obstacle.AddComponent<DamageSource>().multiplier = 1.1f;
            }
            mover.visual = BuildCrocodile(obstacle.transform, spec, theme);
            if (spec.kind == ObstacleKind.Predator && Mathf.Abs(theme.predatorSpin) > 0.01f && mover.visual.childCount > 0)
            {
                Spin spin = mover.visual.GetChild(0).gameObject.AddComponent<Spin>();
                spin.axis = Vector3.forward;
                spin.speed = theme.predatorSpin;
            }
            obstacle.SetActive(true);
        }

        static Transform BuildCrocodile(Transform parent, ObstacleSpec spec, CourseTheme theme)
        {
            var croc = new GameObject("Crocodile").transform;
            croc.SetParent(parent, false);
            croc.localPosition = new Vector3(0f, -0.25f, 0f);
            Vector3 travel = spec.travel.sqrMagnitude > 0.0001f ? spec.travel.normalized : Vector3.right;
            croc.rotation = Quaternion.LookRotation(travel, Vector3.up);

            float length = Mathf.Max(spec.size.x, spec.size.z);
            float depth = Mathf.Min(spec.size.x, spec.size.z);
            Mesh mesh = theme.crocodileMesh;
            float meshLength = theme.predatorLength;
            if (spec.kind == ObstacleKind.Drift && theme.driftMesh != null)
            {
                mesh = theme.driftMesh;
                meshLength = theme.driftLength;
                croc.localPosition = new Vector3(0f, -0.1f, 0f);
            }
            else if (spec.kind == ObstacleKind.Eel && theme.eelMesh != null)
            {
                mesh = theme.eelMesh;
                meshLength = theme.eelLength;
                croc.localPosition = new Vector3(0f, -0.15f, 0f);
            }
            Vector3 scale = meshLength > 0f ? Vector3.one * (length * 1.05f / meshLength) : new Vector3(depth * 1.1f, spec.size.y * 1.1f, length * 1.05f);
            GameObject body = CreateSculpt("Body", croc, mesh, theme.sculptMaterial, Vector3.zero, scale);
            if (spec.kind == ObstacleKind.Drift)
            {
                Spin roll = body.AddComponent<Spin>();
                roll.axis = Vector3.forward;
                roll.speed = 35f;
            }
            else if (spec.kind == ObstacleKind.Eel)
            {
                body.AddComponent<Wiggle>();
            }
            return croc;
        }

        public static GameObject CreateSculpt(string name, Transform parent, Mesh mesh, Material material, Vector3 position, Vector3 scale)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        static void BuildHazards(LevelDefinition level, Transform parent)
        {
            var root = new GameObject("Hazards").transform;
            root.SetParent(parent, false);
            foreach (HazardSpec spec in level.hazards)
            {
                var go = new GameObject("Hazard");
                go.transform.SetParent(root, false);
                go.transform.position = spec.position;
                go.AddComponent<CourseHazard>().radius = spec.radius;
            }
        }

        static ParticleSystem CreateBubbles(Transform parent, Vector3 localPosition, Vector3 boxSize, Vector3 velocity, Material material)
        {
            var go = new GameObject("Bubbles");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;

            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = system.main;
            main.loop = true;
            main.startLifetime = 2.5f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startColor = new Color(1f, 1f, 1f, 0.7f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 300;
            main.playOnAwake = true;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 45f;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = boxSize;

            ParticleSystem.VelocityOverLifetimeModule motion = system.velocityOverLifetime;
            motion.enabled = true;
            motion.space = ParticleSystemSimulationSpace.World;
            motion.x = new ParticleSystem.MinMaxCurve(velocity.x);
            motion.y = new ParticleSystem.MinMaxCurve(velocity.y);
            motion.z = new ParticleSystem.MinMaxCurve(velocity.z);

            ParticleSystem.ColorOverLifetimeModule color = system.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;

            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
            system.Play();
            return system;
        }

        static void BuildRocks(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            if (level.rocks == null || level.rocks.Length == 0)
            {
                return;
            }
            var root = new GameObject("RiverRocks").transform;
            root.SetParent(parent, false);
            var random = new System.Random(CourseGeometry.StableHash(level.id) + 31);
            var batch = new MeshBatch();
            foreach (RockSpec rock in level.rocks)
            {
                var go = new GameObject("Rock");
                go.transform.SetParent(root, false);
                go.transform.position = Lift(new Vector3(rock.position.x, 0f, rock.position.z));
                var collider = go.AddComponent<CapsuleCollider>();
                collider.radius = rock.radius;
                collider.height = 4f;
                collider.center = new Vector3(0f, 0.5f, 0f);
                collider.sharedMaterial = theme.rockPhysics != null ? theme.rockPhysics : theme.physics;
                go.AddComponent<DamageSource>().multiplier = 1.6f;

                if (theme.rockMeshes != null && theme.rockMeshes.Length > 0 && theme.rockMaterial != null)
                {
                    float scale = rock.radius / 1.55f;
                    Mesh mesh = theme.rockMeshes[random.Next(theme.rockMeshes.Length)];
                    var rotation = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);
                    batch.Add(mesh, theme.rockMaterial, go.transform.position + Vector3.down * (0.35f * scale), rotation, new Vector3(scale, scale * 1.1f, scale));
                }
                else if (theme.rockModels != null && theme.rockModels.Length > 0)
                {
                    float scale = rock.radius / 1.55f;
                    GameObject model = theme.rockModels[random.Next(theme.rockModels.Length)];
                    var rotation = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);
                    batch.AddModel(model, go.transform.position + Vector3.down * (0.35f * scale), rotation, new Vector3(scale, scale * 1.1f, scale));
                }
            }
            batch.Build(root, "RockVisual", true);
        }

        static void BuildThorns(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            if (level.thorns == null || level.thorns.Length == 0 || theme.brambleMesh == null)
            {
                return;
            }
            var root = new GameObject("Thorns").transform;
            root.SetParent(parent, false);
            var random = new System.Random(CourseGeometry.StableHash(level.id) + 57);
            var batch = new MeshBatch(24f);
            foreach (ThornSpec thorn in level.thorns)
            {
                float side = Mathf.Sign(thorn.side);
                for (int i = thorn.startNode; i < thorn.endNode && i < level.NodeCount - 1; i++)
                {
                    Vector3 a = level.Edge(i, side);
                    Vector3 b = level.Edge(i + 1, side);
                    float length = Vector3.Distance(a, b);
                    Vector3 direction = (b - a) / Mathf.Max(0.01f, length);
                    Vector3 outward = Vector3.Cross(Vector3.up, direction).normalized * side;
                    int count = Mathf.Max(1, Mathf.CeilToInt(length / 1.7f));
                    for (int k = 0; k < count; k++)
                    {
                        Vector3 position = Vector3.Lerp(a, b, (k + 0.5f) / count) + outward * 0.1f;
                        position.y = 0.75f + CourseProfile.Height(position.x, position.z);
                        var rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, (float)random.NextDouble() * 30f - 15f, 0f);
                        float scale = 1.15f + (float)random.NextDouble() * 0.35f;
                        batch.Add(theme.brambleMesh, theme.thornMaterial != null ? theme.thornMaterial : theme.foliageMaterial != null ? theme.foliageMaterial : theme.sculptMaterial, position, rotation, Vector3.one * scale);
                    }
                }
            }
            batch.Build(root, "Bramble", true);
        }

        static void BuildMills(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            if (level.mills == null || level.mills.Length == 0)
            {
                return;
            }
            const float MeshRadius = 4f;
            var root = new GameObject("Mills").transform;
            root.SetParent(parent, false);
            foreach (MillSpec spec in level.mills)
            {
                if (theme.spinnerMesh != null)
                {
                    BuildSpinner(spec, theme, root);
                    continue;
                }
                var mill = new GameObject("Mill").transform;
                mill.SetParent(root, false);
                mill.SetPositionAndRotation(Lift(new Vector3(spec.position.x, 0f, spec.position.z)), Quaternion.Euler(0f, spec.yaw, 0f));
                float hub = spec.radius - 0.75f;

                for (int s = -1; s <= 1; s += 2)
                {
                    GameObject post = CreateVisual(PrimitiveType.Cylinder, "Post", mill, theme.raftLog);
                    post.transform.localPosition = new Vector3(0f, hub * 0.5f - 0.6f, s * 1.1f);
                    post.transform.localScale = new Vector3(0.5f, hub * 0.5f + 0.6f, 0.5f);
                    var collider = post.AddComponent<CapsuleCollider>();
                    collider.radius = 0.5f;
                    collider.height = 2f;
                    collider.sharedMaterial = theme.physics;
                }
                GameObject axle = CreateVisual(PrimitiveType.Cylinder, "Axle", mill, theme.raftLog);
                axle.transform.localPosition = new Vector3(0f, hub, 0f);
                axle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                axle.transform.localScale = new Vector3(0.35f, 1.25f, 0.35f);

                var wheel = new GameObject("Wheel");
                wheel.SetActive(false);
                wheel.transform.SetParent(mill, false);
                wheel.transform.localPosition = new Vector3(0f, hub, 0f);
                wheel.AddComponent<Rigidbody>();
                wheel.AddComponent<MillWheel>().speed = spec.speed;
                wheel.AddComponent<DamageSource>().multiplier = 1.4f;
                float scale = spec.radius / MeshRadius;
                Material wheelMaterial = theme.millMaterial != null ? theme.millMaterial : theme.raftMaterial != null ? theme.raftMaterial : theme.sculptMaterial;
                CreateSculpt("WheelVisual", wheel.transform, theme.millWheelMesh, wheelMaterial, Vector3.zero, Vector3.one * scale);
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * 45f;
                    var paddle = new GameObject("Paddle").transform;
                    paddle.SetParent(wheel.transform, false);
                    paddle.localRotation = Quaternion.Euler(0f, 0f, angle);
                    paddle.localPosition = paddle.localRotation * new Vector3(spec.radius * 0.8f, 0f, 0f);
                    var box = paddle.gameObject.AddComponent<BoxCollider>();
                    box.size = new Vector3(spec.radius * 0.4f, 0.35f, 1.5f);
                    box.sharedMaterial = theme.physics;
                }
                wheel.SetActive(true);
            }
        }

        static void BuildPickups(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            if (level.pickups == null || level.pickups.Length == 0)
            {
                return;
            }
            var root = new GameObject("Pickups").transform;
            root.SetParent(parent, false);
            foreach (PickupSpec spec in level.pickups)
            {
                var go = new GameObject("Pickup_" + spec.kind);
                go.SetActive(false);
                go.transform.SetParent(root, false);
                go.transform.position = Lift(spec.position);
                var trigger = go.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = 1.6f;
                var floater = go.AddComponent<WaterFloater>();
                floater.heightOffset = 0.35f;
                floater.tiltSampleDistance = 0f;

                var pickup = go.AddComponent<Pickup>();
                pickup.kind = spec.kind;
                pickup.index = root.childCount - 1;
                Mesh mesh = theme.pickupMeshes != null && (int)spec.kind < theme.pickupMeshes.Length ? theme.pickupMeshes[(int)spec.kind] : null;
                GameObject visual = CreateSculpt("Visual", go.transform, mesh, theme.sculptMaterial, Vector3.zero, Vector3.one);
                pickup.visual = visual.transform;
                pickup.burst = EffectFactory.CreateConfetti(go.transform, theme.confetti);
                go.SetActive(true);
            }
        }

        static WaterSurface BuildRiverWater(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            var waterRoot = new GameObject("Water");
            waterRoot.transform.SetParent(parent, false);
            WaterSurface surface = waterRoot.AddComponent<WaterSurface>();

            float margin = theme.elevated ? 0.25f : 8f;
            float step = Mathf.Max(0.75f, theme.waterCellSize);
            var centers = new System.Collections.Generic.List<Vector3>();
            var rights = new System.Collections.Generic.List<Vector3>();
            var halfWidths = new System.Collections.Generic.List<float>();

            Vector3 startDirection = level.Direction(0);
            AddSample(centers, rights, halfWidths, level.Surface(0) - startDirection * margin, startDirection, level.Width(0) * 0.5f + margin);
            for (int i = 0; i < level.NodeCount - 1; i++)
            {
                Vector3 a = level.Surface(i);
                Vector3 b = level.Surface(i + 1);
                int samples = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(a, b) / step));
                for (int k = 0; k < samples; k++)
                {
                    float t = k / (float)samples;
                    Vector3 direction = Vector3.Slerp(level.Direction(i), level.Direction(i + 1), t).normalized;
                    AddSample(centers, rights, halfWidths, Vector3.Lerp(a, b, t), direction, Mathf.Lerp(level.Width(i), level.Width(i + 1), t) * 0.5f + margin);
                }
            }
            int last = level.NodeCount - 1;
            Vector3 endDirection = level.Direction(last);
            AddSample(centers, rights, halfWidths, level.Surface(last), endDirection, level.Width(last) * 0.5f + margin);
            AddSample(centers, rights, halfWidths, level.Surface(last) + endDirection * margin, endDirection, level.Width(last) * 0.5f + margin);

            float maxHalf = 0f;
            foreach (float half in halfWidths)
            {
                maxHalf = Mathf.Max(maxHalf, half);
            }
            int across = Mathf.Max(2, Mathf.CeilToInt(maxHalf * 2f / step) + 1);
            var vertices = new Vector3[centers.Count * across];
            var normals = new Vector3[vertices.Length];
            var colors = new Color[vertices.Length];
            var bed = new Vector3[vertices.Length];
            var bedColors = new Color[vertices.Length];
            var bedUv = new Vector2[vertices.Length];
            for (int r = 0; r < centers.Count; r++)
            {
                for (int c = 0; c < across; c++)
                {
                    float u = c / (float)(across - 1) * 2f - 1f;
                    Vector3 point = centers[r] + rights[r] * (u * halfWidths[r]);
                    point.y = centers[r].y;
                    int index = r * across + c;
                    vertices[index] = point;
                    normals[index] = Vector3.up;
                    float inside = CourseGeometry.WallDistance(level, point);
                    bool open = theme.openSide != 0 && u * theme.openSide > 0f;
                    float shore = open ? 0f : Mathf.Clamp01(1f - inside / ShoreFadeDistance);
                    float far = open ? Mathf.Clamp01(-inside / 5f) : 0f;
                    Vector3 flow = Vector3.Cross(rights[r], Vector3.up) * (1f - far);
                    colors[index] = new Color(shore, flow.x * 0.5f + 0.5f, flow.z * 0.5f + 0.5f, 1f);
                    float depth = open ? -1.9f - Mathf.Clamp01(-inside / 6f) * 0.8f : Mathf.Lerp(-0.7f, -1.9f, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.2f, 4.5f, inside)));
                    bed[index] = new Vector3(point.x, point.y + depth, point.z);
                    colors[index].a = Mathf.Lerp(Mathf.Clamp01(-depth / 3f), 1f, far);
                    bedColors[index] = new Color(0f, Mathf.PerlinNoise(point.x * 0.15f, point.z * 0.15f), 1f, 1f);
                    bedUv[index] = new Vector2(point.x, point.z) * 0.45f;
                }
            }

            var triangles = new int[(centers.Count - 1) * (across - 1) * 6];
            int t2 = 0;
            for (int r = 0; r < centers.Count - 1; r++)
            {
                for (int c = 0; c < across - 1; c++)
                {
                    int i = r * across + c;
                    triangles[t2++] = i;
                    triangles[t2++] = i + across;
                    triangles[t2++] = i + 1;
                    triangles[t2++] = i + 1;
                    triangles[t2++] = i + across;
                    triangles[t2++] = i + across + 1;
                }
            }

            var mesh = new Mesh { name = "RiverWater" };
            mesh.indexFormat = vertices.Length > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            Bounds bounds = mesh.bounds;
            bounds.Expand(new Vector3(0f, 4f, 0f));
            mesh.bounds = bounds;

            var meshObject = new GameObject("RiverWater", typeof(MeshFilter), typeof(MeshRenderer));
            meshObject.transform.SetParent(waterRoot.transform, false);
            meshObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = meshObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = theme.water;
            renderer.shadowCastingMode = ShadowCastingMode.Off;

            var bedMesh = new Mesh { name = "RiverBed" };
            bedMesh.indexFormat = mesh.indexFormat;
            bedMesh.vertices = bed;
            bedMesh.colors = bedColors;
            bedMesh.uv = bedUv;
            bedMesh.triangles = triangles;
            bedMesh.RecalculateNormals();
            bedMesh.RecalculateTangents();
            bedMesh.RecalculateBounds();
            var bedObject = new GameObject("RiverBed", typeof(MeshFilter), typeof(MeshRenderer));
            bedObject.transform.SetParent(waterRoot.transform, false);
            bedObject.GetComponent<MeshFilter>().sharedMesh = bedMesh;
            var bedRenderer = bedObject.GetComponent<MeshRenderer>();
            bedRenderer.sharedMaterial = theme.bank != null ? theme.bank : theme.wall;
            bedRenderer.shadowCastingMode = ShadowCastingMode.Off;
            return surface;
        }

        static Vector3 Lift(Vector3 position)
        {
            position.y += CourseProfile.Height(position.x, position.z);
            return position;
        }

        static void AddSample(System.Collections.Generic.List<Vector3> centers, System.Collections.Generic.List<Vector3> rights, System.Collections.Generic.List<float> halfWidths, Vector3 center, Vector3 direction, float half)
        {
            centers.Add(center);
            rights.Add(Vector3.Cross(Vector3.up, direction).normalized);
            halfWidths.Add(half);
        }

        static GameObject CreateBox(string name, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale, Material material, PhysicsMaterial physics)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.SetPositionAndRotation(position, rotation);
            box.transform.localScale = scale;
            box.GetComponent<MeshRenderer>().sharedMaterial = material;
            box.GetComponent<Collider>().sharedMaterial = physics;
            return box;
        }

        public static GameObject CreateVisual(PrimitiveType type, string name, Transform parent, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }
    }
}
