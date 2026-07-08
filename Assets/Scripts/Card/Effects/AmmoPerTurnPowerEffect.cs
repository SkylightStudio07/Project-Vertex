using UnityEngine;

// R&C 컴퍼니 스피드로더용. 플레이 시 AmmoPerTurnPassive를 플레이어에 등록한다.
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
