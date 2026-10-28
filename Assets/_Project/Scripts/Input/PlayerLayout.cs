using UnityEngine;

namespace FartRafters
{
    public static class PlayerLayout
    {
        public static readonly Color[] Colors =
        {
            new Color(0.95f, 0.35f, 0.3f),
            new Color(0.3f, 0.6f, 1f),
            new Color(0.35f, 0.85f, 0.4f),
            new Color(1f, 0.8f, 0.2f)
        };

        public static Color ColorOf(int player)
        {
            return Colors[Mathf.Clamp(player, 0, Colors.Length - 1)];
        }

        public static bool IsLeft(int player)
        {
            return player % 2 == 0;
        }

        public static bool IsTop(int player, int playerCount)
        {
            return playerCount > 2 && player >= 2;
        }

        public static Rect Zone(int player, int playerCount)
        {
            if (playerCount <= 1)
            {
                return new Rect(0f, 0f, 1f, 1f);
            }
            float x = IsLeft(player) ? 0f : 0.5f;
            if (playerCount <= 2)
            {
                return new Rect(x, 0f, 0.5f, 1f);
            }
            return new Rect(x, IsTop(player, playerCount) ? 0.5f : 0f, 0.5f, 0.5f);
        }

        public static string SeatName(int player, int playerCount)
        {
            if (playerCount <= 1)
            {
                return "";
            }
            if (playerCount <= 2)
            {
                return IsLeft(player) ? "sol" : "sağ";
            }
            string side = IsLeft(player) ? "sol" : "sağ";
            return side + (IsTop(player, playerCount) ? " üst" : " alt");
        }
    }
}
