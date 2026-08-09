[System.Serializable]
public class PressureStatusCardEffect : CardEffect, ICardEndTurnInHandEffect
{
    public int gazeAmount = 1;

    // Playing Pressure only pays its cost and moves it to the discard pile.
    public override void Execute(CardContext context) { }

    public bool OnTurnEndInHand(CardContext context)
    {
        if (context.State?.Player != null && gazeAmount != 0)
            context.State.Player.AddPassive(new PressuredStatus(gazeAmount));

        return true;
    }
}
