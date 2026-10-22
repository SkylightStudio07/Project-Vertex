using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 로비 상점(메뉴 05) 화면. 왼쪽 보급 담당 + 대사, 오른쪽 탭 4개 · 3열 상품 칸 · 구매 바.
// 무기고·마테리얼처럼 로비 버튼이 SetActive(true)로 열고, ← 로비가 끈다.
// 판정·구매는 LobbyShop. 기획: Docs/기획/로비_상점.md / 아트: ArtDirection/LobbyShopMockup (layout.json, 1920×1080)
public class LobbyShopView : MonoBehaviour
{
    private enum Tab { All, Material, Gift, ColorlessCard }

    [System.Serializable]
    private class TabButton
    {
        public Button button;
        public Image image;
        public TextMeshProUGUI label;
    }

    [SerializeField] private ShopCatalog catalog;
    [SerializeField] private Button closeButton;

    [Header("재화")]
    [SerializeField] private TextMeshProUGUI talinText;
    [SerializeField] private TextMeshProUGUI partsText;
    [SerializeField] private TextMeshProUGUI artifactText;

    [Header("탭 (전체 · 강화 재료 · 선물 · 무색 카드)")]
    [SerializeField] private List<TabButton> tabs = new();
    [SerializeField] private Sprite tabNormal;
    [SerializeField] private Sprite tabSelected;

    [Header("상품")]
    [SerializeField] private ShopItemCell cellTemplate;
    [SerializeField] private Transform grid;
    [SerializeField] private TextMeshProUGUI emptyText;

    [Header("구매 바")]
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI totalText;
    [SerializeField] private Image totalIcon;
    [SerializeField] private Button buyButton;
    [SerializeField] private Image buyImage;
    [SerializeField] private TextMeshProUGUI buyLabel;
    [SerializeField] private Sprite buyNormal;
    [SerializeField] private Sprite buyDisabled;

    [Header("대사")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private string lineEntry = "필요한 물건 있어? 천천히 둘러봐.";
    [SerializeField] private string linePurchased = "준비해 뒀어. 잘 써.";
    [SerializeField] private string lineInsufficient = "탈린이 조금 모자라.";
    [SerializeField] private string lineSoldOut = "다음 보급까지 기다려.";
    [SerializeField] private string lineOwned = "그건 이미 네 거야.";

    [Header("색")]
    [SerializeField] private Color inkColor = new(0.086f, 0.094f, 0.106f, 1f);
    [SerializeField] private Color cyanColor = new(0.051f, 0.722f, 0.949f, 1f);
    [SerializeField] private Color grayColor = new(0.58f, 0.6f, 0.616f, 1f);
    [SerializeField] private Color buyLabelColor = new(0.933f, 0.941f, 0.949f, 1f);

    private readonly List<ShopItemCell> _cells = new();
    private Tab _tab = Tab.All;
    private ShopItemCell _selected;
    private int _quantity = 1;

    private void Awake()
    {
        cellTemplate.gameObject.SetActive(false);
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        for (int i = 0; i < tabs.Count; i++)
        {
            var tab = (Tab)i;
            tabs[i].button.onClick.AddListener(() => SwitchTab(tab));
        }
        minusButton.onClick.AddListener(() => ChangeQuantity(-1));
        plusButton.onClick.AddListener(() => ChangeQuantity(+1));
        buyButton.onClick.AddListener(Buy);
    }

    private void OnEnable()
    {
        MetaProgress.OnChanged += RefreshState;
        dialogueText.text = lineEntry;
        SwitchTab(_tab, force: true);
    }

    private void OnDisable() => MetaProgress.OnChanged -= RefreshState;

    private void SwitchTab(Tab tab, bool force = false)
    {
        if (_tab == tab && !force) return;
        _tab = tab;
        for (int i = 0; i < tabs.Count; i++)
            tabs[i].image.sprite = i == (int)tab ? tabSelected : tabNormal;
        Rebuild();
    }

    private bool InTab(ShopItemData item) => _tab switch
    {
        Tab.Material => item.category == ShopCategory.Material,
        Tab.Gift => item.category == ShopCategory.Gift,
        Tab.ColorlessCard => item.category == ShopCategory.ColorlessCard,
        _ => true,
    };

    private void Rebuild()
    {
        foreach (var c in _cells) if (c != null) Destroy(c.gameObject);
        _cells.Clear();
        _selected = null;

        int index = 1;
        if (catalog != null)
            foreach (var item in catalog.items)
            {
                if (item == null || !InTab(item)) continue;
                var cell = Instantiate(cellTemplate, grid);
                cell.gameObject.SetActive(true);
                cell.Bind(item, index++);
                cell.OnClicked += Select;
                _cells.Add(cell);
            }

        emptyText.gameObject.SetActive(_cells.Count == 0);
        if (emptyText.gameObject.activeSelf)
            emptyText.text = _tab == Tab.ColorlessCard ? "아직 들여온 카드가 없어." : "진열된 물건이 없어.";

        // 처음엔 살 수 있는 첫 칸을 고른다
        Select(_cells.Find(c => LobbyShop.Check(c.Item, 1) == LobbyShop.Result.Ok) ?? (_cells.Count > 0 ? _cells[0] : null));
    }

    private void Select(ShopItemCell cell)
    {
        _selected = cell;
        _quantity = 1;
        RefreshState();
    }

    private void ChangeQuantity(int delta)
    {
        if (_selected == null) return;
        int max = Mathf.Max(1, LobbyShop.MaxBuyable(_selected.Item));
        _quantity = Mathf.Clamp(_quantity + delta, 1, max);
        RefreshState();
    }

    private void Buy()
    {
        if (_selected == null) return;
        var item = _selected.Item;
        var result = LobbyShop.Buy(item, _quantity);
        dialogueText.text = result switch
        {
            LobbyShop.Result.Ok => linePurchased,
            LobbyShop.Result.NotEnoughTalin => lineInsufficient,
            LobbyShop.Result.SoldOut => lineSoldOut,
            LobbyShop.Result.AlreadyOwned => lineOwned,
            _ => dialogueText.text,
        };
        _quantity = 1;
        RefreshState(); // MetaProgress.OnChanged로도 오지만, 실패했을 때도 갱신한다
    }

    private void RefreshState()
    {
        talinText.text = MetaProgress.Talin.ToString("N0");
        partsText.text = MetaProgress.Parts.ToString("N0");
        artifactText.text = MetaProgress.Artifacts.ToString("N0");

        foreach (var c in _cells) c.Refresh(c == _selected);

        var item = _selected != null ? _selected.Item : null;
        int max = LobbyShop.MaxBuyable(item);
        _quantity = Mathf.Clamp(_quantity, 1, Mathf.Max(1, max));
        var check = item != null ? LobbyShop.Check(item, _quantity) : LobbyShop.Result.InvalidItem;
        bool canBuy = check == LobbyShop.Result.Ok;
        int total = item != null ? item.price * _quantity : 0;

        balanceText.text = canBuy
            ? $"보유 {MetaProgress.Talin:N0} → 구매 후 {MetaProgress.Talin - total:N0}"
            : $"보유 {MetaProgress.Talin:N0} → 구매 후 —";
        quantityText.text = _quantity.ToString();
        minusButton.interactable = canBuy && _quantity > 1;
        plusButton.interactable = canBuy && _quantity < max;
        totalText.text = item != null ? total.ToString("N0") : "—";
        totalText.color = canBuy ? cyanColor : grayColor;
        totalIcon.color = canBuy ? Color.white : new Color(1f, 1f, 1f, 0.5f);

        buyButton.interactable = canBuy;
        buyImage.sprite = canBuy ? buyNormal : buyDisabled;
        buyLabel.color = canBuy ? buyLabelColor : grayColor;
        buyLabel.text = check switch
        {
            LobbyShop.Result.Ok => "구매하기 →",
            LobbyShop.Result.NotEnoughTalin => "탈린 부족",
            LobbyShop.Result.SoldOut => "품절",
            LobbyShop.Result.AlreadyOwned => "해금 완료",
            _ => "구매하기 →",
        };
    }
}
