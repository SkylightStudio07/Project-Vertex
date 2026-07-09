using UnityEngine;

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
