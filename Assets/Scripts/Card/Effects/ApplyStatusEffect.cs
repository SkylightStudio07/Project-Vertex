using System;

[Serializable]
public class ApplyStatusEffect : CardEffect
{
    public StatusDefinition status;
    public StatusStackOperation operation = StatusStackOperation.Add;
    public int amount;
    public EffectValue scaling;
    public int potency;
    public int secondaryPotency;
    public EffectTargetSelector targets = EffectTargetSelector.PrimaryTarget;

    public override void Execute(CardContext context)
    {
        if (context?.State == null || status == null) return;

        foreach (var target in targets.Resolve(context))
        {
            int resolvedAmount = amount + scaling.Evaluate(context, target);
            if (operation == StatusStackOperation.Add)
                target.AddPassive(status.CreateInstance(resolvedAmount, potency, secondaryPotency, context.Source));
            else
                target.Statuses.Modify(status, operation, resolvedAmount);
        }
    }
}

public enum StatusStackOperation
{
    Add,
    Reduce,
    Multiply,
    Set,
    RemoveAll,
}
