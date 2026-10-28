using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FartRafters
{
    public class TutorialGuide : MonoBehaviour
    {
        public struct Step
        {
            public RectTransform target;
            public string text;

            public Step(RectTransform target, string text)
            {
                this.target = target;
                this.text = text;
            }

            public Step(Component target, string text)
            {
                this.target = target != null ? target.transform as RectTransform : null;
                this.text = text;
            }
        }

        const string Prefix = "tut.";
        const string Registry = "tut.ids";
        const float Padding = 18f;

        static TutorialGuide current;

        RectTransform canvas;
        RectTransform ring;
        RectTransform bubble;
        Text body;
        Text counter;
        Text nextLabel;
        readonly List<Step> steps = new List<Step>();
        string id;
        int index;
        bool pauses;
        float previousTimeScale = 1f;
        Action done;

        public static bool Active => current != null;

        public static bool Seen(string id)
        {
            return PlayerPrefs.GetInt(Prefix + id, 0) == 1;
        }

        public static void ResetAll()
        {
            foreach (string known in PlayerPrefs.GetString(Registry, "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                PlayerPrefs.DeleteKey(Prefix + known);
            }
            PlayerPrefs.DeleteKey(Registry);
            PlayerPrefs.Save();
        }

        static void MarkSeen(string id)
        {
            PlayerPrefs.SetInt(Prefix + id, 1);
            string registry = PlayerPrefs.GetString(Registry, "");
            if (!("," + registry + ",").Contains("," + id + ","))
            {
                PlayerPrefs.SetString(Registry, registry.Length == 0 ? id : registry + "," + id);
            }
            PlayerPrefs.Save();
        }

        public static bool Show(RectTransform canvas, string id, IList<Step> steps, bool pauseGame = false, Action onDone = null)
        {
            if (canvas == null || current != null || Seen(id) || steps == null || steps.Count == 0)
            {
                return false;
            }
            RectTransform root = UiKit.CreateGroup(canvas, "Tutorial_" + id);
            root.SetAsLastSibling();
            var guide = root.gameObject.AddComponent<TutorialGuide>();
            guide.canvas = canvas;
            guide.id = id;
            guide.steps.AddRange(steps);
            guide.pauses = pauseGame;
            guide.done = onDone;
            guide.Build(root);
            current = guide;
            if (pauseGame)
            {
                guide.previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                PlayerBrain.TutorialBlocked = true;
            }
            guide.Present(0);
            return true;
        }

        void Build(RectTransform root)
        {
            var shade = root.gameObject.AddComponent<Image>();
            shade.color = new Color(0f, 0f, 0f, 0.35f);
            shade.raycastTarget = true;

            ring = UiKit.CreateImage(root, "Highlight", UiKit.Rounded, new Color(1f, 0.85f, 0.2f, 0.45f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 100f));
            ring.pivot = new Vector2(0.5f, 0.5f);
            ring.GetComponent<Image>().type = Image.Type.Sliced;
            ring.GetComponent<Image>().raycastTarget = false;

            bubble = UiKit.CreatePanel(root, "Bubble", new Vector2(860f, 300f));
            bubble.GetComponent<Image>().color = new Color(1f, 0.97f, 0.88f, 0.97f);
            body = UiKit.CreateText(bubble, "Text", new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(800f, 170f), 34, TextAnchor.UpperCenter);
            body.color = new Color(0.2f, 0.12f, 0.05f);
            Destroy(body.GetComponent<Shadow>());
            counter = UiKit.CreateText(bubble, "Counter", new Vector2(0.5f, 0f), new Vector2(0f, 38f), new Vector2(200f, 50f), 28, TextAnchor.MiddleCenter);
            counter.color = new Color(0.45f, 0.3f, 0.15f);
            Destroy(counter.GetComponent<Shadow>());
            UiKit.CreateButton(bubble, "Geç", new Vector2(0f, 0f), new Vector2(24f, 20f), new Vector2(220f, 80f), Skip);
            nextLabel = UiKit.CreateButton(bubble, "İleri", new Vector2(1f, 0f), new Vector2(-24f, 20f), new Vector2(220f, 80f), Next);
            ((RectTransform)nextLabel.transform.parent).pivot = new Vector2(1f, 0f);
        }

        void Present(int step)
        {
            index = step;
            Step item = steps[index];
            body.text = item.text;
            counter.text = steps.Count > 1 ? (index + 1) + " / " + steps.Count : "";
            nextLabel.text = index == steps.Count - 1 ? "Tamam" : "İleri";
            Place();
        }

        void Place()
        {
            Vector2 half = canvas.rect.size * 0.5f;
            Vector2 bubbleSize = bubble.sizeDelta;
            RectTransform target = steps[index].target;
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                ring.gameObject.SetActive(false);
                bubble.anchoredPosition = Vector2.zero;
                return;
            }
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            foreach (Vector3 corner in corners)
            {
                Vector2 local = canvas.InverseTransformPoint(corner);
                min = Vector2.Min(min, local);
                max = Vector2.Max(max, local);
            }
            Vector2 center = (min + max) * 0.5f;
            Vector2 size = max - min + Vector2.one * (Padding * 2f);
            ring.gameObject.SetActive(true);
            ring.anchoredPosition = center;
            ring.sizeDelta = size;

            float y = center.y > 0f ? min.y - Padding - bubbleSize.y * 0.5f - 20f : max.y + Padding + bubbleSize.y * 0.5f + 20f;
            if (y + bubbleSize.y * 0.5f > half.y || y - bubbleSize.y * 0.5f < -half.y)
            {
                y = center.y > 0f ? -half.y + bubbleSize.y * 0.5f + 30f : half.y - bubbleSize.y * 0.5f - 30f;
            }
            float x = Mathf.Clamp(center.x, -half.x + bubbleSize.x * 0.5f + 20f, half.x - bubbleSize.x * 0.5f - 20f);
            bubble.anchoredPosition = new Vector2(x, y);
        }

        void Update()
        {
            if (ring != null && ring.gameObject.activeSelf)
            {
                float pulse = 1f + Mathf.Sin(Time.unscaledTime * 5f) * 0.04f;
                ring.localScale = new Vector3(pulse, pulse, 1f);
            }
        }

        void Next()
        {
            if (index + 1 < steps.Count)
            {
                Present(index + 1);
                return;
            }
            Finish();
        }

        void Skip()
        {
            Finish();
        }

        void Finish()
        {
            MarkSeen(id);
            if (pauses)
            {
                Time.timeScale = previousTimeScale;
                PlayerBrain.TutorialBlocked = false;
            }
            current = null;
            Destroy(gameObject);
            done?.Invoke();
        }

        void OnDestroy()
        {
            if (current == this)
            {
                current = null;
                if (pauses)
                {
                    Time.timeScale = previousTimeScale;
                    PlayerBrain.TutorialBlocked = false;
                }
            }
        }
    }
}
