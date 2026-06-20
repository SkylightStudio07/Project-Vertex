using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/AddAmmo")]
public class AddAmmoEffect : CardEffect
{
    public int amount;

    public override void Execute(CardContext context)
    {
        if (context.State == null) return;
        context.State.Ammo += amount;
    }
}