using System;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// filename   : BlessingAffinityManager.cs
// description: 축복 대상자(마키나 등)와의 호감도/친밀도(Affinity) 및
//              축복 관련 영구 이벤트 플래그를 관리하는 싱글톤 매니저.
//              인스펙터에서 실시간 확인 및 슬라이더/버튼으로 디버깅 가능.
//              PlayerPrefs를 통해 런이 종료되어도 영구 보존됩니다.
// ============================================================

[ExecuteAlways]
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

    [Header("🎮 인스펙터 실시간 디버깅")]
    [Tooltip("인스펙터에서 슬라이더를 움직여 마키나 호감도를 즉시 조절할 수 있습니다.")]
    [Range(0f, 15f)]
    [SerializeField] private float machinaAffinity = 0f;

    [Tooltip("현재 호감도에 따른 티어 (0: 0~1.9, 1: 2~3.9, 2: 4~5.9, 3: 6~7.9, 4: 8.0+)")]
    [SerializeField] private int machinaTier = 0;

    [Header("🚩 활성화된 이벤트 플래그 목록")]
    [SerializeField] private List<string> activeFlags = new();

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
            SyncInspectorFields();
        }
        else if (instance != this && Application.isPlaying)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        Load();
        SyncInspectorFields();
    }

    private void Start()
    {
        Load();
        SyncInspectorFields();
    }

    private void OnValidate()
    {
        // 인스펙터에서 슬라이더를 조작했을 때 데이터 즉시 동기화
        affinityMap["machina"] = machinaAffinity;
        machinaTier = GetAffinityTier("machina");

        PlayerPrefs.SetFloat(PREFS_AFFINITY_PREFIX + "machina", machinaAffinity);
        PlayerPrefs.Save();
        OnAffinityChanged?.Invoke("machina", machinaAffinity);
    }

    public void SyncInspectorFields()
    {
        machinaAffinity = GetAffinity("machina");
        machinaTier = GetAffinityTier("machina");
        activeFlags = new List<string>(eventFlags);
    }

    /// <summary>
    /// 대상 엔티티의 현재 호감도 반환
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
    /// 대상 엔티티의 호감도 증가 (+0.5 조우, +1.0 교감 등)
    /// </summary>
    public void AddAffinity(string entityId, float amount)
    {
        if (string.IsNullOrEmpty(entityId) || amount <= 0f) return;

        float current = GetAffinity(entityId);
        float updated = current + amount;
        affinityMap[entityId] = updated;

        PlayerPrefs.SetFloat(PREFS_AFFINITY_PREFIX + entityId, updated);
        PlayerPrefs.Save();

        SyncInspectorFields();
        Debug.Log($"<color=#38BDF8><b>[BlessingAffinity]</b> {entityId} 호감도 +{amount:F1} 적립! (현재: {updated:F1}, Tier: {GetAffinityTier(entityId)})</color>");
        OnAffinityChanged?.Invoke(entityId, updated);
    }

    /// <summary>
    /// 대상 엔티티의 호감도 직접 지정
    /// </summary>
    public void SetAffinity(string entityId, float value)
    {
        if (string.IsNullOrEmpty(entityId)) return;

        affinityMap[entityId] = value;
        PlayerPrefs.SetFloat(PREFS_AFFINITY_PREFIX + entityId, value);
        PlayerPrefs.Save();

        SyncInspectorFields();
        Debug.Log($"[BlessingAffinity] {entityId} 호감도 수동 변경: {value:F1} (Tier: {GetAffinityTier(entityId)})");
        OnAffinityChanged?.Invoke(entityId, value);
    }

    /// <summary>
    /// 호감도 티어 계산 (0: 0.0~1.9, 1: 2.0~3.9, 2: 4.0~5.9, 3: 6.0~7.9, 4: 8.0+)
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
            SyncInspectorFields();
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

        // 마키나 호감도 로드
        affinityMap["machina"] = PlayerPrefs.GetFloat(PREFS_AFFINITY_PREFIX + "machina", 0f);
    }

    [ContextMenu("Tier 0 (0.0) 설정")]
    public void SetTier0() => SetAffinity("machina", 0.0f);

    [ContextMenu("Tier 1 (2.0) 설정")]
    public void SetTier1() => SetAffinity("machina", 2.0f);

    [ContextMenu("Tier 2 (4.0) 설정")]
    public void SetTier2() => SetAffinity("machina", 4.0f);

    [ContextMenu("Tier 3 (6.0) 설정")]
    public void SetTier3() => SetAffinity("machina", 6.0f);

    [ContextMenu("Tier 4 (8.0) 설정")]
    public void SetTier4() => SetAffinity("machina", 8.0f);

    [ContextMenu("호감도 +1.0 증가")]
    public void AddOneAffinity() => AddAffinity("machina", 1.0f);

    [ContextMenu("Reset All Blessing Data (Affinity & Flags)")]
    public void ResetAll()
    {
        affinityMap.Clear();
        eventFlags.Clear();

        PlayerPrefs.DeleteKey(PREFS_AFFINITY_PREFIX + "machina");
        PlayerPrefs.DeleteKey(PREFS_FLAGS_KEY);
        PlayerPrefs.Save();

        SyncInspectorFields();
        Debug.LogWarning("[BlessingAffinity] 모든 축복 호감도 및 플래그 데이터가 초기화되었습니다.");
    }
}
