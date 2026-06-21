// 강화: 가하는 피해에 스택만큼 추가. 영구 효과이므로 TickDown 없음.
public class StrengthStatus : StatusEffectBase
{
    public StrengthStatus(int stacks) : base(stacks) { }

    public override void TickDown() { } // 영구 — 감소하지 않음

    public override DamageInfo ModifyOutgoingDamage(DamageInfo info, BattleState state)
    {
        info.Amount += Stacks;
        return info;
    }
}
