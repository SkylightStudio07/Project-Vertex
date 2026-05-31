using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/GainGold")]
public class GainGoldEffect : CardEffect
{
    public int amount;

    public override void Execute(CardContext context)
    {
        GameManager.Instance.PlayerGold += amount;
    }
}
