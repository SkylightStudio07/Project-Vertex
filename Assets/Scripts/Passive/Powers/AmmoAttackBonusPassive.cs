// 죄와 벌 파워 효과.
// 탄약 코스트가 있는 공격 카드의 데미지에 bonusDamage를 추가한다.
// DamageInfo.IsAmmoAttack 플래그로 대상 카드를 구분한다.
public class AmmoAttackBonusPassive : PowerPassiveBase
{
    private readonly int _bonusDamage;

    public AmmoAttackBonusPassive(int bonusDamage) => _bonusDamage = bonusDamage;

    public override DamageInfo ModifyOutgoingDamage(DamageInfo info, BattleState state)
    {
        if (info.IsAmmoAttack)
            info.Amount += _bonusDamage;
        return info;
    }
}
