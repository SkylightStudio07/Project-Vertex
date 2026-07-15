// 전술보행 파워 — 매 턴 플레이어의 첫 공격에 bonusDamage를 추가한다.
// _firstAttackUsed 플래그를 턴 시작마다 리셋하고 첫 ModifyOutgoingDamage에서 소모하는 방식.
// 연타 카드(hitCount>1)는 타격마다 Resolve를 거치므로 첫 1타에만 보너스가 붙는다.
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

    // 미리보기: 플래그를 소모하지 않고 "지금 공격하면 보너스가 붙는가"만 반영.
    // 기본 위임을 그대로 쓰면 설명문을 그리는 것만으로 첫 공격 보너스가 소모되는 버그가 된다.
    public override DamageInfo PreviewOutgoingDamage(DamageInfo info, BattleState state)
    {
        if (!_firstAttackUsed) info.Amount += _bonusDamage;
        return info;
    }
}
