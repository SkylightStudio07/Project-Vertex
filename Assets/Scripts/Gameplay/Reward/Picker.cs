using System.Collections.Generic;
using UnityEngine;
using static CardData;
using static ItemData;

// 카드&아이템 랜덤 생성 범용 정적 클래스
public static class Picker
{
    // 가중치 기반 카드 레어도 추첨. 가중치 합이 0이면(테이블 미입력) Common 폴백.
    public static CardRarity PickCardRarity(int commonProb, int rareProb, int uniqueProb, System.Random rng, string context = "")
    {
        int total = commonProb + rareProb + uniqueProb;
        if (total <= 0)
        {
            Debug.LogWarning($"[Picker] '{context}' 카드 가중치 합이 0 — 데이터 미입력. Common으로 폴백.");
            return CardRarity.Common;
        }

        int roll = rng.Next(0, total);
        if (roll < commonProb) return CardRarity.Common;
        if (roll < commonProb + rareProb) return CardRarity.Rare;
        return CardRarity.Unique;
    }

    public static ItemRarity PickItemRarity(int commonProb, int uncommonProb, int rareProb, System.Random rng, string context = "")
    {
        int total = commonProb + uncommonProb + rareProb;
        if (total <= 0)
        {
            Debug.LogWarning($"[Picker] '{context}' 아이템 가중치 합이 0 — 데이터 미입력. Common으로 폴백.");
            return ItemRarity.Common;
        }

        int roll = rng.Next(0, total);
        if (roll < commonProb) return ItemRarity.Common;
        if (roll < commonProb + uncommonProb) return ItemRarity.Uncommon;
        return ItemRarity.Rare;
    }

    // 지정 레어도 풀에서 카드 한 장. 풀이 비면 한 단계 낮은 레어도로 폴백
    public static CardData PickCard(
        Dictionary<CardRarity, List<CardData>> pools, CardRarity rarity,
        System.Random rng, List<CardData> exclude = null)
    {
        // exclude의 카드를 제외한 풀에서 탐색
        for (int r = (int)rarity; r >= (int)CardRarity.Common; r--)
        {
            if (!pools.TryGetValue((CardRarity)r, out var pool) || pool == null || pool.Count == 0)
                continue;

            var candidates = pool.FindAll(c => c != null && (exclude == null || !exclude.Contains(c)));
            if (candidates.Count > 0) 
            { 
                return candidates[rng.Next(0, candidates.Count)];
            }
        }
        // 중복 허용 폴백
        for(int r = (int)rarity; r >= (int)CardRarity.Common; r--)
        {
            if (!pools.TryGetValue((CardRarity)r, out var pool) || pool == null) continue;

            var all = pool.FindAll(c => c != null);
            if (all.Count > 0) return all[rng.Next(0, all.Count)];
        }
        return null;
    }

    // 레어도 + ItemGetType 두 조건을 만족하는 아이템 하나.
    // exclude로 같은 화면 중복 방지. 조건 만족 후보가 없으면 null.
    public static ItemData PickItem(
        List<ItemData> pool, ItemRarity rarity, ItemGetType getType,
        System.Random rng, List<ItemData> exclude = null)
    {
        if (pool == null) return null;

        var filtered = pool.FindAll(item =>
            item != null && item.Rarity == rarity && (item.ItemTypes & getType) != 0 &&
            (exclude == null || !exclude.Contains(item)));

        if (filtered.Count == 0) return null;
        return filtered[rng.Next(0, filtered.Count)];
    }
}
