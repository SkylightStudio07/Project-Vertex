// 강화: 가하는 피해에 스택만큼 추가. 영구 효과이므로 TickDown 없음.
public class StrengthStatus : StatusEffectBase
{
    public StrengthStatus(int stacks) : base(stacks) { }

    public override void TickDown() { } // 영구 — 감소하지 않음

    // 음수 스택(힘 감소)도 유효한 상태 — 정확히 0일 때만 정리 (DexterityStatus와 동일 규칙).
    // 지금은 힘을 음수로 주는 카드가 없지만, 힘 감소 디버프가 생기면 기본 판정(<= 0)으로는 즉시 제거된다.
    public override bool IsExpired => Stacks == 0;

    public override DamageInfo ModifyOutgoingDamage(DamageInfo info, BattleState state)
    {
        info.Amount += Stacks;
        return info;
    }
}
