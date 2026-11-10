using UnityEngine;
using UnityEngine.UI;

// 속성 칩 하나 (그림자 · 판 · 글리프). 전투 HP바 위와 적 기록 머리 줄이 같이 쓴다.
// 아트: Resources/EnemyTrait (ArtDirection/EnemyTraitMockup/Extracted, layout.json) — 칩 46×38, 판·그림자·글리프가 같은 캔버스
// 상태: 미상('?') / 공개(흰 글리프) / 켜짐(특효 카드로 겨냥 — 청록 판 + 먹색 글리프)
public class TraitChip : MonoBehaviour
{
    public static readonly Vector2 Size = new(46f, 38f);
    private static readonly Vector2 ShadowOffset = new(2f, -2f);
    private static readonly Color ShadowColor = new(0f, 0f, 0f, 0.5f);
    private static readonly Color GlyphRevealed = Color.white;
    private static readonly Color GlyphUnknown = new(0.886f, 0.898f, 0.902f, 1f); // #E2E5E6
    private static readonly Color GlyphActive = new(0.063f, 0.102f, 0.125f, 1f);   // #101A20

    private Image _plate;
    private Image _glyph;
    private bool _unknown;

    public EnemyTrait Trait { get; private set; }

    private static Sprite Load(string name) => Resources.Load<Sprite>("EnemyTrait/" + name);

    // trait == None 이면 미상 칩
    public static TraitChip Create(Transform parent, EnemyTrait trait)
    {
        var go = new GameObject("TraitChip", typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.sizeDelta = Size;
        var chip = go.AddComponent<TraitChip>();
        Layer(rt, "Shadow", Load("Chip_Shadow"), ShadowColor).rectTransform.anchoredPosition = ShadowOffset;
        chip._plate = Layer(rt, "Plate", Load("Chip_Base"), Color.white);
        chip._glyph = Layer(rt, "Glyph", null, Color.white);
        chip.Set(trait);
        return chip;
    }

    private static Image Layer(RectTransform parent, string name, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = parent.gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    public void Set(EnemyTrait trait)
    {
        Trait = trait;
        _unknown = trait == EnemyTrait.None;
        _glyph.sprite = Load(_unknown ? "Trait_Unknown" : "Trait_" + trait);
        SetActive(false);
    }

    public void SetActive(bool on)
    {
        on &= !_unknown;
        _plate.sprite = Load(on ? "Chip_Active" : "Chip_Base");
        _glyph.color = on ? GlyphActive : _unknown ? GlyphUnknown : GlyphRevealed;
    }
}
