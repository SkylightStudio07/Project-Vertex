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
    [UnityEngine.Tooltip("켜면 수치를 음수로 건다 (예: 힘 2 감소). 설명문에는 양수로 보인다")]
    public bool negative;

    public override void Execute(CardContext context)
    {
        if (context?.State == null || status == null) return;

        foreach (var target in targets.Resolve(context))
        {
            int resolvedAmount = amount + scaling.Evaluate(context, target);
            if (negative) resolvedAmount = -resolvedAmount;
            if (operation == StatusStackOperation.Add)
            {
                target.AddPassive(status.CreateInstance(resolvedAmount, potency, secondaryPotency, context.Source));
                target.Statuses.NotifyStatusGained(context, target, status, resolvedAmount);
            }
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
