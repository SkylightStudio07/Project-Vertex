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
public class PlayerHasStatusCondition : CardCondition
{
    public StatusType statusType;
    public int minimumMagnitude = 1;
    public bool debuffOnly;

    public override bool IsMet(CardContext context)
    {
        var player = context.State?.Player;
        if (player == null) return false;

        foreach (var passive in player.Passives)
        {
            if (passive is not StatusEffectBase status) continue;
            if (!MatchesStatusType(status, statusType)) continue;
            if (debuffOnly && !IsDebuff(status)) continue;
            if (Math.Abs(status.Stacks) >= minimumMagnitude) return true;
        }

        return false;
    }

    private static bool MatchesStatusType(StatusEffectBase status, StatusType type)
    {
        return type switch
        {
            StatusType.Weak => status is WeakStatus,
            StatusType.Vulnerable => status is VulnerableStatus,
            StatusType.Poison => status is PoisonStatus,
            StatusType.Strength => status is StrengthStatus,
            StatusType.Dexterity => status is DexterityStatus,
            StatusType.DamageNullified => status is DamageNullifiedStatus,
            _ => false,
        };
    }

    private static bool IsDebuff(StatusEffectBase status)
    {
        return status switch
        {
            WeakStatus => true,
            VulnerableStatus => true,
            PoisonStatus => true,
            StrengthStatus strength => strength.Stacks < 0,
            DexterityStatus dexterity => dexterity.Stacks < 0,
            _ => false,
        };
    }
}
