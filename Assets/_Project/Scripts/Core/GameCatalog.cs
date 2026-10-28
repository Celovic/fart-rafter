using UnityEngine;

namespace FartRafters
{
    [CreateAssetMenu(menuName = "FartRaft/Oyun Kataloğu", fileName = "GameCatalog")]
    public class GameCatalog : ScriptableObject
    {
        public LevelDefinition[] levels;
        public AvatarDefinition[] avatars;
        public BotDifficulty[] botLevels;
        public AdventureDifficulty[] difficulties;
        public RaftVariant[] rafts;
        public RaftTuning tuning;
        public bool unlockAllLevels;

        public LevelDefinition GetLevel(int index)
        {
            if (levels == null || levels.Length == 0)
            {
                return null;
            }
            return levels[Mathf.Clamp(index, 0, levels.Length - 1)];
        }

        public int RaftCount => rafts != null && rafts.Length > 0 ? rafts.Length : 1;

        public RaftVariant GetRaft(int index)
        {
            if (rafts == null || rafts.Length == 0)
            {
                return null;
            }
            return rafts[Mathf.Clamp(index, 0, rafts.Length - 1)];
        }

        public int NextRaft(int current)
        {
            int count = rafts != null ? rafts.Length : 0;
            if (count == 0)
            {
                return 0;
            }
            var order = new System.Collections.Generic.List<int>();
            for (int i = 1; i < count; i++)
            {
                order.Add(i);
            }
            order.Sort((a, b) => rafts[a].durability != rafts[b].durability ? rafts[a].durability.CompareTo(rafts[b].durability) : a.CompareTo(b));
            order.Insert(0, 0);
            int at = order.IndexOf(current);
            return order[(at + 1) % order.Count];
        }

        public AvatarDefinition GetAvatar(int index)
        {
            if (avatars == null || avatars.Length == 0)
            {
                return null;
            }
            return avatars[(index % avatars.Length + avatars.Length) % avatars.Length];
        }
    }
}
