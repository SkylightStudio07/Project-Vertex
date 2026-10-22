using System;
using System.Collections.Generic;
using UnityEngine;

// 로비 재화와 거점 개발도. 런을 넘어 남는다(PlayerPrefs JSON). 기획: 노션 VERTEX 기획서 v2 > 8. 재화 & 거점 개발도
//   - 탈린: 흔한 화폐. 런 정산(남은 크레딧 1/10)·일반 의뢰로 벌고 로비 상점에서 쓴다
//   - 파츠: 무기 강화·시설 강화. 엘리트·보스·기계 적·의뢰·로비 상점
//   - 아티팩트: 희귀. 막 보스 첫 격파·스토리 의뢰. 새 무기·최종 강화
//   - 거점 개발도: 쓰지 않고 쌓기만 한다. 같은 이정표(key)는 한 번만 인정된다. 레벨마다 시설·강화 해금
// 크레딧(런 골드)은 GameManager가 가진다. 여기서는 런 정산 때 탈린으로 바꿔 줄 뿐이다.
public static class MetaProgress
{
    private const string PrefsKey = "vertex.meta_progress";

    // 개발도 이정표 (기획 8.5)
    public const int DevJoin = 10;         // 동료 첫 합류
    public const int DevBoss = 10;         // 막 보스 첫 격파
    public const int DevStoryQuest = 8;    // 스토리·구출 의뢰 완료
    public const int DevFacilityOpen = 5;  // 시설 열기
    public const int DevFacilityUpgrade = 3;
    public const int DevRankUp = 2;        // 호감도 랭크업
    public const int DevMaterial = 1;      // 마테리얼 편 해금

    // 레벨 n에 필요한 누적 개발도 (Lv.1 = 0)
    private static readonly int[] LevelThresholds = { 0, 10, 30, 60, 100 };

    public const int CreditToTalinDivisor = 10; // 런 정산: 남은 크레딧 1/10
    public const int ShopPartsRestock = 3;      // 스토리 진전마다 로비 상점 파츠 재고를 이만큼으로 채운다

    [Serializable]
    private class SaveData
    {
        public int talin;
        public int parts;
        public int artifacts;
        public int development;
        public List<string> milestones = new();
        public int shopPartsStock;
        public List<ItemCount> gifts = new(); // 로비 상점에서 산 선물 (동료 화면에서 준다)
    }

    [Serializable]
    private class ItemCount
    {
        public string id;
        public int count;
    }

    private static SaveData s_data;
    private static SaveData Data => s_data ??= Load();
    private static bool s_runSettled = true; // 런이 시작되지 않았으면 정산하지 않는다

    public static event Action OnChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { s_data = null; s_runSettled = true; OnChanged = null; }

    public static int Talin => Data.talin;
    public static int Parts => Data.parts;
    public static int Artifacts => Data.artifacts;
    public static int Development => Data.development;
    public static int ShopPartsStock => Data.shopPartsStock;

    // ── 재화 ──
    public static void AddTalin(int amount) => Change(ref Data.talin, amount);
    public static void AddParts(int amount) => Change(ref Data.parts, amount);
    public static void AddArtifacts(int amount) => Change(ref Data.artifacts, amount);
    public static bool TrySpendTalin(int amount) => TrySpend(ref Data.talin, amount);
    public static bool TrySpendParts(int amount) => TrySpend(ref Data.parts, amount);
    public static bool TrySpendArtifacts(int amount) => TrySpend(ref Data.artifacts, amount);

    private static void Change(ref int value, int amount)
    {
        if (amount <= 0) return;
        value += amount;
        Save();
    }

    private static bool TrySpend(ref int value, int amount)
    {
        if (amount < 0 || value < amount) return false;
        if (amount == 0) return true;
        value -= amount;
        Save();
        return true;
    }

    // ── 거점 개발도 ──

    // 이정표(key)를 처음 달성했을 때만 개발도를 올린다. 올렸으면 true
    public static bool AwardDevelopment(string key, int amount)
    {
        if (string.IsNullOrEmpty(key) || amount <= 0 || Data.milestones.Contains(key)) return false;
        Data.milestones.Add(key);
        Data.development += amount;
        Debug.Log($"<color=#0DB8F2>[MetaProgress] 거점 개발도 +{amount} ({key}) → {Data.development} (Lv.{DevelopmentLevel})</color>");
        Save();
        return true;
    }

    // 개발도 없이 이정표만 남긴다 (한 번만 할 일 표시용). 처음이면 true
    public static bool MarkMilestone(string key)
    {
        if (string.IsNullOrEmpty(key) || Data.milestones.Contains(key)) return false;
        Data.milestones.Add(key);
        Save();
        return true;
    }

    public static bool HasMilestone(string key) => !string.IsNullOrEmpty(key) && Data.milestones.Contains(key);

    public static int DevelopmentLevel => LevelOf(Data.development);
    public static int MaxLevel => LevelThresholds.Length;

    public static int LevelOf(int development)
    {
        int level = 1;
        for (int i = 1; i < LevelThresholds.Length; i++)
            if (development >= LevelThresholds[i]) level = i + 1;
        return level;
    }

    // 현재 레벨 구간 안에서의 진행 (최대 레벨이면 total == 0)
    public static (int current, int total) LevelProgress()
    {
        int level = DevelopmentLevel;
        if (level >= LevelThresholds.Length) return (0, 0);
        int from = LevelThresholds[level - 1], to = LevelThresholds[level];
        return (Data.development - from, to - from);
    }

    // ── 로비 상점 파츠 재고 (스토리 진전마다 채움) ──
    public static void RestockShop()
    {
        if (Data.shopPartsStock >= ShopPartsRestock) return;
        Data.shopPartsStock = ShopPartsRestock;
        Save();
    }

    public static bool TryTakeShopParts(int count = 1)
    {
        if (count <= 0 || Data.shopPartsStock < count) return false;
        Data.shopPartsStock -= count;
        Save();
        return true;
    }

    // ── 선물 (로비 상점에서 사고, 동료 화면에서 준다) ──
    public static int GiftCount(string id)
    {
        var e = Data.gifts.Find(g => g.id == id);
        return e != null ? e.count : 0;
    }

    public static void AddGift(string id, int count)
    {
        if (string.IsNullOrEmpty(id) || count <= 0) return;
        var e = Data.gifts.Find(g => g.id == id);
        if (e == null) Data.gifts.Add(e = new ItemCount { id = id });
        e.count += count;
        Save();
    }

    public static bool TryTakeGift(string id, int count = 1)
    {
        var e = Data.gifts.Find(g => g.id == id);
        if (e == null || count <= 0 || e.count < count) return false;
        e.count -= count;
        Save();
        return true;
    }

    // ── 스토리 진전 (보스 첫 격파 등) ──

    // 막 보스를 처음 쓰러뜨렸을 때: 개발도 + 아티팩트 1 + 상점 재고 채움. 처음이었으면 true
    public static bool OnActBossCleared(int act)
    {
        if (!AwardDevelopment($"boss:act{act}", DevBoss)) return false;
        AddArtifacts(1);
        RestockShop();
        return true;
    }

    // ── 런 정산 ──

    public static void BeginRun() => s_runSettled = false;

    // 런이 끝날 때 한 번: 남은 크레딧의 1/10(내림)을 탈린으로. 준 양을 돌려준다
    public static int SettleRun(int credits)
    {
        if (s_runSettled) return 0;
        s_runSettled = true;
        int talin = Mathf.Max(0, credits) / CreditToTalinDivisor;
        AddTalin(talin);
        Debug.Log($"<color=#0DB8F2>[MetaProgress] 런 정산: 크레딧 {credits} → 탈린 +{talin} (보유 {Data.talin})</color>");
        return talin;
    }


    // ── 저장 ──

    private static SaveData Load()
    {
        var json = PlayerPrefs.GetString(PrefsKey, string.Empty);
        if (string.IsNullOrEmpty(json)) return new SaveData();
        try { return JsonUtility.FromJson<SaveData>(json) ?? new SaveData(); }
        catch { return new SaveData(); }
    }

    private static void Save()
    {
        PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
        OnChanged?.Invoke();
    }
}
