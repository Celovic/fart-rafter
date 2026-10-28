using UnityEngine;
using UnityEngine.UI;

namespace FartRafters
{
    public class KeyboardLift : MonoBehaviour
    {
        const float Margin = 30f;
        const float FallbackShare = 0.55f;

        InputField input;
        RectTransform field;
        RectTransform screen;
        Canvas canvas;
        Vector2 home;
        bool lifted;
        readonly Vector3[] corners = new Vector3[4];

        void Setup()
        {
            input = GetComponent<InputField>();
            field = (RectTransform)transform;
            Transform current = transform;
            while (current.parent != null && current.parent.GetComponent<Canvas>() == null)
            {
                current = current.parent;
            }
            screen = current as RectTransform;
            canvas = GetComponentInParent<Canvas>();
            canvas = canvas != null ? canvas.rootCanvas : null;
        }

        void LateUpdate()
        {
            if (screen == null)
            {
                Setup();
                if (screen == null || canvas == null)
                {
                    return;
                }
            }
            bool open = input != null && input.isFocused && TouchScreenKeyboard.isSupported && TouchScreenKeyboard.visible;
            if (!open)
            {
                if (lifted)
                {
                    screen.anchoredPosition = home;
                    lifted = false;
                }
                return;
            }
            if (!lifted)
            {
                home = screen.anchoredPosition;
                lifted = true;
            }
            float scale = Mathf.Max(0.01f, canvas.scaleFactor);
            float keyboard = TouchScreenKeyboard.area.height;
            if (keyboard <= 1f)
            {
                keyboard = Screen.height * FallbackShare;
            }
            field.GetWorldCorners(corners);
            float applied = (screen.anchoredPosition.y - home.y) * scale;
            float bottom = corners[0].y - applied;
            float needed = Mathf.Max(0f, keyboard + Margin * scale - bottom);
            float maxLift = Mathf.Max(0f, Screen.height - (corners[1].y - applied) - Margin * scale);
            screen.anchoredPosition = home + new Vector2(0f, Mathf.Min(needed, maxLift) / scale);
        }

        void OnDisable()
        {
            if (lifted && screen != null)
            {
                screen.anchoredPosition = home;
                lifted = false;
            }
        }
    }
}
