[System.Serializable]
public class LoseHPEffect : CardEffect
{
    public int amount;
    public EffectTargetSelector targets = EffectTargetSelector.Source;

    public override void Execute(CardContext context)
    {
        foreach (var target in targets.Resolve(context))
            target.TakeDamage(new DamageInfo(amount, context.Source, true));
    }
}
