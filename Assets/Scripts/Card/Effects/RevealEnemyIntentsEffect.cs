using System;
using UnityEngine;

[Serializable]
public sealed class RevealEnemyIntentsEffect : CardEffect
{
    [Min(1)] public int lookaheadCount = 2;

    public override void Execute(CardContext context)
    {
        if (context?.State == null) return;
        context.State.EnemyIntentLookahead = Mathf.Max(context.State.EnemyIntentLookahead, lookaheadCount);
        context.Battle?.RefreshAllEnemyIntents();
    }
}
