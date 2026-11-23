using System;
using System.Collections.Generic;

// 새 탄약 이벤트도 상태 정의의 Behavior를 통해 처리한다. 원본 SO에는 실행 상태를 저장하지 않는다.
public static class AmmoStatusEvents
{
    public static void Visit(BattleState state, Action<StatusInstance, StatusBehavior> action)
    {
        if (state?.Player == null) return;
        foreach (var passive in state.Player.Passives.ToArray())
            if (passive is StatusInstance status && !status.IsExpired && status.Definition != null)
                foreach (var behavior in status.Definition.Behaviors)
                    if (behavior != null) action(status, behavior);
    }

    public static int DamageBonus(CardContext context)
    {
        if (context?.State == null || context.TriggeringPassive != null || context.Source != context.State.Player) return 0;
        int bonus = context.NextAmmoDamageBonus;
        if (context.HasAmmoUsage) bonus += context.State.ShotDamageBonus; // 무기고 강화(한방형): 탄약을 쓴 공격만
        Visit(context.State, (status, behavior) => bonus += behavior.CardDamageBonus(status, context));
        return bonus;
    }

    // 플레이어 카드 피해 배율. DamageCalculator가 공격자 패시브(덧셈 보정) 다음에 곱한다.
    public static float DamageMultiplier(CardContext context)
    {
        if (context?.State == null || context.Card == null || context.TriggeringPassive != null || context.Source != context.State.Player) return 1f;
        float multiplier = 1f;
        Visit(context.State, (status, behavior) => multiplier *= behavior.CardDamageMultiplier(status, context));
        return multiplier;
    }

    public static DamageInfo ApplyDamageMultiplier(DamageInfo info, CardContext context)
    {
        if (info.Amount <= 0 || info.Source == null) return info;
        float multiplier = DamageMultiplier(context);
        if (!UnityEngine.Mathf.Approximately(multiplier, 1f))
            info.Amount = UnityEngine.Mathf.RoundToInt(info.Amount * multiplier);
        return info;
    }

    // 카드 효과가 전부 끝난 뒤 BattleManager가 부른다. 소모된 상태는 바로 정리한다.
    public static void NotifyCardResolved(BattleState state, CardContext context)
    {
        if (state?.Player == null || context?.Card == null) return;
        Visit(state, (status, behavior) => behavior.OnCardResolved(status, context));
        state.Player.RemoveExpiredPassives();
    }
}

// 다음 공격 카드 피해 배율: 다음으로 사용하는 공격 카드의 모든 타격 피해에 multiplier를 곱한다.
// 덧셈 보정(활력·힘 등)을 모두 더한 뒤에 곱한다 (합 → 곱). 그 공격 카드의 효과가 모두 끝나면 소모된다.
// 무기 조건 등은 이 상태를 거는 카드 쪽(ConditionalEffect + PlayerWeaponCondition)에서 판정한다.
[Serializable]
public sealed class NextAttackCardMultiplierStatusBehavior : StatusBehavior
{
    public float multiplier = 2f;

    private static bool IsAttackCard(CardContext context) => context?.Card != null && context.Card.Type == CardData.CardType.Attack;

    public override float CardDamageMultiplier(StatusInstance status, CardContext context)
        => IsAttackCard(context) ? multiplier : 1f;

    public override void OnCardResolved(StatusInstance status, CardContext context)
    {
        if (IsAttackCard(context)) status.RemoveAll();
    }
}

[Serializable]
public sealed class AmmoBoundaryDamageStatusBehavior : StatusBehavior
{
    public override int CardDamageBonus(StatusInstance status, CardContext context)
        => status.Potency * ((context.IsFirstAmmoUse ? 1 : 0) + (context.IsLastAmmoUse ? 1 : 0));
}

[Serializable]
public sealed class NextAmmoDamageStatusBehavior : StatusBehavior
{
    public override void OnAmmoSpent(StatusInstance status, CardContext context, int spent, bool first, bool firstConsumption, bool preview)
    {
        if (!firstConsumption) return;
        context.NextAmmoDamageBonus += status.Potency;
        if (!preview) status.RemoveAll();
    }
}

[Serializable]
public sealed class AmmoSpentBlockStatusBehavior : StatusBehavior
{
    public bool firstOnly;
    public bool perAmmo;
    public bool expiresAtTurnEnd;
    public override void OnAmmoSpent(StatusInstance status, CardContext context, int spent, bool first, bool firstConsumption, bool preview)
    {
        if (firstOnly && !first) return;
        int amount = status.Potency * (perAmmo ? spent : 1);
        if (preview) context.PreviewPlayerBlockGain(amount);
        else context.State.Player.AddBlock(amount);
    }
    public override void OnTurnEnd(StatusInstance status, CardContext context, ICombatant owner)
    {
        if (expiresAtTurnEnd) status.RemoveAll();
    }
}

[Serializable]
public sealed class OnReloadEffectsStatusBehavior : TriggeredEffectsStatusBehavior
{
    public override void OnReload(StatusInstance status, CardContext context)
        => Execute(CardContext.CreatePassiveContext(context.State, context.State.Player, null, status, parent: context));
}

[Serializable]
public sealed class NextAmmoDiscountStatusBehavior : StatusBehavior
{
    public override CardPlayCost ModifyCardPlayCost(StatusInstance status, CardPlayCost cost, CardContext context)
    {
        if (cost.Ammo > 0 || cost.Ammo == 0 && context?.State?.Ammo > 0 && UsesAllAmmo(context.Card?.ActiveEffects))
            cost.Energy = Math.Max(0, cost.Energy - status.Potency);
        return cost;
    }
    private static bool UsesAllAmmo(IReadOnlyList<CardEffect> effects)
    {
        if (effects == null) return false;
        foreach (var effect in effects) if (effect is AmmoConsumeAllDamageEffect) return true;
        return false;
    }
    public override void OnAmmoSpent(StatusInstance status, CardContext context, int spent, bool first, bool firstConsumption, bool preview)
    {
        if (!preview && firstConsumption) status.RemoveAll();
    }
    public override void OnTurnEnd(StatusInstance status, CardContext context, ICombatant owner) => status.RemoveAll();
}
