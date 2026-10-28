using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FartRafters
{
    public partial class MainMenu
    {
        const int MaxLocals = 3;
        const int SpectatorSlots = 4;

        static int onlineLocals = 1;
        static bool onlineDirect;

        RectTransform onlineScreen;
        RectTransform joinScreen;
        RectTransform lobbyScreen;
        bool roomPicking;
        bool onlineBusy;
        OnlineRoom boundRoom;

        Text onlineStatus;
        Text directText;
        RectTransform onlineActions;

        Text joinTitle;
        Text joinDisplay;
        Text joinMessage;
        RectTransform joinDot;
        string joinDigits = "";

        Text lobbyCode;
        Text lobbyStatus;
        Text[] seatTexts;
        Image[] seatImages;
        Text[] spectatorTexts;
        Text spectatorHeader;
        Text mapText;
        Text roomBiomeText;
        Text roomBotsText;
        Text roomBotLevelText;
        Text roomRaftText;
        Text roomWeatherText;
        Button[] roomSettingButtons;
        Text clientNote;
        RectTransform inviteButton;
        Text localsText;
        Text[] localAvatarTexts;
        Text readyText;
        Image readyImage;
        Text emojiFeed;
        RectTransform emojiPanel;
        RectTransform quickPanel;
        readonly List<string> emojiLines = new List<string>();
        float lastEmoji = -10f;
        Coroutine emojiRoutine;

        RectTransform playerMenu;
        Text playerMenuTitle;
        RectTransform playerMenuButtons;
        RectTransform friendPicker;
        RectTransform friendPickerList;
        Text friendPickerMessage;

        void BuildOnlineScreens()
        {
            onlineScreen = Register(BuildOnline());
            joinScreen = Register(BuildJoin());
            lobbyScreen = Register(BuildLobby());
        }

        RectTransform BuildOnline()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Online");
            Text title = UiKit.CreateText(screen, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1200f, 100f), 72, TextAnchor.UpperCenter);
            title.text = "Online";
            UiKit.AddOutline(title, 3f);
            UiKit.CreateButton(screen, "Geri", new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(200f, 90f), ShowHome);
            onlineStatus = UiKit.CreateCenterText(screen, "Status", new Vector2(0f, 290f), new Vector2(1600f, 70f), 34);

            onlineActions = UiKit.CreateGroup(screen, "Actions");
            Text host = UiKit.CreateCenterButton(onlineActions, "Oda Kur", new Vector2(0f, 140f), new Vector2(580f, 140f), () =>
            {
                if (onlineBusy)
                {
                    return;
                }
                roomPicking = true;
                ShowModes();
            });
            host.fontSize = 54;
            Text join = UiKit.CreateCenterButton(onlineActions, "Koda Katıl", new Vector2(0f, -30f), new Vector2(580f, 140f), ShowJoin);
            join.fontSize = 54;
            directText = UiKit.CreateCenterButton(onlineActions, "", new Vector2(0f, -190f), new Vector2(700f, 100f), () =>
            {
                onlineDirect = !onlineDirect;
                RefreshOnlineStatus();
            });
            Text info = UiKit.CreateCenterText(onlineActions, "Info", new Vector2(0f, -320f), new Vector2(1700f, 110f), 28);
            bool lan = OnlineConfig.Instance.allowLanMode;
            directText.transform.parent.gameObject.SetActive(lan);
            info.text = lan ? "İnternet: 6 haneli oda koduyla her yerden oynanır.\nYerel Ağ: aynı Wi-Fi'deki telefonlar IP adresiyle bağlanır (hesap gerekmez)." : "Odayı kur, 6 haneli kodu arkadaşlarına ver; kodu giren her yerden katılır.";
            return screen;
        }

        void ShowOnline()
        {
            roomPicking = false;
            if (OnlineRoom.Active)
            {
                ShowLobby();
                return;
            }
            if (NeedsReminder(ShowOnline))
            {
                return;
            }
            Show(onlineScreen);
            RefreshOnlineStatus();
            if (PlayerProfile.OnlineAllowed && !onlineDirect && !OnlineServices.Ready)
            {
                StartServices();
            }
        }

        void RefreshOnlineStatus()
        {
            if (onlineScreen == null || currentScreen != onlineScreen)
            {
                return;
            }
            bool allowed = PlayerProfile.OnlineAllowed;
            if (!OnlineConfig.Instance.allowLanMode)
            {
                onlineDirect = false;
            }
            onlineActions.gameObject.SetActive(allowed);
            directText.text = onlineDirect ? "Bağlantı: Yerel Ağ (Wi-Fi)" : "Bağlantı: İnternet";
            if (!allowed)
            {
                onlineStatus.text = "Online oyun kapalı. Bir yetişkin Ayarlar > Ebeveyn Kontrolü'nden açabilir.";
            }
            else if (onlineBusy)
            {
                onlineStatus.text = "Bağlanıyor…";
            }
            else if (onlineDirect)
            {
                onlineStatus.text = "Yerel Ağ modu: odayı kuran telefonun IP adresini girerek katılın.";
            }
            else if (OnlineServices.Ready)
            {
                onlineStatus.text = "Hazır! Bir oda kur ya da arkadaşının kodunu gir.";
            }
            else
            {
                onlineStatus.text = string.IsNullOrEmpty(OnlineServices.Message) ? "Bağlanıyor…" : OnlineServices.Message;
            }
        }

        RectTransform BuildJoin()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Join");
            joinTitle = UiKit.CreateText(screen, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1200f, 100f), 64, TextAnchor.UpperCenter);
            UiKit.AddOutline(joinTitle, 3f);
            UiKit.CreateButton(screen, "Geri", new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(200f, 90f), ShowOnline);
            joinDisplay = UiKit.CreateCenterText(screen, "Display", new Vector2(-420f, 140f), new Vector2(760f, 140f), 96);
            joinMessage = UiKit.CreateCenterText(screen, "Message", new Vector2(-420f, 10f), new Vector2(760f, 100f), 30);
            joinMessage.color = new Color(1f, 0.7f, 0.5f);
            Vector2 origin = new Vector2(320f, 120f);
            BuildKeypad(screen, origin, digit =>
            {
                if (joinDigits.Length < (onlineDirect ? 15 : 6))
                {
                    joinDigits += digit;
                    RefreshJoin();
                }
            }, () =>
            {
                if (joinDigits.Length > 0)
                {
                    joinDigits = joinDigits.Substring(0, joinDigits.Length - 1);
                    RefreshJoin();
                }
            });
            Text dot = UiKit.CreateCenterButton(screen, ".", new Vector2(origin.x - 148f, origin.y + 120f - 3 * 148f), new Vector2(130f, 130f), () =>
            {
                if (joinDigits.Length < 15)
                {
                    joinDigits += ".";
                    RefreshJoin();
                }
            });
            dot.fontSize = 56;
            joinDot = (RectTransform)dot.transform.parent;
            UiKit.CreateCenterButton(screen, "Katıl", new Vector2(-420f, -140f), new Vector2(380f, 120f), () =>
            {
                if (!onlineDirect && joinDigits.Length != 6)
                {
                    joinMessage.text = "Oda kodu 6 haneli olmalı.";
                    return;
                }
                if (onlineDirect && joinDigits.Split('.').Length != 4)
                {
                    joinMessage.text = "IP adresini tam yaz (ör. 192.168.1.20).";
                    return;
                }
                JoinRoom(onlineDirect, joinDigits);
            });
            return screen;
        }

        void ShowJoin()
        {
            if (onlineBusy)
            {
                return;
            }
            joinDigits = "";
            joinTitle.text = onlineDirect ? "Oda kuranın IP adresi" : "Oda kodunu gir";
            joinDot.gameObject.SetActive(onlineDirect);
            RefreshJoin();
            Show(joinScreen);
        }

        void RefreshJoin()
        {
            joinMessage.text = "";
            if (onlineDirect)
            {
                joinDisplay.text = joinDigits.Length > 0 ? joinDigits : "___.___.___.___";
                joinDisplay.fontSize = 72;
                return;
            }
            string padded = joinDigits.PadRight(6, '_');
            joinDisplay.text = padded.Substring(0, 3) + " " + padded.Substring(3);
            joinDisplay.fontSize = 96;
        }

        void PickRoom(bool adventure, int index)
        {
            roomPicking = false;
            int level = adventure ? 0 : index;
            int difficulty = adventure ? index : 0;
            if (OnlineRoom.Active)
            {
                OnlineRoom room = OnlineRoom.Current;
                if (room.IsHost)
                {
                    room.SetSettings(adventure, level, difficulty, room.State.bots, room.State.botLevel);
                }
                ShowLobby();
                return;
            }
            HostRoom(new RoomState { adventure = adventure, level = level, difficulty = difficulty, bots = 0, botLevel = GameSelection.BotLevelIndex, biome = GameSelection.BiomeChoice, raft = GameSelection.RaftIndex, weather = GameSelection.WeatherChoice });
        }

        DeviceInfo SelfDevice()
        {
            return new DeviceInfo
            {
                playerId = OnlineServices.PlayerId,
                name = PlayerProfile.DisplayName,
                locals = onlineLocals,
                avatars = LocalAvatars(),
                child = PlayerProfile.IsChild
            };
        }

        static int[] LocalAvatars()
        {
            var avatars = new int[MaxLocals];
            for (int i = 0; i < avatars.Length; i++)
            {
                avatars[i] = GameSelection.GetAvatar(i) + GameSelection.GetCostume(i) * AvatarDefinition.CostumeCode;
            }
            return avatars;
        }

        async void HostRoom(RoomState settings)
        {
            if (onlineBusy)
            {
                return;
            }
            onlineBusy = true;
            Show(onlineScreen);
            RefreshOnlineStatus();
            OnlineRoom room = OnlineRoom.Ensure();
            bool ok = await room.Host(onlineDirect, SelfDevice(), settings);
            onlineBusy = false;
            if (this == null)
            {
                return;
            }
            if (!ok)
            {
                string error = room.LastError;
                Destroy(room.gameObject);
                ShowOnline();
                onlineStatus.text = error;
                return;
            }
            ShowLobby();
        }

        async void JoinRoom(bool direct, string code)
        {
            if (onlineBusy || OnlineRoom.Active)
            {
                return;
            }
            if (!PlayerProfile.OnlineAllowed)
            {
                Toast("Online oyun ebeveyn kontrolünden kapatılmış.");
                return;
            }
            onlineBusy = true;
            Show(onlineScreen);
            RefreshOnlineStatus();
            OnlineRoom room = OnlineRoom.Ensure();
            bool ok = await room.Join(direct, code, SelfDevice());
            onlineBusy = false;
            if (this == null)
            {
                return;
            }
            if (!ok)
            {
                string error = room.LastError;
                Destroy(room.gameObject);
                ShowOnline();
                onlineStatus.text = error;
                return;
            }
            ShowLobby();
        }

        void BindRoom(OnlineRoom room)
        {
            if (boundRoom == room)
            {
                return;
            }
            if (boundRoom != null)
            {
                boundRoom.RoomChanged -= RefreshLobby;
                boundRoom.Closed -= OnRoomClosed;
                boundRoom.EmojiReceived -= OnEmoji;
            }
            boundRoom = room;
            if (room != null)
            {
                room.RoomChanged += RefreshLobby;
                room.Closed += OnRoomClosed;
                room.EmojiReceived += OnEmoji;
            }
        }

        void OnRoomClosed(string reason)
        {
            BindRoom(null);
            OnlineRoom.PendingNotice = "";
            if (this == null)
            {
                return;
            }
            Show(onlineScreen);
            RefreshOnlineStatus();
            Toast(reason);
        }

        RectTransform BuildLobby()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Lobby");
            lobbyCode = UiKit.CreateText(screen, "Code", new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1300f, 100f), 66, TextAnchor.UpperCenter);
            lobbyCode.color = UiKit.StarOnColor;
            UiKit.AddOutline(lobbyCode, 3f);
            UiKit.CreateButton(screen, "Çık", new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(200f, 90f), () =>
            {
                OnlineRoom room = OnlineRoom.Current;
                Confirm(room != null && room.IsHost ? "Oda kapatılsın mı? Herkes odadan çıkar." : "Odadan çıkmak istiyor musun?", LeaveRoom);
            });

            Text players = UiKit.CreateCenterText(screen, "PlayersHeader", new Vector2(-450f, 330f), new Vector2(800f, 60f), 44);
            players.text = "Oyuncular";
            int maxSeats = OnlineConfig.Instance.maxSeats;
            seatTexts = new Text[maxSeats];
            seatImages = new Image[maxSeats];
            for (int i = 0; i < maxSeats; i++)
            {
                int index = i;
                Text seat = UiKit.CreateCenterButton(screen, "", new Vector2(-450f, 255f - i * 82f), new Vector2(800f, 72f), () => OpenSeatMenu(index));
                seat.fontSize = 32;
                seatTexts[i] = seat;
                seatImages[i] = seat.transform.parent.GetComponent<Image>();
            }
            spectatorHeader = UiKit.CreateCenterText(screen, "SpectatorHeader", new Vector2(-450f, -80f), new Vector2(800f, 50f), 34);
            spectatorTexts = new Text[SpectatorSlots];
            for (int i = 0; i < SpectatorSlots; i++)
            {
                int index = i;
                Text spectator = UiKit.CreateCenterButton(screen, "", new Vector2(-655f + i % 2 * 410f, -138f - i / 2 * 64f), new Vector2(390f, 56f), () => OpenSpectatorMenu(index));
                spectator.fontSize = 26;
                spectatorTexts[i] = spectator;
            }

            Text settings = UiKit.CreateCenterText(screen, "SettingsHeader", new Vector2(450f, 330f), new Vector2(800f, 60f), 44);
            settings.text = "Oda Ayarları";
            mapText = RoomSetting(screen, 0, () =>
            {
                roomPicking = true;
                ShowModes();
            });
            roomBiomeText = RoomSetting(screen, 1, CycleRoomBiome);
            roomBotsText = RoomSetting(screen, 2, CycleRoomBots);
            roomBotLevelText = RoomSetting(screen, 3, CycleRoomBotLevel);
            roomRaftText = RoomSetting(screen, 4, CycleRoomRaft);
            roomWeatherText = RoomSetting(screen, 5, CycleRoomWeather);
            roomSettingButtons = new[] { Owner(mapText), Owner(roomBiomeText), Owner(roomBotsText), Owner(roomBotLevelText), Owner(roomRaftText), Owner(roomWeatherText) };
            Text invite = UiKit.CreateCenterButton(screen, "Arkadaşını Davet Et", new Vector2(450f, -172f), new Vector2(560f, 76f), OpenFriendPicker);
            invite.fontSize = 34;
            inviteButton = (RectTransform)invite.transform.parent;
            inviteButton.GetComponent<Image>().color = UiKit.AccentColor;
            clientNote = UiKit.CreateCenterText(screen, "ClientNote", new Vector2(450f, -172f), new Vector2(800f, 60f), 30);
            clientNote.text = "Oda ayarlarını oda kurucusu seçer.";

            lobbyStatus = UiKit.CreateCenterText(screen, "Status", new Vector2(0f, -240f), new Vector2(1700f, 50f), 30);
            localsText = UiKit.CreateCenterButton(screen, "", new Vector2(-660f, -320f), new Vector2(420f, 86f), CycleLocals);
            localAvatarTexts = new Text[MaxLocals];
            for (int i = 0; i < MaxLocals; i++)
            {
                int local = i;
                localAvatarTexts[i] = UiKit.CreateCenterButton(screen, "", new Vector2(-280f + i * 330f, -320f), new Vector2(310f, 86f), () => CycleLocalAvatar(local));
                localAvatarTexts[i].fontSize = 24;
            }
            Text quick = UiKit.CreateCenterButton(screen, "Hızlı Mesaj  ▲", new Vector2(-660f, -440f), new Vector2(420f, 90f), () => quickPanel.gameObject.SetActive(!quickPanel.gameObject.activeSelf));
            quick.fontSize = 36;
            readyText = UiKit.CreateCenterButton(screen, "", new Vector2(660f, -430f), new Vector2(440f, 110f), PressReady);
            readyText.fontSize = 46;
            readyImage = readyText.transform.parent.GetComponent<Image>();

            emojiPanel = UiKit.CreateImage(screen, "EmojiFeed", UiKit.Rounded, new Color(0f, 0f, 0f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0f, -126f), new Vector2(640f, 50f));
            emojiPanel.pivot = new Vector2(0.5f, 1f);
            emojiPanel.GetComponent<Image>().type = Image.Type.Sliced;
            emojiFeed = UiKit.CreateCenterText(emojiPanel, "Text", Vector2.zero, new Vector2(620f, 46f), 30);
            emojiFeed.color = UiKit.StarOnColor;
            emojiPanel.gameObject.SetActive(false);

            quickPanel = UiKit.CreatePanel(screen, "QuickMessages", new Vector2(640f, 560f));
            quickPanel.anchoredPosition = new Vector2(-660f, -110f);
            Text messagesHeader = UiKit.CreateCenterText(quickPanel, "MessagesHeader", new Vector2(0f, 240f), new Vector2(600f, 44f), 30);
            messagesHeader.text = "Mesajlar";
            for (int i = 0; i < RoomMessages.Emojis.Length; i++)
            {
                int index = i;
                Text emoji = UiKit.CreateCenterButton(quickPanel, RoomMessages.Emojis[i], new Vector2(-200f + i % 3 * 200f, 170f - i / 3 * 95f), new Vector2(185f, 82f), () =>
                {
                    SendEmoji(index);
                    quickPanel.gameObject.SetActive(false);
                });
                emoji.fontSize = 30;
            }
            Text movesHeader = UiKit.CreateCenterText(quickPanel, "MovesHeader", new Vector2(0f, -15f), new Vector2(600f, 44f), 30);
            movesHeader.text = "Avatar Hareketleri";
            for (int i = 0; i < RoomMessages.Emotes.Length; i++)
            {
                int index = RoomMessages.Emojis.Length + i;
                Text move = UiKit.CreateCenterButton(quickPanel, RoomMessages.Emotes[i], new Vector2(-200f + i % 3 * 200f, -85f - i / 3 * 95f), new Vector2(185f, 82f), () =>
                {
                    SendEmoji(index);
                    quickPanel.gameObject.SetActive(false);
                });
                move.fontSize = 28;
                ((RectTransform)move.transform.parent).GetComponent<Image>().color = new Color(0.62f, 0.85f, 1f);
            }
            quickPanel.gameObject.SetActive(false);

            playerMenu = UiKit.CreateGroup(screen, "PlayerMenu");
            UiKit.CreateImage(playerMenu, "Shade", null, new Color(0f, 0f, 0f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4000f, 3000f)).GetComponent<Image>().raycastTarget = true;
            RectTransform menuPanel = UiKit.CreatePanel(playerMenu, "Panel", new Vector2(900f, 640f));
            playerMenuTitle = UiKit.CreateText(menuPanel, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(860f, 70f), 48, TextAnchor.UpperCenter);
            playerMenuButtons = UiKit.CreateGroup(menuPanel, "Buttons");
            playerMenu.gameObject.SetActive(false);

            friendPicker = UiKit.CreateGroup(screen, "FriendPicker");
            UiKit.CreateImage(friendPicker, "Shade", null, new Color(0f, 0f, 0f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4000f, 3000f)).GetComponent<Image>().raycastTarget = true;
            RectTransform pickerPanel = UiKit.CreatePanel(friendPicker, "Panel", new Vector2(1640f, 860f));
            Text pickerTitle = UiKit.CreateText(pickerPanel, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -25f), new Vector2(1500f, 70f), 48, TextAnchor.UpperCenter);
            pickerTitle.text = "Arkadaşını odaya çağır";
            friendPickerMessage = UiKit.CreateCenterText(pickerPanel, "Message", new Vector2(0f, 250f), new Vector2(1500f, 60f), 32);
            friendPickerList = UiKit.CreateGroup(pickerPanel, "List");
            UiKit.CreateButton(pickerPanel, "Kapat", new Vector2(0.5f, 0f), new Vector2(0f, 25f), new Vector2(300f, 90f), () => friendPicker.gameObject.SetActive(false));
            friendPicker.gameObject.SetActive(false);
            return screen;
        }

        Text RoomSetting(RectTransform screen, int row, System.Action onClick)
        {
            Text label = UiKit.CreateCenterButton(screen, "", new Vector2(450f, 262f - row * 72f), new Vector2(800f, 64f), onClick);
            label.fontSize = 32;
            return label;
        }

        static Button Owner(Text label)
        {
            return label.transform.parent.GetComponent<Button>();
        }

        void ShowLobby()
        {
            if (!OnlineRoom.Active)
            {
                ShowOnline();
                return;
            }
            BindRoom(OnlineRoom.Current);
            playerMenu.gameObject.SetActive(false);
            friendPicker.gameObject.SetActive(false);
            quickPanel.gameObject.SetActive(false);
            Show(lobbyScreen);
            RefreshLobby();
        }

        void RefreshLobby()
        {
            if (lobbyScreen == null || currentScreen != lobbyScreen || !OnlineRoom.Active)
            {
                return;
            }
            OnlineRoom room = OnlineRoom.Current;
            RoomState state = room.State;
            bool host = room.IsHost;
            bool synced = state.devices.Count > 0;
            if (string.IsNullOrEmpty(state.code))
            {
                lobbyCode.text = "Odaya bağlanıyor…";
            }
            else if (state.direct)
            {
                lobbyCode.text = "Yerel Ağ · IP: " + state.code;
            }
            else
            {
                lobbyCode.text = "Oda Kodu: " + state.code.Substring(0, Mathf.Min(3, state.code.Length)) + " " + (state.code.Length > 3 ? state.code.Substring(3) : "");
            }

            for (int i = 0; i < seatTexts.Length; i++)
            {
                SeatInfo seat = i < state.seats.Count ? state.seats[i] : null;
                seatImages[i].color = seat == null ? new Color(1f, 1f, 1f, 0.25f) : seat.bot ? new Color(0.7f, 0.85f, 1f) : UiKit.ButtonColor;
                if (seat == null)
                {
                    seatTexts[i].text = "Boş koltuk";
                    continue;
                }
                AvatarDefinition avatar = catalog.GetAvatar(seat.avatar % AvatarDefinition.CostumeCode);
                string avatarName = avatar != null ? avatar.displayName : "";
                if (seat.bot)
                {
                    seatTexts[i].text = (i + 1) + ".  BOT · " + avatarName;
                    continue;
                }
                DeviceInfo device = state.Device(seat.owner);
                bool ready = device != null && (device.ready || state.devices.IndexOf(device) == 0);
                string tag = device != null && state.devices.IndexOf(device) == 0 ? "Kurucu" : ready ? "Hazır" : "Bekliyor";
                seatTexts[i].text = (i + 1) + ".  " + seat.name + " · " + avatarName + " · " + tag;
            }

            spectatorHeader.text = state.spectators.Count > 0 ? "İzleyiciler (yer kalmadı)" : "";
            for (int i = 0; i < spectatorTexts.Length; i++)
            {
                bool shown = i < state.spectators.Count;
                spectatorTexts[i].transform.parent.gameObject.SetActive(shown);
                if (shown)
                {
                    spectatorTexts[i].text = state.spectators[i].name;
                }
            }

            int botCount = 0;
            foreach (SeatInfo seat in state.seats)
            {
                if (seat.bot)
                {
                    botCount++;
                }
            }
            RaftVariant raftVariant = catalog.GetRaft(state.raft);
            if (stage != null)
            {
                stage.SetRaft(raftVariant);
                stage.SetBiome(state.biome);
                if (state.seats.Count > 0)
                {
                    var codes = new int[Mathf.Min(state.seats.Count, GameSelection.MaxPlayers)];
                    for (int i = 0; i < codes.Length; i++)
                    {
                        codes[i] = state.seats[i].avatar;
                    }
                    stage.SetLineup(codes);
                }
            }
            string arrow = host ? "  ▶" : "";
            mapText.text = "Parkur: " + MapLabel(state) + arrow;
            roomBiomeText.text = "Harita: " + Biomes.ChoiceName(state.biome) + arrow;
            roomBotsText.text = "Botlar: " + (botCount == 0 ? "Yok" : botCount.ToString()) + arrow;
            roomBotLevelText.text = "Bot Seviyesi: " + BotLevelName(state.botLevel) + arrow;
            roomRaftText.text = "Sal: " + (raftVariant != null ? raftVariant.name + "  <size=24>Day " + raftVariant.DurabilityMarks + "  Hız " + raftVariant.AccelerationMarks + "</size>" : "Klasik Sal") + arrow;
            roomWeatherText.text = "Hava: " + Weathers.Name(state.weather) + arrow;
            foreach (Button button in roomSettingButtons)
            {
                button.interactable = host;
                button.GetComponent<Image>().color = host ? UiKit.ButtonColor : new Color(1f, 0.97f, 0.88f, 0.9f);
            }
            clientNote.gameObject.SetActive(!host);
            inviteButton.gameObject.SetActive(host && !state.direct && PlayerProfile.FriendsAllowed && PlayerProfile.InvitesAllowed);

            localsText.text = "Bu telefonda: " + onlineLocals + " oyuncu";
            for (int i = 0; i < localAvatarTexts.Length; i++)
            {
                bool shown = i < onlineLocals;
                localAvatarTexts[i].transform.parent.gameObject.SetActive(shown);
                if (shown)
                {
                    AvatarDefinition avatar = catalog.GetAvatar(GameSelection.GetAvatar(i));
                    int costume = GameSelection.GetCostume(i);
                    localAvatarTexts[i].text = (onlineLocals == 1 ? "Avatarın: " : (i + 1) + ". ") + (avatar != null ? avatar.displayName + (costume > 0 ? " (" + avatar.CostumeName(costume) + ")" : "") : "-") + "  ▶";
                }
            }

            DeviceInfo self = state.Device(room.LocalClientId);
            int waiting = 0;
            for (int i = 1; i < state.devices.Count; i++)
            {
                if (!state.devices[i].ready)
                {
                    waiting++;
                }
            }
            if (host)
            {
                bool canStart = room.AllReady;
                readyText.text = "Başlat";
                readyImage.color = canStart ? UiKit.AccentColor : new Color(1f, 1f, 1f, 0.35f);
                lobbyStatus.text = state.devices.Count <= 1 ? "Arkadaşlarına kodu ver. İstersen tek başına botlarla da başlayabilirsin." : waiting > 0 ? waiting + " telefon hazır olmayı bekliyor." : "Herkes hazır!";
            }
            else
            {
                bool ready = self != null && self.ready;
                readyText.text = ready ? "Hazır değilim" : "Hazırım";
                readyImage.color = ready ? UiKit.AccentColor : UiKit.ButtonColor;
                lobbyStatus.text = !synced ? "Oda bilgisi bekleniyor…" : ready ? "Oda kurucusunun başlatması bekleniyor." : "Hazır olunca \"Hazırım\"a bas.";
            }
        }

        string MapLabel(RoomState state)
        {
            if (state.adventure)
            {
                bool known = catalog.difficulties != null && state.difficulty < catalog.difficulties.Length;
                return "Macera · " + (known ? catalog.difficulties[state.difficulty].displayName : "?");
            }
            LevelDefinition level = catalog.GetLevel(state.level);
            return "Alıştırma · " + (state.level + 1) + ". " + (level != null ? level.displayName : "?");
        }

        string BotLevelName(int index)
        {
            return catalog.botLevels != null && index >= 0 && index < catalog.botLevels.Length ? catalog.botLevels[index].displayName : "-";
        }

        void CycleRoomBots()
        {
            OnlineRoom room = OnlineRoom.Current;
            if (room == null || !room.IsHost)
            {
                return;
            }
            RoomState state = room.State;
            int free = OnlineConfig.Instance.maxSeats - state.HumanSeats;
            int bots = state.bots + 1 > free ? 0 : state.bots + 1;
            room.SetSettings(state.adventure, state.level, state.difficulty, bots, state.botLevel);
        }

        void CycleRoomRaft()
        {
            OnlineRoom room = OnlineRoom.Current;
            if (room == null || !room.IsHost)
            {
                return;
            }
            room.SetRaft(catalog.NextRaft(room.State.raft));
        }

        void CycleRoomWeather()
        {
            OnlineRoom room = OnlineRoom.Current;
            if (room == null || !room.IsHost)
            {
                return;
            }
            room.SetWeather((room.State.weather + 1) % Weathers.Count);
        }

        void CycleRoomBiome()
        {
            OnlineRoom room = OnlineRoom.Current;
            if (room == null || !room.IsHost)
            {
                return;
            }
            room.SetBiome((room.State.biome + 1) % (Biomes.RandomChoice + 1));
        }

        void CycleRoomBotLevel()
        {
            OnlineRoom room = OnlineRoom.Current;
            int count = catalog.botLevels != null ? catalog.botLevels.Length : 0;
            if (room == null || !room.IsHost || count == 0)
            {
                return;
            }
            RoomState state = room.State;
            room.SetSettings(state.adventure, state.level, state.difficulty, state.bots, (state.botLevel + 1) % count);
        }

        void CycleLocals()
        {
            onlineLocals = onlineLocals % MaxLocals + 1;
            PushLocal(null);
        }

        void CycleLocalAvatar(int local)
        {
            int count = catalog.avatars != null ? catalog.avatars.Length : 0;
            if (count == 0)
            {
                return;
            }
            AvatarDefinition current = catalog.GetAvatar(GameSelection.GetAvatar(local));
            int costume = GameSelection.GetCostume(local) + 1;
            if (current != null && costume < current.CostumeCount)
            {
                GameSelection.SetCostume(local, costume);
            }
            else
            {
                GameSelection.SetAvatar(local, (GameSelection.GetAvatar(local) + 1) % count);
                GameSelection.SetCostume(local, 0);
            }
            PushLocal(null);
        }

        void PushLocal(bool? ready)
        {
            OnlineRoom room = OnlineRoom.Current;
            if (room == null)
            {
                return;
            }
            DeviceInfo self = room.State.Device(room.LocalClientId);
            bool value = ready ?? (self != null && self.ready);
            room.SetLocal(onlineLocals, LocalAvatars(), value);
            RefreshLobby();
        }

        void PressReady()
        {
            OnlineRoom room = OnlineRoom.Current;
            if (room == null)
            {
                return;
            }
            if (room.IsHost)
            {
                if (!room.AllReady)
                {
                    Toast("Herkes hazır olunca başlatabilirsin.");
                    return;
                }
                room.StartGame();
                return;
            }
            DeviceInfo self = room.State.Device(room.LocalClientId);
            PushLocal(!(self != null && self.ready));
        }

        void SendEmoji(int index)
        {
            OnlineRoom room = OnlineRoom.Current;
            if (room == null || Time.unscaledTime - lastEmoji < 1f)
            {
                return;
            }
            lastEmoji = Time.unscaledTime;
            room.SendEmoji(index);
        }

        void OnEmoji(EmojiInfo emoji)
        {
            if (this == null || emoji.emoji < 0 || emoji.emoji >= RoomMessages.Count)
            {
                return;
            }
            string name = emoji.name;
            OnlineRoom room = OnlineRoom.Current;
            if (room != null)
            {
                DeviceInfo device = room.State.Device(emoji.clientId);
                if (device != null)
                {
                    if (PlayerProfile.IsBlocked(device.playerId))
                    {
                        return;
                    }
                    name = device.name;
                }
            }
            emojiLines.Insert(0, name + ": " + RoomMessages.Describe(emoji.emoji));
            int emote = RoomMessages.EmoteOf(emoji.emoji);
            if (emote >= 0 && stage != null && room != null)
            {
                for (int i = 0; i < room.State.seats.Count; i++)
                {
                    SeatInfo seat = room.State.seats[i];
                    if (!seat.bot && seat.owner == emoji.clientId)
                    {
                        stage.PlayEmote(i, emote);
                    }
                }
            }
            if (emojiLines.Count > 1)
            {
                emojiLines.RemoveAt(emojiLines.Count - 1);
            }
            emojiFeed.text = string.Join("\n", emojiLines);
            emojiPanel.gameObject.SetActive(true);
            if (emojiRoutine != null)
            {
                StopCoroutine(emojiRoutine);
            }
            emojiRoutine = StartCoroutine(ClearEmojis());
        }

        IEnumerator ClearEmojis()
        {
            yield return new WaitForSecondsRealtime(5f);
            emojiLines.Clear();
            emojiFeed.text = "";
            emojiPanel.gameObject.SetActive(false);
            emojiRoutine = null;
        }

        void LeaveRoom()
        {
            OnlineRoom room = OnlineRoom.Current;
            BindRoom(null);
            if (room != null)
            {
                room.Leave();
            }
            Show(onlineScreen);
            RefreshOnlineStatus();
        }

        void OpenSeatMenu(int index)
        {
            OnlineRoom room = OnlineRoom.Current;
            if (room == null || index >= room.State.seats.Count)
            {
                return;
            }
            SeatInfo seat = room.State.seats[index];
            if (seat.bot)
            {
                return;
            }
            OpenPlayerMenu(seat.owner, seat.name);
        }

        void OpenSpectatorMenu(int index)
        {
            OnlineRoom room = OnlineRoom.Current;
            if (room == null || index >= room.State.spectators.Count)
            {
                return;
            }
            SpectatorInfo spectator = room.State.spectators[index];
            OpenPlayerMenu(spectator.owner, spectator.name);
        }

        void OpenPlayerMenu(ulong owner, string name)
        {
            OnlineRoom room = OnlineRoom.Current;
            DeviceInfo device = room.State.Device(owner);
            if (device == null || owner == room.LocalClientId)
            {
                return;
            }
            ClearChildren(playerMenuButtons);
            playerMenuTitle.text = name;
            var buttons = new List<(string label, System.Action action)>();
            string playerId = device.playerId;
            bool known = !string.IsNullOrEmpty(playerId);
            bool friend = false;
            foreach (string id in FriendsClient.FriendIds())
            {
                friend |= id == playerId;
            }
            if (known && !friend && PlayerProfile.FriendsAllowed && !device.child && !room.State.direct)
            {
                buttons.Add(("Arkadaş Ekle", async () =>
                {
                    string result = await FriendsClient.AddById(playerId);
                    if (this != null)
                    {
                        Toast(result);
                    }
                }));
            }
            if (known)
            {
                buttons.Add(("Engelle", () =>
                {
                    if (FriendsClient.Initialized)
                    {
                        RunFriends(FriendsClient.Block(playerId));
                    }
                    else
                    {
                        PlayerProfile.Block(playerId);
                    }
                    Toast(device.name + " engellendi.");
                }));
                buttons.Add(("Bildir: Uygunsuz ad", () => ReportPlayer(playerId, device.name, "ad")));
                buttons.Add(("Bildir: Rahatsız edici", () => ReportPlayer(playerId, device.name, "davranis")));
            }
            if (room.IsHost)
            {
                buttons.Add(("Odadan Çıkar", () => room.Kick(owner)));
            }
            for (int i = 0; i < buttons.Count; i++)
            {
                System.Action action = buttons[i].action;
                UiKit.CreateCenterButton(playerMenuButtons, buttons[i].label, new Vector2(0f, 180f - i * 92f), new Vector2(620f, 80f), () =>
                {
                    playerMenu.gameObject.SetActive(false);
                    action();
                });
            }
            UiKit.CreateCenterButton(playerMenuButtons, "Kapat", new Vector2(0f, -260f), new Vector2(300f, 80f), () => playerMenu.gameObject.SetActive(false));
            playerMenu.SetAsLastSibling();
            playerMenu.gameObject.SetActive(true);
        }

        void ReportPlayer(string playerId, string name, string reason)
        {
            PlayerProfile.Report(playerId, name, reason);
            Toast("Bildirimin alındı. Teşekkürler!");
        }

        async void OpenFriendPicker()
        {
            ClearChildren(friendPickerList);
            friendPicker.SetAsLastSibling();
            friendPicker.gameObject.SetActive(true);
            friendPickerMessage.text = "Yükleniyor…";
            bool ready = await FriendsClient.EnsureReady();
            if (this == null || !friendPicker.gameObject.activeSelf)
            {
                return;
            }
            if (!ready)
            {
                friendPickerMessage.text = FriendsClient.Message;
                return;
            }
            List<FriendsClient.Person> friends = FriendsClient.Friends();
            friendPickerMessage.text = friends.Count == 0 ? "Henüz arkadaşın yok. Arkadaşlar ekranından ekleyebilirsin." : "";
            for (int i = 0; i < friends.Count && i < 5; i++)
            {
                FriendsClient.Person person = friends[i];
                string id = person.id;
                string name = person.name;
                AddRow(friendPickerList, i, 170f, 100f, name, FriendStatus(person), false, ("Davet Et", () => InviteFriend(id, name)));
            }
        }
    }
}
