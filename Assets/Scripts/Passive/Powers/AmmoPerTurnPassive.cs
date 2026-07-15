// R&C 컴퍼니 스피드로더 파워 효과.
// 매 플레이어 턴 시작 시 탄약을 amountPerTurn만큼 추가한다.
public class AmmoPerTurnPassive : PowerPassiveBase
{
    private readonly int _amountPerTurn;

    public AmmoPerTurnPassive(int amountPerTurn) => _amountPerTurn = amountPerTurn;

    public override void OnTurnStart(BattleState state, ICombatant owner)
        => state.Ammo += _amountPerTurn;
}
