[System.Serializable]
public class BlockEffect : CardEffect
{
    public int amount;

    // 시전자 기준으로 방어도를 부여한다. CardEffect는 적 행동 패턴(EnemyAction)에서도 재사용되므로
    // 무조건 Player에 주면 적의 방어 패턴이 플레이어에게 방어도를 주는 버그가 된다 (실제로 있었음).
    public override void Execute(CardContext context)
    {
        if (context.ActingEnemy != null)
            context.ActingEnemy.AddBlock(amount);
        else
            context.State?.Player?.AddBlock(amount);
    }

    // 표시용 보정 방어도 — 민첩(DexterityStatus) 반영. 음수 보정으로 획득량이 0 미만이면 0으로 표시.
    public override int GetDisplayValue(string fieldName, int rawValue, BattleState state, CardData card, EnemyInstance target = null)
    {
        if (fieldName != nameof(amount) || state?.Player == null) return rawValue;
        return System.Math.Max(0, state.Player.PreviewBlockGain(rawValue));
    }
}
