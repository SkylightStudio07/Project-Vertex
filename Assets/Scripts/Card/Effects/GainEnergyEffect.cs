using System;

[Serializable]
public sealed class GainEnergyEffect : CardEffect
{
    public int amount = 1;

    public override void Execute(CardContext context)
    {
        if (context?.State == null || amount <= 0) return;
        context.State.Energy += amount;
    }
}
