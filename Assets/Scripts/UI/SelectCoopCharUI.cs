using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;


// 협력자 캐릭터 선택을 관리하는 통합 UI
// 후보 선택(사선 띠 3장, 빈 자리는 잠긴 후보) → 후보 확인 → 상세(분리 화면) → 합류
public class SelectCoopCharUI : MonoBehaviour
{
    [SerializeField] private FadeController fadeController;
    [SerializeField] private DialogueView dialogueView;

    [Header("후보 선택")]
    [SerializeField] private RectTransform selectView;
    [Tooltip("띠는 항상 이 개수만큼 보인다. 후보가 모자라면 잠긴 후보로 채운다")]
    [SerializeField] private List<SelectCoopCharBtn> strips = new();
    [SerializeField] private Button detailButton;
    [SerializeField] private TextMeshProUGUI detailButtonLabel;
    [SerializeField] private Image detailButtonArrow;
    [Tooltip("잠긴 후보 띠 아래 해금 조건 문구")]
    [SerializeField] private string lockedHint = "구출 의뢰 · 구조 신호";

    [Header("후보 상세")]
    [SerializeField] private RectTransform detailPanel;
    [SerializeField] private Image detailFullArt;
    [SerializeField] private RectTransform detailArtArea;
    [SerializeField] private TextMeshProUGUI detailNumber;
    [SerializeField] private TextMeshProUGUI detailName;
    [SerializeField] private TextMeshProUGUI detailAffiliation;
    [SerializeField] private Image detailAccent;
    [SerializeField] private TextMeshProUGUI detailDescription;
    [SerializeField] private TextMeshProUGUI affinityLevelText;
    [SerializeField] private List<Image> affinityCells = new();
    [SerializeField] private Sprite affinityEmpty;
    [SerializeField] private Sprite affinityFilled;
    [SerializeField] private Image joinCardArt;
    [SerializeField] private TextMeshProUGUI joinCardName;
    [SerializeField] private TextMeshProUGUI joinCardDescription;
    [SerializeField] private TextMeshProUGUI joinCardCost;
    [SerializeField] private TextMeshProUGUI rewardSummary;
    [SerializeField] private Button backButton;
    [SerializeField] private Button proceedButton;

    [Header("글자 색")]
    [SerializeField] private Color inkColor = new(0.086f, 0.094f, 0.106f, 1f);
    [SerializeField] private Color disabledColor = new(0.62f, 0.64f, 0.67f, 1f);

    private SelectCoopCharBtn selectedStrip;
    private string pendingCharID;
    private bool isConfirming;

    public DialogueView DialogueView => dialogueView;

    private void Awake()
    {
        if (detailButton != null) detailButton.onClick.AddListener(OpenSelectedDetails);
        if (backButton != null) backButton.onClick.AddListener(BackToCandidates);
        if (proceedButton != null) proceedButton.onClick.AddListener(ConfirmSelection);
    }

    // 협력자 선택 이벤트 활성화 시 UI를 초기화하는 메소드
    public void Init()
    {
        List<string> candidates = CollectCandidates();

        // 후보가 없으면 성소를 건너뛰고 바로 맵으로 돌아간다.
        // 이 UI는 캐릭터를 고르는 것 외에 나갈 방법(닫기 버튼)이 없어서, 빈 화면을 띄우면 갇힌다.
        if (candidates.Count == 0)
        {
            Debug.Log("[성소] 현재 층에 선택 가능한 협력자가 없어 성소를 건너뜁니다.");
            CloseUI();
            return;
        }

        fadeController.FadeIn();

        pendingCharID = null;
        isConfirming = false;
        selectedStrip = null;
        if (proceedButton != null) proceedButton.interactable = true;
        if (backButton != null) backButton.interactable = true;
        ShowSelect();

        List<CoopCharData> lockedSources = CollectLockedSilhouettes(candidates);
        for (int i = 0; i < strips.Count; i++)
        {
            if (i < candidates.Count) strips[i].SetCandidate(candidates[i], i);
            else
            {
                int lockedIndex = i - candidates.Count;
                CoopCharData source = lockedIndex < lockedSources.Count ? lockedSources[lockedIndex] : null;
                strips[i].SetLocked(source, i, lockedHint);
            }
        }

        // 후보가 한 명이면 미리 골라 둔다 (바로 후보 확인 가능)
        if (candidates.Count == 1) SelectCandidate(strips[0]);
        else RefreshDetailButton();
    }

    public void SelectCandidate(SelectCoopCharBtn strip)
    {
        if (strip == null || strip.IsLocked) return;
        selectedStrip = strip;
        foreach (SelectCoopCharBtn s in strips) s.SetSelected(s == strip);
        RefreshDetailButton();
    }

    private void RefreshDetailButton()
    {
        bool ready = selectedStrip != null;
        if (detailButton != null) detailButton.interactable = ready;
        if (detailButtonLabel != null) detailButtonLabel.color = ready ? inkColor : disabledColor;
        if (detailButtonArrow != null) detailButtonArrow.color = ready ? inkColor : disabledColor;
    }

    private void OpenSelectedDetails()
    {
        if (selectedStrip != null) OpenDetails(selectedStrip.CharID);
    }

    public void OpenDetails(string charID)
    {
        if (detailPanel == null || CooperationManager.Instance == null || string.IsNullOrEmpty(charID)) return;
        CoopCharData data = CooperationManager.Instance.GetCoopCharData(charID);
        if (data == null) return;

        pendingCharID = charID;
        int index = Mathf.Max(0, strips.FindIndex(s => s.CharID == charID));
        detailNumber.text = $"{index + 1:00}";
        detailName.text = string.IsNullOrWhiteSpace(data.charName) ? charID : data.charName;
        detailAffiliation.text = data.affiliation;
        detailAccent.color = data.themeColor;

        detailDescription.text = string.IsNullOrWhiteSpace(data.charDescription)
            ? "설명이 아직 등록되지 않았습니다."
            : data.charDescription;

        int level = CooperationManager.Instance.GetCoopLevel(charID);
        affinityLevelText.text = $"<size=62%>Lv.</size>{level}";
        for (int i = 0; i < affinityCells.Count; i++)
            affinityCells[i].sprite = i < level ? affinityFilled : affinityEmpty;

        CardData card = data.joinRewardCard;
        joinCardArt.sprite = card != null ? card.CardImage : null;
        joinCardArt.enabled = joinCardArt.sprite != null;
        joinCardName.text = card != null ? card.CardName : "합류 카드 없음";
        joinCardDescription.text = card != null ? card.CardDescription : "";
        joinCardCost.text = card != null ? $"에너지 {card.EnergyCost}  ·  탄약 {card.AmmoCost}" : "";

        rewardSummary.text = BuildRewardSummary(data);
        ApplyFullArt(data);

        if (selectView != null) selectView.gameObject.SetActive(false);
        detailPanel.gameObject.SetActive(true);

        // 소속은 이름 바로 옆에 붙인다 (패널이 켜진 뒤에 재야 폭이 나온다)
        float nameWidth = detailName.GetPreferredValues(detailName.text).x;
        RectTransform affRect = detailAffiliation.rectTransform;
        affRect.anchoredPosition = new Vector2(detailName.rectTransform.anchoredPosition.x + nameWidth + 34f, affRect.anchoredPosition.y);
    }

    private static string BuildRewardSummary(CoopCharData data)
    {
        var parts = new List<string>();
        if (data.rewardPoolCommon.Count > 0) parts.Add($"공용 {data.rewardPoolCommon.Count}");
        if (data.rewardPoolRare.Count > 0) parts.Add($"레어 {data.rewardPoolRare.Count}");
        if (data.rewardPoolUnique.Count > 0) parts.Add($"유니크 {data.rewardPoolUnique.Count}");
        return parts.Count > 0 ? string.Join("  ·  ", parts) : "등록된 보상 카드 없음";
    }

    // 성소 전신 아트 → 스탠딩 → 초상화 순. 영역 높이에 맞추고 너비가 넘치면 너비에 맞춘다.
    private void ApplyFullArt(CoopCharData data)
    {
        Sprite art = data.sanctuaryFullArt != null ? data.sanctuaryFullArt
            : data.standingSprite != null ? data.standingSprite
            : data.charImage;
        detailFullArt.sprite = art;
        detailFullArt.enabled = art != null;
        if (art == null || detailArtArea == null) return;

        Vector2 area = detailArtArea.rect.size;
        float scale = Mathf.Min(area.y / art.rect.height, area.x / art.rect.width);
        RectTransform rt = detailFullArt.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = art.rect.size * scale;
    }

    private void BackToCandidates()
    {
        if (isConfirming) return;
        pendingCharID = null;
        ShowSelect();
    }

    private void ShowSelect()
    {
        if (detailPanel != null) detailPanel.gameObject.SetActive(false);
        if (selectView != null) selectView.gameObject.SetActive(true);
    }

    private void ConfirmSelection()
    {
        if (isConfirming || string.IsNullOrEmpty(pendingCharID) || CooperationManager.Instance == null) return;
        isConfirming = true;
        proceedButton.interactable = false;
        backButton.interactable = false;

        CoopCharData data = CooperationManager.Instance.GetCoopCharData(pendingCharID);
        if (data != null && data.joinDialogueJson != null && dialogueView != null)
            dialogueView.Play(data.joinDialogueJson, FinishSelection);
        else
            FinishSelection();
    }

    private void FinishSelection()
    {
        CooperationManager.Instance.SelectChar(pendingCharID);
        CloseUI();
    }

    // 이번 층의 성소 후보 중 아직 합류하지 않은 캐릭터만 추린다. (띠 개수까지만)
    // 주의: GetSeletableChar()는 HolyPlaceData(SO) 내부 리스트의 참조를 그대로 반환하므로
    //       반환된 리스트를 직접 수정하면 에셋이 영구 변경된다. 반드시 새 리스트에 담는다.
    private List<string> CollectCandidates()
    {
        var candidates = new List<string>();

        if (HolyPlaceManager.Instance == null)
        {
            Debug.LogWarning("[SelectCoopCharUI] HolyPlaceManager.Instance가 없음. 씬(또는 부트 씬)에 HolyPlaceManager가 있는지 확인 필요.");
            return candidates;
        }

        List<string> selectable = HolyPlaceManager.Instance.GetSeletableChar(RunData.Instance.currentFloor);
        if (selectable == null) return candidates;

        foreach (string charID in selectable)
        {
            if (candidates.Count >= strips.Count) break;
            if (CooperationManager.Instance != null && CooperationManager.Instance.IsJoinedInRun(charID)) continue;
            candidates.Add(charID);
        }

        return candidates;
    }

    // 잠긴 자리에 깔 실루엣: 이번 후보도 아니고 이번 런에 합류하지도 않은 협력자
    private static List<CoopCharData> CollectLockedSilhouettes(List<string> candidates)
    {
        var list = new List<CoopCharData>();
        if (CooperationManager.Instance == null) return list;
        foreach (CoopCharData data in CooperationManager.Instance.AllCharData())
        {
            if (candidates.Contains(data.charID) || CooperationManager.Instance.IsJoinedInRun(data.charID)) continue;
            if (data.standingSprite == null) continue;
            list.Add(data);
        }
        return list;
    }

    public void CloseUI()
    {
        // FadeOut()을 부르면서 동시에 SetActive(false)하면 페이드 애니메이션이 재생될 틈도 없이
        // 오브젝트가 꺼져버린다. 페이드 비주얼을 살리려면 타임라인 종료 Signal에 맞춰 닫는 작업이
        // 추후 필요함 (지금은 정확성 우선 — 닫혔는데 맵 클릭이 막히는 버그를 피하는 쪽을 택함).
        gameObject.SetActive(false);
        if (MapUIController.Instance != null) MapUIController.Instance.OpenMap();
    }
}
