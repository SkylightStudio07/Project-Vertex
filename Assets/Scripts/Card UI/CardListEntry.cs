using System;
using UnityEngine;
using UnityEngine.EventSystems;

// 카드 목록에 나열된 카드 하나에 대한 컴포넌트
// 어떤 카드가 선택됐는지 알리기만 하고 실제 처리(제거/강화 등)는 목록을 연 쪽이 콜백으로 담당.
[RequireComponent(typeof(CardView))]
public class CardListEntry : MonoBehaviour, IPointerClickHandler
{
    private CardView cardView;
    private bool selectable = true;

    public event Action<CardData> OnClicked;

    private void Awake() => cardView = GetComponent<CardView>();

    // 보기 전용 목록이거나 제거 불가 카드면 false — 호버·우클릭 상세는 계속 동작한다
    public void SetSelectable(bool value) => selectable = value;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!selectable || cardView.Data == null) return;

        OnClicked?.Invoke(cardView.Data);
    }
}
