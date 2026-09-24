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

    // 상점 등 아이템바 밖의 UI도 같은 툴팁을 빌려 쓴다(UpperPanel이 캔버스 뒤쪽이라 대부분의 화면 위에 그려짐).
    public static ItemInventoryView Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        if (tooltipObj != null) tooltipObj.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
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

    // 슬롯은 항상 최대 슬롯 수(ItemInventoryManager.MaxSlots)만큼 그린다.
    // 앞에서부터 소지 아이템을 채우고, 남는 칸은 빈 칸 이미지(ItemSlot.emptyIcon)로 남긴다.
    // 슬롯을 매번 파괴/재생성하지 않고 재사용한다 — 아이템을 쓸 때마다 전부 다시 만들면
    // 그 시점에 마우스가 올라가 있던 슬롯의 OnPointerExit가 유실돼 툴팁이 남는 문제가 있다.
    private void RefreshInventory()
    {
        var manager = ItemInventoryManager.Instance;
        if (manager == null || slotParent == null || itemPrefab == null) return;

        int slotCount = manager.MaxSlots;
        SyncSlotCount(slotCount);

        var items = manager.Items;
        for (int i = 0; i < _slots.Count; i++)
            _slots[i].SetItem(i < items.Count ? items[i] : null, this);
    }

    private void SyncSlotCount(int slotCount)
    {
        for (int i = _slots.Count - 1; i >= slotCount; i--)
        {
            if (_slots[i] != null) Destroy(_slots[i].gameObject);
            _slots.RemoveAt(i);
        }

        while (_slots.Count < slotCount)
        {
            var slot = Instantiate(itemPrefab, slotParent).GetComponent<ItemSlot>();
            if (slot == null)
            {
                Debug.LogError("[ItemInventoryView] itemPrefab에 ItemSlot 컴포넌트가 없습니다.");
                return;
            }
            _slots.Add(slot);
        }
    }

    // 보상 획득 연출에서 아이콘이 날아갈 목표 슬롯. 범위 밖이면 null.
    public RectTransform GetSlotRect(int index)
        => index >= 0 && index < _slots.Count && _slots[index] != null ? _slots[index].transform as RectTransform : null;

    // 슬롯 호버 시 호출 — 공용 툴팁에 내용 채우고 표시
    public void ShowTooltip(ItemData item, Vector3 worldPos)
    {
        if (tooltipObj == null) return;
        if (tooltipNameText != null) tooltipNameText.text = item.ItemName;
        if (tooltipDescText != null) tooltipDescText.text = item.ItemDescription;
        tooltipObj.position = worldPos;      // 슬롯 위치 기준 (오프셋은 씬에서 조정)
        tooltipObj.gameObject.SetActive(true);
    }

    // anchor 바로 위에 툴팁을 띄운다(가운데 정렬). 툴팁 배경은 프리팹에서 슬롯 오른쪽 아래로 오프셋돼 있어서,
    // 자식까지 포함한 실제 영역을 구해 그 아래 끝이 anchor 위 끝 + margin에 오도록 맞춘다.
    public void ShowTooltipAbove(ItemData item, RectTransform anchor, float margin = 16f)
    {
        if (tooltipObj == null || item == null || anchor == null) return;
        if (tooltipNameText != null) tooltipNameText.text = item.ItemName;
        if (tooltipDescText != null) tooltipDescText.text = item.ItemDescription;
        tooltipObj.gameObject.SetActive(true);

        var corners = new Vector3[4];
        anchor.GetWorldCorners(corners);
        Vector3 anchorTop = (corners[1] + corners[2]) * 0.5f;

        Bounds b = RectTransformUtility.CalculateRelativeRectTransformBounds(tooltipObj);
        Vector3 localBottomCenter = new(b.center.x, b.min.y, 0f);
        Vector3 up = tooltipObj.TransformVector(new Vector3(0f, margin, 0f));
        tooltipObj.position = anchorTop + up - tooltipObj.TransformVector(localBottomCenter);
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
