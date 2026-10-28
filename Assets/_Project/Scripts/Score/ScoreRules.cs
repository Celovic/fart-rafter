using UnityEngine;

namespace FartRafters
{
    public static class ScoreRules
    {
        public const float TimeMax = 45f;
        public const float SafetyMax = 30f;
        public const float GasMax = 15f;
        public const float BonusMax = 10f;

        public const float DamagePerHealth = 0.12f;
        public const float Crocodile = 3.5f;
        public const float Broken = 4.5f;
        public const float Fall = 1.5f;
        public const float Elimination = 6f;
        public const float CulpritShare = 0.25f;
        public const float StuckPerSecond = 0.4f;
        public const float StuckSpeed = 0.5f;
        public const float StuckDelay = 2f;

        public const float PickupBonus = 0.8f;
        public const float CleanCheckpointBonus = 0.6f;
        public const float RescueBonus = 2f;
        public const float NoRespawnBonus = 5f;

        public const float EfficiencyTarget = 0.85f;
        public const float BlameWindow = 2f;
        public const float FallBlameWindow = 1.5f;
        public const int PracticeFullRuns = 3;
        public const float PracticeRepeatFactor = 0.2f;

        static readonly float[] AdventureFactors = { 1f, 1.4f, 2f, 4f };
        static readonly float[] PracticeFactors = { 0.3f, 0.35f, 0.4f, 0.45f, 0.5f };

        public static float DifficultyFactor(bool adventure, int index)
        {
            float[] table = adventure ? AdventureFactors : PracticeFactors;
            return table[Mathf.Clamp(index, 0, table.Length - 1)];
        }

        public static float CrewFactor(int humans)
        {
            switch (Mathf.Clamp(humans, 1, 4))
            {
                case 1:
                    return 1.15f;
                case 2:
                    return 1f;
                case 3:
                    return 0.95f;
                default:
                    return 0.9f;
            }
        }

        public static float BotPenalty(BotLevel level)
        {
            switch (level)
            {
                case BotLevel.Medium:
                    return 0.05f;
                case BotLevel.Good:
                    return 0.1f;
                case BotLevel.Super:
                    return 0.2f;
                default:
                    return 0f;
            }
        }

        public static float BotFactor(int bots, BotLevel level)
        {
            return Mathf.Max(0.5f, 1f - bots * BotPenalty(level));
        }

        public static float ParSpeedFactor(int crew)
        {
            switch (Mathf.Clamp(crew, 1, 4))
            {
                case 1:
                    return 1f;
                case 2:
                    return 0.94f;
                case 3:
                    return 0.89f;
                default:
                    return 0.85f;
            }
        }

        public static float TimeScore(float par, float time)
        {
            if (time <= 0.01f)
            {
                return 0f;
            }
            return TimeMax * Mathf.Clamp01(par / time - 0.2f);
        }

        public static float ReferenceGasPer100(float mass, float drag, float thrust, float crewScale, int crew)
        {
            float team = 100f * mass * drag / Mathf.Max(1f, thrust * crewScale);
            return team * 1.3f / Mathf.Max(1, crew);
        }
    }
}
