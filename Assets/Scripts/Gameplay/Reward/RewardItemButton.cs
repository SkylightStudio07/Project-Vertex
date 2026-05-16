using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RewardItemButton : MonoBehaviour
{
    private RewardItem rewardItem;
    [SerializeField] private TextMeshProUGUI buttonText;

    public event Action<List<CardData>, RewardItemButton> OnCardReward;

    // 버튼 삭제 위한 이벤트. 보상 획득 후 실제 버튼 오브젝트 삭제는 RewardsView에서 처리.
    public event Action<RewardItemButton> OnDestroyed;

    public void Bind(RewardItem item)
    {
        rewardItem = item;
        buttonText.text = item.ItemDescription;
    }

    public void OnClick()
    {
        switch(rewardItem.Type)
        {
            case RewardType.Gold:
                GameManager.Instance.PlayerGold += (int)rewardItem.Data;
                CompleteReward();
                break;
            case RewardType.Card:
                DisplayCardReward();
                break;
            default:
                Debug.LogWarning("Unknown reward type clicked.");
                break;
        }
    }

    private void DisplayCardReward()
    {
        if (rewardItem.Data == null || rewardItem.Type != RewardType.Card)
            return;

        var cards = (List<CardData>)rewardItem.Data;
        OnCardReward?.Invoke(cards, this);
    }
    
    public void CompleteReward()
    {
        OnDestroyed?.Invoke(this);
    }
}