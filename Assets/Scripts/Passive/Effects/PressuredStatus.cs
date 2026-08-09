// Oculiel-exclusive debuff. Its stacks persist until Judgment consumes them.
public class PressuredStatus : StatusEffectBase
{
    public PressuredStatus(int stacks) : base(stacks) { }

    public override void TickDown() { }

    public void ConsumeAll() => Stacks = 0;
}
