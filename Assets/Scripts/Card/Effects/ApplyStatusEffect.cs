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
    // 고리형 강인함류 — "이후 N턴간 턴 시작 시 방어도 X". 부여하는 순간의 민첩·손상 등을 potency에 반영해 고정한다.
    // 상태 쪽 방어도 효과는 BlockEffect.ignoreBlockModifiers를 켜서 고정값을 그대로 줘야 보정이 두 번 들어가지 않는다.
    // 같은 카드를 여러 번 쓰면 각자 수치를 가져야 하므로 상태 정의는 Separate 정책으로 둘 것.
    [UnityEngine.Tooltip("켜면 부여 순간 대상의 방어도 보정(민첩 등)을 potency에 반영해 고정한다. 상태의 BlockEffect는 '방어도 보정 무시'를 켤 것")]
    public bool snapshotPotencyAsBlock;

    public override void Execute(CardContext context)
    {
        if (context?.State == null || status == null) return;

        foreach (var target in targets.Resolve(context))
        {
            int resolvedAmount = amount + scaling.Evaluate(context, target);
            if (negative) resolvedAmount = -resolvedAmount;
            if (operation == StatusStackOperation.Add)
            {
                int resolvedPotency = snapshotPotencyAsBlock ? target.Statuses.ModifyBlockGain(potency, target) : potency;
                target.AddPassive(status.CreateInstance(resolvedAmount, resolvedPotency, secondaryPotency, context.Source));
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
