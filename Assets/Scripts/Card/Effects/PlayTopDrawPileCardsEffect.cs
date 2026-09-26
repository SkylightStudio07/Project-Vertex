using System;

[Serializable]
public sealed class PlayTopDrawPileCardsEffect : CardEffect
{
    public int count = 3;

    public override void Execute(CardContext context)
    {
        if (count <= 0) return;
        context?.Battle?.PlayTopDrawPileCardsForFree(count);
    }
}
