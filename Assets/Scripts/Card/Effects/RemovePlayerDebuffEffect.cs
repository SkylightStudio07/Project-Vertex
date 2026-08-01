[System.Serializable]
public class RemovePlayerDebuffEffect : CardEffect
{
    public StatusType statusType;
    public int amount = 1;

    public override void Execute(CardContext context)
    {
        var player = context.State?.Player;
        if (player == null) return;

        for (int i = 0; i < player.Passives.Count; i++)
        {
            if (player.Passives[i] is not StatusEffectBase status) continue;
            if (!MatchesStatusType(status, statusType)) continue;
            if (!IsDebuff(status)) continue;

            status.ReduceMagnitude(amount);
            player.RemoveExpiredPassives();
            return;
        }
    }

    // 제거하려는 Passive가 현재 플레이어에게 존재하는지 체크
    private static bool MatchesStatusType(StatusEffectBase status, StatusType type)
    {
        return type switch
        {
            StatusType.Weak => status is WeakStatus,
            StatusType.Vulnerable => status is VulnerableStatus,
            StatusType.Poison => status is PoisonStatus,
            StatusType.Strength => status is StrengthStatus,
            StatusType.Dexterity => status is DexterityStatus,
            StatusType.DamageNullified => status is DamageNullifiedStatus,
            _ => false,
        };
    }

    // 제거하려는 Passive가 디버프인지 체크
    private static bool IsDebuff(StatusEffectBase status)
    {
        return status switch
        {
            WeakStatus => true,
            VulnerableStatus => true,
            PoisonStatus => true,
            BurnStatus => true,
            StrengthStatus strength => strength.Stacks < 0,
            DexterityStatus dexterity => dexterity.Stacks < 0,
            _ => false,
        };
    }
}
