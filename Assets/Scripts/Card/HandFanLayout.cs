using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// 손패를 STS처럼 부채꼴로 배치한다. Unity의 HorizontalLayoutGroup 등 기본 레이아웃 그룹은
// 직선 배치만 지원해서 이 용도로 못 쓴다 — cardContainer에서 HorizontalLayoutGroup을 떼고
// 이 컴포넌트를 붙인 뒤, HandView의 fanLayout 필드에 연결할 것 (에디터 작업 필요).
//
// 카드가 늘어나도 간격(spacing)이 maxWidth 안에 수렴하도록 줄어들어서,
// 일자 배치보다 화면을 덜 차지한다.
public class HandFanLayout : MonoBehaviour
{
    // 카드 한 장의 손패 위치와 회전값을 저장한다.
    public readonly struct HandPose
    {
        public readonly Vector2 Position; // 카드가 도착할 위치.
        public readonly float RotationZ; // 카드가 도착할 Z축 회전값.

        // 카드 위치와 회전값을 초기화한다.
        public HandPose(Vector2 position, float rotationZ)
        {
            Position = position;
            RotationZ = rotationZ;
        }
    }

    [Header("가로 배치 — 카드가 늘면 간격이 줄며 maxWidth 안으로 수렴")]
    [FormerlySerializedAs("maxCardSpacing")]
    [SerializeField] private float _maxCardSpacing = 140f; // 카드 1~2장일 때의 간격 상한.
    [FormerlySerializedAs("maxWidth")]
    [SerializeField] private float _maxWidth = 900f; // 손패 전체 폭 상한.

    [Header("부채꼴")]
    [FormerlySerializedAs("totalFanAngle")]
    [SerializeField] private float _maxTotalFanAngle = 32f; // 최외곽 카드 사이의 최대 총 각도.
    [SerializeField] private float _anglePerCardGap = 8f; // 카드가 한 장 늘어날 때 증가할 총 각도.
    [FormerlySerializedAs("arcSag")]
    [SerializeField] private float _arcSag = 60f; // 바깥쪽 카드가 아래로 처지는 높이.

    [Header("Cache")]
    [SerializeField] private int _maxHandSize = 10; // 미리 계산할 최대 손패 수.

    private HandPose[,] _poseCache; // 패 개수와 카드 순서별 자세를 저장하는 캐시.

    // 게임 시작 시 모든 패 개수에 대한 자세를 한 번 계산한다.
    private void Awake()
    {
        BuildPoseCache();
    }

    // 패 개수별 카드 위치와 각도를 캐시에 저장한다.
    private void BuildPoseCache()
    {
        int safeMaxHandSize = Mathf.Max(1, _maxHandSize); // 캐시에 사용할 최소 1 이상의 손패 한도.
        _poseCache = new HandPose[safeMaxHandSize + 1, safeMaxHandSize];

        for (int cardCount = 1; cardCount <= safeMaxHandSize; cardCount++)
        {
            for (int cardIndex = 0; cardIndex < cardCount; cardIndex++)
                _poseCache[cardCount, cardIndex] = CalculatePose(cardIndex, cardCount);
        }
    }

    // 카드 순서와 현재 패 개수에 해당하는 캐시된 자세를 반환한다.
    public HandPose GetPose(int cardIndex, int cardCount)
    {
        if (_poseCache == null)
            BuildPoseCache();

        int cachedMaxHandSize = _poseCache.GetLength(1); // 현재 캐시에 저장된 최대 손패 수.
        int safeCount = Mathf.Clamp(cardCount, 1, cachedMaxHandSize); // 캐시 범위 내 패 개수.
        int safeIndex = Mathf.Clamp(cardIndex, 0, safeCount - 1); // 캐시 범위 내 카드 순서.
        return _poseCache[safeCount, safeIndex];
    }

    // 카드 순서와 패 개수를 이용해 카드 자세를 계산한다.
    private HandPose CalculatePose(int cardIndex, int cardCount)
    {
        float spacing = cardCount <= 1
            ? 0f
            : Mathf.Min(_maxCardSpacing, _maxWidth / (cardCount - 1)); // 카드 사이 실제 간격.
        float half = (cardCount - 1) * 0.5f; // 손패 중앙을 나타내는 인덱스.
        
        float offset = half - cardIndex; // 중앙에서 현재 카드까지의 거리.
        float normalized = half > 0f ? offset / half : 0f; // -1부터 1까지의 카드 위치.
        float totalAngle = Mathf.Min(
            _maxTotalFanAngle,
            _anglePerCardGap * (cardCount - 1)); // 현재 패 개수에서 사용할 전체 각도.
        float angle = normalized * totalAngle * 0.5f; // 현재 카드에 적용할 각도.
        float x = offset * spacing; // 현재 카드의 가로 위치.
        float y = (Mathf.Cos(angle * Mathf.Deg2Rad) - 1f) * _arcSag; // 부채꼴 세로 위치.

        return new HandPose(new Vector2(x, y), -angle);
    }

    // HandView.Refresh()가 카드를 전부 생성한 뒤 한 번 호출한다.
    // 각 카드의 "쉴 때 자세"(위치/회전)를 계산해 CardInteractionView.SetRestingPose로 즉시 적용.
    public void Arrange(IReadOnlyList<CardInteractionView> cards)
    {
        int cardCount = cards.Count; // 현재 배치할 카드 수.
        if (cardCount == 0) return;

        for (int i = 0; i < cardCount; i++)
        {
            if (cards[i] == null) continue;

            HandPose pose = GetPose(i, cardCount); // 현재 카드 순서에 대응하는 캐시된 자세.
            cards[i].SetRestingPose(pose.Position, pose.RotationZ);
        }
    }
}
