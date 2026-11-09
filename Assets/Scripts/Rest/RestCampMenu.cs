using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 휴식 노드 행동 메뉴의 겉모습. 씬의 ActionPanel과 버튼(눌렀을 때의 연결)은 그대로 두고 새 조각을 입힌다.
// 조각과 좌표는 ArtDirection/RestCampMockup/Extracted/layout.json 기준이다. 좌표는 판 왼쪽 위 원점.
public sealed class RestCampMenu : MonoBehaviour
{
    private const string UiPath = "RestCamp/UI/";
    private const float SlideSeconds = 0.22f;
    private const float SlideDistance = 46f;
    private static readonly Vector2 PanelTopLeft = new(1208f - 960f, 540f - 150f);

    private readonly struct RowDef
    {
        public readonly string button, art, number, english, title;
        public readonly float y;
        public readonly Rect artRect;

        public RowDef(string button, string art, float y, Rect artRect, string number, string english, string title)
        {
            this.button = button;
            this.art = art;
            this.y = y;
            this.artRect = artRect;
            this.number = number;
            this.english = english;
            this.title = title;
        }
    }

    private static readonly RowDef[] Rows =
    {
        new("RestButton", "Art_Rest", 144f, new Rect(35f, 32f, 200f, 130f), "01", "REST", "휴식"),
        new("UpgradeButton", "Art_Upgrade", 326f, new Rect(48f, 23f, 160f, 138f), "02", "UPGRADE", "강화"),
        new("DismissButton", "Art_Return", 508f, new Rect(40f, 25f, 196f, 145f), "03", "RETURN", "동료 귀환"),
    };

    private RectTransform _rect;
    private CanvasGroup _group;
    private float _openedAt;

    public static void Apply(GameObject panel, int healAmount)
    {
        if (panel == null || panel.GetComponent<RestCampMenu>() != null) return;

        if (panel.TryGetComponent(out LayoutGroup layout)) layout.enabled = false;

        var rect = (RectTransform)panel.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = PanelTopLeft;
        rect.sizeDelta = new Vector2(640f, 755f);
        rect.localScale = Vector3.one;

        Image background = panel.GetComponent<Image>();
        background.sprite = Load("Menu_Panel");
        background.type = Image.Type.Simple;
        background.color = Color.white;

        TMP_FontAsset font = null;
        Transform header = panel.transform.Find("Header");
        if (header != null && header.TryGetComponent(out TextMeshProUGUI title))
        {
            font = title.font;
            Place(title.rectTransform, new Rect(160f, 46f, 390f, 46f));
            Style(title, "REST / MAINTENANCE", 31f, 0.5f, FontStyles.Bold, TextAlignmentOptions.Left, Hex(0xA7A6A2));
        }

        Place(NewImage(rect, "Star", Load("Menu_Star")).rectTransform, new Rect(49f, 40f, 63f, 67f));
        Place(NewImage(rect, "TitleDivider", Load("Menu_TitleDivider")).rectTransform, new Rect(134f, 42f, 2f, 65f));

        foreach (RowDef def in Rows)
        {
            Transform row = panel.transform.Find(def.button);
            if (row == null) continue;
            string description = def.button switch
            {
                "RestButton" => $"HP {healAmount} 회복",
                "UpgradeButton" => "카드 1장 정비",
                _ => "거점으로 돌려보내기",
            };
            row.gameObject.AddComponent<RestMenuRow>().Setup(def.art, def.artRect, new Rect(10f, def.y, 618f, 174f), font, def.number, def.english, def.title, description);
        }

        panel.AddComponent<RestCampMenu>();
    }

    private void Awake()
    {
        _rect = (RectTransform)transform;
        _group = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        _openedAt = Time.unscaledTime;
        Animate(0f);
    }

    private void Update()
    {
        Animate(Mathf.Clamp01((Time.unscaledTime - _openedAt) / SlideSeconds));
    }

    private void Animate(float t)
    {
        float eased = 1f - (1f - t) * (1f - t) * (1f - t);
        _rect.anchoredPosition = PanelTopLeft + new Vector2(SlideDistance * (1f - eased), 0f);
        _group.alpha = eased;
    }

    public static Sprite Load(string name) => Resources.Load<Sprite>(UiPath + name);

    public static Color Hex(uint rgb) => new((rgb >> 16 & 255) / 255f, (rgb >> 8 & 255) / 255f, (rgb & 255) / 255f, 1f);

    // 부모 왼쪽 위 원점의 사각형으로 자리를 잡는다
    public static void Place(RectTransform rect, Rect area)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(area.x, -area.y);
        rect.sizeDelta = area.size;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    public static Image NewImage(RectTransform parent, string name, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        return image;
    }

    public static void Style(TextMeshProUGUI text, string content, float size, float spacingPx, FontStyles style, TextAlignmentOptions alignment, Color color)
    {
        text.text = content;
        text.fontSize = size;
        text.enableAutoSizing = false;
        text.characterSpacing = spacingPx / size * 100f; // TMP 자간은 글자 크기의 1/100 단위
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
    }
}

// 메뉴 한 칸. 기본 · 마우스 올림 · 잠김을 버튼 상태에 맞춰 그린다
public class RestMenuRow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private enum RowState { None, Normal, Hover, Locked }

    private static readonly Color LockedArt = new(0.72f, 0.72f, 0.72f, 0.43f);

    private Button _button;
    private Image _plate, _art, _arrow, _lock;
    private TextMeshProUGUI[] _texts;
    private Sprite _normal, _hover, _locked;
    private RowState _state;
    private bool _hovered;

    public void Setup(string artName, Rect artRect, Rect rowRect, TMP_FontAsset font, string number, string english, string title, string description)
    {
        _button = GetComponent<Button>();
        _button.transition = Selectable.Transition.None;
        _plate = GetComponent<Image>();
        _plate.type = Image.Type.Simple;
        _plate.color = Color.white;
        _plate.canvasRenderer.SetColor(Color.white);
        _normal = RestCampMenu.Load("Row_Normal");
        _hover = RestCampMenu.Load("Row_Hover");
        _locked = RestCampMenu.Load("Row_Locked");

        var rect = (RectTransform)transform;
        RestCampMenu.Place(rect, rowRect);

        foreach (Transform child in transform)
            if (child.TryGetComponent<TextMeshProUGUI>(out _)) child.gameObject.SetActive(false);

        Transform icon = transform.Find("ActionIcon");
        _art = icon != null ? icon.GetComponent<Image>() : RestCampMenu.NewImage(rect, "Art", null);
        _art.sprite = RestCampMenu.Load(artName);
        _art.preserveAspect = false;
        _art.raycastTarget = false;
        RestCampMenu.Place(_art.rectTransform, artRect);

        _arrow = RestCampMenu.NewImage(rect, "Arrow", RestCampMenu.Load("Glyph_Arrow"));
        RestCampMenu.Place(_arrow.rectTransform, new Rect(562f, 76f, 30f, 25f));
        _lock = RestCampMenu.NewImage(rect, "Lock", RestCampMenu.Load("Glyph_Lock"));
        RestCampMenu.Place(_lock.rectTransform, new Rect(558f, 67f, 31f, 39f));

        _texts = new[]
        {
            NewText(rect, font, new Rect(286f, 24f, 42f, 32f), number, 27f, 0f, FontStyles.Bold),
            NewText(rect, font, new Rect(342f, 28f, 180f, 24f), english, 15f, 7f, FontStyles.Bold),
            NewText(rect, font, new Rect(286f, 57f, 255f, 55f), title, 44f, -1f, FontStyles.Bold),
            NewText(rect, font, new Rect(288f, 115f, 258f, 35f), description, 24f, 0f, FontStyles.Normal),
        };
        Refresh();
    }

    private static TextMeshProUGUI NewText(RectTransform parent, TMP_FontAsset font, Rect area, string content, float size, float spacing, FontStyles style)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;
        RestCampMenu.Place(text.rectTransform, area);
        RestCampMenu.Style(text, content, size, spacing, style, TextAlignmentOptions.Left, Color.black);
        return text;
    }

    private void OnDisable()
    {
        _hovered = false;
        Refresh();
    }

    private void Update() => Refresh();

    public void OnPointerEnter(PointerEventData eventData) => _hovered = true;
    public void OnPointerExit(PointerEventData eventData) => _hovered = false;

    private void Refresh()
    {
        if (_button == null) return;

        RowState state = !_button.interactable ? RowState.Locked : _hovered ? RowState.Hover : RowState.Normal;
        if (state == _state) return;
        _state = state;

        bool locked = state == RowState.Locked;
        _plate.sprite = locked ? _locked : state == RowState.Hover ? _hover : _normal;
        _art.color = locked ? LockedArt : Color.white;
        _arrow.gameObject.SetActive(!locked);
        _arrow.color = RestCampMenu.Hex(state == RowState.Hover ? 0x438F83u : 0x1D2020u);
        _lock.gameObject.SetActive(locked);
        _lock.color = RestCampMenu.Hex(0x858581);

        Color textColor = RestCampMenu.Hex(locked ? 0x858581u : state == RowState.Hover ? 0x141A1Bu : 0x171818u);
        foreach (TextMeshProUGUI text in _texts) text.color = textColor;
    }
}
