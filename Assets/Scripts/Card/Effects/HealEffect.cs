[System.Serializable]
public class HealEffect : CardEffect
{
    public int amount;
    public EffectValue scaling;
    public EffectTargetSelector targets = EffectTargetSelector.Source;

    public override void Execute(CardContext context)
    {
        foreach (var target in targets.Resolve(context))
            target.Heal(amount + scaling.Evaluate(context, target));
    }
}
