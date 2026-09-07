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

    public int Evaluate(CardContext context, ICombatant currentTarget = null)
    {
        if (context == null || source == EffectValueSource.None) return 0;

        int triggerValue;
        switch (source)
        {
            case EffectValueSource.TriggerStatusStacks:
                triggerValue = context.TriggeringStatus?.Stacks ?? 0;
                return flatBonus + triggerValue * multiplier;
            case EffectValueSource.TriggerStatusPotency:
                triggerValue = context.TriggeringStatus?.Potency ?? 0;
                return flatBonus + triggerValue * multiplier;
            case EffectValueSource.TriggerStatusSecondaryPotency:
                triggerValue = context.TriggeringStatus?.SecondaryPotency ?? 0;
                return flatBonus + triggerValue * multiplier;
            case EffectValueSource.ActualDamage:
                return flatBonus + context.ActualDamage * multiplier;
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
        return flatBonus + raw * multiplier;
    }
}
