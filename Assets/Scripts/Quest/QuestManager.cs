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

    // 런 중 알림 종류 (토스트 틀이 다르다)
    public enum NoticeKind { Acquired, Progress, Completed, Deferred }

    // 런 중 알림 (QuestRunToastView). caption: 작은 윗줄, body: 큰 아랫줄
    public readonly struct Notice
    {
        public readonly QuestData Quest;
        public readonly NoticeKind Kind;
        public readonly string Caption, Body;
        public Notice(QuestData quest, NoticeKind kind, string caption, string body)
        { Quest = quest; Kind = kind; Caption = caption; Body = body; }
    }

    public event Action<Notice> OnNotice;
    public event Action OnStateChanged;

    public int MaxActive { get; set; } = DefaultMaxActive;
    public int PendingExp => _state.pendingExp;
    public IReadOnlyList<string> PendingCompleted => _state.pendingCompleted;
    public int ActiveCount => _state.accepted.Count;

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

    public bool IsUnlocked(QuestData quest) => quest != null && FlagsMet(quest.requiredFlags);

    // 게시판 전체 목록: 완료 안 한 의뢰 전부 (게시 조건 미달은 잠긴 카드로 보인다)
    public List<QuestData> Posted()
    {
        var list = new List<QuestData>();
        if (_db == null) return list;
        foreach (var q in _db.quests) if (q != null && !IsCompleted(q.questId)) list.Add(q);
        return list;
    }

    public List<QuestData> History()
    {
        var list = new List<QuestData>();
        foreach (var id in _state.completed) { var q = Find(id); if (q != null) list.Add(q); }
        return list;
    }

    // 게시판이 완료 도장 연출을 재생한 뒤 비운다
    public List<string> ConsumeJustCompleted()
    {
        var list = new List<string>(_state.pendingCompleted);
        if (list.Count == 0) return list;
        _state.pendingCompleted.Clear();
        Save();
        return list;
    }

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
        if (exp <= 0) return 0;
        var userExp = UserExpManager.Instance;
        if (userExp == null) return 0;
        userExp.AddExperience(exp);
        _state.pendingExp = 0; // 완료 도장 목록(pendingCompleted)은 게시판이 연출 후 비운다
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

    public bool IsRunActive => _runActive;
    public bool IsDoneThisRun(QuestData quest) => quest != null && _completedThisRun.Contains(quest.questId);

    // 맵 노드 표식: 이 노드와 관련된 진행 중 의뢰와 말풍선 문구. 없으면 null.
    //   - 층 도달로 물품을 주는 의뢰: 그 층 노드 (아직 못 받았을 때)
    //   - 배달 의뢰: 배달 목적지 종류 노드 (아직 배달 안 했을 때)
    // 적 처치 조건은 어느 노드에서 그 적이 나올지 미리 알 수 없어 표시하지 않는다.
    public QuestData NodeQuest(MapNode node, out string line)
    {
        line = null;
        if (!_runActive || node == null) return null;
        int chapter = GameManager.Instance != null ? GameManager.Instance.Chapter : 1;
        foreach (var quest in Accepted)
        {
            if (IsDoneThisRun(quest)) continue;
            if (quest.itemTrigger == QuestItemTrigger.ReachFloor && quest.questItem != null
                && chapter == quest.triggerChapter && node.floorIndex == quest.triggerFloor
                && !_itemGranted.Contains(quest.questId))
            {
                line = $"{quest.questItem.ItemName} — 도달 시 획득";
                return quest;
            }
            if (quest.goal == QuestGoal.DeliverToNode && node.nodeType == quest.deliverNodeType)
            {
                line = $"{(quest.questItem != null ? quest.questItem.ItemName : quest.title)} — {QuestData.NodeName(quest.deliverNodeType)}에 전달";
                return quest;
            }
        }
        return null;
    }

    // 인벤토리의 의뢰 물품이 어느 의뢰 것인지 (아이템 칸 배지·툴팁용)
    public QuestData QuestForItem(ItemData item)
    {
        if (item == null || !item.IsQuestItem) return null;
        foreach (var quest in Accepted)
            if (quest.questItem != null && quest.questItem.ItemName == item.ItemName) return quest;
        return null;
    }

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
                MarkDone(quest, quest.questItem.ItemName);
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
                if (n >= quest.goalCount) MarkDone(quest, quest.title);
                else OnNotice?.Invoke(new Notice(quest, NoticeKind.Progress, quest.title, $"{n} / {quest.goalCount}"));
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
            OnNotice?.Invoke(new Notice(quest, NoticeKind.Deferred, "아이템 칸이 가득 찼습니다", "다음 노드에서 다시 획득"));
            return;
        }
        _itemGranted.Add(quest.questId);
        _pendingGrants.Remove(quest);
        OnNotice?.Invoke(new Notice(quest, NoticeKind.Acquired, "의뢰 물품 획득", quest.questItem.ItemName));
        Debug.Log($"<color=#22D3EE>[Quest] '{quest.title}' 의뢰 물품 획득: {quest.questItem.ItemName}</color>");
    }

    private void RetryPendingGrants()
    {
        foreach (var quest in new List<QuestData>(_pendingGrants)) GrantItem(quest);
    }

    private void MarkDone(QuestData quest, string body)
    {
        _completedThisRun.Add(quest.questId);
        OnNotice?.Invoke(new Notice(quest, NoticeKind.Completed, "의뢰 달성 — 귀환 시 보상", body));
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
