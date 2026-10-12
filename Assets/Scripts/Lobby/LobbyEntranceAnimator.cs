using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// 로비 메인 화면 등장 연출 (명일방주식 깔끔한 선 긋기 + 슬라이드).
// 배경이 살짝 확대된 채 가라앉음 → 가는 청록 선이 왼쪽에서 오른쪽으로 그어졌다 사라짐 → 고정 UI 패널이 오른쪽에서 들어옴
// → 시설 버튼이 위에서부터 차례로 튀어 들어와 박힘 → 나머지 글자·장식이 가볍게 들어옴.
// 이 오브젝트(LobbyKit)가 켜질 때마다 재생된다. 연출 중에도 입력은 막지 않는다.
public class LobbyEntranceAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform staticUi;
    [SerializeField] private RectTransform sweep;                        // 선 긋기용 가는 선(피벗 왼쪽). 재생할 때만 보인다
    [SerializeField] private List<RectTransform> buttons = new();        // 위에서부터 등장 순서대로
    [SerializeField] private List<RectTransform> sideElements = new();   // 왼쪽 요소는 왼쪽에서, 오른쪽 요소는 위에서

    [Header("타이밍")]
    [SerializeField, Min(0f)] private float buttonStart = 0.22f;
    [SerializeField, Min(0f)] private float buttonStagger = 0.06f;
    [SerializeField, Min(0f)] private float sideStart = 0.35f;
    [SerializeField, Min(0f)] private float sideStagger = 0.03f;

    private readonly Dictionary<RectTransform, Vector2> _rest = new();
    private Sequence _sequence;
    private float _lineWidth;

    private void Awake()
    {
        Cache(background);
        Cache(staticUi);
        foreach (var b in buttons) Cache(b);
        foreach (var e in sideElements) Cache(e);
        if (sweep != null) sweep.gameObject.SetActive(false);
    }

    private void OnEnable() => Play();

    private void OnDisable()
    {
        _sequence?.Kill();
        RestoreAll();
    }

    public void Play()
    {
        _sequence?.Kill();
        RestoreAll();
        _sequence = DOTween.Sequence().SetLink(gameObject);

        if (background != null)
        {
            var group = Group(background);
            group.alpha = 0f;
            background.localScale = Vector3.one * 1.06f;
            _sequence.Insert(0f, group.DOFade(1f, 0.5f));
            _sequence.Insert(0f, background.DOScale(1f, 0.9f).SetEase(Ease.OutCubic));
        }

        if (sweep != null)
        {
            // 가는 선이 왼쪽에서 오른쪽으로 그어진 뒤 사라진다
            sweep.gameObject.SetActive(true);
            if (_lineWidth <= 0f) _lineWidth = sweep.sizeDelta.x;
            sweep.sizeDelta = new Vector2(0f, sweep.sizeDelta.y);
            var sg = Group(sweep);
            sg.alpha = 1f;
            _sequence.Insert(0.05f, sweep.DOSizeDelta(new Vector2(_lineWidth, sweep.sizeDelta.y), 0.35f).SetEase(Ease.OutCubic));
            _sequence.Insert(0.5f, sg.DOFade(0f, 0.3f));
            _sequence.InsertCallback(0.82f, () => sweep.gameObject.SetActive(false));
        }

        if (staticUi != null) SlideIn(staticUi, new Vector2(140f, 0f), 0.12f, 0.45f, Ease.OutQuart);

        for (int i = 0; i < buttons.Count; i++)
        {
            var b = buttons[i];
            if (b == null) continue;
            SlideIn(b, new Vector2(90f, 0f), buttonStart + i * buttonStagger, 0.4f, Ease.OutBack);
        }

        for (int i = 0; i < sideElements.Count; i++)
        {
            var e = sideElements[i];
            if (e == null) continue;
            // 화면 왼쪽 절반은 왼쪽에서, 오른쪽 절반은 위에서 들어온다
            bool left = _rest.TryGetValue(e, out var rest) && rest.x < 836f;
            SlideIn(e, left ? new Vector2(-40f, 0f) : new Vector2(0f, 24f), sideStart + i * sideStagger, 0.35f, Ease.OutCubic);
        }
    }

    private void SlideIn(RectTransform target, Vector2 offset, float at, float duration, Ease ease)
    {
        if (!_rest.TryGetValue(target, out var rest)) return;
        var group = Group(target);
        group.alpha = 0f;
        target.anchoredPosition = rest + offset;
        _sequence.Insert(at, target.DOAnchorPos(rest, duration).SetEase(ease));
        _sequence.Insert(at, group.DOFade(1f, duration * 0.6f));
    }

    private void Cache(RectTransform target)
    {
        if (target != null && !_rest.ContainsKey(target)) _rest[target] = target.anchoredPosition;
    }

    private void RestoreAll()
    {
        foreach (var pair in _rest)
        {
            if (pair.Key == null) continue;
            pair.Key.anchoredPosition = pair.Value;
            if (pair.Key.TryGetComponent<CanvasGroup>(out var g)) g.alpha = 1f;
        }
        if (background != null) background.localScale = Vector3.one;
        if (sweep != null)
        {
            if (_lineWidth > 0f) sweep.sizeDelta = new Vector2(_lineWidth, sweep.sizeDelta.y);
            sweep.gameObject.SetActive(false);
        }
    }

    private static CanvasGroup Group(Component target)
        => target.TryGetComponent<CanvasGroup>(out var g) ? g : target.gameObject.AddComponent<CanvasGroup>();
}
