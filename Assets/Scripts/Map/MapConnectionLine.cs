using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 두 노드 사이의 연결선을 표현하는 컴포넌트
// RectTransform을 중점에 배치하고, 두 점 사이의 거리/각도로 늘리고 회전시킴
// 맵 v1(작전 지도): 지나온 길은 청록 실선, 지금 갈 수 있는 길은 흑연 실선, 나머지는 흐린 점선 (SetStyle)
public class MapConnectionLine : MonoBehaviour
{
    public enum Style { Other, Accessible, Traveled }

    [SerializeField] private Image lineImage; // 선 이미지 (색상/투명도 조절용)
    [Header("맵 v1 (비워두면 예전 단색 선)")]
    [SerializeField] private Sprite solidSprite;
    [SerializeField] private Sprite dashedSprite;

    private static readonly Color LineColor = new(0.22f, 0.24f, 0.26f, 0.7f);
    private static readonly Color TraveledColor = new(0.05f, 0.72f, 0.95f, 1f);
    private static readonly Color AccessibleColor = new(0.16f, 0.17f, 0.19f, 1f);
    private static readonly Color OtherColor = new(0.16f, 0.17f, 0.19f, 0.55f);

    private Color _color = LineColor;

    // 출발 노드의 층. 맵 열기 연출에서 왼쪽 층부터 차례로 켜기 위해 쓴다.
    public int FromFloor { get; private set; }
    public MapNode From { get; private set; }
    public MapNode To { get; private set; }

    // from, to: MapContent 기준 anchoredPosition (노드와 같은 좌표계)
    public void Setup(Vector2 from, Vector2 to, int fromFloor = 0)
    {
        FromFloor = fromFloor;
        Vector2 dir      = to - from;
        float distance   = dir.magnitude;
        float angle      = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        RectTransform rt = GetComponent<RectTransform>();

        // 가로 맵의 노드와 같은 left-center 좌표계를 사용한다.
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        // pivot은 (0.5, 0.5)로 유지해야 중점 기준으로 회전이 올바르게 됨
        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.anchoredPosition = (from + to) * 0.5f;           // 중점
        rt.sizeDelta        = new Vector2(distance, rt.sizeDelta.y); // 길이 (두께는 프리팹에서)
        rt.localRotation    = Quaternion.Euler(0f, 0f, angle);
        lineImage.color = _color;
        lineImage.raycastTarget = false;
    }

    public void SetNodes(MapNode from, MapNode to) { From = from; To = to; }

    public void SetStyle(Style style)
    {
        if (solidSprite == null) return; // 예전 프리팹
        bool dashed = style == Style.Other && dashedSprite != null;
        lineImage.sprite = dashed ? dashedSprite : solidSprite;
        lineImage.type = dashed ? Image.Type.Tiled : Image.Type.Sliced;
        _color = style switch
        {
            Style.Traveled => TraveledColor,
            Style.Accessible => AccessibleColor,
            _ => OtherColor,
        };
        lineImage.DOKill();
        lineImage.color = _color;
    }

    public void PlayReveal(float delay, float duration)
    {
        lineImage.DOKill();
        lineImage.color = new Color(_color.r, _color.g, _color.b, 0f);
        lineImage.DOFade(_color.a, duration).SetDelay(delay).SetUpdate(true).SetLink(gameObject);
    }
}
