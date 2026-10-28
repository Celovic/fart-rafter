using System;
using System.Collections.Generic;

namespace FartRafters
{
    public enum NotificationKind
    {
        FriendRequest,
        RoomInvite,
        Info
    }

    public class GameNotification
    {
        public NotificationKind kind;
        public string text;
        public string playerId;
        public string playerName;
        public string code;
        public DateTime time = DateTime.Now;
    }

    public static class NotificationCenter
    {
        static readonly List<GameNotification> items = new List<GameNotification>();

        public static IReadOnlyList<GameNotification> Items => items;
        public static int Count => items.Count;
        public static event Action<GameNotification> Added;
        public static event Action Changed;

        public static void Add(GameNotification notification)
        {
            if (notification.kind != NotificationKind.Info)
            {
                items.RemoveAll(item => item.kind == notification.kind && item.playerId == notification.playerId);
            }
            items.Add(notification);
            Added?.Invoke(notification);
            Changed?.Invoke();
        }

        public static void Remove(GameNotification notification)
        {
            if (items.Remove(notification))
            {
                Changed?.Invoke();
            }
        }

        public static void RemoveFor(NotificationKind kind, string playerId)
        {
            if (items.RemoveAll(item => item.kind == kind && item.playerId == playerId) > 0)
            {
                Changed?.Invoke();
            }
        }
    }
}
