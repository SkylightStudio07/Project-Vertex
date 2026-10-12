using TMPro;
using UnityEngine;

// 런 중 의뢰 UI 조각 모음 (토스트·출정 준비 요약·맵 노드 표식·아이템 칸 테두리).
// 맵 노드·아이템 칸은 프리팹을 건드리지 않고 이 스킨에서 스프라이트를 꺼내 코드로 겹친다.
// Resources/QuestRunSkin.asset 하나만 둔다. 원본: ArtDirection/QuestRunUI/Extracted → Assets/Art/UI/QuestRun
[CreateAssetMenu(fileName = "QuestRunSkin", menuName = "Game Asset/Quest Run Skin")]
public class QuestRunSkin : ScriptableObject
{
    public TMP_FontAsset font;

    [Header("유형 기호 (검정)")]
    public Sprite iconRecovery, iconDelivery, iconElimination, iconRescue;

    [Header("토스트 420×76")]
    public Sprite toastAcquired, toastProgress, toastCompleted, toastDeferred;
    public Sprite badgeStory, badgeRescue;
    public Sprite stampCompleted;

    [Header("출정 준비 요약")]
    public Sprite summaryHeader, summaryEmpty, chip, chipHover, dot, tooltip;

    [Header("맵 노드 표식 28×28 / 강조 40×40")]
    public Sprite tagRecovery, tagDelivery, tagElimination, tagRescue;
    public Sprite tagRecoveryHighlight, tagDeliveryHighlight, tagEliminationHighlight, tagRescueHighlight;
    public Sprite mapTooltip;

    [Header("아이템 칸 70×70 오버레이")]
    public Sprite itemSlot, itemSlotHover;

    private static QuestRunSkin _instance;
    private static bool _loaded;
    public static QuestRunSkin Instance
    {
        get
        {
            if (!_loaded) { _instance = Resources.Load<QuestRunSkin>("QuestRunSkin"); _loaded = true; }
            return _instance;
        }
    }

    public Sprite Icon(QuestIconKind kind) => kind switch
    {
        QuestIconKind.Delivery => iconDelivery,
        QuestIconKind.Elimination => iconElimination,
        QuestIconKind.Rescue => iconRescue,
        _ => iconRecovery,
    };

    public Sprite Tag(QuestIconKind kind, bool highlight) => kind switch
    {
        QuestIconKind.Delivery => highlight ? tagDeliveryHighlight : tagDelivery,
        QuestIconKind.Elimination => highlight ? tagEliminationHighlight : tagElimination,
        QuestIconKind.Rescue => highlight ? tagRescueHighlight : tagRescue,
        _ => highlight ? tagRecoveryHighlight : tagRecovery,
    };

    public Sprite Toast(QuestManager.NoticeKind kind) => kind switch
    {
        QuestManager.NoticeKind.Progress => toastProgress,
        QuestManager.NoticeKind.Completed => toastCompleted,
        QuestManager.NoticeKind.Deferred => toastDeferred,
        _ => toastAcquired,
    };

    // ── 코드로 조각을 겹칠 때 쓰는 생성 도우미 (왼쪽 위 기준 좌표) ──

    public static readonly Color Accent = new(0.05f, 0.72f, 0.95f, 1f);
    public static readonly Color Graphite = new(0.1f, 0.11f, 0.12f, 1f);
    public static readonly Color GraphiteSoft = new(0.36f, 0.38f, 0.4f, 1f);

    public static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(w, h);
        return rt;
    }

    public static UnityEngine.UI.Image Image(string name, Transform parent, Sprite sprite, float x, float y, float w, float h, bool raycast = false)
    {
        var rt = Rect(name, parent, x, y, w, h);
        rt.gameObject.AddComponent<CanvasRenderer>();
        var img = rt.gameObject.AddComponent<UnityEngine.UI.Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = raycast;
        return img;
    }

    public TextMeshProUGUI Text(string name, Transform parent, float x, float y, float w, float h, float size, Color color,
                                TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft)
    {
        var rt = Rect(name, parent, x, y, w, h);
        rt.gameObject.AddComponent<CanvasRenderer>();
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Ellipsis;
        return t;
    }
}
