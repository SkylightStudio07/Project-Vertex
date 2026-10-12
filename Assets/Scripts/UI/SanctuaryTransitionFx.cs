using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

// 성소(협력자 선택) 화면 전환 연출 — 명일방주식 선 긋기 + 슬라이드.
//   후보 화면: 청록 선이 왼쪽에서 그어짐 → 머리 글자가 왼쪽에서 차례로 → 사선 띠 3장이 오른쪽에서 차례로 박힘 → 후보 확인 버튼이 아래에서
//   상세 화면: 전신 아트가 오른쪽에서 밀려 들어오고 뒤 원형 눈금이 커지며 등장 → 이름 자간이 좁혀지며 등장, 강조선·구분선이 그어짐
//              → 정보 칸이 왼쪽에서 차례로 → 뒤로·합류 버튼이 아래에서
//   합류 확정: 전신 아트가 살짝 튀고 흰 섬광 한 번
// 재생할 때마다 지금 자리를 '제자리'로 잡고, 끊기면(Stop) 그 자리로 되돌린다.
// 화면 배치를 바꾸기 전(캐릭터별 소속 위치 등)에 Stop을 먼저 불러 두면 연출이 끊겨도 배치가 틀어지지 않는다. SelectCoopCharUI가 부른다.
public class SanctuaryTransitionFx : MonoBehaviour
{
    [SerializeField] private RectTransform selectView;
    [SerializeField] private RectTransform detailView;
    [SerializeField] private Color sweepColor = new(0.05f, 0.72f, 0.95f, 1f);
    [SerializeField, Min(0.1f)] private float speed = 1f; // 전체 속도 배율

    private readonly Dictionary<RectTransform, Vector2> _restPos = new();
    private readonly Dictionary<RectTransform, Vector2> _restSize = new();
    private Sequence _seq;
    private RectTransform _sweep;
    private UnityEngine.UI.Image _flash;

    private void Awake()
    {
        if (selectView == null) selectView = transform.Find("SelectView") as RectTransform;
        if (detailView == null) detailView = transform.Find("DetailView") as RectTransform;
    }

    private void OnDisable() => Stop();

    // ───────── 후보 화면 ─────────
    public void PlaySelectIn()
    {
        if (selectView == null) return;
        Begin();
        PlaySweep(selectView, 250f);

        var header = selectView.Find("Header");
        float t = 0.05f;
        if (header != null)
            foreach (RectTransform c in header) { Slide(c, new Vector2(-40f, 0f), t, 0.35f, Ease.OutCubic); t += 0.04f; }

        var strips = selectView.Find("Strips");
        if (strips != null)
        {
            t = 0.12f;
            foreach (RectTransform s in strips)
            {
                if (!s.gameObject.activeSelf) continue;
                Slide(s, new Vector2(180f, 0f), t, 0.45f, Ease.OutQuart);
                t += 0.08f;
            }
        }
        if (selectView.Find("DetailButton") is RectTransform button) Slide(button, new Vector2(0f, -40f), 0.42f, 0.35f, Ease.OutCubic);
    }

    // ───────── 상세 화면 ─────────
    public void PlayDetailIn()
    {
        if (detailView == null) return;
        Begin();

        if (detailView.Find("ArtBackdrop") is RectTransform backdrop)
        {
            var g = Group(backdrop); g.alpha = 0f;
            backdrop.localScale = Vector3.one * 0.8f;
            _seq.Insert(0f, g.DOFade(1f, 0.4f / speed));
            _seq.Insert(0f, backdrop.DOScale(1f, 0.6f / speed).SetEase(Ease.OutCubic));
        }
        if (detailView.Find("ArtArea") is RectTransform art) Slide(art, new Vector2(160f, 0f), 0.05f, 0.55f, Ease.OutQuart);

        PlaySweep(detailView, 300f);

        if (detailView.Find("Name")?.GetComponent<TextMeshProUGUI>() is TextMeshProUGUI name)
        {
            float rest = name.characterSpacing;
            name.characterSpacing = rest + 40f;
            _seq.Insert(0.1f, DOTween.To(() => name.characterSpacing, v => name.characterSpacing = v, rest, 0.45f / speed).SetEase(Ease.OutCubic));
            _seq.OnKill(() => { if (name != null) name.characterSpacing = rest; });
        }

        // 위에서 아래로 차례: 번호 → 이름·소속 → 강조선 → 정보 판 → 구분선·항목 → 버튼
        float t = 0.08f;
        foreach (var n in new[] { "Number", "NumberUnderline", "Name", "Affiliation" }) { Fade(n, new Vector2(-30f, 0f), t); t += 0.04f; }
        Grow("Accent", 0.2f);
        Fade("InfoPanel", new Vector2(-50f, 0f), 0.16f);
        t = 0.26f;
        for (int i = 1; i <= 4; i++)
        {
            Grow("Rule " + i, t);
            Fade("Label " + i, new Vector2(-20f, 0f), t + 0.04f);
            t += 0.07f;
        }
        t = 0.3f;
        foreach (var n in new[] { "Profile", "AffinityLevel", "AffinityCell 1", "AffinityCell 2", "AffinityCell 3", "AffinityCell 4", "AffinityCell 5",
                                  "CardSlot", "CardArtWindow", "CardName", "CardCost", "CardDescription", "RewardSummary" })
        {
            Fade(n, new Vector2(-24f, 0f), t);
            t += 0.025f;
        }
        Fade("BackButton", new Vector2(0f, -40f), 0.45f);
        Fade("JoinButton", new Vector2(0f, -40f), 0.5f);
    }

    // ───────── 합류 확정 ─────────
    public void PlayConfirm()
    {
        if (detailView == null) return;
        if (detailView.Find("ArtArea") is RectTransform art)
            art.DOPunchScale(Vector3.one * 0.04f, 0.35f, 6, 0.6f).SetUpdate(true).SetLink(gameObject);
        var flash = Flash();
        flash.color = new Color(1f, 1f, 1f, 0f);
        DOTween.Sequence().SetUpdate(true).SetLink(gameObject)
            .Append(flash.DOFade(0.55f, 0.06f))
            .Append(flash.DOFade(0f, 0.4f))
            .OnComplete(() => flash.gameObject.SetActive(false));
    }

    // ───────── 내부 ─────────
    private void Begin()
    {
        Stop();
        _seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
    }

    public void Stop()
    {
        if (_seq != null && _seq.IsActive()) _seq.Kill();
        _seq = null;
        foreach (var p in _restPos)
        {
            if (p.Key == null) continue;
            p.Key.anchoredPosition = p.Value;
            if (p.Key.TryGetComponent<CanvasGroup>(out var g)) g.alpha = 1f;
        }
        foreach (var p in _restSize) if (p.Key != null) p.Key.sizeDelta = p.Value;
        _restPos.Clear();
        _restSize.Clear();
        if (_sweep != null) _sweep.gameObject.SetActive(false);
    }

    private void Slide(RectTransform rt, Vector2 offset, float at, float duration, Ease ease)
    {
        if (rt == null || !rt.gameObject.activeSelf) return;
        if (!_restPos.TryGetValue(rt, out var rest)) _restPos[rt] = rest = rt.anchoredPosition;
        var g = Group(rt);
        g.alpha = 0f;
        rt.anchoredPosition = rest + offset;
        _seq.Insert(at / speed, rt.DOAnchorPos(rest, duration / speed).SetEase(ease));
        _seq.Insert(at / speed, g.DOFade(1f, duration * 0.6f / speed));
    }

    private void Fade(string childName, Vector2 offset, float at)
        => Slide(detailView.Find(childName) as RectTransform, offset, at, 0.32f, Ease.OutCubic);

    // 선이 왼쪽 끝에서 오른쪽으로 그어진다 (피벗이 왼쪽 위인 가는 선)
    private void Grow(string childName, float at)
    {
        if (!(detailView.Find(childName) is RectTransform rt) || !rt.gameObject.activeSelf) return;
        if (!_restSize.TryGetValue(rt, out var size)) _restSize[rt] = size = rt.sizeDelta;
        rt.sizeDelta = new Vector2(0f, size.y);
        _seq.Insert(at / speed, rt.DOSizeDelta(size, 0.4f / speed).SetEase(Ease.OutCubic));
    }

    // 화면을 가로지르는 청록 선 하나가 그어졌다 사라진다
    private void PlaySweep(RectTransform parent, float y)
    {
        if (_sweep == null)
        {
            var go = new GameObject("FxSweep", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            go.layer = gameObject.layer;
            _sweep = (RectTransform)go.transform;
            var img = go.GetComponent<UnityEngine.UI.Image>();
            img.color = sweepColor;
            img.raycastTarget = false;
        }
        _sweep.SetParent(parent, false);
        _sweep.SetAsLastSibling();
        _sweep.anchorMin = _sweep.anchorMax = new Vector2(0f, 1f);
        _sweep.pivot = new Vector2(0f, 0.5f);
        _sweep.anchoredPosition = new Vector2(0f, -y);
        _sweep.sizeDelta = new Vector2(0f, 2f);
        _sweep.gameObject.SetActive(true);
        var g = Group(_sweep);
        g.alpha = 1f;
        float width = ((RectTransform)transform).rect.width;
        _seq.Insert(0f, _sweep.DOSizeDelta(new Vector2(width, 2f), 0.35f / speed).SetEase(Ease.OutCubic));
        _seq.Insert(0.3f / speed, g.DOFade(0f, 0.3f / speed));
        _seq.InsertCallback(0.62f / speed, () => { if (_sweep != null) _sweep.gameObject.SetActive(false); });
    }

    private UnityEngine.UI.Image Flash()
    {
        if (_flash == null)
        {
            var go = new GameObject("FxFlash", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            go.layer = gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(transform, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            _flash = go.GetComponent<UnityEngine.UI.Image>();
            _flash.raycastTarget = false;
        }
        _flash.gameObject.SetActive(true);
        _flash.transform.SetAsLastSibling();
        return _flash;
    }

    private static CanvasGroup Group(Component c)
        => c.TryGetComponent<CanvasGroup>(out var g) ? g : c.gameObject.AddComponent<CanvasGroup>();
}
