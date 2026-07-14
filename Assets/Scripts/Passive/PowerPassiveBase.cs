// 전투 종료까지 영구 유지되는 파워 패시브 기반 클래스.
// StatusEffectBase를 상속하지 않으므로 TickPassives()의 만료 제거 루프가 적용되지 않는다.
// 즉, 별도 해제 없이 전투 내내 지속된다.
public abstract class PowerPassiveBase : IPassiveLogic
{
    public virtual void OnBattleStart(BattleState state, ICombatant owner) { }
    public virtual void OnTurnStart(BattleState state, ICombatant owner) { }
    public virtual void OnCardPlayed(CardContext ctx, ICombatant owner) { }
    public virtual DamageInfo ModifyOutgoingDamage(DamageInfo info, BattleState state) => info;
    public virtual DamageInfo ModifyIncomingDamage(DamageInfo info, BattleState state) => info;
    public virtual void OnAfterDamageTaken(int actualDamage, BattleState state, ICombatant owner) { }

    // 표시용 미리보기 — 기본은 Modify 위임.
    // Modify에서 내부 상태를 소모하는 패시브는 반드시 소모 없는 버전으로 override할 것.
    public virtual DamageInfo PreviewOutgoingDamage(DamageInfo info, BattleState state)
        => ModifyOutgoingDamage(info, state);
    public virtual DamageInfo PreviewIncomingDamage(DamageInfo info, BattleState state)
        => ModifyIncomingDamage(info, state);
}
