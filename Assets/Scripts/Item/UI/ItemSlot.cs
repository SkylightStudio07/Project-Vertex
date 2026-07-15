using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 아이템 바의 슬롯 1칸. 아이콘만 표시하고, 호버/클릭은 소유 View로 위임.
// 툴팁은 공용(ItemInventoryView 소유)
public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private ItemData item;
    private ItemInventoryView owner;
    private Image iconImage;

    private void Awake()
    {
        iconImage = GetComponent<Image>();
    }

    public void SetItem(ItemData itemData, ItemInventoryView ownerView)
    {
        item = itemData;
        owner = ownerView;
        if(item == null)
        {
            if (iconImage != null) iconImage.sprite = null;
            return;
        }
        if (iconImage == null) iconImage = GetComponent<Image>();   // 비활성 프리팹 대비
        if (iconImage != null) iconImage.sprite = item.ItemIcon;
    }

    // 호버 시 공용 툴팁 표시 (이름/설명은 View가 채움)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner != null) owner.ShowTooltip(item, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (owner != null) owner.HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (owner != null) owner.OpenActionPopup(item, transform.position);
    }
}
