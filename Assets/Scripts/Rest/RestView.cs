using System.Collections.Generic;
using UnityEngine;

// 휴식 노드 UI 컨트롤러
public class RestView : MonoBehaviour
{
    [Header("복귀")]
    [SerializeField] private MapUIController mapUIController;

    [Header("캠프 (합류 캐릭터가 각자 자리에서 할 일을 하는 휴식 화면)")]
    // 자리를 씬에 배치해 두면 합류 순서대로 채운다. 비워두면 아래 초상화 방식으로 동작(이전 방식).
    [SerializeField] private List<RestCampStation> stations = new();
    // 가운데 베이스캠프. 누르면 휴식/강화 메뉴(actionPanel)를 연다. 비워두면 메뉴가 처음부터 보인다.
    [SerializeField] private UnityEngine.UI.Button baseCampButton;
    [SerializeField] private GameObject actionPanel;
    [SerializeField] private NpcDialogueOverlay dialogueOverlay; // 잡담 한 줄
    [SerializeField] private DialogueManager dialogueManager;    // 호감도 랭크업 이벤트
    [Tooltip("휴식 노드마다 캐릭터별 첫 대화에 주는 호감도 포인트 (0이면 주지 않음)")]
    [SerializeField, Min(0)] private int talkCoopPoint = 1;

    private readonly HashSet<string> _talkedThisVisit = new();
    private bool _eventPlaying;

    [Header("합류 캐릭터 (초상화 — stations가 비어 있을 때만 사용)")]
    [SerializeField] private Transform portraitSlotParent;
    [SerializeField] private GameObject portraitSlotPrefab;
    private List<PortraitSlot> portraitSlots = new List<PortraitSlot>();

    [Header("행동")]
    // 휴식 회복량. CardEffect가 순수 클래스로 전환되며 SO 에셋 참조가 불가능해져 int로 단순화 —
    // 구 HealEffect.Execute도 GameManager.HealPlayer 호출이 전부였다. (구 RestHealEffect.asset 값: 20)
    [SerializeField] private int restHealAmount = 20;
    // 강화 가능한 카드가 없을 때 잠글 버튼. (MapUIController의 ScrollRect처럼 using 추가 없이 정규화 표기)
    [SerializeField] private UnityEngine.UI.Button upgradeButton;


    private void Awake()
    {
        if (baseCampButton != null) baseCampButton.onClick.AddListener(ToggleActionPanel);
        foreach (var station in stations)
            if (station != null) station.OnClicked += OnStationClicked;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        _talkedThisVisit.Clear();
        _eventPlaying = false;

        if (stations.Count > 0) BindStations();
        else DisplayCoopPortraits();

        // 베이스캠프가 있으면 메뉴는 캠프를 눌러야 열린다
        if (actionPanel != null) actionPanel.SetActive(baseCampButton == null);
        RefreshUpgradeButton();
        PlayArrivalLine();
    }

    // 합류 캐릭터가 있으면 그중 한 명이 호감도 레벨에 맞는 진입 대사를 한 마디 한다
    private void PlayArrivalLine()
    {
        if (dialogueOverlay == null || CooperationManager.Instance == null) return;

        var candidates = new List<(CoopCharData data, string line)>();
        foreach (var state in CooperationManager.Instance.GetJoinedInRunCharStates())
        {
            string line = state.charData != null ? state.charData.PickRestArrivalLine(state.currentCoopLevel) : null;
            if (!string.IsNullOrWhiteSpace(line)) candidates.Add((state.charData, line));
        }
        if (candidates.Count == 0) return;

        var pick = candidates[Random.Range(0, candidates.Count)];
        var sequence = new BlessingDialogueSequence
        {
            sequenceId = $"rest_arrival_{pick.data.charID}",
            steps = new List<BlessingDialogueStep> { new() { npcDialogue = pick.line, playerAnswerText = "" } },
        };
        dialogueOverlay.Play(sequence, pick.data.charName, null);
    }

    private void ToggleActionPanel()
    {
        if (actionPanel != null) actionPanel.SetActive(!actionPanel.activeSelf);
    }

    // ==== 캠프 스테이션 ====

    private void BindStations()
    {
        var joined = CooperationManager.Instance != null
            ? CooperationManager.Instance.GetJoinedInRunCharStates()
            : new List<CoopCharState>();
        for (int i = 0; i < stations.Count; i++)
            if (stations[i] != null) stations[i].Bind(i < joined.Count ? joined[i] : null);
        if (joined.Count > stations.Count)
            Debug.LogWarning($"[RestView] 합류 캐릭터({joined.Count})가 캠프 자리({stations.Count})보다 많아 일부가 표시되지 않습니다.");
    }

    private void OnStationClicked(RestCampStation station)
    {
        HandleCharacterClicked(station.State);
    }

    private void HandleCharacterClicked(CoopCharState state)
    {
        if (state == null || state.charData == null || _eventPlaying || (dialogueOverlay != null && dialogueOverlay.IsPlaying)) return;

        // 랭크업 이벤트가 대기 중이면 그 대사를 재생하고, 끝나면 레벨을 확정한다
        if (state.isLevelUp && dialogueManager != null)
        {
            _eventPlaying = true;
            void OnEnd()
            {
                dialogueManager.isDialogueEnd -= OnEnd;
                _eventPlaying = false;
                if (CooperationManager.Instance != null && CooperationManager.Instance.IsCoopLevelUP(state.charID))
                    CooperationManager.Instance.SettlePoint(state.charID);
                RefreshStationMarkers();
            }
            dialogueManager.isDialogueEnd += OnEnd;
            dialogueManager.LoadRelationshipEvent(state.charID);
            return;
        }

        // 평소엔 잡담 한 줄. 이번 휴식에서 이 캐릭터와 처음 대화하면 호감도 포인트를 준다.
        var lines = state.charData.campLines;
        string line = lines != null && lines.Count > 0 ? lines[Random.Range(0, lines.Count)] : "……";
        var sequence = new BlessingDialogueSequence
        {
            sequenceId = $"camp_{state.charID}",
            steps = new List<BlessingDialogueStep> { new() { npcDialogue = line, playerAnswerText = "" } },
        };

        void GrantTalkPoint()
        {
            if (talkCoopPoint <= 0 || !_talkedThisVisit.Add(state.charID)) return;
            CooperationManager.Instance?.AddCoopPoint(state.charID, talkCoopPoint);
            RefreshStationMarkers();
        }

        if (dialogueOverlay != null) dialogueOverlay.Play(sequence, state.charData.charName, GrantTalkPoint);
        else GrantTalkPoint();
    }

    private void RefreshStationMarkers()
    {
        foreach (var station in stations)
            if (station != null) station.RefreshMarker();
    }

    // 합류한 캐릭터들의 초상화 슬롯을 생성하여 표시
    private void DisplayCoopPortraits()
    {
        if (portraitSlotPrefab == null || portraitSlotParent == null)
        {
            Debug.LogWarning("[RestView] portraitSlotPrefab 또는 portraitSlotParent가 Inspector에서 지정되지 않았습니다.");
            return;
        }

        if (CooperationManager.Instance == null)
        {
            Debug.LogWarning("[RestView] CooperationManager.Instance가 null입니다.");
            return;
        }

        // 기존 슬롯 제거
        foreach (var slot in portraitSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        portraitSlots.Clear();
        // 현재 합류한 캐릭터들의 초상화 슬롯 생성
        foreach (var coopCharState in CooperationManager.Instance.GetJoinedInRunCharStates())
        {
            var slotObj = Instantiate(portraitSlotPrefab, portraitSlotParent);
            var slot = slotObj.GetComponent<PortraitSlot>();
            if (slot == null)
            {
                Debug.LogWarning("[RestView] portraitSlotPrefab에 PortraitSlot 컴포넌트가 없습니다.");
                Destroy(slotObj);
                continue;
            }
            slot.SetData(coopCharState, HandleCharacterClicked);
            portraitSlots.Add(slot);
        }
    }

    public void OnRestClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.HealPlayer(restHealAmount);
        else
            Debug.LogWarning("[RestView] GameManager.Instance가 null이라 회복을 적용하지 못했습니다.");
        Finish();
    }
    public void OnUpgradeClicked()
    {
        if (DeckManager.Instance == null || CardListView.Instance == null)
        {
            Debug.LogWarning("[RestView] 카드 강화에 필요한 DeckManager 또는 CardListView가 없습니다.");
            return;
        }

        var upgradable = DeckManager.Instance.GetUpgradableCards();
        if (upgradable.Count == 0) return;

        CardListView.Instance.OpenAsSelector(
            "강화할 카드를 선택하세요",
            upgradable,
            onCardSelected: ShowUpgradePreview,
            closeOnSelect: false);
    }

    // 강화 가능한 카드가 하나도 없으면 버튼을 잠근다.
    private void RefreshUpgradeButton()
    {
        if (upgradeButton == null) return;
        upgradeButton.interactable = DeckManager.Instance != null && DeckManager.Instance.GetUpgradableCards().Count > 0;
    }

    // 강화 후 모습을 보여주려고 임시 복사본을 강화 상태로 만들어 확인창에 띄운다.
    // 원본을 건드리지 않으므로 취소해도 덱은 그대로다.
    private void ShowUpgradePreview(CardData card)
    {
        if (card == null || CardDetailView.Instance == null)
        {
            Debug.LogWarning("[RestView] 강화 미리보기에 필요한 카드 또는 CardDetailView가 없습니다.");
            return;
        }

        var preview = Instantiate(card);
        preview.isUpgraded = true;

        CardDetailView.Instance.ShowWithConfirmation(
            preview, "강화",
            onConfirm: () => { Destroy(preview); ApplyUpgrade(card); },
            onCancel:  () => Destroy(preview));
    }

    private void ApplyUpgrade(CardData card)
    {
        if (DeckManager.Instance == null || card == null || !card.CanUpgrade) return;

        if (CardListView.Instance != null) CardListView.Instance.Close();
        // 카드를 크게 띄워 망치질 연출 후 강화 적용 → 끝나면 맵으로
        CardActionFx.Upgrade(card, () => DeckManager.Instance.UpgradeCard(card), Finish);
    }

    private void Finish()
    {
        gameObject.SetActive(false);

        if (mapUIController != null)
        {
            mapUIController.OpenMap();
            return;
        }

        Debug.LogError("[RestView] mapUIController 참조가 없어 맵으로 복귀할 수 없습니다. Inspector 연결을 확인하세요.");
    }
}
