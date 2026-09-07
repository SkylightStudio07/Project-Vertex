[System.Serializable]
public class BlockEffect : CardEffect
{
    public int amount;
    public EffectValue scaling;
    public EffectTargetSelector targets = EffectTargetSelector.Source;

    // 시전자 기준으로 방어도를 부여한다. CardEffect는 적 행동 패턴(EnemyAction)에서도 재사용되므로
    // 무조건 Player에 주면 적의 방어 패턴이 플레이어에게 방어도를 주는 버그가 된다 (실제로 있었음).
    public override void Execute(CardContext context)
    {
        foreach (var target in targets.Resolve(context))
            target.AddBlock(amount + scaling.Evaluate(context, target));
    }

    // 표시용 보정 방어도 — 패시브의 방어도 보정 반영. 음수 보정으로 획득량이 0 미만이면 0으로 표시.
    public override int GetDisplayValue(string fieldName, int rawValue, BattleState state, CardData card, EnemyInstance target = null)
    {
        if (fieldName != nameof(amount) || state?.Player == null) return rawValue;
        return System.Math.Max(0, state.Player.PreviewBlockGain(rawValue));
    }
}
