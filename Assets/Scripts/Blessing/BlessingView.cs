using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
// filename   : BlessingView.cs
// description: 축복 노드(Blessing) 전체 화면 컨트롤러.
//              마키나와의 조우 핑퐁 대사(NPC 대사 + 플레이어 답변 버튼),
//              대화 종료 후 4개 선택지(1~3번 은총 + 4번 교감),
//              4번 선택 시 친밀도 +1.0 및 심화 대화 시퀀스를 제어합니다.
// ============================================================
[RequireComponent(typeof(PetalFloatingEffect))]
[RequireComponent(typeof(GraphicRaycaster))]
public class BlessingView : MonoBehaviour
{
    public static BlessingView Instance { get; private set; }

    private enum DialogueFlowMode
    {
        None,
        Encounter,
        AffinityTalk
    }

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

    [Header("플레이어 응답 버튼 (핑퐁 대화용)")]
    [SerializeField] private Button playerAnswerButton;
    [SerializeField] private TextMeshProUGUI playerAnswerText;

    [Header("하단 선택지 컨테이너 (4슬롯 지원)")]
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private List<BlessingChoiceRowUI> choiceRows = new();

    [Header("연결 컨트롤러")]
    [SerializeField] private MapUIController mapUIController;

    [Header("🔍 호감도 실시간 모니터 (Read-Only)")]
    [SerializeField] private float debugCurrentAffinity;
    [SerializeField] private int debugCurrentTier;

    // 대화 시퀀스 제어 런타임 변수
    private DialogueFlowMode currentFlowMode = DialogueFlowMode.None;
    private BlessingDialogueSequence currentSequence;
    private int currentStepIndex = 0;
    private bool hasEncounterAwarded = false;

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

        // Auto-resolve PlayerAnswerButton if unlinked
        if (playerAnswerButton == null)
        {
            Transform btnTrans = transform.Find("ChoiceOverlay/PlayerAnswerButton");
            if (btnTrans != null)
            {
                playerAnswerButton = btnTrans.GetComponent<Button>();
                if (playerAnswerText == null)
                    playerAnswerText = btnTrans.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        // Auto-resolve ChoiceContainer & ChoiceRows if unlinked
        if (choiceContainer == null)
        {
            Transform containerTrans = transform.Find("ChoiceOverlay/ChoicePillList");
            if (containerTrans != null) choiceContainer = containerTrans;
        }

        if (choiceRows == null || choiceRows.Count == 0)
        {
            if (choiceContainer != null)
            {
                choiceRows = choiceContainer.GetComponentsInChildren<BlessingChoiceRowUI>(true).ToList();
            }
        }

        if (playerAnswerButton != null)
        {
            playerAnswerButton.onClick.RemoveAllListeners();
            playerAnswerButton.onClick.AddListener(OnPlayerAnswerClicked);
        }

        // 말풍선 자체 클릭으로도 다음 대사 넘어가기 지원 (편의 기능)
        if (dialogueBubblePanel != null)
        {
            var bubbleBtn = dialogueBubblePanel.GetComponent<Button>();
            if (bubbleBtn == null) bubbleBtn = dialogueBubblePanel.AddComponent<Button>();
            bubbleBtn.onClick.RemoveAllListeners();
            bubbleBtn.onClick.AddListener(OnBubbleClicked);
        }
    }

    private void Start()
    {
        if (defaultBlessingData != null && currentBlessingData == null)
        {
            Setup(defaultBlessingData);
        }
    }

    private void OnEnable()
    {
        if (defaultBlessingData != null && currentBlessingData == null)
        {
            Setup(defaultBlessingData);
        }
        UpdateAffinityDebugDisplay();
    }

    public void UpdateAffinityDebugDisplay()
    {
        if (BlessingAffinityManager.Instance != null)
        {
            string id = currentBlessingData != null && !string.IsNullOrEmpty(currentBlessingData.entityId) ? currentBlessingData.entityId : "machina";
            debugCurrentAffinity = BlessingAffinityManager.Instance.GetAffinity(id);
            debugCurrentTier = BlessingAffinityManager.Instance.GetAffinityTier(id);
        }
    }

    /// <summary>
    /// 축복 화면 열기 (외부 맵 노드 또는 이벤트에서 호출)
    /// </summary>
    public void Open(BlessingData data = null)
    {
        gameObject.SetActive(true);
        hasEncounterAwarded = false;

        BlessingData targetData = data ?? currentBlessingData ?? defaultBlessingData;
        if (targetData != null)
        {
            Setup(targetData);
        }
    }

    /// <summary>
    /// 축복 데이터 주입 및 화면 초기화 & 조우 시퀀스 시작
    /// </summary>
    public void Setup(BlessingData data)
    {
        currentBlessingData = data;
        if (data == null) return;

        // 1. 네임플레이트 기본값
        if (entityNameText != null) entityNameText.text = data.entityName;
        if (entityTitleText != null) entityTitleText.text = data.entityTitle;

        // 2. 아바타 기본값
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

        // 3. 배경 이미지
        if (data.background != null && backgroundImage != null)
        {
            backgroundImage.sprite = data.background;
        }

        // 4. 조우 시 친밀도 0.5 적립 (세션당 1회)
        if (!hasEncounterAwarded)
        {
            BlessingAffinityManager.Instance.AddAffinity(data.entityId, 0.5f);
            hasEncounterAwarded = true;
        }
        UpdateAffinityDebugDisplay();

        // 5. 조건에 맞는 조우 핑퐁 시퀀스 선별
        float currentAffinity = BlessingAffinityManager.Instance.GetAffinity(data.entityId);
        var flags = BlessingAffinityManager.Instance.GetAllFlags();
        Func<string, bool> isCompanionJoined = charId =>
        {
            return CooperationManager.Instance != null && CooperationManager.Instance.IsJoinedInRun(charId);
        };

        BlessingDialogueSequence encounterSeq = data.SelectDialogueSequence(
            data.encounterSequences,
            currentAffinity,
            isCompanionJoined,
            flags);

        if (encounterSeq != null && encounterSeq.steps != null && encounterSeq.steps.Count > 0 && playerAnswerButton != null)
        {
            StartDialogueSequence(encounterSeq, DialogueFlowMode.Encounter);
        }
        else
        {
            // 시퀀스가 없거나 버튼 미설정 시 기본 대사 즉시 출력 후 선택지 바로 노출
            if (dialogueText != null) dialogueText.text = data.dialogueText;
            if (playerAnswerButton != null) playerAnswerButton.gameObject.SetActive(false);
            ShowChoiceRows();
        }
    }

    #region 다이얼로그 핑퐁 로직

    /// <summary>
    /// 대화 시퀀스 시작 (조우 또는 교감 대화)
    /// </summary>
    private void StartDialogueSequence(BlessingDialogueSequence sequence, DialogueFlowMode mode)
    {
        currentSequence = sequence;
        currentFlowMode = mode;
        currentStepIndex = 0;

        // 선택지 바 숨기기
        if (choiceContainer != null) choiceContainer.gameObject.SetActive(false);

        // 플레이어 답변 버튼 활성화
        if (playerAnswerButton != null) playerAnswerButton.gameObject.SetActive(true);

        DisplayCurrentStep();
    }

    /// <summary>
    /// 현재 단계의 대사 및 답변 버튼 갱신
    /// </summary>
    private void DisplayCurrentStep()
    {
        if (currentSequence == null || currentSequence.steps == null || currentStepIndex >= currentSequence.steps.Count)
        {
            CompleteCurrentSequence();
            return;
        }

        var step = currentSequence.steps[currentStepIndex];

        // 화자명 및 대사 갱신
        if (entityNameText != null)
        {
            entityNameText.text = !string.IsNullOrEmpty(step.speakerName)
                ? step.speakerName
                : (currentBlessingData != null ? currentBlessingData.entityName : "마키나");
        }

        if (dialogueText != null)
        {
            dialogueText.text = step.npcDialogue;
        }

        // 아바타 갱신
        if (speakerAvatarImage != null)
        {
            if (step.speakerAvatar != null)
            {
                speakerAvatarImage.gameObject.SetActive(true);
                speakerAvatarImage.sprite = step.speakerAvatar;
            }
            else if (currentBlessingData != null && currentBlessingData.speakerIcon != null)
            {
                speakerAvatarImage.gameObject.SetActive(true);
                speakerAvatarImage.sprite = currentBlessingData.speakerIcon;
            }
        }

        // 플레이어 답변 버튼 텍스트 갱신
        if (playerAnswerText != null)
        {
            playerAnswerText.text = !string.IsNullOrEmpty(step.playerAnswerText)
                ? step.playerAnswerText
                : "[방랑자] ...";
        }
    }

    /// <summary>
    /// 말풍선 클릭 시 (답변 버튼이 활성화된 상태에서만 진행)
    /// </summary>
    private void OnBubbleClicked()
    {
        if (currentFlowMode != DialogueFlowMode.None)
        {
            OnPlayerAnswerClicked();
        }
    }

    /// <summary>
    /// 플레이어 답변 버튼 클릭 시 다음 대사로 진행
    /// </summary>
    private void OnPlayerAnswerClicked()
    {
        if (currentSequence == null) return;

        currentStepIndex++;
        if (currentStepIndex < currentSequence.steps.Count)
        {
            DisplayCurrentStep();
        }
        else
        {
            CompleteCurrentSequence();
        }
    }

    /// <summary>
    /// 대화 시퀀스 완료 처리
    /// </summary>
    private void CompleteCurrentSequence()
    {
        if (currentSequence != null && !string.IsNullOrEmpty(currentSequence.setEventFlagOnComplete))
        {
            BlessingAffinityManager.Instance.SetFlag(currentSequence.setEventFlagOnComplete, true);
        }

        if (currentFlowMode == DialogueFlowMode.Encounter)
        {
            // 조우 대화 종료 -> 네임플레이트 원복 후 선택지 4개 표시
            if (entityNameText != null && currentBlessingData != null)
                entityNameText.text = currentBlessingData.entityName;

            if (speakerAvatarImage != null && currentBlessingData != null && currentBlessingData.speakerIcon != null)
                speakerAvatarImage.sprite = currentBlessingData.speakerIcon;

            if (playerAnswerButton != null) playerAnswerButton.gameObject.SetActive(false);

            currentFlowMode = DialogueFlowMode.None;
            currentSequence = null;

            ShowChoiceRows();
        }
        else if (currentFlowMode == DialogueFlowMode.AffinityTalk)
        {
            // 교감 대화 종료 -> 축복 씬 완료 및 맵 복귀
            if (playerAnswerButton != null) playerAnswerButton.gameObject.SetActive(false);

            currentFlowMode = DialogueFlowMode.None;
            currentSequence = null;

            FinishBlessing();
        }
    }

    #endregion

    #region 선택지 구성 및 실행

    /// <summary>
    /// 4개의 선택지(1~3번 은총 + 4번 교감)를 가로 알약 버튼에 바인딩하고 노출
    /// </summary>
    private void ShowChoiceRows()
    {
        if (choiceContainer != null) choiceContainer.gameObject.SetActive(true);
        if (currentBlessingData == null) return;

        // 전체 4개 선택지 구성
        var displayChoices = new List<BlessingChoice>();

        if (currentBlessingData.choices != null)
        {
            // 최대 3개 일반 은총
            for (int i = 0; i < currentBlessingData.choices.Count && i < 3; i++)
            {
                displayChoices.Add(currentBlessingData.choices[i]);
            }
        }

        // 4번째 슬롯: [교감] 선택지
        displayChoices.Add(currentBlessingData.GetTalkChoice());

        for (int i = 0; i < choiceRows.Count; i++)
        {
            if (i < displayChoices.Count)
            {
                choiceRows[i].gameObject.SetActive(true);
                var choice = displayChoices[i];
                choiceRows[i].Setup(choice, () => ExecuteChoice(choice));
            }
            else
            {
                choiceRows[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 선택지 클릭 시 효과 분기
    /// </summary>
    public void ExecuteChoice(BlessingChoice choice)
    {
        if (choice == null) return;

        switch (choice.effectType)
        {
            case BlessingEffectType.AffinityTalk:
                ExecuteAffinityTalk();
                break;
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

    /// <summary>
    /// 4번 [교감] 선택 시 친밀도 +1.0 적립 및 심화 대화 시퀀스 재생
    /// </summary>
    private void ExecuteAffinityTalk()
    {
        if (currentBlessingData == null)
        {
            FinishBlessing();
            return;
        }

        // 친밀도 +1.0 적립
        BlessingAffinityManager.Instance.AddAffinity(currentBlessingData.entityId, 1.0f);
        UpdateAffinityDebugDisplay();

        float currentAffinity = BlessingAffinityManager.Instance.GetAffinity(currentBlessingData.entityId);
        var flags = BlessingAffinityManager.Instance.GetAllFlags();
        Func<string, bool> isCompanionJoined = charId =>
        {
            return CooperationManager.Instance != null && CooperationManager.Instance.IsJoinedInRun(charId);
        };

        BlessingDialogueSequence talkSeq = currentBlessingData.SelectDialogueSequence(
            currentBlessingData.affinityTalkSequences,
            currentAffinity,
            isCompanionJoined,
            flags);

        if (talkSeq != null && talkSeq.steps != null && talkSeq.steps.Count > 0)
        {
            StartDialogueSequence(talkSeq, DialogueFlowMode.AffinityTalk);
        }
        else
        {
            // 폴백 대사 생성
            var fallback = new BlessingDialogueSequence
            {
                steps = new List<BlessingDialogueStep>
                {
                    new BlessingDialogueStep
                    {
                        speakerName = currentBlessingData.entityName,
                        npcDialogue = "「...오늘은 이 정도만 이야기해 두지. 그대의 여정에 피안의 안식이 함께하기를.」",
                        playerAnswerText = "[방랑자] 고개를 끄덕이며 감사를 표한다."
                    }
                }
            };
            StartDialogueSequence(fallback, DialogueFlowMode.AffinityTalk);
        }
    }

    #endregion

    #region 보상 효과 실행 로직

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
            var shuffled = pool.OrderBy(_ => UnityEngine.Random.value).ToList();
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

    #endregion
}
