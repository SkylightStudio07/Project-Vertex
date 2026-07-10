[System.Serializable]
public class GainGoldEffect : CardEffect
{
    public int amount;

    public override void Execute(CardContext context)
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.PlayerGold += amount;
    }
}
