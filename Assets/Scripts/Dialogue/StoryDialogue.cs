using System.Collections.Generic;
using UnityEngine;

// 스토리 진행도(영구 플래그)에 따라 골라 재생하는 대화 한 편.
// 목록에서 위에서부터 조건을 검사해 처음 맞는 것 하나를 고른다 — 구체적인 조건(진행이 많이 된 쪽)을 위에 둔다.
// 플래그는 BlessingAffinityManager에 PlayerPrefs로 영구 저장된다(런이 끝나도 유지).
[System.Serializable]
public class StoryDialogue
{
    [Tooltip("이 플래그가 모두 켜져 있어야 재생 (비우면 조건 없음)")]
    public List<string> requiredFlags = new();
    [Tooltip("이 플래그 중 하나라도 켜져 있으면 재생하지 않음 (예: 첫 만남 대사는 'met' 플래그가 있으면 제외)")]
    public List<string> blockedByFlags = new();
    [Tooltip("다이얼로그 JSON (포맷: Assets/Data/Dialogue/지침.md)")]
    public TextAsset dialogueJson;
    [Tooltip("재생을 마치면 켤 플래그 (비우면 없음)")]
    public string setFlagOnComplete;

    public bool IsAvailable()
    {
        if (dialogueJson == null) return false;
        var flags = BlessingAffinityManager.Instance;
        foreach (var f in requiredFlags)
            if (!string.IsNullOrWhiteSpace(f) && !flags.HasFlag(f)) return false;
        foreach (var f in blockedByFlags)
            if (!string.IsNullOrWhiteSpace(f) && flags.HasFlag(f)) return false;
        return true;
    }

    public void MarkPlayed()
    {
        if (!string.IsNullOrWhiteSpace(setFlagOnComplete))
            BlessingAffinityManager.Instance.SetFlag(setFlagOnComplete, true);
    }

    public static StoryDialogue Pick(IReadOnlyList<StoryDialogue> candidates)
    {
        if (candidates == null) return null;
        foreach (var d in candidates)
            if (d != null && d.IsAvailable()) return d;
        return null;
    }
}
