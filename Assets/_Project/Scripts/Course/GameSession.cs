using UnityEngine;

namespace FartRafters
{
    public class GameSession : MonoBehaviour
    {
        public Rider[] botRiders;
        public BotDifficulty[] botLevels;
        [Min(-1)] public int botLevelIndex = 1;

        public BotDifficulty CurrentBotLevel =>
            botLevels != null && botLevelIndex >= 0 && botLevelIndex < botLevels.Length ? botLevels[botLevelIndex] : null;

        public BotDifficulty AutopilotLevel
        {
            get
            {
                if (CurrentBotLevel != null)
                {
                    return CurrentBotLevel;
                }
                if (botLevels == null || botLevels.Length == 0)
                {
                    return null;
                }
                return botLevels[Mathf.Min(2, botLevels.Length - 1)];
            }
        }

        void Start()
        {
            Apply();
        }

        public void CycleBotLevel()
        {
            int count = botLevels != null ? botLevels.Length : 0;
            botLevelIndex = botLevelIndex + 1 >= count ? -1 : botLevelIndex + 1;
            Apply();
        }

        void Apply()
        {
            BotDifficulty level = CurrentBotLevel;
            foreach (Rider rider in botRiders)
            {
                if (rider == null)
                {
                    continue;
                }
                if (rider.brain is BotBrain bot)
                {
                    bot.difficulty = level;
                }
                rider.gameObject.SetActive(level != null);
            }
        }
    }
}
