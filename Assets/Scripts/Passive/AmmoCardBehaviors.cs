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
}

// 영구 상태(파워류)는 스택 = 효과 수치다 (슬더스 방식). 같은 카드를 다시 쓰면 AddStack으로 수치가 합산된다.
// Potency는 스택이 남은 턴·남은 횟수를 뜻해서 두 번째 숫자가 필요한 상태(발포 준비 등)만 쓴다.

// 탄창 숙련 — 스택 = 초탄·막탄 카드의 추가 피해
[Serializable]
public sealed class AmmoBoundaryDamageStatusBehavior : StatusBehavior
{
    public override int CardDamageBonus(StatusInstance status, CardContext context)
        => status.Stacks * ((context.IsFirstAmmoUse ? 1 : 0) + (context.IsLastAmmoUse ? 1 : 0));
}

// 발포 준비 — 스택 = 남은 카드 수, Potency = 카드 한 장당 추가 피해.
// 탄약을 쓰는 카드 한 장마다 Potency만큼 더하고 스택을 1 소모한다 (강화로 amount 2면 "다음 두 카드").
// 상태 정의는 AddStack으로 둘 것 — 다시 얻으면 카드 수만 늘고 장당 피해는 그대로.
[Serializable]
public sealed class NextAmmoDamageStatusBehavior : StatusBehavior
{
    public override void OnAmmoSpent(StatusInstance status, CardContext context, int spent, bool first, bool firstConsumption, bool preview)
    {
        if (!firstConsumption) return;
        context.NextAmmoDamageBonus += status.Potency;
        if (!preview) status.ReduceMagnitude(1);
    }
}

// 사선 이탈·사격 후 이동 — 스택 = 탄약 사용 시 얻는 방어도
[Serializable]
public sealed class AmmoSpentBlockStatusBehavior : StatusBehavior
{
    public bool firstOnly;
    public bool perAmmo;
    public bool expiresAtTurnEnd;
    public override void OnAmmoSpent(StatusInstance status, CardContext context, int spent, bool first, bool firstConsumption, bool preview)
    {
        if (firstOnly && !first) return;
        int amount = status.Stacks * (perAmmo ? spent : 1);
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

// 권총 속사 준비 — 스택 = 다음 탄약 사용 카드의 에너지 감소량.
// 이번 턴 한정 1회성: 탄약을 쓰는 카드를 내거나 턴이 끝나면 통째로 사라진다 (그래서 정의상 지속은 Permanent로 두고 여기서 지운다).
// 사라지기 전에 다시 얻으면 AddStack으로 감소량이 합산된다 (두 번 쓰면 -2).
[Serializable]
public sealed class NextAmmoDiscountStatusBehavior : StatusBehavior
{
    public override CardPlayCost ModifyCardPlayCost(StatusInstance status, CardPlayCost cost, CardContext context)
    {
        if (cost.Ammo > 0 || cost.Ammo == 0 && context?.State?.Ammo > 0 && UsesAllAmmo(context.Card?.ActiveEffects))
            cost.Energy = Math.Max(0, cost.Energy - status.Stacks);
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
