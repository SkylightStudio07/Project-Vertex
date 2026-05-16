using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

// 보상 카드 선택 UI 띄우는 스크립트. 카드 보상 버튼 클릭 시 열리는 뷰.
public class CardRewardView : MonoBehaviour
{
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private Transform cardContainer;

    RewardItemButton button;

    public void Open(List<CardData> cardRewardList, RewardItemButton button)
    {
        this.button = button;
        gameObject.SetActive(true);
        Refresh(cardRewardList);
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void Refresh(List<CardData> cardRewardList)
    {
        // 기존 카드 뷰 전부 제거. 
        foreach (Transform child in cardContainer){
            if (child.TryGetComponent<CardReward>(out var cardReward))
            {
                cardReward.Onclick -= Close;
                cardReward.Onclick -= button.CompleteReward;
            }
            Destroy(child.gameObject);
        }

        foreach (var cardData in cardRewardList)
        {
            var view = Instantiate(cardPrefab, cardContainer);
            view.SetCard(cardData);
            // 카드 클릭시 UI 닫기, 버튼 삭제용 이벤트 구독
            if(view.gameObject.TryGetComponent<CardReward>(out var reward))
            {
                reward.Onclick += Close;
                reward.Onclick += button.CompleteReward;
            }
        }
    }
}
