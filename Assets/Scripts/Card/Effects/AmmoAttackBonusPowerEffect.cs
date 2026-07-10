// 죄와 벌 카드용. "탄약 코스트가 있는 공격"의 데미지를 올리는 영구 패시브를 등록한다.
// 대상 카드 판별은 DamageEffect가 세팅하는 DamageInfo.IsAmmoAttack 플래그에 의존 —
// 탄약 공격 카드를 새로 만들 때 DamageEffect를 안 거치면 이 보너스가 적용되지 않으니 주의.
[System.Serializable]
public class AmmoAttackBonusPowerEffect : CardEffect
{
    public int bonusDamage = 6;

    public override void Execute(CardContext ctx)
    {
        if (ctx.State?.Player == null) return;
        ctx.State.Player.AddPassive(new AmmoAttackBonusPassive(bonusDamage));
    }
}
