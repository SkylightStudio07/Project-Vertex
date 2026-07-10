using System.Collections.Generic;
using UnityEngine;
using TMPro;

// 아이템 인벤토리 바.
// ItemInventoryManager를 구독해 슬롯을 갱신하고, 공용 툴팁(이름/설명)을 소유해 슬롯들이 공유하게 함.
public class ItemInventoryView : MonoBehaviour
{
    [Header("슬롯")]
    [SerializeField] private Transform slotParent;
    [SerializeField] private GameObject itemPrefab;

    [Header("공용 툴팁")]
    [SerializeField] private RectTransform tooltipObj;          // 시작 시 비활성 처리됨
    [SerializeField] private TextMeshProUGUI tooltipNameText;
    [SerializeField] private TextMeshProUGUI tooltipDescText;

    [Header("공용 사용/버리기 팝업")]
    [SerializeField] private ItemActionPopup actionPopup;

    private readonly List<ItemSlot> _slots = new();
    private bool _subscribed;

    private void Awake()
    {
        if (tooltipObj != null) tooltipObj.gameObject.SetActive(false);
    }

    private void OnEnable() => TrySubscribe();
    private void Start()    => TrySubscribe();   // 매니저 Awake가 더 늦게 도는 경우 대비

    private void OnDisable()
    {
        if (_subscribed && ItemInventoryManager.Instance != null)
            ItemInventoryManager.Instance.OnInventoryChanged -= RefreshInventory;
        _subscribed = false;
    }

    private void TrySubscribe()
    {
        if (_subscribed || ItemInventoryManager.Instance == null) return;
        ItemInventoryManager.Instance.OnInventoryChanged += RefreshInventory;
        _subscribed = true;
        RefreshInventory();
    }

    private void RefreshInventory()
    {
        foreach (var s in _slots) if (s != null) Destroy(s.gameObject);
        _slots.Clear();

        foreach (var item in ItemInventoryManager.Instance.Items)
        {
            var slot = Instantiate(itemPrefab, slotParent).GetComponent<ItemSlot>();
            slot.SetItem(item, this);        // 슬롯에 데이터 + 공용 툴팁 소유자(this) 주입
            _slots.Add(slot);
        }
    }

    // 슬롯 호버 시 호출 — 공용 툴팁에 내용 채우고 표시
    public void ShowTooltip(ItemData item, Vector3 worldPos)
    {
        if (tooltipObj == null) return;
        if (tooltipNameText != null) tooltipNameText.text = item.ItemName;
        if (tooltipDescText != null) tooltipDescText.text = item.ItemDescription;
        tooltipObj.position = worldPos;      // 슬롯 위치 기준 (오프셋은 씬에서 조정)
        tooltipObj.gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltipObj != null) tooltipObj.gameObject.SetActive(false);
    }

    // 슬롯 클릭 시 호출 — 공용 사용/버리기 팝업 표시
    public void OpenActionPopup(ItemData item, Vector3 worldPos)
    {
        HideTooltip();                       // 팝업 뜰 때 툴팁은 닫기
        if (actionPopup != null) actionPopup.Open(item, worldPos);
    }
}
