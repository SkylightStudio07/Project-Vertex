[System.Serializable]
public class HealEffect : CardEffect
{
    public int amount;
    public EffectValue scaling;
    public EffectTargetSelector targets = EffectTargetSelector.Source;

    public override void Execute(CardContext context)
    {
        // 전투 밖(맵에서 아이템 사용 등)에는 전투 대상이 없다. 자신 대상 회복만 런 HP로 처리한다.
        // scaling은 전투 상태가 있어야 계산되므로 이 경로에서는 기본 수치만 적용된다.
        if (context?.State == null)
        {
            if (targets.mode == EffectTargetMode.Source && GameManager.Instance != null)
                GameManager.Instance.HealPlayer(amount);
            return;
        }

        foreach (var target in targets.Resolve(context))
            target.Heal(amount + scaling.Evaluate(context, target));
    }
}
