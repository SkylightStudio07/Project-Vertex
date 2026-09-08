using System;
using System.Collections.Generic;

// 상태 종류와 런타임 수치를 분리한 공통 상태 인스턴스.
public sealed class StatusInstance : IPassiveLogic
{
    private readonly HashSet<StatusBehavior> _usedThisTurn = new();

    public StatusDefinition Definition { get; }
    public int Stacks { get; private set; }
    public int Potency { get; private set; }
    public int SecondaryPotency { get; private set; }
    public ICombatant Source { get; private set; }

    public bool IsExpired => Stacks == 0;
    public bool IsDebuff => Definition != null && Definition.GetDisposition(Stacks) == StatusDisposition.Debuff;

    public StatusInstance(StatusDefinition definition, int stacks, int potency = 0, int secondaryPotency = 0, ICombatant source = null)
    {
        Definition = definition;
        Stacks = Clamp(stacks);
        Potency = potency;
        SecondaryPotency = secondaryPotency;
        Source = source;
    }

    public bool TryMerge(StatusInstance other)
    {
        if (other == null || other.Definition != Definition || Definition == null) return false;

        switch (Definition.StackPolicy)
        {
            case StatusStackPolicy.Replace:
                Stacks = Clamp(other.Stacks);
                Potency = other.Potency;
                SecondaryPotency = other.SecondaryPotency;
                Source = other.Source ?? Source;
                break;
            case StatusStackPolicy.KeepHigherMagnitude:
                if (Math.Abs(other.Stacks) > Math.Abs(Stacks)) Stacks = Clamp(other.Stacks);
                if (Math.Abs(other.Potency) > Math.Abs(Potency)) Potency = other.Potency;
                if (Math.Abs(other.SecondaryPotency) > Math.Abs(SecondaryPotency)) SecondaryPotency = other.SecondaryPotency;
                break;
            case StatusStackPolicy.ExtendDuration:
                // 동일한 시한부 효과를 다시 얻으면 지속 시간만 늘린다.
                // 효과의 위력은 유지하고 남은 턴만 합산한다.
                Stacks = Clamp(Stacks + other.Stacks);
                Source = other.Source ?? Source;
                break;
            default:
                Stacks = Clamp(Stacks + other.Stacks);
                Potency += other.Potency;
                SecondaryPotency += other.SecondaryPotency;
                Source = other.Source ?? Source;
                break;
        }
        return true;
    }

    public void ReduceMagnitude(int amount)
    {
        if (amount <= 0) return;
        Stacks = Stacks > 0 ? Math.Max(0, Stacks - amount) : Math.Min(0, Stacks + amount);
    }

    public void SetMagnitude(int amount) => Stacks = Clamp(amount);
    public void MultiplyMagnitude(int multiplier) => Stacks = multiplier < 0 ? Stacks : Clamp(Stacks * multiplier);
    public void RemoveAll() => Stacks = 0;

    public void TickDown(StatusDurationPolicy timing)
    {
        if (timing != StatusDurationPolicy.DecreaseOnTurnStart && timing != StatusDurationPolicy.DecreaseOnTurnEnd) return;
        if (Definition == null || Definition.DurationPolicy != timing) return;
        ReduceMagnitude(1);
    }

    public void ResetTurnUsage() => _usedThisTurn.Clear();

    public bool WasUsedThisTurn(StatusBehavior behavior) => _usedThisTurn.Contains(behavior);
    public void MarkUsedThisTurn(StatusBehavior behavior) => _usedThisTurn.Add(behavior);

    public void OnBattleStart(CardContext context, ICombatant owner)
    {
        if (Definition == null) return;
        foreach (var behavior in Definition.Behaviors) behavior?.OnBattleStart(this, context, owner);
    }

    public void OnTurnStart(CardContext context, ICombatant owner)
    {
        if (Definition == null) return;
        foreach (var behavior in Definition.Behaviors) behavior?.OnTurnStart(this, context, owner);
    }

    public void OnTurnEnd(CardContext context, ICombatant owner)
    {
        if (Definition == null) return;
        foreach (var behavior in Definition.Behaviors) behavior?.OnTurnEnd(this, context, owner);
    }

    public void OnCardPlayed(CardContext context, ICombatant owner)
    {
        if (Definition == null) return;
        foreach (var behavior in Definition.Behaviors) behavior?.OnCardPlayed(this, context, owner);
    }

    public DamageInfo ModifyOutgoingDamage(DamageInfo info, BattleState state)
        => ModifyOutgoing(info, state, false);

    public DamageInfo PreviewOutgoingDamage(DamageInfo info, BattleState state)
        => ModifyOutgoing(info, state, true);

    public DamageInfo ModifyIncomingDamage(DamageInfo info, BattleState state)
        => ModifyIncoming(info, state, false);

    public DamageInfo PreviewIncomingDamage(DamageInfo info, BattleState state)
        => ModifyIncoming(info, state, true);

    public int ModifyBlockGain(int amount, ICombatant owner)
    {
        if (Definition == null) return amount;
        foreach (var behavior in Definition.Behaviors)
            if (behavior != null) amount = behavior.ModifyBlockGain(this, amount, owner);
        return amount;
    }

    public CardPlayCost ModifyCardPlayCost(CardPlayCost cost, CardContext context)
    {
        if (Definition == null) return cost;
        foreach (var behavior in Definition.Behaviors)
            if (behavior != null) cost = behavior.ModifyCardPlayCost(this, cost, context);
        return cost;
    }

    public void OnAfterDamageTaken(CardContext context, ICombatant owner)
    {
        if (Definition == null) return;
        foreach (var behavior in Definition.Behaviors) behavior?.OnAfterDamageTaken(this, context, owner);
    }

    public void OnAfterDamageDealt(CardContext context, ICombatant owner)
    {
        if (Definition == null) return;
        foreach (var behavior in Definition.Behaviors) behavior?.OnAfterDamageDealt(this, context, owner);
    }

    private DamageInfo ModifyOutgoing(DamageInfo info, BattleState state, bool preview)
    {
        if (Definition == null) return info;
        foreach (var behavior in Definition.Behaviors)
            if (behavior != null) info = behavior.ModifyOutgoingDamage(this, info, state, preview);
        return info;
    }

    private DamageInfo ModifyIncoming(DamageInfo info, BattleState state, bool preview)
    {
        if (Definition == null) return info;
        foreach (var behavior in Definition.Behaviors)
            if (behavior != null) info = behavior.ModifyIncomingDamage(this, info, state, preview);
        return info;
    }

    private int Clamp(int value)
    {
        int max = Definition != null ? Definition.MaxStacks : 0;
        return max <= 0 ? value : Math.Max(-max, Math.Min(max, value));
    }
}
