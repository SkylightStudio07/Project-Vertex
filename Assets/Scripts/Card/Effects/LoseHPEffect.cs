using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/LoseHP")]
public class LoseHPEffect : CardEffect
{
    public int amount;

    public override void Execute(CardContext context)
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.TakeDamage(amount);
    }
}
