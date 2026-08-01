[System.Serializable]
public class LoseHPEffect : CardEffect
{
    public int amount;

    public override void Execute(CardContext context)
    {
        if (GameManager.Instance == null) return;
        int hpBefore = GameManager.Instance.PlayerHP;
        GameManager.Instance.TakeDamage(amount);

        if (context.State != null && GameManager.Instance.PlayerHP < hpBefore)
            context.State.PlayerLostHpThisTurn = true;
    }
}
