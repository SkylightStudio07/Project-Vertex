// Q.E.D 효과 — N턴 동안 매 턴 시작 시 방어도를 blockPerTurn만큼 부여한다.
// Stacks = 남은 턴 수. TickDown으로 매 턴 1씩 감소해 자연 만료된다.
// 주의: 같은 타입이면 TryMerge가 Stacks를 합산하므로 Q.E.D를 두 번 쓰면
// "6씩 6턴"이 된다 ("12씩 3턴"이 아님). 기획 의도와 다르면 TryMerge 정책 조정 필요.
public class TimedBlockPassive : StatusEffectBase
{
    private readonly int _blockPerTurn;

    public TimedBlockPassive(int turns, int blockPerTurn) : base(turns)
        => _blockPerTurn = blockPerTurn;

    public override void OnTurnStart(BattleState state, ICombatant owner)
        => owner.AddBlock(_blockPerTurn);
}
