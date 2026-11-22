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
    // 새 상태의 기본은 AddStack (다시 얻으면 스택만 더하고 위력은 처음 값 유지).
    // 기존 에셋은 저장된 값(대부분 AddStackAndPotency)을 그대로 쓴다.
    [Tooltip("같은 상태를 다시 얻을 때 합치는 방식.\nAddStack: 스택만 더함(기본)\nSeparate: 합치지 않고 따로 쌓음(부여 시 수치를 고정하는 효과용)\nAddStackAndPotency: 스택·위력 모두 더함\nReplace: 새 값으로 교체\nKeepHigherMagnitude: 각각 큰 값 유지\nExtendDuration: 지속(스택)만 더함")]
    [SerializeField] private StatusStackPolicy stackPolicy = StatusStackPolicy.AddStack;
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

// 주의: 에셋에는 정수 값으로 저장된다. 이름·선언 순서는 바꿔도 되지만 숫자는 절대 바꾸지 말 것.
// (선언 순서 = 인스펙터 드롭다운 순서. 기본인 AddStack을 맨 위에 둔다)
public enum StatusStackPolicy
{
    AddStack = 4,           // 기본. 스택만 더하고 Potency·SecondaryPotency는 처음 값 유지
    Separate = 5,           // 합치지 않고 부여할 때마다 별도 인스턴스 (각자 수치·남은 턴을 가짐)
    AddStackAndPotency = 0, // 스택과 Potency·SecondaryPotency를 모두 더함 (구 Add)
    Replace = 1,
    KeepHigherMagnitude = 2,
    ExtendDuration = 3,
}

public enum StatusDurationPolicy
{
    DecreaseOnTurnStart = 0,
    Permanent = 1,
    ConsumeOnly = 2,
    DecreaseOnTurnEnd = 3,
}
