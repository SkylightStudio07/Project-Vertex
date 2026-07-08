using UnityEngine;

// Q.E.D용. 플레이 시 TimedBlockPassive를 플레이어에 등록한다.
// 다음 turns턴 동안 매 턴 시작 시 blockPerTurn만큼 방어도를 부여한다.
[CreateAssetMenu(menuName = "Cards/Effects/TimedBlock")]
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
