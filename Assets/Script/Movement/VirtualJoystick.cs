using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform container;
    public RectTransform handle;

    public Vector2 InputVector { get; private set; }

    public void OnDrag(PointerEventData ped)
    {
        Vector2 position = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            container,
            ped.position,
            ped.pressEventCamera,
            out position
        );

        // Menghitung offset relatif dari pusat joystick
        Vector2 size = container.sizeDelta;
        position.x = (position.x / (size.x / 2));
        position.y = (position.y / (size.y / 2));

        InputVector = new Vector2(position.x, position.y);
        InputVector = (InputVector.magnitude > 1.0f) ? InputVector.normalized : InputVector;

        // Gerakkan handle analog
        handle.anchoredPosition = new Vector2(
            InputVector.x * (size.x / 3),
            InputVector.y * (size.y / 3)
        );
    }

    public void OnPointerDown(PointerEventData ped)
    {
        OnDrag(ped);
    }

    public void OnPointerUp(PointerEventData ped)
    {
        InputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}