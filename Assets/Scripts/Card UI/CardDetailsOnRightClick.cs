using UnityEngine;
using UnityEngine.EventSystems;

// 우클릭하면 카드 상세(확대) UI를 띄운다.
public class CardDetailsOnRightClick : MonoBehaviour, IPointerClickHandler
{
    private CardView cardView;

    private void Awake() => cardView = GetComponent<CardView>();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (cardView.Data == null) return;

        Debug.Log($"Right clicked on card: {cardView.Data.CardName}");
        CardDetailView.Instance?.Show(cardView.Data);
    }
}
