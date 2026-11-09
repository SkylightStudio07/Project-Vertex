using System.Collections.Generic;

// 행동/패시브가 공유하는 적 생성 효과. 사망한 시전자에게서도 실행할 수 있다.
[System.Serializable]
public sealed class SummonEnemiesEffect : CardEffect
{
    public List<EnemyData> enemies = new();

    public override void Execute(CardContext context)
        => context?.Battle?.SpawnEnemies(enemies);
}
