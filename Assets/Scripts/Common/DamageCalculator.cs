// 피해 파이프라인 유틸.
// DamageInfo.Source(공격자) 패시브 → target 패시브 → TakeDamage 순으로 실행.
public static class DamageCalculator
{
    public static void Resolve(DamageInfo info, ICombatant target, BattleState state, CardContext originContext = null)
    {
        // 1. 공격자 패시브 (Weak, Strength 등) — Source가 null이면 환경 피해
        if (info.Source != null)
        {
            foreach (var p in info.Source.Passives)
                info = p.ModifyOutgoingDamage(info, state);
            info.Source.RemoveExpiredPassives();
        }

        // 2. 방어자 패시브 (Vulnerable 등)
        foreach (var p in target.Passives)
            info = p.ModifyIncomingDamage(info, state);
        target.RemoveExpiredPassives();

        // 3. 블록 흡수 + HP 감소 (TakeDamage 내부에서 처리)
        int hpBefore = target.HP;
        target.TakeDamage(info);
        int actualDamage = System.Math.Max(0, hpBefore - target.HP);
        if (actualDamage <= 0) return;

        target.Statuses.NotifyAfterDamageTaken(state, target, info.Source, actualDamage, originContext);

        if (info.Source == null) return;

        info.Source.Statuses.NotifyAfterDamageDealt(state, info.Source, target, actualDamage, originContext);
    }
}
