using UnityEngine;

// 흡혈: 대상에게 피해를 주고, 실제로 깎인 체력만큼 시전자가 회복한다.
// 방어도에 막힌 만큼은 회복하지 않는다. 기획: Docs/기획/동료_스킬.md
[System.Serializable]
public class LifeDrainEffect : CardEffect
{
    public int amount;
    public EffectValue scaling;
    public EffectTargetSelector targets = EffectTargetSelector.PrimaryTarget;
    [Tooltip("방어도를 무시하고 체력에 바로 피해")]
    public bool piercing;

    public override void Execute(CardContext context)
    {
        if (context?.State == null || context.Source == null) return;

        int drained = 0;
        foreach (var target in targets.Resolve(context))
        {
            int resolved = amount + scaling.Evaluate(context, target);
            if (resolved <= 0) continue;
            int hpBefore = target.HP;
            DamageCalculator.Resolve(new DamageInfo(resolved, context.Source, piercing), target, context.State, context);
            drained += Mathf.Max(0, hpBefore - target.HP);
        }

        if (drained > 0 && !context.Source.IsDead) context.Source.Heal(drained);
    }

    public override int GetDisplayValue(string fieldName, int rawValue, BattleState state, CardData card, EnemyInstance target = null)
    {
        if (fieldName != nameof(amount) || state?.Player == null) return rawValue;

        var info = new DamageInfo(rawValue, state.Player, piercing);
        foreach (var passive in state.Player.Passives)
            info = passive.PreviewOutgoingDamage(info, state);
        if (target != null)
            foreach (var passive in target.Passives)
                info = passive.PreviewIncomingDamage(info, state);
        return info.Amount;
    }
}
