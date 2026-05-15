using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class CardRewardView : MonoBehaviour
{
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private Transform cardContainer;

    public void Open(List<CardData> cardRewardList)
    {
        gameObject.SetActive(true);
        Refresh(cardRewardList);
    }

    private void Refresh(List<CardData> cardRewardList)
    {
        // 기존 카드 뷰 전부 제거. 
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        foreach (var cardData in cardRewardList)
        {
            var view = Instantiate(cardPrefab, cardContainer);
            view.SetCard(cardData);
        }
    }

}
