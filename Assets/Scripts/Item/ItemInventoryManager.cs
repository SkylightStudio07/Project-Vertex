using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// 아이템 인벤토리 관리용 싱글톤 클래스
// GameManager에서 아이템 인벤토리를 관리할까 했지만 너무 두꺼워질 것 같아서 별도로 분리
// 사용한 아이템의 "지속 효과"(여러 전투 동안 남는 효과)도 여기서 관리한다 — 런 시작 초기화(Clear)가
// 이미 여기 있고, 개념상 아이템에 속하기 때문.
public class ItemInventoryManager : MonoBehaviour
{
    public static ItemInventoryManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // BattleManager도 씬 오브젝트라 Awake 순서에 기대지 않고 Start에서 구독한다.
    private void Start() => TrySubscribeBattle();

    private readonly List<ItemData> _items = new();

    // 아이템 바에 표시되는 슬롯 수이자 소지 한도. 슬더스의 승천처럼 런 조건에 따라 바뀔 수 있어
    // 인스펙터에서 조절하고 런타임에도 SetMaxSlots로 바꿀 수 있게 열어둔다.
    // UI(ItemInventoryView)는 이 값을 읽어 슬롯을 그리므로 한도는 여기 하나로만 관리한다.
    [FormerlySerializedAs("maxCapacityHint")]
    [SerializeField, Min(0)] private int maxSlots = 3;

    public IReadOnlyList<ItemData> Items => _items;
    public int MaxSlots => maxSlots;
    public event Action OnInventoryChanged;

    // 런 중 최대 슬롯 수 변경용. 줄어들어도 이미 가진 아이템을 임의로 버리지는 않는다
    // (어느 걸 버릴지는 게임 규칙의 문제라 여기서 정하면 안 됨) — 대신 경고만 남긴다.
    public void SetMaxSlots(int value)
    {
        int clamped = Mathf.Max(0, value);
        if (clamped == maxSlots) return;

        maxSlots = clamped;
        if (_items.Count > maxSlots)
            Debug.LogWarning($"[ItemInventoryManager] 최대 슬롯({maxSlots})보다 소지 아이템({_items.Count})이 많습니다. 초과분은 자동으로 버리지 않습니다.");

        OnInventoryChanged?.Invoke();
    }

    public int Capacity => maxSlots;
    public bool HasSpace => _items.Count < maxSlots;

    // ── 지속 효과 ────────────────────────────────────────────────
    // 사용한 아이템의 효과가 남은 전투 수만큼 매 전투 시작 시 다시 적용된다.
    private class LingeringItemEffect
    {
        public ItemData source;
        public int remaining;   // 남은 전투 수
    }
    private readonly List<LingeringItemEffect> _lingering = new();
    private bool _battleSubscribed;

    public bool AddItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("추가하려는 아이템 데이터가 null입니다.");
            return false;
        }
        if (_items.Count >= maxSlots)
        {
            Debug.LogWarning("인벤토리 용량 초과. 아이템을 추가할 수 없습니다.");
            return false;
        }
        _items.Add(Instantiate(item));
        OnInventoryChanged?.Invoke();
        Debug.Log($"Added item: {item.ItemName}");
        Debug.Log($"Current capacity: {_items.Count}/{maxSlots}");
        return true;
    }
    public void RemoveItem(ItemData item)
    {
        if (_items.Remove(item))
        {
            OnInventoryChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning("아이템을 인벤토리에서 찾을 수 없습니다.");
        }
    }

    // 새 런 시작 시 인벤토리 비우기
    public void Clear()
    {
        _items.Clear();
        _lingering.Clear();
        OnInventoryChanged?.Invoke();
    }

    // 전투 밖(맵·이벤트 화면 등)에서의 아이템 사용. 전투 중 사용은 BattleManager.TryUseItem이 담당한다.
    // 적 지정형은 대상이 없으므로 전투 밖에서는 사용할 수 없다.
    public bool UseOutsideBattle(ItemData item)
    {
        if (item == null) return false;
        if (!item.UsableOutsideBattle) return false;
        if (item.UseMode == ItemData.ItemUseMode.SelectTarget) return false;
        if (BattleManager.Instance != null && BattleManager.Instance.IsInBattle) return false;

        // 전투 정보가 없는 컨텍스트 — 전투 상태를 요구하는 효과(피해·상태이상 등)는 동작하지 않는다.
        var ctx = new CardContext { Item = item };
        EffectRunner.ExecuteImmediate(item.ItemEffects, ctx);

        ConsumeWithLingering(item, ctx);
        return true;
    }

    // 사용된 아이템을 소비하면서 지속 효과를 등록한다. 전투/비전투 공용.
    // usedContext에 전투 상태가 있으면(전투 중 사용) 이번 전투에도 즉시 적용한다 — 사용한 전투를 1회로 세기 때문.
    public void ConsumeWithLingering(ItemData item, CardContext usedContext)
    {
        if (item == null) return;

        if (item.LingeringBattleCount > 0 && item.lingeringEffects != null && item.lingeringEffects.Count > 0)
        {
            TrySubscribeBattle();

            if (usedContext != null && usedContext.State != null)
                EffectRunner.ExecuteImmediate(item.lingeringEffects, usedContext);

            _lingering.Add(new LingeringItemEffect { source = item, remaining = item.LingeringBattleCount });
            Debug.Log($"[Item] '{item.ItemName}' 지속 효과 등록 — 앞으로 {item.LingeringBattleCount}전투");
        }

        RemoveItem(item);
    }

    private void TrySubscribeBattle()
    {
        if (_battleSubscribed || BattleManager.Instance == null) return;
        BattleManager.Instance.OnBattleStarted += ApplyLingeringOnBattleStart;
        BattleManager.Instance.OnBattleVictory += TickLingeringOnVictory;
        _battleSubscribed = true;
    }

    private void OnDestroy()
    {
        if (!_battleSubscribed || BattleManager.Instance == null) return;
        BattleManager.Instance.OnBattleStarted -= ApplyLingeringOnBattleStart;
        BattleManager.Instance.OnBattleVictory -= TickLingeringOnVictory;
    }

    // 전투 시작 시 남아있는 지속 효과를 다시 적용한다.
    // 상태이상은 전투마다 초기화되므로(PlayerCombatant 재생성) 매 전투 다시 걸어야 한다.
    private void ApplyLingeringOnBattleStart()
    {
        if (_lingering.Count == 0) return;

        var battle = BattleManager.Instance;
        if (battle == null || battle.State == null) return;

        var ctx = new CardContext { State = battle.State, Battle = battle };
        foreach (var entry in _lingering)
        {
            if (entry.source == null || entry.remaining <= 0) continue;
            EffectRunner.ExecuteImmediate(entry.source.lingeringEffects, ctx);
        }
    }

    // 전투를 이겼을 때만 횟수를 소모한다.
    private void TickLingeringOnVictory(BattleReward _)
    {
        for (int i = _lingering.Count - 1; i >= 0; i--)
        {
            _lingering[i].remaining--;
            if (_lingering[i].remaining > 0) continue;

            Debug.Log($"[Item] '{_lingering[i].source?.ItemName}' 지속 효과 종료");
            _lingering.RemoveAt(i);
        }
    }
}
