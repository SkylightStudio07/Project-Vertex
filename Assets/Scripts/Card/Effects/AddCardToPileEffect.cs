using System.Collections.Generic;

public enum AddCardToPileSourceMode
{
    FixedCard,
    RandomFromPlayerDeck,
    PlayedCardCopy, // 지금 쓰고 있는 카드의 복사본 (강화 상태 그대로) — 알케미스트 '환각'
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
        {
            if (sourceMode == AddCardToPileSourceMode.PlayedCardCopy)
            {
                if (context?.Card == null) break;
                var copy = UnityEngine.Object.Instantiate(context.Card);
                copy.name = context.Card.name;
                cardsToAdd.Add(copy);
            }
            else cardsToAdd.Add(cardToAdd);
        }

        return cardsToAdd;
    }
}
