// 무기고 카드 영구 강화 (로비). 강화한 카드는 런에서 처음부터 강화된 상태로 쓴다 (DeckManager.CopyForRun — 시작 덱·보상·상점·변화·훈련장 모두).
// 비용: 탈린 + 파츠. 파츠는 무기 강화보다 싸게 둔다. 강화 기록은 MetaProgress 이정표 "card:up:<카드 키>".
public static class CardUpgrades
{
    public const int TalinCost = 80;
    public const int PartsCost = 3;

    public enum Result { Ok, AlreadyUpgraded, NotUpgradable, NotEnoughTalin, NotEnoughParts, InvalidCard }

    public static string Key(CardData card) => "card:up:" + PlayerRecord.KeyOf(card);

    public static bool IsUpgraded(CardData card) => card != null && MetaProgress.HasMilestone(Key(card));

    public static Result Check(CardData card)
    {
        if (card == null) return Result.InvalidCard;
        if (!card.CanUpgrade) return Result.NotUpgradable;
        if (IsUpgraded(card)) return Result.AlreadyUpgraded;
        if (MetaProgress.Talin < TalinCost) return Result.NotEnoughTalin;
        if (MetaProgress.Parts < PartsCost) return Result.NotEnoughParts;
        return Result.Ok;
    }

    public static Result TryUpgrade(CardData card)
    {
        var result = Check(card);
        if (result != Result.Ok) return result;
        if (!MetaProgress.TrySpendTalin(TalinCost)) return Result.NotEnoughTalin;
        if (!MetaProgress.TrySpendParts(PartsCost)) { MetaProgress.AddTalin(TalinCost); return Result.NotEnoughParts; }
        MetaProgress.MarkMilestone(Key(card));
        DeckManager.ApplyArmoryUpgrade(card);
        return Result.Ok;
    }
}
