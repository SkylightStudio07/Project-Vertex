[System.Serializable]
public class HealEffect : CardEffect
{
    public int amount;

    public override void Execute(CardContext context) 
    { 
        if(GameManager.Instance != null){
            GameManager.Instance.HealPlayer(amount);
        }
    }
}
