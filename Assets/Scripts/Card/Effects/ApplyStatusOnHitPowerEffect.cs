[System.Serializable]
public class ApplyStatusOnHitPowerEffect : CardEffect
{
    public StatusType statusType;
    public int amount = 1;

    public override void Execute(CardContext context)
    {
        if (context.State?.Player == null) return;
        context.State.Player.AddPassive(new ApplyStatusOnHitPassive(statusType, amount));
    }
}
