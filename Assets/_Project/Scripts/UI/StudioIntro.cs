using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace FartRafters
{
    public class StudioIntro : MonoBehaviour
    {
        const string ClipPath = "Intro/TwoHearts";
        static readonly Color Backdrop = new Color(0.086f, 0.09f, 0.235f, 1f);
        static readonly string[] SkipArgs = { "-botsim", "-tutorialshots", "-menushots", "-gameshots", "-qalog", "-peer", "-nettest" };

        public static bool Playing { get; private set; }

        VideoPlayer player;
        RenderTexture target;
        CanvasGroup group;
        float started;
        float fadeStart = -1f;
        bool prepared;
        static string shotFolder;
        int shotIndex;
        Canvas own;
        readonly List<Canvas> hidden = new List<Canvas>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-introshots")
                {
                    shotFolder = args[i + 1];
                }
            }
            foreach (string arg in args)
            {
                if (Array.IndexOf(SkipArgs, arg) >= 0)
                {
                    return;
                }
            }
            VideoClip clip = Resources.Load<VideoClip>(ClipPath);
            if (clip == null)
            {
                return;
            }
            var go = new GameObject("StudioIntro");
            DontDestroyOnLoad(go);
            go.AddComponent<StudioIntro>().Begin(clip);
        }

        void Begin(VideoClip clip)
        {
            Playing = true;
            var canvasObject = new GameObject("IntroCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            own = canvas;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            group = canvasObject.GetComponent<CanvasGroup>();

            var back = new GameObject("Backdrop", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            back.transform.SetParent(canvasObject.transform, false);
            back.color = Backdrop;
            Stretch(back.rectTransform);

            target = new RenderTexture((int)clip.width, (int)clip.height, 0);
            var view = new GameObject("Video", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter)).GetComponent<RawImage>();
            view.transform.SetParent(canvasObject.transform, false);
            view.texture = target;
            view.color = new Color(1f, 1f, 1f, 0f);
            Stretch(view.rectTransform);
            var fitter = view.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = (float)clip.width / clip.height;

            player = gameObject.AddComponent<VideoPlayer>();
            player.playOnAwake = false;
            player.source = VideoSource.VideoClip;
            player.clip = clip;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.targetTexture = target;
            player.isLooping = false;
            var voice = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.ignoreListenerPause = true;
            voice.ignoreListenerVolume = true;
            voice.spatialBlend = 0f;
            voice.mute = PlayerPrefs.GetInt("audio.sfx", 1) == 0;
            player.audioOutputMode = VideoAudioOutputMode.AudioSource;
            player.EnableAudioTrack(0, true);
            player.SetTargetAudioSource(0, voice);
            AudioListener.pause = true;
            player.prepareCompleted += source =>
            {
                prepared = true;
                view.color = Color.white;
                source.Play();
            };
            player.loopPointReached += source => Finish();
            player.errorReceived += (source, message) => Finish();
            player.Prepare();
            started = Time.unscaledTime;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        void HideOthers()
        {
            foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (canvas != own && canvas.isRootCanvas && canvas.enabled)
                {
                    canvas.enabled = false;
                    hidden.Add(canvas);
                }
            }
        }

        void RestoreOthers()
        {
            foreach (Canvas canvas in hidden)
            {
                if (canvas != null)
                {
                    canvas.enabled = true;
                }
            }
            hidden.Clear();
        }

        void LateUpdate()
        {
            if (fadeStart < 0f)
            {
                HideOthers();
            }
        }

        void Update()
        {
            if (fadeStart >= 0f)
            {
                float t = (Time.unscaledTime - fadeStart) / 0.35f;
                group.alpha = 1f - Mathf.Clamp01(t);
                if (t >= 1f)
                {
                    Destroy(gameObject);
                }
                return;
            }
            float elapsed = Time.unscaledTime - started;
            if (shotFolder != null && prepared && shotIndex < 4 && player.time >= 0.6f + shotIndex * 0.8f)
            {
                System.IO.Directory.CreateDirectory(shotFolder);
                ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(shotFolder, "intro_" + shotIndex + ".png"));
                shotIndex++;
            }
            if ((!prepared && elapsed > 10f) || elapsed > 15f)
            {
                Finish();
            }
        }

        void Finish()
        {
            if (fadeStart >= 0f)
            {
                return;
            }
            fadeStart = Time.unscaledTime;
            RestoreOthers();
            AudioListener.pause = false;
            if (player != null)
            {
                player.Stop();
            }
        }

        void OnDestroy()
        {
            Playing = false;
            RestoreOthers();
            AudioListener.pause = false;
            if (target != null)
            {
                target.Release();
                Destroy(target);
            }
        }
    }
}
