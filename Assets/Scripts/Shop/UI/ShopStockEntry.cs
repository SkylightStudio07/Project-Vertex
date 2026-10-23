using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 상점의 상품 하나
public class ShopStockEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CardView cardView;      // 카드 상품일 때 사용
    [SerializeField] private Image itemIconImage;    // 아이템 상품일 때 사용
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private GameObject soldOutOverlay;
    [SerializeField] private Color affordableColor = Color.white;
    [SerializeField] private Color unaffordableColor = Color.red;

    [Header("상점 v2 (로비 스타일) — 비워두면 쓰지 않음")]
    [Tooltip("가격표 띠. 호버 때 hoverSprite로 바꾼다")]
    [SerializeField] private Image priceTag;
    [Tooltip("아이템 칸 틀 (아이템 상품만). 호버 때 hoverSprite로 바꾼다")]
    [SerializeField] private Image cellFrame;
    [SerializeField] private Sprite priceTagNormal, priceTagHover, cellNormal, cellHover;

    [Header("상점 v4 — 살 수 없음 상태 · 보급품 이름 (비워두면 쓰지 않음)")]
    [Tooltip("크레딧이 모자랄 때. 마우스를 올려도 이 그림을 유지한다 (품절 > 살 수 없음 > 올림 > 보통)")]
    [SerializeField] private Sprite priceTagUnaffordable, cellUnaffordable;
    [SerializeField] private TextMeshProUGUI nameText;
    [Tooltip("품절이면 흐리게 할 그림 묶음 (카드/아이콘)")]
    [SerializeField] private CanvasGroup dimOnSold;

    private ShopGoods goods;
    public event Action<ShopGoods> OnPurchased;

    public ShopGoods Goods => goods;

    public void ApplyFont(TMP_FontAsset font)
    {
        if (font == null) return;

        foreach (var text in GetComponentsInChildren<TMP_Text>(true))
        {
            text.font = font;
        }
    }

    public void Bind(ShopGoods goods)
    {
        this.goods = goods;
        
        bool isCard = goods.Type == ShopGoodsType.Card;
        if(cardView != null)
        {
            cardView.gameObject.SetActive(isCard);
            if (isCard)
            {
                cardView.SetCard(goods.Data as CardData);
            }
        }
        if(itemIconImage != null)
        {
            itemIconImage.gameObject.SetActive(!isCard);
            if (!isCard)
            {
                itemIconImage.sprite = (goods.Data as ItemData).ItemIcon;
            }
        }
        if (nameText != null) nameText.text = !isCard && goods.Data is ItemData item ? item.ItemName : "";

        priceText.text = goods.Price.ToString();
        Refresh();
    }

    public void Refresh()
    {
        if (goods == null) return;

        if(soldOutOverlay != null)
        {
            soldOutOverlay.SetActive(goods.IsSold);
        }

        bool affordable = GameManager.Instance != null && GameManager.Instance.PlayerGold >= goods.Price;
        priceText.color = goods.IsSold ? unaffordableColor : affordable ? affordableColor : unaffordableColor;
        if (dimOnSold != null) dimOnSold.alpha = goods.IsSold ? 0.45f : 1f;
        ApplyFrames();
    }

    private bool _hovered;

    private void SetHover(bool on)
    {
        _hovered = on;
        ApplyFrames();
    }

    // 품절 > 살 수 없음 > 올림 > 보통. 살 수 없을 때는 올려도 그림이 바뀌지 않는다 (툴팁은 뜬다)
    private void ApplyFrames()
    {
        bool sold = goods != null && goods.IsSold;
        bool poor = goods != null && !sold && (GameManager.Instance == null || GameManager.Instance.PlayerGold < goods.Price);
        bool hover = _hovered && goods != null && !sold && !poor;
        if (priceTag != null && priceTagNormal != null)
            priceTag.sprite = poor && priceTagUnaffordable != null ? priceTagUnaffordable : hover && priceTagHover != null ? priceTagHover : priceTagNormal;
        if (cellFrame != null && cellNormal != null)
            cellFrame.sprite = poor && cellUnaffordable != null ? cellUnaffordable : hover && cellHover != null ? cellHover : cellNormal;
    }

    // 아이템 상품은 호버 시 아이템바와 같은 공용 툴팁(이름·설명)을 상품 위쪽에 띄운다.
    // 카드는 호버 확대(HoverScaleEffect)로 설명이 보이므로 툴팁을 쓰지 않는다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHover(true);
        if (goods == null || goods.Type == ShopGoodsType.Card) return;
        var anchor = itemIconImage != null ? itemIconImage.rectTransform : transform as RectTransform;
        ItemInventoryView.Instance?.ShowTooltipAbove(goods.Data as ItemData, anchor);
    }

    public void OnPointerExit(PointerEventData eventData) { SetHover(false); HideItemTooltip(); }

    // 호버 중에 상점이 닫히거나 목록이 갱신되면 Exit가 오지 않아 툴팁이 남는다
    private void OnDisable() { SetHover(false); HideItemTooltip(); }

    private void HideItemTooltip()
    {
        if (goods != null && goods.Type != ShopGoodsType.Card)
            ItemInventoryView.Instance?.HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 우클릭 — 카드 상품이면 상세 확대.
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (goods != null && goods.Type == ShopGoodsType.Card)
                CardDetailView.Instance?.Show(goods.Data as CardData);
            return;
        }

        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (goods == null || goods.IsSold) return;

        OnPurchased?.Invoke(goods);
    }
}
