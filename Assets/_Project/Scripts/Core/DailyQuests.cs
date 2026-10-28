using System;
using System.Collections.Generic;
using UnityEngine;

namespace FartRafters
{
    public enum QuestKind
    {
        FinishRaces,
        FinishBiome,
        FinishHard,
        CollectPickups,
        PassCheckpoints,
        FartSeconds,
        CleanFinish,
        CostumeFinish,
        RaftFinish
    }

    public struct Quest
    {
        public QuestKind kind;
        public int target;
        public int arg;
        public int reward;
        public int progress;
        public bool claimed;
        public bool Done => progress >= target;
    }

    public struct RunSummary
    {
        public bool finished;
        public Biome biome;
        public bool adventure;
        public int difficulty;
        public bool broken;
        public int costume;
        public int raft;
    }

    public static class DailyQuests
    {
        public const int Count = 3;
        public const int AllDoneBonus = 300;
        static readonly int[] StreakRewards = { 50, 75, 100, 125, 150, 175, 250 };
        static readonly string[] RaftNames = { "Klasik Sal", "Şişme Bot", "Ahşap Kayık", "Dev Simit", "Küvet" };

        const string DayKey = "quest.day";
        const string ProgressKey = "quest.p";
        const string ClaimKey = "quest.c";
        const string BonusKey = "quest.bonus";
        const string LoginDayKey = "login.day";
        const string LoginStreakKey = "login.streak";
        const string LoginClaimKey = "login.claimed";

        public static event Action<Quest> Completed;

        static string Today => DateTime.Now.ToString("yyyyMMdd");

        static int DaySeed(string day)
        {
            int seed = 17;
            foreach (char c in day)
            {
                seed = seed * 31 + c;
            }
            return seed;
        }

        static void Roll()
        {
            string today = Today;
            if (PlayerPrefs.GetString(DayKey, "") == today)
            {
                return;
            }
            PlayerPrefs.SetString(DayKey, today);
            for (int i = 0; i < Count; i++)
            {
                PlayerPrefs.SetInt(ProgressKey + i, 0);
                PlayerPrefs.SetInt(ClaimKey + i, 0);
            }
            PlayerPrefs.SetInt(BonusKey, 0);
            PlayerPrefs.Save();
        }

        public static Quest[] Current
        {
            get
            {
                Roll();
                var random = new System.Random(DaySeed(Today));
                var pool = new List<QuestKind>((QuestKind[])Enum.GetValues(typeof(QuestKind)));
                var quests = new Quest[Count];
                for (int i = 0; i < Count; i++)
                {
                    int pick = random.Next(pool.Count);
                    QuestKind kind = pool[pick];
                    pool.RemoveAt(pick);
                    quests[i] = Make(kind, random);
                    quests[i].progress = Mathf.Min(PlayerPrefs.GetInt(ProgressKey + i, 0), quests[i].target);
                    quests[i].claimed = PlayerPrefs.GetInt(ClaimKey + i, 0) == 1;
                }
                return quests;
            }
        }

        static Quest Make(QuestKind kind, System.Random random)
        {
            switch (kind)
            {
                case QuestKind.FinishRaces:
                    return new Quest { kind = kind, target = random.Next(2, 4), reward = 150 };
                case QuestKind.FinishBiome:
                    return new Quest { kind = kind, target = 1, arg = random.Next(Biomes.Count), reward = 120 };
                case QuestKind.FinishHard:
                    return new Quest { kind = kind, target = 1, arg = 1 + random.Next(2), reward = 200 };
                case QuestKind.CollectPickups:
                    return new Quest { kind = kind, target = 6 + random.Next(3) * 3, reward = 120 };
                case QuestKind.PassCheckpoints:
                    return new Quest { kind = kind, target = 10 + random.Next(3) * 5, reward = 130 };
                case QuestKind.FartSeconds:
                    return new Quest { kind = kind, target = 60 + random.Next(3) * 30, reward = 100 };
                case QuestKind.CleanFinish:
                    return new Quest { kind = kind, target = 1, reward = 200 };
                case QuestKind.CostumeFinish:
                    return new Quest { kind = kind, target = 1, reward = 100 };
                default:
                    return new Quest { kind = kind, target = 1, arg = 1 + random.Next(RaftNames.Length - 1), reward = 100 };
            }
        }

        public static string Describe(Quest quest)
        {
            switch (quest.kind)
            {
                case QuestKind.FinishRaces:
                    return quest.target + " yarış bitir";
                case QuestKind.FinishBiome:
                    return Biomes.Name((Biome)quest.arg) + " haritasında bir yarış bitir";
                case QuestKind.FinishHard:
                    return (quest.arg >= 2 ? "Zor ya da Uzman" : "Orta ya da üstü") + " Macera bitir";
                case QuestKind.CollectPickups:
                    return quest.target + " eklenti topla";
                case QuestKind.PassCheckpoints:
                    return quest.target + " kontrol noktasından geç";
                case QuestKind.FartSeconds:
                    return quest.target + " saniye osur";
                case QuestKind.CleanFinish:
                    return "Sal hiç parçalanmadan bir yarış bitir";
                case QuestKind.CostumeFinish:
                    return "Kostümlü bir avatarla yarış bitir";
                default:
                    return RaftNames[Mathf.Clamp(quest.arg, 0, RaftNames.Length - 1)] + " ile bir yarış bitir";
            }
        }

        static void Add(QuestKind kind, int amount, Func<Quest, bool> match = null)
        {
            if (amount <= 0 || BotSimulation.Running)
            {
                return;
            }
            Quest[] quests = Current;
            for (int i = 0; i < quests.Length; i++)
            {
                Quest quest = quests[i];
                if (quest.kind != kind || quest.Done || (match != null && !match(quest)))
                {
                    continue;
                }
                int value = Mathf.Min(quest.target, quest.progress + amount);
                PlayerPrefs.SetInt(ProgressKey + i, value);
                if (value >= quest.target)
                {
                    quest.progress = value;
                    Completed?.Invoke(quest);
                }
            }
            PlayerPrefs.Save();
        }

        public static void Pickup()
        {
            Add(QuestKind.CollectPickups, 1);
        }

        public static void Checkpoint()
        {
            Add(QuestKind.PassCheckpoints, 1);
        }

        public static void Fart(int seconds)
        {
            Add(QuestKind.FartSeconds, seconds);
        }

        public static void Run(RunSummary run)
        {
            if (!run.finished)
            {
                return;
            }
            Add(QuestKind.FinishRaces, 1);
            Add(QuestKind.FinishBiome, 1, q => q.arg == (int)run.biome);
            Add(QuestKind.FinishHard, 1, q => run.adventure && run.difficulty >= q.arg);
            if (!run.broken)
            {
                Add(QuestKind.CleanFinish, 1);
            }
            if (run.costume > 0)
            {
                Add(QuestKind.CostumeFinish, 1);
            }
            Add(QuestKind.RaftFinish, 1, q => q.arg == run.raft);
        }

        public static bool Claim(int index)
        {
            Quest[] quests = Current;
            if (index < 0 || index >= quests.Length || !quests[index].Done || quests[index].claimed)
            {
                return false;
            }
            PlayerPrefs.SetInt(ClaimKey + index, 1);
            ProgressStore.AddBonus(quests[index].reward);
            ScoreSync.QueueReward("quest:" + Today + ":" + index, quests[index].reward);
            return true;
        }

        public static bool AllDone
        {
            get
            {
                foreach (Quest quest in Current)
                {
                    if (!quest.claimed)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public static bool BonusClaimed => PlayerPrefs.GetInt(BonusKey, 0) == 1;

        public static bool ClaimBonus()
        {
            if (!AllDone || BonusClaimed)
            {
                return false;
            }
            PlayerPrefs.SetInt(BonusKey, 1);
            ProgressStore.AddBonus(AllDoneBonus);
            ScoreSync.QueueReward("bonus:" + Today, AllDoneBonus);
            return true;
        }

        public static int LoginStreak
        {
            get
            {
                string today = Today;
                string last = PlayerPrefs.GetString(LoginDayKey, "");
                if (last == today)
                {
                    return PlayerPrefs.GetInt(LoginStreakKey, 1);
                }
                string yesterday = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                int streak = last == yesterday ? PlayerPrefs.GetInt(LoginStreakKey, 0) % StreakRewards.Length + 1 : 1;
                PlayerPrefs.SetString(LoginDayKey, today);
                PlayerPrefs.SetInt(LoginStreakKey, streak);
                PlayerPrefs.SetInt(LoginClaimKey, 0);
                PlayerPrefs.Save();
                return streak;
            }
        }

        public static int LoginReward => StreakRewards[Mathf.Clamp(LoginStreak - 1, 0, StreakRewards.Length - 1)];

        public static bool LoginClaimed
        {
            get
            {
                int streak = LoginStreak;
                return PlayerPrefs.GetInt(LoginClaimKey, 0) == 1 && streak > 0;
            }
        }

        public static bool ClaimLogin()
        {
            if (LoginClaimed)
            {
                return false;
            }
            int reward = LoginReward;
            PlayerPrefs.SetInt(LoginClaimKey, 1);
            ProgressStore.AddBonus(reward);
            ScoreSync.QueueReward("login:" + Today, reward);
            return true;
        }

        public static int Claimable
        {
            get
            {
                int count = LoginClaimed ? 0 : 1;
                foreach (Quest quest in Current)
                {
                    if (quest.Done && !quest.claimed)
                    {
                        count++;
                    }
                }
                if (AllDone && !BonusClaimed)
                {
                    count++;
                }
                return count;
            }
        }
    }
}
