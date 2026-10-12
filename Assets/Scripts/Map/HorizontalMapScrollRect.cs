using UnityEngine;
using UnityEngine.EventSystems;

// 휠을 아래로 내리면 진행 방향(오른쪽)으로 이동한다.
// 사용자가 직접 드래그·휠을 쓰면 UserScrolled를 알린다(맵 도입 패닝을 멈추는 데 쓴다).
public class HorizontalMapScrollRect : UnityEngine.UI.ScrollRect
{
    public event System.Action UserScrolled;

    public override void OnBeginDrag(PointerEventData eventData)
    {
        UserScrolled?.Invoke();
        base.OnBeginDrag(eventData);
    }

    public override void OnScroll(PointerEventData eventData)
    {
        UserScrolled?.Invoke();
        Vector2 originalDelta = eventData.scrollDelta;
        try
        {
            if (horizontal && !vertical && Mathf.Abs(originalDelta.y) > Mathf.Abs(originalDelta.x))
                eventData.scrollDelta = new Vector2(originalDelta.y, 0f);
            base.OnScroll(eventData);
        }
        finally
        {
            eventData.scrollDelta = originalDelta;
        }
    }
}
