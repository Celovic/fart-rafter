using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FartRafters
{
    public partial class MainMenu
    {
        enum FriendsTab
        {
            Friends,
            Requests,
            Find,
            Blocked
        }

        const float RowWidth = 1500f;
        const int FriendRows = 6;
        const int BoardRows = 8;

        RectTransform friendsScreen;
        RectTransform friendsButton;
        Image friendsBadge;
        Text friendsBadgeText;
        FriendsTab friendsTab;
        Image[] friendsTabImages;
        Text friendsMessage;
        RectTransform friendsList;
        RectTransform friendsFind;
        InputField friendInput;
        Text friendFindMessage;
        Text friendOwnTag;
        int friendsPage;
        Text friendsPageText;
        RectTransform friendsPager;

        RectTransform boardScreen;
        LeaderboardClient.Board board;
        Image[] boardTabImages;
        Text boardMessage;
        RectTransform boardList;
        Text boardOwn;
        int boardRequest;
        int boardPage;
        Text boardPageText;
        RectTransform boardPager;
        LeaderboardClient.Result boardResult;

        RectTransform toastRoot;
        Text toastText;
        CanvasGroup toastGroup;
        Coroutine toastRoutine;
        RectTransform invitePopup;
        Text inviteText;
        GameNotification shownInvite;
        bool servicesStarting;

        void BuildSocialScreens()
        {
            friendsScreen = Register(BuildFriends());
            boardScreen = Register(BuildBoard());
        }

        static RectTransform AddRow(RectTransform root, int index, float top, float height, string label, string detail, bool highlight, params (string label, Action action)[] actions)
        {
            RectTransform row = UiKit.CreateImage(root, "Row" + index, UiKit.Rounded, highlight ? new Color(1f, 0.82f, 0.15f, 0.35f) : new Color(0f, 0f, 0f, 0.45f), new Vector2(0.5f, 0.5f), new Vector2(0f, top - index * height), new Vector2(RowWidth, height - 12f));
            row.pivot = new Vector2(0.5f, 0.5f);
            row.GetComponent<Image>().type = Image.Type.Sliced;
            Text name = UiKit.CreateText(row, "Name", new Vector2(0f, 0.5f), new Vector2(30f, 0f), new Vector2(620f, height - 12f), 38, TextAnchor.MiddleLeft);
            name.text = label;
            name.horizontalOverflow = HorizontalWrapMode.Overflow;
            Text info = UiKit.CreateText(row, "Detail", new Vector2(0f, 0.5f), new Vector2(660f, 0f), new Vector2(300f, height - 12f), 28, TextAnchor.MiddleLeft);
            info.text = detail;
            info.color = new Color(1f, 1f, 1f, 0.75f);
            const float buttonWidth = 200f;
            for (int i = 0; i < actions.Length; i++)
            {
                (string text, Action action) = actions[actions.Length - 1 - i];
                Text button = UiKit.CreateButton(row, text, new Vector2(1f, 0.5f), new Vector2(-16f - i * (buttonWidth + 12f), 0f), new Vector2(buttonWidth, height - 32f), action);
                button.fontSize = 30;
            }
            return row;
        }

        static void ClearChildren(RectTransform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Destroy(root.GetChild(i).gameObject);
            }
        }

        Image[] CreateTabs(RectTransform screen, string[] labels, Action<int> select)
        {
            var images = new Image[labels.Length];
            const float width = 360f;
            const float gap = 20f;
            float start = -(labels.Length - 1) * (width + gap) * 0.5f;
            for (int i = 0; i < labels.Length; i++)
            {
                int index = i;
                Text tab = UiKit.CreateCenterButton(screen, labels[i], new Vector2(start + i * (width + gap), 370f), new Vector2(width, 90f), () => select(index));
                images[i] = tab.transform.parent.GetComponent<Image>();
            }
            return images;
        }

        static void HighlightTab(Image[] tabs, int active)
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                tabs[i].color = i == active ? UiKit.AccentColor : UiKit.ButtonColor;
            }
        }

        RectTransform CreatePager(RectTransform screen, out Text label, Action<int> step)
        {
            RectTransform pager = UiKit.CreateGroup(screen, "Pager");
            UiKit.CreateCenterButton(pager, "<", new Vector2(-200f, -430f), new Vector2(110f, 90f), () => step(-1)).fontSize = 50;
            UiKit.CreateCenterButton(pager, ">", new Vector2(200f, -430f), new Vector2(110f, 90f), () => step(1)).fontSize = 50;
            label = UiKit.CreateCenterText(pager, "Page", new Vector2(0f, -430f), new Vector2(260f, 80f), 34);
            return pager;
        }

        RectTransform BuildFriends()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Friends");
            Text title = UiKit.CreateText(screen, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1200f, 100f), 68, TextAnchor.UpperCenter);
            title.text = "Arkadaşlar";
            UiKit.AddOutline(title, 3f);
            UiKit.CreateButton(screen, "Geri", new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(200f, 90f), ShowHome);
            friendsTabImages = CreateTabs(screen, new[] { "Arkadaşlarım", "İstekler", "Arkadaş Bul", "Engellenenler" }, index =>
            {
                friendsTab = (FriendsTab)index;
                friendsPage = 0;
                RefreshFriends();
            });
            friendsMessage = UiKit.CreateCenterText(screen, "Message", new Vector2(0f, 290f), new Vector2(1600f, 60f), 32);
            friendsList = UiKit.CreateGroup(screen, "List");
            friendsPager = CreatePager(screen, out friendsPageText, step =>
            {
                friendsPage += step;
                RefreshFriends();
            });

            friendsFind = UiKit.CreateGroup(screen, "Find");
            friendOwnTag = UiKit.CreateCenterText(friendsFind, "Own", new Vector2(0f, 190f), new Vector2(1600f, 70f), 42);
            friendOwnTag.color = UiKit.StarOnColor;
            Text info = UiKit.CreateCenterText(friendsFind, "Info", new Vector2(0f, 110f), new Vector2(1600f, 60f), 30);
            info.text = "Arkadaşının adını #etiketiyle birlikte yaz (ör. Gaz Kaptanı#1234).";
            friendInput = UiKit.CreateInput(friendsFind, "Ad#1234", new Vector2(0f, 0f), new Vector2(900f, 110f), 50, 48);
            UiKit.CreateCenterButton(friendsFind, "İstek Gönder", new Vector2(0f, -140f), new Vector2(460f, 110f), SendFriendRequest);
            friendFindMessage = UiKit.CreateCenterText(friendsFind, "Result", new Vector2(0f, -250f), new Vector2(1600f, 60f), 32);
            return screen;
        }

        void ShowFriends()
        {
            friendsPage = 0;
            friendFindMessage.text = "";
            Show(friendsScreen);
            RefreshFriends();
            if (PlayerProfile.FriendsAllowed && !FriendsClient.Initialized)
            {
                StartServices();
            }
        }

        async void SendFriendRequest()
        {
            friendFindMessage.text = "Gönderiliyor…";
            string result = await FriendsClient.AddByName(friendInput.text);
            if (this == null)
            {
                return;
            }
            friendFindMessage.text = result;
            RefreshFriends();
        }

        void RefreshFriends()
        {
            if (friendsScreen == null || currentScreen != friendsScreen)
            {
                return;
            }
            HighlightTab(friendsTabImages, (int)friendsTab);
            ClearChildren(friendsList);
            bool find = friendsTab == FriendsTab.Find;
            friendsFind.gameObject.SetActive(find);
            friendsPager.gameObject.SetActive(false);
            friendsMessage.text = "";
            if (!PlayerProfile.FriendsAllowed)
            {
                friendsFind.gameObject.SetActive(false);
                friendsMessage.text = "Arkadaş özelliği kapalı. Bir yetişkin Ayarlar > Ebeveyn Kontrolü'nden açabilir.";
                return;
            }
            if (!FriendsClient.Initialized)
            {
                friendsMessage.text = !string.IsNullOrEmpty(FriendsClient.Message) ? FriendsClient.Message : !string.IsNullOrEmpty(OnlineServices.Message) ? OnlineServices.Message : "Bağlanıyor…";
                return;
            }
            if (find)
            {
                string tag = OnlineServices.ServiceName;
                int hash = tag.LastIndexOf('#');
                friendOwnTag.text = string.IsNullOrEmpty(tag) ? "" : "Senin adın: " + NameGenerator.FromServiceName(tag) + (hash > 0 ? tag.Substring(hash) : "");
                return;
            }
            var rows = new List<(string label, string detail, (string, Action)[] actions)>();
            if (friendsTab == FriendsTab.Friends)
            {
                foreach (FriendsClient.Person person in FriendsClient.Friends())
                {
                    rows.Add((person.name, FriendStatus(person), FriendActions(person)));
                }
                if (rows.Count == 0)
                {
                    friendsMessage.text = "Henüz arkadaşın yok. \"Arkadaş Bul\" sekmesinden ekleyebilirsin.";
                }
            }
            else if (friendsTab == FriendsTab.Requests)
            {
                foreach (GameNotification invite in NotificationCenter.Items)
                {
                    if (invite.kind != NotificationKind.RoomInvite)
                    {
                        continue;
                    }
                    GameNotification captured = invite;
                    rows.Add((invite.playerName, "oda daveti", new (string, Action)[]
                    {
                        ("Katıl", () => AcceptInvite(captured)),
                        ("Sil", () => NotificationCenter.Remove(captured))
                    }));
                }
                foreach (FriendsClient.Person person in FriendsClient.Incoming())
                {
                    string id = person.id;
                    string name = person.name;
                    rows.Add((name, "seni ekledi", new (string, Action)[]
                    {
                        ("Kabul Et", () => RunFriends(FriendsClient.Accept(id))),
                        ("Reddet", () => RunFriends(FriendsClient.Decline(id))),
                        ("Engelle", () => ConfirmBlock(id, name))
                    }));
                }
                foreach (FriendsClient.Person person in FriendsClient.Outgoing())
                {
                    string id = person.id;
                    rows.Add((person.name, "yanıt bekleniyor", new (string, Action)[] { ("İptal", () => RunFriends(FriendsClient.Cancel(id))) }));
                }
                if (rows.Count == 0)
                {
                    friendsMessage.text = "Bekleyen istek ya da davet yok.";
                }
            }
            else
            {
                foreach (FriendsClient.Person person in FriendsClient.Blocked())
                {
                    string id = person.id;
                    rows.Add((person.name, "engelli", new (string, Action)[] { ("Engeli Kaldır", () => RunFriends(FriendsClient.Unblock(id))) }));
                }
                if (rows.Count == 0)
                {
                    friendsMessage.text = "Engellediğin kimse yok.";
                }
            }
            int pages = Mathf.Max(1, (rows.Count + FriendRows - 1) / FriendRows);
            friendsPage = Mathf.Clamp(friendsPage, 0, pages - 1);
            friendsPager.gameObject.SetActive(pages > 1);
            friendsPageText.text = (friendsPage + 1) + " / " + pages;
            for (int i = 0; i < FriendRows; i++)
            {
                int index = friendsPage * FriendRows + i;
                if (index >= rows.Count)
                {
                    break;
                }
                AddRow(friendsList, i, 200f, 104f, rows[index].label, rows[index].detail, false, rows[index].actions);
            }
        }

        static string FriendStatus(FriendsClient.Person person)
        {
            if (!person.online)
            {
                return "çevrimdışı";
            }
            return string.IsNullOrEmpty(person.status) ? "çevrimiçi" : person.status;
        }

        (string, Action)[] FriendActions(FriendsClient.Person person)
        {
            var actions = new List<(string, Action)>();
            string id = person.id;
            string name = person.name;
            OnlineRoom room = OnlineRoom.Current;
            if (OnlineRoom.Active && room.IsHost && !room.State.direct && PlayerProfile.InvitesAllowed)
            {
                actions.Add(("Davet Et", () => InviteFriend(id, name)));
            }
            else if (!OnlineRoom.Active && person.online && person.status == "odada" && !string.IsNullOrEmpty(person.code) && PlayerProfile.InvitesAllowed && PlayerProfile.OnlineAllowed)
            {
                string code = person.code;
                actions.Add(("Katıl", () => JoinRoom(false, code)));
            }
            actions.Add(("Sil", () => Confirm(name + " arkadaş listenden çıkarılsın mı?", () =>
            {
                RunFriends(FriendsClient.Remove(id));
                ShowFriends();
            })));
            actions.Add(("Engelle", () => ConfirmBlock(id, name)));
            return actions.ToArray();
        }

        void ConfirmBlock(string playerId, string name)
        {
            Confirm(name + " engellensin mi? Sana istek ve davet gönderemeyecek.", () =>
            {
                RunFriends(FriendsClient.Block(playerId));
                ShowFriends();
            });
        }

        async void InviteFriend(string playerId, string name)
        {
            OnlineRoom room = OnlineRoom.Current;
            if (room == null)
            {
                return;
            }
            bool sent = await FriendsClient.Invite(playerId, room.State.code);
            if (this != null)
            {
                Toast(sent ? name + " odaya davet edildi." : "Davet gönderilemedi.");
            }
        }

        async void RunFriends(System.Threading.Tasks.Task task)
        {
            await task;
            if (this != null)
            {
                RefreshFriends();
                UpdateFriendsBadge();
            }
        }

        void UpdateFriendsBadge()
        {
            if (friendsBadge == null)
            {
                return;
            }
            int count = 0;
            foreach (GameNotification item in NotificationCenter.Items)
            {
                if (item.kind != NotificationKind.Info)
                {
                    count++;
                }
            }
            friendsBadge.gameObject.SetActive(count > 0);
            friendsBadgeText.text = count > 9 ? "9+" : count.ToString();
        }

        RectTransform BuildBoard()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Leaderboard");
            Text title = UiKit.CreateText(screen, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1200f, 100f), 68, TextAnchor.UpperCenter);
            title.text = "Puan Listesi";
            UiKit.AddOutline(title, 3f);
            UiKit.CreateButton(screen, "Geri", new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(200f, 90f), ShowHome);
            boardTabImages = CreateTabs(screen, new[] { "Tüm Zamanlar", "Haftalık", "Arkadaşlar" }, index =>
            {
                board = (LeaderboardClient.Board)index;
                LoadBoard();
            });
            boardMessage = UiKit.CreateCenterText(screen, "Message", new Vector2(0f, 290f), new Vector2(1600f, 60f), 32);
            boardList = UiKit.CreateGroup(screen, "List");
            boardOwn = UiKit.CreateCenterText(screen, "Own", new Vector2(0f, -340f), new Vector2(1600f, 60f), 36);
            boardOwn.color = UiKit.StarOnColor;
            boardPager = CreatePager(screen, out boardPageText, step =>
            {
                boardPage += step;
                FillBoard();
            });
            return screen;
        }

        void ShowLeaderboard()
        {
            Show(boardScreen);
            LoadBoard();
        }

        async void LoadBoard()
        {
            int request = ++boardRequest;
            HighlightTab(boardTabImages, (int)board);
            ClearChildren(boardList);
            boardPager.gameObject.SetActive(false);
            boardOwn.text = "";
            boardResult = null;
            boardPage = 0;
            if (board == LeaderboardClient.Board.Friends && !PlayerProfile.FriendsAllowed)
            {
                boardMessage.text = "Arkadaş özelliği kapalı. Bir yetişkin Ayarlar > Ebeveyn Kontrolü'nden açabilir.";
                return;
            }
            boardMessage.text = "Yükleniyor…";
            if (board == LeaderboardClient.Board.Friends)
            {
                await FriendsClient.EnsureReady();
            }
            LeaderboardClient.Result result = await LeaderboardClient.Load(board);
            if (this == null || request != boardRequest || currentScreen != boardScreen)
            {
                return;
            }
            boardResult = result;
            FillBoard();
        }

        void FillBoard()
        {
            ClearChildren(boardList);
            LeaderboardClient.Result result = boardResult;
            if (result == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(result.error))
            {
                boardMessage.text = result.error;
                return;
            }
            boardMessage.text = result.rows.Count == 0
                ? (board == LeaderboardClient.Board.Friends ? "Arkadaşların henüz puan toplamadı." : "Liste henüz boş. İlk sen ol!")
                : PlayerProfile.LeaderboardVisible ? "" : "Ebeveyn kontrolü nedeniyle sen listede görünmüyorsun.";
            int pages = Mathf.Max(1, (result.rows.Count + BoardRows - 1) / BoardRows);
            boardPage = Mathf.Clamp(boardPage, 0, pages - 1);
            boardPager.gameObject.SetActive(pages > 1);
            boardPageText.text = (boardPage + 1) + " / " + pages;
            for (int i = 0; i < BoardRows; i++)
            {
                int index = boardPage * BoardRows + i;
                if (index >= result.rows.Count)
                {
                    break;
                }
                LeaderboardClient.Row row = result.rows[index];
                AddRow(boardList, i, 215f, 66f, row.rank + ".  " + row.name, row.score + " puan", row.me);
            }
            boardOwn.text = result.me != null ? "Sen: " + result.me.rank + ". sıra · " + result.me.score + " puan" : PlayerProfile.LeaderboardVisible ? "Henüz bu listede puanın yok." : "";
        }

        void BuildNotifications()
        {
            toastRoot = UiKit.CreateImage(canvasRect, "Toast", UiKit.Rounded, UiKit.PanelColor, new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(1300f, 110f));
            toastRoot.GetComponent<Image>().type = Image.Type.Sliced;
            toastGroup = toastRoot.gameObject.AddComponent<CanvasGroup>();
            toastGroup.blocksRaycasts = false;
            toastText = UiKit.CreateCenterText(toastRoot, "Text", Vector2.zero, new Vector2(1260f, 100f), 34);
            toastRoot.gameObject.SetActive(false);

            invitePopup = UiKit.CreateImage(canvasRect, "Invite", UiKit.Rounded, UiKit.PanelColor, new Vector2(0.5f, 1f), new Vector2(0f, -270f), new Vector2(1300f, 230f));
            Image inviteImage = invitePopup.GetComponent<Image>();
            inviteImage.type = Image.Type.Sliced;
            inviteImage.raycastTarget = true;
            inviteText = UiKit.CreateText(invitePopup, "Text", new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(1240f, 80f), 38, TextAnchor.UpperCenter);
            UiKit.CreateButton(invitePopup, "Katıl", new Vector2(0.5f, 0f), new Vector2(-180f, 25f), new Vector2(300f, 100f), () =>
            {
                GameNotification invite = shownInvite;
                HideInvite();
                if (invite != null)
                {
                    AcceptInvite(invite);
                }
            });
            UiKit.CreateButton(invitePopup, "Sonra", new Vector2(0.5f, 0f), new Vector2(180f, 25f), new Vector2(300f, 100f), HideInvite);
            invitePopup.gameObject.SetActive(false);

            NotificationCenter.Added += OnNotification;
            NotificationCenter.Changed += OnNotificationsChanged;
            FriendsClient.Changed += OnFriendsChanged;
            OnlineServices.StateChanged += OnServicesChanged;
            UpdateFriendsBadge();
        }

        void OnDestroy()
        {
            NotificationCenter.Added -= OnNotification;
            NotificationCenter.Changed -= OnNotificationsChanged;
            FriendsClient.Changed -= OnFriendsChanged;
            OnlineServices.StateChanged -= OnServicesChanged;
            BindRoom(null);
        }

        void OnNotification(GameNotification notification)
        {
            if (notification.kind == NotificationKind.RoomInvite)
            {
                if (!OnlineRoom.Active)
                {
                    shownInvite = notification;
                    inviteText.text = notification.text;
                    invitePopup.SetAsLastSibling();
                    invitePopup.gameObject.SetActive(true);
                }
                return;
            }
            Toast(notification.text);
        }

        void HideInvite()
        {
            shownInvite = null;
            invitePopup.gameObject.SetActive(false);
        }

        void AcceptInvite(GameNotification invite)
        {
            NotificationCenter.Remove(invite);
            if (OnlineRoom.Active)
            {
                Toast("Önce bulunduğun odadan çık.");
                return;
            }
            if (!PlayerProfile.OnlineAllowed)
            {
                Toast("Online oyun ebeveyn kontrolünden kapatılmış.");
                return;
            }
            JoinRoom(false, invite.code);
        }

        void OnNotificationsChanged()
        {
            UpdateFriendsBadge();
            RefreshFriends();
        }

        void OnFriendsChanged()
        {
            UpdateFriendsBadge();
            RefreshFriends();
        }

        void OnServicesChanged()
        {
            RefreshOnlineStatus();
            RefreshFriends();
        }

        void Toast(string message)
        {
            if (toastRoot == null || string.IsNullOrEmpty(message))
            {
                return;
            }
            if (toastRoutine != null)
            {
                StopCoroutine(toastRoutine);
            }
            toastText.text = message;
            toastRoot.SetAsLastSibling();
            toastRoot.gameObject.SetActive(true);
            toastRoutine = StartCoroutine(FadeToast());
        }

        IEnumerator FadeToast()
        {
            toastGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(3f);
            for (float t = 0f; t < 0.4f; t += Time.unscaledDeltaTime)
            {
                toastGroup.alpha = 1f - t / 0.4f;
                yield return null;
            }
            toastRoot.gameObject.SetActive(false);
            toastRoutine = null;
        }

        async void StartServices()
        {
            if (servicesStarting)
            {
                return;
            }
            servicesStarting = true;
            try
            {
                if (await OnlineServices.EnsureReady())
                {
                    if (PlayerProfile.FriendsAllowed)
                    {
                        await FriendsClient.EnsureReady();
                    }
                    ScoreSync.Flush();
                }
            }
            finally
            {
                servicesStarting = false;
            }
            if (this == null)
            {
                return;
            }
            UpdateFriendsBadge();
            RefreshFriends();
            RefreshOnlineStatus();
        }
    }
}
