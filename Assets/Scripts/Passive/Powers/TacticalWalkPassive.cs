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
