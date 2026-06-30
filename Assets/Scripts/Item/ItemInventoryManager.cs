using System;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private int maxCapacityHint = 3;

    public IReadOnlyList<ItemData> Items => _items;
    public event Action OnInventoryChanged;

    public bool AddItem(ItemData item)
    {
        if (_items.Count >= maxCapacityHint)
        {
            Debug.LogWarning("인벤토리 용량 초과. 아이템을 추가할 수 없습니다.");
            return false;
        }
        _items.Add(Instantiate(item));
        OnInventoryChanged?.Invoke();
        Debug.Log($"Added item: {item.ItemName}");
        Debug.Log($"Current capacity: {_items.Count}/{maxCapacityHint}");
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
