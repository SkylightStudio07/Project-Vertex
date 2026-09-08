using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
// filename   : BlessingView.cs
// description: 축복 노드(Blessing) 전체 화면 컨트롤러.
//              BlessingData SO를 주입받아 좌하단 네임플레이트,
//              하단 대사 말풍선, 가로 알약형 선택지 바를 동적으로 렌더링하고
//              다양한 축복 효과(제거, 강화, 아이템 지급, 골드, 체력)를 실행합니다.
// ============================================================
[RequireComponent(typeof(PetalFloatingEffect))]
[RequireComponent(typeof(GraphicRaycaster))]
public class BlessingView : MonoBehaviour
{
    public static BlessingView Instance { get; private set; }

    [Header("컴포넌트")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;
    [SerializeField] private PetalFloatingEffect petalEffect;

    [Header("축복 데이터")]
    [SerializeField] private BlessingData defaultBlessingData;
    private BlessingData currentBlessingData;

    [Header("좌하단 네임플레이트")]
    [SerializeField] private GameObject nameplatePanel;
    [SerializeField] private TextMeshProUGUI entityNameText;
    [SerializeField] private TextMeshProUGUI entityTitleText;

    [Header("하단 대사 말풍선")]
    [SerializeField] private GameObject dialogueBubblePanel;
    [SerializeField] private Image speakerAvatarImage;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("하단 선택지 컨테이너")]
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private List<BlessingChoiceRowUI> choiceRows = new();

    [Header("연결 컨트롤러")]
    [SerializeField] private MapUIController mapUIController;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (petalEffect == null)
            petalEffect = GetComponent<PetalFloatingEffect>();

        if (backgroundImage == null)
        {
            Transform bg = transform.Find("Background");
            if (bg != null) backgroundImage = bg.GetComponent<Image>();
        }

        if (characterImage == null)
        {
            Transform ch = transform.Find("BlessingCharacter");
            if (ch != null) characterImage = ch.GetComponent<Image>();
        }

        if (mapUIController == null)
            mapUIController = FindObjectOfType<MapUIController>(true);
    }

    private void Start()
    {
        if (defaultBlessingData != null)
        {
            Setup(defaultBlessingData);
        }
    }

    private void OnEnable()
    {
        if (defaultBlessingData != null)
        {
            Setup(defaultBlessingData);
        }
    }

    public void Setup(BlessingData data)
    {
        currentBlessingData = data;
        if (data == null) return;

        // 1. 네임플레이트 갱신
        if (entityNameText != null) entityNameText.text = data.entityName;
        if (entityTitleText != null) entityTitleText.text = data.entityTitle;

        // 2. 대사 말풍선 갱신
        if (dialogueText != null) dialogueText.text = data.dialogueText;
        if (speakerAvatarImage != null)
        {
            if (data.speakerIcon != null)
            {
                speakerAvatarImage.gameObject.SetActive(true);
                speakerAvatarImage.sprite = data.speakerIcon;
            }
            else
            {
                speakerAvatarImage.gameObject.SetActive(false);
            }
        }

        // 3. 배경 갱신 (지정된 경우)
        if (data.background != null && backgroundImage != null)
        {
            backgroundImage.sprite = data.background;
        }

        // 4. 선택지 바 바인딩
        for (int i = 0; i < choiceRows.Count; i++)
        {
            if (i < data.choices.Count)
            {
                choiceRows[i].gameObject.SetActive(true);
                var choice = data.choices[i];
                choiceRows[i].Setup(choice, () => ExecuteChoice(choice));
            }
            else
            {
                choiceRows[i].gameObject.SetActive(false);
            }
        }
    }

    public void Open(BlessingData data = null)
    {
        gameObject.SetActive(true);
        if (data != null)
        {
            Setup(data);
        }
        else if (currentBlessingData == null && defaultBlessingData != null)
        {
            Setup(defaultBlessingData);
        }
        else if (currentBlessingData != null)
        {
            Setup(currentBlessingData);
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
        if (mapUIController != null)
            mapUIController.OpenMap();
    }

    public void FinishBlessing()
    {
        Close();
    }

    // --- 선택지 효과 라우터 ---
    public void ExecuteChoice(BlessingChoice choice)
    {
        if (choice == null) return;

        switch (choice.effectType)
        {
            case BlessingEffectType.RemoveCard:
                ExecuteRemoveCard(choice.valueCount);
                break;
            case BlessingEffectType.UpgradeCards:
                ExecuteUpgradeCards(choice.valueCount);
                break;
            case BlessingEffectType.GainRandomItems:
                ExecuteGainRandomItems(choice.valueCount);
                break;
            case BlessingEffectType.GainGold:
                ExecuteGainGold(choice.valueCount);
                break;
            case BlessingEffectType.HealHP:
                ExecuteHealHP(choice.valueCount);
                break;
            case BlessingEffectType.MaxHP:
                ExecuteMaxHP(choice.valueCount);
                break;
            default:
                Debug.LogWarning($"[Blessing] 미구현 효과 타입: {choice.effectType}");
                FinishBlessing();
                break;
        }
    }

    // 1. 카드 제거
    private void ExecuteRemoveCard(int count)
    {
        if (DeckManager.Instance == null || DeckManager.Instance.PlayerDeck == null || DeckManager.Instance.PlayerDeck.Count == 0)
        {
            Debug.LogWarning("[Blessing] 제거할 수 있는 카드가 덱에 없습니다.");
            FinishBlessing();
            return;
        }

        if (CardListView.Instance == null)
        {
            Debug.LogWarning("[Blessing] CardListView 인스턴스가 없습니다.");
            FinishBlessing();
            return;
        }

        CardListView.Instance.OpenAsSelector(
            "축복: 제거할 카드를 선택하세요",
            DeckManager.Instance.PlayerDeck,
            onCardSelected: card =>
            {
                if (CardDetailView.Instance != null)
                {
                    CardDetailView.Instance.ShowWithConfirmation(
                        card,
                        "제거",
                        onConfirm: () =>
                        {
                            DeckManager.Instance.RemoveCardFromPlayerDeck(card);
                            CardListView.Instance.Close();
                            FinishBlessing();
                        });
                }
                else
                {
                    DeckManager.Instance.RemoveCardFromPlayerDeck(card);
                    CardListView.Instance.Close();
                    FinishBlessing();
                }
            },
            closeOnSelect: false
        );
    }

    // 2. 카드 강화 (count 만큼 순차 진행)
    private void ExecuteUpgradeCards(int targetCount)
    {
        if (DeckManager.Instance == null || DeckManager.Instance.PlayerDeck == null)
        {
            FinishBlessing();
            return;
        }

        var upgradable = DeckManager.Instance.PlayerDeck.Where(c => c != null && !c.isUpgraded).ToList();
        if (upgradable.Count == 0)
        {
            Debug.LogWarning("[Blessing] 강화 가능한 카드가 덱에 없습니다.");
            FinishBlessing();
            return;
        }

        StartUpgradeStep(1, targetCount);
    }

    private void StartUpgradeStep(int currentStep, int totalSteps)
    {
        var upgradable = DeckManager.Instance.PlayerDeck.Where(c => c != null && !c.isUpgraded).ToList();
        if (upgradable.Count == 0 || currentStep > totalSteps)
        {
            CardListView.Instance?.Close();
            FinishBlessing();
            return;
        }

        string title = totalSteps > 1
            ? $"축복: 강화할 카드를 선택하세요 ({currentStep}/{totalSteps})"
            : "축복: 강화할 카드를 선택하세요";

        CardListView.Instance.OpenAsSelector(
            title,
            upgradable,
            onCardSelected: card =>
            {
                if (CardDetailView.Instance != null)
                {
                    CardDetailView.Instance.ShowWithConfirmation(
                        card,
                        "강화",
                        onConfirm: () =>
                        {
                            card.isUpgraded = true;
                            Debug.Log($"[Blessing] 카드 강화 ({currentStep}/{totalSteps}): {card.CardName}");
                            if (currentStep < totalSteps)
                            {
                                StartUpgradeStep(currentStep + 1, totalSteps);
                            }
                            else
                            {
                                CardListView.Instance.Close();
                                FinishBlessing();
                            }
                        });
                }
                else
                {
                    card.isUpgraded = true;
                    if (currentStep < totalSteps) StartUpgradeStep(currentStep + 1, totalSteps);
                    else
                    {
                        CardListView.Instance.Close();
                        FinishBlessing();
                    }
                }
            },
            closeOnSelect: false
        );
    }

    // 3. 임의의 아이템 지급
    private void ExecuteGainRandomItems(int count)
    {
        List<ItemData> pool = null;
        if (GameManager.Instance != null && GameManager.Instance.ItemPool != null && GameManager.Instance.ItemPool.Count > 0)
        {
            pool = new List<ItemData>(GameManager.Instance.ItemPool);
        }

        if (pool == null || pool.Count == 0)
        {
            var loadedItems = Resources.FindObjectsOfTypeAll<ItemData>();
            if (loadedItems != null && loadedItems.Length > 0)
                pool = new List<ItemData>(loadedItems);
        }

        if (pool != null && pool.Count > 0 && ItemInventoryManager.Instance != null)
        {
            var shuffled = pool.OrderBy(_ => Random.value).ToList();
            for (int i = 0; i < count; i++)
            {
                ItemData item = (i < shuffled.Count) ? shuffled[i] : shuffled[0];
                ItemInventoryManager.Instance.AddItem(item);
            }
            Debug.Log($"[Blessing] 임의의 아이템 {count}개 지급 완료.");
        }

        FinishBlessing();
    }

    // 4. 골드 획득
    private void ExecuteGainGold(int amount)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerGold += amount;
            Debug.Log($"[Blessing] 골드 {amount} 획득. 현재 골드: {GameManager.Instance.PlayerGold}");
        }
        FinishBlessing();
    }

    // 5. 체력 회복
    private void ExecuteHealHP(int amount)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HealPlayer(amount);
            Debug.Log($"[Blessing] HP {amount} 회복.");
        }
        FinishBlessing();
    }

    // 6. 최대 체력 증가
    private void ExecuteMaxHP(int amount)
    {
        FinishBlessing();
    }
}
