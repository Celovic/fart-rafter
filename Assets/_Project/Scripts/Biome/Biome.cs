using UnityEngine;

namespace FartRafters
{
    public enum Biome
    {
        River,
        Polar,
        Ocean,
        Fun,
        Lava,
        Night,
        Desert,
        Swamp
    }

    public static class Biomes
    {
        public const int Count = 8;
        public const int RandomChoice = Count;
        public const int LegacyRandomChoice = 4;

        static readonly string[] Names = { "Nehir", "Kutup", "Okyanus", "Eğlence", "Lav Nehri", "Gece Ormanı", "Çöl Kanyonu", "Bataklık" };

        public static string Name(Biome biome)
        {
            return Names[Mathf.Clamp((int)biome, 0, Count - 1)];
        }

        public static string ChoiceName(int choice)
        {
            return choice >= Count || choice < 0 ? "Rastgele" : Names[choice];
        }

        public static Biome Resolve(int choice, int seed)
        {
            if (choice >= 0 && choice < Count)
            {
                return (Biome)choice;
            }
            return (Biome)(new System.Random(seed).Next(Count));
        }
    }
}
