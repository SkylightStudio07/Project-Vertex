public interface IPassiveLogic
{
    void OnBattleStart(BattleState state, ICombatant owner);
    void OnTurnStart(BattleState state, ICombatant owner);
    void OnCardPlayed(CardContext ctx, ICombatant owner);

    // 피해 파이프라인 훅 — DamageCalculator가 attacker → target 순으로 호출
    DamageInfo ModifyOutgoingDamage(DamageInfo info, BattleState state);
    DamageInfo ModifyIncomingDamage(DamageInfo info, BattleState state);

    // 표시 전용 미리보기 훅 — 카드 설명문이 "보정 후 데미지"를 보여줄 때 사용.
    // 반드시 상태 변경 없이(읽기 전용) 동작해야 한다. Modify 쪽이 순수 계산이면
    // 기본 구현(위임)으로 충분하고, 호출 시 내부 상태를 소모하는 패시브만
    // 소모 없는 버전을 따로 구현할 것 (공격 측: TacticalWalkPassive, 방어 측: DamageNullifiedStatus 참고).
    DamageInfo PreviewOutgoingDamage(DamageInfo info, BattleState state);
    DamageInfo PreviewIncomingDamage(DamageInfo info, BattleState state);

    void OnAfterDamageTaken(int actualDamage, BattleState state, ICombatant owner);
    void OnAfterDamageDealt(int actualDamage, ICombatant target, BattleState state, ICombatant owner);
}
