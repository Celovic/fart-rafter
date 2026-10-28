using System;
using UnityEngine;
using UnityEngine.UI;

namespace FartRafters
{
    public partial class MainMenu
    {
        static readonly string[] Ones = { "", "bir", "iki", "üç", "dört", "beş", "altı", "yedi", "sekiz", "dokuz" };
        static readonly string[] Tens = { "", "on", "yirmi", "otuz", "kırk", "elli", "altmış", "yetmiş", "seksen", "doksan" };

        RectTransform ageScreen;
        RectTransform nameScreen;
        RectTransform settingsScreen;
        RectTransform gateScreen;
        RectTransform parentalScreen;
        RectTransform privacyScreen;
        RectTransform reminderScreen;
        RectTransform confirmScreen;

        Text ageDisplay;
        string ageDigits = "";
        Text ageMessage;
        bool ageFirstRun;

        Text childNameText;
        InputField nameInput;
        Text nameMessage;
        Text nameHint;
        RectTransform childNameGroup;
        RectTransform adultNameGroup;
        string pendingChildName;
        bool nameFirstRun;
        readonly System.Random nameRandom = new System.Random();

        Text settingsProfile;
        Text settingsTag;
        Text graphicsText;
        Text fpsText;
        Text steeringText;
        Text vibrateText;
        Text gateQuestion;
        Text gateDisplay;
        string gateDigits = "";
        int gateAnswer;
        Text[] parentalLabels;
        bool[] parentalValues;
        Text confirmText;
        Action confirmAction;
        Action reminderDone;

        void BuildProfileScreens()
        {
            ageScreen = Register(BuildAge());
            nameScreen = Register(BuildName());
            settingsScreen = Register(BuildSettings());
            gateScreen = Register(BuildGate());
            parentalScreen = Register(BuildParental());
            privacyScreen = Register(BuildPrivacy());
            reminderScreen = Register(BuildReminder());
            confirmScreen = Register(BuildConfirm());
        }

        RectTransform BuildAge()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Age");
            UiKit.CreateImage(screen, "Shade", null, new Color(0f, 0f, 0f, 0.45f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4000f, 3000f)).pivot = new Vector2(0.5f, 0.5f);
            Text title = UiKit.CreateCenterText(screen, "Title", new Vector2(0f, 400f), new Vector2(1600f, 100f), 64);
            title.text = "Doğum yılın kaç?";
            UiKit.AddOutline(title, 3f);
            Text info = UiKit.CreateCenterText(screen, "Info", new Vector2(0f, 320f), new Vector2(1600f, 60f), 32);
            info.text = "Oyunu sana uygun ayarlarla açmak için soruyoruz. Bilgi telefonunda kalır.";
            ageDisplay = UiKit.CreateCenterText(screen, "Display", new Vector2(-420f, 120f), new Vector2(520f, 140f), 110);
            ageMessage = UiKit.CreateCenterText(screen, "Message", new Vector2(-420f, -10f), new Vector2(700f, 60f), 30);
            ageMessage.color = new Color(1f, 0.7f, 0.5f);
            BuildKeypad(screen, new Vector2(320f, 120f), digit =>
            {
                if (ageDigits.Length < 4)
                {
                    ageDigits += digit;
                    RefreshAge();
                }
            }, () =>
            {
                if (ageDigits.Length > 0)
                {
                    ageDigits = ageDigits.Substring(0, ageDigits.Length - 1);
                    RefreshAge();
                }
            });
            UiKit.CreateCenterButton(screen, "Devam", new Vector2(-420f, -160f), new Vector2(380f, 120f), ConfirmAge);
            return screen;
        }

        void BuildKeypad(RectTransform screen, Vector2 origin, Action<int> digit, Action delete)
        {
            const float size = 130f;
            const float gap = 18f;
            for (int i = 1; i <= 9; i++)
            {
                int value = i;
                float x = origin.x + ((i - 1) % 3 - 1) * (size + gap);
                float y = origin.y + 120f - (i - 1) / 3 * (size + gap);
                Text key = UiKit.CreateCenterButton(screen, value.ToString(), new Vector2(x, y), new Vector2(size, size), () => digit(value));
                key.fontSize = 56;
            }
            Text zero = UiKit.CreateCenterButton(screen, "0", new Vector2(origin.x, origin.y + 120f - 3 * (size + gap)), new Vector2(size, size), () => digit(0));
            zero.fontSize = 56;
            Text back = UiKit.CreateCenterButton(screen, "Sil", new Vector2(origin.x + size + gap, origin.y + 120f - 3 * (size + gap)), new Vector2(size, size), delete);
            back.fontSize = 40;
        }

        void ShowAge(bool firstRun)
        {
            ageFirstRun = firstRun;
            ageDigits = "";
            RefreshAge();
            Show(ageScreen);
        }

        void RefreshAge()
        {
            ageDisplay.text = string.Join(" ", ageDigits.PadRight(4, '_').ToCharArray());
            ageMessage.text = "";
        }

        void ConfirmAge()
        {
            int now = DateTime.Now.Year;
            if (ageDigits.Length != 4 || !int.TryParse(ageDigits, out int year) || year < now - 110 || year > now)
            {
                ageMessage.text = "Geçerli bir yıl yaz (ör. 2010).";
                return;
            }
            PlayerProfile.SetBirthYear(year);
            if (ageFirstRun || !PlayerProfile.HasName)
            {
                ShowName(true);
            }
            else
            {
                ShowSettings();
            }
        }

        RectTransform BuildName()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Name");
            UiKit.CreateImage(screen, "Shade", null, new Color(0f, 0f, 0f, 0.45f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4000f, 3000f)).pivot = new Vector2(0.5f, 0.5f);
            Text title = UiKit.CreateCenterText(screen, "Title", new Vector2(0f, 380f), new Vector2(1600f, 100f), 64);
            title.text = "Oyuncu adın";
            UiKit.AddOutline(title, 3f);

            childNameGroup = UiKit.CreateGroup(screen, "Child");
            Text childInfo = UiKit.CreateCenterText(childNameGroup, "Info", new Vector2(0f, 260f), new Vector2(1600f, 60f), 32);
            childInfo.text = "Güvenliğin için adını oyun seçer. Beğenmezsen yenisini iste!";
            childNameText = UiKit.CreateCenterText(childNameGroup, "Generated", new Vector2(0f, 110f), new Vector2(1600f, 130f), 84);
            childNameText.color = UiKit.StarOnColor;
            UiKit.AddOutline(childNameText, 3f);
            UiKit.CreateCenterButton(childNameGroup, "Başka isim", new Vector2(-230f, -60f), new Vector2(400f, 110f), () =>
            {
                pendingChildName = NameGenerator.ChildName(nameRandom);
                childNameText.text = pendingChildName;
            });
            UiKit.CreateCenterButton(childNameGroup, "Bunu seçtim", new Vector2(230f, -60f), new Vector2(400f, 110f), () => SaveName(pendingChildName));

            adultNameGroup = UiKit.CreateGroup(screen, "Adult");
            Text adultInfo = UiKit.CreateCenterText(adultNameGroup, "Info", new Vector2(0f, 260f), new Vector2(1600f, 60f), 32);
            adultInfo.text = "Gerçek adını değil, komik bir oyuncu adı kullan. (3-20 harf)";
            nameInput = UiKit.CreateInput(adultNameGroup, "Oyuncu adın", new Vector2(0f, 130f), new Vector2(900f, 120f), 56, ProfanityFilter.MaxLength);
            UiKit.CreateCenterButton(adultNameGroup, "Zar At", new Vector2(-230f, -30f), new Vector2(400f, 110f), () =>
            {
                nameInput.text = NameGenerator.FunnyName(nameRandom);
            });
            UiKit.CreateCenterButton(adultNameGroup, "Kaydet", new Vector2(230f, -30f), new Vector2(400f, 110f), () => SaveName(nameInput.text));
            nameMessage = UiKit.CreateCenterText(screen, "Message", new Vector2(0f, -170f), new Vector2(1500f, 60f), 34);
            nameMessage.color = new Color(1f, 0.7f, 0.5f);
            nameHint = UiKit.CreateCenterText(screen, "Hint", new Vector2(0f, -250f), new Vector2(1500f, 60f), 28);
            UiKit.CreateButton(screen, "Geri", new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(200f, 90f), () =>
            {
                if (PlayerProfile.HasName)
                {
                    ShowSettings();
                }
            });
            return screen;
        }

        void ShowName(bool firstRun)
        {
            nameFirstRun = firstRun;
            bool child = PlayerProfile.IsChild;
            childNameGroup.gameObject.SetActive(child);
            adultNameGroup.gameObject.SetActive(!child);
            nameMessage.text = "";
            nameHint.text = child ? "" : "Örnek: Fasulye Fırtınası, Gaz Kaptanı, Pastırma Pilotu";
            if (child)
            {
                pendingChildName = PlayerProfile.HasName && NameGenerator.IsGenerated(PlayerProfile.DisplayName) ? PlayerProfile.DisplayName : NameGenerator.ChildName(nameRandom);
                childNameText.text = pendingChildName;
            }
            else
            {
                nameInput.text = PlayerProfile.HasName ? PlayerProfile.DisplayName : "";
            }
            Show(nameScreen);
        }

        void SaveName(string value)
        {
            string name = (value ?? "").Trim();
            if (!PlayerProfile.IsChild)
            {
                ProfanityFilter.Result result = ProfanityFilter.Check(name);
                if (result != ProfanityFilter.Result.Ok)
                {
                    nameMessage.text = ProfanityFilter.Message(result);
                    return;
                }
            }
            bool first = !PlayerProfile.HasName;
            PlayerProfile.SetName(name);
            if (OnlineServices.Ready)
            {
                _ = OnlineServices.SyncName();
            }
            if (first || nameFirstRun)
            {
                StartServices();
                ShowHome();
            }
            else
            {
                ShowSettings();
            }
        }

        RectTransform BuildSettings()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Settings");
            Text title = UiKit.CreateText(screen, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1200f, 100f), 72, TextAnchor.UpperCenter);
            title.text = "Ayarlar";
            UiKit.AddOutline(title, 3f);
            UiKit.CreateButton(screen, "Geri", new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(200f, 90f), ShowHome);
            settingsProfile = UiKit.CreateCenterText(screen, "Profile", new Vector2(0f, 290f), new Vector2(1600f, 70f), 44);
            settingsTag = UiKit.CreateCenterText(screen, "Tag", new Vector2(0f, 225f), new Vector2(1600f, 50f), 30);
            UiKit.CreateCenterButton(screen, "İsmimi değiştir", new Vector2(-300f, 130f), new Vector2(520f, 100f), () => ShowName(false));
            UiKit.CreateCenterButton(screen, "Doğum yılı", new Vector2(300f, 130f), new Vector2(520f, 100f), () => ShowAge(false));
            UiKit.CreateCenterButton(screen, "Ebeveyn Kontrolü", new Vector2(-300f, 15f), new Vector2(520f, 100f), OpenParental);
            UiKit.CreateCenterButton(screen, "Gizlilik Politikası", new Vector2(300f, 15f), new Vector2(520f, 100f), OpenPrivacy);
            UiKit.CreateCenterButton(screen, "Güvenlik Bilgisi", new Vector2(-300f, -100f), new Vector2(520f, 100f), () => ShowReminder(ShowSettings));
            Text delete = UiKit.CreateCenterButton(screen, "Hesabı Sil", new Vector2(300f, -100f), new Vector2(520f, 100f), () => Confirm(
                "Hesabın, puanların ve arkadaş listen kalıcı olarak silinecek. Emin misin?",
                DeleteAccount));
            ((RectTransform)delete.transform.parent).GetComponent<Image>().color = new Color(0.95f, 0.45f, 0.4f);
            graphicsText = UiKit.CreateCenterButton(screen, "", new Vector2(-300f, -215f), new Vector2(520f, 100f), () =>
            {
                GraphicsQuality.CycleMode();
                RefreshGraphics();
            });
            fpsText = UiKit.CreateCenterButton(screen, "", new Vector2(300f, -215f), new Vector2(520f, 100f), () =>
            {
                GraphicsQuality.SetTargetFps(GraphicsQuality.TargetFps == 60 ? 30 : 60);
                RefreshGraphics();
            });
            steeringText = UiKit.CreateCenterButton(screen, "", new Vector2(-300f, -330f), new Vector2(520f, 100f), () =>
            {
                if (!ControlSettings.Gyro && !ControlSettings.GyroSupported)
                {
                    Toast("Bu cihazda eğim sensörü yok, joystick kullanılıyor.");
                    return;
                }
                ControlSettings.Gyro = !ControlSettings.Gyro;
                if (ControlSettings.Gyro)
                {
                    Toast("Jiroskop açık: telefonu eğerek yön ver, gaz düğmesi aynı. Tek oyunculu oyunlarda çalışır.");
                }
                RefreshGraphics();
            });
            vibrateText = UiKit.CreateCenterButton(screen, "", new Vector2(0f, -445f), new Vector2(520f, 100f), () =>
            {
                Haptics.Enabled = !Haptics.Enabled;
                RefreshGraphics();
            });
            UiKit.CreateCenterButton(screen, "Öğreticileri Sıfırla", new Vector2(300f, -330f), new Vector2(520f, 100f), () =>
            {
                TutorialGuide.ResetAll();
                Toast("Öğreticiler tekrar gösterilecek.");
            });
            return screen;
        }

        void RefreshGraphics()
        {
            if (graphicsText != null)
            {
                graphicsText.text = "Grafik: " + GraphicsQuality.ModeName;
                fpsText.text = "FPS: " + GraphicsQuality.TargetFps;
                steeringText.text = "Yönlendirme: " + ControlSettings.ModeName;
                vibrateText.text = "Titreşim: " + (Haptics.Enabled ? "Açık" : "Kapalı");
            }
        }

        void ShowSettings()
        {
            settingsProfile.text = PlayerProfile.DisplayName + (PlayerProfile.IsChild ? "   ·   Çocuk modu" : "");
            string tag = OnlineServices.ServiceName;
            settingsTag.text = string.IsNullOrEmpty(tag) ? (OnlineServices.Ready ? "" : OnlineServices.Message) : "Arkadaşların seni şu adla bulur: " + NameGenerator.FromServiceName(tag) + tag.Substring(Mathf.Max(0, tag.LastIndexOf('#')));
            RefreshGraphics();
            Show(settingsScreen);
        }

        async void DeleteAccount()
        {
            Toast("Hesap siliniyor…");
            bool ok = await OnlineServices.DeleteAccount();
            if (!ok)
            {
                Toast("Hesap silinemedi. İnternet bağlantını kontrol edip tekrar dene.");
                ShowSettings();
                return;
            }
            Toast("Hesabın silindi.");
            ShowAge(true);
        }

        void OpenParental()
        {
            if (!PlayerProfile.IsChild)
            {
                Toast("13 yaş ve üstü hesaplarda tüm özellikler açık.");
                return;
            }
            gateDigits = "";
            gateAnswer = UnityEngine.Random.Range(120, 990);
            gateQuestion.text = "Bu bölüm yetişkinler içindir.\nŞu sayıyı rakamla yazın: " + NumberWords(gateAnswer);
            gateDisplay.text = "_ _ _";
            Show(gateScreen);
        }

        static string NumberWords(int value)
        {
            int hundreds = value / 100;
            int tens = value / 10 % 10;
            int ones = value % 10;
            string words = hundreds > 0 ? (hundreds == 1 ? "yüz" : Ones[hundreds] + " yüz") : "";
            if (tens > 0)
            {
                words += " " + Tens[tens];
            }
            if (ones > 0)
            {
                words += " " + Ones[ones];
            }
            return words.Trim();
        }

        RectTransform BuildGate()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Gate");
            UiKit.CreateImage(screen, "Shade", null, new Color(0f, 0f, 0f, 0.55f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4000f, 3000f)).pivot = new Vector2(0.5f, 0.5f);
            UiKit.CreateButton(screen, "Geri", new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(200f, 90f), ShowSettings);
            gateQuestion = UiKit.CreateCenterText(screen, "Question", new Vector2(-420f, 260f), new Vector2(900f, 200f), 40);
            gateDisplay = UiKit.CreateCenterText(screen, "Display", new Vector2(-420f, 60f), new Vector2(500f, 140f), 100);
            BuildKeypad(screen, new Vector2(320f, 120f), digit =>
            {
                if (gateDigits.Length < 3)
                {
                    gateDigits += digit;
                    gateDisplay.text = string.Join(" ", gateDigits.PadRight(3, '_').ToCharArray());
                }
            }, () =>
            {
                if (gateDigits.Length > 0)
                {
                    gateDigits = gateDigits.Substring(0, gateDigits.Length - 1);
                    gateDisplay.text = string.Join(" ", gateDigits.PadRight(3, '_').ToCharArray());
                }
            });
            UiKit.CreateCenterButton(screen, "Onayla", new Vector2(-420f, -120f), new Vector2(380f, 120f), () =>
            {
                if (int.TryParse(gateDigits, out int value) && value == gateAnswer)
                {
                    ShowParental();
                }
                else
                {
                    ShowSettings();
                    Toast("Doğrulama başarısız.");
                }
            });
            return screen;
        }

        RectTransform BuildParental()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Parental");
            Text title = UiKit.CreateText(screen, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1400f, 100f), 64, TextAnchor.UpperCenter);
            title.text = "Ebeveyn Kontrolü";
            UiKit.AddOutline(title, 3f);
            Text info = UiKit.CreateCenterText(screen, "Info", new Vector2(0f, 330f), new Vector2(1600f, 60f), 30);
            info.text = "Oyunda serbest sohbet yoktur. Aşağıdaki özellikleri çocuğunuz için açıp kapatabilirsiniz.";
            string[] labels = { "Online oyun (kodla oda)", "Arkadaş ekleme", "Arkadaşı odaya davet", "Global puan listesinde görünme" };
            parentalLabels = new Text[labels.Length];
            parentalValues = new bool[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                int index = i;
                parentalLabels[i] = UiKit.CreateCenterButton(screen, labels[i], new Vector2(0f, 200f - i * 130f), new Vector2(1000f, 110f), () =>
                {
                    parentalValues[index] = !parentalValues[index];
                    RefreshParental();
                });
                parentalLabels[i].name = labels[i];
            }
            UiKit.CreateCenterButton(screen, "Kaydet", new Vector2(0f, -360f), new Vector2(400f, 110f), () =>
            {
                PlayerProfile.SetParental(parentalValues[1], parentalValues[2], parentalValues[3], parentalValues[0]);
                if (parentalValues[3])
                {
                    ScoreSync.Flush();
                }
                ShowSettings();
                Toast("Ebeveyn ayarları kaydedildi.");
            });
            return screen;
        }

        void ShowParental()
        {
            parentalValues[0] = PlayerProfile.ParentOnline;
            parentalValues[1] = PlayerProfile.ParentFriends;
            parentalValues[2] = PlayerProfile.ParentInvites;
            parentalValues[3] = PlayerProfile.ParentListed;
            RefreshParental();
            Show(parentalScreen);
        }

        void RefreshParental()
        {
            for (int i = 0; i < parentalLabels.Length; i++)
            {
                string label = parentalLabels[i].name;
                parentalLabels[i].text = label + ":  " + (parentalValues[i] ? "AÇIK" : "KAPALI");
            }
        }

        RectTransform BuildPrivacy()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Privacy");
            RectTransform panel = UiKit.CreatePanel(screen, "Panel", new Vector2(1700f, 900f));
            Text title = UiKit.CreateText(panel, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1600f, 80f), 56, TextAnchor.UpperCenter);
            title.text = "Gizlilik Politikası";
            Text body = UiKit.CreateText(panel, "Body", new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(1580f, 620f), 28, TextAnchor.UpperLeft);
            body.text = PrivacyText.Summary;
            UiKit.CreateButton(panel, "Kapat", new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(300f, 100f), ShowSettings).transform.parent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
            return screen;
        }

        void OpenPrivacy()
        {
            string url = OnlineConfig.Instance.privacyPolicyUrl;
            if (!string.IsNullOrEmpty(url) && !PlayerProfile.IsChild)
            {
                Application.OpenURL(url);
                return;
            }
            Show(privacyScreen);
        }

        RectTransform BuildReminder()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Reminder");
            RectTransform panel = UiKit.CreatePanel(screen, "Panel", new Vector2(1500f, 760f));
            Text title = UiKit.CreateText(panel, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1400f, 80f), 56, TextAnchor.UpperCenter);
            title.text = "İnternette güvende kal!";
            Text body = UiKit.CreateText(panel, "Body", new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(1380f, 460f), 34, TextAnchor.UpperLeft);
            body.text = "• Gerçek adını, okulunu, adresini ya da telefonunu kimseyle paylaşma.\n• Oda kodunu yalnızca tanıdığın kişilere ver.\n• Seni rahatsız eden olursa \"Engelle\" ve \"Bildir\" düğmelerini kullan.\n• Ekranın arkasındaki kişiyi her zaman tanımayabilirsin; bir şey seni üzerse bir büyüğüne anlat.";
            UiKit.CreateButton(panel, "Anladım", new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(320f, 110f), () =>
            {
                PlayerProfile.MarkReminderShown();
                Action done = reminderDone;
                reminderDone = null;
                done?.Invoke();
            }).transform.parent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
            return screen;
        }

        void ShowReminder(Action done)
        {
            reminderDone = done;
            Show(reminderScreen);
        }

        bool NeedsReminder(Action then)
        {
            if (PlayerProfile.ReminderShown)
            {
                return false;
            }
            ShowReminder(then);
            return true;
        }

        RectTransform BuildConfirm()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Confirm");
            RectTransform panel = UiKit.CreatePanel(screen, "Panel", new Vector2(1200f, 520f));
            confirmText = UiKit.CreateText(panel, "Body", new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(1100f, 260f), 40, TextAnchor.UpperCenter);
            UiKit.CreateButton(panel, "Vazgeç", new Vector2(0.5f, 0f), new Vector2(-200f, 40f), new Vector2(320f, 110f), () => Show(previousScreen)).transform.parent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
            UiKit.CreateButton(panel, "Evet", new Vector2(0.5f, 0f), new Vector2(200f, 40f), new Vector2(320f, 110f), () =>
            {
                Action action = confirmAction;
                confirmAction = null;
                action?.Invoke();
            }).transform.parent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
            return screen;
        }

        RectTransform previousScreen;

        void Confirm(string message, Action action)
        {
            previousScreen = currentScreen;
            confirmText.text = message;
            confirmAction = action;
            Show(confirmScreen);
        }
    }
}
