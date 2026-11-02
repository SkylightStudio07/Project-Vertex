using System;

public enum EffectValueSource
{
    None,
    SpecificStatusStacks,
    DebuffStacks,
    BuffStacks,
    CurrentHp,
    MissingHp,
    Block,
    TriggerStatusStacks,
    TriggerStatusPotency,
    TriggerStatusSecondaryPotency,
    ActualDamage,
}

public enum EffectValueTarget { Source, CurrentTarget, PrimaryTarget }

[Serializable]
public struct EffectValue
{
    public EffectValueSource source;
    public EffectValueTarget target;
    public StatusDefinition status;
    public int multiplier;
    public int flatBonus;
    [UnityEngine.Tooltip("0이면 그대로. 1 이상이면 (값 × multiplier)의 이 퍼센트만 쓴다 (내림). 예: 중독의 20% → multiplier 1, percent 20")]
    public int percent;

    public int Evaluate(CardContext context, ICombatant currentTarget = null)
    {
        if (context == null || source == EffectValueSource.None) return 0;

        int triggerValue;
        switch (source)
        {
            case EffectValueSource.TriggerStatusStacks:
                triggerValue = context.TriggeringStatus?.Stacks ?? 0;
                return flatBonus + Scale(triggerValue);
            case EffectValueSource.TriggerStatusPotency:
                triggerValue = context.TriggeringStatus?.Potency ?? 0;
                return flatBonus + Scale(triggerValue);
            case EffectValueSource.TriggerStatusSecondaryPotency:
                triggerValue = context.TriggeringStatus?.SecondaryPotency ?? 0;
                return flatBonus + Scale(triggerValue);
            case EffectValueSource.ActualDamage:
                return flatBonus + Scale(context.ActualDamage);
        }

        ICombatant subject = target switch
        {
            EffectValueTarget.Source => context.Source,
            EffectValueTarget.PrimaryTarget => context.PrimaryTarget,
            _ => currentTarget ?? context.PrimaryTarget,
        };
        if (subject == null) return flatBonus;

        int raw = source switch
        {
            EffectValueSource.SpecificStatusStacks => subject.Statuses.GetMagnitude(status),
            EffectValueSource.DebuffStacks => subject.Statuses.GetTotalMagnitude(StatusDisposition.Debuff),
            EffectValueSource.BuffStacks => subject.Statuses.GetTotalMagnitude(StatusDisposition.Buff),
            EffectValueSource.CurrentHp => subject.HP,
            EffectValueSource.MissingHp => Math.Max(0, subject.MaxHP - subject.HP),
            EffectValueSource.Block => subject.Block,
            _ => 0,
        };
        return flatBonus + Scale(raw);
    }

    private int Scale(int raw)
    {
        int v = raw * multiplier;
        return percent > 0 ? v * percent / 100 : v;
    }
}
