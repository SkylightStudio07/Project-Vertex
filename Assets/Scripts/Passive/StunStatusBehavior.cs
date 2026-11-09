// 스택 하나당 적의 행동 기회를 한 번 소비한다. 패턴 순서는 진행하지 않는다.
[System.Serializable]
public sealed class StunStatusBehavior : StatusBehavior
{
    public EnemyAction skippedAction;
}
