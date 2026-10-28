using UnityEngine;

namespace FartRafters
{
    public static class GameSelection
    {
        public const string MenuScene = "MainMenu";
        public const string GameScene = "Game";
        public const int MinPlayers = 1;
        public const int MaxPlayers = 4;

        public enum Mode
        {
            Practice,
            Adventure
        }

        const string LevelKey = "sel.level";
        const string BotKey = "sel.bots";
        const string AvatarKey = "sel.avatar";
        const string PlayersKey = "sel.players";
        const string BotCountKey = "sel.botcount";
        const string ModeKey = "sel.mode";
        const string DifficultyKey = "sel.difficulty";
        const string BiomeKey = "sel.biome";
        const string CostumeKey = "sel.costume";
        const string RaftKey = "sel.raft";
        const string WeatherKey = "sel.weather";
        const string BiomeVersionKey = "sel.biomever";

        static readonly int[] DefaultAvatars = { 0, 1, 2, 3 };

        static bool loaded;
        static int levelIndex;
        static int botLevelIndex = 1;
        static int playerCount = MinPlayers;
        static int botCount = MaxPlayers - MinPlayers;
        static Mode mode;
        static int difficultyIndex;
        static int biomeChoice;
        static int raftIndex;
        static int weatherChoice;
        static readonly int[] costumes = new int[MaxPlayers];
        static readonly int[] avatars = new int[MaxPlayers];

        public static int LevelIndex
        {
            get
            {
                Load();
                return levelIndex;
            }
            set
            {
                Load();
                levelIndex = Mathf.Max(0, value);
                Save();
            }
        }

        public static int BotLevelIndex
        {
            get
            {
                Load();
                return botLevelIndex;
            }
            set
            {
                Load();
                botLevelIndex = Mathf.Max(0, value);
                Save();
            }
        }

        public static int PlayerCount
        {
            get
            {
                Load();
                return playerCount;
            }
            set
            {
                Load();
                playerCount = Mathf.Clamp(value, MinPlayers, MaxPlayers);
                botCount = Mathf.Min(botCount, MaxPlayers - playerCount);
                Save();
            }
        }

        public static int MaxBots => MaxPlayers - PlayerCount;

        public static int BotCount
        {
            get
            {
                Load();
                return botCount;
            }
            set
            {
                Load();
                botCount = Mathf.Clamp(value, 0, MaxPlayers - playerCount);
                Save();
            }
        }

        public static Mode CurrentMode
        {
            get
            {
                Load();
                return mode;
            }
            set
            {
                Load();
                mode = value;
                Save();
            }
        }

        public static int DifficultyIndex
        {
            get
            {
                Load();
                return difficultyIndex;
            }
            set
            {
                Load();
                difficultyIndex = Mathf.Max(0, value);
                Save();
            }
        }

        public static int BiomeChoice
        {
            get
            {
                Load();
                return biomeChoice;
            }
            set
            {
                Load();
                biomeChoice = Mathf.Clamp(value, 0, Biomes.RandomChoice);
                Save();
            }
        }

        public static int RaftIndex
        {
            get
            {
                Load();
                return raftIndex;
            }
            set
            {
                Load();
                raftIndex = Mathf.Max(0, value);
                Save();
            }
        }

        public static int WeatherChoice
        {
            get
            {
                Load();
                return weatherChoice;
            }
            set
            {
                Load();
                weatherChoice = Mathf.Clamp(value, 0, Weathers.Count - 1);
                Save();
            }
        }

        public static int GetCostume(int player)
        {
            Load();
            return costumes[Mathf.Clamp(player, 0, MaxPlayers - 1)];
        }

        public static void SetCostume(int player, int index)
        {
            Load();
            costumes[Mathf.Clamp(player, 0, MaxPlayers - 1)] = Mathf.Max(0, index);
            Save();
        }

        public static int GetAvatar(int player)
        {
            Load();
            return avatars[Mathf.Clamp(player, 0, MaxPlayers - 1)];
        }

        public static void SetAvatar(int player, int index)
        {
            Load();
            avatars[Mathf.Clamp(player, 0, MaxPlayers - 1)] = Mathf.Max(0, index);
            Save();
        }

        static void Load()
        {
            if (loaded)
            {
                return;
            }
            loaded = true;
            levelIndex = PlayerPrefs.GetInt(LevelKey, 0);
            mode = (Mode)Mathf.Clamp(PlayerPrefs.GetInt(ModeKey, 0), 0, 1);
            difficultyIndex = Mathf.Max(0, PlayerPrefs.GetInt(DifficultyKey, 0));
            biomeChoice = Mathf.Clamp(PlayerPrefs.GetInt(BiomeKey, 0), 0, Biomes.RandomChoice);
            if (PlayerPrefs.GetInt(BiomeVersionKey, 1) < 2 && biomeChoice == Biomes.LegacyRandomChoice)
            {
                biomeChoice = Biomes.RandomChoice;
            }
            raftIndex = Mathf.Max(0, PlayerPrefs.GetInt(RaftKey, 0));
            weatherChoice = Mathf.Clamp(PlayerPrefs.GetInt(WeatherKey, 0), 0, Weathers.Count - 1);
            botLevelIndex = Mathf.Max(0, PlayerPrefs.GetInt(BotKey, 1));
            playerCount = Mathf.Clamp(PlayerPrefs.GetInt(PlayersKey, MinPlayers), MinPlayers, MaxPlayers);
            botCount = Mathf.Clamp(PlayerPrefs.GetInt(BotCountKey, MaxPlayers - playerCount), 0, MaxPlayers - playerCount);
            for (int i = 0; i < MaxPlayers; i++)
            {
                avatars[i] = PlayerPrefs.GetInt(AvatarKey + i, DefaultAvatars[i]);
                costumes[i] = Mathf.Max(0, PlayerPrefs.GetInt(CostumeKey + i, 0));
            }
        }

        static void Save()
        {
            PlayerPrefs.SetInt(LevelKey, levelIndex);
            PlayerPrefs.SetInt(ModeKey, (int)mode);
            PlayerPrefs.SetInt(DifficultyKey, difficultyIndex);
            PlayerPrefs.SetInt(BiomeKey, biomeChoice);
            PlayerPrefs.SetInt(BiomeVersionKey, 2);
            PlayerPrefs.SetInt(RaftKey, raftIndex);
            PlayerPrefs.SetInt(WeatherKey, weatherChoice);
            PlayerPrefs.SetInt(BotKey, botLevelIndex);
            PlayerPrefs.SetInt(PlayersKey, playerCount);
            PlayerPrefs.SetInt(BotCountKey, botCount);
            for (int i = 0; i < MaxPlayers; i++)
            {
                PlayerPrefs.SetInt(AvatarKey + i, avatars[i]);
                PlayerPrefs.SetInt(CostumeKey + i, costumes[i]);
            }
            PlayerPrefs.Save();
        }
    }
}
