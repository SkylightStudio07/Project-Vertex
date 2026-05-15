using UnityEngine;
using UnityEngine.UI;

// 두 노드 사이의 연결선을 표현하는 컴포넌트
// RectTransform을 중점에 배치하고, 두 점 사이의 거리/각도로 늘리고 회전시킴
public class MapConnectionLine : MonoBehaviour
{
    [SerializeField] private Image lineImage; // 선 이미지 (색상/투명도 조절용)

    // from, to: MapContent 기준 anchoredPosition (노드와 같은 좌표계)
    public void Setup(Vector2 from, Vector2 to)
    {
        Vector2 dir      = to - from;
        float distance   = dir.magnitude;
        float angle      = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        RectTransform rt = GetComponent<RectTransform>();

        // 노드와 같은 좌표계를 쓰도록 anchor를 bottom-center로 통일
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        // pivot은 (0.5, 0.5)로 유지해야 중점 기준으로 회전이 올바르게 됨
        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.anchoredPosition = (from + to) * 0.5f;           // 중점
        rt.sizeDelta        = new Vector2(distance, rt.sizeDelta.y); // 길이 (두께는 프리팹에서)
        rt.localRotation    = Quaternion.Euler(0f, 0f, angle);
    }
}
