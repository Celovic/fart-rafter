using System;
using System.Collections.Generic;
using UnityEngine;

namespace FartRafters
{
    public static class ProgressStore
    {
        const string BestPrefix = "best.";
        const string ScorePrefix = "bestscore.";
        const string DonePrefix = "done.";
        const string DailyPrefix = "daily.";
        const string TotalKey = "score.total";
        const string WeekKey = "score.week";
        const string WeekStartKey = "score.weekstart";
        const string QueueKey = "score.queue";

        [Serializable]
        public class Submission
        {
            public string id;
            public string level;
            public int score;
            public float elapsed;
            public float par;
            public int humans;
            public int bots;
            public string mode;
            public int difficulty;
            public long stamp;
        }

        [Serializable]
        class SubmissionList
        {
            public List<Submission> items = new List<Submission>();
        }

        public static bool TryGetBestTime(LevelDefinition level, out float time)
        {
            return TryGetBestTime(level != null ? level.id : null, out time);
        }

        public static bool TryGetBestTime(string id, out float time)
        {
            time = id != null ? PlayerPrefs.GetFloat(BestPrefix + id, -1f) : -1f;
            return time >= 0f;
        }

        public static bool TryGetBestScore(string id, out int score)
        {
            score = id != null ? PlayerPrefs.GetInt(ScorePrefix + id, -1) : -1;
            return score >= 0;
        }

        public static bool IsFinished(string id)
        {
            return id != null && (PlayerPrefs.GetInt(DonePrefix + id, 0) == 1 || TryGetBestTime(id, out _));
        }

        public static bool IsUnlocked(GameCatalog catalog, int index)
        {
            if (catalog == null || catalog.levels == null || index < 0 || index >= catalog.levels.Length)
            {
                return false;
            }
            return catalog.unlockAllLevels || index == 0 || IsFinished(catalog.levels[index - 1].id);
        }

        public static int TotalScore => PlayerPrefs.GetInt(TotalKey, 0);

        public static int WeekScore
        {
            get
            {
                RollWeek();
                return PlayerPrefs.GetInt(WeekKey, 0);
            }
        }

        public static float RepeatFactor(string id, bool adventure)
        {
            if (adventure)
            {
                return 1f;
            }
            return PlayerPrefs.GetInt(DailyKey(id), 0) >= ScoreRules.PracticeFullRuns ? ScoreRules.PracticeRepeatFactor : 1f;
        }

        public static bool RecordRun(string id, float time, int score, bool adventure, out bool newBestScore, out int awarded)
        {
            bool newBestTime = false;
            if (!TryGetBestTime(id, out float bestTime) || time < bestTime)
            {
                PlayerPrefs.SetFloat(BestPrefix + id, time);
                newBestTime = true;
            }
            awarded = Mathf.RoundToInt(score * RepeatFactor(id, adventure));
            newBestScore = !TryGetBestScore(id, out int bestScore) || awarded > bestScore;
            if (newBestScore)
            {
                PlayerPrefs.SetInt(ScorePrefix + id, awarded);
            }
            PlayerPrefs.SetInt(DonePrefix + id, 1);
            if (!adventure)
            {
                string key = DailyKey(id);
                PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key, 0) + 1);
            }
            PlayerPrefs.SetInt(TotalKey, TotalScore + awarded);
            RollWeek();
            PlayerPrefs.SetInt(WeekKey, PlayerPrefs.GetInt(WeekKey, 0) + awarded);
            PlayerPrefs.Save();
            return newBestTime;
        }

        public static void AddBonus(int points)
        {
            if (points <= 0)
            {
                return;
            }
            PlayerPrefs.SetInt(TotalKey, TotalScore + points);
            RollWeek();
            PlayerPrefs.SetInt(WeekKey, PlayerPrefs.GetInt(WeekKey, 0) + points);
            PlayerPrefs.Save();
        }

        public static void Enqueue(Submission submission)
        {
            SubmissionList list = LoadQueue();
            list.items.Add(submission);
            while (list.items.Count > 50)
            {
                list.items.RemoveAt(0);
            }
            SaveQueue(list);
        }

        public static List<Submission> PendingSubmissions()
        {
            return LoadQueue().items;
        }

        public static void RemoveSubmission(string id)
        {
            SubmissionList list = LoadQueue();
            list.items.RemoveAll(item => item.id == id);
            SaveQueue(list);
        }

        public static void ClearAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        static SubmissionList LoadQueue()
        {
            string json = PlayerPrefs.GetString(QueueKey, "");
            if (string.IsNullOrEmpty(json))
            {
                return new SubmissionList();
            }
            try
            {
                return JsonUtility.FromJson<SubmissionList>(json) ?? new SubmissionList();
            }
            catch (ArgumentException)
            {
                return new SubmissionList();
            }
        }

        static void SaveQueue(SubmissionList list)
        {
            PlayerPrefs.SetString(QueueKey, JsonUtility.ToJson(list));
            PlayerPrefs.Save();
        }

        static string DailyKey(string id)
        {
            return DailyPrefix + id + "." + DateTime.Now.ToString("yyyyMMdd");
        }

        static void RollWeek()
        {
            DateTime now = DateTime.UtcNow.Date;
            int offset = ((int)now.DayOfWeek + 6) % 7;
            string monday = now.AddDays(-offset).ToString("yyyyMMdd");
            if (PlayerPrefs.GetString(WeekStartKey, "") != monday)
            {
                PlayerPrefs.SetString(WeekStartKey, monday);
                PlayerPrefs.SetInt(WeekKey, 0);
            }
        }
    }
}
