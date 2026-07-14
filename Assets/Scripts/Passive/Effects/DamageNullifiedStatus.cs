// 버퍼: 받는 공격을 스택당 1회 완전 무효화한다 (연타는 첫 타만 막히고 나머지는 들어옴).
// 턴 경과로 사라지지 않고(TickDown 무효), 피격 시에만 스택이 소모된다.
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

    // 미리보기: 스택을 소모하지 않고 "지금 맞으면 무효화되는가"만 반영.
    // 기본 위임을 그대로 쓰면 타겟팅 호버만으로 버퍼가 소모되는 버그가 된다.
    public override DamageInfo PreviewIncomingDamage(DamageInfo info, BattleState state)
    {
        if (Stacks > 0 && info.Amount > 0) info.Amount = 0;
        return info;
    }
}
