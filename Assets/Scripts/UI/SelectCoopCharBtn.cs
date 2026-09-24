using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SelectCoopCharBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ICanvasRaycastFilter
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private UnityEngine.UI.Image charImage;
    private SelectCoopCharUI selectCoopCharUI;
    private string charID;
    private Sprite normalPortrait;
    private Sprite hoverPortrait;
    private Vector2 normalFocus = new Vector2(0.5f, 0.5f);
    private Vector2 hoverFocus = new Vector2(0.5f, 0.5f);
    private SanctuarySliceGraphic sliceMask;
    private SanctuarySliceGraphic sliceOutline;
    private UnityEngine.UI.Image panelBackground;
    private UnityEngine.UI.Image selectAction;
    private TextMeshProUGUI candidateLabel;
    private TextMeshProUGUI characterName;
    private TextMeshProUGUI characterSummary;
    private TextMeshProUGUI selectLabel;
    private TextMeshProUGUI coordinateLabel;
    private Vector4 sliceEdges = new Vector4(0f, 1f, 0f, 1f);
    private int candidateIndex;
    private TMP_FontAsset uiFont;
    private string displayName;
    private string displaySummary;
    private bool isHovered;

    // Property
    public string CharID => charID;

    private void Start()
    {
        if (GetComponentInParent<SelectCoopCharUI>() != null)
        {
            selectCoopCharUI = GetComponentInParent<SelectCoopCharUI>();
        }
    }

    public void SetBtn(string charID)
    {
        this.charID = charID;

        if (CooperationManager.Instance == null)
        {
            Debug.LogWarning("[SelectCoopCharBtn] CooperationManager.Instance가 없음. 씬(또는 부트 씬)에 CooperationManager가 있는지 확인 필요.");
            return;
        }

        CoopCharData data = CooperationManager.Instance.GetCoopCharData(charID);
        normalPortrait = data != null
            ? (data.sanctuarySelectionArt != null ? data.sanctuarySelectionArt : data.charImage)
            : null;
        hoverPortrait = data != null ? data.sanctuaryHoverArt : null;
        normalFocus = data != null ? data.sanctuarySelectionFocus : new Vector2(0.5f, 0.5f);
        hoverFocus = data != null ? data.sanctuaryHoverFocus : normalFocus;
        displayName = data != null && !string.IsNullOrWhiteSpace(data.charName) ? data.charName : charID;
        displaySummary = data != null && !string.IsNullOrWhiteSpace(data.charDescription)
            ? data.charDescription
            : "동행자 관측 기록이 아직 등록되지 않았습니다.";
        ApplyCopy();
        ResetHover();
        if (normalPortrait == null)
        {
            Debug.Log($"{charID}에 해당하는 캐릭터 이미지 없음");
            return;
        }
    }

    public void ConfigureDiagonalStrip(Vector4 edges, bool first, bool last, int index, TMP_FontAsset font)
    {
        sliceEdges = edges;
        candidateIndex = index;
        uiFont = font;
        if (rectTransform == null) rectTransform = transform as RectTransform;
        if (sliceMask == null)
        {
            Transform existing = transform.Find("Diagonal Portrait");
            if (existing != null) sliceMask = existing.GetComponent<SanctuarySliceGraphic>();
            if (sliceMask == null)
            {
                GameObject maskObject = new GameObject("Diagonal Portrait", typeof(RectTransform),
                    typeof(SanctuarySliceGraphic), typeof(UnityEngine.UI.Mask));
                maskObject.transform.SetParent(transform, false);
                sliceMask = maskObject.GetComponent<SanctuarySliceGraphic>();
            }
            Stretch(sliceMask.rectTransform);
            sliceMask.color = Color.white;
            sliceMask.GetComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;

            // The trapezoid is the UI panel only. Character art stays outside the mask so that
            // arbitrary full-body splash ratios remain visible instead of being cropped to it.
            if (charImage == null)
            {
                GameObject artObject = new GameObject("Character Art", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                charImage = artObject.GetComponent<UnityEngine.UI.Image>();
            }
            charImage.transform.SetParent(transform, false);
            charImage.raycastTarget = false;
            Transform legacyMask = transform.Find("Portrait Mask");
            if (legacyMask != null) legacyMask.gameObject.SetActive(false);
            Transform legacyFrame = transform.Find("Sanctuary Frame");
            if (legacyFrame != null) legacyFrame.gameObject.SetActive(false);
        }

        if (charImage.transform.parent != transform) charImage.transform.SetParent(transform, false);
        EnsurePanelVisuals();

        if (sliceOutline == null)
        {
            Transform existing = transform.Find("Diagonal Divider");
            if (existing != null) sliceOutline = existing.GetComponent<SanctuarySliceGraphic>();
            if (sliceOutline == null)
            {
                GameObject outline = new GameObject("Diagonal Divider", typeof(RectTransform), typeof(SanctuarySliceGraphic));
                outline.transform.SetParent(transform, false);
                sliceOutline = outline.GetComponent<SanctuarySliceGraphic>();
            }
            Stretch(sliceOutline.rectTransform);
        }

        sliceMask.Configure(edges, false, false, false);
        sliceOutline.Configure(edges, true, !first, !last);
        LayoutPanelVisuals();
        charImage.transform.SetAsLastSibling();
        sliceOutline.transform.SetAsLastSibling();
        if (transform.Find("Info Header") != null) transform.Find("Info Header").SetAsLastSibling();
        if (transform.Find("Info Footer") != null) transform.Find("Info Footer").SetAsLastSibling();
        UnityEngine.UI.Image hitArea = GetComponent<UnityEngine.UI.Image>();
        hitArea.sprite = null;
        hitArea.color = Color.clear;
        hitArea.raycastTarget = true;
        GetComponent<UnityEngine.UI.Button>().transition = UnityEngine.UI.Selectable.Transition.None;
        ApplyCopy();
        ApplyPortrait();
    }

    private void ApplyPortrait()
    {
        if (charImage == null) return;
        bool useHoverArt = isHovered && hoverPortrait != null;
        Sprite art = useHoverArt ? hoverPortrait : normalPortrait;
        Vector2 focus = useHoverArt ? hoverFocus : normalFocus;
        charImage.sprite = art;
        charImage.enabled = art != null;
        charImage.color = Color.white;
        if (sliceOutline != null)
            sliceOutline.color = isHovered
                ? new Color(0.25f, 0.8f, 0.94f, 1f)
                : new Color(0.045f, 0.065f, 0.08f, 1f);
        if (panelBackground != null)
            panelBackground.color = isHovered
                ? new Color(0.84f, 0.97f, 0.98f, 1f)
                : candidateIndex % 2 == 0
                    ? new Color(0.93f, 0.91f, 0.86f, 1f)
                    : new Color(0.86f, 0.88f, 0.86f, 1f);
        if (selectAction != null)
            selectAction.color = isHovered
                ? new Color(0.25f, 0.8f, 0.94f, 0.96f)
                : new Color(0.035f, 0.055f, 0.065f, 0.94f);
        if (selectLabel != null)
            selectLabel.color = isHovered
                ? new Color(0.02f, 0.08f, 0.1f, 1f)
                : new Color(0.94f, 0.93f, 0.88f, 1f);
        if (sliceMask == null || art == null) return;

        // Fit the entire splash into a stable character area. The diagonal panel is a background,
        // not an image crop frame; this supports tall, wide, and differently sized source art.
        Vector2 viewport = sliceMask.rectTransform.rect.size;
        if (viewport.x <= 0f || viewport.y <= 0f) return;
        float panelWidth = Mathf.Min(sliceEdges.y - sliceEdges.x, sliceEdges.w - sliceEdges.z) * viewport.x;
        Vector2 artArea = new Vector2(panelWidth * 0.92f, viewport.y * 0.76f);
        float scale = Mathf.Min(artArea.x / art.rect.width, artArea.y / art.rect.height);
        Vector2 size = art.rect.size * scale * (isHovered ? 1.035f : 1f);
        RectTransform artRect = charImage.rectTransform;
        artRect.anchorMin = artRect.anchorMax = artRect.pivot = new Vector2(0.5f, 0.5f);
        artRect.localScale = Vector3.one;
        artRect.sizeDelta = size;
        float artCenterY = 0.52f;
        float left = Mathf.Lerp(sliceEdges.x, sliceEdges.z, artCenterY);
        float right = Mathf.Lerp(sliceEdges.y, sliceEdges.w, artCenterY);
        float centerX = Mathf.Lerp(left, right, Mathf.Clamp01(focus.x));
        float remainingY = Mathf.Max(0f, artArea.y - size.y);
        artRect.anchoredPosition = new Vector2(
            (centerX - 0.5f) * viewport.x,
            (artCenterY - 0.5f) * viewport.y + remainingY * (Mathf.Clamp01(focus.y) - 0.5f));
        charImage.preserveAspect = false;
    }

    private void EnsurePanelVisuals()
    {
        Transform background = sliceMask.transform.Find("Panel Background");
        if (background == null)
        {
            GameObject go = new GameObject("Panel Background", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(sliceMask.transform, false);
            background = go.transform;
        }
        panelBackground = background.GetComponent<UnityEngine.UI.Image>();
        Stretch(panelBackground.rectTransform);
        panelBackground.sprite = null;
        panelBackground.raycastTarget = false;
        panelBackground.transform.SetAsFirstSibling();

        RectTransform header = GetOrCreateRect("Info Header");
        candidateLabel = GetOrCreateText(header, "Candidate Label");
        characterName = GetOrCreateText(header, "Character Name");
        characterSummary = GetOrCreateText(header, "Character Summary");

        RectTransform footer = GetOrCreateRect("Info Footer");
        coordinateLabel = GetOrCreateText(footer, "Coordinate Label");
        Transform action = footer.Find("Select Action");
        if (action == null)
        {
            GameObject go = new GameObject("Select Action", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(footer, false);
            action = go.transform;
        }
        selectAction = action.GetComponent<UnityEngine.UI.Image>();
        selectAction.raycastTarget = false;
        selectLabel = GetOrCreateText((RectTransform)action, "Select Label");
    }

    private void LayoutPanelVisuals()
    {
        if (sliceMask == null) return;
        Vector2 viewport = sliceMask.rectTransform.rect.size;
        if (viewport.x <= 0f || viewport.y <= 0f) return;
        float panelWidth = Mathf.Min(sliceEdges.y - sliceEdges.x, sliceEdges.w - sliceEdges.z) * viewport.x;
        float safeWidth = Mathf.Max(180f, panelWidth * 0.78f);

        RectTransform header = transform.Find("Info Header") as RectTransform;
        RectTransform footer = transform.Find("Info Footer") as RectTransform;
        PlaceBand(header, 0.84f, safeWidth, Mathf.Min(170f, viewport.y * 0.2f));
        PlaceBand(footer, 0.12f, safeWidth, Mathf.Min(112f, viewport.y * 0.14f));

        float titleSize = Mathf.Clamp(panelWidth * 0.072f, 25f, 46f);
        SetTextRect(candidateLabel, new Vector2(0f, 0.76f), new Vector2(1f, 1f), 11f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        SetTextRect(characterName, new Vector2(0f, 0.38f), new Vector2(1f, 0.78f), titleSize, FontStyles.Bold, TextAlignmentOptions.Left);
        SetTextRect(characterSummary, new Vector2(0f, 0f), new Vector2(0.78f, 0.38f), 13f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        characterSummary.overflowMode = TextOverflowModes.Ellipsis;

        SetTextRect(coordinateLabel, new Vector2(0f, 0.68f), new Vector2(1f, 1f), 11f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        RectTransform actionRect = selectAction.rectTransform;
        actionRect.anchorMin = new Vector2(0f, 0.06f);
        actionRect.anchorMax = new Vector2(0.74f, 0.62f);
        actionRect.offsetMin = actionRect.offsetMax = Vector2.zero;
        SetTextRect(selectLabel, Vector2.zero, Vector2.one, 13f, FontStyles.Bold, TextAlignmentOptions.Center);
    }

    private void PlaceBand(RectTransform band, float y, float width, float height)
    {
        if (band == null) return;
        Vector2 viewport = sliceMask.rectTransform.rect.size;
        float left = Mathf.Lerp(sliceEdges.x, sliceEdges.z, y);
        float right = Mathf.Lerp(sliceEdges.y, sliceEdges.w, y);
        band.anchorMin = band.anchorMax = band.pivot = new Vector2(0.5f, 0.5f);
        band.sizeDelta = new Vector2(width, height);
        band.anchoredPosition = new Vector2(((left + right) * 0.5f - 0.5f) * viewport.x, (y - 0.5f) * viewport.y);
    }

    private void ApplyCopy()
    {
        if (candidateLabel != null) candidateLabel.text = $"VERTEX // COMPANION CANDIDATE {candidateIndex + 1:00}";
        if (characterName != null) characterName.text = string.IsNullOrWhiteSpace(displayName) ? "UNREGISTERED" : displayName;
        if (characterSummary != null) characterSummary.text = displaySummary;
        if (coordinateLabel != null) coordinateLabel.text = $"OBS-{candidateIndex + 1:00}  ◇  LINK READY";
        if (selectLabel != null) selectLabel.text = "DETAIL  /  후보 확인";
    }

    private RectTransform GetOrCreateRect(string objectName)
    {
        Transform existing = transform.Find(objectName);
        if (existing != null) return existing as RectTransform;
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(transform, false);
        return (RectTransform)go.transform;
    }

    private TextMeshProUGUI GetOrCreateText(RectTransform parent, string objectName)
    {
        Transform existing = parent.Find(objectName);
        TextMeshProUGUI text;
        if (existing != null) text = existing.GetComponent<TextMeshProUGUI>();
        else
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            text = go.GetComponent<TextMeshProUGUI>();
        }
        text.font = uiFont != null ? uiFont : TMP_Settings.defaultFontAsset;
        text.color = new Color(0.035f, 0.055f, 0.065f, 1f);
        text.raycastTarget = false;
        text.enableWordWrapping = true;
        return text;
    }

    private static void SetTextRect(TextMeshProUGUI text, Vector2 min, Vector2 max, float size,
        FontStyles style, TextAlignmentOptions alignment)
    {
        if (text == null) return;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        return sliceMask == null || sliceMask.ContainsScreenPoint(screenPoint, eventCamera);
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplyPortrait();
    }

    private void OnDisable()
    {
        ResetHover();
    }

    private void ResetHover()
    {
        isHovered = false;
        ApplyPortrait();
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // 마우스가 캐릭터 창 위에 위치했을 때 선택되었다는 표시가 나타나도록 함
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        ApplyPortrait();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetHover();
    }

    // 첫 클릭은 합류를 확정하지 않고 상세 확인 화면을 연다.
    public void OnClickBtn()
    {
        ResetHover();
        if (selectCoopCharUI == null) selectCoopCharUI = GetComponentInParent<SelectCoopCharUI>();
        if (selectCoopCharUI != null) selectCoopCharUI.OpenDetails(charID);
    }
}
