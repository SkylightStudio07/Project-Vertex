public interface IPassiveLogic
{
    void OnBattleStart(CardContext context, ICombatant owner);
    void OnTurnStart(CardContext context, ICombatant owner);
    void OnTurnEnd(CardContext context, ICombatant owner);
    void OnCardPlayed(CardContext context, ICombatant owner);

    // 피해 파이프라인 훅 — DamageCalculator가 attacker → target 순으로 호출
    DamageInfo ModifyOutgoingDamage(DamageInfo info, BattleState state);
    DamageInfo ModifyIncomingDamage(DamageInfo info, BattleState state);

    // 표시 전용 미리보기 훅 — 카드 설명문이 "보정 후 데미지"를 보여줄 때 사용.
    // 반드시 상태 변경 없이(읽기 전용) 동작해야 한다. Modify 쪽이 순수 계산이면
    // 기본 구현(위임)으로 충분하고, 호출 시 내부 상태를 소모하는 패시브만
    // 소모 없는 버전을 따로 구현할 것.
    DamageInfo PreviewOutgoingDamage(DamageInfo info, BattleState state);
    DamageInfo PreviewIncomingDamage(DamageInfo info, BattleState state);

    // 방어도 획득량도 상태뿐 아니라 향후 유물 등 모든 패시브가 같은 경로에서 보정한다.
    int ModifyBlockGain(int amount, ICombatant owner);

    // 피해 후 Context는 Source=패시브 보유자, PrimaryTarget=피해 사건의 상대방이다.
    // 실제 HP 피해량과 발동 패시브는 context.ActualDamage/TriggeringPassive로 전달한다.
    void OnAfterDamageTaken(CardContext context, ICombatant owner);
    void OnAfterDamageDealt(CardContext context, ICombatant owner);
}
