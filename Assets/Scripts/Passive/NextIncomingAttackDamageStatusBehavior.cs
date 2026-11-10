// 압박감: 다음 적 공격의 모든 타격에 적용하고, 행동 전체가 끝난 뒤 소모한다.
[System.Serializable]
public sealed class NextIncomingAttackDamageStatusBehavior : StatusBehavior
{
    public override DamageInfo ModifyIncomingDamage(StatusInstance status, DamageInfo info, BattleState state, bool preview)
    {
        // 환경 피해(중독·턴 종료 카드 등)는 공격으로 취급하지 않는다.
        if (info.Source is not EnemyInstance || info.Amount <= 0) return info;
        info.Amount += status.Stacks;
        if (!preview) status.MarkUsedThisTurn(this);
        return info;
    }

    public override void OnIncomingEnemyActionEnd(StatusInstance status, CardContext context, EnemyAction action)
    {
        // 방어도로 전부 막아도 공격은 소모한다. 미리보기·비공격·기절은 소모하지 않는다.
        if (status.WasUsedThisTurn(this)) status.RemoveAll();
    }
}
