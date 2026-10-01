using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// 성소 후보 띠 하나.
// Fill·Mask·NameBand·Frame 조각은 모두 같은 816×680 캔버스·같은 꼭짓점이라 겹쳐 놓기만 하면 맞는다.
// 캐릭터 그림은 Strip_Mask(Mask) 아래에서 크롭되고, 잠긴 후보는 같은 그림을 실루엣 머티리얼로 칠한다.
public class SelectCoopCharBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ICanvasRaycastFilter
{
    // 띠 모양 꼭짓점 (816×680 캔버스, 좌상단 원점) — layout.json strip_geometry
    private const float CanvasW = 816f, CanvasH = 680f;
    private const float TopLeftX = 366f, TopRightX = 815f, BottomRightX = 460f, BottomLeftX = 0f;

    [Header("띠 조각")]
    [SerializeField] private Image fill;
    [SerializeField] private Image charImage;
    [SerializeField] private Image nameBand;
    [SerializeField] private Image frame;
    [SerializeField] private Image lockIcon;
    [SerializeField] private Image accentLine;
    [SerializeField] private Image numberUnderline;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI subText;

    [Header("상태 스프라이트")]
    [SerializeField] private Sprite fillNormal;
    [SerializeField] private Sprite fillLocked;
    [SerializeField] private Sprite bandNormal;
    [SerializeField] private Sprite bandLocked;
    [SerializeField] private Sprite frameNormal;
    [SerializeField] private Sprite frameHover;
    [SerializeField] private Sprite frameSelected;
    [SerializeField] private Sprite frameLocked;

    [Header("캐릭터 크롭")]
    [Tooltip("띠 높이 대비 캐릭터 그림 높이 (클수록 확대)")]
    [SerializeField] private float artHeightRatio = 1.55f;
    [Tooltip("그림 윗변이 띠 윗변에서 내려오는 거리(px). 음수면 위로 잘린다")]
    [SerializeField] private float artTopOffset = 8f;
    [SerializeField] private float hoverArtScale = 1.03f;

    [Header("잠긴 후보")]
    [SerializeField] private Material silhouetteMaterial;
    [SerializeField] private Color silhouetteColor = new(0.63f, 0.65f, 0.68f, 1f);
    [SerializeField] private string lockedName = "잠긴 후보";

    [Header("글자 색")]
    [SerializeField] private Color numberColor = new(0.086f, 0.094f, 0.106f, 1f);
    [SerializeField] private Color numberSelectedColor = new(0.05f, 0.72f, 0.95f, 1f);
    [SerializeField] private Color lockedNumberColor = new(0.42f, 0.44f, 0.47f, 1f);
    [SerializeField] private Color affiliationColor = new(0.05f, 0.72f, 0.95f, 1f);
    [SerializeField] private Color lockedSubColor = new(0.66f, 0.68f, 0.71f, 1f);

    private SelectCoopCharUI owner;
    private string charID;
    private bool isLocked;
    private bool isSelected;
    private bool isHovered;
    private Sprite portrait;
    private Vector2 focus = new(0.5f, 0.5f);

    public string CharID => charID;
    public bool IsLocked => isLocked;

    private void Awake()
    {
        owner = GetComponentInParent<SelectCoopCharUI>(true);
    }

    // 선택 가능한 후보
    public void SetCandidate(string id, int index)
    {
        charID = id;
        isLocked = false;
        CoopCharData data = CooperationManager.Instance != null ? CooperationManager.Instance.GetCoopCharData(id) : null;
        portrait = PickPortrait(data);
        focus = data != null ? data.sanctuarySelectionFocus : new Vector2(0.5f, 0.5f);

        numberText.text = $"{index + 1:00}";
        nameText.text = data != null && !string.IsNullOrWhiteSpace(data.charName) ? data.charName : id;
        subText.text = data != null ? data.affiliation : "";
        subText.color = affiliationColor;
        accentLine.enabled = true;
        accentLine.color = data != null ? data.themeColor : affiliationColor;
        lockIcon.enabled = false;
        ResetState();
    }

    // 잠긴 자리. silhouetteSource의 전투 스탠딩(배경 투명 보장)이 있으면 실루엣으로 깐다.
    // 초상화(charImage)는 배경이 칠해진 그림일 수 있어 실루엣이 사각형이 되므로 쓰지 않는다.
    public void SetLocked(CoopCharData silhouetteSource, int index, string unlockHint)
    {
        charID = null;
        isLocked = true;
        portrait = silhouetteSource != null ? silhouetteSource.standingSprite : null;
        focus = silhouetteSource != null ? silhouetteSource.sanctuarySelectionFocus : new Vector2(0.5f, 0.5f);

        numberText.text = $"{index + 1:00}";
        nameText.text = lockedName;
        subText.text = unlockHint;
        subText.color = lockedSubColor;
        accentLine.enabled = false;
        lockIcon.enabled = true;
        ResetState();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected && !isLocked;
        Refresh();
    }

    private static Sprite PickPortrait(CoopCharData data)
    {
        if (data == null) return null;
        if (data.sanctuarySelectionArt != null) return data.sanctuarySelectionArt;
        if (data.charImage != null) return data.charImage;
        return data.standingSprite;
    }

    private void ResetState()
    {
        isSelected = false;
        isHovered = false;
        Refresh();
    }

    private void Refresh()
    {
        fill.sprite = isLocked ? fillLocked : fillNormal;
        nameBand.sprite = isLocked ? bandLocked : bandNormal;
        frame.sprite = isLocked ? frameLocked : isSelected ? frameSelected : isHovered ? frameHover : frameNormal;
        numberText.color = isLocked ? lockedNumberColor : isSelected ? numberSelectedColor : numberColor;
        if (numberUnderline != null) numberUnderline.color = numberText.color;

        charImage.sprite = portrait;
        charImage.enabled = portrait != null;
        charImage.material = isLocked ? silhouetteMaterial : null;
        charImage.color = isLocked ? silhouetteColor : Color.white;
        LayoutPortrait();
    }

    // 그림 높이를 띠 높이 × artHeightRatio로 맞추고, 가로는 focus.x로 띠 안에서 정렬한다.
    private void LayoutPortrait()
    {
        if (portrait == null) return;
        RectTransform rt = charImage.rectTransform;
        RectTransform self = (RectTransform)transform;
        float sx = self.rect.width / CanvasW;
        float sy = self.rect.height / CanvasH;

        float height = CanvasH * artHeightRatio;
        float width = height * portrait.rect.width / portrait.rect.height;
        float scale = !isLocked && (isHovered || isSelected) ? hoverArtScale : 1f;

        // 띠 가운데 높이에서의 왼쪽·오른쪽 변 사이로 정렬
        float midLeft = Mathf.Lerp(TopLeftX, BottomLeftX, 0.5f);
        float midRight = Mathf.Lerp(TopRightX, BottomRightX, 0.5f);
        float centerX = Mathf.Lerp(midLeft, midRight, Mathf.Clamp01(focus.x));

        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(width * sx, height * sy) * scale;
        rt.anchoredPosition = new Vector2(centerX * sx, -artTopOffset * sy);
        rt.localScale = Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isLocked) return;
        isHovered = true;
        Refresh();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHovered) return;
        isHovered = false;
        Refresh();
    }

    // 한 번 누르면 선택, 선택된 띠를 다시 누르면(또는 더블클릭) 상세로.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isLocked || eventData.button != PointerEventData.InputButton.Left) return;
        if (owner == null) owner = GetComponentInParent<SelectCoopCharUI>(true);
        if (owner == null) return;
        if (isSelected || eventData.clickCount >= 2) owner.OpenDetails(charID);
        else owner.SelectCandidate(this);
    }

    private void OnDisable()
    {
        isHovered = false;
    }

    // 사각 영역이 옆 띠와 겹치므로 사선 띠 안쪽만 클릭을 받는다.
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        RectTransform self = (RectTransform)transform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(self, screenPoint, eventCamera, out Vector2 local))
            return false;
        Rect r = self.rect;
        float x = (local.x - r.xMin) / r.width * CanvasW;
        float y = (r.yMax - local.y) / r.height * CanvasH;
        if (y < 0f || y > CanvasH) return false;
        float t = y / (CanvasH - 1f);
        return x >= Mathf.Lerp(TopLeftX, BottomLeftX, t) && x <= Mathf.Lerp(TopRightX, BottomRightX, t);
    }
}
