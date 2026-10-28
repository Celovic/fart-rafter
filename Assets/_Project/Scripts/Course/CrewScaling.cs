using UnityEngine;

namespace FartRafters
{
    public static class CrewScaling
    {
        public const float SoloIntensity = 0.75f;

        public static float Intensity(int crew)
        {
            float f = 0.7f + 0.1f * (Mathf.Clamp(crew, 1, 4) - 1);
            return crew <= 1 ? SoloIntensity : f;
        }

        public static AdventureDifficulty Apply(AdventureDifficulty source, int crew)
        {
            float f = Intensity(crew);
            AdventureDifficulty scaled = Object.Instantiate(source);
            scaled.name = source.name;
            scaled.crocodileSpeed *= f;
            scaled.currentStrength *= f;
            scaled.damageMultiplier *= f;
            scaled.millSpeed *= f;
            scaled.ragingChance *= f;
            scaled.fallChance *= f;
            scaled.zigzagChance *= f;
            scaled.narrowChance *= f;
            scaled.millWeight *= f;
            scaled.rockFieldWeight *= f;
            scaled.barrelWeight *= f;
            scaled.featureSpacing /= f;
            scaled.maxRocksPerCluster = Mathf.Max(1, Mathf.RoundToInt(source.maxRocksPerCluster * f));
            return scaled;
        }

        public static LevelDefinition Apply(LevelDefinition source, int crew)
        {
            float f = Intensity(crew);
            LevelDefinition scaled = Object.Instantiate(source);
            scaled.name = source.name;
            scaled.damageMultiplier *= f;
            scaled.fallChance *= f;
            for (int i = 0; i < scaled.obstacles.Length; i++)
            {
                scaled.obstacles[i].speed *= f;
            }
            for (int i = 0; i < scaled.currents.Length; i++)
            {
                scaled.currents[i].strength *= f;
            }
            for (int i = 0; i < scaled.mills.Length; i++)
            {
                scaled.mills[i].speed *= f;
            }
            return scaled;
        }
    }
}
