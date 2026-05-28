using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Draw")]
public class DrawEffect : CardEffect
{
    public int count;

    public override void Execute(CardContext context)
    {
        context.Battle?.DrawCards(count);
    }
}
