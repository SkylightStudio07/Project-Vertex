// 전술보행 파워 효과.
// 매 턴 플레이어가 처음 가하는 공격 데미지에 bonusDamage를 추가한다.
// 턴 시작마다 플래그를 리셋해 '첫 번째 공격'을 판정한다.
public class TacticalWalkPassive : PowerPassiveBase
{
    private readonly int _bonusDamage;
    private bool _firstAttackUsed;

    public TacticalWalkPassive(int bonusDamage) => _bonusDamage = bonusDamage;

    public override void OnTurnStart(BattleState state, ICombatant owner)
        => _firstAttackUsed = false;

    public override DamageInfo ModifyOutgoingDamage(DamageInfo info, BattleState state)
    {
        if (_firstAttackUsed) return info;
        _firstAttackUsed = true;
        info.Amount += _bonusDamage;
        return info;
    }
}
