using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// 아이템 인벤토리 관리용 싱글톤 클래스
// GameManager에서 아이템 인벤토리를 관리할까 했지만 너무 두꺼워질 것 같아서 별도로 분리
public class ItemInventoryManager : MonoBehaviour
{
    public static ItemInventoryManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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
        OnInventoryChanged?.Invoke();
    }
}
