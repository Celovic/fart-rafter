using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FartRafters
{
    public partial class MainMenu
    {
        static string shotFolder;
        static bool tutorialShots;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ReadShotArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-menushots")
                {
                    shotFolder = args[i + 1];
                    AudioListener.volume = 0f;
                }
                if (args[i] == "-tutorialshots")
                {
                    shotFolder = args[i + 1];
                    tutorialShots = true;
                    AudioListener.volume = 0f;
                }
            }
        }

        void OnEnable()
        {
            if (!string.IsNullOrEmpty(shotFolder))
            {
                StartCoroutine(tutorialShots ? CaptureTutorials() : CaptureScreens());
            }
        }

        IEnumerator CaptureTutorials()
        {
            Directory.CreateDirectory(shotFolder);
            yield return new WaitForSecondsRealtime(2f);
            PlayerProfile.SetBirthYear(1990);
            PlayerProfile.SetName("Gaz Kaptanı");
            PlayerProfile.MarkReminderShown();
            TutorialGuide.ResetAll();
            yield return Shot("t01_home", ShowHome);
            yield return Shot("t02_home_next", () => FindAnyObjectByType<TutorialGuide>().SendMessage("Next"));
            yield return Shot("t03_home_skip", () => FindAnyObjectByType<TutorialGuide>().SendMessage("Skip"));
            yield return Shot("t04_modes", ShowModes);
            FindAnyObjectByType<TutorialGuide>().SendMessage("Skip");
            yield return Shot("t05_setup", () => SelectLevel(0));
            for (int i = 0; i < 5; i++)
            {
                FindAnyObjectByType<TutorialGuide>().SendMessage("Next");
            }
            yield return Shot("t06_setup_raft", () => { });
            FindAnyObjectByType<TutorialGuide>().SendMessage("Skip");
            yield return Shot("t07_setup_done", () => { });
            yield return Shot("t08_costume_raft", () => { ChangeAvatar(0, 1); CycleCostume(0); CycleCostume(0); CycleRaft(); CycleRaft(); CycleRaft(); CycleRaft(); });
            yield return Shot("t09_two_players", () => { CyclePlayers(); });
            DailyQuests.Pickup();
            DailyQuests.Checkpoint();
            yield return Shot("t10_quests", ShowQuests);
            yield return Shot("t11_home", ShowHome);
            Application.Quit();
        }

        IEnumerator CaptureScreens()
        {
            Directory.CreateDirectory(shotFolder);
            yield return new WaitForSecondsRealtime(2f);
            PlayerProfile.SetBirthYear(1990);
            yield return Shot("01_age", () => ShowAge(true));
            yield return Shot("02_name_adult", () => ShowName(true));
            PlayerProfile.SetName("Gaz Kaptanı");
            yield return Shot("03_home", ShowHome);
            yield return Shot("04_settings", ShowSettings);
            yield return Shot("05_reminder", () => ShowReminder(ShowHome));
            PlayerProfile.MarkReminderShown();
            yield return Shot("06_online", ShowOnline);
            onlineDirect = true;
            yield return Shot("07_join_direct", ShowJoin);
            onlineDirect = false;
            yield return Shot("08_join_code", ShowJoin);
            yield return Shot("09_friends", ShowFriends);
            yield return Shot("10_leaderboard", ShowLeaderboard);
            yield return Shot("11_privacy", () => Show(privacyScreen));
            onlineDirect = true;
            HostRoom(new RoomState { adventure = true, difficulty = 2, bots = 1, botLevel = 3, biome = 1, raft = 4 });
            yield return new WaitForSecondsRealtime(1.5f);
            yield return Shot("12_lobby", ShowLobby);
            OnEmoji(new EmojiInfo { clientId = 99, name = "Fasulye Fırtınası", emoji = 1 });
            yield return Shot("13_lobby_emoji", RefreshLobby);
            yield return Shot("13_lobby_quick", () => quickPanel.gameObject.SetActive(true));
            if (OnlineRoom.Current != null)
            {
                OnlineRoom.Current.Leave();
            }
            yield return new WaitForSecondsRealtime(1f);
            PlayerProfile.SetBirthYear(DateTime.Now.Year - 9);
            yield return Shot("14_name_child", () => ShowName(true));
            yield return Shot("15_friends_child", ShowFriends);
            gateAnswer = 0;
            OpenParental();
            yield return Shot("16_gate", () => { });
            ShowParental();
            yield return Shot("17_parental", () => { });
            Toast("Ebeveyn ayarları kaydedildi.");
            yield return Shot("18_toast", ShowHome);
            Application.Quit();
        }

        IEnumerator Shot(string name, Action show)
        {
            show();
            yield return null;
            yield return new WaitForSecondsRealtime(0.4f);
            ScreenCapture.CaptureScreenshot(Path.Combine(shotFolder, name + ".png"));
            yield return null;
            yield return null;
        }
    }
}
