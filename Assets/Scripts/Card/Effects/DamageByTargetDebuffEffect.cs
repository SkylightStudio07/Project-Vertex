using System.Collections.Generic;

[System.Serializable]
public class DamageByTargetDebuffEffect : CardEffect
{
    public StatusType statusType;
    public int damagePerStack = 1;
    public TargetType targetType = TargetType.SingleEnemy;

    public override void Execute(CardContext context)
    {
        if (context.State == null) return;

        switch (targetType)
        {
            case TargetType.SingleEnemy:
                if (context.Target != null && !context.Target.IsDead)
                    DealDamage(context, context.Target);
                break;

            case TargetType.AllEnemies:
                if (context.AllEnemies == null) return;
                foreach (var enemy in context.AllEnemies)
                    if (enemy != null && !enemy.IsDead) DealDamage(context, enemy);
                break;

            case TargetType.RandomEnemy:
                DealDamageToRandomEnemy(context);
                break;
        }
    }

    private void DealDamageToRandomEnemy(CardContext context)
    {
        if (context.AllEnemies == null) return;

        var aliveEnemies = new List<EnemyInstance>();
        foreach (var enemy in context.AllEnemies)
            if (enemy != null && !enemy.IsDead) aliveEnemies.Add(enemy);

        if (aliveEnemies.Count == 0) return;

        int index = context.Battle != null
            ? context.Battle.Rnd.Next(0, aliveEnemies.Count)
            : UnityEngine.Random.Range(0, aliveEnemies.Count);

        DealDamage(context, aliveEnemies[index]);
    }

    private void DealDamage(CardContext context, EnemyInstance enemy)
    {
        int debuffAmount = GetDebuffMagnitude(enemy);
        if (debuffAmount <= 0) return;

        int damage = debuffAmount * damagePerStack;
        if (damage <= 0) return;

        bool isAmmoAttack = context.Card != null && context.Card.AmmoCost > 0;
        DamageCalculator.Resolve(
            new DamageInfo(damage, context.Attacker, false, isAmmoAttack),
            enemy,
            context.State);
    }

    private int GetDebuffMagnitude(EnemyInstance enemy)
    {
        foreach (var passive in enemy.Passives)
        {
            if (passive is not StatusEffectBase status) continue;
            if (!MatchesStatusType(status, statusType)) continue;
            if (!IsDebuff(status)) continue;

            return System.Math.Abs(status.Stacks);
        }

        return 0;
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

    private static bool IsDebuff(StatusEffectBase status)
    {
        return status switch
        {
            WeakStatus => true,
            VulnerableStatus => true,
            PoisonStatus => true,
            StrengthStatus strength => strength.Stacks < 0,
            DexterityStatus dexterity => dexterity.Stacks < 0,
            _ => false,
        };
    }
}
