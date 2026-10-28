using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace FartRafters
{
    public class PlayerBrain : RiderBrain
    {
        public int playerIndex;
        public Camera viewCamera;
        [Range(0f, 0.5f)] public float topReservedFraction = 0.2f;
        [Range(0f, 0.3f)] public float pauseReservedFraction = 0.13f;
        [Range(0f, 0.2f)] public float deadZoneFraction = 0.03f;
        [Range(0.02f, 0.4f)] public float maxDragFraction = 0.12f;
        [Range(0.02f, 0.4f)] public float compactMaxDragFraction = 0.1f;
        [Range(0f, 0.5f)] public float gasButtonInsetFraction = 0.13f;
        [Range(0f, 0.5f)] public float soloGasButtonInsetFraction = 0.16f;
        [Range(0f, 1f)] public float gasButtonHeightFraction = 0.24f;
        [Range(0f, 0.5f)] public float compactGasButtonHeightFraction = 0.17f;
        [Range(0.02f, 0.3f)] public float gasButtonRadiusFraction = 0.11f;
        [Range(0.02f, 0.3f)] public float compactGasButtonRadiusFraction = 0.085f;
        [Min(1f)] public float gasButtonHitScale = 1.3f;

        int stickTouchId = -1;
        int gasTouchId = -1;
        Vector3 lastAim;

        public static bool InputBlocked { get; set; }
        public static bool TutorialBlocked { get; set; }
        public bool UsesGyro => ControlSettings.Gyro && Solo && ControlSettings.GyroSupported;
        public static int ActivePlayers { get; set; } = 2;

        public bool Compact => ActivePlayers > 2;
        public bool Solo => ActivePlayers <= 1;
        public bool IsLeft => PlayerLayout.IsLeft(playerIndex);
        public bool IsTop => PlayerLayout.IsTop(playerIndex, ActivePlayers);
        public bool JoystickActive { get; private set; }
        public Vector2 JoystickOrigin { get; private set; }
        public Vector2 JoystickCurrent { get; private set; }
        public bool GasHeld { get; private set; }
        public float MaxDragPixels => Screen.height * (Compact ? compactMaxDragFraction : maxDragFraction);
        public float GasButtonRadiusPixels => Screen.height * (Compact ? compactGasButtonRadiusFraction : gasButtonRadiusFraction);

        public Vector2 GasButtonCenter
        {
            get
            {
                float height = Compact ? compactGasButtonHeightFraction : gasButtonHeightFraction;
                float y = IsTop ? 1f - height : height;
                if (Solo)
                {
                    return new Vector2(Screen.width * (1f - soloGasButtonInsetFraction), Screen.height * y);
                }
                float sign = IsLeft ? -1f : 1f;
                return new Vector2(Screen.width * (0.5f + sign * gasButtonInsetFraction), Screen.height * y);
            }
        }

        void OnEnable()
        {
            if (!EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Enable();
            }
            if (UsesGyro)
            {
                ControlSettings.Calibrate();
            }
        }

        void OnDisable()
        {
            stickTouchId = -1;
            gasTouchId = -1;
            JoystickActive = false;
            GasHeld = false;
        }

        public override RiderCommand GetCommand(Rider rider)
        {
            Vector2 drag = Vector2.zero;
            bool stickActive = false;
            bool touchGas = false;

            bool blocked = InputBlocked || TutorialBlocked;
            if (blocked)
            {
                stickTouchId = -1;
                gasTouchId = -1;
            }
            else
            {
                ReadTouches(out drag, out stickActive, out touchGas);
            }
            bool gyro = UsesGyro;
            JoystickActive = stickActive && !gyro;

            bool keyGas = ReadKeys(out Vector2 keyDirection);
            GasHeld = keyGas || touchGas;

            Vector2 direction = keyDirection;
            if (direction == Vector2.zero && gyro && !blocked && ControlSettings.TryTilt(out Vector2 tilt))
            {
                direction = tilt;
            }
            else if (direction == Vector2.zero && !gyro && stickActive && drag.magnitude >= Screen.height * deadZoneFraction)
            {
                direction = drag;
            }
            if (direction != Vector2.zero)
            {
                lastAim = ScreenToWorld(direction.normalized);
            }

            return new RiderCommand(lastAim, GasHeld);
        }

        void ReadTouches(out Vector2 drag, out bool stickActive, out bool gas)
        {
            drag = Vector2.zero;
            bool stickAlive = false;
            bool gasAlive = false;

            foreach (ETouch touch in ETouch.activeTouches)
            {
                bool live = touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
                if (!live)
                {
                    continue;
                }
                if (touch.touchId == stickTouchId)
                {
                    drag = Track(touch.screenPosition);
                    stickAlive = true;
                }
                else if (touch.touchId == gasTouchId)
                {
                    gasAlive = true;
                }
            }

            if (!stickAlive)
            {
                stickTouchId = -1;
            }
            if (!gasAlive)
            {
                gasTouchId = -1;
            }

            foreach (ETouch touch in ETouch.activeTouches)
            {
                if (touch.phase != TouchPhase.Began || touch.touchId == stickTouchId || touch.touchId == gasTouchId)
                {
                    continue;
                }

                Vector2 start = touch.startScreenPosition;
                if (gasTouchId < 0 && IsOnGasButton(start))
                {
                    gasTouchId = touch.touchId;
                    gasAlive = true;
                    continue;
                }
                if (stickTouchId < 0 && IsInZone(start))
                {
                    stickTouchId = touch.touchId;
                    JoystickOrigin = start;
                    drag = Track(touch.screenPosition);
                    stickAlive = true;
                }
            }

            stickActive = stickAlive;
            gas = gasAlive;
        }

        Vector2 Track(Vector2 position)
        {
            Vector2 drag = position - JoystickOrigin;
            float max = MaxDragPixels;
            if (drag.magnitude > max)
            {
                JoystickOrigin = position - drag.normalized * max;
                drag = position - JoystickOrigin;
            }
            JoystickCurrent = position;
            return drag;
        }

        bool IsOnGasButton(Vector2 position)
        {
            float radius = GasButtonRadiusPixels * gasButtonHitScale;
            return (position - GasButtonCenter).sqrMagnitude <= radius * radius;
        }

        bool IsInZone(Vector2 position)
        {
            Vector2 normalized = new Vector2(position.x / Screen.width, position.y / Screen.height);
            if (!PlayerLayout.Zone(playerIndex, ActivePlayers).Contains(normalized))
            {
                return false;
            }
            if (!Compact)
            {
                return normalized.y <= 1f - topReservedFraction;
            }
            bool nearPause = Mathf.Abs(normalized.x - 0.5f) < pauseReservedFraction * 0.5f && normalized.y > 1f - pauseReservedFraction;
            return !nearPause;
        }

        bool ReadKeys(out Vector2 direction)
        {
            direction = Vector2.zero;
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || InputBlocked || TutorialBlocked)
            {
                return false;
            }

            switch (playerIndex)
            {
                case 0:
                    direction = Axis(keyboard.aKey, keyboard.dKey, keyboard.wKey, keyboard.sKey);
                    if (Solo)
                    {
                        direction += Axis(keyboard.leftArrowKey, keyboard.rightArrowKey, keyboard.upArrowKey, keyboard.downArrowKey);
                        return keyboard.spaceKey.isPressed || keyboard.enterKey.isPressed || keyboard.numpadEnterKey.isPressed;
                    }
                    return keyboard.spaceKey.isPressed;
                case 1:
                    direction = Axis(keyboard.leftArrowKey, keyboard.rightArrowKey, keyboard.upArrowKey, keyboard.downArrowKey);
                    return keyboard.enterKey.isPressed || keyboard.numpadEnterKey.isPressed;
                case 2:
                    direction = Axis(keyboard.jKey, keyboard.lKey, keyboard.iKey, keyboard.kKey);
                    return keyboard.rightShiftKey.isPressed;
                default:
                    direction = Axis(keyboard.numpad4Key, keyboard.numpad6Key, keyboard.numpad8Key, keyboard.numpad5Key);
                    return keyboard.numpad0Key.isPressed;
            }
        }

        static Vector2 Axis(KeyControl left, KeyControl right, KeyControl up, KeyControl down)
        {
            Vector2 direction = Vector2.zero;
            if (left.isPressed) direction.x -= 1f;
            if (right.isPressed) direction.x += 1f;
            if (up.isPressed) direction.y += 1f;
            if (down.isPressed) direction.y -= 1f;
            return direction;
        }

        Vector3 ScreenToWorld(Vector2 direction)
        {
            Camera cam = viewCamera != null ? viewCamera : Camera.main;
            if (cam == null)
            {
                return new Vector3(direction.x, 0f, direction.y).normalized;
            }

            Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
            return (right * direction.x + forward * direction.y).normalized;
        }
    }
}
