// 무기고 영구 강화 (로비). 단계는 PlayerRecord에 무기별로 저장되고, 전투 시작 때 BattleState가 읽는다.
// 강화 종류·단계별 수치·비용은 WeaponData 인스펙터의 "무기고 강화"에서 정한다.
// 비용: 파츠 (WeaponData.UpgradeCostTo). 마지막 단계는 무기고 시설이 강화(Lv.2)되어 있어야 한다.
public static class WeaponUpgrades
{
    public enum Result { Ok, MaxLevel, NotEnoughParts, NeedsFacility, InvalidWeapon }

    public static int GetLevel(WeaponData weapon) => PlayerRecord.GetWeaponUpgrade(weapon);

    // 다음 단계로 올리는 파츠
    public static int NextCost(WeaponData weapon) => weapon == null ? 0 : weapon.UpgradeCostTo(GetLevel(weapon) + 1);

    // 다음 단계가 마지막 단계면 시설 강화가 필요하다 (최대 단계가 2 이상일 때만)
    public static bool NextNeedsFacility(WeaponData weapon)
        => weapon != null && weapon.MaxUpgradeLevel >= 2 && GetLevel(weapon) + 1 == weapon.MaxUpgradeLevel;

    public static bool CanUpgrade(WeaponData weapon)
        => weapon != null && GetLevel(weapon) < weapon.MaxUpgradeLevel;

    public static Result Check(WeaponData weapon, bool facilityUpgraded)
    {
        if (weapon == null) return Result.InvalidWeapon;
        if (!CanUpgrade(weapon)) return Result.MaxLevel;
        if (NextNeedsFacility(weapon) && !facilityUpgraded) return Result.NeedsFacility;
        if (MetaProgress.Parts < NextCost(weapon)) return Result.NotEnoughParts;
        return Result.Ok;
    }

    public static Result TryUpgrade(WeaponData weapon, bool facilityUpgraded)
    {
        var result = Check(weapon, facilityUpgraded);
        if (result != Result.Ok) return result;
        if (!MetaProgress.TrySpendParts(NextCost(weapon))) return Result.NotEnoughParts;
        PlayerRecord.SetWeaponUpgrade(weapon, GetLevel(weapon) + 1);
        return Result.Ok;
    }
}
