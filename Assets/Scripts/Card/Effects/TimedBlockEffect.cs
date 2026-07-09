using UnityEngine;

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
