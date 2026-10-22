using UnityEngine;

// 로비 상점 구매 판정과 처리. 화면(LobbyShopView)은 이 결과만 보고 그린다.
//   - 강화 재료: 탈린 → 파츠. 파츠 상품끼리 재고(MetaProgress.ShopPartsStock)를 공유한다
//   - 선물: 탈린 → 선물 수 (동료 화면에서 준다)
//   - 무색 카드: 탈린 → 해금 기록. 한 번만
public static class LobbyShop
{
    public const int MaxQuantity = 99;

    public enum Result
    {
        Ok,
        NotEnoughTalin,
        SoldOut,
        AlreadyOwned,
        InvalidQuantity,
        InvalidItem,
    }

    public static string UnlockKey(ShopItemData item) => "shop:unlock:" + item.itemId;

    public static bool IsOwned(ShopItemData item)
        => item != null && item.Stock == ShopStockRule.OneTime && MetaProgress.HasMilestone(UnlockKey(item));

    // 남은 재고. 제한 없으면 MaxQuantity
    public static int Remaining(ShopItemData item)
    {
        if (item == null) return 0;
        return item.Stock switch
        {
            ShopStockRule.StoryRestock => MetaProgress.ShopPartsStock,
            ShopStockRule.OneTime => IsOwned(item) ? 0 : 1,
            _ => MaxQuantity,
        };
    }

    // 지금 살 수 있는 최대 개수 (재고 · 탈린 · 한도 중 작은 값)
    public static int MaxBuyable(ShopItemData item)
    {
        if (item == null) return 0;
        int byTalin = item.price > 0 ? MetaProgress.Talin / item.price : MaxQuantity;
        return Mathf.Clamp(Mathf.Min(Remaining(item), byTalin), 0, MaxQuantity);
    }

    public static Result Check(ShopItemData item, int quantity)
    {
        if (item == null || string.IsNullOrEmpty(item.itemId)) return Result.InvalidItem;
        if (quantity <= 0 || quantity > MaxQuantity) return Result.InvalidQuantity;
        if (IsOwned(item)) return Result.AlreadyOwned;
        if (item.Stock == ShopStockRule.OneTime && quantity > 1) return Result.InvalidQuantity;
        if (Remaining(item) < quantity) return Result.SoldOut;
        if (MetaProgress.Talin < item.price * quantity) return Result.NotEnoughTalin;
        return Result.Ok;
    }

    public static Result Buy(ShopItemData item, int quantity = 1)
    {
        var result = Check(item, quantity);
        if (result != Result.Ok) return result;

        if (item.Stock == ShopStockRule.StoryRestock && !MetaProgress.TryTakeShopParts(quantity)) return Result.SoldOut;
        if (!MetaProgress.TrySpendTalin(item.price * quantity)) return Result.NotEnoughTalin;

        switch (item.category)
        {
            case ShopCategory.Material: MetaProgress.AddParts(item.partsAmount * quantity); break;
            case ShopCategory.Gift: MetaProgress.AddGift(item.itemId, quantity); break;
            case ShopCategory.ColorlessCard: MetaProgress.MarkMilestone(UnlockKey(item)); break;
        }
        Debug.Log($"<color=#0DB8F2>[LobbyShop] {item.displayName} ×{quantity} 구매 (-{item.price * quantity} 탈린, 보유 {MetaProgress.Talin})</color>");
        return Result.Ok;
    }

    // 화면에 띄울 짧은 사유
    public static string Describe(Result result) => result switch
    {
        Result.NotEnoughTalin => "탈린이 부족해",
        Result.SoldOut => "재고가 없어",
        Result.AlreadyOwned => "이미 해금했어",
        _ => "",
    };
}
