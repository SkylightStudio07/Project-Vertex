// 무기고 영구 강화 (로비). 단계는 PlayerRecord에 무기별로 저장되고, 전투 시작 때 BattleState가 읽는다.
// 강화 종류·단계별 수치·비용은 WeaponData 인스펙터의 "무기고 강화"에서 정한다.
// TODO: 강화 재화가 정해지면 TryUpgrade에서 비용(WeaponData.UpgradeCostTo)을 차감한다. 지금은 단계만 올린다.
public static class WeaponUpgrades
{
    public static int GetLevel(WeaponData weapon) => PlayerRecord.GetWeaponUpgrade(weapon);

    public static bool CanUpgrade(WeaponData weapon)
        => weapon != null && GetLevel(weapon) < weapon.MaxUpgradeLevel;

    public static bool TryUpgrade(WeaponData weapon)
    {
        if (!CanUpgrade(weapon)) return false;
        PlayerRecord.SetWeaponUpgrade(weapon, GetLevel(weapon) + 1);
        return true;
    }
}
