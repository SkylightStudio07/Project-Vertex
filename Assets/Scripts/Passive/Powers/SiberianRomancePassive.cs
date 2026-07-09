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
