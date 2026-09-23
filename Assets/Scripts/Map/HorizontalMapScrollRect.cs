using UnityEngine;
using UnityEngine.EventSystems;

// 휠을 아래로 내리면 진행 방향(오른쪽)으로 이동한다.
public class HorizontalMapScrollRect : UnityEngine.UI.ScrollRect
{
    public override void OnScroll(PointerEventData eventData)
    {
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
