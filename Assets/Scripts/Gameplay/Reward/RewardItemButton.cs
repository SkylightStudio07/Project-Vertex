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

    public event Action<List<CardData>> OnCardReward;
    void Start()
    {

    }

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
                Destroy(gameObject);
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
        OnCardReward?.Invoke(cards);
    }
}