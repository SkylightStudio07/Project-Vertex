using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Block")]
public class BlockEffect : CardEffect
{
    public int amount;

    public override void Execute(CardContext context)
    {
        context.State?.Player?.AddBlock(amount);
    }
}
