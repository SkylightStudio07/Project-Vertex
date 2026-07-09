using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/TacticalWalkPower")]
public class TacticalWalkPowerEffect : CardEffect
{
    public int dexterityBonus  = 2;
    public int firstAttackBonus = 6;

    public override void Execute(CardContext ctx)
    {
        if (ctx.State?.Player == null) return;
        ctx.State.Player.AddPassive(new DexterityStatus(dexterityBonus));
        ctx.State.Player.AddPassive(new TacticalWalkPassive(firstAttackBonus));
    }
}
