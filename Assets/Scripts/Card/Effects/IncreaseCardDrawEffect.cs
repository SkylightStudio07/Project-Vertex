// 매턴 뽑는 카드의 장수를 늘리는 카드이펙트
[System.Serializable]
public class IncreaseCardDrawEffect : CardEffect
{
    public int count;
    public override void Execute(CardContext context)
    {
        if (context.Battle != null)
        {
            context.Battle.State.DrawCount += count;
        }
    }
}
