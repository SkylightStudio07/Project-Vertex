using System;
using System.Collections.Generic;
using UnityEngine;

// 의뢰 진행 관리 (로비·런 공용, 씬을 넘어 유지). 기획: Docs/의뢰.md
// - 로비: 게시판 목록(Available), 수주(Accept), 복귀 시 보상 수령(ClaimRewards)
// - 런: BeginRun에서 이벤트 구독 → 노드 도달·적 처치로 퀘스트 아이템 지급, 조건 진행
// - 런 종료(클리어·사망): SettleRun에서 완료 판정 → 보상 경험치는 로비에서 받을 때까지 적립
// 영구 상태(수주 목록·완료 목록·미수령 경험치)는 PlayerPrefs에 JSON으로 저장한다.
public class QuestManager : MonoBehaviour
{
    private const string PrefsKey = "VERTEX_QUEST_STATE";
    public const int DefaultMaxActive = 2;

    private static QuestManager _instance;
    public static QuestManager Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = FindObjectOfType<QuestManager>();
            if (_instance == null)
            {
                var go = new GameObject("[QuestManager]");
                _instance = go.AddComponent<QuestManager>();
                if (Application.isPlaying) DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [Serializable]
    private class SaveState
    {
        public List<string> accepted = new();
        public List<string> completed = new();
        public int pendingExp;
        public List<string> pendingCompleted = new(); // 로비에서 완료 연출을 보여줄 의뢰
    }

    // 게시판 카드·런 중 알림용. (의뢰, 알림 문구)
    public event Action<QuestData, string> OnQuestProgress;
    public event Action OnStateChanged;

    public int MaxActive { get; set; } = DefaultMaxActive;
    public int PendingExp => _state.pendingExp;
    public IReadOnlyList<string> PendingCompleted => _state.pendingCompleted;

    private QuestDatabase _db;
    private SaveState _state = new();

    // 런 단위 진행 (런 시작마다 초기화)
    private readonly HashSet<string> _itemGranted = new();
    private readonly HashSet<string> _completedThisRun = new();
    private readonly Dictionary<string, int> _defeatCounts = new();
    private readonly List<QuestData> _pendingGrants = new(); // 칸이 꽉 차 못 준 퀘스트 아이템
    private bool _runActive;
    private BattleManager _subscribedBattle;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        _db = Resources.Load<QuestDatabase>("QuestDatabase");
        Load();
    }

    // ── 로비 ──────────────────────────────────────────────

    public QuestData Find(string questId) => _db != null ? _db.Find(questId) : null;

    public IEnumerable<QuestData> Accepted
    {
        get { foreach (var id in _state.accepted) { var q = Find(id); if (q != null) yield return q; } }
    }

    public bool IsAccepted(string questId) => _state.accepted.Contains(questId);
    public bool IsCompleted(string questId) => _state.completed.Contains(questId);

    // 게시판에 뜰 수 있는 의뢰: 아직 완료 안 했고, 게시 조건 플래그를 모두 만족
    public List<QuestData> Available()
    {
        var list = new List<QuestData>();
        if (_db == null) return list;
        foreach (var q in _db.quests)
            if (q != null && !IsCompleted(q.questId) && FlagsMet(q.requiredFlags)) list.Add(q);
        return list;
    }

    public bool CanAccept(QuestData quest)
        => quest != null && !IsAccepted(quest.questId) && !IsCompleted(quest.questId)
           && _state.accepted.Count < MaxActive && FlagsMet(quest.requiredFlags);

    public bool Accept(QuestData quest)
    {
        if (!CanAccept(quest)) return false;
        _state.accepted.Add(quest.questId);
        Save();
        OnStateChanged?.Invoke();
        return true;
    }

    public void Abandon(QuestData quest)
    {
        if (quest == null || !_state.accepted.Remove(quest.questId)) return;
        Save();
        OnStateChanged?.Invoke();
    }

    // 로비 복귀 시 호출: 적립된 경험치를 로비 경험치로 옮긴다. 옮긴 양을 돌려준다.
    public int ClaimRewards()
    {
        int exp = _state.pendingExp;
        if (exp <= 0 && _state.pendingCompleted.Count == 0) return 0;
        var userExp = UserExpManager.Instance;
        if (userExp == null) return 0;
        if (exp > 0) userExp.AddExperience(exp);
        _state.pendingExp = 0;
        _state.pendingCompleted.Clear();
        Save();
        OnStateChanged?.Invoke();
        return exp;
    }

    // ── 런 ────────────────────────────────────────────────

    // GameManager.InitializeRun에서 호출
    public void BeginRun()
    {
        _itemGranted.Clear();
        _completedThisRun.Clear();
        _defeatCounts.Clear();
        _pendingGrants.Clear();
        _runActive = true;

        MapManager.NodeEntered -= HandleNodeEntered;
        MapManager.NodeEntered += HandleNodeEntered;
        BattleManager.EnemyDefeated -= HandleEnemyDefeated;
        BattleManager.EnemyDefeated += HandleEnemyDefeated;
        if (_subscribedBattle != null) _subscribedBattle.OnBattleDefeat -= SettleRun;
        _subscribedBattle = BattleManager.Instance;
        if (_subscribedBattle != null) _subscribedBattle.OnBattleDefeat += SettleRun;
    }

    // 런 종료(클리어 화면·패배)에서 호출. 한 런에 한 번만 정산한다.
    public void SettleRun()
    {
        if (!_runActive) return;
        _runActive = false;

        foreach (var quest in new List<QuestData>(Accepted))
        {
            bool done = _completedThisRun.Contains(quest.questId)
                        || (quest.goal == QuestGoal.HoldItemAtRunEnd && HoldsQuestItem(quest));
            if (done) Complete(quest);
        }
        // 의뢰는 런 1회 단위. 결과와 상관없이 진행 목록에서 뺀다
        _state.accepted.Clear();
        Save();
        OnStateChanged?.Invoke();

        MapManager.NodeEntered -= HandleNodeEntered;
        BattleManager.EnemyDefeated -= HandleEnemyDefeated;
        if (_subscribedBattle != null) _subscribedBattle.OnBattleDefeat -= SettleRun;
        _subscribedBattle = null;
    }

    public bool HoldsQuestItem(QuestData quest) => FindHeldItem(quest) != null;

    public int DefeatProgress(QuestData quest)
        => quest != null && _defeatCounts.TryGetValue(quest.questId, out var n) ? n : 0;

    private void HandleNodeEntered(MapNode node)
    {
        if (!_runActive || node == null) return;
        RetryPendingGrants();
        int chapter = GameManager.Instance != null ? GameManager.Instance.Chapter : 1;

        foreach (var quest in new List<QuestData>(Accepted))
        {
            if (quest.itemTrigger == QuestItemTrigger.ReachFloor
                && chapter == quest.triggerChapter && node.floorIndex >= quest.triggerFloor)
                GrantItem(quest);

            if (quest.goal == QuestGoal.DeliverToNode && node.nodeType == quest.deliverNodeType
                && !_completedThisRun.Contains(quest.questId))
            {
                var held = FindHeldItem(quest);
                if (held == null) continue;
                ItemInventoryManager.Instance.RemoveItem(held);
                MarkDone(quest, $"{quest.title} — 배달 완료");
            }
        }
    }

    private void HandleEnemyDefeated(EnemyInstance enemy)
    {
        if (!_runActive || enemy == null || enemy.Data == null) return;
        foreach (var quest in new List<QuestData>(Accepted))
        {
            if (quest.itemTrigger == QuestItemTrigger.DefeatEnemy && SameEnemy(quest.triggerEnemy, enemy.Data))
                GrantItem(quest);

            if (quest.goal == QuestGoal.DefeatCount && SameEnemy(quest.goalEnemy, enemy.Data)
                && !_completedThisRun.Contains(quest.questId))
            {
                _defeatCounts.TryGetValue(quest.questId, out var n);
                _defeatCounts[quest.questId] = ++n;
                if (n >= quest.goalCount) MarkDone(quest, $"{quest.title} {n}/{quest.goalCount} — 귀환 시 보상");
                else OnQuestProgress?.Invoke(quest, $"{quest.title} {n}/{quest.goalCount}");
            }
        }
    }

    // 퀘스트 아이템은 일반 아이템 칸을 쓴다. 칸이 꽉 차면 다음 노드에서 다시 시도한다.
    private void GrantItem(QuestData quest)
    {
        if (quest.questItem == null || _itemGranted.Contains(quest.questId)) return;
        var inv = ItemInventoryManager.Instance;
        if (inv == null) return;
        if (!inv.AddItem(quest.questItem))
        {
            if (!_pendingGrants.Contains(quest)) _pendingGrants.Add(quest);
            OnQuestProgress?.Invoke(quest, $"{quest.questItem.ItemName} — 아이템 칸이 가득 찼습니다");
            return;
        }
        _itemGranted.Add(quest.questId);
        _pendingGrants.Remove(quest);
        OnQuestProgress?.Invoke(quest, $"의뢰 물품 획득: {quest.questItem.ItemName}");
        Debug.Log($"<color=#22D3EE>[Quest] '{quest.title}' 의뢰 물품 획득: {quest.questItem.ItemName}</color>");
    }

    private void RetryPendingGrants()
    {
        foreach (var quest in new List<QuestData>(_pendingGrants)) GrantItem(quest);
    }

    private void MarkDone(QuestData quest, string message)
    {
        _completedThisRun.Add(quest.questId);
        OnQuestProgress?.Invoke(quest, message);
        Debug.Log($"<color=#22D3EE>[Quest] '{quest.title}' 조건 달성</color>");
    }

    private void Complete(QuestData quest)
    {
        if (!_state.completed.Contains(quest.questId)) _state.completed.Add(quest.questId);
        _state.pendingCompleted.Add(quest.questId);
        _state.pendingExp += quest.rewardExp;
        var flags = BlessingAffinityManager.Instance;
        foreach (var f in quest.rewardFlags)
            if (!string.IsNullOrWhiteSpace(f)) flags.SetFlag(f, true);
        if (quest.tag == QuestTag.Rescue && quest.RescueFlag != null) flags.SetFlag(quest.RescueFlag, true);
        Debug.Log($"<color=#22D3EE>[Quest] '{quest.title}' 완료 — 보상 {quest.rewardExp} EXP 적립</color>");
    }

    // 인벤토리의 아이템은 원본의 복제본이라 이름으로 찾는다
    private static ItemData FindHeldItem(QuestData quest)
    {
        var inv = ItemInventoryManager.Instance;
        if (quest?.questItem == null || inv == null) return null;
        foreach (var item in inv.Items)
            if (item != null && item.IsQuestItem && item.ItemName == quest.questItem.ItemName) return item;
        return null;
    }

    private static bool SameEnemy(EnemyData target, EnemyData actual)
        => target != null && actual != null && (target == actual || target.enemyName == actual.enemyName);

    private static bool FlagsMet(List<string> flags)
    {
        if (flags == null) return true;
        foreach (var f in flags)
            if (!string.IsNullOrWhiteSpace(f) && !BlessingAffinityManager.Instance.HasFlag(f)) return false;
        return true;
    }

    // ── 저장 ──────────────────────────────────────────────

    private void Load()
    {
        var json = PlayerPrefs.GetString(PrefsKey, string.Empty);
        _state = string.IsNullOrEmpty(json) ? new SaveState() : (JsonUtility.FromJson<SaveState>(json) ?? new SaveState());
    }

    private void Save()
    {
        PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(_state));
        PlayerPrefs.Save();
    }

    [ContextMenu("Test/Reset Quest Save")]
    private void ResetSave()
    {
        _state = new SaveState();
        Save();
        OnStateChanged?.Invoke();
    }
}
