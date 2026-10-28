using UnityEngine;

namespace FartRafters
{
    public static class Haptics
    {
        const string Key = "fx.vibe";
        const float MinGap = 0.06f;

        static bool loaded;
        static bool enabled;
        static float lastTime = -1f;
#if UNITY_ANDROID && !UNITY_EDITOR
        static AndroidJavaObject vibrator;
        static AndroidJavaClass effect;
        static int sdk;
        static bool ready;
#endif

        public static bool Enabled
        {
            get
            {
                if (!loaded)
                {
                    loaded = true;
                    enabled = PlayerPrefs.GetInt(Key, 1) == 1;
                }
                return enabled;
            }
            set
            {
                loaded = true;
                enabled = value;
                PlayerPrefs.SetInt(Key, value ? 1 : 0);
                PlayerPrefs.Save();
                if (value)
                {
                    Pulse(40, 160);
                }
            }
        }

        public static void Light()
        {
            Pulse(18, 70);
        }

        public static void Medium()
        {
            Pulse(40, 150);
        }

        public static void Heavy()
        {
            Pulse(90, 255);
        }

        public static void Impact(float strength)
        {
            float t = Mathf.Clamp01(strength);
            Pulse(Mathf.RoundToInt(Mathf.Lerp(25f, 85f, t)), Mathf.RoundToInt(Mathf.Lerp(110f, 255f, t)));
        }

        public static void Break()
        {
            Pattern(new long[] { 0, 90, 60, 140 }, new[] { 0, 255, 0, 200 });
        }

        public static void Celebrate()
        {
            Pattern(new long[] { 0, 30, 70, 30, 70, 60 }, new[] { 0, 120, 0, 160, 0, 220 });
        }

        static void Pulse(int milliseconds, int amplitude)
        {
            if (!Allowed())
            {
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Prepare())
            {
                return;
            }
            try
            {
                if (sdk >= 26)
                {
                    using (AndroidJavaObject shot = effect.CallStatic<AndroidJavaObject>("createOneShot", (long)milliseconds, Mathf.Clamp(amplitude, 1, 255)))
                    {
                        vibrator.Call("vibrate", shot);
                    }
                }
                else
                {
                    vibrator.Call("vibrate", (long)milliseconds);
                }
            }
            catch (System.Exception)
            {
                ready = false;
            }
#endif
        }

        static void Pattern(long[] timings, int[] amplitudes)
        {
            if (!Allowed())
            {
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Prepare())
            {
                return;
            }
            try
            {
                if (sdk >= 26)
                {
                    using (AndroidJavaObject wave = effect.CallStatic<AndroidJavaObject>("createWaveform", timings, amplitudes, -1))
                    {
                        vibrator.Call("vibrate", wave);
                    }
                }
                else
                {
                    vibrator.Call("vibrate", timings, -1);
                }
            }
            catch (System.Exception)
            {
                ready = false;
            }
#endif
        }

        static bool Allowed()
        {
            if (!Enabled || !Application.isMobilePlatform)
            {
                return false;
            }
            float now = Time.unscaledTime;
            if (now - lastTime < MinGap)
            {
                return false;
            }
            lastTime = now;
            return true;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        static bool Prepare()
        {
            if (ready)
            {
                return vibrator != null;
            }
            ready = true;
            try
            {
                using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    sdk = version.GetStatic<int>("SDK_INT");
                }
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }
                if (sdk >= 26)
                {
                    effect = new AndroidJavaClass("android.os.VibrationEffect");
                }
                if (vibrator != null && !vibrator.Call<bool>("hasVibrator"))
                {
                    vibrator = null;
                }
            }
            catch (System.Exception)
            {
                vibrator = null;
            }
            return vibrator != null;
        }
#endif

        public static void FallbackBuzz()
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
