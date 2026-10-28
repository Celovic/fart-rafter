using UnityEngine;

namespace FartRafters
{
    public enum Weather
    {
        Clear,
        Rain,
        Fog,
        Storm
    }

    public static class Weathers
    {
        public const int Count = 4;

        static readonly string[] Names = { "Açık", "Yağmur", "Sis", "Fırtına" };
        static readonly string[] Effects =
        {
            "Güneşli, sakin nehir",
            "Akıntı ve dalgalar güçlenir",
            "Görüş mesafesi kısalır",
            "Rüzgar salı iter, şimşek çakar"
        };

        public static Weather Resolve(int choice)
        {
            return (Weather)Mathf.Clamp(choice, 0, Count - 1);
        }

        public static string Name(int choice)
        {
            return Names[(int)Resolve(choice)];
        }

        public static string Effect(int choice)
        {
            return Effects[(int)Resolve(choice)];
        }

        public static float CurrentScale(Weather weather)
        {
            switch (weather)
            {
                case Weather.Rain:
                    return 1.15f;
                case Weather.Storm:
                    return 1.2f;
                default:
                    return 1f;
            }
        }

        public static float WaveScale(Weather weather)
        {
            switch (weather)
            {
                case Weather.Rain:
                    return 1.25f;
                case Weather.Storm:
                    return 1.7f;
                default:
                    return 1f;
            }
        }

        public static void ScaleCurrents(LevelDefinition level, Weather weather)
        {
            float scale = CurrentScale(weather);
            if (level == null || level.currents == null || Mathf.Approximately(scale, 1f))
            {
                return;
            }
            for (int i = 0; i < level.currents.Length; i++)
            {
                level.currents[i].strength *= scale;
            }
        }
    }
}
