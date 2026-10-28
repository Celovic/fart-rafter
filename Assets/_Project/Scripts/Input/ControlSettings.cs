using UnityEngine;
using UnityEngine.InputSystem;

namespace FartRafters
{
    public static class ControlSettings
    {
        const string GyroKey = "ctrl.gyro";
        const float DeadZone = 0.08f;

        static bool loaded;
        static bool gyro;
        static Vector3 neutral;
        static bool hasNeutral;

        public static bool Gyro
        {
            get
            {
                if (!loaded)
                {
                    loaded = true;
                    gyro = PlayerPrefs.GetInt(GyroKey, 0) == 1;
                }
                return gyro;
            }
            set
            {
                loaded = true;
                gyro = value;
                PlayerPrefs.SetInt(GyroKey, value ? 1 : 0);
                PlayerPrefs.Save();
                hasNeutral = false;
            }
        }

        public static bool GyroSupported => GravitySensor.current != null || Accelerometer.current != null;

        public static string ModeName => Gyro ? "Jiroskop" : "Joystick";

        static Vector3 Raw()
        {
            GravitySensor gravity = GravitySensor.current;
            if (gravity != null)
            {
                if (!gravity.enabled)
                {
                    InputSystem.EnableDevice(gravity);
                }
                return gravity.gravity.ReadValue();
            }
            Accelerometer accelerometer = Accelerometer.current;
            if (accelerometer != null)
            {
                if (!accelerometer.enabled)
                {
                    InputSystem.EnableDevice(accelerometer);
                }
                return accelerometer.acceleration.ReadValue();
            }
            return Vector3.zero;
        }

        static Vector3 ScreenSpace(Vector3 value)
        {
            if (InputSystem.settings == null || InputSystem.settings.compensateForScreenOrientation)
            {
                return value;
            }
            switch (Screen.orientation)
            {
                case ScreenOrientation.LandscapeLeft:
                    return new Vector3(-value.y, value.x, value.z);
                case ScreenOrientation.LandscapeRight:
                    return new Vector3(value.y, -value.x, value.z);
                case ScreenOrientation.PortraitUpsideDown:
                    return new Vector3(-value.x, -value.y, value.z);
                default:
                    return value;
            }
        }

        public static void Calibrate()
        {
            Vector3 value = Raw();
            if (value.sqrMagnitude > 0.01f)
            {
                neutral = ScreenSpace(value).normalized;
                hasNeutral = true;
            }
        }

        public static bool TryTilt(out Vector2 direction)
        {
            direction = Vector2.zero;
            Vector3 value = Raw();
            if (value.sqrMagnitude < 0.01f)
            {
                return false;
            }
            Vector3 current = ScreenSpace(value).normalized;
            if (!hasNeutral)
            {
                neutral = current;
                hasNeutral = true;
            }
            Vector3 delta = current - neutral;
            var tilt = new Vector2(delta.x, delta.y);
            if (tilt.magnitude < DeadZone)
            {
                return false;
            }
            direction = tilt;
            return true;
        }
    }
}
