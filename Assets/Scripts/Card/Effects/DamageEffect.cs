using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Damage")]
public class DamageEffect : CardEffect
{
    public int amount;
    public int hitCount = 1;
    public TargetType targetType = TargetType.SingleEnemy;
    // hitCount > 1일 때만 적용. 0이면 딜레이 없이 동기 실행(Execute()와 동일)
    public float hitInterval = 0.08f;

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

    // hitCount == 1이거나 hitInterval == 0이면 동기 Execute()로 폴백.
    // 그 외에는 히트 사이에 hitInterval만큼 대기하면서 데미지를 순차 적용한다.
    public override IEnumerator ExecuteCoroutine(CardContext ctx)
    {
        if (hitCount <= 1 || hitInterval <= 0f)
        {
            Execute(ctx);
            yield break;
        }

        if (ctx.State == null) yield break;
        ICombatant attacker = ctx.Attacker;

        switch (targetType)
        {
            case TargetType.SingleEnemy:
                ICombatant single = ResolveSingleTarget(ctx);
                if (single == null || single.IsDead) yield break;
                for (int i = 0; i < hitCount; i++)
                {
                    if (single.IsDead) yield break;
                    DamageCalculator.Resolve(new DamageInfo(amount, attacker), single, ctx.State);
                    if (i < hitCount - 1) yield return new WaitForSeconds(hitInterval);
                }
                break;

            case TargetType.AllEnemies:
                foreach (var enemy in ctx.AllEnemies)
                {
                    if (enemy.IsDead) continue;
                    for (int i = 0; i < hitCount; i++)
                    {
                        if (enemy.IsDead) break;
                        DamageCalculator.Resolve(new DamageInfo(amount, attacker), enemy, ctx.State);
                        if (i < hitCount - 1) yield return new WaitForSeconds(hitInterval);
                    }
                }
                break;

            case TargetType.RandomEnemy:
                var alive = new List<EnemyInstance>();
                foreach (var e in ctx.AllEnemies)
                    if (!e.IsDead) alive.Add(e);
                if (alive.Count == 0) yield break;
                for (int i = 0; i < hitCount; i++)
                {
                    var t = alive[Random.Range(0, alive.Count)];
                    if (!t.IsDead)
                        DamageCalculator.Resolve(new DamageInfo(amount, attacker), t, ctx.State);
                    if (i < hitCount - 1) yield return new WaitForSeconds(hitInterval);
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
