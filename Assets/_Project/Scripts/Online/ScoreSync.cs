using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using Unity.Services.Leaderboards;
using UnityEngine;

namespace FartRafters
{
    public static class ScoreSync
    {
        static bool flushing;

        public static void Queue(ScoreReport report, string levelId, int awarded)
        {
            if (report == null || awarded <= 0)
            {
                return;
            }
            ProgressStore.Enqueue(new ProgressStore.Submission
            {
                id = Guid.NewGuid().ToString("N"),
                level = levelId,
                score = awarded,
                elapsed = report.elapsed,
                par = report.par,
                humans = report.humans,
                bots = report.bots,
                mode = report.adventure ? "adventure" : "practice",
                difficulty = report.difficultyIndex,
                stamp = DateTime.UtcNow.Ticks
            });
            Flush();
        }

        public const string RewardMode = "reward";

        public static void QueueReward(string key, int points)
        {
            if (string.IsNullOrEmpty(key) || points <= 0)
            {
                return;
            }
            ProgressStore.Enqueue(new ProgressStore.Submission
            {
                id = Guid.NewGuid().ToString("N"),
                level = key,
                score = points,
                mode = RewardMode,
                stamp = DateTime.UtcNow.Ticks
            });
            Flush();
        }

        public static async void Flush()
        {
            if (flushing || !PlayerProfile.LeaderboardVisible)
            {
                return;
            }
            if (!await OnlineServices.EnsureReady())
            {
                return;
            }
            flushing = true;
            try
            {
                foreach (ProgressStore.Submission submission in new List<ProgressStore.Submission>(ProgressStore.PendingSubmissions()))
                {
                    if (await Submit(submission))
                    {
                        ProgressStore.RemoveSubmission(submission.id);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            finally
            {
                flushing = false;
            }
        }

        static async Task<bool> SubmitReward(ProgressStore.Submission submission)
        {
            var args = new Dictionary<string, object>
            {
                { "submissionId", submission.id },
                { "key", submission.level },
                { "points", submission.score }
            };
            try
            {
                await CloudCodeService.Instance.CallEndpointAsync(OnlineConfig.Instance.claimRewardFunction, args);
                return true;
            }
            catch (CloudCodeException exception) when (exception.ToString().Contains("invalid-"))
            {
                Debug.LogWarning("[FartRafters] Reward rejected: " + exception.Message);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Reward submit: " + exception.Message);
                return false;
            }
        }

        static async Task<bool> Submit(ProgressStore.Submission submission)
        {
            if (submission.mode == RewardMode)
            {
                return await SubmitReward(submission);
            }
            OnlineConfig config = OnlineConfig.Instance;
            var args = new Dictionary<string, object>
            {
                { "submissionId", submission.id },
                { "score", submission.score },
                { "level", submission.level },
                { "elapsed", submission.elapsed },
                { "par", submission.par },
                { "humans", submission.humans },
                { "bots", submission.bots },
                { "mode", submission.mode },
                { "difficulty", submission.difficulty }
            };
            try
            {
                await CloudCodeService.Instance.CallEndpointAsync(config.submitScoreFunction, args);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Cloud Code submit: " + exception.Message);
                if (!config.allowDirectScoreSubmit)
                {
                    return false;
                }
            }
            try
            {
                await LeaderboardsService.Instance.AddPlayerScoreAsync(config.allTimeLeaderboard, submission.score);
                await LeaderboardsService.Instance.AddPlayerScoreAsync(config.weeklyLeaderboard, submission.score);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Leaderboard submit: " + exception.Message);
                return false;
            }
        }
    }
}
