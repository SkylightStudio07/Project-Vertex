[System.Serializable]
public class AddAmmoEffect : CardEffect
{
    public int amount;
    public EffectValue scaling;

    public override void Execute(CardContext context)
    {
        if (context.State == null) return;
        context.State.GainAmmo(amount + scaling.Evaluate(context));
    }
}
