using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FartRafters
{
    public class GameShots : MonoBehaviour
    {
        static string folder;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-gameshots")
                {
                    folder = args[i + 1];
                }
            }
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }
            AudioListener.volume = 0f;
            TutorialGuide.ResetAll();
            GameSelection.CurrentMode = GameSelection.Mode.Practice;
            GameSelection.LevelIndex = 0;
            GameSelection.PlayerCount = 1;
            GameSelection.BotCount = 3;
            GameSelection.BiomeChoice = (int)Biome.Fun;
            var go = new GameObject("GameShots");
            DontDestroyOnLoad(go);
            go.AddComponent<GameShots>();
            SceneManager.LoadScene(GameSelection.GameScene);
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(folder);
            yield return new WaitForSecondsRealtime(3f);
            yield return Capture("g01_tutorial");
            for (int i = 0; i < 2; i++)
            {
                Send("Next");
            }
            yield return Capture("g02_gas");
            for (int i = 0; i < 4; i++)
            {
                Send("Next");
            }
            yield return Capture("g03_autopilot");
            Send("Skip");
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Capture("g04_biome");
            Send("Skip");
            yield return new WaitForSecondsRealtime(4f);
            yield return Capture("g05_play");
            FindAnyObjectByType<GameHud>().SendMessage("SetPaused", true);
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Capture("g06_pause");
            Application.Quit();
        }

        static void Send(string message)
        {
            TutorialGuide guide = FindAnyObjectByType<TutorialGuide>();
            if (guide != null)
            {
                guide.SendMessage(message);
            }
        }

        IEnumerator Capture(string name)
        {
            yield return new WaitForSecondsRealtime(0.6f);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(folder, name + ".png"));
            yield return new WaitForSecondsRealtime(0.3f);
        }
    }
}
