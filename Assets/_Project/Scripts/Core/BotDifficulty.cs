using UnityEngine;

namespace FartRafters
{
    public enum BotLevel
    {
        Bad,
        Medium,
        Good,
        Super
    }

    [CreateAssetMenu(menuName = "FartRaft/Bot Seviyesi", fileName = "BotDifficulty")]
    public class BotDifficulty : ScriptableObject
    {
        public BotLevel level = BotLevel.Medium;
        public string displayName = "Orta";

        [Header("Sürüş")]
        [Min(0.5f)] public float cruiseSpeed = 4.5f;
        [Min(0.5f)] public float minCruiseSpeed = 2.5f;
        [Min(0.02f)] public float decisionInterval = 0.15f;
        [Min(0f)] public float speedTolerance = 1f;
        [Range(0f, 1f)] public float minGasToStart = 0.35f;
        [Range(0f, 1f)] public float gasReserve;
        [Range(0f, 90f)] public float aimNoiseDegrees = 0f;
        [Range(5f, 180f)] public float facingTolerance = 60f;
        [Min(0.1f)] public float responsiveness = 1.6f;

        [Header("Hatalar")]
        [Min(0f)] public float mistakeChancePerSecond = 0.12f;
        public Vector2 mistakeDuration = new Vector2(0.3f, 0.7f);
        [Min(0f)] public float hazardMistakeChancePerSecond = 0f;

        [Header("Öngörü")]
        [Range(0f, 1f)] public float cornerLookAhead = 0f;
        [Min(0f)] public float cornerLookAheadDistance = 10f;
        public bool avoidObstacles;
        [Min(0f)] public float obstacleLookDistance = 9f;
        [Min(0f)] public float obstaclePredictionSeconds = 1f;
        [Range(0f, 1f)] public float obstacleGapFraction = 0.9f;

        [Header("Rota planı")]
        [Min(0f)] public float planLookAhead;
        [Min(3)] public int planLanes = 9;
        [Min(0f)] public float planMargin = 1.2f;
        public bool predictCrocodiles;
        public bool readCurrents;
        public bool collectPickups;
        [Min(1f)] public float dangerSlowdown = 40f;

        [Header("Ekip")]
        public bool waitForSwimmers;
        public bool timeCrocodiles;
        public bool chargeCurrents;
        [Range(0f, 1f)] public float teamwork = 1f;
    }
}
