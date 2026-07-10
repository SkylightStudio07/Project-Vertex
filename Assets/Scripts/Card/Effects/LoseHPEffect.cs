[System.Serializable]
public class LoseHPEffect : CardEffect
{
    public int amount;

    public override void Execute(CardContext context)
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.TakeDamage(amount);
    }
}
