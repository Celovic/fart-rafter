using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

namespace FartRafters
{
    public static class LeaderboardClient
    {
        public enum Board
        {
            AllTime,
            Weekly,
            Friends
        }

        public class Row
        {
            public int rank;
            public string playerId;
            public string name;
            public long score;
            public bool me;
        }

        public class Result
        {
            public readonly List<Row> rows = new List<Row>();
            public Row me;
            public string error;
        }

        public static async Task<Result> Load(Board board)
        {
            var result = new Result();
            if (!await OnlineServices.EnsureReady())
            {
                result.error = OnlineServices.Message;
                return result;
            }
            OnlineConfig config = OnlineConfig.Instance;
            string id = board == Board.Weekly ? config.weeklyLeaderboard : config.allTimeLeaderboard;
            string myId = OnlineServices.PlayerId;
            try
            {
                List<LeaderboardEntry> entries;
                if (board == Board.Friends)
                {
                    var ids = new List<string>(FriendsClient.FriendIds()) { myId };
                    LeaderboardScoresWithNotFoundPlayerIds scores = await LeaderboardsService.Instance.GetScoresByPlayerIdsAsync(id, ids);
                    entries = scores.Results ?? new List<LeaderboardEntry>();
                    entries.Sort((a, b) => b.Score.CompareTo(a.Score));
                }
                else
                {
                    LeaderboardScoresPage page = await LeaderboardsService.Instance.GetScoresAsync(id, new GetScoresOptions { Limit = 50 });
                    entries = page.Results ?? new List<LeaderboardEntry>();
                }
                for (int i = 0; i < entries.Count; i++)
                {
                    LeaderboardEntry entry = entries[i];
                    if (PlayerProfile.IsBlocked(entry.PlayerId))
                    {
                        continue;
                    }
                    var row = new Row
                    {
                        rank = board == Board.Friends ? i + 1 : entry.Rank + 1,
                        playerId = entry.PlayerId,
                        name = NameGenerator.FromServiceName(entry.PlayerName),
                        score = (long)Math.Round(entry.Score),
                        me = entry.PlayerId == myId
                    };
                    result.rows.Add(row);
                    if (row.me)
                    {
                        result.me = row;
                    }
                }
                if (result.me == null && board != Board.Friends)
                {
                    try
                    {
                        LeaderboardEntry mine = await LeaderboardsService.Instance.GetPlayerScoreAsync(id);
                        result.me = new Row { rank = mine.Rank + 1, playerId = mine.PlayerId, name = NameGenerator.FromServiceName(mine.PlayerName), score = (long)Math.Round(mine.Score), me = true };
                    }
                    catch (Exception)
                    {
                        result.me = null;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Leaderboard: " + exception.Message);
                result.error = "Liste yüklenemedi. Biraz sonra tekrar dene.";
            }
            return result;
        }
    }
}
