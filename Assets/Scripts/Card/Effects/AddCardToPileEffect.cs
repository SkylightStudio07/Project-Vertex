using System.Collections.Generic;

public enum AddCardToPileSourceMode
{
    FixedCard,
    RandomFromPlayerDeck
}

[System.Serializable]
public class AddCardToPileEffect : CardEffect
{
    public AddCardToPileSourceMode sourceMode = AddCardToPileSourceMode.FixedCard;
    public CardData cardToAdd;
    public PileType targetPile = PileType.DiscardPile;
    public int count = 1;
    public bool allowDuplicateRandomCards;

    public override void Execute(CardContext context)
    {
        var cardsToAdd = ResolveCardsToAdd(context);

        if (targetPile == PileType.Hand)
        {
            context.Battle.AddCardsToHand(cardsToAdd);
            return;
        }

        foreach (var card in cardsToAdd)
        {
            switch (targetPile)
            {
                case PileType.DrawPile:    context.Battle.AddCardToDrawPile(card);    break;
                case PileType.DiscardPile: context.Battle.AddCardToDiscardPile(card); break;
            }
        }
    }

    private List<CardData> ResolveCardsToAdd(CardContext context)
    {
        if (sourceMode == AddCardToPileSourceMode.RandomFromPlayerDeck)
            return context.Battle.GetRandomCardsFromPlayerDeck(count, allowDuplicateRandomCards);

        var cardsToAdd = new List<CardData>(count);
        for (int i = 0; i < count; i++)
            cardsToAdd.Add(cardToAdd);

        return cardsToAdd;
    }
}
