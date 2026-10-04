using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
// filename   : BlessingIntroCinematic.cs
// description: 축복 노드 진입 연출 (블루 아카이브 인연 스토리 CG 도입부 문법).
//              암전 → 배경 이곳저곳을 천천히 훑는 컷 2개(컷 사이 짧은 암전, 캐릭터는 숨김)
//              → 암전 뒤 캐릭터가 나타난 얼굴 살짝 클로즈업 → 원래 구도로 빠지며 대사·선택지 UI가 들어온다.
//              배경(Background)과 캐릭터(BlessingCharacter)를 같은 '카메라'로 함께 확대·이동한다.
//              화면 아무 곳이나 누르면 건너뛴다. BlessingView.Open이 Play를 부른다.
// ============================================================
public class BlessingIntroCinematic : MonoBehaviour
{
    [Serializable]
    public class Shot
    {
        [Tooltip("컷 시작 시 화면 가운데에 올 지점 (BlessingView 기준 캔버스 좌표, 가운데 원점)")]
        public Vector2 fromCenter;
        public Vector2 toCenter;
        public float fromZoom = 1.8f;
        public float toZoom = 1.8f;
        [Min(0.1f)] public float duration = 2f;
    }

    [Header("대상 (비우면 이름으로 찾음)")]
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform character;
    [SerializeField] private CanvasGroup uiGroup; // ChoiceOverlay — 연출 중 숨긴다
    [Tooltip("풍경 컷 동안 캐릭터를 숨기고 얼굴 컷에서 등장시킨다")]
    [SerializeField] private bool hideCharacterInScenery = true;

    [Header("컷")]
    [Tooltip("배경을 훑는 컷들. 컷 사이에는 짧은 암전이 들어간다")]
    [SerializeField] private List<Shot> sceneryShots = new()
    {
        new Shot { fromCenter = new Vector2(-420f, -170f), toCenter = new Vector2(-240f, -205f), fromZoom = 1.9f, toZoom = 1.8f, duration = 2.2f }, // 꽃밭·호수
        new Shot { fromCenter = new Vector2(-60f, 150f), toCenter = new Vector2(0f, 200f), fromZoom = 1.7f, toZoom = 1.6f, duration = 2.2f },      // 보름달 (위로 천천히)
    };
    [Tooltip("마지막 얼굴 클로즈업 컷. x는 캐릭터(BlessingCharacter) 위치 기준이라 캐릭터를 옮겨도 따라간다")]
    [SerializeField] private Shot faceShot = new() { fromCenter = new Vector2(20f, 360f), toCenter = new Vector2(20f, 372f), fromZoom = 1.9f, toZoom = 2.05f, duration = 2f };

    [Header("시간")]
    [SerializeField] private float openingBlack = 0.6f; // 암전 유지
    [SerializeField] private float dipDuration = 0.35f; // 컷 사이 암전 (나가기·들어오기 각각)
    [SerializeField] private float pullBackDuration = 1.1f;
    [SerializeField] private float uiFadeDuration = 0.4f;
    [SerializeField] private int sortingOrderWhilePlaying = 200; // 연출 중에는 맵 HUD 위로 올린다

    public bool IsPlaying => _seq != null && _seq.IsActive();

    private Image _black;
    private Sequence _seq;
    private Canvas _canvas;
    private CanvasGroup _characterGroup;
    private int _baseSorting;
    private bool _baseOverride;
    private Vector3 _bgPos, _bgScale, _chPos, _chScale;
    private bool _hasBase;

    private void Awake()
    {
        if (background == null) background = transform.Find("Background") as RectTransform;
        if (character == null) character = transform.Find("BlessingCharacter") as RectTransform;
        if (uiGroup == null)
        {
            var overlay = transform.Find("ChoiceOverlay");
            if (overlay != null)
            {
                uiGroup = overlay.GetComponent<CanvasGroup>();
                if (uiGroup == null) uiGroup = overlay.gameObject.AddComponent<CanvasGroup>();
            }
        }
        _canvas = GetComponent<Canvas>();
        if (character != null)
        {
            _characterGroup = character.GetComponent<CanvasGroup>();
            if (_characterGroup == null) _characterGroup = character.gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void OnDisable() => Finish();

    public void Play()
    {
        if (background == null) return;
        Finish();
        CaptureBase();
        EnsureBlack();

        if (_canvas != null)
        {
            _baseOverride = _canvas.overrideSorting;
            _baseSorting = _canvas.sortingOrder;
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = sortingOrderWhilePlaying;
        }

        _black.gameObject.SetActive(true);
        _black.transform.SetAsLastSibling(); // 꽃잎보다 위
        _black.color = Color.black;
        if (uiGroup != null) { uiGroup.alpha = 0f; uiGroup.blocksRaycasts = false; }
        if (_characterGroup != null && hideCharacterInScenery && sceneryShots.Count > 0) _characterGroup.alpha = 0f;

        var first = sceneryShots.Count > 0 ? sceneryShots[0] : FaceShotOnCharacter();
        SetCamera(first.fromCenter, first.fromZoom);

        _seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        _seq.AppendInterval(openingBlack);

        var face = FaceShotOnCharacter();
        var shots = new List<Shot>(sceneryShots) { face };
        for (int i = 0; i < shots.Count; i++)
        {
            var shot = shots[i];
            var s = shot;
            // 들어오기: 암전에서 컷이 열리고, 컷이 끝날 때까지 카메라가 천천히 흐른다
            bool isFace = i == shots.Count - 1;
            _seq.AppendCallback(() =>
            {
                SetCamera(s.fromCenter, s.fromZoom);
                if (isFace && _characterGroup != null) _characterGroup.alpha = 1f; // 암전 뒤에서 등장
            });
            float t0 = _seq.Duration(false);
            _seq.Insert(t0, _black.DOFade(0f, dipDuration).SetEase(Ease.OutSine));
            _seq.Insert(t0, DOTween.To(() => 0f, k => SetCamera(Vector2.Lerp(s.fromCenter, s.toCenter, k), Mathf.Lerp(s.fromZoom, s.toZoom, k)), 1f, shot.duration).SetEase(Ease.Linear));
            // 풍경 컷이 끝나면 짧게 암전, 얼굴 컷은 그대로 원래 구도로 빠진다
            if (i < shots.Count - 1)
                _seq.Insert(t0 + shot.duration - dipDuration, _black.DOFade(1f, dipDuration).SetEase(Ease.InSine));
        }

        _seq.Append(DOTween.To(() => 0f, k => SetCamera(Vector2.Lerp(face.toCenter, Vector2.zero, k), Mathf.Lerp(face.toZoom, 1f, k)), 1f, pullBackDuration).SetEase(Ease.InOutCubic));
        if (uiGroup != null)
            _seq.Append(uiGroup.DOFade(1f, uiFadeDuration));
        _seq.OnComplete(Finish);
    }

    // 건너뛰기·종료: 원래 구도·UI로 되돌린다
    public void Finish()
    {
        if (_seq != null && _seq.IsActive()) _seq.Kill();
        _seq = null;
        if (_hasBase) SetCamera(Vector2.zero, 1f);
        if (_black != null) _black.gameObject.SetActive(false);
        if (uiGroup != null) { uiGroup.alpha = 1f; uiGroup.blocksRaycasts = true; }
        if (_characterGroup != null) _characterGroup.alpha = 1f;
        if (_canvas != null && _canvas.sortingOrder == sortingOrderWhilePlaying)
        {
            _canvas.sortingOrder = _baseSorting;
            _canvas.overrideSorting = _baseOverride;
        }
    }

    private Shot FaceShotOnCharacter()
    {
        var offset = new Vector2(_chPos.x, 0f);
        return new Shot
        {
            fromCenter = faceShot.fromCenter + offset, toCenter = faceShot.toCenter + offset,
            fromZoom = faceShot.fromZoom, toZoom = faceShot.toZoom, duration = faceShot.duration
        };
    }

    private void CaptureBase()
    {
        if (_hasBase) return; // 연출 도중 다시 불려도 처음 구도를 기준으로 삼는다
        _bgPos = background.localPosition; _bgScale = background.localScale;
        if (character != null) { _chPos = character.localPosition; _chScale = character.localScale; }
        _hasBase = true;
    }

    // 카메라: center 지점을 화면 가운데에 두고 zoom배 확대. 배경 밖이 보이지 않게 중심을 제한한다
    private void SetCamera(Vector2 center, float zoom)
    {
        zoom = Mathf.Max(1f, zoom);
        var size = ((RectTransform)transform).rect.size * 0.5f;
        float mx = size.x * (1f - 1f / zoom), my = size.y * (1f - 1f / zoom);
        center = new Vector2(Mathf.Clamp(center.x, -mx, mx), Mathf.Clamp(center.y, -my, my));
        Vector3 c = center;
        background.localPosition = (_bgPos - c) * zoom;
        background.localScale = _bgScale * zoom;
        if (character != null)
        {
            character.localPosition = (_chPos - c) * zoom;
            character.localScale = _chScale * zoom;
        }
    }

    private void EnsureBlack()
    {
        if (_black != null) return;
        var go = new GameObject("IntroBlack", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        var rt = (RectTransform)go.transform;
        rt.SetParent(transform, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        _black = go.GetComponent<Image>();
        _black.raycastTarget = true; // 투명해져도 클릭을 받아 건너뛰기에 쓴다
        var btn = go.GetComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(Finish);
    }
}
