using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 로비 시설 화면 등장 연출 (명일방주식). 훈련장·마테리얼처럼 "머리 + 세로 열 여러 개" 구조인 화면 루트에 붙인다.
//   종이 바탕이 살짝 커진 채 가라앉음 → 머리 아래로 청록 선이 왼쪽에서 그어짐 → 머리 글자가 왼쪽에서 차례로
//   → 열(왼쪽 → 가운데 → 오른쪽)마다 요소가 아래에서 차례로 올라옴 → 목록 줄·칸이 위에서부터 하나씩 나타남
// 요소 분류는 위치로 자동으로 한다(머리 = 위쪽 띠, 열 = x 위치). 따로 연결할 것 없이 붙이면 켜질 때마다 재생된다.
// 연출 중에도 입력은 막지 않는다. 끊기면 모든 위치·투명도를 제자리로 되돌린다.
public class UIScreenEntrance : MonoBehaviour
{
    [SerializeField] private string backgroundName = "Base";
    [SerializeField] private float headerBottom = 125f;              // 이 y(위에서부터)보다 위는 머리
    [SerializeField] private float sweepY = 138f;                   // 청록 선을 그을 y (머리 아래 구분선 위치)
    [SerializeField] private float[] columnEdges = { 560f, 1120f };  // 열 경계 x (키트 좌표)
    [SerializeField] private Color sweepColor = new(0.05f, 0.72f, 0.95f, 1f);
    [SerializeField, Min(0)] private int maxListItems = 10;          // 목록마다 차례로 보여 줄 앞쪽 줄 수
    [SerializeField, Min(0.1f)] private float speed = 1f;

    private readonly Dictionary<RectTransform, Vector2> _pos = new();
    private readonly Dictionary<Transform, Vector3> _scale = new();
    private readonly List<CanvasGroup> _groups = new();
    private Sequence _seq;
    private RectTransform _sweep;
    private Coroutine _co;

    private void OnEnable()
    {
        Restore();
        _co = StartCoroutine(PlayNextFrame());
    }

    private void OnDisable()
    {
        if (_co != null) StopCoroutine(_co);
        Restore();
    }

    // 목록 줄은 화면 스크립트가 켜질 때 만들기도 해서 한 프레임 뒤에 모은다
    private IEnumerator PlayNextFrame()
    {
        HideAllImmediately();
        yield return null;
        Play();
    }

    private void HideAllImmediately()
    {
        foreach (RectTransform c in transform) if (c.gameObject.activeSelf) Group(c).alpha = 0f;
    }

    public void Play()
    {
        Restore();
        _seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

        var items = new List<RectTransform>();
        foreach (RectTransform c in transform) if (c.gameObject.activeSelf) items.Add(c);

        // 바탕
        var bg = transform.Find(backgroundName) as RectTransform;
        if (bg != null)
        {
            var g = Group(bg); g.alpha = 0f;
            Scale(bg, 1.02f);
            _seq.Insert(0f, g.DOFade(1f, 0.25f / speed));
            _seq.Insert(0f, bg.DOScale(_scale[bg], 0.5f / speed).SetEase(Ease.OutCubic));
            items.Remove(bg);
        }

        PlaySweep();

        // 머리 · 열 나누기
        var header = new List<RectTransform>();
        var cols = new List<RectTransform>[columnEdges.Length + 1];
        for (int i = 0; i < cols.Length; i++) cols[i] = new List<RectTransform>();
        foreach (var rt in items)
        {
            Vector2 tl = PlaceOf(rt);
            if (tl.y < headerBottom) { header.Add(rt); continue; }
            int col = 0;
            while (col < columnEdges.Length && tl.x >= columnEdges[col]) col++;
            cols[col].Add(rt);
        }

        header.Sort((a, b) => PlaceOf(a).x.CompareTo(PlaceOf(b).x));
        for (int i = 0; i < header.Count; i++) Slide(header[i], new Vector2(-36f, 0f), 0.08f + i * 0.035f, 0.35f);

        for (int c = 0; c < cols.Length; c++)
        {
            var list = cols[c];
            list.Sort((a, b) => PlaceOf(a).y.CompareTo(PlaceOf(b).y));
            float colStart = 0.16f + c * 0.08f;
            for (int i = 0; i < list.Count; i++)
            {
                float at = colStart + i * 0.025f;
                var rt = list[i];
                if (rt.name.Contains("Backdrop"))                         // 과녁 원판: 돌며 커짐
                {
                    var g = Group(rt); g.alpha = 0f;
                    Scale(rt, 0.86f);
                    var rot = rt.localEulerAngles;
                    rt.localEulerAngles = new Vector3(0f, 0f, -24f);
                    _seq.Insert(at / speed, g.DOFade(1f, 0.35f / speed));
                    _seq.Insert(at / speed, rt.DOScale(_scale[rt], 0.6f / speed).SetEase(Ease.OutCubic));
                    _seq.Insert(at / speed, rt.DOLocalRotate(rot, 0.6f / speed).SetEase(Ease.OutCubic));
                }
                else if (rt.name.Contains("PreviewArt"))                  // 미리보기 그림: 살짝 커지며 또렷해짐
                {
                    var g = Group(rt); g.alpha = 0f;
                    Scale(rt, 0.94f);
                    _seq.Insert((at + 0.1f) / speed, g.DOFade(1f, 0.35f / speed));
                    _seq.Insert((at + 0.1f) / speed, rt.DOScale(_scale[rt], 0.45f / speed).SetEase(Ease.OutBack));
                }
                else if (rt.name.Contains("Keeper"))                      // 시설 담당 캐릭터: 왼쪽에서 천천히 들어옴
                    Slide(rt, new Vector2(-70f, 0f), at, 0.55f);
                else Slide(rt, new Vector2(0f, -22f), at, 0.32f);

                var scroll = rt.GetComponent<ScrollRect>();
                if (scroll != null && scroll.content != null) StaggerList(scroll.content, at + 0.12f);
            }
        }
    }

    // 목록 줄·칸: 위에서부터 하나씩 (레이아웃이 위치를 잡으므로 투명도·크기만 만진다)
    private void StaggerList(RectTransform content, float start)
    {
        int n = 0;
        foreach (RectTransform row in content)
        {
            if (!row.gameObject.activeSelf) continue;
            if (n >= maxListItems) break;
            var g = Group(row); g.alpha = 0f;
            Scale(row, 0.96f);
            float at = (start + n * 0.035f) / speed;
            _seq.Insert(at, g.DOFade(1f, 0.25f / speed));
            _seq.Insert(at, row.DOScale(_scale[row], 0.3f / speed).SetEase(Ease.OutCubic));
            n++;
        }
    }

    private void Slide(RectTransform rt, Vector2 offset, float at, float duration)
    {
        if (!_pos.ContainsKey(rt)) _pos[rt] = rt.anchoredPosition;
        var g = Group(rt); g.alpha = 0f;
        rt.anchoredPosition = _pos[rt] + offset;
        _seq.Insert(at / speed, rt.DOAnchorPos(_pos[rt], duration / speed).SetEase(Ease.OutCubic));
        _seq.Insert(at / speed, g.DOFade(1f, duration * 0.7f / speed));
    }

    private void PlaySweep()
    {
        if (_sweep == null)
        {
            var go = new GameObject("EntranceSweep", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = gameObject.layer;
            _sweep = (RectTransform)go.transform;
            _sweep.SetParent(transform, false);
            var img = go.GetComponent<Image>(); img.color = sweepColor; img.raycastTarget = false;
        }
        var parent = (RectTransform)transform;
        _sweep.SetAsLastSibling();
        _sweep.anchorMin = _sweep.anchorMax = new Vector2(0f, 1f);
        _sweep.pivot = new Vector2(0f, 0.5f);
        _sweep.anchoredPosition = new Vector2(90f, -sweepY);
        _sweep.sizeDelta = new Vector2(0f, 2f);
        _sweep.gameObject.SetActive(true);
        var g = Group(_sweep); g.alpha = 1f;
        float width = parent.rect.width > 0 ? parent.rect.width - 180f : 1490f;
        _seq.Insert(0.04f / speed, _sweep.DOSizeDelta(new Vector2(width, 2f), 0.38f / speed).SetEase(Ease.OutCubic));
        _seq.Insert(0.4f / speed, g.DOFade(0f, 0.3f / speed));
        _seq.InsertCallback(0.72f / speed, () => { if (_sweep != null) _sweep.gameObject.SetActive(false); });
    }

    private void Restore()
    {
        if (_seq != null && _seq.IsActive()) _seq.Kill();
        _seq = null;
        foreach (var p in _pos) if (p.Key != null) p.Key.anchoredPosition = p.Value;
        foreach (var s in _scale) if (s.Key != null) s.Key.localScale = s.Value;
        foreach (var g in _groups) if (g != null) g.alpha = 1f;
        foreach (var p in _pos) if (p.Key != null && p.Key.name.Contains("Backdrop")) p.Key.localEulerAngles = Vector3.zero;
        _pos.Clear(); _scale.Clear(); _groups.Clear();
        if (_sweep != null) _sweep.gameObject.SetActive(false);
    }

    private void Scale(Transform t, float factor)
    {
        if (!_scale.ContainsKey(t)) _scale[t] = t.localScale;
        if (t is RectTransform rt && !_pos.ContainsKey(rt)) _pos[rt] = rt.anchoredPosition;   // 되돌릴 때 같이 복구
        t.localScale = _scale[t] * factor;
    }

    private CanvasGroup Group(Component c)
    {
        if (!c.TryGetComponent<CanvasGroup>(out var g)) g = c.gameObject.AddComponent<CanvasGroup>();
        if (!_groups.Contains(g)) _groups.Add(g);
        return g;
    }

    // 분류용 위치. 화면 전체로 펼쳐진 묶음(미리보기 정보·상세 판 등)은 안쪽 내용 중 가장 왼쪽 위 기준
    private Vector2 PlaceOf(RectTransform rt)
    {
        var parent = (RectTransform)rt.parent;
        if (rt.rect.width < parent.rect.width * 0.9f) return TopLeft(rt);
        Vector2 best = new(float.MaxValue, float.MaxValue);
        foreach (RectTransform c in rt)
        {
            if (!c.gameObject.activeSelf) continue;
            Vector2 p = TopLeft(c);
            if (p.y < best.y || (Mathf.Approximately(p.y, best.y) && p.x < best.x)) best = p;
        }
        return best.x == float.MaxValue ? TopLeft(rt) : best;
    }

    // 부모 기준 왼쪽 위 좌표 (위쪽 0, 아래로 +)
    private Vector2 TopLeft(RectTransform rt)
    {
        var parent = (RectTransform)rt.parent;
        Vector3[] c = new Vector3[4];
        rt.GetWorldCorners(c);
        Vector3 tl = parent.InverseTransformPoint(c[1]);
        Rect r = parent.rect;
        return new Vector2(tl.x - r.xMin, r.yMax - tl.y);
    }
}
