using UnityEngine;

// 시베리안 로망스 카드용. 매 턴 힘+strengthPerTurn / 민첩-dexPenalty가 누적되는 영구 패시브를 등록한다.
// 사용한 턴에는 변화 없음 — 누적은 다음 턴 시작(TickPassives)부터 시작된다.
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
