using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 동료 스킬 UI(지시 목록·컷인)를 코드로 조립하는 도구.
// 조각: Resources/CompanionSkill/ (원본 ArtDirection/CompanionSkillMockup/Extracted — 2배 해상도, PPU 200).
// 좌표는 layout.json의 1920×1080 좌상단 기준 값을 그대로 쓴다(앵커·피벗 왼쪽 위).
public static class CompanionSkillUIKit
{
    public static readonly Color Ink = new(0.086f, 0.094f, 0.106f, 1f);   // #16181B
    public static readonly Color Paper = new(0.933f, 0.941f, 0.949f, 1f); // #EEF0F2
    public static readonly Color Accent = new(0.051f, 0.722f, 0.949f, 1f); // #0DB8F2
    public static readonly Color Muted = new(0.573f, 0.6f, 0.624f, 1f);   // #92999F

    public static Sprite Load(string name)
    {
        var s = Resources.Load<Sprite>("CompanionSkill/" + name);
        if (s == null) Debug.LogWarning($"[CompanionSkillUI] 조각 없음: Resources/CompanionSkill/{name}");
        return s;
    }

    public static RectTransform Group(string name, Transform parent, bool stretch = true)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        if (stretch)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }
        else TopLeft(rt, 0, 0, 0, 0);
        return rt;
    }

    public static void TopLeft(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(w, h);
    }

    public static Image Pic(Transform parent, string name, string sprite, float x, float y, float w, float h, bool sliced = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = sprite != null ? Load(sprite) : null;
        img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        img.raycastTarget = false;
        TopLeft(img.rectTransform, x, y, w, Mathf.Max(h, 1f));
        return img;
    }

    public static TextMeshProUGUI Text(Transform parent, string name, TMP_FontAsset font, float x, float y, float w, float h,
        float size, Color color, TextAlignmentOptions align, FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
        TopLeft(t.rectTransform, x, y, w, h);
        return t;
    }
}
