using UnityEngine;
using UnityEngine.EventSystems;

namespace FartRafters
{
    public class ButtonBounce : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Range(0.5f, 1f)] public float pressedScale = 0.93f;
        [Min(0.1f)] public float speed = 18f;

        bool pressed;

        public void OnPointerDown(PointerEventData eventData)
        {
            pressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pressed = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pressed = false;
        }

        void OnDisable()
        {
            pressed = false;
            transform.localScale = Vector3.one;
        }

        void Update()
        {
            float target = pressed ? pressedScale : 1f;
            float scale = Mathf.Lerp(transform.localScale.x, target, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
