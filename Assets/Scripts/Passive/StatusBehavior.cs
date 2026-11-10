using System;
using System.Collections.Generic;
using UnityEngine;

// StatusDefinition에 조합되는 규칙. 런타임 값은 반드시 StatusInstance에 저장한다.
[Serializable]
public abstract class StatusBehavior
{
    public virtual void OnReload(StatusInstance status, CardContext context) { }
    public virtual void OnAmmoSpent(StatusInstance status, CardContext context, int spent, bool first, bool firstConsumption, bool preview) { }
    public virtual int CardDamageBonus(StatusInstance status, CardContext context) => 0;
    public virtual void OnBattleStart(StatusInstance status, CardContext context, ICombatant owner) { }
    public virtual void OnTurnStart(StatusInstance status, CardContext context, ICombatant owner) { }
    public virtual void OnTurnEnd(StatusInstance status, CardContext context, ICombatant owner) { }
    public virtual void OnEnemyActionEnd(StatusInstance status, CardContext context, EnemyAction action) { }
    public virtual void OnIncomingEnemyActionEnd(StatusInstance status, CardContext context, EnemyAction action) { }
    public virtual void OnDeath(StatusInstance status, CardContext context, ICombatant owner) { }
    public virtual void OnCardPlayed(StatusInstance status, CardContext context, ICombatant owner) { }
    public virtual DamageInfo ModifyOutgoingDamage(StatusInstance status, DamageInfo info, BattleState state, bool preview) => info;
    public virtual DamageInfo ModifyIncomingDamage(StatusInstance status, DamageInfo info, BattleState state, bool preview) => info;
    public virtual int ModifyBlockGain(StatusInstance status, int amount, ICombatant owner) => amount;
    public virtual CardPlayCost ModifyCardPlayCost(StatusInstance status, CardPlayCost cost, CardContext context) => cost;
    public virtual bool AllowsCardPlay(StatusInstance status, CardContext context) => true;
    public virtual void OnAfterDamageTaken(StatusInstance status, CardContext context, ICombatant owner) { }
    public virtual void OnAfterDamageDealt(StatusInstance status, CardContext context, ICombatant owner) { }
    // 이 상태의 주인이 다른 상태를 새로 얻었을 때 (ApplyStatusEffect로 부여된 경우)
    public virtual void OnStatusGained(StatusInstance status, CardContext context, ICombatant owner, StatusDefinition gained, int amount) { }
}

public enum DamageModifierSide { Outgoing, Incoming }
// Cap: 피해를 constantAmount 이하로 제한 (알케미스트 '시야 차단' — 받는 피해 1). 에셋에 정수로 저장되므로 새 항목은 끝에
public enum DamageModifierOperation { Add, Multiply, Nullify, Cap }
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
            case DamageModifierOperation.Cap:
                info.Amount = Mathf.Min(info.Amount, Mathf.Max(0, ResolveMagnitude(status)));
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

// 이 상태를 가진 쪽의 공격이 방어도를 일부 관통한다 (예: 카리나 '천부적 살인술' 25%)
[Serializable]
public sealed class BlockPierceStatusBehavior : StatusBehavior
{
    [Range(0f, 1f)] public float percent = 0.25f;

    public override DamageInfo ModifyOutgoingDamage(StatusInstance status, DamageInfo info, BattleState state, bool preview)
    {
        if (info.Source != null) info.PiercePercent = Mathf.Max(info.PiercePercent, percent);
        return info;
    }
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

// 주인이 지정한 상태를 얻을 때마다 효과 발동 (알케미스트 '동귀어진' — 자신이 중독될 때마다 적에게 중독 1)
[Serializable]
public sealed class OnStatusGainedEffectsStatusBehavior : TriggeredEffectsStatusBehavior
{
    public StatusDefinition watchedStatus;

    public override void OnStatusGained(StatusInstance status, CardContext context, ICombatant owner, StatusDefinition gained, int amount)
    {
        if (gained != null && gained == watchedStatus && amount > 0) Execute(context);
    }
}

// 다음에 쓰는 '지정 상태를 부여하는 카드' 1장의 에너지 코스트를 0으로 (알케미스트 '촉매 반응' — 다음 중독 카드)
[Serializable]
public sealed class NextStatusCardFreeStatusBehavior : StatusBehavior
{
    public StatusDefinition cardAppliesStatus;

    public override CardPlayCost ModifyCardPlayCost(StatusInstance status, CardPlayCost cost, CardContext context)
    {
        if (Matches(context?.Card)) cost.Energy = 0;
        return cost;
    }

    public override void OnCardPlayed(StatusInstance status, CardContext context, ICombatant owner)
    {
        if (Matches(context?.Card)) status.RemoveAll();
    }

    private bool Matches(CardData card)
    {
        if (card == null || cardAppliesStatus == null || card.ActiveEffects == null) return false;
        foreach (var effect in card.ActiveEffects)
            if (effect is ApplyStatusEffect apply && apply.status == cardAppliesStatus && apply.operation == StatusStackOperation.Add) return true;
        return false;
    }
}

[Serializable]
public sealed class OnTurnStartEffectsStatusBehavior : TriggeredEffectsStatusBehavior
{
    public override void OnTurnStart(StatusInstance status, CardContext context, ICombatant owner)
        => Execute(context);
}

// N턴마다 효과 발동. 스택이 "발동까지 남은 턴"이다 (턴 시작마다 1씩 줄고, 1에서 발동 후 interval로 되돌아감).
// 상태 정의의 durationPolicy는 Permanent로 둘 것 (스택이 0이 되어 사라지지 않게).
[Serializable]
public sealed class CountdownEffectsStatusBehavior : TriggeredEffectsStatusBehavior
{
    [Min(1)] public int interval = 3;

    public override void OnTurnStart(StatusInstance status, CardContext context, ICombatant owner)
    {
        if (status.Stacks <= 1)
        {
            Execute(context);
            status.SetMagnitude(interval);
        }
        else status.ReduceMagnitude(1);
    }
}

[Serializable]
public sealed class OnTurnEndEffectsStatusBehavior : TriggeredEffectsStatusBehavior
{
    public override void OnTurnEnd(StatusInstance status, CardContext context, ICombatant owner)
        => Execute(context);
}

// 실제 적 행동을 완료했을 때만 발동한다. 기절로 건너뛴 턴에는 발동하지 않는다.
[Serializable]
public sealed class OnEnemyActionEndEffectsStatusBehavior : TriggeredEffectsStatusBehavior
{
    public override void OnEnemyActionEnd(StatusInstance status, CardContext context, EnemyAction action)
    {
        if (action != null) Execute(context);
    }
}

[Serializable]
public sealed class OnDeathEffectsStatusBehavior : TriggeredEffectsStatusBehavior
{
    public override void OnDeath(StatusInstance status, CardContext context, ICombatant owner)
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

// 한 턴에 손으로 낼 수 있는 카드 수 제한. 스택이 최대 장수다 (예: 전술 대형 전환 — 3장, 강화 4장).
// 상태 정의의 durationPolicy는 Permanent로 둘 것.
[Serializable]
public sealed class CardPlayLimitStatusBehavior : StatusBehavior
{
    public override bool AllowsCardPlay(StatusInstance status, CardContext context)
        => context?.State == null || context.State.CardsPlayedThisTurn < status.Stacks;
}
