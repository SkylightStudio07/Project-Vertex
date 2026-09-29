using DG.Tweening;
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
        RefreshQuestFrame();
    }

    // 의뢰 물품 칸: 청록 테두리 + 왼쪽 아래 유형 배지 오버레이 (QuestRunSkin, 투명 가운데)
    private Image _questFrame;

    private void RefreshQuestFrame()
    {
        bool quest = item != null && item.IsQuestItem;
        var skin = QuestRunSkin.Instance;
        if (!quest || skin == null)
        {
            if (_questFrame != null) _questFrame.gameObject.SetActive(false);
            return;
        }
        if (_questFrame == null)
        {
            var size = ((RectTransform)transform).rect.size;
            _questFrame = QuestRunSkin.Image("QuestFrame", transform, skin.itemSlot, 0f, 0f, size.x, size.y);
            _questFrame.preserveAspect = false;
            var rt = _questFrame.rectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
        _questFrame.sprite = skin.itemSlot;
        _questFrame.gameObject.SetActive(true);
    }

    private void SetQuestFrameHover(bool hover)
    {
        var skin = QuestRunSkin.Instance;
        if (_questFrame != null && _questFrame.gameObject.activeSelf && skin != null)
            _questFrame.sprite = hover ? skin.itemSlotHover : skin.itemSlot;
    }

    // 새 아이템이 들어왔을 때: 아이콘이 디졸브로 나타나며 살짝 튀어오른다 (ItemInventoryView가 호출)
    public void PlayAppear(Texture noise, float duration)
    {
        if (iconImage == null) iconImage = GetComponent<Image>();
        if (iconImage == null) return;

        transform.DOKill(true);
        var baseScale = transform.localScale;
        transform.localScale = baseScale * 0.7f;
        transform.DOScale(baseScale, duration).SetEase(Ease.OutBack).SetLink(gameObject);
        UIDissolve.In(iconImage, noise, duration, UIDissolve.DefaultEdge);
    }

    // 호버 시 공용 툴팁 표시 (이름/설명은 View가 채움).
    // 빈 칸은 보여줄 내용이 없으므로 아무 반응도 하지 않는다 — 예전엔 item이 null이어도
    // 그대로 넘겨서 View가 item.ItemName을 읽다 터졌다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner == null || item == null) return;
        SetQuestFrameHover(true);
        owner.ShowTooltip(item, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetQuestFrameHover(false);
        if (owner != null) owner.HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (owner == null || item == null) return;
        owner.OpenActionPopup(item, transform.position);
    }
}
