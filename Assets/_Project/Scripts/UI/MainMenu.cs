using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FartRafters
{
    public partial class MainMenu : MonoBehaviour
    {
        public GameCatalog catalog;
        public MenuStage stage;
        [Range(0f, 1f)] public float musicLevel = 0.55f;

        static readonly Color LockedColor = new Color(1f, 1f, 1f, 0.3f);
        static readonly Color CardColor = new Color(0f, 0f, 0f, 0.55f);

        RectTransform canvasRect;
        readonly List<RectTransform> screens = new List<RectTransform>();
        RectTransform currentScreen;
        Text totalText;
        RectTransform homeScreen;
        RectTransform levelScreen;
        RectTransform modeScreen;
        RectTransform difficultyScreen;
        Text[] difficultyBest;
        RectTransform setupScreen;
        LevelCard[] cards;
        Text setupTitle;
        Text botCountText;
        Text botLevelText;
        Text playersText;
        Text biomeText;
        Text raftText;
        Text raftInfo;
        AvatarSlot[] slots;
        RectTransform[] slotAnchors;
        Text soundText;
        Text musicText;

        class LevelCard
        {
            public Image background;
            public Text title;
            public Text best;
            public Button button;
        }

        class AvatarSlot
        {
            public RectTransform root;
            public Text heading;
            public Image swatch;
            public Text name;
            public Text costume;
            public Image[] gasPips;
            public Image[] powerPips;
            public Image[] refillPips;
            public RectTransform previous;
            public RectTransform next;
        }

        void Start()
        {
            GameAudio.SetMusicLevel(musicLevel);
            UiKit.EnsureEventSystem();
            canvasRect = UiKit.CreateCanvas(transform, out _);
            homeScreen = Register(BuildHome());
            modeScreen = Register(BuildModes());
            levelScreen = Register(BuildLevels());
            difficultyScreen = Register(BuildDifficulties());
            setupScreen = Register(BuildSetup());
            questsScreen = Register(BuildQuests());
            BuildProfileScreens();
            BuildSocialScreens();
            BuildOnlineScreens();
            BuildNotifications();
            Route();
        }

        RectTransform Register(RectTransform screen)
        {
            screens.Add(screen);
            return screen;
        }

        void Route()
        {
            if (!PlayerProfile.HasAge)
            {
                ShowAge(true);
            }
            else if (!PlayerProfile.HasName)
            {
                ShowName(true);
            }
            else if (OnlineRoom.Active)
            {
                ShowLobby();
            }
            else
            {
                ShowHome();
            }
            if (!string.IsNullOrEmpty(OnlineRoom.PendingNotice))
            {
                Toast(OnlineRoom.PendingNotice);
                OnlineRoom.PendingNotice = "";
            }
            if (PlayerProfile.HasName)
            {
                StartServices();
            }
        }

        void ShowHome()
        {
            totalText.text = "Toplam puan: " + ProgressStore.TotalScore;
            RefreshQuestBadge();
            Show(homeScreen);
        }

        void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Back();
            }
        }

        void Back()
        {
            if (currentScreen == setupScreen)
            {
                BackFromSetup();
            }
            else if (currentScreen == levelScreen || currentScreen == difficultyScreen)
            {
                ShowModes();
            }
            else if (currentScreen == modeScreen)
            {
                LeaveModes();
            }
            else if (currentScreen == joinScreen)
            {
                ShowOnline();
            }
            else if (currentScreen == gateScreen || currentScreen == parentalScreen || currentScreen == privacyScreen)
            {
                ShowSettings();
            }
            else if (currentScreen == lobbyScreen || currentScreen == reminderScreen || currentScreen == confirmScreen || currentScreen == ageScreen || (currentScreen == nameScreen && !PlayerProfile.HasName))
            {
                return;
            }
            else if (currentScreen == questsScreen)
            {
                ShowHome();
            }
            else if (currentScreen != homeScreen)
            {
                ShowHome();
            }
        }

        void LeaveModes()
        {
            if (roomPicking)
            {
                roomPicking = false;
                if (OnlineRoom.Active)
                {
                    ShowLobby();
                }
                else
                {
                    ShowOnline();
                }
                return;
            }
            ShowHome();
        }

        void Show(RectTransform screen)
        {
            currentScreen = screen;
            foreach (RectTransform item in screens)
            {
                item.gameObject.SetActive(item == screen);
            }
            if (stage != null)
            {
                stage.ShowPreviews(screen == setupScreen, slotAnchors);
                bool room = (screen == lobbyScreen || roomPicking) && OnlineRoom.Active;
                stage.SetRaft(catalog.GetRaft(room ? OnlineRoom.Current.State.raft : GameSelection.RaftIndex));
                stage.SetBiome(room ? OnlineRoom.Current.State.biome : GameSelection.BiomeChoice);
                if (!room && stage.ShowingLineup)
                {
                    stage.SetCrew(GameSelection.PlayerCount, GameSelection.BotCount);
                }
            }
            ShowTutorial(screen);
        }

        RectTransform BuildHome()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Home");
            Text title = UiKit.CreateText(screen, "Title", new Vector2(0.5f, 0.5f), new Vector2(0f, 260f), new Vector2(1600f, 200f), 150, TextAnchor.MiddleCenter);
            title.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            title.text = "Fart Rafter";
            UiKit.AddOutline(title, 4f);

            Text subtitle = UiKit.CreateText(screen, "Subtitle", new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1600f, 80f), 48, TextAnchor.MiddleCenter);
            subtitle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            subtitle.text = "Osur, yön ver, salı bitişe götür!";

            Text play = UiKit.CreateButton(screen, "Oyna", new Vector2(0.5f, 0.5f), new Vector2(0f, -60f), new Vector2(460f, 150f), ShowModes);
            homePlay = play;
            play.fontSize = 64;
            ((RectTransform)play.transform.parent).pivot = new Vector2(0.5f, 0.5f);

            homeOnline = UiKit.CreateCenterButton(screen, "Online", new Vector2(-570f, -250f), new Vector2(360f, 110f), ShowOnline);
            friendsButton = (RectTransform)UiKit.CreateCenterButton(screen, "Arkadaşlar", new Vector2(-190f, -250f), new Vector2(360f, 110f), ShowFriends).transform.parent;
            UiKit.AddBadge(friendsButton, out friendsBadge, out friendsBadgeText);
            homeBoard = UiKit.CreateCenterButton(screen, "Puan Listesi", new Vector2(190f, -250f), new Vector2(360f, 110f), ShowLeaderboard);
            homeSettings = UiKit.CreateCenterButton(screen, "Ayarlar", new Vector2(570f, -250f), new Vector2(360f, 110f), ShowSettings);
            totalText = UiKit.CreateCenterText(screen, "Total", new Vector2(0f, 55f), new Vector2(1000f, 60f), 40);
            totalText.color = UiKit.StarOnColor;

            Text version = UiKit.CreateText(screen, "Version", new Vector2(1f, 0f), new Vector2(-30f, 20f), new Vector2(400f, 40f), 26, TextAnchor.LowerRight);
            version.text = "v" + Application.version;

            soundText = UiKit.CreateButton(screen, SoundLabel(), new Vector2(0f, 0f), new Vector2(30f, 20f), new Vector2(240f, 80f), ToggleSound);
            musicText = UiKit.CreateButton(screen, MusicLabel(), new Vector2(0f, 0f), new Vector2(290f, 20f), new Vector2(240f, 80f), ToggleMusic);
            BuildQuestButton(screen);
            return screen;
        }

        RectTransform BuildModes()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Modes");
            Text title = UiKit.CreateText(screen, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1200f, 100f), 72, TextAnchor.UpperCenter);
            title.text = "Mod Seç";
            UiKit.AddOutline(title, 3f);
            UiKit.CreateButton(screen, "Geri", new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(200f, 90f), LeaveModes);

            practiceCard = CreateModeCard(screen, -300f, "Alıştırma", "5 kısa harita\nKontrolleri öğren, puan topla", ShowLevels);
            adventureCard = CreateModeCard(screen, 300f, "Macera", "Her seferinde yeni bir nehir\nKolay, Orta, Zor, Uzman", ShowDifficulties);
            return screen;
        }

        RectTransform CreateModeCard(RectTransform screen, float x, string name, string description, Action onClick)
        {
            RectTransform rect = UiKit.CreateImage(screen, name, null, CardColor, new Vector2(0.5f, 0.5f), new Vector2(x, -40f), new Vector2(520f, 460f));
            rect.pivot = new Vector2(0.5f, 0.5f);
            var image = rect.GetComponent<Image>();
            image.sprite = UiKit.Rounded;
            image.type = Image.Type.Sliced;
            image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.onClick.AddListener(() =>
            {
                GameAudio.Play(GameAudio.Sfx.Click, 0.7f);
                onClick();
            });
            rect.gameObject.AddComponent<ButtonBounce>();

            Text heading = UiKit.CreateText(rect, "Name", new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(500f, 110f), 80, TextAnchor.UpperCenter);
            heading.text = name;
            UiKit.AddOutline(heading, 3f);
            Text body = UiKit.CreateText(rect, "Description", new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(480f, 200f), 36, TextAnchor.LowerCenter);
            body.text = description;
            return rect;
        }

        RectTransform BuildDifficulties()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Difficulties");
            Text title = UiKit.CreateText(screen, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1200f, 100f), 72, TextAnchor.UpperCenter);
            title.text = "Macera · Zorluk";
            UiKit.AddOutline(title, 3f);
            UiKit.CreateButton(screen, "Geri", new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(200f, 90f), ShowModes);

            int count = catalog != null && catalog.difficulties != null ? catalog.difficulties.Length : 0;
            difficultyBest = new Text[count];
            const float cardWidth = 360f;
            const float gap = 30f;
            float startX = -(count - 1) * (cardWidth + gap) * 0.5f;
            for (int i = 0; i < count; i++)
            {
                int index = i;
                AdventureDifficulty difficulty = catalog.difficulties[i];
                RectTransform rect = UiKit.CreateImage(screen, "Difficulty_" + i, null, CardColor, new Vector2(0.5f, 0.5f), new Vector2(startX + i * (cardWidth + gap), -40f), new Vector2(cardWidth, 420f));
                rect.pivot = new Vector2(0.5f, 0.5f);
                var image = rect.GetComponent<Image>();
                image.sprite = UiKit.Rounded;
                image.type = Image.Type.Sliced;
                image.raycastTarget = true;
                var button = rect.gameObject.AddComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    GameAudio.Play(GameAudio.Sfx.Click, 0.7f);
                    SelectDifficulty(index);
                });
                rect.gameObject.AddComponent<ButtonBounce>();

                Text name = UiKit.CreateText(rect, "Name", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(cardWidth, 100f), 64, TextAnchor.UpperCenter);
                name.text = difficulty.displayName;
                name.color = DifficultyColor(i, count);
                UiKit.AddOutline(name, 3f);
                Text length = UiKit.CreateText(rect, "Length", new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(cardWidth - 20f, 60f), 40, TextAnchor.MiddleCenter);
                length.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                length.text = string.IsNullOrEmpty(difficulty.summary) ? (difficulty.length >= 1000f ? $"~{difficulty.length / 1000f:0.#} km" : $"~{difficulty.length:0} m") : difficulty.summary;
                length.fontSize = 30;
                difficultyBest[i] = UiKit.CreateText(rect, "Best", new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(cardWidth - 20f, 60f), 30, TextAnchor.LowerCenter);
            }
            return screen;
        }

        static Color DifficultyColor(int index, int count)
        {
            float t = count > 1 ? index / (float)(count - 1) : 0f;
            return Color.Lerp(new Color(0.5f, 0.95f, 0.5f), new Color(1f, 0.4f, 0.35f), t);
        }

        void ShowModes()
        {
            Show(modeScreen);
        }

        void ShowDifficulties()
        {
            for (int i = 0; i < difficultyBest.Length; i++)
            {
                difficultyBest[i].text = ProgressStore.TryGetBestScore("macera_" + catalog.difficulties[i].id, out int bestScore)
                    ? "En iyi: " + bestScore + " puan"
                    : ProgressStore.TryGetBestTime("macera_" + catalog.difficulties[i].id, out float best)
                    ? "En iyi: " + UiKit.FormatTime(best)
                    : "Henüz oynanmadı";
            }
            Show(difficultyScreen);
        }

        void SelectDifficulty(int index)
        {
            if (roomPicking)
            {
                PickRoom(true, index);
                return;
            }
            SelectDifficultyLocal(index);
        }

        void SelectDifficultyLocal(int index)
        {
            GameSelection.CurrentMode = GameSelection.Mode.Adventure;
            GameSelection.DifficultyIndex = index;
            setupTitle.text = "Macera · " + catalog.difficulties[index].displayName;
            RefreshSetup();
            Show(setupScreen);
        }

        void BackFromSetup()
        {
            if (GameSelection.CurrentMode == GameSelection.Mode.Adventure)
            {
                ShowDifficulties();
            }
            else
            {
                ShowLevels();
            }
        }

        RectTransform BuildLevels()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Levels");
            Text title = UiKit.CreateText(screen, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1200f, 100f), 72, TextAnchor.UpperCenter);
            title.text = "Seviye Seç";
            UiKit.AddOutline(title, 3f);
            UiKit.CreateButton(screen, "Geri", new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(200f, 90f), ShowModes);

            int count = catalog != null && catalog.levels != null ? catalog.levels.Length : 0;
            cards = new LevelCard[count];
            const float cardWidth = 320f;
            const float cardHeight = 420f;
            const float gap = 30f;
            float startX = -(count - 1) * (cardWidth + gap) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                int index = i;
                var card = new LevelCard();
                RectTransform rect = UiKit.CreateImage(screen, "Level_" + (i + 1), null, CardColor, new Vector2(0.5f, 0.5f), new Vector2(startX + i * (cardWidth + gap), -40f), new Vector2(cardWidth, cardHeight));
                rect.pivot = new Vector2(0.5f, 0.5f);
                card.background = rect.GetComponent<Image>();
                card.background.sprite = UiKit.Rounded;
                card.background.type = Image.Type.Sliced;
                card.background.raycastTarget = true;
                card.button = rect.gameObject.AddComponent<Button>();
                card.button.onClick.AddListener(() =>
                {
                    GameAudio.Play(GameAudio.Sfx.Click, 0.7f);
                    SelectLevel(index);
                });
                rect.gameObject.AddComponent<ButtonBounce>();

                Text number = UiKit.CreateText(rect, "Number", new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(cardWidth, 130f), 110, TextAnchor.UpperCenter);
                number.text = (i + 1).ToString();
                card.title = UiKit.CreateText(rect, "Name", new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(cardWidth - 20f, 90f), 36, TextAnchor.UpperCenter);
                card.best = UiKit.CreateText(rect, "Best", new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(cardWidth - 20f, 60f), 30, TextAnchor.LowerCenter);
                cards[i] = card;
            }
            return screen;
        }

        void RefreshLevels()
        {
            for (int i = 0; i < cards.Length; i++)
            {
                LevelCard card = cards[i];
                LevelDefinition level = catalog.levels[i];
                bool unlocked = ProgressStore.IsUnlocked(catalog, i);
                card.button.interactable = unlocked;
                card.background.color = unlocked ? CardColor : new Color(0f, 0f, 0f, 0.3f);
                card.title.text = level.displayName;
                card.title.color = unlocked ? Color.white : LockedColor;

                if (!unlocked)
                {
                    card.best.text = "Kilitli";
                }
                else if (ProgressStore.TryGetBestScore(level.id, out int bestScore))
                {
                    card.best.text = "En iyi: " + bestScore + " puan";
                }
                else if (ProgressStore.TryGetBestTime(level, out float best))
                {
                    card.best.text = "En iyi: " + UiKit.FormatTime(best);
                }
                else
                {
                    card.best.text = "Henüz oynanmadı";
                }
            }
        }

        RectTransform BuildSetup()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Setup");
            setupTitle = UiKit.CreateText(screen, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1400f, 100f), 72, TextAnchor.UpperCenter);
            UiKit.AddOutline(setupTitle, 3f);
            UiKit.CreateButton(screen, "Geri", new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(200f, 90f), BackFromSetup);

            slots = new AvatarSlot[GameSelection.MaxPlayers];
            slotAnchors = new RectTransform[slots.Length];
            for (int player = 0; player < slots.Length; player++)
            {
                slots[player] = BuildAvatarSlot(screen, player);
                slotAnchors[player] = slots[player].swatch.rectTransform;
            }

            playersText = CreateSetupButton(screen, "Players", -630f, CyclePlayers);
            botCountText = CreateSetupButton(screen, "BotCount", -210f, CycleBotCount);
            botLevelText = CreateSetupButton(screen, "BotLevel", 210f, CycleBotLevel);
            biomeText = CreateSetupButton(screen, "Biome", 630f, CycleBiome);
            raftText = UiKit.CreateButton(screen, "Sal", new Vector2(0f, 0f), new Vector2(40f, 50f), new Vector2(600f, 170f), CycleRaft);
            raftText.fontSize = 40;
            raftText.lineSpacing = 0.9f;
            ((RectTransform)raftText.transform.parent).pivot = new Vector2(0f, 0f);
            raftInfo = UiKit.CreateText(screen, "RaftInfo", new Vector2(0f, 0f), new Vector2(40f, 228f), new Vector2(600f, 40f), 28, TextAnchor.LowerCenter);
            weatherText = UiKit.CreateButton(screen, "Hava", new Vector2(1f, 0f), new Vector2(-40f, 50f), new Vector2(560f, 170f), CycleWeather);
            weatherText.fontSize = 40;
            weatherText.lineSpacing = 0.9f;
            ((RectTransform)weatherText.transform.parent).pivot = new Vector2(1f, 0f);

            Text start = UiKit.CreateButton(screen, "Başla", new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(460f, 140f), StartGame);
            setupStart = start;
            start.fontSize = 60;
            ((RectTransform)start.transform.parent).pivot = new Vector2(0.5f, 0f);
            return screen;
        }

        static Text CreateSetupButton(RectTransform screen, string name, float x, System.Action onClick)
        {
            Text text = UiKit.CreateButton(screen, name, new Vector2(0.5f, 0f), new Vector2(x, 250f), new Vector2(380f, 90f), onClick);
            ((RectTransform)text.transform.parent).pivot = new Vector2(0.5f, 0f);
            return text;
        }

        AvatarSlot BuildAvatarSlot(RectTransform screen, int player)
        {
            var slot = new AvatarSlot();
            slot.root = (RectTransform)new GameObject("Slot" + player, typeof(RectTransform)).transform;
            slot.root.SetParent(screen, false);
            slot.root.anchorMin = slot.root.anchorMax = new Vector2(0.5f, 0.5f);
            slot.root.pivot = new Vector2(0.5f, 0.5f);
            slot.root.sizeDelta = new Vector2(440f, 500f);

            slot.heading = UiKit.CreateText(slot.root, "Heading", new Vector2(0.5f, 0.5f), new Vector2(0f, 190f), new Vector2(440f, 70f), 44, TextAnchor.MiddleCenter);
            slot.heading.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            slot.heading.color = Color.Lerp(PlayerLayout.ColorOf(player), Color.white, 0.35f);

            RectTransform swatch = UiKit.CreateImage(slot.root, "Swatch", UiKit.Circle, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240f, 240f));
            swatch.pivot = new Vector2(0.5f, 0.5f);
            slot.swatch = swatch.GetComponent<Image>();
            slot.swatch.enabled = false;

            slot.name = UiKit.CreateText(slot.root, "AvatarName", new Vector2(0.5f, 0.5f), new Vector2(0f, -118f), new Vector2(400f, 50f), 44, TextAnchor.MiddleCenter);
            slot.name.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            int captured = player;
            slot.costume = UiKit.CreateButton(slot.root, "Kostüm", new Vector2(0.5f, 0.5f), new Vector2(0f, -170f), new Vector2(400f, 58f), () => CycleCostume(captured));
            slot.costume.fontSize = 32;
            ((RectTransform)slot.costume.transform.parent).pivot = new Vector2(0.5f, 0.5f);
            slot.gasPips = StatBar(slot.root, "Depo", -212f, new Color(0.45f, 0.85f, 0.4f));
            slot.powerPips = StatBar(slot.root, "Güç", -234f, new Color(1f, 0.6f, 0.25f));
            slot.refillPips = StatBar(slot.root, "Dolum", -256f, new Color(0.4f, 0.75f, 1f));
            Text previous = UiKit.CreateButton(slot.root, "<", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110f, 110f), () => ChangeAvatar(captured, -1));
            previous.fontSize = 60;
            slot.previous = (RectTransform)previous.transform.parent;
            slot.previous.pivot = new Vector2(0.5f, 0.5f);
            Text next = UiKit.CreateButton(slot.root, ">", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110f, 110f), () => ChangeAvatar(captured, 1));
            next.fontSize = 60;
            slot.next = (RectTransform)next.transform.parent;
            slot.next.pivot = new Vector2(0.5f, 0.5f);
            return slot;
        }

        const int StatPips = 5;

        static Image[] StatBar(RectTransform parent, string name, float y, Color color)
        {
            Text label = UiKit.CreateText(parent, name + "Label", new Vector2(0.5f, 0.5f), new Vector2(-150f, y), new Vector2(110f, 24f), 22, TextAnchor.MiddleLeft);
            label.rectTransform.pivot = new Vector2(0f, 0.5f);
            label.text = name;
            var pips = new Image[StatPips];
            for (int i = 0; i < StatPips; i++)
            {
                RectTransform pip = UiKit.CreateImage(parent, name + "Pip" + i, UiKit.Rounded, color, new Vector2(0.5f, 0.5f), new Vector2(-40f + i * 40f, y), new Vector2(34f, 16f));
                pip.pivot = new Vector2(0f, 0.5f);
                pips[i] = pip.GetComponent<Image>();
                pips[i].type = Image.Type.Sliced;
            }
            return pips;
        }

        static void SetPips(Image[] pips, float value, float min, float max, Color color)
        {
            float t = max - min > 0.0001f ? (value - min) / (max - min) : 1f;
            int filled = Mathf.Clamp(1 + Mathf.RoundToInt(t * (StatPips - 1)), 1, StatPips);
            for (int i = 0; i < pips.Length; i++)
            {
                pips[i].color = i < filled ? color : new Color(color.r, color.g, color.b, 0.18f);
            }
        }

        void RefreshStats(AvatarSlot slot, AvatarDefinition avatar)
        {
            if (avatar == null || catalog.avatars == null)
            {
                return;
            }
            float maxGas = 0.1f;
            float maxPower = 0.1f;
            float maxRefill = 0.1f;
            float minGas = float.MaxValue;
            float minPower = float.MaxValue;
            float minRefill = float.MaxValue;
            foreach (AvatarDefinition other in catalog.avatars)
            {
                if (other != null)
                {
                    maxGas = Mathf.Max(maxGas, other.gasCapacitySeconds);
                    maxPower = Mathf.Max(maxPower, other.thrustForce);
                    maxRefill = Mathf.Max(maxRefill, other.refillPerSecond);
                    minGas = Mathf.Min(minGas, other.gasCapacitySeconds);
                    minPower = Mathf.Min(minPower, other.thrustForce);
                    minRefill = Mathf.Min(minRefill, other.refillPerSecond);
                }
            }
            SetPips(slot.gasPips, avatar.gasCapacitySeconds, minGas, maxGas, new Color(0.45f, 0.85f, 0.4f));
            SetPips(slot.powerPips, avatar.thrustForce, minPower, maxPower, new Color(1f, 0.6f, 0.25f));
            SetPips(slot.refillPips, avatar.refillPerSecond, minRefill, maxRefill, new Color(0.4f, 0.75f, 1f));
        }

        void LayoutSlots(int playerCount)
        {
            bool compact = playerCount > 2;
            float spacing = playerCount <= 2 ? 840f : playerCount == 3 ? 560f : 460f;
            float swatchSize = compact ? 190f : 240f;
            float arrowSize = compact ? 90f : 110f;
            float arrowOffset = swatchSize * 0.5f + arrowSize * 0.5f + 12f;

            for (int player = 0; player < slots.Length; player++)
            {
                AvatarSlot slot = slots[player];
                bool active = player < playerCount;
                slot.root.gameObject.SetActive(active);
                if (!active)
                {
                    continue;
                }
                float slotX = playerCount == 1 ? -560f : (player - (playerCount - 1) * 0.5f) * spacing;
                slot.root.anchoredPosition = new Vector2(slotX, 178f);
                string seat = PlayerLayout.SeatName(player, playerCount);
                slot.heading.text = seat.Length > 0 ? $"Oyuncu {player + 1} · {seat}" : $"Oyuncu {player + 1}";
                slot.heading.fontSize = compact ? 36 : 44;
                slot.name.fontSize = compact ? 40 : 48;
                slot.swatch.rectTransform.sizeDelta = new Vector2(swatchSize, swatchSize);
                slot.previous.sizeDelta = new Vector2(arrowSize, arrowSize);
                slot.previous.anchoredPosition = new Vector2(-arrowOffset, 0f);
                slot.next.sizeDelta = new Vector2(arrowSize, arrowSize);
                slot.next.anchoredPosition = new Vector2(arrowOffset, 0f);
            }
        }

        void ShowLevels()
        {
            RefreshLevels();
            Show(levelScreen);
        }

        void SelectLevel(int index)
        {
            if (!ProgressStore.IsUnlocked(catalog, index))
            {
                return;
            }
            if (roomPicking)
            {
                PickRoom(false, index);
                return;
            }
            GameSelection.CurrentMode = GameSelection.Mode.Practice;
            GameSelection.LevelIndex = index;
            setupTitle.text = $"{index + 1}. {catalog.levels[index].displayName}";
            RefreshSetup();
            Show(setupScreen);
        }

        void RefreshSetup()
        {
            int playerCount = GameSelection.PlayerCount;
            LayoutSlots(playerCount);
            playersText.text = playerCount + " oyuncu";
            int botCount = GameSelection.BotCount;
            botCountText.text = botCount == 0 ? "Bot yok" : botCount + " bot";
            botCountText.transform.parent.gameObject.SetActive(GameSelection.MaxBots > 0);
            botLevelText.transform.parent.gameObject.SetActive(botCount > 0);

            for (int player = 0; player < slots.Length; player++)
            {
                AvatarDefinition avatar = catalog.GetAvatar(GameSelection.GetAvatar(player));
                Color color = PlayerLayout.ColorOf(player);
                slots[player].swatch.color = new Color(color.r, color.g, color.b, 0.3f);
                slots[player].name.text = avatar != null ? avatar.displayName : "-";
                slots[player].costume.text = "Kostüm: " + (avatar != null ? avatar.CostumeName(GameSelection.GetCostume(player)) : "Klasik") + "  \u25B6";
                RefreshStats(slots[player], avatar);
            }

            int botIndex = GameSelection.BotLevelIndex;
            bool known = catalog.botLevels != null && botIndex < catalog.botLevels.Length;
            botLevelText.text = "Bot: " + (known ? catalog.botLevels[botIndex].displayName : "-");
            biomeText.text = "Harita: " + Biomes.ChoiceName(GameSelection.BiomeChoice);
            weatherText.text = "Hava: " + Weathers.Name(GameSelection.WeatherChoice) + "  \u25B6\n<size=26>" + Weathers.Effect(GameSelection.WeatherChoice) + "</size>";
            RaftVariant raft = catalog.GetRaft(GameSelection.RaftIndex);
            raftText.text = raft != null ? "Sal: " + raft.name + "  \u25B6\n<size=28>Dayanıklılık " + raft.DurabilityMarks + "   Hızlanma " + raft.AccelerationMarks + "</size>\n<size=24>" + raft.description + "</size>" : "Sal: Klasik Sal";
            if (stage != null)
            {
                stage.SetRaft(raft);
                stage.SetBiome(GameSelection.BiomeChoice);
            }
            raftInfo.text = "";
        }

        void CycleCostume(int player)
        {
            AvatarDefinition avatar = catalog.GetAvatar(GameSelection.GetAvatar(player));
            if (avatar == null)
            {
                return;
            }
            GameSelection.SetCostume(player, (GameSelection.GetCostume(player) + 1) % avatar.CostumeCount);
            if (stage != null)
            {
                stage.SetAvatar(player, avatar, true);
            }
            RefreshSetup();
        }

        void CycleRaft()
        {
            GameSelection.RaftIndex = catalog.NextRaft(GameSelection.RaftIndex);
            RaftVariant raft = catalog.GetRaft(GameSelection.RaftIndex);
            RefreshSetup();
        }

        Text weatherText;

        void CycleWeather()
        {
            GameSelection.WeatherChoice = (GameSelection.WeatherChoice + 1) % Weathers.Count;
            RefreshSetup();
        }

        void CycleBiome()
        {
            GameSelection.BiomeChoice = (GameSelection.BiomeChoice + 1) % (Biomes.RandomChoice + 1);
            RefreshSetup();
        }

        void ChangeAvatar(int player, int step)
        {
            int count = catalog.avatars != null ? catalog.avatars.Length : 0;
            if (count == 0)
            {
                return;
            }
            int index = (GameSelection.GetAvatar(player) + step + count) % count;
            GameSelection.SetAvatar(player, index);
            GameSelection.SetCostume(player, 0);
            if (stage != null)
            {
                stage.SetAvatar(player, catalog.GetAvatar(index), true);
            }
            RefreshSetup();
        }

        void CyclePlayers()
        {
            int count = GameSelection.PlayerCount + 1;
            GameSelection.PlayerCount = count > GameSelection.MaxPlayers ? GameSelection.MinPlayers : count;
            RefreshCrew();
        }

        void CycleBotCount()
        {
            int count = GameSelection.BotCount + 1;
            GameSelection.BotCount = count > GameSelection.MaxBots ? 0 : count;
            RefreshCrew();
        }

        void CycleBotLevel()
        {
            int count = catalog.botLevels != null ? catalog.botLevels.Length : 0;
            if (count == 0)
            {
                return;
            }
            GameSelection.BotLevelIndex = (GameSelection.BotLevelIndex + 1) % count;
            RefreshSetup();
        }

        void RefreshCrew()
        {
            if (stage != null)
            {
                stage.SetCrew(GameSelection.PlayerCount, GameSelection.BotCount);
            }
            RefreshSetup();
        }

        string SoundLabel()
        {
            return GameAudio.SfxOn ? "Ses: Açık" : "Ses: Kapalı";
        }

        string MusicLabel()
        {
            return GameAudio.MusicOn ? "Müzik: Açık" : "Müzik: Kapalı";
        }

        void ToggleSound()
        {
            GameAudio.SetSfx(!GameAudio.SfxOn);
            soundText.text = SoundLabel();
        }

        void ToggleMusic()
        {
            GameAudio.SetMusic(!GameAudio.MusicOn);
            musicText.text = MusicLabel();
        }

        void StartGame()
        {
            SceneManager.LoadScene(GameSelection.GameScene);
        }
    }
}
