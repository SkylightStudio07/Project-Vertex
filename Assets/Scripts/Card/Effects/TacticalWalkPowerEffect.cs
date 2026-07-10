using UnityEngine;

// 전술보행 카드용. 패시브 두 개를 한 번에 등록한다:
// 1) DexterityStatus(+dexterityBonus) — 즉시 발동하는 영구 민첩 (방어도 획득량 증가)
// 2) TacticalWalkPassive — 매 턴 첫 공격에 +firstAttackBonus 데미지
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
