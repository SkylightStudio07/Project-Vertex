[System.Serializable]
public class ApplyStatusOnHitPassive : PowerPassiveBase
{
    private readonly StatusType _statusType;
    private readonly int _amount;

    public ApplyStatusOnHitPassive(StatusType statusType, int amount)
    {
        _statusType = statusType;
        _amount = amount;
    }

    public override void OnAfterDamageDealt(int actualDamage, ICombatant target, BattleState state, ICombatant owner)
    {
        if (actualDamage <= 0) return;
        if (target is not EnemyInstance enemy || enemy.IsDead) return;

        var passive = CreatePassive();
        if (passive != null)
            enemy.AddPassive(passive);
    }

    private StatusEffectBase CreatePassive() => _statusType switch
    {
        StatusType.Vulnerable => new VulnerableStatus(System.Math.Abs(_amount)),
        StatusType.Weak => new WeakStatus(System.Math.Abs(_amount)),
        StatusType.Strength => new StrengthStatus(-System.Math.Abs(_amount)),
        StatusType.Dexterity => new DexterityStatus(-System.Math.Abs(_amount)),
        StatusType.Poison => new PoisonStatus(System.Math.Abs(_amount)),
        _ => null,
    };
}
