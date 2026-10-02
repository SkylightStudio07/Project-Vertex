using System;
using System.Collections.Generic;
using UnityEngine;

// 마테리얼(작전 기록 / 도감). FGO 인연 자료처럼 기록이 쌓일수록 글이 하나씩 열린다. 기획: Docs/기획/마테리얼.md
//   적: 격퇴 수로 1~5편 해금. 일반 3회 · 엘리트 2회 · 보스 1회마다 한 편. 첫 편이 열리면 훈련장 상대로도 열린다.
//   동료: 호감도 레벨마다 한 편 (Lv.1~5).
// 글은 JSON(TextAsset)으로 쓰고 EnemyData / CoopCharData의 materialJson에 연결한다. 형식: Docs/기획/마테리얼.md
public static class MaterialArchive
{
    public const int EntryCount = 5;

    [Serializable]
    public class Entry
    {
        public string title;
        [TextArea] public string body;
    }

    [Serializable]
    public class File
    {
        public string id;
        public List<Entry> entries = new();
    }

    private static readonly Dictionary<TextAsset, File> s_cache = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => s_cache.Clear();

    public static File Load(TextAsset json)
    {
        if (json == null) return null;
        if (s_cache.TryGetValue(json, out var cached)) return cached;
        File file;
        try { file = JsonUtility.FromJson<File>(json.text) ?? new File(); }
        catch (Exception e)
        {
            Debug.LogWarning($"[MaterialArchive] '{json.name}' 파싱 실패: {e.Message}");
            file = new File();
        }
        s_cache[json] = file;
        return file;
    }

    // ── 적 ──
    public static int DefeatStep(EnemyEncounterType rank) => rank switch
    {
        EnemyEncounterType.Boss  => 1,
        EnemyEncounterType.Elite => 2,
        _                        => 3,
    };

    // index번째(0부터) 기록이 열리는 격퇴 수
    public static int RequiredDefeats(EnemyData enemy, int index) => DefeatStep(enemy.rank) * (index + 1);

    public static int UnlockedCount(EnemyData enemy)
        => enemy == null ? 0 : Mathf.Clamp(PlayerRecord.GetDefeatCount(enemy) / DefeatStep(enemy.rank), 0, EntryCount);

    // 첫 기록이 열리면 훈련장 상대로도 열린다
    public static bool IsTrainingUnlocked(EnemyData enemy) => UnlockedCount(enemy) >= 1;

    // ── 동료 ──
    public static int UnlockedCount(int coopLevel) => Mathf.Clamp(coopLevel, 0, EntryCount);
}
