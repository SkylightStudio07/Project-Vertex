using UnityEngine;

// 전술보행용. 플레이 시 즉시 민첩+2 부여, TacticalWalkPassive 등록.
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
