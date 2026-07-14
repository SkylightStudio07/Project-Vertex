// 시베리안 로망스 파워 — 매 턴 시작 시 힘/민첩 상태를 새로 부여한다.
// 직접 수치를 들고 있지 않고 StrengthStatus/DexterityStatus를 반복 등록 —
// AddPassive의 TryMerge가 같은 타입끼리 스택을 합산해줘서 자연스럽게 누적된다.
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
