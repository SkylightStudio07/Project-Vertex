using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// 재화·거점 개발도. 실제 저장(PlayerPrefs)을 쓰므로 테스트 전후로 원래 값을 보관·복원한다.
public class MetaProgressTests
{
    private const string Key = "vertex.meta_progress";
    private string _backup;
    private bool _had;

    [SetUp]
    public void SetUp()
    {
        _had = PlayerPrefs.HasKey(Key);
        _backup = PlayerPrefs.GetString(Key, string.Empty);
        PlayerPrefs.DeleteKey(Key);
        ResetCache();
    }

    [TearDown]
    public void TearDown()
    {
        if (_had) PlayerPrefs.SetString(Key, _backup);
        else PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
        ResetCache();
    }

    private static void ResetCache()
    {
        var t = typeof(MetaProgress);
        t.GetField("s_data", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);
        t.GetField("s_runSettled", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, true);
    }

    [Test]
    public void Level_FollowsThresholds()
    {
        Assert.AreEqual(1, MetaProgress.LevelOf(0));
        Assert.AreEqual(1, MetaProgress.LevelOf(9));
        Assert.AreEqual(2, MetaProgress.LevelOf(10));
        Assert.AreEqual(3, MetaProgress.LevelOf(59));
        Assert.AreEqual(4, MetaProgress.LevelOf(60));
        Assert.AreEqual(5, MetaProgress.LevelOf(250));
    }

    [Test]
    public void Development_CountsEachMilestoneOnce()
    {
        Assert.IsTrue(MetaProgress.AwardDevelopment("join:Cp_01", 10));
        Assert.IsFalse(MetaProgress.AwardDevelopment("join:Cp_01", 10), "같은 동료 두 번째 합류");
        Assert.AreEqual(10, MetaProgress.Development);
        Assert.AreEqual(2, MetaProgress.DevelopmentLevel);
        Assert.AreEqual((0, 20), MetaProgress.LevelProgress());
    }

    [Test]
    public void RunSettlement_TenthOfCredits_OncePerRun()
    {
        Assert.AreEqual(0, MetaProgress.SettleRun(500), "런이 시작되지 않았으면 정산 없음");
        MetaProgress.BeginRun();
        Assert.AreEqual(34, MetaProgress.SettleRun(349), "1/10 내림");
        Assert.AreEqual(0, MetaProgress.SettleRun(349), "한 런에 한 번");
        Assert.AreEqual(34, MetaProgress.Talin);
    }

    [Test]
    public void Spend_FailsWhenShort()
    {
        MetaProgress.AddParts(3);
        Assert.IsFalse(MetaProgress.TrySpendParts(4));
        Assert.IsTrue(MetaProgress.TrySpendParts(3));
        Assert.AreEqual(0, MetaProgress.Parts);
    }

    [Test]
    public void ActBoss_FirstClearOnly_GivesArtifactAndRestock()
    {
        Assert.IsTrue(MetaProgress.OnActBossCleared(1));
        Assert.IsFalse(MetaProgress.OnActBossCleared(1));
        Assert.AreEqual(1, MetaProgress.Artifacts);
        Assert.AreEqual(MetaProgress.DevBoss, MetaProgress.Development);
        Assert.AreEqual(MetaProgress.ShopPartsRestock, MetaProgress.ShopPartsStock);
        Assert.IsTrue(MetaProgress.TryTakeShopParts());
        Assert.AreEqual(MetaProgress.ShopPartsRestock - 1, MetaProgress.ShopPartsStock);
    }
}
