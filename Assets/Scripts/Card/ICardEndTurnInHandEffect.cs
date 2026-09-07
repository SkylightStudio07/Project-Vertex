/// <summary>
/// A card effect that triggers only when its card is still in the player's hand
/// at the end of the turn.
/// </summary>
public interface ICardEndTurnInHandEffect
{
    /// <returns>True when the card should return to the draw pile instead of being discarded.</returns>
    bool OnTurnEndInHand(CardContext context);
}
