using System;

[Serializable]
public sealed class EscapeBattleEffect : CardEffect
{
    public override void Execute(CardContext context)
    {
        context?.Battle?.TryEscapeBattle();
    }
}
