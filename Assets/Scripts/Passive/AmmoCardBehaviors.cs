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
        Visit(context.State, (status, behavior) => bonus += behavior.CardDamageBonus(status, context));
        return bonus;
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
