using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace FartRafters
{
    public class OnlineSmokeTest : MonoBehaviour
    {
        static string role = "";
        static string address = "127.0.0.1";
        static float duration = 40f;

        OnlineRoom room;
        static bool IsHost => role == "host" || role == "relayhost";
        bool connected;
        float watchdog;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                switch (args[i])
                {
                    case "-nettest":
                        role = args[i + 1];
                        break;
                    case "-netaddr":
                        address = args[i + 1];
                        break;
                    case "-nettime":
                        duration = float.Parse(args[i + 1]);
                        break;
                }
            }
            if (role != "host" && role != "join" && role != "relayhost" && role != "relayjoin" && role != "svc")
            {
                return;
            }
            AudioListener.volume = 0f;
            AudioListener.pause = true;
            var go = new GameObject("OnlineSmokeTest");
            DontDestroyOnLoad(go);
            go.AddComponent<OnlineSmokeTest>();
        }

        async Task ServiceCheck()
        {
            bool ready = await OnlineServices.EnsureReady();
            Log("svc ready=" + ready + " player=" + OnlineServices.PlayerId + " name=" + OnlineServices.ServiceName);
            if (!ready)
            {
                return;
            }
            var args = new System.Collections.Generic.Dictionary<string, object>
            {
                { "submissionId", Guid.NewGuid().ToString("N") },
                { "score", 42 },
                { "level", "svc-test" },
                { "elapsed", 120f },
                { "par", 100f },
                { "humans", 1 },
                { "bots", 3 },
                { "mode", "adventure" },
                { "difficulty", 1 }
            };
            try
            {
                string reply = await Unity.Services.CloudCode.CloudCodeService.Instance.CallEndpointAsync(OnlineConfig.Instance.submitScoreFunction, args);
                Log("svc cloudcode ok reply=" + reply);
            }
            catch (Exception exception)
            {
                Log("svc cloudcode FAIL " + exception.ToString().Replace(Environment.NewLine, " | "));
            }
            string day = DateTime.Now.ToString("yyyyMMdd");
            foreach ((string key, int points) in new[] { ("login:" + day, 50), ("login:" + day, 50), ("quest:" + day + ":0", 999) })
            {
                try
                {
                    string reply = await Unity.Services.CloudCode.CloudCodeService.Instance.CallEndpointAsync(OnlineConfig.Instance.claimRewardFunction, new System.Collections.Generic.Dictionary<string, object> { { "submissionId", Guid.NewGuid().ToString("N") }, { "key", key }, { "points", points } });
                    Log("svc reward " + key + " " + points + " ok " + reply.Replace(Environment.NewLine, " "));
                }
                catch (Exception exception)
                {
                    string text = exception.ToString();
                    int at = text.IndexOf("Error: ");
                    Log("svc reward " + key + " " + points + " FAIL " + (at >= 0 ? text.Substring(at, Math.Min(40, text.Length - at)) : exception.Message));
                }
            }
            foreach (LeaderboardClient.Board board in new[] { LeaderboardClient.Board.AllTime, LeaderboardClient.Board.Weekly })
            {
                LeaderboardClient.Result result = await LeaderboardClient.Load(board);
                Log("svc board=" + board + " error=" + result.error + " rows=" + result.rows.Count + " me=" + (result.me != null ? result.me.score.ToString() : "-"));
            }
            bool friends = await FriendsClient.EnsureReady();
            Log("svc friends ready=" + friends + " message=" + FriendsClient.Message + " count=" + FriendsClient.Friends().Count);
        }

        void Update()
        {
            watchdog += Time.unscaledDeltaTime;
            if (watchdog > duration + 150f)
            {
                Finish("watchdog");
            }
        }

        IEnumerator Start()
        {
            Log("start role=" + role);
            room = OnlineRoom.Ensure();
            room.RoomChanged += () => Log("room devices=" + room.State.devices.Count + " seats=" + room.State.seats.Count + " spectators=" + room.State.spectators.Count + " code=" + room.State.code);
            room.EmojiReceived += emoji => Log("emoji from=" + emoji.name + " index=" + emoji.emoji);
            room.Closed += reason => Log("closed reason=" + reason);
            if (role == "svc")
            {
                Task services = ServiceCheck();
                while (!services.IsCompleted)
                {
                    yield return null;
                }
                Finish("svc done");
                yield break;
            }
            bool relay = role.StartsWith("relay");
            if (relay)
            {
                Task<bool> ready = OnlineServices.EnsureReady();
                while (!ready.IsCompleted)
                {
                    yield return null;
                }
                Log("services ready=" + ready.Result + " state=" + OnlineServices.State + " message=" + OnlineServices.Message + " player=" + OnlineServices.PlayerId);
                if (!ready.Result)
                {
                    Finish("services failed");
                    yield break;
                }
            }
            if (IsHost)
            {
                Connect(room.Host(!relay, new DeviceInfo { name = "Test Kurucu", locals = 3, avatars = new[] { 0, 1, 2 } }, new RoomState { adventure = true, difficulty = 0, bots = 2, botLevel = 3 }));
            }
            else
            {
                Connect(room.Join(!relay, address, new DeviceInfo { name = "Test Konuk", locals = 2, avatars = new[] { 3, 4, 5 } }));
            }
            float deadline = Time.realtimeSinceStartup + 60f;
            while (!connected && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
            if (!connected)
            {
                Finish("connect failed error=" + room.LastError);
                yield break;
            }
            if (relay && IsHost)
            {
                Log("relay code=" + room.State.code);
                System.IO.File.WriteAllText("relay_code.txt", room.State.code);
            }
            if (IsHost)
            {
                while (Time.realtimeSinceStartup < deadline && (room.State.devices.Count < 2 || !room.AllReady))
                {
                    yield return null;
                }
                if (room.State.devices.Count < 2)
                {
                    Finish("no client joined");
                    yield break;
                }
                Log("planned seats=" + Describe(room.State));
                room.SendEmoji(0);
                room.SetLocal(1, new[] { 0, 1, 2 }, true);
                room.SetSettings(true, 0, 0, 1, 3);
                yield return new WaitForSecondsRealtime(1f);
                Log("starting seats=" + Describe(room.State));
                room.StartGame();
            }
            else
            {
                while (Time.realtimeSinceStartup < deadline && room.State.devices.Count < 2)
                {
                    yield return null;
                }
                room.SetLocal(2, new[] { 3, 4, 5 }, true);
                room.SendEmoji(2);
                while (Time.realtimeSinceStartup < deadline && !room.InGame)
                {
                    yield return null;
                }
                if (!room.InGame)
                {
                    Finish("game did not start");
                    yield break;
                }
            }
            yield return new WaitForSecondsRealtime(1f);
            float end = Time.realtimeSinceStartup + duration;
            while (Time.realtimeSinceStartup < end && OnlineRoom.Active)
            {
                CourseManager course = FindFirstObjectByType<CourseManager>();
                if (course != null && course.raft != null)
                {
                    Vector3 position = course.raft.transform.position;
                    int riders = FindObjectsByType<Rider>(FindObjectsSortMode.None).Length;
                    Log("t=" + OnlineGame.SharedTime.ToString("F1") + " elapsed=" + course.ElapsedTime.ToString("F1") + " running=" + course.Running + " checkpoint=" + course.LastCheckpoint + " raft=" + position.x.ToString("F1") + "," + position.z.ToString("F1") + " riders=" + riders + " online=" + OnlineGame.Active + " client=" + OnlineGame.IsClient);
                }
                yield return new WaitForSecondsRealtime(3f);
            }
            if (IsHost && OnlineRoom.Active)
            {
                room.BackToLobby();
                yield return new WaitForSecondsRealtime(3f);
                Log("back in lobby devices=" + room.State.devices.Count + " ingame=" + room.InGame);
            }
            else
            {
                yield return new WaitForSecondsRealtime(4f);
                Log("client after host lobby ingame=" + (OnlineRoom.Active && room.InGame) + " active=" + OnlineRoom.Active);
            }
            Finish("done");
        }

        async void Connect(System.Threading.Tasks.Task<bool> task)
        {
            bool ok = await task;
            Log("connect ok=" + ok + " error=" + room.LastError);
            connected = ok;
        }

        static string Describe(RoomState state)
        {
            var builder = new System.Text.StringBuilder();
            foreach (SeatInfo seat in state.seats)
            {
                builder.Append(seat.bot ? "[bot]" : "[" + seat.name + "@" + seat.owner + "]");
            }
            foreach (SpectatorInfo spectator in state.spectators)
            {
                builder.Append("(izleyici " + spectator.name + ")");
            }
            return builder.ToString();
        }

        static void Log(string message)
        {
            Debug.Log("[NetTest] " + role + " " + message);
        }

        void Finish(string message)
        {
            Log("finish " + message);
            Application.Quit();
        }
    }
}
