using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


// 상점 UI 전체를 관리하는 뷰
public class ShopView : MonoBehaviour
{
    [Header("상점 입구")]
    [SerializeField] private GameObject entrancePanel;   // NPC/건물 이미지
    [SerializeField] private Button shopkeeperButton;

    [Header("상품 진열")]
    [SerializeField] private GameObject goodsPanel;
    [SerializeField] private ShopStockEntry cardEntryPrefab;
    [SerializeField] private ShopStockEntry itemEntryPrefab;
    [SerializeField] private Transform cardGoodsContainer;
    [SerializeField] private Transform itemGoodsContainer;
    [SerializeField] private Button closeGoodsButton;

    [Header("카드 제거")]
    [SerializeField] private Button cardRemoveButton;
    [SerializeField] private TextMeshProUGUI cardRemovePriceText;

    [Header("공통")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Button proceedButton;       // 진행 → 맵
    [SerializeField] private MapUIController mapUIController;

    private ShopStock stock;
    private readonly List<ShopStockEntry> entries = new();
    private bool removedCardThisVisit = false;

    private void Awake()
    {
        if (shopkeeperButton != null) shopkeeperButton.onClick.AddListener(OpenGoods);
        if (closeGoodsButton != null) closeGoodsButton.onClick.AddListener(CloseGoods);
        if (proceedButton != null) proceedButton.onClick.AddListener(Proceed);
        if (cardRemoveButton != null) cardRemoveButton.onClick.AddListener(OpenCardRemove);

        gameObject.SetActive(false);
    }

    public void Open(ShopStock stock)
    {
        this.stock = stock;
        gameObject.SetActive(true);
        entrancePanel.SetActive(true);
        goodsPanel.SetActive(false);

        SetUp();
    }

    private void SetUp()
    {
        // 카드 제거 버튼 활성화 여부는 상점 방문마다 초기화 —
        // RefreshRemovePrice()가 이 값을 읽으므로 반드시 먼저 리셋한다.
        removedCardThisVisit = false;

        RefreshGold();
        RefreshRemovePrice();
        ClearEntries();

        // 상품 데이터 생성
        if(stock != null)
        {
            foreach (var goods in stock.CardGoods)
            {
                SpawnEntry(cardEntryPrefab, cardGoodsContainer, goods);
            }
            foreach (var goods in stock.ItemGoods)
            {
                SpawnEntry(itemEntryPrefab, itemGoodsContainer, goods);
            }
        }
    }
    private void SpawnEntry(ShopStockEntry entry, Transform container, ShopGoods goods)
    {
        var newEntry = Instantiate(entry, container);
        newEntry.Bind(goods);
        newEntry.OnPurchased += TryPurchase;
        entries.Add(newEntry);
    }

    private void OpenGoods()
    {
        goodsPanel.SetActive(true);        
    }
    private void CloseGoods()
    {
        goodsPanel.SetActive(false);
    }

    private void RefreshAll()
    {
        foreach(var entry in entries)
        {
            if (entry != null) entry.Refresh();
        }
    }
    private void ClearEntries()
    {
        foreach(var entry in entries)
        {
            if (entry != null)
            {    
                entry.OnPurchased -= TryPurchase;
                Destroy(entry.gameObject);
            }
        }
        // 파괴된 엔트리 참조가 쌓이지 않도록 리스트도 비운다
        entries.Clear();
    }

    private void RefreshGold()
    {
        if (goldText != null) goldText.text = GameManager.Instance.PlayerGold.ToString();
    }

    // 구매처리: 골드 검사 → 지급 성공 확인 → 차감
    private void TryPurchase(ShopGoods goods)
    {
        if (goods == null || goods.IsSold) return;

        if (GameManager.Instance.PlayerGold < goods.Price)
        {
            Debug.Log("[Shop] 골드 부족");
            return;
        }

        if (goods.Type == ShopGoodsType.Card)
        {
            DeckManager.Instance?.AddCardToPlayerDeck(goods.Data as CardData);
        }
        else
        {
            if(!ItemInventoryManager.Instance.AddItem(goods.Data as ItemData))
            {
                Debug.Log("[Shop] 인벤토리 공간 부족");
                return;
            }
        }

        GameManager.Instance.PlayerGold -= goods.Price;
        Debug.Log("[Shop] 구매 완료: " + goods.DisplayName + " / 남은 골드: " + GameManager.Instance.PlayerGold);
        goods.IsSold = true;

        // 품절 표시·가격 색·골드 표기 갱신. 골드가 줄었으니 제거 버튼 활성화 조건도 다시 평가한다.
        RefreshGold();
        RefreshAll();
        RefreshRemovePrice();
    }

    // ==== 카드 제거 ====
    private void OpenCardRemove()
    {
        // 카드 제거는 상점 한 곳당 1회. 버튼 비활성화와 별개로 여기서도 막는다.
        if (removedCardThisVisit)
        {
            Debug.Log("[Shop] 이 상점에서는 이미 카드를 제거함");
            return;
        }

        int price = RunData.Instance.GetCardRemovePrice();
        if(GameManager.Instance.PlayerGold < price)
        {
            Debug.Log("[Shop] 카드 제거: 골드 부족");
            return;
        }

        CardListView.Instance.OpenAsSelector(
            "제거할 카드를 선택하세요", 
            DeckManager.Instance.PlayerDeck,
            onCardSelected: card => CardDetailView.Instance.ShowWithConfirmation(
                card,
                $"제거",
                onConfirm: () => RemoveCard(card, price)),
            closeOnSelect: false
            );
    }

    private void RemoveCard(CardData card, int price)
    {
        if (!DeckManager.Instance.RemoveCardFromPlayerDeck(card)) return;

        GameManager.Instance.PlayerGold -= price;
        RunData.Instance.cardRemoveCount++;

        CardListView.Instance.Close();

        removedCardThisVisit = true;

        // 골드 차감·제거 1회 소진을 표시에 반영 (RefreshRemovePrice가 removedCardThisVisit을 읽는다)
        RefreshGold();
        RefreshAll();
        RefreshRemovePrice();
    }

    private void RefreshRemovePrice()
    {
        int price = RunData.Instance.GetCardRemovePrice();
        if (cardRemovePriceText != null)
        {
            cardRemovePriceText.text = price.ToString();
        }
        if(cardRemoveButton != null)
        {
            // 골드가 충분하고, 이 상점에서 아직 제거하지 않았을 때만 누를 수 있다
            cardRemoveButton.interactable = GameManager.Instance.PlayerGold >= price && !removedCardThisVisit;
        }
    }


    private void Proceed()
    {
        gameObject.SetActive(false);
        mapUIController?.OpenMap();
    }
}
