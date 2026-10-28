using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace FartRafters
{
    public class OnlinePeer : MonoBehaviour
    {
        static string commandFile;
        static string peerName = "Bilgisayar Test";
        int handled;
        OnlineRoom room;
        bool autoJoinInvite;
        bool busy;
        float nextStatus;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-peer")
                {
                    commandFile = args[i + 1];
                }
                if (args[i] == "-peername")
                {
                    peerName = args[i + 1];
                }
            }
            if (string.IsNullOrEmpty(commandFile) || FindAnyObjectByType<OnlinePeer>() != null)
            {
                return;
            }
            AudioListener.volume = 0f;
            var go = new GameObject("OnlinePeer");
            DontDestroyOnLoad(go);
            go.AddComponent<OnlinePeer>();
        }

        static void Log(string message)
        {
            Debug.Log("[Peer] " + message);
        }

        IEnumerator Start()
        {
            PlayerProfile.SetBirthYear(1990);
            PlayerProfile.SetName(peerName);
            PlayerProfile.MarkReminderShown();
            Task<bool> ready = FriendsClient.EnsureReady();
            while (!ready.IsCompleted)
            {
                yield return null;
            }
            Log("ready friends=" + ready.Result + " name=" + OnlineServices.ServiceName + " id=" + OnlineServices.PlayerId + " msg=" + FriendsClient.Message);
            NotificationCenter.Added += OnNotification;
            Room();
        }

        OnlineRoom Room()
        {
            if (room != null)
            {
                return room;
            }
            room = OnlineRoom.Ensure();
            OnlineRoom current = room;
            current.RoomChanged += () => Log("room devices=" + current.State.devices.Count + " code=" + current.State.code + " ingame=" + current.InGame);
            current.Closed += reason => Log("room closed " + reason);
            return room;
        }

        void OnNotification(GameNotification notification)
        {
            Log("notification kind=" + notification.kind + " from=" + notification.playerName + " code=" + notification.code + " text=" + notification.text);
            if (notification.kind == NotificationKind.RoomInvite && autoJoinInvite && !string.IsNullOrEmpty(notification.code))
            {
                autoJoinInvite = false;
                Join(notification.code, "davet");
            }
        }

        async void Join(string code, string via)
        {
            busy = true;
            bool ok = await Room().Join(false, code, new DeviceInfo { name = peerName, locals = 1, avatars = new[] { 4 } });
            Log("join via=" + via + " code=" + code + " ok=" + ok + " error=" + room.LastError);
            if (ok)
            {
                room.SetLocal(1, new[] { 4 }, true);
            }
            busy = false;
        }

        void Update()
        {
            if (Time.unscaledTime >= nextStatus && OnlineRoom.Active)
            {
                nextStatus = Time.unscaledTime + 10f;
                CourseManager course = FindFirstObjectByType<CourseManager>();
                Log("tick ingame=" + OnlineRoom.Current.InGame + " devices=" + OnlineRoom.Current.State.devices.Count + (course != null ? " elapsed=" + course.ElapsedTime.ToString("F0") + " cp=" + course.LastCheckpoint + " raft=" + course.raft.transform.position.ToString("F0") : ""));
            }
            if (busy || !File.Exists(commandFile))
            {
                return;
            }
            string[] lines;
            try
            {
                lines = File.ReadAllLines(commandFile);
            }
            catch (IOException)
            {
                return;
            }
            while (handled < lines.Length)
            {
                string line = lines[handled++].Trim();
                if (line.Length > 0)
                {
                    Execute(line);
                    return;
                }
            }
        }

        async void Execute(string line)
        {
            busy = true;
            string[] parts = line.Split(new[] { ' ' }, 2);
            string arg = parts.Length > 1 ? parts[1] : "";
            Log("cmd " + line);
            try
            {
                switch (parts[0])
                {
                    case "status":
                        Status();
                        break;
                    case "accept":
                        foreach (FriendsClient.Person person in FriendsClient.Incoming())
                        {
                            await FriendsClient.Accept(person.id);
                            Log("accepted " + person.name);
                        }
                        Status();
                        break;
                    case "decline":
                        foreach (FriendsClient.Person person in FriendsClient.Incoming())
                        {
                            await FriendsClient.Decline(person.id);
                            Log("declined " + person.name);
                        }
                        Status();
                        break;
                    case "join":
                        busy = false;
                        Join(arg, "kod");
                        return;
                    case "waitinvite":
                        autoJoinInvite = true;
                        Log("waiting for invite");
                        break;
                    case "leave":
                        Room().Leave();
                        Log("left");
                        break;
                    case "quit":
                        Application.Quit();
                        break;
                }
            }
            catch (Exception exception)
            {
                Log("cmd error " + exception.Message);
            }
            busy = false;
        }

        void Status()
        {
            Log("status friends=" + Names(FriendsClient.Friends()) + " incoming=" + Names(FriendsClient.Incoming()) + " outgoing=" + Names(FriendsClient.Outgoing()));
        }

        static string Names(List<FriendsClient.Person> people)
        {
            var names = new List<string>();
            foreach (FriendsClient.Person person in people)
            {
                names.Add(person.name + (person.online ? "(online)" : ""));
            }
            return "[" + string.Join(", ", names) + "]";
        }
    }
}
