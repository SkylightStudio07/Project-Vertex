using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 씬에 깔린 코드 도형 버튼(VertexFrame = BlessingPanelGraphic)을 v3 래스터 버튼으로 바꿔 입힌다.
// 조각: Resources/UIButtons/ (원본 ArtDirection/RasterRedo_v3/Extracted/Buttons, 좌표 · 글자 자리는 그 layout.json)
//   Close   — 흰 종이 CLOSE (덱 목록)
//   Primary — 먹색 확정 버튼: 흰 번호 칸 01 + 영문 소제목 + 한글 동작 + 조준 문양 (카드 상세 제거 · 강화 등)
//   Cancel  — 흰 종이 취소 버튼: 먹색 번호 칸 02 + RETURN + Cancel + 오른쪽 큰 흐린 02
// 마우스를 올리면 문구 밑 청록 막대가 뜬다. 판 크기는 layout.json 그대로 두고 scale로 줄인다.
public static class UIButtonSkinV3
{
    public enum Kind { Close, Primary, Cancel }

    private static readonly Color Ink = new(0.090f, 0.114f, 0.125f, 1f);   // #171D20
    private static readonly Color Paper = new(0.945f, 0.949f, 0.945f, 1f); // #F1F2F1
    private static readonly Color NumLight = new(0.898f, 0.898f, 0.882f, 1f); // #E5E5E1

    private static Sprite S(string name) => Resources.Load<Sprite>("UIButtons/" + name);

    // label: 버튼에 이미 있는 글자(호출자가 나중에 문구를 바꾸는 그 TMP)를 재사용한다. 반환: 영문 소제목(없으면 null)
    public static TextMeshProUGUI Apply(Button button, TextMeshProUGUI label, Kind kind, float scale = 1f)
    {
        if (button == null) return null;
        var rt = (RectTransform)button.transform;
        var frame = rt.Find("VertexFrame");
        if (frame != null) frame.gameObject.SetActive(false);

        string key = kind switch { Kind.Close => "Close", Kind.Primary => "Remove", _ => "Cancel" };
        Vector2 size = kind switch { Kind.Close => new(322, 96), Kind.Primary => new(370, 151), _ => new(375, 147) };
        var img = button.GetComponent<Image>();
        img.sprite = S($"Button_{key}_Normal");
        img.type = Image.Type.Sliced;
        img.color = Color.white;
        img.raycastTarget = true;
        button.transition = Selectable.Transition.SpriteSwap;
        button.spriteState = new SpriteState
        {
            highlightedSprite = S($"Button_{key}_Hover"),
            selectedSprite = S($"Button_{key}_Normal"),
            pressedSprite = S($"Button_{key}_Pressed"),
            disabledSprite = S($"Button_{key}_Pressed"),
        };
        img.canvasRenderer.SetColor(Color.white); // 앞서 칠해진 색조 전환을 지운다
        rt.sizeDelta = size;
        rt.localScale = Vector3.one * scale;

        var font = label != null ? label.font : TMP_Settings.defaultFontAsset;
        TextMeshProUGUI eyebrow = null;
        Image underline;
        switch (kind)
        {
            case Kind.Close:
                if (label != null) { Place(label.rectTransform, 0, 24, 322, 45); Style(label, 32, Ink, TextAlignmentOptions.Center, FontStyles.Bold); }
                underline = Pic(rt, "Underline", "Button_Close_Underline", 128, 69, 65, 3);
                break;
            case Kind.Primary:
                Txt(rt, font, "Number", "01", 0, 45, 52, 42, 29, Ink, TextAlignmentOptions.Center, FontStyles.Bold);
                eyebrow = Txt(rt, font, "Eyebrow", "REMOVE", 96, 25, 175, 20, 12, Paper, TextAlignmentOptions.Left, FontStyles.Normal, 33f);
                if (label != null) { Place(label.rectTransform, 96, 52, 160, 55); Style(label, 42, Color.white, TextAlignmentOptions.Left, FontStyles.Bold); }
                underline = Pic(rt, "Underline", "Button_Underline", 98, 112, 46, 4);
                break;
            default:
                // 큰 흐린 02 — 오른쪽 잘린 모서리 밖으로 안 넘치게 layout보다 조금 안쪽 · 작게
                Txt(rt, font, "Ghost", "02", 262, 48, 95, 90, 80, new Color(0.110f, 0.133f, 0.149f, 0.12f), TextAlignmentOptions.Left, FontStyles.Bold);
                Txt(rt, font, "Number", "02", 0, 27, 53, 42, 28, NumLight, TextAlignmentOptions.Center, FontStyles.Bold);
                eyebrow = Txt(rt, font, "Eyebrow", "RETURN", 96, 23, 180, 20, 12, Ink, TextAlignmentOptions.Left, FontStyles.Normal, 33f);
                if (label != null) { Place(label.rectTransform, 96, 57, 190, 49); Style(label, 37, Ink, TextAlignmentOptions.Left, FontStyles.Bold); label.transform.SetAsLastSibling(); }
                underline = Pic(rt, "Underline", "Button_Underline", 98, 110, 46, 4);
                break;
        }
        underline.gameObject.SetActive(false);
        if (!button.TryGetComponent<HoverToggle>(out var hover)) hover = button.gameObject.AddComponent<HoverToggle>();
        hover.Target = underline.gameObject;
        return eyebrow;
    }

    // 확정 동작(한글) → 영문 소제목
    public static string EnglishFor(string verb) => verb switch
    {
        "제거" => "REMOVE",
        "강화" => "UPGRADE",
        "복제" => "DUPLICATE",
        "변환" => "TRANSFORM",
        "선택" => "SELECT",
        _ => "CONFIRM",
    };

    private static void Place(RectTransform r, float x, float y, float w, float h)
    {
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0f, 1f);
        r.anchoredPosition = new Vector2(x, -y);
        r.sizeDelta = new Vector2(w, h);
    }

    private static void Style(TextMeshProUGUI t, float size, Color color, TextAlignmentOptions align, FontStyles style)
    {
        t.enableAutoSizing = false;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.fontStyle = style;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.raycastTarget = false;
    }

    private static TextMeshProUGUI Txt(RectTransform parent, TMP_FontAsset font, string name, string text, float x, float y, float w, float h,
        float size, Color color, TextAlignmentOptions align, FontStyles style, float spacing = 0f)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text;
        t.characterSpacing = spacing;
        Style(t, size, color, align, style);
        Place(t.rectTransform, x, y, w, h);
        return t;
    }

    private static Image Pic(RectTransform parent, string name, string sprite, float x, float y, float w, float h)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = S(sprite);
        img.type = Image.Type.Sliced;
        img.raycastTarget = false;
        Place(img.rectTransform, x, y, w, h);
        return img;
    }

    private class HoverToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject Target;
        public void OnPointerEnter(PointerEventData e) { if (Target != null) Target.SetActive(true); }
        public void OnPointerExit(PointerEventData e) { if (Target != null) Target.SetActive(false); }
        private void OnDisable() { if (Target != null) Target.SetActive(false); }
    }
}
