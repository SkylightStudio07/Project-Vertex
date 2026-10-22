using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// 무기고 강화 비용(파츠)·시설 조건, 카드 강화 비용(탈린+파츠). 저장값(MetaProgress·PlayerRecord)을 보관·복원한다.
public class WeaponUpgradeCostTests
{
    private static readonly string[] Keys = { "vertex.meta_progress", "vertex.player_record" };
    private readonly string[] _backup = new string[2];
    private readonly bool[] _had = new bool[2];
    private WeaponData _weapon;

    [SetUp]
    public void SetUp()
    {
        for (int i = 0; i < Keys.Length; i++) { _had[i] = PlayerPrefs.HasKey(Keys[i]); _backup[i] = PlayerPrefs.GetString(Keys[i], ""); PlayerPrefs.DeleteKey(Keys[i]); }
        ResetCaches();
        _weapon = ScriptableObject.CreateInstance<WeaponData>();
        _weapon.name = "test_weapon";
        Field("upgradeType", WeaponUpgradeType.ShotDamage);
        Field("upgradeValues", new[] { 1, 2 });
        Field("upgradeCosts", new[] { 10, 20 });
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = 0; i < Keys.Length; i++) { if (_had[i]) PlayerPrefs.SetString(Keys[i], _backup[i]); else PlayerPrefs.DeleteKey(Keys[i]); }
        PlayerPrefs.Save();
        ResetCaches();
        Object.DestroyImmediate(_weapon);
    }

    private void Field(string name, object value)
        => typeof(WeaponData).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_weapon, value);

    private static void ResetCaches()
    {
        typeof(MetaProgress).GetField("s_data", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);
        typeof(PlayerRecord).GetField("s_data", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);
    }

    [Test]
    public void Upgrade_SpendsParts_PerLevel()
    {
        MetaProgress.AddParts(25);
        Assert.AreEqual(10, WeaponUpgrades.NextCost(_weapon));
        Assert.AreEqual(WeaponUpgrades.Result.Ok, WeaponUpgrades.TryUpgrade(_weapon, facilityUpgraded: false));
        Assert.AreEqual(1, WeaponUpgrades.GetLevel(_weapon));
        Assert.AreEqual(15, MetaProgress.Parts);
        Assert.AreEqual(WeaponUpgrades.Result.NeedsFacility, WeaponUpgrades.Check(_weapon, facilityUpgraded: false), "마지막 단계는 시설 Lv.2");
        Assert.AreEqual(WeaponUpgrades.Result.NotEnoughParts, WeaponUpgrades.TryUpgrade(_weapon, facilityUpgraded: true));
        Assert.AreEqual(15, MetaProgress.Parts);
        MetaProgress.AddParts(5);
        Assert.AreEqual(WeaponUpgrades.Result.Ok, WeaponUpgrades.TryUpgrade(_weapon, facilityUpgraded: true));
        Assert.AreEqual(0, MetaProgress.Parts);
        Assert.AreEqual(WeaponUpgrades.Result.MaxLevel, WeaponUpgrades.Check(_weapon, true));
    }

    [Test]
    public void CardUpgrade_RejectsCardWithoutUpgradedState()
    {
        var card = ScriptableObject.CreateInstance<CardData>();
        MetaProgress.AddTalin(500); MetaProgress.AddParts(10);
        Assert.AreEqual(CardUpgrades.Result.NotUpgradable, CardUpgrades.TryUpgrade(card));
        Assert.AreEqual(500, MetaProgress.Talin);
        Object.DestroyImmediate(card);
    }
}
