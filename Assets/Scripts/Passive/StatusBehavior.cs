using System;
using System.Collections.Generic;
using UnityEngine;

// StatusDefinition에 조합되는 규칙. 런타임 값은 반드시 StatusInstance에 저장한다.
[Serializable]
public abstract class StatusBehavior
{
    public virtual void OnBattleStart(StatusInstance status, CardContext context, ICombatant owner) { }
    public virtual void OnTurnStart(StatusInstance status, CardContext context, ICombatant owner) { }
    public virtual void OnCardPlayed(StatusInstance status, CardContext context, ICombatant owner) { }
    public virtual DamageInfo ModifyOutgoingDamage(StatusInstance status, DamageInfo info, BattleState state, bool preview) => info;
    public virtual DamageInfo ModifyIncomingDamage(StatusInstance status, DamageInfo info, BattleState state, bool preview) => info;
    public virtual int ModifyBlockGain(StatusInstance status, int amount, ICombatant owner) => amount;
    public virtual CardPlayCost ModifyCardPlayCost(StatusInstance status, CardPlayCost cost, CardContext context) => cost;
    public virtual void OnAfterDamageTaken(StatusInstance status, CardContext context, ICombatant owner) { }
    public virtual void OnAfterDamageDealt(StatusInstance status, CardContext context, ICombatant owner) { }
}

public enum DamageModifierSide { Outgoing, Incoming }
public enum DamageModifierOperation { Add, Multiply, Nullify }
public enum StatusMagnitudeSource { Constant, Stacks, Potency, SecondaryPotency }

[Serializable]
public sealed class CardBloodCostStatusBehavior : StatusBehavior
{
    public bool convertEnergyCost = true;
    public bool convertAmmoCost = true;
    public int hpPerEnergy = 1;
    public int hpPerAmmo = 1;
    public StatusMagnitudeSource flatHpCostSource = StatusMagnitudeSource.Constant;
    public int constantFlatHpCost;
    public bool attackCardsOnly;

    public override CardPlayCost ModifyCardPlayCost(StatusInstance status, CardPlayCost cost, CardContext context)
    {
        if (context?.Card == null) return cost;
        if (attackCardsOnly && context.Card.Type != CardData.CardType.Attack) return cost;

        int hpCost = ResolveFlatHpCost(status);
        if (convertEnergyCost)
        {
            hpCost += cost.Energy * Math.Max(0, hpPerEnergy);
            cost.Energy = 0;
        }

        if (convertAmmoCost)
        {
            hpCost += cost.Ammo * Math.Max(0, hpPerAmmo);
            cost.Ammo = 0;
        }

        if (hpCost > 0) cost.Hp += hpCost;
        return cost;
    }

    private int ResolveFlatHpCost(StatusInstance status) => flatHpCostSource switch
    {
        StatusMagnitudeSource.Stacks => Math.Abs(status.Stacks),
        StatusMagnitudeSource.Potency => Math.Abs(status.Potency),
        StatusMagnitudeSource.SecondaryPotency => Math.Abs(status.SecondaryPotency),
        _ => Math.Max(0, constantFlatHpCost),
    };
}

[Serializable]
public sealed class DamageModifierStatusBehavior : StatusBehavior
{
    public DamageModifierSide side;
    public DamageModifierOperation operation;
    public StatusMagnitudeSource magnitudeSource = StatusMagnitudeSource.Constant;
    public int constantAmount;
    public float multiplier = 1f;
    public bool ammoAttacksOnly;
    public bool oncePerTurn;
    [Min(0)] public int consumeStacksOnApply;

    public override DamageInfo ModifyOutgoingDamage(StatusInstance status, DamageInfo info, BattleState state, bool preview)
        => side == DamageModifierSide.Outgoing ? Apply(status, info, preview) : info;

    public override DamageInfo ModifyIncomingDamage(StatusInstance status, DamageInfo info, BattleState state, bool preview)
        => side == DamageModifierSide.Incoming ? Apply(status, info, preview) : info;

    private DamageInfo Apply(StatusInstance status, DamageInfo info, bool preview)
    {
        if (info.Amount <= 0 || ammoAttacksOnly && !info.IsAmmoAttack) return info;
        if (oncePerTurn && status.WasUsedThisTurn(this)) return info;

        switch (operation)
        {
            case DamageModifierOperation.Add:
                info.Amount += ResolveMagnitude(status);
                break;
            case DamageModifierOperation.Multiply:
                info.Amount = Mathf.RoundToInt(info.Amount * multiplier);
                break;
            case DamageModifierOperation.Nullify:
                info.Amount = 0;
                break;
        }

        if (!preview)
        {
            if (oncePerTurn) status.MarkUsedThisTurn(this);
            if (consumeStacksOnApply > 0) status.ReduceMagnitude(consumeStacksOnApply);
        }
        return info;
    }

    private int ResolveMagnitude(StatusInstance status) => magnitudeSource switch
    {
        StatusMagnitudeSource.Stacks => status.Stacks,
        StatusMagnitudeSource.Potency => status.Potency,
        StatusMagnitudeSource.SecondaryPotency => status.SecondaryPotency,
        _ => constantAmount,
    };
}

[Serializable]
public sealed class BlockModifierStatusBehavior : StatusBehavior
{
    public StatusMagnitudeSource magnitudeSource = StatusMagnitudeSource.Stacks;
    public int constantAmount;

    public override int ModifyBlockGain(StatusInstance status, int amount, ICombatant owner)
        => amount + (magnitudeSource switch
        {
            StatusMagnitudeSource.Stacks => status.Stacks,
            StatusMagnitudeSource.Potency => status.Potency,
            StatusMagnitudeSource.SecondaryPotency => status.SecondaryPotency,
            _ => constantAmount,
        });
}

[Serializable]
public abstract class TriggeredEffectsStatusBehavior : StatusBehavior
{
    [SerializeReference, SubclassPicker] public List<CardEffect> effects = new();

    protected void Execute(CardContext context)
    {
        EffectRunner.ExecuteImmediate(effects, context);
    }
}

[Serializable]
public sealed class OnBattleStartEffectsStatusBehavior : TriggeredEffectsStatusBehavior
{
    public override void OnBattleStart(StatusInstance status, CardContext context, ICombatant owner)
        => Execute(context);
}

[Serializable]
public sealed class OnTurnStartEffectsStatusBehavior : TriggeredEffectsStatusBehavior
{
    public override void OnTurnStart(StatusInstance status, CardContext context, ICombatant owner)
        => Execute(context);
}

[Serializable]
public sealed class OnCardPlayedEffectsStatusBehavior : TriggeredEffectsStatusBehavior
{
    public override void OnCardPlayed(StatusInstance status, CardContext context, ICombatant owner)
        => Execute(context);
}

[Serializable]
public sealed class AfterDamageTakenEffectsStatusBehavior : TriggeredEffectsStatusBehavior
{
    public override void OnAfterDamageTaken(StatusInstance status, CardContext context, ICombatant owner)
        => Execute(context);
}

[Serializable]
public sealed class AfterDamageDealtEffectsStatusBehavior : TriggeredEffectsStatusBehavior
{
    public override void OnAfterDamageDealt(StatusInstance status, CardContext context, ICombatant owner)
        => Execute(context);
}
