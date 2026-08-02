using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 상점의 상품 하나
public class ShopStockEntry : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CardView cardView;      // 카드 상품일 때 사용
    [SerializeField] private Image itemIconImage;    // 아이템 상품일 때 사용
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private GameObject soldOutOverlay;
    [SerializeField] private Color affordableColor = Color.white;
    [SerializeField] private Color unaffordableColor = Color.red;

    private ShopGoods goods;
    public event Action<ShopGoods> OnPurchased;

    public ShopGoods Goods => goods;

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
        priceText.color = affordable ? affordableColor : unaffordableColor;
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
