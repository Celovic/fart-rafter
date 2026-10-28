using UnityEngine;

namespace FartRafters
{
    [CreateAssetMenu(menuName = "FartRaft/Online Ayarları", fileName = "OnlineConfig")]
    public class OnlineConfig : ScriptableObject
    {
        public string environment = "production";
        public string allTimeLeaderboard = "fr_alltime";
        public string weeklyLeaderboard = "fr_weekly";
        public string submitScoreFunction = "submit-score";
        public string claimRewardFunction = "claim-reward";
        public bool allowDirectScoreSubmit = true;
        public bool playGamesEnabled;
        public bool allowLanMode = true;
        public string privacyPolicyUrl = "";
        public string accountDeletionUrl = "";
        public int maxSeats = 4;
        public int maxDevices = 8;
        public ushort directPort = 7777;

        static OnlineConfig instance;

        public static OnlineConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<OnlineConfig>("OnlineConfig");
                    if (instance == null)
                    {
                        instance = CreateInstance<OnlineConfig>();
                    }
                }
                return instance;
            }
        }
    }
}
