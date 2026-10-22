using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// 아카이브용 기록(얻은 아이템 · 런 기록). PlayerRecord 저장값을 보관·복원한다.
public class PlayerRecordArchiveTests
{
    private const string Key = "vertex.player_record";
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
        => typeof(PlayerRecord).GetField("s_data", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);

    [Test]
    public void ObtainedItem_IgnoresCloneSuffix()
    {
        var item = ScriptableObject.CreateInstance<ItemData>();
        item.name = "섬광탄";
        var clone = Object.Instantiate(item);
        Assert.IsFalse(PlayerRecord.HasObtained(item));
        PlayerRecord.AddObtainedItem(clone);
        Assert.IsTrue(PlayerRecord.HasObtained(item));
        Object.DestroyImmediate(item); Object.DestroyImmediate(clone);
    }

    [Test]
    public void Runs_NewestFirst_AndCapped()
    {
        for (int i = 1; i <= PlayerRecord.MaxRuns + 5; i++)
            PlayerRecord.AddRun(new PlayerRecord.RunEntry { act = 1, floor = i });
        Assert.AreEqual(PlayerRecord.MaxRuns, PlayerRecord.RunCount);
        var runs = PlayerRecord.Runs().ToList();
        Assert.AreEqual(PlayerRecord.MaxRuns + 5, runs[0].floor, "최근 것이 먼저");
        Assert.AreEqual(6, runs[^1].floor, "오래된 것부터 버린다");
    }
}
