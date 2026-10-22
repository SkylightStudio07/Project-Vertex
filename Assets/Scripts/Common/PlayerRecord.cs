using System;
using System.Collections.Generic;
using UnityEngine;

// 런을 넘어 남는 플레이 기록. 훈련장(·마테리얼) 해금 조건에 쓴다. PlayerPrefs JSON 하나에 저장한다.
//   - 적 격퇴 수: 실전 전투에서 적이 쓰러질 때 (훈련 전투는 세지 않음) — BattleManager.SetupEnemies
//   - 얻은 카드: 런 중 플레이어 덱에 카드가 들어올 때 — DeckManager.AddCardToPlayerDeck
//   - 동료 호감도: 레벨·포인트가 바뀔 때 — CooperationManager
//   - 무기고 강화 단계: 무기별 — WeaponUpgrades
//   - 얻은 아이템: 인벤토리에 처음 들어올 때 — ItemInventoryManager.AddItem
//   - 런 기록: 런이 끝날 때(클리어·패배) — RunClearView
// 키는 에셋 이름이다(Instantiate한 복사본의 "(Clone)"은 떼고 센다).
public static class PlayerRecord
{
    private const string PrefsKey = "vertex.player_record";

    [Serializable]
    private class CountEntry
    {
        public string id;
        public int count;
    }

    [Serializable]
    private class CoopEntry
    {
        public string id;
        public int level;
        public int point;
    }

    [Serializable]
    private class SaveData
    {
        public List<CountEntry> defeats = new();
        public List<string> obtainedCards = new();
        public List<CoopEntry> coop = new();
        public List<CountEntry> weaponUpgrades = new();
        public List<string> obtainedItems = new();
        public List<RunEntry> runs = new(); // 오래된 것부터
    }

    // 끝난 런 하나 (아카이브 > 플레이 기록). 훈련장은 남기지 않는다
    [Serializable]
    public class RunEntry
    {
        public long endedTicks;   // DateTime.Now.Ticks
        public bool cleared;
        public int act;
        public int floor;         // 1부터
        public int kills;
        public int turns;
        public int cards;
        public int talin;
        public DateTime Ended => new(endedTicks);
    }

    public const int MaxRuns = 100;

    private static SaveData s_data;
    private static SaveData Data => s_data ??= Load();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => s_data = null;

    public static string KeyOf(UnityEngine.Object asset)
        => asset == null ? null : asset.name.Replace("(Clone)", "").Trim();

    // ── 적 격퇴 ──
    public static int GetDefeatCount(EnemyData enemy)
    {
        string key = KeyOf(enemy);
        var entry = key != null ? Data.defeats.Find(e => e.id == key) : null;
        return entry != null ? entry.count : 0;
    }

    public static void AddDefeat(EnemyData enemy)
    {
        string key = KeyOf(enemy);
        if (key == null) return;
        var entry = Data.defeats.Find(e => e.id == key);
        if (entry == null) Data.defeats.Add(entry = new CountEntry { id = key });
        entry.count++;
        Save();
        // 마테리얼 편이 새로 열리면 거점 개발도 +1 (편마다 한 번)
        int step = MaterialArchive.DefeatStep(enemy.rank);
        if (step > 0 && entry.count % step == 0 && entry.count / step <= MaterialArchive.EntryCount)
            MetaProgress.AwardDevelopment($"material:{key}:{entry.count / step}", MetaProgress.DevMaterial);
    }

    // ── 얻은 카드 ──
    public static bool HasObtained(CardData card)
    {
        string key = KeyOf(card);
        return key != null && Data.obtainedCards.Contains(key);
    }

    public static void AddObtainedCard(CardData card)
    {
        string key = KeyOf(card);
        if (key == null || Data.obtainedCards.Contains(key)) return;
        Data.obtainedCards.Add(key);
        Save();
    }

    // ── 얻은 아이템 (아카이브 > 아이템 도감) ──
    public static bool HasObtained(ItemData item)
    {
        string key = KeyOf(item);
        return key != null && Data.obtainedItems.Contains(key);
    }

    public static void AddObtainedItem(ItemData item)
    {
        string key = KeyOf(item);
        if (key == null || Data.obtainedItems.Contains(key)) return;
        Data.obtainedItems.Add(key);
        Save();
    }

    // ── 런 기록 (아카이브 > 플레이 기록) ──
    public static int RunCount => Data.runs.Count;

    // 최근 것부터
    public static IEnumerable<RunEntry> Runs()
    {
        for (int i = Data.runs.Count - 1; i >= 0; i--) yield return Data.runs[i];
    }

    public static void AddRun(RunEntry entry)
    {
        if (entry == null) return;
        Data.runs.Add(entry);
        if (Data.runs.Count > MaxRuns) Data.runs.RemoveRange(0, Data.runs.Count - MaxRuns);
        Save();
    }

    // ── 동료 호감도 ──
    public static bool TryGetCoop(string charID, out int level, out int point)
    {
        var entry = Data.coop.Find(e => e.id == charID);
        level = entry != null ? entry.level : 0;
        point = entry != null ? entry.point : 0;
        return entry != null;
    }

    public static IEnumerable<(string id, int level)> AllCoop()
    {
        foreach (var e in Data.coop) yield return (e.id, e.level);
    }

    public static void SetCoop(string charID, int level, int point)
    {
        if (string.IsNullOrEmpty(charID)) return;
        var entry = Data.coop.Find(e => e.id == charID);
        if (entry == null) Data.coop.Add(entry = new CoopEntry { id = charID });
        entry.level = level;
        entry.point = point;
        Save();
    }

    // ── 무기고 강화 ──
    public static int GetWeaponUpgrade(WeaponData weapon)
    {
        string key = KeyOf(weapon);
        var entry = key != null ? Data.weaponUpgrades.Find(e => e.id == key) : null;
        return entry != null ? entry.count : 0;
    }

    public static void SetWeaponUpgrade(WeaponData weapon, int level)
    {
        string key = KeyOf(weapon);
        if (key == null) return;
        var entry = Data.weaponUpgrades.Find(e => e.id == key);
        if (entry == null) Data.weaponUpgrades.Add(entry = new CountEntry { id = key });
        entry.count = Mathf.Max(0, level);
        Save();
    }

    private static SaveData Load()
    {
        string json = PlayerPrefs.GetString(PrefsKey, string.Empty);
        if (string.IsNullOrEmpty(json)) return new SaveData();
        try { return JsonUtility.FromJson<SaveData>(json) ?? new SaveData(); }
        catch (Exception) { return new SaveData(); }
    }

    private static void Save()
    {
        PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Vertex/기록/플레이 기록 초기화")]
    private static void ClearRecord()
    {
        PlayerPrefs.DeleteKey(PrefsKey);
        s_data = null;
        Debug.Log("[PlayerRecord] 플레이 기록 초기화");
    }
#endif
}
