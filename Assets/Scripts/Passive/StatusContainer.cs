using System;
using System.Collections.Generic;

// 플레이어와 적이 공유하는 상태/패시브 저장소.
public sealed class StatusContainer
{
    private readonly List<IPassiveLogic> _entries;

    public StatusContainer(List<IPassiveLogic> entries) => _entries = entries;
    public IReadOnlyList<IPassiveLogic> Entries => _entries;

    public void Add(IPassiveLogic passive)
    {
        if (passive == null) return;

        if (passive is StatusInstance incoming)
        {
            foreach (var entry in _entries)
                if (entry is StatusInstance existing && existing.TryMerge(incoming)) return;
        }

        _entries.Add(passive);
    }

    public StatusInstance Find(StatusDefinition definition)
    {
        if (definition == null) return null;
        foreach (var entry in _entries)
            if (entry is StatusInstance status && status.Definition == definition) return status;
        return null;
    }

    public int GetMagnitude(StatusDefinition definition) => Find(definition)?.Stacks ?? 0;

    public int GetTotalMagnitude(StatusDisposition disposition)
    {
        int total = 0;
        foreach (var entry in _entries)
            if (entry is StatusInstance status && status.Definition != null &&
                status.Definition.GetDisposition(status.Stacks) == disposition)
                total += Math.Abs(status.Stacks);
        return total;
    }

    public bool Has(StatusDefinition definition, int minimumMagnitude = 1)
        => Math.Abs(GetMagnitude(definition)) >= Math.Max(0, minimumMagnitude);

    public bool HasDisposition(StatusDisposition disposition, int minimumMagnitude = 1)
        => GetTotalMagnitude(disposition) >= Math.Max(0, minimumMagnitude);

    public void Modify(StatusDefinition definition, StatusStackOperation operation, int amount)
    {
        var status = Find(definition);
        if (status == null) return;

        switch (operation)
        {
            case StatusStackOperation.Reduce: status.ReduceMagnitude(amount); break;
            case StatusStackOperation.Multiply: status.MultiplyMagnitude(amount); break;
            case StatusStackOperation.Set: status.SetMagnitude(amount); break;
            case StatusStackOperation.RemoveAll: status.RemoveAll(); break;
        }
        RemoveExpired();
    }

    public int ModifyBlockGain(int amount, ICombatant owner)
    {
        foreach (var entry in _entries)
            amount = entry.ModifyBlockGain(amount, owner);
        return Math.Max(0, amount);
    }

    public CardPlayCost ModifyCardPlayCost(CardPlayCost cost, CardContext context)
    {
        foreach (var entry in _entries)
            if (entry is StatusInstance status)
                cost = status.ModifyCardPlayCost(cost, context);

        cost.ClampToNonNegative();
        return cost;
    }

    public void NotifyBattleStart(BattleState state, ICombatant owner)
    {
        var snapshot = new List<IPassiveLogic>(_entries);
        foreach (var passive in snapshot)
            passive.OnBattleStart(CardContext.CreatePassiveContext(state, owner, null, passive), owner);
        RemoveExpired();
    }

    public void NotifyCardPlayed(CardContext context, ICombatant owner)
    {
        var snapshot = new List<IPassiveLogic>(_entries);
        foreach (var passive in snapshot)
        {
            var passiveContext = CardContext.CreatePassiveContext(
                context?.State,
                owner,
                context?.PrimaryTarget,
                passive,
                parent: context);
            passive.OnCardPlayed(passiveContext, owner);
        }
        RemoveExpired();
    }

    public void Tick(BattleState state, ICombatant owner)
    {
        var snapshot = new List<IPassiveLogic>(_entries);
        foreach (var passive in snapshot)
            passive.OnTurnStart(CardContext.CreatePassiveContext(state, owner, null, passive), owner);
        foreach (var passive in snapshot)
            if (passive is StatusInstance status) status.TickDown();
        RemoveExpired();
    }

    public void NotifyAfterDamageTaken(
        BattleState state,
        ICombatant owner,
        ICombatant attacker,
        int actualDamage,
        CardContext originContext = null)
    {
        var snapshot = new List<IPassiveLogic>(_entries);
        foreach (var passive in snapshot)
        {
            var context = CardContext.CreatePassiveContext(
                state,
                owner,
                attacker,
                passive,
                actualDamage,
                originContext);
            passive.OnAfterDamageTaken(context, owner);
        }
        RemoveExpired();
    }

    public void NotifyAfterDamageDealt(
        BattleState state,
        ICombatant owner,
        ICombatant target,
        int actualDamage,
        CardContext originContext = null)
    {
        var snapshot = new List<IPassiveLogic>(_entries);
        foreach (var passive in snapshot)
        {
            var context = CardContext.CreatePassiveContext(
                state,
                owner,
                target,
                passive,
                actualDamage,
                originContext);
            passive.OnAfterDamageDealt(context, owner);
        }
        RemoveExpired();
    }

    public void RemoveExpired()
    {
        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            if (_entries[i] is StatusInstance status && status.IsExpired)
                _entries.RemoveAt(i);
        }
    }
}
