using UnityEngine;

// 시베리안 로망스용. 플레이 시 SiberianRomancePassive를 등록한다.
// 매 턴 힘+strengthPerTurn, 민첩-dexPenalty 누적.
[CreateAssetMenu(menuName = "Cards/Effects/SiberianRomancePower")]
public class SiberianRomancePowerEffect : CardEffect
{
    public int strengthPerTurn = 2;
    public int dexPenalty      = 1;

    public override void Execute(CardContext ctx)
    {
        if (ctx.State?.Player == null) return;
        ctx.State.Player.AddPassive(new SiberianRomancePassive(strengthPerTurn, dexPenalty));
    }
}
