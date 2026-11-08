using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CityWalk
{
    /// <summary>
    /// Fixed-position on-screen joystick for touch screens (also works with a mouse).
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public float radius = 90f;
        public Vector2 Value { get; private set; }

        RectTransform baseRect;
        RectTransform knob;

        public static VirtualJoystick Create(Transform canvas)
        {
            var bg = UiFactory.Panel("Joystick", canvas, new Vector2(0f, 0f), new Vector2(0.5f, 0.5f),
                new Vector2(230f, 210f), new Vector2(240f, 240f), new Color(1f, 1f, 1f, 0.35f), UiFactory.Circle);
            var knobImg = UiFactory.Panel("Knob", bg.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(96f, 96f), new Color(1f, 1f, 1f, 0.9f), UiFactory.Circle);
            knobImg.raycastTarget = false;

            var js = bg.gameObject.AddComponent<VirtualJoystick>();
            js.baseRect = (RectTransform)bg.transform;
            js.knob = (RectTransform)knobImg.transform;
            return js;
        }

        public void OnPointerDown(PointerEventData e) => OnDrag(e);

        public void OnDrag(PointerEventData e)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, e.position, e.pressEventCamera, out var local))
                return;
            var clamped = Vector2.ClampMagnitude(local, radius);
            knob.anchoredPosition = clamped;
            Value = clamped / radius;
        }

        public void OnPointerUp(PointerEventData e)
        {
            knob.anchoredPosition = Vector2.zero;
            Value = Vector2.zero;
        }

        void OnDisable()
        {
            Value = Vector2.zero;
            if (knob != null) knob.anchoredPosition = Vector2.zero;
        }
    }
}
