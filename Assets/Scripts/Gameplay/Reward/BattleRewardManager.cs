using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static CardData;

public class BattleRewardManager : MonoBehaviour
{
    [Header("카드 보상 풀")]
    [SerializeField] private List<CardData> commonPool;
    [SerializeField] private List<CardData> rarePool;
    [SerializeField] private List<CardData> uniquePool;

    [Header("카드 보상 확률")]
    [SerializeField] private List<RewardProbabilityData> rewardTable;

    // 골드 보상
    private int goldRewardAmount;
    // 카드 보상 개수
    [SerializeField] private int cardRewardAmount = 3;
    // 카드 보상
    private List<CardData> cardRewards = new List<CardData>();


    private void Start()
    {
        BattleManager.Instance.OnBattleVictory += GenerateReward;
    }

    private void GenerateReward(BattleType battleType)
    {
        // TODO: 현재 진행 중인 챕터 받아오기
        int currentChapter = 1;
        cardRewards.Clear();

        RewardProbabilityData probabilityData = GetRewardProbability(currentChapter, battleType);


        for(int i = 0; i< cardRewardAmount; i++)
        {
            CardRarity rarity = GetRarity(probabilityData);
            CardData cardData = PickCard(rarity);
            cardRewards.Add(cardData);
            Debug.Log($"카드 보상: {cardData.CardName}");
        }
    }
    private RewardProbabilityData GetRewardProbability(int chapter, BattleType battleType)
    {
        return rewardTable.Find(t => t.Chapter == chapter && t.BattleType == battleType);
    }
    private CardRarity GetRarity(RewardProbabilityData probailityData)
    {
        int total = probailityData.CommonProbability + probailityData.RareProbability + probailityData.UniqueProbability;
        int roll = Random.Range(0, total);

        if (roll < probailityData.CommonProbability)
            return CardRarity.Common;
        else if (roll < probailityData.CommonProbability + probailityData.RareProbability)
            return CardRarity.Rare;
        else
            return CardRarity.Unique;
    }
    private CardData PickCard(CardRarity rarity)
    {
        List<CardData> cardPool = commonPool;

        switch (rarity)
        {
            case CardRarity.Common:
                cardPool = commonPool;
                break;
            case CardRarity.Rare:
                cardPool = rarePool;
                break;
            case CardRarity.Unique:
                cardPool = uniquePool;
                break;
        }

        int index = Random.Range(0 , cardPool.Count);
        return cardPool[index];
    }

    private void OnDestroy()
    {
        BattleManager.Instance.OnBattleVictory -= GenerateReward;
    }
}
