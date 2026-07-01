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
                    // UnityEngine.Random은 시드와 무관해 결정론적 재현이 안 된다.
                    // BattleManager.Rnd(시드 기반 System.Random)를 사용한다.
                    int idx = context.Battle != null
                        ? context.Battle.Rnd.Next(0, alive.Count)
                        : UnityEngine.Random.Range(0, alive.Count);
                    DamageCalculator.Resolve(new DamageInfo(amount, attacker), alive[idx], context.State);
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

        // 루프마다 new WaitForSeconds를 생성하면 GC Alloc이 hitCount만큼 발생한다.
        // 한 번만 생성해서 재사용한다.
        var delay = new WaitForSeconds(hitInterval);

        switch (targetType)
        {
            case TargetType.SingleEnemy:
                ICombatant single = ResolveSingleTarget(ctx);
                if (single == null || single.IsDead) yield break;
                for (int i = 0; i < hitCount; i++)
                {
                    // yield return 사이에 적이 사망할 수 있으므로 매 히트마다 체크
                    if (single.IsDead) yield break;
                    DamageCalculator.Resolve(new DamageInfo(amount, attacker), single, ctx.State);
                    if (i < hitCount - 1) yield return delay;
                }
                break;

            case TargetType.AllEnemies:
                // ctx.AllEnemies를 직접 foreach하면 yield return 대기 중 리스트가 수정(적 사망 제거)될 때
                // InvalidOperationException이 발생할 수 있어서 복사본을 사용한다.
                var targets = new List<EnemyInstance>(ctx.AllEnemies);
                foreach (var enemy in targets)
                {
                    if (enemy.IsDead) continue;
                    for (int i = 0; i < hitCount; i++)
                    {
                        if (enemy.IsDead) break;
                        DamageCalculator.Resolve(new DamageInfo(amount, attacker), enemy, ctx.State);
                        if (i < hitCount - 1) yield return delay;
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
                    // 이전 히트로 적이 사망했을 수 있으므로 매 히트 전에 dead 제거.
                    // alive를 초기에만 구성하면 이미 죽은 적을 계속 타겟팅하는 버그가 생긴다.
                    alive.RemoveAll(e => e.IsDead);
                    if (alive.Count == 0) yield break;
                    // UnityEngine.Random은 시드와 무관해 결정론적 재현이 안 된다.
                    // BattleManager.Rnd(시드 기반 System.Random)를 사용한다.
                    int idx = ctx.Battle != null
                        ? ctx.Battle.Rnd.Next(0, alive.Count)
                        : UnityEngine.Random.Range(0, alive.Count);
                    DamageCalculator.Resolve(new DamageInfo(amount, attacker), alive[idx], ctx.State);
                    if (i < hitCount - 1) yield return delay;
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
