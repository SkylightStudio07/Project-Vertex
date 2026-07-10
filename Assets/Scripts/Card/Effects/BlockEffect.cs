using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Block")]
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
}
