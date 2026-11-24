// 피해 파이프라인 유틸.
// DamageInfo.Source(공격자) 패시브 → target 패시브 → TakeDamage 순으로 실행.
public static class DamageCalculator
{
    public static bool Resolve(DamageInfo info, ICombatant target, BattleState state, CardContext originContext = null)
    {
        // 1. 공격자 패시브 (Weak, Strength 등) — Source가 null이면 환경 피해
        if (info.Source != null)
        {
            foreach (var p in info.Source.Passives)
                info = p.ModifyOutgoingDamage(info, state);
            info.Source.RemoveExpiredPassives();
            // 1-1. 플레이어 카드 피해 배율 — 덧셈 보정(힘 등)을 모두 더한 뒤 곱한다 (다음 공격 카드 강화 등)
            info = AmmoStatusEvents.ApplyDamageMultiplier(info, originContext);
        }

        // 2. 방어자 패시브 (Vulnerable 등)
        foreach (var p in target.Passives)
            info = p.ModifyIncomingDamage(info, state);
        target.RemoveExpiredPassives();

        // 3. 블록 흡수 + HP 감소 (TakeDamage 내부에서 처리)
        int hpBefore = target.HP;
        target.TakeDamage(info);
        bool killed = hpBefore > 0 && target.IsDead;
        int actualDamage = System.Math.Max(0, hpBefore - target.HP);
        if (actualDamage <= 0) return killed;

        target.Statuses.NotifyAfterDamageTaken(state, target, info.Source, actualDamage, originContext);

        if (info.Source == null) return killed;

        info.Source.Statuses.NotifyAfterDamageDealt(state, info.Source, target, actualDamage, originContext);
        return killed;
    }
}
