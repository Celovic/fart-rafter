using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FartRafters
{
    [DefaultExecutionOrder(-100)]
    public class GameBootstrap : MonoBehaviour
    {
        public GameCatalog catalog;
        public CourseTheme theme;
        public CourseTheme[] themes;
        public CourseManager course;
        public Raft raft;
        public Rider[] seats;
        public PlayerBrain[] playerBrains;
        public BotBrain[] botBrains;
        public GameSession session;
        [Min(0f)] public float spawnHeight = 0.6f;

        public LevelDefinition Level { get; private set; }
        public int LevelIndex { get; private set; }
        public bool IsAdventure { get; private set; }
        public AdventureDifficulty Difficulty { get; private set; }
        public Biome Biome { get; private set; }
        public Weather Weather { get; private set; }
        public int DifficultyIndex { get; private set; }
        public int PlayerCount { get; private set; }
        public Rider[] PlayerRiders { get; private set; }
        public PlayerBrain[] ActivePlayerBrains { get; private set; }
        public bool HasNextLevel => !IsAdventure && catalog != null && catalog.levels != null && LevelIndex + 1 < catalog.levels.Length;
        public bool NextLevelUnlocked => HasNextLevel && ProgressStore.IsUnlocked(catalog, LevelIndex + 1);

        void Awake()
        {
            if (catalog == null || catalog.levels == null || catalog.levels.Length == 0 || theme == null)
            {
                Debug.LogError("[FartRafters] GameBootstrap catalog or theme missing");
                return;
            }

            StartInfo online = OnlineRoom.Active && OnlineRoom.Current.InGame ? OnlineRoom.Current.Started : null;
            bool adventure = online != null ? online.adventure : GameSelection.CurrentMode == GameSelection.Mode.Adventure;
            int crew = online != null ? online.seats.Count : Mathf.Min(seats.Length, Mathf.Clamp(GameSelection.PlayerCount, GameSelection.MinPlayers, GameSelection.MaxPlayers) + GameSelection.BotCount);
            int seed = online != null ? online.seed : BotSimulation.SeedOverride ?? System.Environment.TickCount;
            Biome = online != null ? (Biome)Mathf.Clamp(online.biome, 0, Biomes.Count - 1) : Biomes.Resolve(GameSelection.BiomeChoice, seed ^ 0x5bd1e99);
            theme = ThemeFor(Biome);
            CourseTheme.Active = theme;
            float severity;
            if (adventure && catalog.difficulties != null && catalog.difficulties.Length > 0)
            {
                IsAdventure = true;
                int difficulty = Mathf.Clamp(online != null ? online.difficulty : GameSelection.DifficultyIndex, 0, catalog.difficulties.Length - 1);
                DifficultyIndex = difficulty;
                Difficulty = CrewScaling.Apply(catalog.difficulties[difficulty], crew);
                Level = CourseGenerator.Generate(Difficulty, seed, CrewScaling.Intensity(crew));
                severity = catalog.difficulties.Length > 1 ? difficulty / (float)(catalog.difficulties.Length - 1) : 0.5f;
            }
            else
            {
                LevelIndex = Mathf.Clamp(online != null ? online.level : GameSelection.LevelIndex, 0, catalog.levels.Length - 1);
                Level = CrewScaling.Apply(catalog.levels[LevelIndex], crew);
                severity = 0.15f + LevelIndex * 0.1f;
            }
            BiomeFeatures.Apply(Level, Biome, severity, CrewScaling.Intensity(crew), seed);
            Weather = Weathers.Resolve(online != null ? online.weather : GameSelection.WeatherChoice);
            Weathers.ScaleCurrents(Level, Weather);
            CourseProfile.Setup(Level);

            BuiltCourse built = CourseBuilder.Build(Level, theme, transform);
            built.surface.waveAmplitude = theme.waveAmplitude * Weathers.WaveScale(Weather);
            built.surface.waveLength = theme.waveLength;
            built.surface.waveSpeed = theme.waveSpeed;
            BiomeEnvironment.Apply(theme, gameObject);
            WeatherSystem.Apply(gameObject, Weather, theme, course);
            course.waypoints = built.waypoints;
            course.checkpoints = built.checkpoints;
            course.spawnHeight = spawnHeight;

            raft.transform.SetPositionAndRotation(built.spawnPosition + Vector3.up * spawnHeight, built.spawnRotation);
            Buoyancy buoyancy = raft.GetComponent<Buoyancy>();
            if (buoyancy != null)
            {
                buoyancy.surface = built.surface;
            }
            RaftFall fall = raft.GetComponent<RaftFall>();
            if (fall != null)
            {
                fall.chance = Level.fallChance;
            }
            RaftVariant variant = catalog.GetRaft(online != null ? online.raft : GameSelection.RaftIndex);
            raft.ApplyVariant(variant);
            RaftHealth health = raft.GetComponent<RaftHealth>();
            if (health != null)
            {
                health.damageMultiplier = Level.damageMultiplier * raft.DamageScale;
            }

            if (online != null)
            {
                AssignOnlineSeats(online);
            }
            else
            {
                AssignSeats();
            }
            gameObject.AddComponent<ScoreTracker>().Setup(this);
            gameObject.AddComponent<RaftUnstick>().Setup(course, raft);
            gameObject.AddComponent<QuestTracker>().Setup(this, online != null ? online.raft : GameSelection.RaftIndex);
            if (IsAdventure && DifficultyIndex >= ChaseDifficulty)
            {
                ChaseAnimal.Create(course, theme, DifficultyIndex);
            }
            if (theme.raftLight)
            {
                AddRaftLight();
            }
            if (online != null)
            {
                gameObject.AddComponent<OnlineGame>().Setup(this, OnlineRoom.Current, onlineSeats);
            }
        }

        const int ChaseDifficulty = 2;

        void AddRaftLight()
        {
            var holder = new GameObject("RaftLantern");
            holder.transform.SetParent(raft.transform, false);
            holder.transform.localPosition = new Vector3(0f, 2.6f, 0f);
            var lamp = holder.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = new Color(1f, 0.82f, 0.5f);
            lamp.range = 16f;
            lamp.intensity = 2.2f;
            lamp.shadows = LightShadows.None;
            lamp.renderMode = LightRenderMode.ForcePixel;
        }

        SeatInfo[] onlineSeats;

        void OnDestroy()
        {
            CourseProfile.Clear();
        }

        CourseTheme ThemeFor(Biome biome)
        {
            if (themes != null)
            {
                foreach (CourseTheme candidate in themes)
                {
                    if (candidate != null && candidate.biome == biome)
                    {
                        return candidate;
                    }
                }
            }
            return theme;
        }

        void AssignOnlineSeats(StartInfo start)
        {
            OnlineRoom room = OnlineRoom.Current;
            bool host = room.IsHost;
            ulong me = room.LocalClientId;
            onlineSeats = new SeatInfo[seats.Length];
            var players = new List<Rider>();
            var brains = new List<PlayerBrain>();
            var bots = new List<Rider>();
            int localCount = 0;
            foreach (SeatInfo seat in start.seats)
            {
                if (!seat.bot && seat.owner == me)
                {
                    localCount++;
                }
            }
            PlayerCount = Mathf.Max(1, localCount);
            PlayerBrain.ActivePlayers = PlayerCount;

            int localIndex = 0;
            for (int i = 0; i < seats.Length; i++)
            {
                Rider rider = seats[i];
                if (rider == null)
                {
                    continue;
                }
                SeatInfo info = i < start.seats.Count ? start.seats[i] : null;
                onlineSeats[i] = info;
                PlayerBrain playerBrain = i < playerBrains.Length ? playerBrains[i] : null;
                BotBrain botBrain = i < botBrains.Length ? botBrains[i] : null;
                bool used = info != null;
                bool local = used && !info.bot && info.owner == me;
                bool bot = used && info.bot;
                if (playerBrain != null)
                {
                    playerBrain.enabled = local;
                    if (local)
                    {
                        playerBrain.playerIndex = localIndex;
                    }
                }
                if (botBrain != null)
                {
                    botBrain.enabled = bot && host;
                }
                rider.brain = local ? playerBrain : bot && host ? botBrain : null;
                rider.gameObject.SetActive(used);
                if (!used)
                {
                    continue;
                }
                int avatars = catalog.avatars != null && catalog.avatars.Length > 0 ? catalog.avatars.Length : 1;
                int code = Mathf.Abs(info.avatar);
                rider.SetAvatar(catalog.GetAvatar(code % AvatarDefinition.CostumeCode % avatars), theme, code / AvatarDefinition.CostumeCode);
                if (local)
                {
                    AvatarVisualFactory.AddPlayerMarker(rider.transform, theme, localIndex);
                    players.Add(rider);
                    brains.Add(playerBrain);
                    localIndex++;
                }
                else if (bot)
                {
                    bots.Add(rider);
                }
            }

            PlayerRiders = players.ToArray();
            ActivePlayerBrains = brains.ToArray();
            if (session != null)
            {
                session.botRiders = bots.ToArray();
                session.botLevels = catalog.botLevels;
                session.botLevelIndex = Mathf.Max(0, start.botLevel);
            }
        }

        void AssignSeats()
        {
            PlayerCount = Mathf.Clamp(GameSelection.PlayerCount, GameSelection.MinPlayers, Mathf.Min(GameSelection.MaxPlayers, seats.Length));
            PlayerBrain.ActivePlayers = PlayerCount;
            int crew = Mathf.Min(seats.Length, PlayerCount + GameSelection.BotCount);

            var players = new List<Rider>();
            var brains = new List<PlayerBrain>();
            var bots = new List<Rider>();

            for (int i = 0; i < seats.Length; i++)
            {
                Rider rider = seats[i];
                if (rider == null)
                {
                    continue;
                }

                bool human = i < PlayerCount;
                bool bot = !human && i < crew;
                PlayerBrain playerBrain = i < playerBrains.Length ? playerBrains[i] : null;
                BotBrain botBrain = i < botBrains.Length ? botBrains[i] : null;
                if (playerBrain != null)
                {
                    playerBrain.playerIndex = i;
                    playerBrain.enabled = human;
                }
                if (botBrain != null)
                {
                    botBrain.enabled = bot;
                }
                rider.brain = human ? (RiderBrain)playerBrain : botBrain;
                rider.gameObject.SetActive(human || bot);

                if (human)
                {
                    rider.SetAvatar(catalog.GetAvatar(GameSelection.GetAvatar(i)), theme, GameSelection.GetCostume(i));
                    AvatarVisualFactory.AddPlayerMarker(rider.transform, theme, i);
                    players.Add(rider);
                    brains.Add(playerBrain);
                }
                else if (bot)
                {
                    AvatarDefinition botAvatar = catalog.GetAvatar(Random.Range(0, catalog.avatars.Length));
                    rider.SetAvatar(botAvatar, theme, botAvatar != null ? Random.Range(0, botAvatar.CostumeCount) : 0);
                    bots.Add(rider);
                }
            }

            PlayerRiders = players.ToArray();
            ActivePlayerBrains = brains.ToArray();

            if (session != null)
            {
                session.botRiders = bots.ToArray();
                session.botLevels = catalog.botLevels;
                session.botLevelIndex = GameSelection.BotLevelIndex;
            }
        }

        public void LoadNextLevel()
        {
            if (!HasNextLevel)
            {
                return;
            }
            GameSelection.LevelIndex = LevelIndex + 1;
            SceneManager.LoadScene(GameSelection.GameScene);
        }

        public void LoadMenu()
        {
            SceneManager.LoadScene(GameSelection.MenuScene);
        }
    }
}
