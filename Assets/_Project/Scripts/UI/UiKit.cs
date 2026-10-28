using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace FartRafters
{
    public static class UiKit
    {
        public static readonly Color PanelColor = new Color(0.05f, 0.16f, 0.28f, 0.85f);
        public static readonly Color ButtonColor = new Color(1f, 0.83f, 0.28f, 1f);
        public static readonly Color ButtonTextColor = new Color(0.28f, 0.16f, 0.04f, 1f);
        public static readonly Color AccentColor = new Color(0.55f, 0.85f, 0.3f, 1f);
        public static readonly Color StarOnColor = new Color(1f, 0.82f, 0.15f, 1f);
        public static readonly Color StarOffColor = new Color(1f, 1f, 1f, 0.2f);

        static Font font;
        static Sprite circle;
        static Sprite star;
        static Sprite rounded;

        public static Font Font
        {
            get
            {
                if (font == null)
                {
                    font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                return font;
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (circle == null)
                {
                    circle = CreateSprite(128, (x, y, r) => Mathf.Clamp01(r - Vector2.Distance(new Vector2(x, y), new Vector2(r, r))));
                }
                return circle;
            }
        }

        public static Sprite Star
        {
            get
            {
                if (star == null)
                {
                    star = CreateSprite(128, StarAlpha);
                }
                return star;
            }
        }

        public static Sprite Rounded
        {
            get
            {
                if (rounded == null)
                {
                    rounded = CreateRoundedSprite(64, 22f);
                }
                return rounded;
            }
        }

        public static string FormatTime(float seconds)
        {
            int minutes = (int)(seconds / 60f);
            return $"{minutes:00}:{seconds - minutes * 60f:00.0}";
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        public static RectTransform CreateCanvas(Transform parent, out Canvas canvas)
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return (RectTransform)go.transform;
        }

        public static RectTransform CreatePanel(RectTransform parent, string name, Vector2 size)
        {
            RectTransform panel = CreateImage(parent, name, Rounded, PanelColor, new Vector2(0.5f, 0.5f), Vector2.zero, size);
            panel.GetComponent<Image>().type = Image.Type.Sliced;
            panel.pivot = new Vector2(0.5f, 0.5f);
            return panel;
        }

        public static RectTransform CreateGroup(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        public static Text CreateText(RectTransform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var text = go.GetComponent<Text>();
            text.font = Font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(2f, -2f);
            return text;
        }

        public static Text CreateButton(RectTransform parent, string label, Vector2 anchor, Vector2 position, Vector2 size, Action onClick)
        {
            RectTransform rect = CreateImage(parent, label, Rounded, ButtonColor, anchor, position, size);
            var image = rect.GetComponent<Image>();
            image.type = Image.Type.Sliced;
            image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.onClick.AddListener(() =>
            {
                GameAudio.Play(GameAudio.Sfx.Click, 0.7f);
                onClick();
            });
            rect.gameObject.AddComponent<ButtonBounce>();

            Text text = CreateText(rect, "Label", new Vector2(0.5f, 0.5f), Vector2.zero, size, 34, TextAnchor.MiddleCenter);
            text.color = ButtonTextColor;
            Object.Destroy(text.GetComponent<Shadow>());
            text.text = label;
            return text;
        }

        public static Text CreateCenterButton(RectTransform parent, string label, Vector2 position, Vector2 size, Action onClick)
        {
            Text text = CreateButton(parent, label, new Vector2(0.5f, 0.5f), position, size, onClick);
            ((RectTransform)text.transform.parent).pivot = new Vector2(0.5f, 0.5f);
            return text;
        }

        public static Text CreateCenterText(RectTransform parent, string name, Vector2 position, Vector2 size, int fontSize)
        {
            Text text = CreateText(parent, name, new Vector2(0.5f, 0.5f), position, size, fontSize, TextAnchor.MiddleCenter);
            text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            return text;
        }

        public static InputField CreateInput(RectTransform parent, string placeholder, Vector2 position, Vector2 size, int fontSize, int limit)
        {
            RectTransform rect = CreateImage(parent, "Input", Rounded, new Color(1f, 1f, 1f, 0.95f), new Vector2(0.5f, 0.5f), position, size);
            rect.pivot = new Vector2(0.5f, 0.5f);
            var image = rect.GetComponent<Image>();
            image.type = Image.Type.Sliced;
            image.raycastTarget = true;

            Text hint = CreateText(rect, "Placeholder", new Vector2(0.5f, 0.5f), Vector2.zero, size - new Vector2(40f, 10f), fontSize, TextAnchor.MiddleLeft);
            hint.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            hint.text = placeholder;
            hint.color = new Color(0.3f, 0.3f, 0.35f, 0.6f);
            Object.Destroy(hint.GetComponent<Shadow>());
            Text value = CreateText(rect, "Text", new Vector2(0.5f, 0.5f), Vector2.zero, size - new Vector2(40f, 10f), fontSize, TextAnchor.MiddleLeft);
            value.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            value.color = new Color(0.1f, 0.1f, 0.15f);
            value.supportRichText = false;
            Object.Destroy(value.GetComponent<Shadow>());

            var input = rect.gameObject.AddComponent<InputField>();
            input.textComponent = value;
            input.placeholder = hint;
            input.characterLimit = limit;
            input.lineType = InputField.LineType.SingleLine;
            rect.gameObject.AddComponent<KeyboardLift>();
            return input;
        }

        public static void AddBadge(RectTransform button, out Image dot, out Text count)
        {
            RectTransform rect = CreateImage(button, "Badge", Circle, new Color(0.9f, 0.15f, 0.15f), new Vector2(1f, 1f), new Vector2(10f, 10f), new Vector2(54f, 54f));
            rect.pivot = new Vector2(0.5f, 0.5f);
            dot = rect.GetComponent<Image>();
            count = CreateCenterText(rect, "Count", Vector2.zero, new Vector2(54f, 54f), 30);
            dot.gameObject.SetActive(false);
        }

        public static void AddOutline(Text text, float size)
        {
            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.1f, 0.2f, 0.35f, 0.9f);
            outline.effectDistance = new Vector2(size, -size);
        }

        public static RectTransform CreateImage(RectTransform parent, string name, Sprite sprite, Color color, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        public static Image[] CreateStars(RectTransform parent, Vector2 anchor, Vector2 position, float size, float spacing)
        {
            var stars = new Image[3];
            for (int i = 0; i < stars.Length; i++)
            {
                Vector2 offset = new Vector2((i - 1) * (size + spacing), 0f);
                RectTransform rect = CreateImage(parent, "Star", Star, StarOffColor, anchor, position + offset, new Vector2(size, size));
                rect.pivot = new Vector2(0.5f, 0.5f);
                stars[i] = rect.GetComponent<Image>();
            }
            return stars;
        }

        public static void SetStars(Image[] stars, int count)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].color = i < count ? StarOnColor : StarOffColor;
            }
        }

        static float StarAlpha(float x, float y, float radius)
        {
            Vector2 p = new Vector2(x - radius, y - radius) / radius;
            float angle = Mathf.Atan2(p.x, p.y);
            float sector = Mathf.PI * 2f / 5f;
            float local = Mathf.Repeat(angle + sector * 0.5f, sector) - sector * 0.5f;
            float t = Mathf.Abs(local) / (sector * 0.5f);
            float outer = 0.95f;
            float inner = 0.42f;
            float edge = Mathf.Lerp(outer, inner, t);
            return Mathf.Clamp01((edge - p.magnitude) * radius);
        }

        static Sprite CreateRoundedSprite(int size, float radius)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float dx = Mathf.Max(radius - px, px - (size - radius), 0f);
                    float dy = Mathf.Max(radius - py, py - (size - radius), 0f);
                    float alpha = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            float border = radius + 2f;
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        static Sprite CreateSprite(int size, Func<float, float, float, float> alpha)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            float radius = size * 0.5f;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    byte a = (byte)(Mathf.Clamp01(alpha(x + 0.5f, y + 0.5f, radius)) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
