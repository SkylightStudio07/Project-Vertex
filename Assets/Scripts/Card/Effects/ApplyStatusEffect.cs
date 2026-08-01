using System.Collections.Generic;

[System.Serializable]
public class ApplyStatusEffect : CardEffect
{
    public StatusType statusType;
    public StatusStackOperation operation = StatusStackOperation.Add;
    public int        amount;
    public TargetType target;

    public override void Execute(CardContext context)
    {
        if (context.State == null) return;

        switch (target)
        {
            case TargetType.SingleEnemy:
                if (context.Target != null && !context.Target.IsDead)
                    ApplyToEnemy(context.Target);
                break;

            case TargetType.AllEnemies:
                if (context.AllEnemies == null) return;
                foreach (var e in context.AllEnemies)
                    if (e != null && !e.IsDead) ApplyToEnemy(e);
                break;

            case TargetType.RandomEnemy:
                ApplyToRandomEnemy(context);
                break;

            case TargetType.Self:
                ApplyToPlayer(context.State.Player);
                break;
        }
    }

    private void ApplyToRandomEnemy(CardContext context)
    {
        if (context.AllEnemies == null) return;

        var aliveEnemies = new List<EnemyInstance>();
        foreach (var enemy in context.AllEnemies)
            if (enemy != null && !enemy.IsDead) aliveEnemies.Add(enemy);

        if (aliveEnemies.Count == 0) return;

        int index = context.Battle != null
            ? context.Battle.Rnd.Next(0, aliveEnemies.Count)
            : UnityEngine.Random.Range(0, aliveEnemies.Count);

        ApplyToEnemy(aliveEnemies[index]);
    }

    private void ApplyToPlayer(PlayerCombatant player)
    {
        if (player == null) return;

        if (operation == StatusStackOperation.Add)
        {
            var passive = CreatePassive();
            if (passive != null) player.AddPassive(passive);
            return;
        }

        ModifyExisting(player);
    }

    private void ApplyToEnemy(EnemyInstance enemy)
    {
        if (enemy == null) return;

        if (operation == StatusStackOperation.Add)
        {
            var passive = CreatePassive();
            if (passive != null) enemy.AddPassive(passive);
            return;
        }

        ModifyExisting(enemy);
    }

    private void ModifyExisting(ICombatant combatant)
    {
        foreach (var passive in combatant.Passives)
        {
            if (passive is not StatusEffectBase status) continue;
            if (!MatchesStatusType(status, statusType)) continue;

            ApplyOperation(status);
            combatant.RemoveExpiredPassives();
            return;
        }
    }

    private void ApplyOperation(StatusEffectBase status)
    {
        switch (operation)
        {
            case StatusStackOperation.Multiply:
                status.MultiplyMagnitude(amount);
                break;

            case StatusStackOperation.Set:
                status.SetMagnitude(amount);
                break;
        }
    }

    private static bool MatchesStatusType(StatusEffectBase status, StatusType type)
    {
        return type switch
        {
            StatusType.Weak => status is WeakStatus,
            StatusType.Vulnerable => status is VulnerableStatus,
            StatusType.Poison => status is PoisonStatus,
            StatusType.Strength => status is StrengthStatus,
            StatusType.Dexterity => status is DexterityStatus,
            StatusType.DamageNullified => status is DamageNullifiedStatus,
            _ => false,
        };
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

public enum StatusStackOperation
{
    Add,
    Multiply,
    Set,
}
