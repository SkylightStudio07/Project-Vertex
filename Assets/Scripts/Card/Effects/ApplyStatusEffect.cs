[System.Serializable]
public class ApplyStatusEffect : CardEffect
{
    public StatusType statusType;
    public int        amount;
    public TargetType target;

    public override void Execute(CardContext context)
    {
        if (context.State == null) return;

        bool isActingEnemy = context.ActingEnemy != null;

        switch (target)
        {
            case TargetType.SingleEnemy:
                if (context.Target != null && !context.Target.IsDead)
                    ApplyTo(context.Target);
                break;

            case TargetType.AllEnemies:
                foreach (var e in context.AllEnemies)
                    if (!e.IsDead) ApplyTo(e);
                break;

            case TargetType.Self:
                var selfPassive = CreatePassive();
                if(selfPassive == null) return;
                if (isActingEnemy) context.ActingEnemy?.AddPassive(selfPassive);
                else context.State.Player?.AddPassive(selfPassive);
                break;
        }
    }

    private void ApplyTo(EnemyInstance enemy)
    {
        var passive = CreatePassive();
        if (passive != null) enemy.AddPassive(passive);
    }

    private StatusEffectBase CreatePassive() => statusType switch
    {
        StatusType.Vulnerable => new VulnerableStatus(amount),
        StatusType.Weak       => new WeakStatus(amount),
        StatusType.Strength   => new StrengthStatus(amount),
        StatusType.Poison     => new PoisonStatus(amount),
        StatusType.DamageNullified => new DamageNullifiedStatus(amount),
        _                     => null,
    };
}
