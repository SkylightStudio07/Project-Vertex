using System.Collections;
using System.Collections.Generic;

// 카드/적/아이템/이벤트가 공유하는 유일한 효과 실행 진입점.
public static class EffectRunner
{
    private const int MaxNestedDepth = 32;

    public static void ExecuteImmediate(IReadOnlyList<CardEffect> effects, CardContext context)
    {
        if (effects == null || context == null || context.ExecutionDepth >= MaxNestedDepth) return;
        context.ExecutionDepth++;
        try
        {
            foreach (var effect in effects) effect?.Execute(context);
        }
        finally
        {
            context.ExecutionDepth--;
        }
    }

    public static IEnumerator ExecuteSequence(IReadOnlyList<CardEffect> effects, CardContext context)
    {
        if (effects == null || context == null || context.ExecutionDepth >= MaxNestedDepth) yield break;
        context.ExecutionDepth++;
        try
        {
            foreach (var effect in effects)
                if (effect != null) yield return effect.ExecuteCoroutine(context);
        }
        finally
        {
            context.ExecutionDepth--;
        }
    }

    public static EffectPreview Preview(IReadOnlyList<CardEffect> effects, CardContext context)
    {
        var preview = new EffectPreview();
        CollectPreview(effects, context, preview, 0);
        return preview;
    }

    private static void CollectPreview(
        IReadOnlyList<CardEffect> effects,
        CardContext context,
        EffectPreview preview,
        int depth)
    {
        if (effects == null || context == null || depth >= MaxNestedDepth) return;
        foreach (var effect in effects)
        {
            switch (effect)
            {
                case DamageEffect damage:
                    foreach (var target in damage.targets.Resolve(context))
                    {
                        var info = new DamageInfo(
                            damage.GetRawAmount(context, target),
                            damage.environmentalDamage ? null : context.Source,
                            damage.piercing,
                            context.Card != null && context.Card.AmmoCost > 0);
                        if (info.Source != null)
                            foreach (var passive in info.Source.Passives)
                                info = passive.PreviewOutgoingDamage(info, context.State);
                        foreach (var passive in target.Passives)
                            info = passive.PreviewIncomingDamage(info, context.State);
                        int dealt = System.Math.Max(0, info.Amount) * damage.hitCount;
                        preview.TotalDamage += dealt;
                        preview.AddTargetDamage(target, dealt, info.IsPiercing);
                        preview.HitCount += damage.hitCount;
                        preview.HasDamage = true;
                    }
                    break;
                case ApplyStatusEffect status when status.status != null:
                    preview.Statuses.Add(status.status);
                    break;
                case ConditionalEffect conditional when conditional.condition != null &&
                                                       conditional.condition.IsMet(context):
                    CollectPreview(conditional.effectsWhenMet, context, preview, depth + 1);
                    break;
                case RepeatEffect repeat:
                    for (int i = 0; i < repeat.count; i++)
                        CollectPreview(repeat.effects, context, preview, depth + 1);
                    break;
            }
        }
    }
}

public sealed class EffectPreview
{
    public bool HasDamage;
    public int TotalDamage;
    public int HitCount;
    public readonly List<StatusDefinition> Statuses = new();

    // 대상별 피해 (HP바 예상 피해 표시용). 관통 피해는 방어도를 무시하므로 따로 모은다.
    public readonly Dictionary<ICombatant, (int normal, int piercing)> DamageByTarget = new();

    public void AddTargetDamage(ICombatant target, int amount, bool piercing)
    {
        if (target == null || amount <= 0) return;
        DamageByTarget.TryGetValue(target, out var d);
        DamageByTarget[target] = piercing ? (d.normal, d.piercing + amount) : (d.normal + amount, d.piercing);
    }

    // 방어도를 먼저 깎고 남은 만큼 + 관통 피해 = 실제로 줄어들 HP
    public int GetHpLoss(ICombatant target, int block)
    {
        if (target == null || !DamageByTarget.TryGetValue(target, out var d)) return 0;
        return System.Math.Max(0, d.normal - System.Math.Max(0, block)) + d.piercing;
    }
}
