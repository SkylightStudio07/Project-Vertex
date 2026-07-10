// Q.E.D 카드용. 플레이 시 TimedBlockPassive(시한부 상태 효과)를 플레이어에 등록한다.
// 실제 방어도 부여는 매 턴 시작 시 TickPassives()가 패시브의 OnTurnStart를 호출하며 일어난다.
// 사용한 턴에는 방어도가 오르지 않음 — "다음 턴부터 N턴" 동작이 의도.
[System.Serializable]
public class TimedBlockEffect : CardEffect
{
    public int turns = 3;
    public int blockPerTurn = 6;

    public override void Execute(CardContext ctx)
    {
        if (ctx.State?.Player == null) return;
        ctx.State.Player.AddPassive(new TimedBlockPassive(turns, blockPerTurn));
    }
}
