[System.Serializable]
public class ChangeWeaponEffect : CardEffect
{
    public WeaponData weapon;

    public override void Execute(CardContext context)
    {
        // 플레이어에 적용하는 전투 효과. 빈 이벤트 Context에는 적용하지 않는다.
        if (context?.State?.Player == null) return;
        if (context.Battle != null && context.Battle.State == context.State)
            context.Battle.ChangePlayerWeapon(weapon);
        else
            context.State.ChangePlayerWeapon(weapon);
    }
}
