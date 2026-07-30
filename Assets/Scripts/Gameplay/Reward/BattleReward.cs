using System.Collections.Generic;
using UnityEngine;
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


// 전투 보상 플레인 클래스
// 전투 전체 보상 데이터 관리
public class BattleReward
{
    // 아이템 보상 전체 풀 - 런 중에는 바뀔 일이 없으므로 GameManager에서 런 시작에 초기화하고 static으로 관리
    private static List<ItemData> itemRewardsPool = new List<ItemData>();
    public static void SetItemRewardsPool(List<ItemData> pool) => itemRewardsPool = pool;

    private int goldReward;
    private ItemData itemReward = null;
    private int numCardReward;
    private readonly List<CardData> cardRewards = new List<CardData>();
    // 보상 전용 RNG — RunRng.For(RngStream.Reward, ...)로 노드 고정 재시드된 것을 주입받는다
    private readonly System.Random random;

    public int NumofCardReward => numCardReward;

    public BattleReward(Dictionary<CardRarity, List<CardData>> cardRewardsPool, RewardProbabilityData rewardData, System.Random rng, int numCardReward = 3)
    {
        this.random = rng;
        this.numCardReward = numCardReward;
        GenerateReward(cardRewardsPool, rewardData);
    }

    private void GenerateReward(Dictionary<CardRarity, List<CardData>> cardRewardsPool, RewardProbabilityData rewardData)
    {
        // 테이블 누락(GetRewardProbability 폴백까지 실패) 시에도 골드는 지급 — 보상 화면이 완전히 비는 것 방지
        if (rewardData == null)
        {
            Debug.LogError("[BattleReward] RewardProbabilityData가 null — 골드만 지급. 보상 테이블 등록 확인 필요.");
            GenerateGoldReward(BattleType.Normal);
            return;
        }

        GenerateCardReward(cardRewardsPool, rewardData);
        GenerateGoldReward(rewardData.BattleType);
        GenerateItemReward(rewardData);
    }

    // 카드 보상 생성 메서드
    // 카드 레어도 결정 -> 해당 레어도 카드 풀에서 카드 선택 -> 카드 보상 리스트에 추가 - 카드 보상 수 만큼 반복
    // RewardPicker의 카드 데이터 생성 함수 사용
    private void GenerateCardReward(Dictionary<CardRarity, List<CardData>> cardRewardsPool, RewardProbabilityData rewardData)
    {
        cardRewards.Clear();
        for (int i = 0; i < numCardReward; i++)
        {
            CardRarity rarity = Picker.PickCardRarity(rewardData.CommonCardProb, rewardData.RareCardProb, rewardData.UniqueCardProb, random, rewardData.name);
            CardData cardData = Picker.PickCard(cardRewardsPool, rarity, random, cardRewards);

            if (cardData != null)
            {
                cardRewards.Add(cardData);
            }
            else
            {
                Debug.LogWarning("[BattleReward] 모든 레어도 풀이 비어 있어 카드 보상을 생성하지 못함. PlayerRewardPool 확인 필요.");
            }
        }
    }
    private void GenerateGoldReward(BattleType battleType)
    {
        int goldReward = 0;
        switch (battleType)
        {
            case BattleType.Normal:
                goldReward = random.Next(10, 20);
                break;
            case BattleType.Elite:
                goldReward = random.Next(25, 35);
                break;
            case BattleType.Boss:
                goldReward = random.Next(50, 80);
                break;
        }
        this.goldReward = goldReward;
    }

    // 아이템 보상 생성 메서드
    // 아이템 보상 등장 여부 결정 -> 희귀도 결정 -> 전체 풀에서 희귀도 일치/ItemGetType 플래그 포함 두 조건 모두 만족하는 아이템 필터링 & 풀 구성 
    // -> 아이템 보상 리스트에서 랜덤으로 하나 선택
    private void GenerateItemReward(RewardProbabilityData rewardData)
    {
        if(!IsItemRewardGiven(rewardData))
        {
            itemReward = null;
            return;
        }
        ItemRarity rarity = Picker.PickItemRarity(rewardData.CommonItemProb, rewardData.UncommonItemProb, rewardData.RareItemProb, random, rewardData.name);
        ItemGetType getType = GetItemGetType(rewardData.BattleType);
        itemReward = Picker.PickItem(itemRewardsPool, rarity, getType, random);
    }

    // 아이템 등장 여부 결정
    private bool IsItemRewardGiven(RewardProbabilityData rewardData)
    {
        int roll = random.Next(0, 100);
        return roll < rewardData.ItemProbability;
    }

    // BattleType - ItemGetType 매핑 메서드
    private ItemGetType GetItemGetType(BattleType battleType)
    {
        return battleType switch
        {
            BattleType.Normal => ItemGetType.BattleReward,
            BattleType.Elite => ItemGetType.EliteReward,
            BattleType.Boss => ItemGetType.BossReward,
            _ => ItemGetType.BattleReward
        };
    }

    // 단일 보상 데이터 반환 메서드
    public T GetReward<T>(RewardType type)
    {
        return type switch
        {
            RewardType.Gold => (T)(object)goldReward,
            RewardType.Card => (T)(object)cardRewards,
            RewardType.Item => (T)(object)itemReward,
            _ => default
        };
    }

    // 보상 아이템 리스트 반환 메서드
    public List<RewardItem> GetRewardList()
    {
        var items = new List<RewardItem>();

        if(goldReward > 0)
            items.Add(new RewardItem(RewardType.Gold, goldReward));
        if(cardRewards.Count > 0)
            items.Add(new RewardItem(RewardType.Card, cardRewards));
        if(itemReward != null)
            items.Add(new RewardItem(RewardType.Item, itemReward));

        return items;
    }
}