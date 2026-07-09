using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Block")]
public class BlockEffect : CardEffect
{
    public int amount;

    public override void Execute(CardContext context)
    {
        if (context.ActingEnemy != null)
            context.ActingEnemy.AddBlock(amount);
        else
            context.State?.Player?.AddBlock(amount);
    }
}
