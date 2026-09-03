

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
    public event Action Onclick;

    private void Awake() => cardView = GetComponent<CardView>();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (cardView.Data == null) return;

        DeckManager.Instance.AddCardToPlayerDeck(cardView.Data);
        Onclick?.Invoke();
    }
}
