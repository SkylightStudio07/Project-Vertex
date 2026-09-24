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

    [Header("상점 주인")]
    // 막마다 다른 상점 주인. GameManager.Chapter와 일치하는 것을 쓰고, 없으면 첫 번째.
    [SerializeField] private List<ShopkeeperData> shopkeepers = new();
    [SerializeField] private NpcDialogueOverlay dialogueOverlay;
    [Tooltip("상점 화면의 고정 말풍선 텍스트 — 상점 주인/말풍선 클릭 시 ShopkeeperData.idleLines에서 바뀐다")]
    [SerializeField] private TextMeshProUGUI shopkeeperBubbleText;
    [SerializeField] private Button shopkeeperBubbleButton;

    [Header("공통")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Button proceedButton;       // 진행 → 맵
    [SerializeField] private MapUIController mapUIController;

    private ShopStock stock;
    private readonly List<ShopStockEntry> entries = new();
    private bool removedCardThisVisit = false;

    [Header("카드 진열 그리드")]
    // 상점 카드는 패널 오른쪽 절반에 3열×2줄로 놓여 세로 공간이 병목이다. 칸을 카드 비율(가로:세로 ≈ 307:470,
    // 가격 표기 포함)에 맞춰야 열 사이가 비지 않고 카드가 최대한 커진다. 카드 크기는 cardEntryScale로 조정.
    [SerializeField] private float cardEntryScale = 0.66f;
    [SerializeField] private int cardGridColumns = 3;
    [SerializeField] private Vector2 cardGridSpacing = new(40f, 12f);
    private static readonly Vector2 CardEntryFootprint = new(310f, 472f); // 카드 그림(1024×1480×0.3) + 아래 가격

    private const string CardGridContainerName = "CardGridContainer";

    private void Awake()
    {
        if (shopkeeperButton != null)
        {
            shopkeeperButton.onClick.AddListener(OpenGoods);
            shopkeeperButton.onClick.AddListener(ChangeBubbleLine);
        }
        if (shopkeeperBubbleButton != null) shopkeeperBubbleButton.onClick.AddListener(ChangeBubbleLine);
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
        goodsPanel.SetActive(true);

        SetUp();
        PlayShopkeeperGreeting();
    }

    // 방문마다 호감도를 올린 뒤, 조건에 맞는 대사(첫 만남 > 등급업 이벤트 > 일반 인사)를 하나 재생한다.
    // 대사 오버레이가 화면 전체 입력을 막으므로, 대사가 끝나야 상품을 고를 수 있다.
    private void PlayShopkeeperGreeting()
    {
        var keeper = ResolveShopkeeper();
        if (keeper == null) return;
        // 입장 시엔 목록 첫 줄(인사말)로 시작하고, 클릭할 때부터 무작위로 바뀐다
        if (shopkeeperBubbleText != null && keeper.idleLines.Count > 0) shopkeeperBubbleText.text = keeper.idleLines[0];
        if (dialogueOverlay == null) return;

        var affinity = BlessingAffinityManager.Instance;
        affinity.AddAffinity(keeper.entityId, keeper.visitAffinityGain);

        var sequence = keeper.SelectVisitSequence(
            affinity.GetAffinity(keeper.entityId),
            charId => CooperationManager.Instance != null && CooperationManager.Instance.IsJoinedInRun(charId),
            affinity.GetAllFlags());
        if (sequence != null) dialogueOverlay.Play(sequence, keeper.entityName, null);
    }

    private void ChangeBubbleLine()
    {
        var keeper = ResolveShopkeeper();
        if (keeper == null || shopkeeperBubbleText == null) return;
        shopkeeperBubbleText.text = keeper.PickIdleLine(shopkeeperBubbleText.text);
    }

    private ShopkeeperData ResolveShopkeeper()
    {
        if (shopkeepers == null || shopkeepers.Count == 0) return null;
        int chapter = GameManager.Instance != null ? GameManager.Instance.Chapter : 1;
        return shopkeepers.Find(k => k != null && k.chapter == chapter) ?? shopkeepers.Find(k => k != null);
    }

    private void SetUp()
    {
        Transform cardEntryContainer = ConfigureCardGrid();
        ApplyShopFont();

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
                SpawnEntry(cardEntryPrefab, cardEntryContainer, goods);
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
        newEntry.ApplyFont(ResolveShopFont());
        if (goods.Type == ShopGoodsType.Card)
        {
            newEntry.transform.localScale = new Vector3(cardEntryScale, cardEntryScale, 1f);
        }
        newEntry.Bind(goods);
        newEntry.OnPurchased += TryPurchase;
        entries.Add(newEntry);
    }

    private Transform ConfigureCardGrid()
    {
        if (cardGoodsContainer == null) return null;

        var horizontalLayout = cardGoodsContainer.GetComponent<HorizontalLayoutGroup>();
        if (horizontalLayout != null) horizontalLayout.enabled = false;

        Transform gridContainer = cardGoodsContainer.Find(CardGridContainerName);
        if (gridContainer == null)
        {
            var gridObject = new GameObject(CardGridContainerName, typeof(RectTransform));
            gridContainer = gridObject.transform;
            gridContainer.SetParent(cardGoodsContainer, false);
        }

        if (gridContainer is RectTransform gridRect)
        {
            gridRect.anchorMin = Vector2.zero;
            gridRect.anchorMax = Vector2.one;
            gridRect.pivot = new Vector2(0.5f, 0.5f);
            gridRect.offsetMin = Vector2.zero;
            gridRect.offsetMax = Vector2.zero;
            gridRect.localScale = Vector3.one;
        }

        gridContainer.SetAsLastSibling();

        var gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();
        }

        gridLayout.enabled = true;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        int columns = Mathf.Max(1, cardGridColumns);
        int rows = Mathf.Max(1, Mathf.CeilToInt((stock?.CardGoods.Count ?? columns) / (float)columns));
        Vector2 cell = CardEntryFootprint * cardEntryScale;
        gridLayout.constraintCount = columns;
        gridLayout.cellSize = cell;
        gridLayout.spacing = cardGridSpacing;

        // 컨테이너 크기를 그리드 전체 크기에 맞춘다. 위치·스케일은 씬에서 잡는다(스케일 1 기준으로 배치됨).
        if (cardGoodsContainer is RectTransform cardRect)
        {
            cardRect.sizeDelta = new Vector2(
                columns * cell.x + (columns - 1) * cardGridSpacing.x,
                rows * cell.y + (rows - 1) * cardGridSpacing.y);
        }

        return gridContainer;
    }

    private TMP_FontAsset ResolveShopFont()
    {
        if (cardEntryPrefab == null) return null;

        var fontSource = cardEntryPrefab.GetComponentInChildren<TMP_Text>(true);
        return fontSource != null ? fontSource.font : null;
    }

    private void ApplyShopFont()
    {
        var shopFont = ResolveShopFont();
        if (shopFont == null) return;

        foreach (var text in GetComponentsInChildren<TMP_Text>(true))
        {
            text.font = shopFont;
        }
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
        if (dialogueOverlay != null && dialogueOverlay.IsPlaying) return;
        gameObject.SetActive(false);
        mapUIController?.OpenMap();
    }
}
