using System;
using System.Collections.Generic;

namespace FartRafters
{
    [Serializable]
    public class DeviceInfo
    {
        public ulong clientId;
        public string playerId;
        public string name;
        public int locals = 1;
        public int[] avatars = { 0, 1, 2 };
        public bool ready;
        public bool child;
    }

    [Serializable]
    public class SeatInfo
    {
        public ulong owner;
        public int local;
        public string name;
        public int avatar;
        public bool bot;
        public string playerId;
    }

    [Serializable]
    public class SpectatorInfo
    {
        public ulong owner;
        public int local;
        public string name;
    }

    [Serializable]
    public class RoomState
    {
        public string code;
        public bool direct;
        public bool adventure;
        public int level;
        public int difficulty;
        public int bots;
        public int botLevel = 1;
        public int biome;
        public int raft;
        public int weather;
        public List<DeviceInfo> devices = new List<DeviceInfo>();
        public List<SeatInfo> seats = new List<SeatInfo>();
        public List<SpectatorInfo> spectators = new List<SpectatorInfo>();

        public DeviceInfo Device(ulong clientId)
        {
            return devices.Find(device => device.clientId == clientId);
        }

        public int HumanSeats
        {
            get
            {
                int count = 0;
                foreach (SeatInfo seat in seats)
                {
                    if (!seat.bot)
                    {
                        count++;
                    }
                }
                return count;
            }
        }
    }

    [Serializable]
    public class StartInfo
    {
        public int seed;
        public bool adventure;
        public int level;
        public int difficulty;
        public int botLevel;
        public int biome;
        public int raft;
        public int weather;
        public List<SeatInfo> seats = new List<SeatInfo>();
    }

    [Serializable]
    public class EmojiInfo
    {
        public ulong clientId;
        public string name;
        public int emoji;
    }

    public enum NetEvent : byte
    {
        Damaged,
        Broken,
        Caught,
        ShieldSaved,
        Pickup,
        Checkpoint,
        RiderFell,
        RiderBoarded,
        RiderEliminated,
        RiderReturned,
        Respawned,
        Finished,
        Shocked,
        Barrel
    }

    public static class RoomMessages
    {
        public const string Hello = "fr.hello";
        public const string Room = "fr.room";
        public const string Ready = "fr.ready";
        public const string Emoji = "fr.emoji";
        public const string Kick = "fr.kick";
        public const string Start = "fr.start";
        public const string Input = "fr.input";
        public const string Snapshot = "fr.snap";
        public const string Event = "fr.event";
        public const string Report = "fr.report";
        public const string Lobby = "fr.lobby";
        public const string Rematch = "fr.rematch";

        public static readonly string[] Emojis = { "Haydi!", "Bas gaza!", "Süper!", "Ups!", "Bekleyin!", "Hahaha!" };
        public static readonly string[] Emotes = { "Dans", "El Salla", "Zıpla", "Alkış", "Kas Göster" };

        public static int Count => Emojis.Length + Emotes.Length;

        public static int EmoteOf(int index)
        {
            return index >= Emojis.Length && index < Count ? index - Emojis.Length : -1;
        }

        public static string Describe(int index)
        {
            int emote = EmoteOf(index);
            return emote >= 0 ? "[" + Emotes[emote] + "]" : index >= 0 && index < Emojis.Length ? Emojis[index] : "";
        }
    }

    public static class SeatPlanner
    {
        public static void Assign(RoomState room, int maxSeats)
        {
            room.seats.Clear();
            room.spectators.Clear();
            if (room.devices.Count == 0)
            {
                return;
            }
            DeviceInfo host = room.devices[0];
            for (int i = 0; i < host.locals && room.seats.Count < maxSeats; i++)
            {
                room.seats.Add(Seat(host, i));
            }
            for (int d = 1; d < room.devices.Count; d++)
            {
                if (room.seats.Count < maxSeats)
                {
                    room.seats.Add(Seat(room.devices[d], 0));
                }
            }
            for (int d = 1; d < room.devices.Count; d++)
            {
                DeviceInfo device = room.devices[d];
                for (int i = 1; i < device.locals && room.seats.Count < maxSeats; i++)
                {
                    room.seats.Add(Seat(device, i));
                }
            }
            foreach (DeviceInfo device in room.devices)
            {
                for (int i = 0; i < device.locals; i++)
                {
                    if (!room.seats.Exists(seat => !seat.bot && seat.owner == device.clientId && seat.local == i))
                    {
                        room.spectators.Add(new SpectatorInfo { owner = device.clientId, local = i, name = LocalName(device, i) });
                    }
                }
            }
            room.bots = Math.Max(0, Math.Min(room.bots, maxSeats - room.seats.Count));
            for (int b = 0; b < room.bots; b++)
            {
                room.seats.Add(new SeatInfo { owner = host.clientId, local = -1, name = "Bot", avatar = (b * 3 + 4) % 8 + (b % 3) * 100, bot = true });
            }
        }

        static SeatInfo Seat(DeviceInfo device, int local)
        {
            int avatar = device.avatars != null && local < device.avatars.Length ? device.avatars[local] : local;
            return new SeatInfo { owner = device.clientId, local = local, name = LocalName(device, local), avatar = avatar, playerId = local == 0 ? device.playerId : "" };
        }

        public static string LocalName(DeviceInfo device, int local)
        {
            return local == 0 ? device.name : device.name + " +" + local;
        }
    }
}
