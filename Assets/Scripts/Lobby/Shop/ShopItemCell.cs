using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 로비 상점 상품 칸 하나. 상태: 보통 / 선택 / 탈린 부족(가격 회색) / 품절(흐림 + 띠) / 해금 완료(띠, 가격 숨김).
// 무색 카드는 상품 그림 대신 카드 앞면에 카드 그림을 넣는다.
public class ShopItemCell : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image frame;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TextMeshProUGUI indexText;
    [SerializeField] private TextMeshProUGUI stockText;
    [SerializeField] private Image art;
    [SerializeField] private GameObject cardFace;
    [SerializeField] private Image cardArt;
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private GameObject priceRoot;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image band;
    [SerializeField] private TextMeshProUGUI bandText;

    [Header("모양")]
    [SerializeField] private Sprite frameNormal;
    [SerializeField] private Sprite frameSelected;
    [SerializeField] private Sprite bandSoldOut;
    [SerializeField] private Sprite bandUnlocked;
    [SerializeField] private Color inkColor = new(0.086f, 0.094f, 0.106f, 1f);
    [SerializeField] private Color paperColor = new(0.933f, 0.941f, 0.949f, 1f);
    [SerializeField] private Color grayColor = new(0.58f, 0.6f, 0.616f, 1f);
    [SerializeField, Range(0f, 1f)] private float soldOutAlpha = 0.45f;

    public ShopItemData Item { get; private set; }
    public event Action<ShopItemCell> OnClicked;

    private void Awake() => button.onClick.AddListener(() => OnClicked?.Invoke(this));

    public void Bind(ShopItemData item, int index)
    {
        Item = item;
        indexText.text = index.ToString("00");
        nameText.text = item.displayName;
        descText.text = item.Subtitle;
        priceText.text = item.price.ToString("N0");

        bool isCard = item.category == ShopCategory.ColorlessCard;
        art.gameObject.SetActive(!isCard);
        cardFace.SetActive(isCard);
        if (isCard)
        {
            var sprite = item.icon != null ? item.icon : item.card != null ? item.card.CardImage : null;
            cardArt.sprite = sprite;
            cardArt.enabled = sprite != null;
            cardName.text = item.card != null ? item.card.BaseCardName : item.displayName;
        }
        else
        {
            art.sprite = item.icon;
            art.enabled = item.icon != null;
        }
    }

    public void Refresh(bool selected)
    {
        if (Item == null) return;
        bool owned = LobbyShop.IsOwned(Item);
        bool soldOut = !owned && LobbyShop.Remaining(Item) <= 0;
        bool affordable = MetaProgress.Talin >= Item.price;

        frame.sprite = selected ? frameSelected : frameNormal;
        group.alpha = soldOut ? soldOutAlpha : 1f;

        stockText.gameObject.SetActive(Item.Stock == ShopStockRule.StoryRestock);
        if (Item.Stock == ShopStockRule.StoryRestock)
            stockText.text = $"재고 {MetaProgress.ShopPartsStock}/{MetaProgress.ShopPartsRestock}";

        priceRoot.SetActive(!owned);
        priceText.color = affordable ? inkColor : grayColor;

        band.gameObject.SetActive(owned || soldOut);
        if (owned)
        {
            band.sprite = bandUnlocked;
            bandText.text = "해금 완료";
            bandText.color = inkColor;
        }
        else if (soldOut)
        {
            band.sprite = bandSoldOut;
            bandText.text = "품절";
            bandText.color = paperColor;
        }
    }
}
