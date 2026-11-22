[System.Serializable]
public class BlockEffect : CardEffect
{
    public int amount;
    public EffectValue scaling;
    public EffectTargetSelector targets = EffectTargetSelector.Source;
    [UnityEngine.Tooltip("켜면 민첩·손상 등 방어도 보정 없이 그대로 준다. 부여 시점에 수치를 고정한 상태(ApplyStatusEffect.snapshotPotencyAsBlock)에서 쓸 것")]
    public bool ignoreBlockModifiers;

    // 시전자 기준으로 방어도를 부여한다. CardEffect는 적 행동 패턴(EnemyAction)에서도 재사용되므로
    // 무조건 Player에 주면 적의 방어 패턴이 플레이어에게 방어도를 주는 버그가 된다 (실제로 있었음).
    public override void Execute(CardContext context)
    {
        foreach (var target in targets.Resolve(context))
        {
            int value = amount + scaling.Evaluate(context, target);
            if (!ignoreBlockModifiers) { target.AddBlock(value); continue; }
            switch (target)
            {
                case PlayerCombatant player: player.AddBlockRaw(value); break;
                case EnemyInstance enemy: enemy.AddBlockRaw(value); break;
                default: target.AddBlock(value); break;
            }
        }
    }

    // 표시용 보정 방어도 — 패시브의 방어도 보정 반영. 음수 보정으로 획득량이 0 미만이면 0으로 표시.
    public override int GetDisplayValue(string fieldName, int rawValue, BattleState state, CardData card, EnemyInstance target = null)
    {
        if (fieldName != nameof(amount) || state?.Player == null || ignoreBlockModifiers) return rawValue;
        return System.Math.Max(0, state.Player.PreviewBlockGain(rawValue));
    }
}
