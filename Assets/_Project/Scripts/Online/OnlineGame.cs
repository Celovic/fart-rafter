using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace FartRafters
{
    public class OnlineGame : MonoBehaviour
    {
        const float SnapshotInterval = 1f / 15f;
        const float InputInterval = 1f / 20f;
        const float InterpolationDelay = 0.1f;

        struct Frame
        {
            public float time;
            public Vector3 position;
            public Quaternion rotation;
        }

        public static OnlineGame Current { get; private set; }
        public static bool Active => Current != null;
        public static bool IsClient => Current != null && !Current.host;
        public static ScoreReport RemoteReport { get; private set; }

        GameBootstrap bootstrap;
        OnlineRoom room;
        bool host;
        Raft raft;
        RaftHealth health;
        Rider[] riders;
        SeatInfo[] seats;
        readonly Dictionary<int, RemoteBrain> remote = new Dictionary<int, RemoteBrain>();
        readonly List<int> localSeats = new List<int>();
        readonly List<Frame> frames = new List<Frame>();
        float nextSnapshot;
        float nextInput;
        float hostTime;
        float hostTimeReceived;
        bool suppress;

        public static float SharedTime
        {
            get
            {
                if (Current == null)
                {
                    return Time.timeSinceLevelLoad;
                }
                return Current.host ? Time.timeSinceLevelLoad : Current.hostTime + (Time.time - Current.hostTimeReceived);
            }
        }

        public void Setup(GameBootstrap owner, OnlineRoom onlineRoom, SeatInfo[] seatInfos)
        {
            Current = this;
            RemoteReport = null;
            bootstrap = owner;
            room = onlineRoom;
            host = room.IsHost;
            raft = owner.raft;
            health = raft.GetComponent<RaftHealth>();
            riders = owner.seats;
            seats = seatInfos;
            MovingObstacle.SharedClock = () => SharedTime;
            MillWheel.SharedClock = () => SharedTime;
            Geyser.SharedClock = () => SharedTime;
            IcicleDrop.SharedClock = () => SharedTime;
            room.GameMessage += OnMessage;
            room.DeviceLeft += OnDeviceLeft;
            room.EmojiReceived += OnEmoji;

            for (int i = 0; i < seats.Length && i < riders.Length; i++)
            {
                if (seats[i] != null && !seats[i].bot && seats[i].owner == room.LocalClientId)
                {
                    localSeats.Add(i);
                }
            }

            if (host)
            {
                ScoreTracker.SeatNames = seat => seat >= 0 && seat < seats.Length && seats[seat] != null ? seats[seat].name : "";
                for (int i = 0; i < seats.Length && i < riders.Length; i++)
                {
                    if (seats[i] != null && !seats[i].bot && seats[i].owner != room.LocalClientId)
                    {
                        var brain = riders[i].gameObject.GetComponent<RemoteBrain>() ?? riders[i].gameObject.AddComponent<RemoteBrain>();
                        riders[i].brain = brain;
                        remote[i] = brain;
                    }
                }
                if (health != null)
                {
                    health.Damaged += amount => SendEvent(NetEvent.Damaged, -1, amount);
                    health.Broken += () => SendEvent(NetEvent.Broken, -1, 0f);
                    health.Caught += () => SendEvent(NetEvent.Caught, -1, 0f);
                    health.ShieldSaved += () => SendEvent(NetEvent.ShieldSaved, -1, 0f);
                    health.Shocked += () => SendEvent(NetEvent.Shocked, -1, 0f);
                }
                Pickup.Taken += OnPickupTaken;
                Barrel.Exploded += OnBarrelExploded;
                Rider.StateChanged += OnRiderState;
                if (bootstrap.course != null)
                {
                    bootstrap.course.Respawned += () => SendEvent(NetEvent.Respawned, -1, 0f);
                    bootstrap.course.RaceFinished += () => SendEvent(NetEvent.Finished, -1, 0f);
                }
                if (ScoreTracker.Current != null)
                {
                    ScoreTracker.Current.Completed += SendReport;
                }
            }
            else
            {
                Rigidbody body = raft.GetComponent<Rigidbody>();
                body.isKinematic = true;
                body.interpolation = RigidbodyInterpolation.None;
                DisableOn<Buoyancy>(raft);
                DisableOn<RaftFall>(raft);
                DisableOn<BotCrew>(raft);
                if (ScoreTracker.Current != null)
                {
                    ScoreTracker.Current.enabled = false;
                }
                if (bootstrap.course != null)
                {
                    bootstrap.course.NetworkClient = true;
                }
                foreach (Rider rider in riders)
                {
                    if (rider != null)
                    {
                        rider.NetworkDriven = true;
                    }
                }
            }
        }

        static void DisableOn<T>(Raft raft) where T : Behaviour
        {
            T component = raft.GetComponent<T>();
            if (component != null)
            {
                component.enabled = false;
            }
        }

        void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
            MovingObstacle.SharedClock = null;
            MillWheel.SharedClock = null;
            Geyser.SharedClock = null;
            IcicleDrop.SharedClock = null;
            ScoreTracker.SeatNames = null;
            Pickup.Taken -= OnPickupTaken;
            Barrel.Exploded -= OnBarrelExploded;
            Rider.StateChanged -= OnRiderState;
            if (room != null)
            {
                room.GameMessage -= OnMessage;
                room.DeviceLeft -= OnDeviceLeft;
                room.EmojiReceived -= OnEmoji;
            }
        }

        void Update()
        {
            if (room == null || !OnlineRoom.Active)
            {
                return;
            }
            if (host)
            {
                if (Time.time >= nextSnapshot)
                {
                    nextSnapshot = Time.time + SnapshotInterval;
                    SendSnapshot();
                }
                return;
            }
            SendInput();
            Interpolate();
        }

        void SendInput()
        {
            if (localSeats.Count == 0)
            {
                return;
            }
            var commands = new RiderCommand[localSeats.Count];
            for (int i = 0; i < localSeats.Count; i++)
            {
                Rider rider = riders[localSeats[i]];
                RiderCommand command = !rider.Autopilot && rider.brain != null && rider.brain.isActiveAndEnabled ? rider.brain.GetCommand(rider) : default;
                commands[i] = command;
                rider.PredictAim(command.aim);
            }
            if (Time.time < nextInput)
            {
                return;
            }
            nextInput = Time.time + InputInterval;
            using (var writer = new FastBufferWriter(64, Allocator.Temp))
            {
                writer.WriteValueSafe((byte)localSeats.Count);
                for (int i = 0; i < localSeats.Count; i++)
                {
                    writer.WriteValueSafe((byte)localSeats[i]);
                    writer.WriteValueSafe((sbyte)Mathf.RoundToInt(Mathf.Clamp(commands[i].aim.x, -1f, 1f) * 127f));
                    writer.WriteValueSafe((sbyte)Mathf.RoundToInt(Mathf.Clamp(commands[i].aim.z, -1f, 1f) * 127f));
                    writer.WriteValueSafe((byte)((commands[i].gas ? 1 : 0) | (riders[localSeats[i]].Autopilot ? 2 : 0)));
                }
                room.SendRaw(RoomMessages.Input, NetworkManager.ServerClientId, writer, NetworkDelivery.UnreliableSequenced);
            }
        }

        void SendSnapshot()
        {
            CourseManager course = bootstrap.course;
            using (var writer = new FastBufferWriter(256, Allocator.Temp))
            {
                writer.WriteValueSafe(Time.timeSinceLevelLoad);
                writer.WriteValueSafe(course != null ? course.ElapsedTime : 0f);
                writer.WriteValueSafe((byte)((course != null && course.Running ? 1 : 0) | (course != null && course.Finished ? 2 : 0)));
                Vector3 position = raft.Body.position;
                Quaternion rotation = raft.Body.rotation;
                writer.WriteValueSafe(position);
                writer.WriteValueSafe((short)(rotation.x * 32767f));
                writer.WriteValueSafe((short)(rotation.y * 32767f));
                writer.WriteValueSafe((short)(rotation.z * 32767f));
                writer.WriteValueSafe((short)(rotation.w * 32767f));
                writer.WriteValueSafe((byte)Mathf.RoundToInt(health != null ? health.Normalized * 255f : 255f));
                writer.WriteValueSafe((byte)riders.Length);
                foreach (Rider rider in riders)
                {
                    bool active = rider != null && rider.gameObject.activeSelf;
                    byte flags = (byte)((active ? 1 : 0) | (active && rider.IsFiring ? 2 : 0) | (active && rider.InWater ? 4 : 0) | (rider != null && rider.Eliminated ? 8 : 0));
                    writer.WriteValueSafe(flags);
                    writer.WriteValueSafe((byte)Mathf.RoundToInt((rider != null ? rider.LocalYaw : 0f) / 360f * 255f));
                    writer.WriteValueSafe((byte)Mathf.RoundToInt((rider != null ? rider.Gas.Normalized : 0f) * 255f));
                    if (active && rider.InWater)
                    {
                        Vector3 swim = rider.transform.position;
                        writer.WriteValueSafe(swim.x);
                        writer.WriteValueSafe(swim.z);
                    }
                }
                foreach (ulong client in room.RemoteClients())
                {
                    room.SendRaw(RoomMessages.Snapshot, client, writer, NetworkDelivery.UnreliableSequenced);
                }
            }
        }

        void OnMessage(string name, ulong sender, FastBufferReader reader)
        {
            try
            {
                switch (name)
                {
                    case RoomMessages.Input:
                        if (host)
                        {
                            ReadInput(sender, reader);
                        }
                        break;
                    case RoomMessages.Snapshot:
                        if (!host)
                        {
                            ReadSnapshot(reader);
                        }
                        break;
                    case RoomMessages.Event:
                        if (!host)
                        {
                            ReadEvent(reader);
                        }
                        break;
                    case RoomMessages.Report:
                        if (!host)
                        {
                            ReadReport(reader);
                        }
                        break;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Net message " + name + ": " + exception.Message);
            }
        }

        void ReadInput(ulong sender, FastBufferReader reader)
        {
            reader.ReadValueSafe(out byte count);
            for (int i = 0; i < count; i++)
            {
                reader.ReadValueSafe(out byte seat);
                reader.ReadValueSafe(out sbyte x);
                reader.ReadValueSafe(out sbyte z);
                reader.ReadValueSafe(out byte gas);
                if (seat < seats.Length && seats[seat] != null && seats[seat].owner == sender && remote.TryGetValue(seat, out RemoteBrain brain))
                {
                    var aim = new Vector3(x / 127f, 0f, z / 127f);
                    brain.Command = new RiderCommand(aim.sqrMagnitude > 0.0004f ? aim.normalized : Vector3.zero, (gas & 1) == 1);
                    bool auto = (gas & 2) == 2;
                    if (riders[seat].Autopilot != auto)
                    {
                        GameSession session = FindAnyObjectByType<GameSession>();
                        riders[seat].SetAutopilot(auto, session != null ? session.AutopilotLevel : null);
                    }
                }
            }
        }

        void ReadSnapshot(FastBufferReader reader)
        {
            reader.ReadValueSafe(out float time);
            reader.ReadValueSafe(out float elapsed);
            reader.ReadValueSafe(out byte flags);
            reader.ReadValueSafe(out Vector3 position);
            reader.ReadValueSafe(out short rx);
            reader.ReadValueSafe(out short ry);
            reader.ReadValueSafe(out short rz);
            reader.ReadValueSafe(out short rw);
            reader.ReadValueSafe(out byte healthByte);
            reader.ReadValueSafe(out byte count);
            if (frames.Count > 0 && time <= frames[frames.Count - 1].time)
            {
                return;
            }
            hostTime = time;
            hostTimeReceived = Time.time;
            var rotation = new Quaternion(rx / 32767f, ry / 32767f, rz / 32767f, rw / 32767f);
            frames.Add(new Frame { time = time, position = position, rotation = rotation.normalized });
            while (frames.Count > 8)
            {
                frames.RemoveAt(0);
            }
            if (bootstrap.course != null)
            {
                bootstrap.course.SetNetworkTime(elapsed, (flags & 1) != 0);
            }
            if (health != null)
            {
                health.SetNetworkHealth(healthByte / 255f * health.maxHealth);
            }
            for (int i = 0; i < count; i++)
            {
                reader.ReadValueSafe(out byte riderFlags);
                reader.ReadValueSafe(out byte yaw);
                reader.ReadValueSafe(out byte gas);
                Vector3 swim = Vector3.zero;
                if ((riderFlags & 5) == 5)
                {
                    reader.ReadValueSafe(out float sx);
                    reader.ReadValueSafe(out float sz);
                    WaterSurface water = WaterSurface.Instance;
                    swim = new Vector3(sx, (water != null ? water.GetHeight(sx, sz, Time.time) : 0f) - 0.4f, sz);
                }
                if (i >= riders.Length || riders[i] == null)
                {
                    continue;
                }
                Rider rider = riders[i];
                bool active = (riderFlags & 1) != 0;
                if (active && !rider.gameObject.activeSelf && !rider.Eliminated)
                {
                    rider.gameObject.SetActive(true);
                }
                if (rider.gameObject.activeSelf && rider.InWater && (riderFlags & 4) == 0)
                {
                    rider.ApplyNetworkEvent(RiderEvent.Boarded, Vector3.zero);
                }
                if (rider.gameObject.activeSelf)
                {
                    rider.ApplyNetwork((riderFlags & 2) != 0, gas / 255f, yaw / 255f * 360f, localSeats.Contains(i), swim);
                }
            }
        }

        void Interpolate()
        {
            if (frames.Count == 0)
            {
                return;
            }
            float target = SharedTime - InterpolationDelay;
            Frame a = frames[0];
            Frame b = frames[frames.Count - 1];
            for (int i = 0; i < frames.Count - 1; i++)
            {
                if (frames[i].time <= target && frames[i + 1].time >= target)
                {
                    a = frames[i];
                    b = frames[i + 1];
                    break;
                }
            }
            float span = Mathf.Max(0.0001f, b.time - a.time);
            float t = Mathf.Clamp01((target - a.time) / span);
            if (target > b.time)
            {
                a = b;
                t = 0f;
            }
            Vector3 position = Vector3.Lerp(a.position, b.position, t);
            Quaternion rotation = Quaternion.Slerp(a.rotation, b.rotation, t);
            raft.transform.SetPositionAndRotation(position, rotation);
            raft.Body.position = position;
            raft.Body.rotation = rotation;
        }

        void SendEvent(NetEvent kind, int seat, float value)
        {
            if (!host || suppress)
            {
                return;
            }
            using (var writer = new FastBufferWriter(32, Allocator.Temp))
            {
                writer.WriteValueSafe((byte)kind);
                writer.WriteValueSafe((sbyte)seat);
                writer.WriteValueSafe(value);
                Vector3 outward = Vector3.zero;
                if (seat >= 0 && seat < riders.Length && riders[seat] != null)
                {
                    outward = riders[seat].transform.position - raft.transform.position;
                }
                writer.WriteValueSafe(outward);
                foreach (ulong client in room.RemoteClients())
                {
                    room.SendRaw(RoomMessages.Event, client, writer, NetworkDelivery.ReliableSequenced);
                }
            }
        }

        void ReadEvent(FastBufferReader reader)
        {
            reader.ReadValueSafe(out byte kindByte);
            reader.ReadValueSafe(out sbyte seat);
            reader.ReadValueSafe(out float value);
            reader.ReadValueSafe(out Vector3 outward);
            var kind = (NetEvent)kindByte;
            switch (kind)
            {
                case NetEvent.Barrel:
                    foreach (Barrel barrel in FindObjectsByType<Barrel>(FindObjectsSortMode.None))
                    {
                        if (barrel.index == (int)value)
                        {
                            barrel.Explode(raft, false);
                        }
                    }
                    break;
                case NetEvent.Damaged:
                case NetEvent.Broken:
                case NetEvent.Caught:
                case NetEvent.ShieldSaved:
                case NetEvent.Shocked:
                    if (health != null)
                    {
                        health.RaiseNetwork(kind, value);
                    }
                    break;
                case NetEvent.Pickup:
                    foreach (Pickup pickup in FindObjectsByType<Pickup>(FindObjectsSortMode.None))
                    {
                        if (pickup.index == (int)value)
                        {
                            pickup.Collect(raft.Boosts);
                        }
                    }
                    break;
                case NetEvent.RiderFell:
                case NetEvent.RiderBoarded:
                case NetEvent.RiderEliminated:
                case NetEvent.RiderReturned:
                    if (seat >= 0 && seat < riders.Length && riders[seat] != null)
                    {
                        RiderEvent change = kind == NetEvent.RiderFell ? RiderEvent.Fell : kind == NetEvent.RiderBoarded ? RiderEvent.Boarded : kind == NetEvent.RiderEliminated ? RiderEvent.Eliminated : RiderEvent.Returned;
                        riders[seat].ApplyNetworkEvent(change, outward);
                    }
                    break;
                case NetEvent.Respawned:
                    frames.Clear();
                    foreach (Rider rider in riders)
                    {
                        if (rider != null && rider.InWater)
                        {
                            rider.ApplyNetworkEvent(RiderEvent.Boarded, Vector3.zero);
                        }
                    }
                    if (bootstrap.course != null)
                    {
                        bootstrap.course.NetworkRespawn();
                    }
                    break;
                case NetEvent.Finished:
                    if (bootstrap.course != null)
                    {
                        bootstrap.course.NetworkFinish();
                    }
                    break;
            }
        }

        void OnEmoji(EmojiInfo emoji)
        {
            int emote = RoomMessages.EmoteOf(emoji.emoji);
            if (emote < 0 || this == null)
            {
                return;
            }
            for (int i = 0; i < seats.Length && i < riders.Length; i++)
            {
                if (seats[i] != null && !seats[i].bot && seats[i].owner == emoji.clientId && riders[i] != null)
                {
                    AvatarRig rig = riders[i].GetComponentInChildren<AvatarRig>();
                    if (rig != null)
                    {
                        rig.PlayEmote(emote);
                    }
                }
            }
        }

        void OnPickupTaken(Pickup pickup)
        {
            SendEvent(NetEvent.Pickup, -1, pickup.index);
        }

        void OnBarrelExploded(int index)
        {
            SendEvent(NetEvent.Barrel, -1, index);
        }

        void OnRiderState(Rider rider, RiderEvent change)
        {
            int seat = Array.IndexOf(riders, rider);
            if (seat < 0)
            {
                return;
            }
            NetEvent kind = change == RiderEvent.Fell ? NetEvent.RiderFell : change == RiderEvent.Boarded ? NetEvent.RiderBoarded : change == RiderEvent.Eliminated ? NetEvent.RiderEliminated : NetEvent.RiderReturned;
            SendEvent(kind, seat, 0f);
        }

        void SendReport(ScoreReport report)
        {
            if (!host || report == null)
            {
                return;
            }
            string json = JsonUtility.ToJson(report);
            using (var writer = new FastBufferWriter(json.Length * 2 + 16, Allocator.Temp, 1 << 18))
            {
                writer.WriteValueSafe(json);
                foreach (ulong client in room.RemoteClients())
                {
                    room.SendRaw(RoomMessages.Report, client, writer, NetworkDelivery.ReliableFragmentedSequenced);
                }
            }
        }

        void ReadReport(FastBufferReader reader)
        {
            reader.ReadValueSafe(out string json);
            ScoreReport report = JsonUtility.FromJson<ScoreReport>(json);
            if (report == null)
            {
                return;
            }
            int mine = localSeats.Count > 0 ? localSeats[0] : -1;
            foreach (PlayerScore player in report.players)
            {
                player.main = player.seat == mine;
            }
            RemoteReport = report;
        }

        void OnDeviceLeft(ulong clientId)
        {
            if (!host)
            {
                return;
            }
            BotDifficulty level = bootstrap.session != null && bootstrap.session.CurrentBotLevel != null
                ? bootstrap.session.CurrentBotLevel
                : bootstrap.catalog.botLevels != null && bootstrap.catalog.botLevels.Length > 2 ? bootstrap.catalog.botLevels[2] : null;
            for (int i = 0; i < seats.Length && i < riders.Length; i++)
            {
                if (seats[i] == null || seats[i].bot || seats[i].owner != clientId)
                {
                    continue;
                }
                seats[i].bot = true;
                remote.Remove(i);
                BotBrain bot = i < bootstrap.botBrains.Length ? bootstrap.botBrains[i] : null;
                if (bot != null)
                {
                    bot.difficulty = level;
                    bot.enabled = true;
                    riders[i].brain = bot;
                }
            }
        }
    }
}
