using UnityEngine;

// 죄와 벌용. 플레이 시 AmmoAttackBonusPassive를 플레이어에 등록한다.
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
