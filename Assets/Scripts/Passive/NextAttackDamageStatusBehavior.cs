// 활력: 다음 공격 행동의 모든 타격에 더하고, 행동 전체가 끝나면 소모한다.
[System.Serializable]
public sealed class NextAttackDamageStatusBehavior : StatusBehavior
{
    public override DamageInfo ModifyOutgoingDamage(StatusInstance status, DamageInfo info, BattleState state, bool preview)
    {
        if (info.Amount > 0) info.Amount += status.Stacks;
        return info;
    }

    public override void OnEnemyActionEnd(StatusInstance status, CardContext context, EnemyAction action)
    {
        if (action != null && action.intentType == IntentType.Attack) status.RemoveAll();
    }
}
