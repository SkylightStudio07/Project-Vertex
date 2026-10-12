[System.Serializable]
public class IncreaseMagazineEffect : CardEffect
{
    public WeaponData weapon;
    public int amount = 1;
    public override void Execute(CardContext context) => context?.State?.IncreaseMagazine(weapon, amount);
}
