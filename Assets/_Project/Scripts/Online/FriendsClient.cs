using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Friends;
using Unity.Services.Friends.Models;
using Unity.Services.Friends.Notifications;
using UnityEngine;

namespace FartRafters
{
    public static class FriendsClient
    {
        public class Activity
        {
            public string status;
            public string code;
        }

        public class RoomInvite
        {
            public string code;
            public string host;
        }

        public class Person
        {
            public string id;
            public string name;
            public bool online;
            public string status;
            public string code;
        }

        static bool initialized;
        static Task<bool> pending;

        public static bool Initialized => initialized;
        public static string Message { get; private set; } = "";
        public static event Action Changed;

        public static Task<bool> EnsureReady()
        {
            if (initialized)
            {
                return Task.FromResult(true);
            }
            if (pending == null || pending.IsCompleted)
            {
                pending = Initialize();
            }
            return pending;
        }

        static async Task<bool> Initialize()
        {
            if (!PlayerProfile.FriendsAllowed)
            {
                Message = "Arkadaş özelliği ebeveyn kontrolünden açılmalı.";
                return false;
            }
            if (!await OnlineServices.EnsureReady())
            {
                Message = OnlineServices.Message;
                return false;
            }
            try
            {
                await FriendsService.Instance.InitializeAsync();
                FriendsService.Instance.RelationshipAdded += OnRelationshipAdded;
                FriendsService.Instance.RelationshipDeleted += OnRelationshipDeleted;
                FriendsService.Instance.MessageReceived += OnMessage;
                FriendsService.Instance.PresenceUpdated += OnPresence;
                initialized = true;
                await SetActivity("menü", "");
                foreach (Relationship request in FriendsService.Instance.IncomingFriendRequests)
                {
                    NotifyRequest(request.Member);
                }
                Changed?.Invoke();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Friends: " + exception.Message);
                Message = "Arkadaş listesine bağlanılamadı.";
                return false;
            }
        }

        public static Task Shutdown()
        {
            if (initialized)
            {
                FriendsService.Instance.RelationshipAdded -= OnRelationshipAdded;
                FriendsService.Instance.RelationshipDeleted -= OnRelationshipDeleted;
                FriendsService.Instance.MessageReceived -= OnMessage;
                FriendsService.Instance.PresenceUpdated -= OnPresence;
            }
            initialized = false;
            return Task.CompletedTask;
        }

        public static IEnumerable<string> FriendIds()
        {
            if (!initialized)
            {
                yield break;
            }
            foreach (Relationship relationship in FriendsService.Instance.Friends)
            {
                yield return relationship.Member.Id;
            }
        }

        public static List<Person> Friends()
        {
            var list = new List<Person>();
            if (!initialized)
            {
                return list;
            }
            foreach (Relationship relationship in FriendsService.Instance.Friends)
            {
                Member member = relationship.Member;
                var person = ToPerson(member);
                if (member.Presence != null)
                {
                    Availability availability = member.Presence.Availability;
                    person.online = availability == Availability.Online || availability == Availability.Busy || availability == Availability.Away;
                    Activity activity = null;
                    try
                    {
                        activity = member.Presence.GetActivity<Activity>();
                    }
                    catch (Exception)
                    {
                        activity = null;
                    }
                    person.status = activity != null ? activity.status : "";
                    person.code = activity != null ? activity.code : "";
                }
                list.Add(person);
            }
            list.Sort((a, b) => b.online.CompareTo(a.online));
            return list;
        }

        public static List<Person> Incoming()
        {
            return Map(initialized ? FriendsService.Instance.IncomingFriendRequests : null);
        }

        public static List<Person> Outgoing()
        {
            return Map(initialized ? FriendsService.Instance.OutgoingFriendRequests : null);
        }

        public static List<Person> Blocked()
        {
            return Map(initialized ? FriendsService.Instance.Blocks : null);
        }

        static List<Person> Map(IReadOnlyList<Relationship> relationships)
        {
            var list = new List<Person>();
            if (relationships == null)
            {
                return list;
            }
            foreach (Relationship relationship in relationships)
            {
                list.Add(ToPerson(relationship.Member));
            }
            return list;
        }

        static Person ToPerson(Member member)
        {
            string name = member.Profile != null ? member.Profile.Name : "";
            return new Person { id = member.Id, name = NameGenerator.FromServiceName(name) };
        }

        public static async Task<string> AddByName(string input)
        {
            if (!await EnsureReady())
            {
                return Message;
            }
            string trimmed = (input ?? "").Trim();
            int hash = trimmed.LastIndexOf('#');
            if (hash <= 0 || hash == trimmed.Length - 1)
            {
                return "İsmin sonundaki #etiketi de yaz (ör. Gaz Kaptanı#1234).";
            }
            string name = NameGenerator.ToServiceName(trimmed.Substring(0, hash)) + "#" + trimmed.Substring(hash + 1);
            try
            {
                await FriendsService.Instance.AddFriendByNameAsync(name);
                Changed?.Invoke();
                return "İstek gönderildi. Kabul edince arkadaş olacaksınız.";
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Add friend: " + exception.Message);
                return "Bu isimde bir oyuncu bulunamadı.";
            }
        }

        public static async Task<string> AddById(string playerId)
        {
            if (!await EnsureReady())
            {
                return Message;
            }
            try
            {
                await FriendsService.Instance.AddFriendAsync(playerId);
                Changed?.Invoke();
                return "Arkadaşlık isteği gönderildi.";
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Add friend: " + exception.Message);
                return "İstek gönderilemedi.";
            }
        }

        public static async Task Accept(string playerId)
        {
            await Run(() => FriendsService.Instance.AddFriendAsync(playerId));
            NotificationCenter.RemoveFor(NotificationKind.FriendRequest, playerId);
        }

        public static async Task Decline(string playerId)
        {
            await Run(() => FriendsService.Instance.DeleteIncomingFriendRequestAsync(playerId));
            NotificationCenter.RemoveFor(NotificationKind.FriendRequest, playerId);
        }

        public static Task Cancel(string playerId)
        {
            return Run(() => FriendsService.Instance.DeleteOutgoingFriendRequestAsync(playerId));
        }

        public static Task Remove(string playerId)
        {
            return Run(() => FriendsService.Instance.DeleteFriendAsync(playerId));
        }

        public static async Task Block(string playerId)
        {
            PlayerProfile.Block(playerId);
            await Run(() => FriendsService.Instance.AddBlockAsync(playerId));
            NotificationCenter.RemoveFor(NotificationKind.FriendRequest, playerId);
        }

        public static async Task Unblock(string playerId)
        {
            PlayerProfile.Unblock(playerId);
            await Run(() => FriendsService.Instance.DeleteBlockAsync(playerId));
        }

        public static async Task<bool> Invite(string playerId, string code)
        {
            if (!PlayerProfile.InvitesAllowed || !initialized)
            {
                return false;
            }
            try
            {
                await FriendsService.Instance.MessageAsync(playerId, new RoomInvite { code = code, host = PlayerProfile.DisplayName });
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Invite: " + exception.Message);
                return false;
            }
        }

        public static async Task SetActivity(string status, string code)
        {
            if (!initialized)
            {
                return;
            }
            try
            {
                await FriendsService.Instance.SetPresenceAsync(Availability.Online, new Activity { status = status, code = code });
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Presence: " + exception.Message);
            }
        }

        static async Task Run(Func<Task> action)
        {
            if (!await EnsureReady())
            {
                return;
            }
            try
            {
                await action();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Friends action: " + exception.Message);
            }
            Changed?.Invoke();
        }

        static void OnRelationshipAdded(IRelationshipAddedEvent added)
        {
            Relationship relationship = added.Relationship;
            if (relationship != null && relationship.Type == RelationshipType.FriendRequest && relationship.Member != null && relationship.Member.Role == MemberRole.Source)
            {
                NotifyRequest(relationship.Member);
            }
            Changed?.Invoke();
        }

        static string FriendName(string playerId)
        {
            foreach (Relationship relationship in FriendsService.Instance.Friends)
            {
                if (relationship.Member != null && relationship.Member.Id == playerId)
                {
                    return ToPerson(relationship.Member).name;
                }
            }
            return null;
        }

        static void NotifyRequest(Member member)
        {
            if (member == null || PlayerProfile.IsBlocked(member.Id))
            {
                return;
            }
            Person person = ToPerson(member);
            NotificationCenter.Add(new GameNotification
            {
                kind = NotificationKind.FriendRequest,
                playerId = person.id,
                playerName = person.name,
                text = person.name + " seninle arkadaş olmak istiyor."
            });
        }

        static void OnRelationshipDeleted(IRelationshipDeletedEvent deleted)
        {
            Changed?.Invoke();
        }

        static void OnPresence(IPresenceUpdatedEvent updated)
        {
            Changed?.Invoke();
        }

        static void OnMessage(IMessageReceivedEvent message)
        {
            if (!PlayerProfile.InvitesAllowed || PlayerProfile.IsBlocked(message.UserId))
            {
                return;
            }
            RoomInvite invite;
            try
            {
                invite = message.GetAs<RoomInvite>();
            }
            catch (Exception)
            {
                return;
            }
            if (invite == null || string.IsNullOrEmpty(invite.code))
            {
                return;
            }
            string host = FriendName(message.UserId);
            if (host == null)
            {
                return;
            }
            NotificationCenter.Add(new GameNotification
            {
                kind = NotificationKind.RoomInvite,
                playerId = message.UserId,
                playerName = host,
                code = invite.code,
                text = host + " seni odasına çağırıyor."
            });
        }
    }
}
