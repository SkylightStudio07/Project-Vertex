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
    [SerializeField] private List<RewardProbabilityData> cardRewardTable;

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
            Debug.Log("선택된 카드 희귀도: " + rarity);
            CardData cardData = PickCard(rarity);
            cardRewards.Add(cardData);
            Debug.Log($"카드 보상: {cardData.CardName}");
        }
    }
    private RewardProbabilityData GetRewardProbability(int chapter, BattleType battleType)
    {
        return cardRewardTable.Find(t => t.Chapter == chapter && t.BattleType == battleType);
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

    // 테스트용 - 빌드 전 제거
    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current[UnityEngine.InputSystem.Key.T].wasPressedThisFrame)
            BattleManager.Instance.TestVictory(BattleType.Normal);
    }
}

public class Reward
{
    private int goldRewardAmount;
    private int numofCardReward = 3;
    private readonly List<CardData> cardRewards = new List<CardData>();

    public int GoldRewardAmount => goldRewardAmount;
    public int NumofCardReward => numofCardReward;

    public Reward(Dictionary<CardRarity, List<CardData>> cardRewardsPool, RewardProbabilityData rewardData, BattleType battleType)
    {
        GenerateReward(cardRewardsPool, rewardData, battleType);
    }

    private void GenerateReward(Dictionary<CardRarity, List<CardData>> cardRewardsPool, RewardProbabilityData rewardData, BattleType battleType)
    {
        GenerateCardReward(cardRewardsPool, rewardData);
        GenerateGoldReward(battleType);
    }

    private void GenerateCardReward(Dictionary<CardRarity, List<CardData>> cardRewardsPool, RewardProbabilityData rewardData)
    {
        cardRewards.Clear();
        for (int i = 0; i<numofCardReward; i++)
        {
            CardRarity rarity = GetRarity(rewardData);
            CardData cardData = PickCard(cardRewardsPool[rarity]);
            cardRewards.Add(cardData);
        }
    }
    private void GenerateGoldReward(BattleType battleType)
    {
        int goldReward = 0;
        switch (battleType)
        {
            case BattleType.Normal:
                goldReward = Random.Range(10, 20);
                break;
            case BattleType.Elite:
                goldReward = Random.Range(25, 35);
                break;
            case BattleType.Boss:
                goldReward = Random.Range(50, 80);
                break;
        }
        goldRewardAmount = goldReward;
    }

    private CardRarity GetRarity(RewardProbabilityData rewardData)
    {
        int total = rewardData.CommonProbability + rewardData.RareProbability + rewardData.UniqueProbability;
        int roll = Random.Range(0, total);

        if (roll < rewardData.CommonProbability)
            return CardRarity.Common;
        else if (roll < rewardData.CommonProbability + rewardData.RareProbability)
            return CardRarity.Rare;
        else
            return CardRarity.Unique;
    }
    private CardData PickCard(List<CardData> cardPool)
    {
        int index = Random.Range(0, cardPool.Count);
        return cardPool[index];
    }
}