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
    private void GenerateCardReward(Dictionary<CardRarity, List<CardData>> cardRewardsPool, RewardProbabilityData rewardData)
    {
        cardRewards.Clear();
        for (int i = 0; i < numCardReward; i++)
        {
            CardRarity rarity = GetCardRarity(rewardData);

            // 굴린 레어도의 풀이 비어 있으면 한 단계 낮은 레어도로 폴백.
            // 예전에 "3장 생성인데 2장만 표시"류 무증상 결함이 있었어서, 조용히 장수가 줄지 않게 한다.
            CardData cardData = null;
            for (int r = (int)rarity; r >= (int)CardRarity.Common && cardData == null; r--)
            {
                if (cardRewardsPool.TryGetValue((CardRarity)r, out var pool))
                    cardData = PickCard(pool);
            }

            if (cardData != null)
                cardRewards.Add(cardData);
            else
                Debug.LogWarning("[BattleReward] 모든 레어도 풀이 비어 있어 카드 보상을 생성하지 못함. PlayerRewardPool 확인 필요.");
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
        Debug.Log("Item reward is given.");
        ItemRarity rarity = GetItemRarity(rewardData);
        ItemGetType getType = GetItemGetType(rewardData.BattleType);
        
        List<ItemData> filteredPool = itemRewardsPool.FindAll(item => item.Rarity == rarity && (item.ItemTypes & getType) != 0);

        if(filteredPool.Count == 0)
        {
            itemReward = null;
            return;
        }

        itemReward = filteredPool[random.Next(0, filteredPool.Count)];
        Debug.Log("Item reward generated: " + itemReward.ItemName);
    }

    // 아이템 등장 여부 결정
    private bool IsItemRewardGiven(RewardProbabilityData rewardData)
    {
        int roll = random.Next(0, 100);
        return roll < rewardData.ItemProbability;
    }

    // 레어도 결정 메서드들 - 카드/아이템 각각 확률 데이터에서 확률에 따라 레어도 결정
    private CardRarity GetCardRarity(RewardProbabilityData rewardData)
    {
        int total = rewardData.CommonCardProb + rewardData.RareCardProb + rewardData.UniqueCardProb;

        // 가중치 합 0 = 테이블 미입력. 이전엔 Next(0,0)=0이 분기를 전부 미끄러져
        // 조용히 Unique로 떨어졌다 (엘리트 테이블 미입력 때 실제 발생) — 명시적으로 잡는다.
        if (total <= 0)
        {
            Debug.LogWarning($"[BattleReward] '{rewardData.name}' 카드 가중치 합이 0 — 데이터 미입력. Common으로 폴백.");
            return CardRarity.Common;
        }

        int roll = random.Next(0, total);

        if (roll < rewardData.CommonCardProb)
            return CardRarity.Common;
        else if (roll < rewardData.CommonCardProb + rewardData.RareCardProb)
            return CardRarity.Rare;
        else
            return CardRarity.Unique;
    }
    private ItemRarity GetItemRarity(RewardProbabilityData rewardData)
    {
        int total = rewardData.CommonItemProb + rewardData.UncommonItemProb + rewardData.RareItemProb;

        // 카드 쪽 GetCardRarity와 같은 이유의 가드 — 미입력 시 조용히 Rare로 떨어지던 것 방지
        if (total <= 0)
        {
            Debug.LogWarning($"[BattleReward] '{rewardData.name}' 아이템 가중치 합이 0 — 데이터 미입력. Common으로 폴백.");
            return ItemRarity.Common;
        }

        int roll = random.Next(0, total);

        if (roll < rewardData.CommonItemProb)
            return ItemRarity.Common;
        else if (roll < rewardData.CommonItemProb + rewardData.UncommonItemProb)
            return ItemRarity.Uncommon;
        else
            return ItemRarity.Rare;
    }

    // 카드 풀에서 카드 선택하는 메서드
    // 이미 뽑힌 카드를 제외한 후보에서 선택 -> 한 보상 화면에 같은 카드 중복 방지
    // 후보가 없을 경우(레어도 풀 < 보상 수, 사실상 발생 안 함)에만 중복 허용 폴백 -> 무한 재귀/StackOverflow 방지
    private CardData PickCard(List<CardData> cardPool)
    {
        if (cardPool == null || cardPool.Count == 0)
            return null;

        List<CardData> availableCards = cardPool.FindAll(card => card != null && !cardRewards.Contains(card));
        if (availableCards.Count == 0)
            availableCards = cardPool;

        int index = random.Next(0, availableCards.Count);
        return availableCards[index];
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