// Q.E.D 효과 — N턴 동안 매 턴 시작 시 방어도를 blockPerTurn만큼 부여한다.
// Stacks = 남은 턴 수. TickDown으로 매 턴 1씩 감소해 자연 만료된다.
public class TimedBlockPassive : StatusEffectBase
{
    private readonly int _blockPerTurn;

    public TimedBlockPassive(int turns, int blockPerTurn) : base(turns)
        => _blockPerTurn = blockPerTurn;

    public override void OnTurnStart(BattleState state, ICombatant owner)
        => owner.AddBlock(_blockPerTurn);
}
