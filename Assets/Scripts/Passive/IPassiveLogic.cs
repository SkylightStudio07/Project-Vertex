public interface IPassiveLogic
{
    void OnBattleStart(BattleState state, ICombatant owner);
    void OnTurnStart(BattleState state, ICombatant owner);
    void OnCardPlayed(CardContext ctx, ICombatant owner);

    // 피해 파이프라인 훅 — DamageCalculator가 attacker → target 순으로 호출
    DamageInfo ModifyOutgoingDamage(DamageInfo info, BattleState state);
    DamageInfo ModifyIncomingDamage(DamageInfo info, BattleState state);

    void OnAfterDamageTaken(int actualDamage, BattleState state, ICombatant owner);
}
