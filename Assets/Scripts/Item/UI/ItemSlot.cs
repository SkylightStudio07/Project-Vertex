using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 아이템 바의 슬롯 1칸. 아이콘만 표시하고, 호버/클릭은 소유 View로 위임.
// 툴팁은 공용(ItemInventoryView 소유)
public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // 아이템이 없는 칸에 깔아둘 이미지(Assets/Art/UI/NonItem.png).
    // 비워두면 예전처럼 sprite를 지워 투명해진다.
    [SerializeField] private Sprite emptyIcon;

    private ItemData item;
    private ItemInventoryView owner;
    private Image iconImage;

    public bool IsEmpty => item == null;

    private void Awake()
    {
        iconImage = GetComponent<Image>();
    }

    public void SetItem(ItemData itemData, ItemInventoryView ownerView)
    {
        item = itemData;
        owner = ownerView;

        if (iconImage == null) iconImage = GetComponent<Image>();   // 비활성 프리팹 대비
        if (iconImage == null) return;

        iconImage.sprite = item != null ? item.ItemIcon : emptyIcon;
    }

    // 호버 시 공용 툴팁 표시 (이름/설명은 View가 채움).
    // 빈 칸은 보여줄 내용이 없으므로 아무 반응도 하지 않는다 — 예전엔 item이 null이어도
    // 그대로 넘겨서 View가 item.ItemName을 읽다 터졌다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner == null || item == null) return;
        owner.ShowTooltip(item, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (owner != null) owner.HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (owner == null || item == null) return;
        owner.OpenActionPopup(item, transform.position);
    }
}
