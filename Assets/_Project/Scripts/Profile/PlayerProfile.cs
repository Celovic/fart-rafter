using System;
using System.Collections.Generic;
using UnityEngine;

namespace FartRafters
{
    public static class PlayerProfile
    {
        public const int AdultAge = 13;

        const string BirthYearKey = "profile.birthyear";
        const string NameKey = "profile.name";
        const string FriendsKey = "parent.friends";
        const string InvitesKey = "parent.invites";
        const string ListedKey = "parent.listed";
        const string OnlineKey = "parent.online";
        const string ReminderKey = "profile.reminder";
        const string BlockedKey = "profile.blocked";
        const string ReportedKey = "profile.reported";

        public static event Action Changed;

        public static bool HasAge => PlayerPrefs.GetInt(BirthYearKey, 0) > 0;

        public static int BirthYear => PlayerPrefs.GetInt(BirthYearKey, 0);

        public static int Age => HasAge ? DateTime.Now.Year - BirthYear : 0;

        public static bool IsChild => !HasAge || Age < AdultAge;

        public static bool HasName => !string.IsNullOrEmpty(PlayerPrefs.GetString(NameKey, ""));

        public static string DisplayName => PlayerPrefs.GetString(NameKey, "");

        public static bool ReminderShown => PlayerPrefs.GetInt(ReminderKey, 0) == 1;

        public static bool FriendsAllowed => IsChild ? PlayerPrefs.GetInt(FriendsKey, 0) == 1 : true;

        public static bool InvitesAllowed => IsChild ? PlayerPrefs.GetInt(InvitesKey, 0) == 1 : true;

        public static bool LeaderboardVisible => IsChild ? PlayerPrefs.GetInt(ListedKey, 0) == 1 : true;

        public static bool OnlineAllowed => !IsChild || PlayerPrefs.GetInt(OnlineKey, 1) == 1;

        public static void SetBirthYear(int year)
        {
            PlayerPrefs.SetInt(BirthYearKey, year);
            if (IsChild && HasName && !NameGenerator.IsGenerated(DisplayName))
            {
                PlayerPrefs.SetString(NameKey, NameGenerator.ChildName(new System.Random()));
            }
            Save();
        }

        public static void SetName(string name)
        {
            PlayerPrefs.SetString(NameKey, name);
            Save();
        }

        public static void SetParental(bool friends, bool invites, bool listed, bool online)
        {
            PlayerPrefs.SetInt(FriendsKey, friends ? 1 : 0);
            PlayerPrefs.SetInt(InvitesKey, invites ? 1 : 0);
            PlayerPrefs.SetInt(ListedKey, listed ? 1 : 0);
            PlayerPrefs.SetInt(OnlineKey, online ? 1 : 0);
            Save();
        }

        public static bool ParentFriends => PlayerPrefs.GetInt(FriendsKey, 0) == 1;
        public static bool ParentInvites => PlayerPrefs.GetInt(InvitesKey, 0) == 1;
        public static bool ParentListed => PlayerPrefs.GetInt(ListedKey, 0) == 1;
        public static bool ParentOnline => PlayerPrefs.GetInt(OnlineKey, 1) == 1;

        public static void MarkReminderShown()
        {
            PlayerPrefs.SetInt(ReminderKey, 1);
            Save();
        }

        public static IReadOnlyCollection<string> Blocked => Read(BlockedKey);

        public static bool IsBlocked(string playerId)
        {
            return !string.IsNullOrEmpty(playerId) && Read(BlockedKey).Contains(playerId);
        }

        public static void Block(string playerId)
        {
            Add(BlockedKey, playerId);
        }

        public static void Unblock(string playerId)
        {
            HashSet<string> set = Read(BlockedKey);
            set.Remove(playerId);
            Write(BlockedKey, set);
        }

        public static void Report(string playerId, string name, string reason)
        {
            Add(ReportedKey, playerId + "|" + name + "|" + reason + "|" + DateTime.UtcNow.ToString("o"));
        }

        public static IReadOnlyCollection<string> PendingReports => Read(ReportedKey);

        public static void ClearReports()
        {
            Write(ReportedKey, new HashSet<string>());
        }

        static void Add(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            HashSet<string> set = Read(key);
            set.Add(value);
            Write(key, set);
        }

        static HashSet<string> Read(string key)
        {
            string raw = PlayerPrefs.GetString(key, "");
            return new HashSet<string>(raw.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }

        static void Write(string key, HashSet<string> set)
        {
            PlayerPrefs.SetString(key, string.Join("\n", set));
            Save();
        }

        static void Save()
        {
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
