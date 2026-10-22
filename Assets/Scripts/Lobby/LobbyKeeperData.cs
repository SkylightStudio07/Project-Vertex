using System;
using System.Collections.Generic;
using UnityEngine;

// 로비 시설 담당 캐릭터(보급 담당 · 기록관 담당 · 정비 담당)의 대사와 호감도.
// 런 상점 주인(ShopkeeperData)처럼 호감도는 BlessingAffinityManager에 entityId로 저장한다(등급 0~4: 0 / 2 / 4 / 6 / 8).
// 대사 묶음은 minTier 이상이면 열리고, 열린 묶음을 모두 섞어서 고른다 — 호감도가 오를수록 새 대사가 끼어든다.
[CreateAssetMenu(fileName = "LobbyKeeper", menuName = "Game Asset/Lobby Keeper")]
public class LobbyKeeperData : ScriptableObject
{
    [Serializable]
    public class TieredLines
    {
        [Range(0, 4)] public int minTier;
        [TextArea(1, 3)] public List<string> lines = new();
    }

    [Tooltip("호감도 저장 키 (예: lobby_supply)")]
    public string entityId;
    public string displayName;

    [Header("호감도")]
    [Tooltip("시설 화면을 열 때 오르는 호감도 (로비에 들어올 때마다 한 번)")]
    public float visitAffinityGain = 0.5f;

    [Header("대사")]
    [Tooltip("화면을 열 때")]
    public List<TieredLines> greetingLines = new();
    [Tooltip("캐릭터나 대사창을 눌렀을 때")]
    public List<TieredLines> clickLines = new();
    [Tooltip("가만히 있을 때 idleInterval초마다")]
    public List<TieredLines> idleLines = new();
    [Tooltip("호감도 등급이 오른 뒤 처음 열 때 (minTier = 새 등급과 같은 묶음)")]
    public List<TieredLines> tierUpLines = new();
    [Min(3f)] public float idleInterval = 14f;

    // 열린 묶음 전체에서 무작위 (직전 문구는 피한다)
    public static string Pick(List<TieredLines> groups, int tier, string current)
    {
        var pool = new List<string>();
        foreach (var g in groups)
            if (g != null && g.minTier <= tier && g.lines != null)
                foreach (var l in g.lines)
                    if (!string.IsNullOrWhiteSpace(l)) pool.Add(l);
        if (pool.Count == 0) return null;
        if (pool.Count > 1) pool.Remove(current);
        return pool[UnityEngine.Random.Range(0, pool.Count)];
    }

    public string TierUpLine(int tier)
    {
        var g = tierUpLines.Find(t => t != null && t.minTier == tier && t.lines.Count > 0);
        return g != null ? g.lines[UnityEngine.Random.Range(0, g.lines.Count)] : null;
    }
}
