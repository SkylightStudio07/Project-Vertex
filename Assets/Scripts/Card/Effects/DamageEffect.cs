using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Damage")]
public class DamageEffect : CardEffect
{
    public int amount;
    public int hitCount = 1;
    public TargetType targetType = TargetType.SingleEnemy;

    public override void Execute(CardContext context)
    {
        if (context.State == null) return;

        ICombatant attacker = context.Attacker;

        switch (targetType)
        {
            case TargetType.SingleEnemy:
                ICombatant single = ResolveSingleTarget(context);
                if (single == null || single.IsDead) return;
                for (int i = 0; i < hitCount; i++)
                    DamageCalculator.Resolve(new DamageInfo(amount, attacker), single, context.State);
                break;

            case TargetType.AllEnemies:
                foreach (var enemy in context.AllEnemies)
                {
                    if (enemy.IsDead) continue;
                    for (int i = 0; i < hitCount; i++)
                        DamageCalculator.Resolve(new DamageInfo(amount, attacker), enemy, context.State);
                }
                break;

            case TargetType.RandomEnemy:
                var alive = new List<EnemyInstance>();
                foreach (var e in context.AllEnemies)
                    if (!e.IsDead) alive.Add(e);
                if (alive.Count == 0) return;
                for (int i = 0; i < hitCount; i++)
                {
                    var t = alive[Random.Range(0, alive.Count)];
                    DamageCalculator.Resolve(new DamageInfo(amount, attacker), t, context.State);
                }
                break;
        }
    }

    // 플레이어 카드 → ctx.Target(적), 적 행동 → 플레이어
    private static ICombatant ResolveSingleTarget(CardContext ctx)
    {
        if (ctx.Target != null) return ctx.Target;
        if (ctx.ActingEnemy != null) return ctx.State?.Player;
        return null;
    }
}
