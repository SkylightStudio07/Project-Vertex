using UnityEngine;
using UnityEngine.EventSystems;

// 성소 v3 후보 칸 하나의 클릭 판정. 칸이 사선 다각형이라 사각형 대신 꼭짓점 안쪽만 받는다.
// 이 오브젝트는 1920×1080 판 전체를 덮는 투명 Image에 붙인다 (꼭짓점은 layout.json candidates[].polygon 그대로).
public class SanctuarySlotHit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ICanvasRaycastFilter
{
    private const float CanvasW = 1920f, CanvasH = 1080f;

    [Tooltip("칸 꼭짓점 (1920×1080, 좌상단 원점)")]
    public Vector2[] polygon;

    public bool Interactable { get; set; } = true;

    private SanctuarySelectBoard _board;
    private bool _hovered;

    private SanctuarySelectBoard Board => _board != null ? _board : _board = GetComponentInParent<SanctuarySelectBoard>(true);

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Interactable) return;
        _hovered = true;
        Board?.OnSlotHover(this, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_hovered) return;
        _hovered = false;
        Board?.OnSlotHover(this, false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Interactable || eventData.button != PointerEventData.InputButton.Left) return;
        Board?.OnSlotClicked(this);
    }

    private void OnDisable()
    {
        if (_hovered) Board?.OnSlotHover(this, false);
        _hovered = false;
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (polygon == null || polygon.Length < 3) return false;
        var self = (RectTransform)transform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(self, screenPoint, eventCamera, out Vector2 local))
            return false;
        Rect r = self.rect;
        var p = new Vector2((local.x - r.xMin) / r.width * CanvasW, (r.yMax - local.y) / r.height * CanvasH);

        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            Vector2 a = polygon[i], b = polygon[j];
            if ((a.y > p.y) != (b.y > p.y) && p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x)
                inside = !inside;
        }
        return inside;
    }
}
