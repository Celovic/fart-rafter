using UnityEngine;
using UnityEngine.UI;

namespace FartRafters
{
    public partial class MainMenu
    {
        RectTransform questsScreen;
        Image questsBadge;
        Text questsBadgeText;
        Text loginText;
        Text loginButton;
        Text bonusText;
        Text bonusButton;
        Text[] questLabels;
        Text[] questCounts;
        Text[] questButtons;
        RectTransform[] questFills;
        const float QuestBarWidth = 520f;

        void BuildQuestButton(RectTransform home)
        {
            Text label = UiKit.CreateButton(home, "Görevler", new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(300f, 100f), ShowQuests);
            label.fontSize = 40;
            var rect = (RectTransform)label.transform.parent;
            rect.pivot = new Vector2(1f, 1f);
            UiKit.AddBadge(rect, out questsBadge, out questsBadgeText);
        }

        RectTransform BuildQuests()
        {
            RectTransform screen = UiKit.CreateGroup(canvasRect, "Quests");
            Text title = UiKit.CreateText(screen, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1200f, 100f), 72, TextAnchor.UpperCenter);
            title.text = "Günlük Görevler";
            UiKit.AddOutline(title, 3f);
            UiKit.CreateButton(screen, "Geri", new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(200f, 90f), ShowHome);

            RectTransform login = UiKit.CreatePanel(screen, "Login", new Vector2(1400f, 120f));
            login.anchoredPosition = new Vector2(0f, 255f);
            loginText = UiKit.CreateText(login, "Text", new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(1000f, 90f), 38, TextAnchor.MiddleLeft);
            loginText.rectTransform.pivot = new Vector2(0f, 0.5f);
            loginButton = UiKit.CreateButton(login, "Al", new Vector2(1f, 0.5f), new Vector2(-30f, 0f), new Vector2(240f, 86f), () =>
            {
                if (DailyQuests.ClaimLogin())
                {
                    Rewarded("Giriş ödülü alındı!");
                }
            });
            ((RectTransform)loginButton.transform.parent).pivot = new Vector2(1f, 0.5f);

            questLabels = new Text[DailyQuests.Count];
            questCounts = new Text[DailyQuests.Count];
            questButtons = new Text[DailyQuests.Count];
            questFills = new RectTransform[DailyQuests.Count];
            for (int i = 0; i < DailyQuests.Count; i++)
            {
                int index = i;
                RectTransform row = UiKit.CreatePanel(screen, "Quest" + i, new Vector2(1400f, 130f));
                row.anchoredPosition = new Vector2(0f, 110f - i * 145f);
                questLabels[i] = UiKit.CreateText(row, "Text", new Vector2(0f, 1f), new Vector2(40f, -14f), new Vector2(1000f, 60f), 38, TextAnchor.UpperLeft);
                questLabels[i].rectTransform.pivot = new Vector2(0f, 1f);
                RectTransform back = UiKit.CreateImage(row, "Bar", UiKit.Rounded, new Color(0f, 0f, 0f, 0.35f), new Vector2(0f, 0f), new Vector2(40f, 22f), new Vector2(QuestBarWidth, 30f));
                back.pivot = new Vector2(0f, 0f);
                back.GetComponent<Image>().type = Image.Type.Sliced;
                questFills[i] = UiKit.CreateImage(back, "Fill", UiKit.Rounded, new Color(0.45f, 0.85f, 0.4f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(0f, 30f));
                questFills[i].pivot = new Vector2(0f, 0.5f);
                questFills[i].GetComponent<Image>().type = Image.Type.Sliced;
                questCounts[i] = UiKit.CreateText(row, "Count", new Vector2(0f, 0f), new Vector2(QuestBarWidth + 70f, 18f), new Vector2(360f, 40f), 32, TextAnchor.LowerLeft);
                questCounts[i].rectTransform.pivot = new Vector2(0f, 0f);
                questButtons[i] = UiKit.CreateButton(row, "Al", new Vector2(1f, 0.5f), new Vector2(-30f, 0f), new Vector2(240f, 90f), () =>
                {
                    if (DailyQuests.Claim(index))
                    {
                        Rewarded("Görev ödülü alındı!");
                    }
                });
                ((RectTransform)questButtons[i].transform.parent).pivot = new Vector2(1f, 0.5f);
            }

            RectTransform bonus = UiKit.CreatePanel(screen, "Bonus", new Vector2(1400f, 120f));
            bonus.anchoredPosition = new Vector2(0f, -330f);
            bonusText = UiKit.CreateText(bonus, "Text", new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(1000f, 90f), 36, TextAnchor.MiddleLeft);
            bonusText.rectTransform.pivot = new Vector2(0f, 0.5f);
            bonusButton = UiKit.CreateButton(bonus, "Al", new Vector2(1f, 0.5f), new Vector2(-30f, 0f), new Vector2(240f, 86f), () =>
            {
                if (DailyQuests.ClaimBonus())
                {
                    Rewarded("Büyük ödül alındı!");
                }
            });
            ((RectTransform)bonusButton.transform.parent).pivot = new Vector2(1f, 0.5f);

            Text note = UiKit.CreateText(screen, "Note", new Vector2(0.5f, 0f), new Vector2(0f, 25f), new Vector2(1400f, 40f), 28, TextAnchor.LowerCenter);
            note.text = "Görevler her gece yarısı yenilenir. Ödüller toplam puanına eklenir.";
            return screen;
        }

        void ShowQuests()
        {
            RefreshQuests();
            Show(questsScreen);
        }

        void Rewarded(string message)
        {
            GameAudio.Play(GameAudio.Sfx.Star, 0.8f, 1f);
            Haptics.Medium();
            Toast(message + "  Toplam puan: " + ProgressStore.TotalScore);
            RefreshQuests();
        }

        void RefreshQuests()
        {
            if (questsScreen == null)
            {
                return;
            }
            int streak = DailyQuests.LoginStreak;
            bool loginDone = DailyQuests.LoginClaimed;
            loginText.text = "Günlük giriş · Gün " + streak + "/7   <color=#FFD54F>+" + DailyQuests.LoginReward + " puan</color>";
            SetClaim(loginButton, !loginDone, loginDone ? "Alındı" : "Al");

            Quest[] quests = DailyQuests.Current;
            for (int i = 0; i < quests.Length; i++)
            {
                Quest quest = quests[i];
                questLabels[i].text = DailyQuests.Describe(quest) + "   <color=#FFD54F>+" + quest.reward + "</color>";
                questCounts[i].text = quest.progress + " / " + quest.target;
                questFills[i].sizeDelta = new Vector2(QuestBarWidth * quest.progress / Mathf.Max(1, quest.target), 30f);
                SetClaim(questButtons[i], quest.Done && !quest.claimed, quest.claimed ? "Alındı" : quest.Done ? "Al" : "Devam");
            }

            bool all = DailyQuests.AllDone;
            bonusText.text = "Üç görevi de tamamla   <color=#FFD54F>+" + DailyQuests.AllDoneBonus + " büyük ödül</color>";
            SetClaim(bonusButton, all && !DailyQuests.BonusClaimed, DailyQuests.BonusClaimed ? "Alındı" : all ? "Al" : "Kilitli");
            RefreshQuestBadge();
        }

        static void SetClaim(Text label, bool active, string text)
        {
            label.text = text;
            var button = label.transform.parent.GetComponent<Button>();
            button.interactable = active;
            label.transform.parent.GetComponent<Image>().color = active ? UiKit.ButtonColor : new Color(0.6f, 0.6f, 0.62f, 0.9f);
        }

        void RefreshQuestBadge()
        {
            if (questsBadge == null)
            {
                return;
            }
            int count = DailyQuests.Claimable;
            questsBadge.gameObject.SetActive(count > 0);
            questsBadgeText.text = count.ToString();
        }
    }
}
