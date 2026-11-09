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
        public float radius = 100f;
        public Vector2 Value { get; private set; }

        RectTransform baseRect;
        RectTransform knob;

        public static VirtualJoystick Create(Transform canvas)
        {
            var centre = new Vector2(0.5f, 0.5f);
            var bg = UiFactory.CircleImage("Joystick", canvas, Vector2.zero, centre,
                new Vector2(237f, 207f), new Vector2(254f, 254f), new Color(0.05f, 0.19f, 0.17f, 0.6f));
            UiFactory.AddCircleOutline(bg);
            var knobImg = UiFactory.CircleImage("Knob", bg.transform, centre, centre,
                Vector2.zero, new Vector2(104f, 104f), UiFactory.Cream);
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
