using System;
using System.Collections.Generic;
using UnityEngine;

// 상태의 불변 데이터. 전투 중 수치는 StatusInstance가 소유한다.
[CreateAssetMenu(fileName = "NewStatus", menuName = "Game Asset/Status Definition")]
public sealed class StatusDefinition : ScriptableObject
{
    [SerializeField] private string statusId;
    [SerializeField] private string displayName;
    [TextArea, SerializeField] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private StatusDisposition disposition = StatusDisposition.Neutral;
    [SerializeField] private StatusStackPolicy stackPolicy = StatusStackPolicy.Add;
    [SerializeField] private StatusDurationPolicy durationPolicy = StatusDurationPolicy.DecreaseOnTurnStart;
    [Min(0), SerializeField] private int maxStacks;
    [SerializeField] private bool dispellable = true;
    [SerializeReference, SubclassPicker] private List<StatusBehavior> behaviors = new();

    public string Id => statusId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public StatusStackPolicy StackPolicy => stackPolicy;
    public StatusDurationPolicy DurationPolicy => durationPolicy;
    public int MaxStacks => maxStacks;
    public bool Dispellable => dispellable;
    public IReadOnlyList<StatusBehavior> Behaviors => behaviors;

    public StatusDisposition GetDisposition(int stacks)
    {
        if (disposition != StatusDisposition.DynamicBySign)
            return disposition;
        if (stacks > 0) return StatusDisposition.Buff;
        if (stacks < 0) return StatusDisposition.Debuff;
        return StatusDisposition.Neutral;
    }

    public StatusInstance CreateInstance(int stacks, int potency = 0, int secondaryPotency = 0, ICombatant source = null)
        => new(this, stacks, potency, secondaryPotency, source);

    public override string ToString() => DisplayName;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(statusId))
            statusId = name;
        if (maxStacks < 0)
            maxStacks = 0;
    }
#endif
}

public enum StatusDisposition
{
    Neutral,
    Buff,
    Debuff,
    DynamicBySign,
}

public enum StatusStackPolicy
{
    Add,
    Replace,
    KeepHigherMagnitude,
    ExtendDuration,
}

public enum StatusDurationPolicy
{
    DecreaseOnTurnStart,
    Permanent,
    ConsumeOnly,
}
