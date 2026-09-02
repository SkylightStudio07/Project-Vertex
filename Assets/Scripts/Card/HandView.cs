// ============================================================
// filename   : HandView.cs
// description   : BattleManager.Hand 보고 그대로 CardView 인스턴스를 생성/갱신.
//             씬에 하나만 존재하는 손패 컨테이너 오브젝트에 부착.
// ============================================================

using System.Collections.Generic;
using UnityEngine;

public class HandView : MonoBehaviour
{
    [SerializeField] private CardView cardPrefab;    // 카드 프리팹 (CardView 부착된 것)
    [SerializeField] private RectTransform cardContainer; // 카드들이 나열될 부모 Transform
    // 부채꼴 배치 담당. 비워두면 카드가 부모의 기본 배치(레이아웃 그룹 등)를 그대로 따름.
    [SerializeField] private HandFanLayout fanLayout;

    private void Start()
    {
        BattleManager.Instance.OnHandChanged += Refresh;
        // 손패 선택 모드 진입/종료 시 대상 아닌 카드의 흐림 표시를 갱신한다
        HandCardSelector.OnSelectionModeChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnHandChanged -= Refresh;
        HandCardSelector.OnSelectionModeChanged -= Refresh;
    }

    private void Refresh()
    {
        // 기존 카드 뷰 전부 제거. 
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        // 손패의 각 CardData마다 CardView 생성. 늘 그렇듯 이런 식이 퍼포먼스에 썩 좋을지는 모르겠는데, 달리 대안이 없음.
        IReadOnlyList<CardData> hand = BattleManager.Instance.Hand;
        var interactionViews = new List<CardInteractionView>(hand.Count);

        for (int i = 0; i < hand.Count; i++)
        {
            var view = Instantiate(cardPrefab, cardContainer);
            view.SetCard(hand[i]);

            // 선택 모드에서 대상이 아닌 카드는 흐리게 — 어떤 카드를 고를 수 있는지 보이도록
            if (HandCardSelector.IsSelecting && HandCardSelector.Instance != null &&
                !HandCardSelector.Instance.IsSelectable(hand[i]))
            {
                var group = view.GetComponent<CanvasGroup>();
                if (group == null) group = view.gameObject.AddComponent<CanvasGroup>();
                group.alpha = 0.4f;
            }

            CardInteractionView interactionView = view.GetComponent<CardInteractionView>();
            if (interactionView != null)
            {
                interactionView.SetRestingSortingOrder(i);
                interactionView.SetTargetingAnchor(cardContainer);
                interactionViews.Add(interactionView);
            }
        }

        // 카드를 전부 생성한 뒤 한 번에 배치 — 개수(n)를 알아야 부채꼴 간격/각도를 계산할 수 있다.
        if (fanLayout != null)
            fanLayout.Arrange(interactionViews);
    }
}
