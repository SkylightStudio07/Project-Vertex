using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/AmmoAttackBonusPower")]
public class AmmoAttackBonusPowerEffect : CardEffect
{
    public int bonusDamage = 6;

    public override void Execute(CardContext ctx)
    {
        if (ctx.State?.Player == null) return;
        ctx.State.Player.AddPassive(new AmmoAttackBonusPassive(bonusDamage));
    }
}
