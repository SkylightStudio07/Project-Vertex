using UnityEngine;

// 엠티 매거진용. 현재 탄약을 전량 소모하고 damagePerAmmo * 소모량만큼 피해를 준다.
// 탄약이 없으면 효과 없음.
[CreateAssetMenu(menuName = "Cards/Effects/AmmoConsumeAllDamage")]
public class AmmoConsumeAllDamageEffect : CardEffect
{
    public int damagePerAmmo = 8;
    public TargetType targetType = TargetType.SingleEnemy;

    public override void Execute(CardContext ctx)
    {
        if (ctx.State == null) return;

        int ammo = ctx.State.Ammo;
        if (ammo <= 0) return;

        ctx.State.Ammo = 0;

        int totalDamage = damagePerAmmo * ammo;
        ICombatant attacker = ctx.Attacker;

        switch (targetType)
        {
            case TargetType.SingleEnemy:
                ICombatant target = ResolveSingleTarget(ctx);
                if (target == null || target.IsDead) return;
                DamageCalculator.Resolve(new DamageInfo(totalDamage, attacker, false, true), target, ctx.State);
                break;

            case TargetType.AllEnemies:
                foreach (var enemy in ctx.AllEnemies)
                {
                    if (enemy.IsDead) continue;
                    DamageCalculator.Resolve(new DamageInfo(totalDamage, attacker, false, true), enemy, ctx.State);
                }
                break;
        }
    }

    private static ICombatant ResolveSingleTarget(CardContext ctx)
    {
        if (ctx.Target != null) return ctx.Target;
        if (ctx.ActingEnemy != null) return ctx.State?.Player;
        return null;
    }
}
