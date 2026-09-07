[System.Serializable]
public sealed class ApplyStatusAtEndTurnInHandEffect : CardEffect, ICardEndTurnInHandEffect
{
    public StatusDefinition status;
    public int amount = 1;
    public bool returnToDrawPile = true;

    public override void Execute(CardContext context) { }

    public bool OnTurnEndInHand(CardContext context)
    {
        if (status != null && context?.Source != null)
            context.Source.AddPassive(status.CreateInstance(amount, source: context.Source));
        return returnToDrawPile;
    }
}

