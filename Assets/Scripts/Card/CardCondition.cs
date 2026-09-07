using System;

[System.Serializable]
public abstract class CardCondition
{
    public abstract bool IsMet(CardContext context);

    public virtual bool IsMet(BattleState state, CardData card, EnemyInstance target = null)
    {
        return IsMet(new CardContext
        {
            State = state,
            Card = card,
            Target = target,
            AllEnemies = state?.Enemies,
        });
    }

    protected static bool Compare(int left, IntComparison comparison, int right)
    {
        return comparison switch
        {
            IntComparison.LessThan => left < right,
            IntComparison.LessOrEqual => left <= right,
            IntComparison.Equal => left == right,
            IntComparison.GreaterOrEqual => left >= right,
            IntComparison.GreaterThan => left > right,
            _ => false,
        };
    }
}

public enum IntComparison
{
    LessThan,
    LessOrEqual,
    Equal,
    GreaterOrEqual,
    GreaterThan,
}

[System.Serializable]
public class AlwaysCondition : CardCondition
{
    public override bool IsMet(CardContext context) => true;
}

[System.Serializable]
public class PlayerAmmoCondition : CardCondition
{
    public IntComparison comparison = IntComparison.LessOrEqual;
    public int amount;

    public override bool IsMet(CardContext context)
    {
        if (context.State == null) return false;
        return Compare(context.State.Ammo, comparison, amount);
    }
}

[System.Serializable]
public class PlayerEnergyCondition : CardCondition
{
    public IntComparison comparison = IntComparison.LessOrEqual;
    public int amount;

    public override bool IsMet(CardContext context)
    {
        if (context.State == null) return false;
        return Compare(context.State.Energy, comparison, amount);
    }
}

[System.Serializable]
public class PlayerLostHpThisTurnCondition : CardCondition
{
    public bool expected = true;

    public override bool IsMet(CardContext context)
    {
        if (context.State == null) return false;
        return context.State.PlayerLostHpThisTurn == expected;
    }
}

[System.Serializable]
public class HasStatusCondition : CardCondition
{
    public EffectTargetSelector targets = EffectTargetSelector.PrimaryTarget;
    public StatusDefinition status;
    public int minimumMagnitude = 1;

    public override bool IsMet(CardContext context)
    {
        foreach (var target in targets.Resolve(context))
            if (target.Statuses.Has(status, minimumMagnitude)) return true;
        return false;
    }
}

[System.Serializable]
public class HasStatusDispositionCondition : CardCondition
{
    public EffectTargetSelector targets = EffectTargetSelector.PrimaryTarget;
    public StatusDisposition disposition = StatusDisposition.Debuff;
    public int minimumMagnitude = 1;

    public override bool IsMet(CardContext context)
    {
        foreach (var target in targets.Resolve(context))
            if (target.Statuses.HasDisposition(disposition, minimumMagnitude)) return true;
        return false;
    }
}

public enum CombatantValue
{
    Hp,
    MissingHp,
    Block,
    StatusStacks,
    DebuffStacks,
    BuffStacks,
}

[System.Serializable]
public class CombatantValueCondition : CardCondition
{
    public EffectTargetSelector targets = EffectTargetSelector.PrimaryTarget;
    public CombatantValue value;
    public StatusDefinition status;
    public IntComparison comparison;
    public int amount;

    public override bool IsMet(CardContext context)
    {
        foreach (var target in targets.Resolve(context))
        {
            int current = value switch
            {
                CombatantValue.Hp => target.HP,
                CombatantValue.MissingHp => Math.Max(0, target.MaxHP - target.HP),
                CombatantValue.Block => target.Block,
                CombatantValue.StatusStacks => target.Statuses.GetMagnitude(status),
                CombatantValue.DebuffStacks => target.Statuses.GetTotalMagnitude(StatusDisposition.Debuff),
                CombatantValue.BuffStacks => target.Statuses.GetTotalMagnitude(StatusDisposition.Buff),
                _ => 0,
            };
            if (Compare(current, comparison, amount)) return true;
        }
        return false;
    }
}

[System.Serializable]
public class AllConditions : CardCondition
{
    [UnityEngine.SerializeReference, SubclassPicker] public System.Collections.Generic.List<CardCondition> conditions = new();

    public override bool IsMet(CardContext context)
    {
        if (conditions.Count == 0) return false;
        foreach (var condition in conditions)
            if (condition == null || !condition.IsMet(context)) return false;
        return true;
    }
}

[System.Serializable]
public class AnyCondition : CardCondition
{
    [UnityEngine.SerializeReference, SubclassPicker] public System.Collections.Generic.List<CardCondition> conditions = new();

    public override bool IsMet(CardContext context)
    {
        foreach (var condition in conditions)
            if (condition != null && condition.IsMet(context)) return true;
        return false;
    }
}

[System.Serializable]
public class NotCondition : CardCondition
{
    [UnityEngine.SerializeReference, SubclassPicker] public CardCondition condition;
    public override bool IsMet(CardContext context) => condition != null && !condition.IsMet(context);
}

// 기존 직렬화 타입을 보존하기 위한 호환 셸. 신규 데이터에서는 HasStatusCondition을 사용한다.
[Obsolete("Use HasStatusCondition with an explicit target selector.")]
[System.Serializable]
public class PlayerHasStatusCondition : HasStatusCondition
{
    public PlayerHasStatusCondition() => targets = EffectTargetSelector.Source;
}
