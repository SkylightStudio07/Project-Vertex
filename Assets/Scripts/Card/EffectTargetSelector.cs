using System;
using System.Collections.Generic;

public enum EffectTargetMode
{
    Source,
    PrimaryTarget,
    AllOpponents,
    RandomOpponent,
}

[Serializable]
public struct EffectTargetSelector
{
    public EffectTargetMode mode;

    public EffectTargetSelector(EffectTargetMode mode) => this.mode = mode;

    public static EffectTargetSelector Source => new(EffectTargetMode.Source);
    public static EffectTargetSelector PrimaryTarget => new(EffectTargetMode.PrimaryTarget);

    public override string ToString() => mode switch
    {
        EffectTargetMode.Source => "자신",
        EffectTargetMode.PrimaryTarget => "선택한 대상",
        EffectTargetMode.AllOpponents => "모든 적",
        EffectTargetMode.RandomOpponent => "무작위 적",
        _ => mode.ToString(),
    };

    public List<ICombatant> Resolve(CardContext context)
    {
        var result = new List<ICombatant>();
        if (context?.State == null) return result;

        ICombatant source = context.Source;
        switch (mode)
        {
            case EffectTargetMode.Source:
                AddAlive(result, source);
                break;
            case EffectTargetMode.PrimaryTarget:
                AddAlive(result, context.PrimaryTarget);
                break;
            case EffectTargetMode.AllOpponents:
                AddOpponents(result, context, source);
                break;
            case EffectTargetMode.RandomOpponent:
                var opponents = new List<ICombatant>();
                AddOpponents(opponents, context, source);
                if (opponents.Count > 0)
                {
                    int index = context.Battle != null
                        ? context.Battle.Rnd.Next(opponents.Count)
                        : UnityEngine.Random.Range(0, opponents.Count);
                    result.Add(opponents[index]);
                }
                break;
        }
        return result;
    }

    private static void AddOpponents(List<ICombatant> result, CardContext context, ICombatant source)
    {
        if (source is EnemyInstance)
        {
            AddAlive(result, context.State.Player);
            return;
        }

        if (context.State.Enemies == null) return;
        foreach (var enemy in context.State.Enemies) AddAlive(result, enemy);
    }

    private static void AddAlive(List<ICombatant> result, ICombatant combatant)
    {
        if (combatant != null && !combatant.IsDead) result.Add(combatant);
    }
}
