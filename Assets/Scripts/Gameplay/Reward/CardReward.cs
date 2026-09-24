

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

// 보상 카드 UI 프리팹에 부착하는 컴포넌트
// 카드 보상 클릭 시 카드 획득 처리
[RequireComponent(typeof(CardView))]
public class CardReward : MonoBehaviour, IPointerClickHandler
{
    private CardView cardView;
    // 선택된 카드를 넘겨준다 — CardRewardView가 고른 카드는 덱으로 날리고 나머지는 디졸브하기 위해 필요.
    public event Action<CardReward> Onclick;
    public CardView View => cardView;

    private void Awake() => cardView = GetComponent<CardView>();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (cardView.Data == null) return;

        DeckManager.Instance.AddCardToPlayerDeck(cardView.Data);
        Onclick?.Invoke(this);
    }
}
