using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FartRafters
{
    public class GameHud : MonoBehaviour
    {
        public GameBootstrap bootstrap;
        public CourseManager course;
        public GameSession session;
        public RaftTuning tuning;

        static readonly Color GasEmptyColor = new Color(0.95f, 0.55f, 0.2f, 1f);

        Canvas canvas;
        RectTransform canvasRect;
        Text timerText;
        Text checkpointText;
        Text botButtonText;
        Text finishTitle;
        Text finishTime;
        Text finishBest;
        RectTransform finishRows;
        GameObject nextButton;
        GameObject retryButton;
        GameObject finishMenuButton;
        GameObject finishPanel;
        GameObject tuningPanel;
        GameObject pausePanel;
        Rider[] riders;
        PlayerBrain[] brains;
        GasBar[] bars;
        Stick[] sticks;
        GasButton[] gasButtons;
        bool resultRecorded;
        RaftHealth health;
        RaftBoosts boosts;
        RectTransform healthFill;
        Image healthImage;
        Text boostText;
        Text messageText;
        float messageUntil;
        bool paused;

        bool Compact => riders.Length > 2;
        bool HasBots => session != null && session.botRiders != null && session.botRiders.Length > 0;

        class GasBar
        {
            public Text autopilot;
            public RectTransform fill;
            public Image fillImage;
            public Color color;
        }

        class Stick
        {
            public RectTransform ring;
            public RectTransform knob;
        }

        class GasButton
        {
            public RectTransform rect;
            public Image image;
            public Color color;
        }

        void Start()
        {
            UiKit.EnsureEventSystem();
            canvasRect = UiKit.CreateCanvas(transform, out canvas);
            riders = bootstrap != null && bootstrap.PlayerRiders != null ? bootstrap.PlayerRiders : new Rider[0];
            brains = bootstrap != null && bootstrap.ActivePlayerBrains != null ? bootstrap.ActivePlayerBrains : new PlayerBrain[0];

            if (Compact)
            {
                BuildCompactBar();
            }
            else
            {
                BuildTopBar();
            }

            bars = new GasBar[riders.Length];
            sticks = new Stick[riders.Length];
            gasButtons = new GasButton[riders.Length];
            for (int i = 0; i < riders.Length; i++)
            {
                bars[i] = BuildGasBar(i);
                sticks[i] = BuildStick(i);
                gasButtons[i] = BuildGasButton(i);
            }

            BuildHealth();
            BuildFinishPanel();
            BuildPausePanel();
            BuildTuningPanel();
            ShowGameTutorial();
        }

        void ShowGameTutorial()
        {
            if (OnlineGame.Active || BotSimulation.Running || riders.Length == 0)
            {
                return;
            }
            bool gyro = brains.Length > 0 && brains[0] != null && brains[0].UsesGyro;
            var steps = new System.Collections.Generic.List<TutorialGuide.Step>
            {
                new TutorialGuide.Step((RectTransform)null, "Amaç: salı tüm kontrol noktalarından geçirip bitiş bayrağına ulaştırmak. Ne kadar hızlı ve az kazayla bitirirsen o kadar çok puan!"),
                new TutorialGuide.Step((RectTransform)null, gyro ? "Yön vermek için telefonu eğ: sağa eğersen karakter sağa, ileri eğersen ileri döner." : "Ekranın kendi tarafında parmağını sürükle: karakterin o yöne döner. Bırakınca son yöne bakmaya devam eder."),
                new TutorialGuide.Step(gasButtons[0] != null ? gasButtons[0].rect : null, "GAZ'a basılı tut: karakter baktığı yöne osurur ve salı iter. Herkes aynı yöne iterse sal hızlanır!"),
                new TutorialGuide.Step(bars[0] != null && bars[0].fill != null ? (RectTransform)bars[0].fill.parent : null, "Gaz çubuğu: osurdukça biter, bırakınca dolar. Tamamen biterse biraz dinlenmen gerekir."),
                new TutorialGuide.Step(healthFill != null ? (RectTransform)healthFill.parent : null, "Salın canı: kayalara ve dikenli kıyılara çarpınca azalır, kendiliğinden dolmaz. Tamir Kiti bulursan can tamamen dolar. Biterse son kontrol noktasına dönersin."),
                new TutorialGuide.Step(checkpointText, "Kontrol noktaları: bayraklı şamandıraların arasından geç. Timsaha yakalanırsan son geçtiğin noktaya dönersin."),
                new TutorialGuide.Step(bars[0] != null && bars[0].autopilot != null ? (RectTransform)bars[0].autopilot.transform.parent : null, "Bota Devret: karakterini bir süre bota bırak, Devral ile geri al. Botla geçen süre puanını biraz düşürür."),
                new TutorialGuide.Step((RectTransform)null, "Sudaki eklentileri topla: yemekler salına güç verir, Kalkan seni korur, Mıknatıs eklentileri çeker. İyi eğlenceler!")
            };
            if (!TutorialGuide.Show(canvasRect, "game", steps, true, ShowBiomeTutorial))
            {
                ShowBiomeTutorial();
            }
        }

        void ShowBiomeTutorial()
        {
            Biome biome = bootstrap != null ? bootstrap.Biome : Biome.River;
            string text;
            switch (biome)
            {
                case Biome.Polar:
                    text = "Kutup haritası: buzlanmış bölgelerde sal kayar, durmak ve dönmek zorlaşır. Buza girmeden yavaşla! Timsahın yerinde orkalar var.";
                    break;
                case Biome.Ocean:
                    text = "Okyanus haritası: dalgalar büyük, bir taraf açık deniz. Girdaplar salı içine çeker ve döndürür; merkezine girersen sal hasar alır. Köpek balıklarından kaç!";
                    break;
                case Biome.Fun:
                    text = "Eğlence haritası: havada kaydıraklardasın! Rampalarda sal hızlanır. Dikenli topaklar seni geri sektirir, kayan dikenli silindirlere ve dönen kollara dikkat.";
                    break;
                case Biome.Lava:
                    text = "Lav Nehri: yanardağdan lav topları düşer, gölgesini görünce kaç! Lav girdapları salı içine çeker. Ateş semenderlerine yakalanma.";
                    break;
                case Biome.Night:
                    text = "Gece Ormanı: karanlıkta salının feneri yolu aydınlatır. Su fışkırtan gayzerler salı havaya atar. Dev yayın balıklarına dikkat!";
                    break;
                case Biome.Desert:
                    text = "Çöl Kanyonu: kanyon duvarlarından kayalar düşer, gölgelerden uzak dur. Kıyıdaki kaktüsler salı çizer. Nehrin timsahları burada da var!";
                    break;
                case Biome.Swamp:
                    text = "Bataklık: sazlıklar ve yosunlu kütükler arasında yol bul. Bataklık gazı gayzerleri salı zıplatır. Timsahlar sinsice bekliyor!";
                    break;
                default:
                    return;
            }
            TutorialGuide.Show(canvasRect, "biome_" + biome.ToString().ToLowerInvariant(), new[] { new TutorialGuide.Step((RectTransform)null, text) }, true);
        }

        void Update()
        {
            if (course == null)
            {
                return;
            }

            timerText.text = UiKit.FormatTime(course.ElapsedTime);
            checkpointText.text = $"Checkpoint {course.LastCheckpoint}/{course.CheckpointGoal}";
            for (int i = 0; i < riders.Length; i++)
            {
                PlayerBrain brain = i < brains.Length ? brains[i] : null;
                UpdateGasBar(bars[i], riders[i]);
                UpdateStick(sticks[i], brain);
                UpdateGasButton(gasButtons[i], brain);
            }

            UpdateHealth();
            UpdateMessage();

            if (course.Finished && !resultRecorded && (!OnlineGame.IsClient || OnlineGame.RemoteReport != null))
            {
                resultRecorded = true;
                SetPaused(false);
                ShowResult();
            }

            Keyboard keyboard = Keyboard.current;
            if (!course.Finished && keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                SetPaused(!paused);
            }

            PlayerBrain.InputBlocked = tuningPanel.activeSelf || paused || course.Finished;
        }

        void OnDestroy()
        {
            if (health != null)
            {
                health.Broken -= OnBroken;
                health.Caught -= OnCaught;
                health.ShieldSaved -= OnShieldSaved;
                health.Shocked -= OnShocked;
            }
            CourseNotices.Notice -= ShowMessage;
            DailyQuests.Completed -= OnQuestCompleted;
            if (boosts != null)
            {
                boosts.Collected -= OnCollected;
            }
            Rider.StateChanged -= OnRiderState;
            PlayerBrain.InputBlocked = false;
            Time.timeScale = 1f;
        }

        void SetPaused(bool value)
        {
            paused = value;
            if (pausePanel != null)
            {
                pausePanel.SetActive(value);
            }
            if (!value && tuningPanel != null)
            {
                tuningPanel.SetActive(false);
            }
            Time.timeScale = value && !OnlineGame.Active ? 0f : 1f;
        }

        void ShowResult()
        {
            float time = course.ElapsedTime;
            LevelDefinition level = bootstrap != null ? bootstrap.Level : null;
            ScoreTracker tracker = ScoreTracker.Current;
            ScoreReport report = OnlineGame.IsClient ? OnlineGame.RemoteReport : tracker != null ? tracker.Report ?? tracker.Build() : null;
            string id = bootstrap != null ? ScoreTracker.ScoreId(bootstrap) : "level";
            bool adventure = bootstrap != null && bootstrap.IsAdventure;

            finishTitle.text = level != null ? $"{level.displayName} bitti!" : "Bitti!";
            string parText = report != null ? $"   ·   Hedef: {UiKit.FormatTime(report.par)}" : "";
            finishTime.text = $"Süre: {UiKit.FormatTime(time)}{parText}   ·   Başa dönme: {course.RespawnCount}";

            finishBest.text = "";
            PlayerScore main = report != null ? report.Main : null;
            if (report != null)
            {
                FillScoreTable(report);
            }
            if (main != null && !main.bot)
            {
                ProgressStore.RecordRun(id, time, main.total, adventure, out bool newBest, out int awarded);
                string repeat = awarded < main.total ? "   ·   Tekrar: %20" : "";
                finishBest.text = (newBest ? "Yeni rekor!   " : "") + $"Toplam puanın: {ProgressStore.TotalScore} (+{awarded}){repeat}";
                ScoreSync.Queue(report, id, awarded);
            }

            nextButton.SetActive(bootstrap != null && bootstrap.NextLevelUnlocked && !OnlineGame.Active);
            if (OnlineGame.Active)
            {
                bool roomHost = OnlineRoom.Current != null && OnlineRoom.Current.IsHost;
                retryButton.SetActive(true);
                retryButton.GetComponentInChildren<Text>().text = "Tekrar Oyna";
                finishMenuButton.GetComponentInChildren<Text>().text = roomHost ? "Odayı Kapat" : "Odadan Çık";
            }
            finishPanel.SetActive(true);
            GameAudio.Play(GameAudio.Sfx.Star, 0.8f, 1f);
        }

        void BuildTopBar()
        {
            timerText = UiKit.CreateText(canvasRect, "Timer", new Vector2(0f, 1f), new Vector2(30f, -25f), new Vector2(420f, 70f), 56, TextAnchor.UpperLeft);
            UiKit.AddOutline(timerText, 2f);
            checkpointText = UiKit.CreateText(canvasRect, "Checkpoint", new Vector2(0f, 1f), new Vector2(30f, -95f), new Vector2(520f, 50f), 36, TextAnchor.UpperLeft);

            UiKit.CreateButton(canvasRect, "Yeniden Doğ", new Vector2(1f, 1f), new Vector2(-30f, -25f), new Vector2(280f, 90f), () => course.Respawn());
            botButtonText = UiKit.CreateButton(canvasRect, BotLabel(), new Vector2(1f, 1f), new Vector2(-330f, -25f), new Vector2(280f, 90f), CycleBots);
            botButtonText.transform.parent.gameObject.SetActive(HasBots);
            float x = HasBots ? -630f : -330f;
            UiKit.CreateButton(canvasRect, "II  Mola", new Vector2(1f, 1f), new Vector2(x, -25f), new Vector2(240f, 90f), () => SetPaused(true));
            if (OnlineRoom.Active)
            {
                BuildEmoteMenu(x - 270f);
            }
        }

        RectTransform emotePanel;
        float lastEmote = -10f;

        void BuildEmoteMenu(float x)
        {
            UiKit.CreateButton(canvasRect, "Hareket", new Vector2(1f, 1f), new Vector2(x, -25f), new Vector2(240f, 90f), () => emotePanel.gameObject.SetActive(!emotePanel.gameObject.activeSelf));
            emotePanel = UiKit.CreateImage(canvasRect, "EmotePanel", UiKit.Rounded, new Color(0f, 0f, 0f, 0.45f), new Vector2(1f, 1f), new Vector2(x, -125f), new Vector2(240f, 20f + RoomMessages.Emotes.Length * 82f));
            emotePanel.pivot = new Vector2(1f, 1f);
            emotePanel.GetComponent<Image>().type = Image.Type.Sliced;
            for (int i = 0; i < RoomMessages.Emotes.Length; i++)
            {
                int index = RoomMessages.Emojis.Length + i;
                Text move = UiKit.CreateButton(emotePanel, RoomMessages.Emotes[i], new Vector2(0.5f, 1f), new Vector2(0f, -10f - i * 82f), new Vector2(220f, 72f), () =>
                {
                    emotePanel.gameObject.SetActive(false);
                    if (OnlineRoom.Current == null || Time.unscaledTime - lastEmote < 1f)
                    {
                        return;
                    }
                    lastEmote = Time.unscaledTime;
                    OnlineRoom.Current.SendEmoji(index);
                });
                move.fontSize = 30;
                ((RectTransform)move.transform.parent).pivot = new Vector2(0.5f, 1f);
            }
            emotePanel.gameObject.SetActive(false);
        }

        void BuildCompactBar()
        {
            Text pause = UiKit.CreateButton(canvasRect, "II", new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(130f, 90f), () => SetPaused(true));
            pause.fontSize = 44;
            ((RectTransform)pause.transform.parent).pivot = new Vector2(0.5f, 1f);

            timerText = UiKit.CreateText(canvasRect, "Timer", new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(400f, 66f), 50, TextAnchor.LowerCenter);
            UiKit.AddOutline(timerText, 2f);
            checkpointText = UiKit.CreateText(canvasRect, "Checkpoint", new Vector2(0.5f, 0f), new Vector2(0f, 84f), new Vector2(500f, 44f), 30, TextAnchor.LowerCenter);
        }

        void BuildPausePanel()
        {
            RectTransform panel = UiKit.CreatePanel(canvasRect, "PausePanel", new Vector2(640f, HasBots && Compact ? 760f : 640f));
            pausePanel = panel.gameObject;

            Text title = UiKit.CreateText(panel, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(600f, 80f), 56, TextAnchor.UpperCenter);
            title.text = "Mola";
            UiKit.AddOutline(title, 2f);

            var actions = new List<(string label, Action action)>
            {
                ("Devam", () => SetPaused(false)),
                ("Yeniden Doğ", () =>
                {
                    course.Respawn();
                    SetPaused(false);
                })
            };
            int botIndex = -1;
            if (HasBots && Compact)
            {
                botIndex = actions.Count;
                actions.Add((BotLabel(), CycleBots));
            }
            actions.Add(("Ayarlar", () => tuningPanel.SetActive(true)));
            actions.Add(("Menü", GoToMenu));

            for (int i = 0; i < actions.Count; i++)
            {
                Text label = UiKit.CreateButton(panel, actions[i].label, new Vector2(0.5f, 1f), new Vector2(0f, -130f - i * 120f), new Vector2(440f, 100f), actions[i].action);
                ((RectTransform)label.transform.parent).pivot = new Vector2(0.5f, 1f);
                if (i == botIndex)
                {
                    botButtonText = label;
                }
            }
            pausePanel.SetActive(false);
        }

        string BotLabel()
        {
            BotDifficulty level = session != null ? session.CurrentBotLevel : null;
            return level != null ? "Botlar: " + level.displayName : "Botlar: Kapalı";
        }

        void CycleBots()
        {
            if (session == null)
            {
                return;
            }
            session.CycleBotLevel();
            if (session.botLevelIndex >= 0)
            {
                GameSelection.BotLevelIndex = session.botLevelIndex;
            }
            if (botButtonText != null)
            {
                botButtonText.text = BotLabel();
            }
        }

        void ToggleAutopilot(int index)
        {
            if (index < 0 || index >= riders.Length || riders[index] == null || course == null || course.Finished)
            {
                return;
            }
            Rider rider = riders[index];
            bool on = !rider.Autopilot;
            if (OnlineGame.IsClient)
            {
                rider.SetAutopilot(on, null, true);
            }
            else
            {
                rider.SetAutopilot(on, session != null ? session.AutopilotLevel : null);
            }
            if (bars[index] != null && bars[index].autopilot != null)
            {
                bars[index].autopilot.text = rider.Autopilot ? "Devral" : "Bota Devret";
            }
            ShowMessage((index + 1) + ". oyuncu " + (rider.Autopilot ? "bota devretti" : "kontrolü geri aldı"), new Color(0.8f, 0.9f, 1f));
        }

        void GoToMenu()
        {
            Time.timeScale = 1f;
            if (OnlineRoom.Active)
            {
                OnlineRoom.Current.Leave();
                UnityEngine.SceneManagement.SceneManager.LoadScene(GameSelection.MenuScene);
                return;
            }
            if (bootstrap != null)
            {
                bootstrap.LoadMenu();
            }
        }

        void Retry()
        {
            Time.timeScale = 1f;
            if (OnlineRoom.Active)
            {
                OnlineRoom.Current.RequestRematch();
                retryButton.GetComponentInChildren<Text>().text = "Lobiye dönülüyor…";
                return;
            }
            course.Restart();
        }

        GasBar BuildGasBar(int index)
        {
            Rider rider = riders[index];
            bool left = PlayerLayout.IsLeft(index);
            bool top = PlayerLayout.IsTop(index, riders.Length);
            float width = Compact ? 320f : 420f;
            var size = new Vector2(width, 84f);

            var group = (RectTransform)new GameObject("GasBar_" + index, typeof(RectTransform)).transform;
            group.SetParent(canvasRect, false);
            group.anchorMin = group.anchorMax = new Vector2(left ? 0f : 1f, top ? 1f : 0f);
            group.pivot = new Vector2(0.5f, 0.5f);
            group.sizeDelta = size;
            group.anchoredPosition = new Vector2((left ? 1f : -1f) * (40f + width * 0.5f), (top ? -1f : 1f) * (40f + size.y * 0.5f));
            if (top)
            {
                group.localRotation = Quaternion.Euler(0f, 0f, 180f);
            }

            var bar = new GasBar { color = PlayerLayout.ColorOf(index) };
            RectTransform background = UiKit.CreateImage(group, "Background", null, UiKit.PanelColor, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(width, 36f));
            RectTransform fill = UiKit.CreateImage(background, "Fill", null, bar.color, new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(1f, 1f);
            fill.offsetMin = new Vector2(4f, 4f);
            fill.offsetMax = new Vector2(-4f, -4f);
            bar.fill = fill;
            bar.fillImage = fill.GetComponent<Image>();

            bool alignLeft = left != top;
            string avatarName = rider != null && rider.avatar != null ? rider.avatar.displayName : "";
            Text label = UiKit.CreateText(group, "Label", new Vector2(alignLeft ? 0f : 1f, 0f), new Vector2(0f, 44f), new Vector2(width, 40f), 30, alignLeft ? TextAnchor.LowerLeft : TextAnchor.LowerRight);
            label.text = $"{index + 1} · {avatarName}";
            label.color = Color.Lerp(bar.color, Color.white, 0.35f);

            int captured = index;
            bar.autopilot = UiKit.CreateButton(group, "Bota Devret", new Vector2(alignLeft ? 1f : 0f, 0f), new Vector2(alignLeft ? 125f : -125f, 18f), new Vector2(220f, 64f), () => ToggleAutopilot(captured));
            bar.autopilot.fontSize = 24;
            ((RectTransform)bar.autopilot.transform.parent).pivot = new Vector2(0.5f, 0.5f);
            return bar;
        }

        void UpdateGasBar(GasBar bar, Rider rider)
        {
            if (bar == null || rider == null)
            {
                return;
            }
            float value = rider.Gas.Normalized;
            bar.fill.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
            bar.fillImage.color = rider.Gas.Exhausted ? GasEmptyColor : bar.color;
        }

        Stick BuildStick(int index)
        {
            Color color = PlayerLayout.ColorOf(index);
            var stick = new Stick();
            stick.ring = UiKit.CreateImage(canvasRect, "StickRing", UiKit.Circle, new Color(color.r, color.g, color.b, 0.22f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200f, 200f));
            stick.knob = UiKit.CreateImage(stick.ring, "StickKnob", UiKit.Circle, new Color(1f, 1f, 1f, 0.6f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(90f, 90f));
            stick.ring.pivot = new Vector2(0.5f, 0.5f);
            stick.knob.pivot = new Vector2(0.5f, 0.5f);
            stick.ring.gameObject.SetActive(false);
            return stick;
        }

        void UpdateStick(Stick stick, PlayerBrain brain)
        {
            bool show = brain != null && brain.JoystickActive;
            if (stick.ring.gameObject.activeSelf != show)
            {
                stick.ring.gameObject.SetActive(show);
            }
            if (!show)
            {
                return;
            }

            float scale = canvas.scaleFactor > 0f ? canvas.scaleFactor : 1f;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, brain.JoystickOrigin, null, out Vector2 origin);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, brain.JoystickCurrent, null, out Vector2 current);
            float diameter = brain.MaxDragPixels * 2f / scale;
            stick.ring.anchoredPosition = origin;
            stick.ring.sizeDelta = new Vector2(diameter, diameter);
            stick.knob.anchoredPosition = current - origin;
        }

        GasButton BuildGasButton(int index)
        {
            var button = new GasButton { color = PlayerLayout.ColorOf(index) };
            button.rect = UiKit.CreateImage(canvasRect, "GasButton", UiKit.Circle, Tint(button.color, 0.35f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220f, 220f));
            button.rect.pivot = new Vector2(0.5f, 0.5f);
            if (PlayerLayout.IsTop(index, riders.Length))
            {
                button.rect.localRotation = Quaternion.Euler(0f, 0f, 180f);
            }
            button.image = button.rect.GetComponent<Image>();
            Text label = UiKit.CreateText(button.rect, "Label", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220f, 80f), Compact ? 38 : 44, TextAnchor.MiddleCenter);
            label.text = "GAZ";
            return button;
        }

        void UpdateGasButton(GasButton button, PlayerBrain brain)
        {
            if (button == null || brain == null)
            {
                return;
            }

            float scale = canvas.scaleFactor > 0f ? canvas.scaleFactor : 1f;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, brain.GasButtonCenter, null, out Vector2 center);
            float diameter = brain.GasButtonRadiusPixels * 2f / scale;
            button.rect.anchoredPosition = center;
            button.rect.sizeDelta = new Vector2(diameter, diameter);
            button.image.color = Tint(button.color, brain.GasHeld ? 0.85f : 0.35f);
        }

        static Color Tint(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        void BuildHealth()
        {
            health = course != null && course.raft != null ? course.raft.GetComponent<RaftHealth>() : null;
            boosts = course != null && course.raft != null ? course.raft.GetComponent<RaftBoosts>() : null;

            Vector2 anchor = Compact ? new Vector2(0.5f, 0f) : new Vector2(0f, 1f);
            Vector2 position = Compact ? new Vector2(-180f, 132f) : new Vector2(30f, -150f);
            const float width = 360f;

            Text label = UiKit.CreateText(canvasRect, "HealthLabel", anchor, position + new Vector2(0f, Compact ? 40f : 0f), new Vector2(width, 34f), 26, TextAnchor.UpperLeft);
            label.text = "Sal";
            RectTransform background = UiKit.CreateImage(canvasRect, "Health", null, UiKit.PanelColor, anchor, position + new Vector2(60f, Compact ? 40f : 0f), new Vector2(width - 60f, 30f));
            background.pivot = new Vector2(0f, 1f);
            healthFill = UiKit.CreateImage(background, "Fill", null, new Color(0.35f, 0.85f, 0.4f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            healthFill.anchorMin = new Vector2(0f, 0f);
            healthFill.anchorMax = new Vector2(1f, 1f);
            healthFill.offsetMin = new Vector2(3f, 3f);
            healthFill.offsetMax = new Vector2(-3f, -3f);
            healthImage = healthFill.GetComponent<Image>();
            momentumText = UiKit.CreateText(canvasRect, "Momentum", anchor, position + new Vector2(width + 14f, Compact ? 40f : 0f), new Vector2(220f, 34f), 26, TextAnchor.UpperLeft);
            UiKit.AddOutline(momentumText, 2f);

            boostText = UiKit.CreateText(canvasRect, "Boosts", new Vector2(0.5f, 1f), new Vector2(0f, Compact ? -122f : -28f), new Vector2(1300f, 60f), 46, TextAnchor.UpperCenter);
            boostText.rectTransform.pivot = new Vector2(0.5f, 1f);
            UiKit.AddOutline(boostText, 3f);

            messageText = UiKit.CreateText(canvasRect, "Message", new Vector2(0.5f, 0.5f), new Vector2(0f, 180f), new Vector2(1400f, 110f), 72, TextAnchor.MiddleCenter);
            messageText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            UiKit.AddOutline(messageText, 3f);
            messageText.gameObject.SetActive(false);

            if (health != null)
            {
                health.Broken += OnBroken;
                health.Caught += OnCaught;
                health.ShieldSaved += OnShieldSaved;
                health.Shocked += OnShocked;
            }
            else
            {
                label.gameObject.SetActive(false);
                background.gameObject.SetActive(false);
            }
            CourseNotices.Notice += ShowMessage;
            DailyQuests.Completed += OnQuestCompleted;
            if (boosts != null)
            {
                boosts.Collected += OnCollected;
            }
            Rider.StateChanged += OnRiderState;
        }

        void OnRiderState(Rider rider, RiderEvent state)
        {
            if (rider == null || rider.avatar == null || course == null || rider.raft != course.raft)
            {
                return;
            }
            string name = rider.avatar.displayName;
            switch (state)
            {
                case RiderEvent.Fell:
                    ShowMessage(name + " suya düştü!", new Color(0.55f, 0.85f, 1f));
                    if (rider.brain is PlayerBrain)
                    {
                        Haptics.Medium();
                    }
                    break;
                case RiderEvent.Eliminated:
                    ShowMessage(name + " elendi!", new Color(1f, 0.5f, 0.4f));
                    break;
                case RiderEvent.Returned:
                    ShowMessage(name + " geri döndü!", new Color(0.6f, 1f, 0.6f));
                    break;
            }
        }

        Text momentumText;

        void UpdateMomentum()
        {
            Raft raft = course != null ? course.raft : null;
            if (momentumText == null || raft == null)
            {
                return;
            }
            int bonus = Mathf.RoundToInt((raft.Momentum - 1f) * 100f);
            momentumText.text = bonus >= 2 ? "Süzülme +%" + bonus : "";
            momentumText.color = Color.Lerp(new Color(0.75f, 0.95f, 1f), new Color(1f, 0.85f, 0.3f), Mathf.InverseLerp(0f, Raft.MomentumPerLevel * RaftVariant.MaxDurability * 100f, bonus));
        }

        void UpdateHealth()
        {
            UpdateMomentum();
            if (health != null)
            {
                float value = Mathf.Clamp01(health.Normalized);
                healthFill.anchorMax = new Vector2(value, 1f);
                healthImage.color = Color.Lerp(new Color(0.92f, 0.28f, 0.22f), new Color(0.35f, 0.85f, 0.4f), Mathf.InverseLerp(0.15f, 0.7f, value));
            }
            if (boosts == null)
            {
                return;
            }
            var builder = new System.Text.StringBuilder();
            for (int i = 0; i < RaftBoosts.KindCount; i++)
            {
                var kind = (PickupKind)i;
                if (!boosts.IsActive(kind) || boosts.Duration(kind) <= 0f)
                {
                    continue;
                }
                if (builder.Length > 0)
                {
                    builder.Append("     ");
                }
                builder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(Pickup.ColorOf(kind))).Append('>')
                    .Append(Pickup.NameOf(kind)).Append(' ').Append(Mathf.CeilToInt(boosts.Remaining(kind))).Append(" sn");
                if (kind == PickupKind.Egg)
                {
                    builder.Append(" · zırh %").Append(Mathf.RoundToInt(boosts.ShieldNormalized * 100f));
                }
                builder.Append("</color>");
            }
            boostText.text = builder.ToString();
        }

        void UpdateMessage()
        {
            if (messageText != null && messageText.gameObject.activeSelf && Time.unscaledTime >= messageUntil)
            {
                messageText.gameObject.SetActive(false);
            }
            if (IceZone.Active.Count > 0 && course != null && course.raft != null)
            {
                bool onIce = IceZone.Amount(course.raft.transform.position) > 0.5f;
                if (onIce && !wasOnIce && Time.unscaledTime >= nextIceNotice)
                {
                    ShowMessage("Buzlanma! Sal kayıyor!", new Color(0.7f, 0.92f, 1f));
                    nextIceNotice = Time.unscaledTime + 12f;
                }
                wasOnIce = onIce;
            }
        }

        bool wasOnIce;
        float nextIceNotice;

        void ShowMessage(string text, Color color)
        {
            if (messageText == null)
            {
                return;
            }
            messageText.text = text;
            messageText.color = color;
            messageText.gameObject.SetActive(true);
            messageUntil = Time.unscaledTime + 1.8f;
        }

        void OnBroken()
        {
            ShowMessage("Sal parçalandı!", new Color(1f, 0.55f, 0.4f));
        }

        void OnCaught()
        {
            CourseTheme theme = CourseTheme.Active;
            ShowMessage(theme != null && !string.IsNullOrEmpty(theme.caughtMessage) ? theme.caughtMessage : "Timsaha yakalandı!", new Color(0.6f, 1f, 0.5f));
        }

        void OnShocked()
        {
            ShowMessage("Elektrik şoku! 2 saniye gaz yok!", new Color(1f, 0.95f, 0.4f));
        }

        void OnShieldSaved()
        {
            ShowMessage("Yumurta zırhı seni kurtardı!", new Color(0.95f, 0.95f, 0.8f));
        }

        void OnQuestCompleted(Quest quest)
        {
            ShowMessage("Görev tamam: " + DailyQuests.Describe(quest) + "!", new Color(1f, 0.84f, 0.3f));
            Haptics.Medium();
        }

        void OnCollected(PickupKind kind)
        {
            Haptics.Light();
            string effect;
            switch (kind)
            {
                case PickupKind.Beans:
                    effect = "Gaz deposu ×1,5";
                    break;
                case PickupKind.Pastirma:
                    effect = "Osuruk gücü ×1,5";
                    break;
                case PickupKind.Onion:
                    effect = "Sınırsız gaz!";
                    break;
                case PickupKind.Chickpea:
                    effect = "Herkesin gazı doldu!";
                    break;
                case PickupKind.Pepper:
                    effect = "Turbo ×2!";
                    break;
                case PickupKind.Repair:
                    effect = "Sal tamir edildi!";
                    break;
                case PickupKind.Bubble:
                    effect = "6 saniye hiçbir şey sana zarar veremez!";
                    break;
                case PickupKind.Magnet:
                    effect = "Yakındaki eklentiler sana geliyor!";
                    break;
                default:
                    effect = "Yumurta zırhı açıldı!";
                    break;
            }
            ShowMessage(Pickup.NameOf(kind) + "!  " + effect, Pickup.ColorOf(kind));
        }

        void BuildFinishPanel()
        {
            RectTransform panel = UiKit.CreatePanel(canvasRect, "FinishPanel", new Vector2(1760f, 1000f));
            finishPanel = panel.gameObject;

            finishTitle = UiKit.CreateText(panel, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(1700f, 76f), 58, TextAnchor.UpperCenter);
            UiKit.AddOutline(finishTitle, 2f);
            finishTime = UiKit.CreateText(panel, "Time", new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(1700f, 46f), 32, TextAnchor.UpperCenter);
            finishRows = UiKit.CreateGroup(panel, "Rows");
            finishRows.anchorMin = new Vector2(0f, 1f);
            finishRows.anchorMax = new Vector2(1f, 1f);
            finishRows.pivot = new Vector2(0.5f, 1f);
            finishRows.offsetMin = new Vector2(40f, -720f);
            finishRows.offsetMax = new Vector2(-40f, -160f);
            finishBest = UiKit.CreateText(panel, "Best", new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(1700f, 50f), 36, TextAnchor.LowerCenter);
            finishBest.color = UiKit.StarOnColor;

            Vector2 bottom = new Vector2(0.5f, 0f);
            Vector2 buttonSize = new Vector2(250f, 100f);
            finishMenuButton = CreateCenteredButton(panel, "Menü", bottom, new Vector2(-280f, 30f), buttonSize, GoToMenu);
            retryButton = CreateCenteredButton(panel, "Tekrar", bottom, new Vector2(0f, 30f), buttonSize, Retry);
            nextButton = CreateCenteredButton(panel, "Sonraki", bottom, new Vector2(280f, 30f), buttonSize, () => bootstrap.LoadNextLevel());

            finishPanel.SetActive(false);
        }

        void FillScoreTable(ScoreReport report)
        {
            for (int i = finishRows.childCount - 1; i >= 0; i--)
            {
                Destroy(finishRows.GetChild(i).gameObject);
            }
            string[] headers = { "Oyuncu", "Süre", "Güvenlik", "Gaz", "Bonus", "Puan" };
            float[] columns = { 0f, 700f, 860f, 1050f, 1200f, 1380f };
            float[] widths = { 680f, 150f, 180f, 140f, 160f, 280f };
            for (int c = 0; c < headers.Length; c++)
            {
                Text header = UiKit.CreateText(finishRows, headers[c], new Vector2(0f, 1f), new Vector2(columns[c], 0f), new Vector2(widths[c], 50f), 30, c == 0 ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter);
                header.color = new Color(1f, 1f, 1f, 0.6f);
            }
            float rowHeight = Mathf.Min(130f, 500f / Mathf.Max(1, report.players.Count));
            for (int r = 0; r < report.players.Count; r++)
            {
                PlayerScore player = report.players[r];
                float y = -60f - r * rowHeight;
                Text name = UiKit.CreateText(finishRows, "Name" + r, new Vector2(0f, 1f), new Vector2(columns[0], y), new Vector2(widths[0], 52f), 36, TextAnchor.MiddleLeft);
                name.text = player.bot ? player.name + "  <color=#9fb4c8>BOT</color>" : player.name;
                name.supportRichText = true;
                string[] values = { player.time.ToString("0.0"), player.safety.ToString("0.0"), player.gas.ToString("0.0"), player.bonus.ToString("0.0"), player.total.ToString() };
                for (int c = 0; c < values.Length; c++)
                {
                    Text value = UiKit.CreateText(finishRows, "Value" + r + c, new Vector2(0f, 1f), new Vector2(columns[c + 1], y), new Vector2(widths[c + 1], 52f), c == values.Length - 1 ? 46 : 34, TextAnchor.MiddleCenter);
                    value.text = values[c];
                    if (c == values.Length - 1)
                    {
                        value.color = player.bot ? new Color(1f, 1f, 1f, 0.6f) : UiKit.StarOnColor;
                    }
                }
                if (player.badges.Count > 0)
                {
                    Text badges = UiKit.CreateText(finishRows, "Badges" + r, new Vector2(0f, 1f), new Vector2(columns[0] + 24f, y - 48f), new Vector2(1600f, 44f), 27, TextAnchor.MiddleLeft);
                    badges.supportRichText = true;
                    var parts = new List<string>();
                    foreach (Badge badge in player.badges)
                    {
                        parts.Add($"<color={(badge.praise ? "#9be37a" : "#ffb35c")}><b>{badge.title}</b></color> <size=22>{badge.description}</size>");
                    }
                    badges.text = string.Join("     ", parts);
                }
            }
        }
        GameObject CreateCenteredButton(RectTransform parent, string label, Vector2 anchor, Vector2 position, Vector2 size, Action onClick)
        {
            Text text = UiKit.CreateButton(parent, label, anchor, position, size, onClick);
            var rect = (RectTransform)text.transform.parent;
            rect.pivot = new Vector2(0.5f, 0f);
            return rect.gameObject;
        }

        void BuildTuningPanel()
        {
            RectTransform panel = UiKit.CreatePanel(canvasRect, "TuningPanel", new Vector2(900f, 640f));
            tuningPanel = panel.gameObject;

            Text title = UiKit.CreateText(panel, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(840f, 60f), 40, TextAnchor.UpperCenter);
            title.text = "Test ayarları";

            if (tuning != null)
            {
                AddSlider(panel, 0, "İtme gücü", 0.3f, 3f, () => tuning.thrustMultiplier, v => tuning.thrustMultiplier = v, "x{0:0.00}");
                AddSlider(panel, 1, "Gaz süresi", 0.3f, 3f, () => tuning.gasMultiplier, v => tuning.gasMultiplier = v, "x{0:0.00}");
                AddSlider(panel, 2, "Su sürtünmesi", 0f, 3f, () => tuning.waterLinearDrag, v => tuning.waterLinearDrag = v, "{0:0.00}");
                AddSlider(panel, 3, "Dönme sürtünmesi", 0f, 5f, () => tuning.waterAngularDrag, v => tuning.waterAngularDrag = v, "{0:0.00}");
                AddSlider(panel, 4, "Yüzdürme", 1.2f, 5f, () => tuning.buoyancyStrength, v => tuning.buoyancyStrength = v, "{0:0.00}");
            }

            CreateCenteredButton(panel, "Kapat", new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(260f, 80f), () => tuningPanel.SetActive(false));
            tuningPanel.SetActive(false);
        }

        void AddSlider(RectTransform panel, int row, string label, float min, float max, Func<float> getter, Action<float> setter, string format)
        {
            float y = -110f - row * 86f;
            Text labelText = UiKit.CreateText(panel, label, new Vector2(0f, 1f), new Vector2(40f, y), new Vector2(420f, 60f), 32, TextAnchor.MiddleLeft);

            GameObject sliderObject = DefaultControls.CreateSlider(new DefaultControls.Resources());
            sliderObject.transform.SetParent(panel, false);
            var rect = (RectTransform)sliderObject.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(470f, y - 30f);
            rect.sizeDelta = new Vector2(380f, 40f);

            Image[] images = sliderObject.GetComponentsInChildren<Image>(true);
            foreach (Image image in images)
            {
                image.sprite = null;
                image.color = image.name == "Fill" ? UiKit.AccentColor : image.name == "Handle" ? Color.white : new Color(1f, 1f, 1f, 0.25f);
                if (image.name == "Handle")
                {
                    image.sprite = UiKit.Circle;
                    image.rectTransform.sizeDelta = new Vector2(40f, 0f);
                }
            }

            var slider = sliderObject.GetComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = getter();
            labelText.text = $"{label}  {string.Format(format, slider.value)}";
            slider.onValueChanged.AddListener(v =>
            {
                setter(v);
                labelText.text = $"{label}  {string.Format(format, v)}";
            });
        }
    }
}
