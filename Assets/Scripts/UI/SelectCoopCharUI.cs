using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;


// 협력자 캐릭터 선택을 관리하는 통합 UI
public class SelectCoopCharUI : MonoBehaviour
{
    [SerializeField] private RectTransform selectedUI;
    [SerializeField] private List<SelectCoopCharBtn> selectCoopCharBtns;
    [SerializeField] private FadeController fadeController;
    [SerializeField] private DialogueView dialogueView;
    [Header("Sanctuary visuals (character art remains data-driven)")]
    [SerializeField] private Sprite sanctuaryBackground;
    [Tooltip("화면 높이 대비 대각선의 가로 이동량")]
    [SerializeField, Range(0f, 0.7f)] private float diagonalSlope = 0.5f;
    [Header("Sanctuary detail")]
    [SerializeField] private RectTransform detailPanel;
    [SerializeField] private Image detailFullArt;
    [SerializeField] private TextMeshProUGUI detailArtPlaceholder;
    [SerializeField] private TextMeshProUGUI detailName;
    [SerializeField] private TextMeshProUGUI detailDescription;
    [SerializeField] private RectTransform joinCardPanel;
    [SerializeField] private Image joinCardArt;
    [SerializeField] private TextMeshProUGUI joinCardName;
    [SerializeField] private TextMeshProUGUI joinCardDescription;
    [SerializeField] private TextMeshProUGUI joinCardCost;
    [SerializeField] private Button backButton;
    [SerializeField] private Button proceedButton;

    private string pendingCharID;
    private bool isConfirming;
    private int activeCandidateCount;
    private Vector2 lastChoiceSize;

    public DialogueView DialogueView => dialogueView;

    // 협력자 선택 이벤트 활성화 시 UI를 초기화하는 메소드

    private void Awake()
    {
        selectCoopCharBtns = new List<SelectCoopCharBtn>(GetComponentsInChildren<SelectCoopCharBtn>(true));
        if (backButton != null) backButton.onClick.AddListener(BackToCandidates);
        if (proceedButton != null) proceedButton.onClick.AddListener(ConfirmSelection);
    }

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
        if (detailPanel != null) detailPanel.gameObject.SetActive(false);
        if (proceedButton != null) proceedButton.interactable = true;
        if (backButton != null) backButton.interactable = true;
        if (selectCoopCharBtns.Count > 0)
            selectCoopCharBtns[0].transform.parent.gameObject.SetActive(true);

        EnsureButtonCapacity(candidates.Count);
        activeCandidateCount = candidates.Count;
        ApplySanctuaryLayout(candidates.Count);

        // 후보 수와 버튼 수가 다를 수 있으므로 남는 버튼은 끈다.
        for (int i = 0; i < selectCoopCharBtns.Count; i++)
        {
            bool hasCandidate = i < candidates.Count;
            selectCoopCharBtns[i].gameObject.SetActive(hasCandidate);
            if (hasCandidate) selectCoopCharBtns[i].SetBtn(candidates[i]);
        }
    }

    private void LateUpdate()
    {
        if (activeCandidateCount == 0 || selectCoopCharBtns.Count == 0) return;
        RectTransform choices = selectCoopCharBtns[0].transform.parent as RectTransform;
        if (choices != null && choices.gameObject.activeInHierarchy && choices.rect.size != lastChoiceSize)
            ApplySanctuaryLayout(activeCandidateCount);
    }

    public void OpenDetails(string charID)
    {
        if (detailPanel == null || CooperationManager.Instance == null) return;
        CoopCharData data = CooperationManager.Instance.GetCoopCharData(charID);
        if (data == null) return;

        pendingCharID = charID;
        detailName.text = string.IsNullOrWhiteSpace(data.charName) ? charID : data.charName;
        detailDescription.text = string.IsNullOrWhiteSpace(data.charDescription)
            ? "설명이 아직 등록되지 않았습니다."
            : data.charDescription;

        // This is a dedicated full-screen art slot. Deprecated selection portraits are not reused here.
        detailFullArt.sprite = data.sanctuaryFullArt;
        detailFullArt.enabled = data.sanctuaryFullArt != null;
        if (detailArtPlaceholder != null)
            detailArtPlaceholder.gameObject.SetActive(data.sanctuaryFullArt == null);
        if (data.sanctuaryFullArt != null)
        {
            RectTransform artRect = detailFullArt.rectTransform;
            RectTransform screenRect = detailPanel;
            float screenHeight = screenRect.rect.height;
            float ratio = data.sanctuaryFullArt.rect.width / data.sanctuaryFullArt.rect.height;
            artRect.sizeDelta = new Vector2(screenHeight * ratio, screenHeight);
            artRect.anchoredPosition = new Vector2(screenRect.rect.width * 0.23f, 0f);
        }

        CardData card = data.joinRewardCard;
        joinCardPanel.gameObject.SetActive(true);
        joinCardArt.sprite = card != null ? card.CardImage : null;
        joinCardArt.enabled = card != null && card.CardImage != null;
        joinCardName.text = card != null ? card.CardName : "합류 카드 미지정";
        joinCardDescription.text = card != null ? card.CardDescription : "현재 캐릭터 데이터에 합류 카드가 없습니다.";
        joinCardCost.text = card != null ? $"에너지 {card.EnergyCost}  ·  탄약 {card.AmmoCost}" : "";

        selectCoopCharBtns[0].transform.parent.gameObject.SetActive(false);
        detailPanel.gameObject.SetActive(true);
    }

    private void BackToCandidates()
    {
        if (isConfirming) return;
        pendingCharID = null;
        detailPanel.gameObject.SetActive(false);
        selectCoopCharBtns[0].transform.parent.gameObject.SetActive(true);
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

    private void EnsureButtonCapacity(int count)
    {
        if (selectCoopCharBtns.Count == 0) return;

        while (selectCoopCharBtns.Count < count)
        {
            SelectCoopCharBtn button = Instantiate(selectCoopCharBtns[0], selectCoopCharBtns[0].transform.parent);
            button.name = $"Character Choice {selectCoopCharBtns.Count + 1}";
            Button unityButton = button.GetComponent<Button>();
            unityButton.onClick = new Button.ButtonClickedEvent();
            unityButton.onClick.AddListener(button.OnClickBtn);
            selectCoopCharBtns.Add(button);
        }
    }

    private void ApplySanctuaryLayout(int count)
    {
        if (count == 0 || selectCoopCharBtns.Count == 0) return;

        RectTransform panel = transform.Find("Panel") as RectTransform;
        if (panel == null) return;

        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;
        Image panelImage = panel.GetComponent<Image>();
        if (panelImage != null && sanctuaryBackground != null)
        {
            panelImage.sprite = sanctuaryBackground;
            panelImage.color = Color.white;
            panelImage.type = Image.Type.Simple;
        }

        RectTransform choiceLayer = selectCoopCharBtns[0].transform.parent as RectTransform;
        choiceLayer.anchorMin = Vector2.zero;
        choiceLayer.anchorMax = Vector2.one;
        choiceLayer.offsetMin = Vector2.zero;
        choiceLayer.offsetMax = Vector2.zero;
        choiceLayer.localScale = Vector3.one;
        if (choiceLayer.GetComponent<UnityEngine.UI.RectMask2D>() == null)
            choiceLayer.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        Image legacyBackdrop = choiceLayer.GetComponent<Image>();
        if (legacyBackdrop != null) legacyBackdrop.enabled = false;
        if (selectedUI != null) selectedUI.gameObject.SetActive(false);

        choiceLayer.ForceUpdateRectTransforms();
        float width = choiceLayer.rect.width;
        float height = choiceLayer.rect.height;
        if (width <= 0f || height <= 0f) return;
        lastChoiceSize = choiceLayer.rect.size;
        float cellWidth = width / count;
        float skew = count > 1 ? Mathf.Min(height * diagonalSlope, cellWidth * 0.85f) : 0f;
        TMP_FontAsset uiFont = detailName != null && detailName.font != null
            ? detailName.font
            : TMP_Settings.defaultFontAsset;

        for (int i = 0; i < selectCoopCharBtns.Count; i++)
        {
            SelectCoopCharBtn choice = selectCoopCharBtns[i];
            if (i >= count) continue;
            RectTransform rect = choice.transform as RectTransform;
            // Shared boundaries tile the entire screen. Outer edges remain flush with the viewport.
            float bottomLeft = i == 0 ? 0f : i * cellWidth - skew * 0.5f;
            float topLeft = i == 0 ? 0f : i * cellWidth + skew * 0.5f;
            float bottomRight = i == count - 1 ? width : (i + 1) * cellWidth - skew * 0.5f;
            float topRight = i == count - 1 ? width : (i + 1) * cellWidth + skew * 0.5f;
            float boundsWidth = topRight - bottomLeft;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(boundsWidth, 0f);
            rect.anchoredPosition = new Vector2(bottomLeft, 0f);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            choice.ConfigureDiagonalStrip(new Vector4(
                0f, (bottomRight - bottomLeft) / boundsWidth,
                (topLeft - bottomLeft) / boundsWidth, 1f), i == 0, i == count - 1, i, uiFont);
        }
    }

    // 이번 층의 성소 후보 중 아직 합류하지 않은 캐릭터만 추린다.
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
            if (CooperationManager.Instance != null && CooperationManager.Instance.IsJoinedInRun(charID)) continue;
            candidates.Add(charID);
        }

        return candidates;
    }

    // 선택된 캐릭터 창의 위치에 선택 표시 UI를 이동시키는 메소드
    public void Selected(Transform transform)
    {
        selectedUI.position = transform.position;

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
