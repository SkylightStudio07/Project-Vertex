using System;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// filename   : BlessingAffinityManager.cs
// description: 축복 대상자(마키나 등)와의 친밀도(Affinity) 및
//              축복 관련 영구 이벤트 플래그를 관리하는 싱글톤 매니저.
//              PlayerPrefs를 통해 런이 종료되어도 영구 누적 보존됩니다.
// ============================================================

public class BlessingAffinityManager : MonoBehaviour
{
    private static BlessingAffinityManager instance;
    public static BlessingAffinityManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BlessingAffinityManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("[BlessingAffinityManager]");
                    instance = go.AddComponent<BlessingAffinityManager>();
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return instance;
        }
    }

    private const string PREFS_AFFINITY_PREFIX = "VERTEX_BLESSING_AFFINITY_";
    private const string PREFS_FLAGS_KEY = "VERTEX_BLESSING_FLAGS";

    private readonly Dictionary<string, float> affinityMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> eventFlags = new(StringComparer.OrdinalIgnoreCase);

    public event Action<string, float> OnAffinityChanged;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (transform.parent == null && Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
            Load();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 대상 엔티티의 현재 친밀도 반환
    /// </summary>
    public float GetAffinity(string entityId)
    {
        if (string.IsNullOrEmpty(entityId)) return 0f;
        if (!affinityMap.TryGetValue(entityId, out float val))
        {
            val = PlayerPrefs.GetFloat(PREFS_AFFINITY_PREFIX + entityId, 0f);
            affinityMap[entityId] = val;
        }
        return val;
    }

    /// <summary>
    /// 대상 엔티티의 친밀도 증가 (+0.5 조우, +1.0 교감 등)
    /// </summary>
    public void AddAffinity(string entityId, float amount)
    {
        if (string.IsNullOrEmpty(entityId) || amount <= 0f) return;

        float current = GetAffinity(entityId);
        float updated = current + amount;
        affinityMap[entityId] = updated;

        PlayerPrefs.SetFloat(PREFS_AFFINITY_PREFIX + entityId, updated);
        PlayerPrefs.Save();

        Debug.Log($"<color=#38BDF8><b>[BlessingAffinity]</b> {entityId} 친밀도 +{amount:F1} 적립! (현재: {updated:F1}, Tier: {GetAffinityTier(entityId)})</color>");
        OnAffinityChanged?.Invoke(entityId, updated);
    }

    /// <summary>
    /// 대상 엔티티의 친밀도 직접 지정
    /// </summary>
    public void SetAffinity(string entityId, float value)
    {
        if (string.IsNullOrEmpty(entityId)) return;

        affinityMap[entityId] = value;
        PlayerPrefs.SetFloat(PREFS_AFFINITY_PREFIX + entityId, value);
        PlayerPrefs.Save();

        Debug.Log($"[BlessingAffinity] {entityId} 친밀도 수동 변경: {value:F1}");
        OnAffinityChanged?.Invoke(entityId, value);
    }

    /// <summary>
    /// 친밀도 티어 계산 (0: 0.0~1.9, 1: 2.0~3.9, 2: 4.0~5.9, 3: 6.0~7.9, 4: 8.0+)
    /// </summary>
    public int GetAffinityTier(string entityId)
    {
        float aff = GetAffinity(entityId);
        if (aff < 2.0f) return 0;
        if (aff < 4.0f) return 1;
        if (aff < 6.0f) return 2;
        if (aff < 8.0f) return 3;
        return 4;
    }

    /// <summary>
    /// 특정 이벤트 플래그 보유 여부
    /// </summary>
    public bool HasFlag(string flagId)
    {
        if (string.IsNullOrEmpty(flagId)) return false;
        return eventFlags.Contains(flagId);
    }

    /// <summary>
    /// 이벤트 플래그 활성화 또는 비활성화
    /// </summary>
    public void SetFlag(string flagId, bool value = true)
    {
        if (string.IsNullOrEmpty(flagId)) return;

        bool changed = false;
        if (value)
        {
            if (eventFlags.Add(flagId)) changed = true;
        }
        else
        {
            if (eventFlags.Remove(flagId)) changed = true;
        }

        if (changed)
        {
            SaveFlags();
            Debug.Log($"[BlessingAffinity] 이벤트 플래그 {(value ? "획득" : "제거")}: {flagId}");
        }
    }

    /// <summary>
    /// 모든 활성 플래그 복사본 반환
    /// </summary>
    public HashSet<string> GetAllFlags()
    {
        return new HashSet<string>(eventFlags);
    }

    private void SaveFlags()
    {
        string serialized = string.Join(";", eventFlags);
        PlayerPrefs.SetString(PREFS_FLAGS_KEY, serialized);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        eventFlags.Clear();
        string serialized = PlayerPrefs.GetString(PREFS_FLAGS_KEY, string.Empty);
        if (!string.IsNullOrEmpty(serialized))
        {
            string[] tokens = serialized.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in tokens)
            {
                if (!string.IsNullOrWhiteSpace(t)) eventFlags.Add(t.Trim());
            }
        }
    }

    [ContextMenu("Reset All Blessing Data (Affinity & Flags)")]
    public void ResetAll()
    {
        affinityMap.Clear();
        eventFlags.Clear();

        PlayerPrefs.DeleteKey(PREFS_AFFINITY_PREFIX + "machina");
        PlayerPrefs.DeleteKey(PREFS_FLAGS_KEY);
        PlayerPrefs.Save();

        Debug.LogWarning("[BlessingAffinity] 모든 축복 친밀도 및 플래그 데이터가 초기화되었습니다.");
    }
}
