using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 패널이 켜질 때(OnEnable) 자동으로 등장 연출을 재생하는 범용 컴포넌트.
// 여는 쪽 코드는 기존처럼 SetActive(true)만 하면 되므로, 어느 창이든 이 컴포넌트만 붙이면 된다.
//
// 닫힘 연출(Hide)은 선택 사항이다. 이 프로젝트는 "창이 열려 있나"를 activeSelf로 검사하는 로직이 많아서
// (MapUIController.CanAdvanceToNextNode 등) 닫힘을 늦추면 그 사이 판정이 틀어질 수 있다.
// 그래서 Hide는 호출하는 쪽이 IsHiding까지 고려할 수 있는 곳(맵 등)에서만 쓴다.
[DisallowMultipleComponent]
public class UIPanelTransition : MonoBehaviour
{
    public enum Style { Fade, SlideFromRight, SlideFromLeft, SlideFromBottom, SlideFromTop, Scale, Dissolve }

    [SerializeField] private Style style = Style.Fade;
    [SerializeField, Min(0f)] private float duration = 0.3f;
    [SerializeField, Min(0f)] private float delay;
    [SerializeField] private Ease ease = Ease.OutCubic;
    [Tooltip("슬라이드 거리(px). 슬라이드 계열에서만 사용")]
    [SerializeField] private float slideDistance = 400f;
    [Tooltip("Scale 계열 시작 배율")]
    [SerializeField] private float startScale = 0.92f;
    [Tooltip("연출 중 버튼 입력 막기 — 등장 도중 눌려 상태가 꼬이는 것 방지")]
    [SerializeField] private bool blockInputWhilePlaying = true;
    [Min(0f)]
    [SerializeField] private float hideDuration = 0.2f;

    [Header("Dissolve 스타일 전용")]
    [Tooltip("디졸브할 이미지. 비우면 이 오브젝트의 Graphic. 자식 내용물은 페이드로 함께 나타난다")]
    [SerializeField] private Graphic dissolveTarget;
    [SerializeField] private Texture dissolveNoise;

    private RectTransform _rect;
    private CanvasGroup _group;
    private Vector2 _basePos;
    private Vector3 _baseScale;
    private bool _hasBase;
    private Sequence _seq;

    public bool IsHiding { get; private set; }
    public event Action OnShown;

    private void Awake()
    {
        _rect = transform as RectTransform;
        _group = GetComponent<CanvasGroup>();
        if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable() => PlayEnter();

    // 연출 도중 꺼지면(다른 코드가 SetActive(false)) 중간 상태로 굳지 않도록 원래 값으로 되돌린다.
    private void OnDisable()
    {
        _seq?.Kill();
        IsHiding = false;
        RestoreBase();
    }

    // 이미 켜져 있는데 닫히는 중이면 SetActive(true)로는 OnEnable이 안 불리므로 직접 다시 재생한다.
    public void Show()
    {
        if (gameObject.activeSelf && IsHiding) PlayEnter();
        else gameObject.SetActive(true);
    }

    public void Hide(Action onHidden = null)
    {
        if (!gameObject.activeSelf) { onHidden?.Invoke(); return; }

        CaptureBase();
        _seq?.Kill();
        IsHiding = true;
        _group.interactable = false;

        _seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        _seq.Join(_group.DOFade(0f, hideDuration));
        Vector2 offset = SlideOffset();
        if (_rect != null && offset != Vector2.zero)
            _seq.Join(_rect.DOAnchorPos(_basePos + offset * 0.5f, hideDuration).SetEase(Ease.InCubic));
        _seq.OnComplete(() =>
        {
            IsHiding = false;
            gameObject.SetActive(false); // OnDisable에서 원래 위치·알파로 복구됨
            onHidden?.Invoke();
        });
    }

    private void PlayEnter()
    {
        if (_group == null) Awake();
        CaptureBase();
        _seq?.Kill();
        IsHiding = false;
        RestoreBase();

        _seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        if (blockInputWhilePlaying) _group.interactable = false;

        switch (style)
        {
            case Style.Scale:
                _group.alpha = 0f;
                transform.localScale = _baseScale * startScale;
                _seq.Insert(delay, _group.DOFade(1f, duration * 0.8f));
                _seq.Insert(delay, transform.DOScale(_baseScale, duration).SetEase(Ease.OutBack));
                break;

            case Style.Dissolve:
                var target = dissolveTarget != null ? dissolveTarget : GetComponent<Graphic>();
                var dissolve = UIDissolve.In(target, dissolveNoise, duration, UIDissolve.DefaultEdge);
                if (dissolve != null) _seq.Insert(delay, dissolve);
                // 자식 내용물은 디졸브가 60% 진행됐을 때 페이드인
                foreach (Transform child in transform)
                {
                    if (!child.gameObject.activeSelf) continue;
                    // Unity 오브젝트엔 ?? 가 가짜 null을 못 걸러서 TryGetComponent로 분기한다.
                    if (!child.TryGetComponent<CanvasGroup>(out var cg)) cg = child.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    _seq.Insert(delay + duration * 0.6f, cg.DOFade(1f, duration * 0.6f));
                }
                break;

            default:
                _group.alpha = 0f;
                _seq.Insert(delay, _group.DOFade(1f, duration * 0.8f));
                Vector2 offset = SlideOffset();
                if (_rect != null && offset != Vector2.zero)
                {
                    _rect.anchoredPosition = _basePos + offset;
                    _seq.Insert(delay, _rect.DOAnchorPos(_basePos, duration).SetEase(ease));
                }
                break;
        }

        _seq.OnComplete(() =>
        {
            _group.interactable = true;
            OnShown?.Invoke();
        });
    }

    private Vector2 SlideOffset() => style switch
    {
        Style.SlideFromRight  => new Vector2(slideDistance, 0f),
        Style.SlideFromLeft   => new Vector2(-slideDistance, 0f),
        Style.SlideFromBottom => new Vector2(0f, -slideDistance),
        Style.SlideFromTop    => new Vector2(0f, slideDistance),
        _                     => Vector2.zero,
    };

    // 기준 위치·크기는 처음 켜질 때 한 번만 잡는다. 이후엔 연출이 끝난 값이 아니라 이 값으로 복귀.
    private void CaptureBase()
    {
        if (_hasBase) return;
        if (_rect != null) _basePos = _rect.anchoredPosition;
        _baseScale = transform.localScale;
        _hasBase = true;
    }

    private void RestoreBase()
    {
        if (!_hasBase) return;
        if (_rect != null) _rect.anchoredPosition = _basePos;
        transform.localScale = _baseScale;
        if (_group != null)
        {
            _group.alpha = 1f;
            _group.interactable = true;
        }
        if (style == Style.Dissolve)
            foreach (Transform child in transform)
                if (child.TryGetComponent<CanvasGroup>(out var cg)) cg.alpha = 1f;
    }
}
