using System;
using System.Collections.Generic;
using UnityEngine;

// 런을 넘어 남는 플레이 기록. 훈련장(·마테리얼) 해금 조건에 쓴다. PlayerPrefs JSON 하나에 저장한다.
//   - 적 격퇴 수: 실전 전투에서 적이 쓰러질 때 (훈련 전투는 세지 않음) — BattleManager.SetupEnemies
//   - 얻은 카드: 런 중 플레이어 덱에 카드가 들어올 때 — DeckManager.AddCardToPlayerDeck
//   - 동료 호감도: 레벨·포인트가 바뀔 때 — CooperationManager
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
    }

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

    // ── 동료 호감도 ──
    public static bool TryGetCoop(string charID, out int level, out int point)
    {
        var entry = Data.coop.Find(e => e.id == charID);
        level = entry != null ? entry.level : 0;
        point = entry != null ? entry.point : 0;
        return entry != null;
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
