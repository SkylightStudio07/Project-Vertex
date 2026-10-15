using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using K = CompanionSkillUIKit;

// 초탄·막탄 표시. 기획: Docs/기획/무기_시스템.md
//   1. 탄약 칸 옆 칩 "초탄" "막탄": 아직 쓸 수 있으면 먹색, 손패에 그걸 발동시킬 카드가 있으면 청록으로 맥동, 이미 썼으면 회색.
//      초탄·막탄 판정을 쓰는 무기(WeaponData.UsesAmmoBoundary)가 아니면 칩을 숨긴다.
//   2. 손패 카드 위 태그: 지금 내면 초탄·막탄이 되는 카드에 "초탄" / "막탄" / "초탄 · 막탄".
//   3. 발동: 대상 적 위(대상이 없으면 탄약 칸 옆)에 "초탄!" "막탄!"이 튀어 올랐다 사라진다.
// UI는 Awake에서 코드로 만든다. 칩 좌표는 1920×1080 좌상단 기준.
public class AmmoBoundaryView : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private int sortingOrder = 92; // 손패(5)·카드 컷인(90) 위
    [SerializeField] private Vector2 chipPosition = new(1792f, 856f); // 첫 칩 왼쪽 위 (탄약 숫자 오른쪽, 헤어라인 위)
    [SerializeField] private Vector2 chipSize = new(50f, 24f);
    [SerializeField] private HandView hand;

    private static readonly Color Spent = new(0.79f, 0.81f, 0.83f, 1f);

    private BattleManager _battle;
    private RectTransform _root;
    private Chip _first, _last;
    private readonly Dictionary<CardView, Tag> _tags = new();
    private int _hash = int.MinValue;

    private void Awake()
    {
        _root = (RectTransform)transform;
        var canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        if (font == null) font = TMP_Settings.defaultFontAsset;
        _first = BuildChip("초탄", chipPosition);
        _last = BuildChip("막탄", chipPosition + new Vector2(chipSize.x + 4f, 0f));
    }

    private void Start()
    {
        _battle = BattleManager.Instance;
        if (hand == null) hand = FindFirstObjectByType<HandView>(FindObjectsInactive.Include);
        if (_battle != null) _battle.OnAmmoBoundaryUsed += HandleUsed;
    }

    private void OnDestroy()
    {
        if (_battle != null) _battle.OnAmmoBoundaryUsed -= HandleUsed;
    }

    private void Update()
    {
        var state = _battle != null && _battle.IsInBattle ? _battle.State : null;
        bool boundary = state?.CurrentWeapon != null && state.CurrentWeapon.UsesAmmoBoundary;

        // 손패 카드마다 예측 (값이 바뀐 프레임에만)
        bool anyFirst = false, anyLast = false;
        int hash = boundary ? 17 : 0;
        if (boundary)
        {
            hash = hash * 31 + state.Ammo;
            hash = hash * 31 + state.Energy;
            hash = hash * 31 + (state.FirstAmmoUseAvailable ? 1 : 0) + (state.LastAmmoUseAvailable ? 2 : 0);
            hash = hash * 31 + state.CurrentWeapon.GetInstanceID();
        }
        if (hand != null) foreach (var view in hand.CardViews) if (view != null) hash = hash * 31 + view.GetInstanceID();

        if (hash != _hash)
        {
            _hash = hash;
            var gone = new List<CardView>();
            foreach (var key in _tags.Keys) if (key == null) gone.Add(key);
            foreach (var key in gone) _tags.Remove(key);
            if (hand != null)
                foreach (var view in hand.CardViews)
                {
                    if (view == null) continue;
                    var (f, l) = boundary ? _battle.PredictAmmoBoundary(view.Data) : (false, false);
                    anyFirst |= f; anyLast |= l;
                    TagOf(view).Set(f, l);
                }
            _first.Set(boundary, state != null && state.FirstAmmoUseAvailable && state.Ammo > 0, anyFirst);
            _last.Set(boundary, state != null && state.LastAmmoUseAvailable && state.Ammo > 0, anyLast);
        }
    }

    // ───────── 칩 ─────────

    private Chip BuildChip(string label, Vector2 pos)
    {
        var chip = new Chip();
        chip.Back = K.Pic(_root, "Chip_" + label, null, pos.x, pos.y, chipSize.x, chipSize.y);
        chip.Text = K.Text(chip.Back.transform, "Label", font, 0, 0, chipSize.x, chipSize.y, 14, K.Paper, TextAlignmentOptions.Center, FontStyles.Bold);
        chip.Text.text = label;
        chip.Back.gameObject.SetActive(false);
        return chip;
    }

    private class Chip
    {
        public Image Back;
        public TextMeshProUGUI Text;
        private Tween _pulse;

        public void Set(bool visible, bool available, bool armed)
        {
            Back.gameObject.SetActive(visible);
            _pulse?.Kill();
            Back.transform.localScale = Vector3.one;
            if (!visible) return;
            Back.color = armed ? K.Accent : available ? K.Ink : Spent;
            Text.color = armed ? K.Ink : available ? K.Paper : K.Muted;
            if (armed)
                _pulse = Back.transform.DOScale(1.08f, 0.45f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetLink(Back.gameObject);
        }
    }

    // ───────── 손패 카드 태그 ─────────

    private Tag TagOf(CardView view)
    {
        if (_tags.TryGetValue(view, out var tag) && tag.Root != null) return tag;
        tag = new Tag();
        var go = new GameObject("AmmoBoundaryTag", typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = view.gameObject.layer;
        tag.Root = (RectTransform)go.transform;
        // 카드 그림 위쪽 가장자리 바로 위. 카드 그림은 축소된 큰 캔버스라 배율을 되돌려 화면 크기를 맞춘다
        var visual = view.Visual;
        float s = Mathf.Approximately(visual.localScale.x, 0f) ? 1f : 1f / visual.localScale.x;
        tag.Root.SetParent(visual, false);
        tag.Root.localScale = Vector3.one * s;
        tag.Root.anchorMin = tag.Root.anchorMax = new Vector2(0.5f, 1f);
        tag.Root.pivot = new Vector2(0.5f, 0f);
        tag.Root.anchoredPosition = new Vector2(0f, -14f * s); // 프레임 위 여백만큼 내려 모서리에 걸치게
        var back = go.AddComponent<Image>();
        back.color = K.Ink;
        back.raycastTarget = false;
        tag.Back = back;
        var bar = K.Pic(tag.Root, "Bar", null, 0, 0, 5, 30);
        bar.color = K.Accent;
        tag.Text = K.Text(tag.Root, "Label", font, 14, 0, 140, 30, 19, K.Accent, TextAlignmentOptions.Left, FontStyles.Bold);
        _tags[view] = tag;
        return tag;
    }

    private class Tag
    {
        public RectTransform Root;
        public Image Back;
        public TextMeshProUGUI Text;

        public void Set(bool first, bool last)
        {
            bool on = first || last;
            Root.gameObject.SetActive(on);
            if (!on) return;
            Text.text = first && last ? "초탄 · 막탄" : first ? "초탄" : "막탄";
            float w = Text.GetPreferredValues(Text.text).x + 28f;
            Root.sizeDelta = new Vector2(w, 30f);
        }
    }

    // ───────── 발동 팝업 ─────────

    private void HandleUsed(CardContext context, bool first, bool last)
    {
        string label = first && last ? "초탄 · 막탄!" : first ? "초탄!" : "막탄!";
        Vector2 local = FindPopupPoint(context);

        var rt = K.Group("BoundaryPopup", _root, stretch: false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = local;
        var back = K.Pic(rt, "Back", null, 0, 0, 0, 40);
        back.color = K.Ink;
        var bar = K.Pic(rt, "Bar", null, 0, 0, 5, 40);
        bar.color = K.Accent;
        var text = K.Text(rt, "Label", font, 16, 0, 200, 40, 26, K.Accent, TextAlignmentOptions.Left, FontStyles.Bold);
        text.text = label;
        float w = text.GetPreferredValues(label).x + 32f;
        rt.sizeDelta = new Vector2(w, 40f);
        back.rectTransform.sizeDelta = new Vector2(w, 40f);
        var group = rt.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;

        rt.localScale = Vector3.one * 1.4f;
        group.alpha = 0f;
        DOTween.Sequence().SetLink(rt.gameObject)
            .Append(group.DOFade(1f, 0.08f))
            .Join(rt.DOScale(1f, 0.18f).SetEase(Ease.OutBack))
            .Append(rt.DOAnchorPosY(local.y + 46f, 0.7f).SetEase(Ease.OutCubic))
            .Insert(0.55f, group.DOFade(0f, 0.35f))
            .OnComplete(() => Destroy(rt.gameObject));
    }

    // 대상 적 머리 위. 대상이 없거나(광역) 적 뷰를 못 찾으면 탄약 칸 위
    private Vector2 FindPopupPoint(CardContext context)
    {
        var cam = GetComponentInParent<Canvas>().rootCanvas.worldCamera;
        if (context?.PrimaryTarget is EnemyInstance target)
            foreach (var view in FindObjectsByType<EnemyView>(FindObjectsSortMode.None))
                if (view.Instance == target)
                {
                    var screen = RectTransformUtility.WorldToScreenPoint(cam, view.transform.position);
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screen, cam, out var p))
                        return p + new Vector2(0f, 150f);
                }
        return new Vector2(chipPosition.x - 960f + chipSize.x, 540f - chipPosition.y + 40f);
    }
}
