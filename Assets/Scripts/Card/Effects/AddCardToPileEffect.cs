using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Add Card To Pile")]
public class AddCardToPileEffect : CardEffect
{
    public CardData cardToAdd;
    public PileType targetPile = PileType.DiscardPile;
    public int count = 1;

    public override void Execute(CardContext context)
    {
        if (targetPile == PileType.Hand)
        {
            var cardsToAdd = new List<CardData>(count);
            for (int i = 0; i < count; i++)
                cardsToAdd.Add(cardToAdd);

            context.Battle.AddCardsToHand(cardsToAdd);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            switch (targetPile)
            {
                case PileType.DrawPile:    context.Battle.AddCardToDrawPile(cardToAdd);    break;
                case PileType.DiscardPile: context.Battle.AddCardToDiscardPile(cardToAdd); break;
            }
        }
    }
}
