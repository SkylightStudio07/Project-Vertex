using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// 로비 스토리 카드의 "지금 챕터" 판정. MetaProgress 이정표를 쓰므로 저장값을 보관·복원한다.
public class StoryProgressTests
{
    private const string Key = "vertex.meta_progress";
    private string _backup;
    private bool _had;
    private StoryChapterList _list;

    [SetUp]
    public void SetUp()
    {
        _had = PlayerPrefs.HasKey(Key);
        _backup = PlayerPrefs.GetString(Key, string.Empty);
        PlayerPrefs.DeleteKey(Key);
        ResetCache();

        _list = ScriptableObject.CreateInstance<StoryChapterList>();
        _list.chapters.Add(Chapter("1-1", Boss(1)));
        _list.chapters.Add(Chapter("1-2", Boss(2), Boss(3)));
        _list.chapters.Add(Chapter("2-1"));
    }

    [TearDown]
    public void TearDown()
    {
        if (_had) PlayerPrefs.SetString(Key, _backup);
        else PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
        ResetCache();
        foreach (var c in _list.chapters) Object.DestroyImmediate(c);
        Object.DestroyImmediate(_list);
    }

    private static void ResetCache()
        => typeof(MetaProgress).GetField("s_data", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);

    private static StoryObjective Boss(int act) => new() { type = StoryObjectiveType.ActBoss, act = act };

    private static StoryChapterData Chapter(string id, params StoryObjective[] objectives)
    {
        var c = ScriptableObject.CreateInstance<StoryChapterData>();
        c.chapterId = id;
        c.objectives.AddRange(objectives);
        return c;
    }

    [Test]
    public void Fresh_StartsAtFirstChapter()
    {
        var cur = _list.GetCurrent();
        Assert.AreEqual("1-1", cur.chapter.chapterId);
        Assert.AreEqual("1막 보스 격파", cur.next.Label);
    }

    [Test]
    public void ClearingObjectives_AdvancesInOrder()
    {
        MetaProgress.MarkMilestone("boss:act1");
        var cur = _list.GetCurrent();
        Assert.AreEqual("1-2", cur.chapter.chapterId);
        Assert.AreEqual(2, cur.next.act);
        Assert.AreEqual(0, cur.doneCount);

        // 순서와 상관없이 남은 첫 목표를 보여 준다
        MetaProgress.MarkMilestone("boss:act3");
        cur = _list.GetCurrent();
        Assert.AreEqual(2, cur.next.act);
        Assert.AreEqual(1, cur.doneCount);
    }

    [Test]
    public void EmptyChapter_StopsAsComingSoon()
    {
        MetaProgress.MarkMilestone("boss:act1");
        MetaProgress.MarkMilestone("boss:act2");
        MetaProgress.MarkMilestone("boss:act3");
        var cur = _list.GetCurrent();
        Assert.AreEqual("2-1", cur.chapter.chapterId);
        Assert.IsNull(cur.next);
        Assert.AreEqual(0, cur.totalCount);
    }

    [Test]
    public void AllCleared_StaysOnLastChapter()
    {
        _list.chapters.RemoveAt(2);
        MetaProgress.MarkMilestone("boss:act1");
        MetaProgress.MarkMilestone("boss:act2");
        MetaProgress.MarkMilestone("boss:act3");
        var cur = _list.GetCurrent();
        Assert.AreEqual("1-2", cur.chapter.chapterId);
        Assert.IsTrue(cur.AllCleared);
        Assert.AreEqual(2, cur.doneCount);
    }
}
