[System.Serializable]
public class DrawEffect : CardEffect
{
    public int count;

    public override void Execute(CardContext context)
    {
        context.Battle?.DrawCards(count);
    }
}
