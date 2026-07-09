using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/AmmoPerTurnPower")]
public class AmmoPerTurnPowerEffect : CardEffect
{
    public int ammoPerTurn = 1;

    public override void Execute(CardContext ctx)
    {
        if (ctx.State?.Player == null) return;
        ctx.State.Player.AddPassive(new AmmoPerTurnPassive(ammoPerTurn));
    }
}
