using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static CardData;
using static ItemData;

public enum RewardType { Gold, Item, Card }

// 보상 아이템 구조체 - 보상 유형과 데이터를 함께 담는 용도
// UI에서 버튼마다 클릭 시 로직 다르게 처리하는게 Reward만으로 애매해서 추가로 만듬
public struct RewardItem
{
    public RewardType Type;
    public object Data;
    public string ItemDescription => Type switch
    {
        RewardType.Gold => $"{Data} Gold",
        RewardType.Item => ((ItemData)Data).ItemName,
        RewardType.Card => $"Cards",
        _ => "Unknown Reward"
    };
    public RewardItem(RewardType type, object data)
    {
        Type = type;
        Data = data;
    }
}


// 보상 플레인 클래스
// 전투 전체 보상 데이터 관리
public class Reward
{
    // 아이템 보상 전체 풀 - 런 중에는 바뀔 일이 없으므로 GameManager에서 런 시작에 초기화하고 static으로 관리
    private static List<ItemData> itemRewardsPool = new List<ItemData>();
    public static void SetItemRewardsPool(List<ItemData> pool) => itemRewardsPool = pool;

    private int gold;
    private ItemData itemReward = null;
    private int numCardReward;
    private readonly List<CardData> cardRewards = new List<CardData>();

    public int NumofCardReward => numCardReward;

    public Reward(Dictionary<CardRarity, List<CardData>> cardRewardsPool, RewardProbabilityData rewardData, int numCardReward = 3)
    {
        this.numCardReward = numCardReward;
        GenerateReward(cardRewardsPool, rewardData);
    }

    private void GenerateReward(Dictionary<CardRarity, List<CardData>> cardRewardsPool, RewardProbabilityData rewardData)
    {
        GenerateCardReward(cardRewardsPool, rewardData);
        GenerateGoldReward(rewardData.BattleType);
        GenerateItemReward(rewardData);
    }

    // 카드 보상 생성 메서드
    // 카드 레어도 결정 -> 해당 레어도 카드 풀에서 카드 선택 -> 카드 보상 리스트에 추가 - 카드 보상 수 만큼 반복
    private void GenerateCardReward(Dictionary<CardRarity, List<CardData>> cardRewardsPool, RewardProbabilityData rewardData)
    {
        cardRewards.Clear();
        for (int i = 0; i<numCardReward; i++)
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
        gold = goldReward;
    }

    // 아이템 보상 생성 메서드
    // 아이템 보상 등장 여부 결정 -> 희귀도 결정 -> 전체 풀에서 희귀도 일치/ItemGetType 플래그 포함 두 조건 모두 만족하는 아이템 필터링 & 풀 구성 
    // -> 아이템 보상 리스트에서 랜덤으로 하나 선택
    private void GenerateItemReward(RewardProbabilityData rewardData)
    {
        // TODO: 아이템 보상 생성 로직 - 
    }

    private CardRarity GetRarity(RewardProbabilityData rewardData)
    {
        int total = rewardData.CommonCardProb + rewardData.RareCardProb + rewardData.UniqueCardProb;
        int roll = Random.Range(0, total);

        if (roll < rewardData.CommonCardProb)
            return CardRarity.Common;
        else if (roll < rewardData.CommonCardProb + rewardData.RareCardProb)
            return CardRarity.Rare;
        else
            return CardRarity.Unique;
    }
    // 카드 풀에서 카드 선택하는 메서드 - 중복되면 중복되지 않는 카드 나올 때까지 다시 뽑는 방식
    private CardData PickCard(List<CardData> cardPool)
    {
        int index = Random.Range(0, cardPool.Count);
        if(cardRewards.Contains(cardPool[index]))
        {
            return PickCard(cardPool);
        }
        return cardPool[index];
    }

    // 단일 보상 데이터 반환 메서드
    public T GetReward<T>(RewardType type)
    {
        return type switch
        {
            RewardType.Gold => (T)(object)gold,
            RewardType.Card => (T)(object)cardRewards,
            _ => default
        };
    }

    // 보상 아이템 리스트 반환 메서드
    public List<RewardItem> GetRewardList()
    {
        var items = new List<RewardItem>();

        if(gold > 0)
            items.Add(new RewardItem(RewardType.Gold, gold));
        if(cardRewards.Count > 0)
            items.Add(new RewardItem(RewardType.Card, cardRewards));
        if(itemReward != null)
            items.Add(new RewardItem(RewardType.Item, itemReward));

        return items;
    }
}