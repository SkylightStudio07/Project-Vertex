using System;

[Serializable]
public sealed class GainEnergyEffect : CardEffect
{
    public int amount = 1;
    public EffectValue scaling; // 예: 파워 상태 스택만큼 (겹쳐 쓰면 더 얻음)

    public override void Execute(CardContext context)
    {
        if (context?.State == null) return;
        int value = amount + scaling.Evaluate(context);
        if (value <= 0) return;
        context.State.Energy += value;
    }
}
