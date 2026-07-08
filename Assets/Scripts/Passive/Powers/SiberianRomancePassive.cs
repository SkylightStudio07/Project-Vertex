// 시베리안 로망스 파워 효과.
// 매 턴 시작 시 힘을 strengthPerTurn만큼 누적하고, 민첩을 dexPenalty만큼 감소시킨다.
public class SiberianRomancePassive : PowerPassiveBase
{
    private readonly int _strengthPerTurn;
    private readonly int _dexPenalty;

    public SiberianRomancePassive(int strengthPerTurn, int dexPenalty)
    {
        _strengthPerTurn = strengthPerTurn;
        _dexPenalty      = dexPenalty;
    }

    public override void OnTurnStart(BattleState state, ICombatant owner)
    {
        if (owner is not PlayerCombatant player) return;
        player.AddPassive(new StrengthStatus(_strengthPerTurn));
        player.AddPassive(new DexterityStatus(-_dexPenalty));
    }
}
