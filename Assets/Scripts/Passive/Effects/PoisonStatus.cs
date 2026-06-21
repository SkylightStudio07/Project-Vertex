// 독: 매 턴 stacks만큼 피해 후 스택 1 감소. (향후 구현 예정)
public class PoisonStatus : StatusEffectBase
{
    public PoisonStatus(int stacks) : base(stacks) { }

    public override void OnTurnStart(BattleState state, ICombatant owner)
    {
        // TODO: owner에게 Stacks만큼 관통 피해 → TickDown()
    }
}
