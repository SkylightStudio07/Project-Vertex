using System.Collections.Generic;
using UnityEngine;

// 손패를 STS처럼 부채꼴로 배치한다. Unity의 HorizontalLayoutGroup 등 기본 레이아웃 그룹은
// 직선 배치만 지원해서 이 용도로 못 쓴다 — cardContainer에서 HorizontalLayoutGroup을 떼고
// 이 컴포넌트를 붙인 뒤, HandView의 fanLayout 필드에 연결할 것 (에디터 작업 필요).
//
// 카드가 늘어나도 간격(spacing)이 maxWidth 안에 수렴하도록 줄어들어서,
// 일자 배치보다 화면을 덜 차지한다.
public class HandFanLayout : MonoBehaviour
{
    [Header("가로 배치 — 카드가 늘면 간격이 줄며 maxWidth 안으로 수렴")]
    [SerializeField] private float maxCardSpacing = 140f; // 카드 1~2장일 때의 간격 상한
    [SerializeField] private float maxWidth = 900f;        // 손패 전체 폭 상한

    [Header("부채꼴")]
    [SerializeField] private float totalFanAngle = 32f; // 최외곽 카드 사이의 총 각도(도)
    [SerializeField] private float arcSag = 60f;         // 바깥쪽 카드가 아래로 처지는 높이(px)

    // HandView.Refresh()가 카드를 전부 생성한 뒤 한 번 호출한다.
    // 각 카드의 "쉴 때 자세"(위치/회전)를 계산해 CardInteractionView.SetRestingPose로 즉시 적용.
    public void Arrange(IReadOnlyList<CardInteractionView> cards)
    {
        int n = cards.Count;
        if (n == 0) return;

        float spacing = n <= 1 ? 0f : Mathf.Min(maxCardSpacing, maxWidth / (n - 1));
        float half = (n - 1) / 2f;

        for (int i = 0; i < n; i++)
        {
            if (cards[i] == null) continue;

            float t = i - half; // 중앙 기준 대칭 오프셋 (예: 5장이면 -2,-1,0,1,2)
            float normalized = half > 0f ? t / half : 0f; // -1 ~ 1
            float angle = normalized * (totalFanAngle * 0.5f);

            float x = t * spacing;
            // 중앙(angle=0)이 가장 높고 바깥으로 갈수록 cos가 줄어 처지는 부채꼴 곡선
            float y = (Mathf.Cos(angle * Mathf.Deg2Rad) - 1f) * arcSag;

            cards[i].SetRestingPose(new Vector2(x, y), -angle);
        }
    }
}
