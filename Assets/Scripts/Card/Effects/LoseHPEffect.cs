[System.Serializable]
public class LoseHPEffect : CardEffect
{
    public int amount;
    public EffectValue scaling;
    public EffectTargetSelector targets = EffectTargetSelector.Source;

    public override void Execute(CardContext context)
    {
        foreach (var target in targets.Resolve(context))
        {
            int value = amount + scaling.Evaluate(context, target);
            if (value > 0) target.TakeDamage(new DamageInfo(value, context.Source, true));
        }
    }
}
