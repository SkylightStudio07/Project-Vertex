// 엠티 매거진 카드용. 남은 탄약을 전량 소모하고 damagePerAmmo × 소모량의 단발 피해를 준다
// (슬더스의 '소용돌이'처럼 자원을 전부 태우는 카드). 탄약 0이면 아무 일도 없음.
// 카드 자체의 ammoCost는 0이어야 함 — 코스트로 먼저 차감되면 여기서 셀 탄약이 줄어든다.
[System.Serializable]
public class AmmoConsumeAllDamageEffect : CardEffect
{
    public int damagePerAmmo = 8;
    public EffectTargetSelector targets = EffectTargetSelector.PrimaryTarget;

    public override void Execute(CardContext ctx)
    {
        if (ctx.State == null) return;

        int ammo = ctx.State.Ammo;
        if (ammo <= 0) return;

        ctx.State.Ammo = 0;

        int totalDamage = damagePerAmmo * ammo;
        foreach (var target in targets.Resolve(ctx))
            DamageCalculator.Resolve(new DamageInfo(totalDamage, ctx.Source, false, true), target, ctx.State, ctx);
    }
}
