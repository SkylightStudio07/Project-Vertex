

public class DamageNullifiedStatus : StatusEffectBase
{
    public DamageNullifiedStatus(int stacks) : base(stacks)
    {
    }

    public override void TickDown() {}

    public override DamageInfo ModifyIncomingDamage(DamageInfo info, BattleState state)
    {
        if (Stacks <= 0) return info;
        if (info.Amount <= 0) return info;

        info.Amount = 0;
        Stacks--;
        return info;
    }
}
