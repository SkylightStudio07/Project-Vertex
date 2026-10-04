using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// ============================================================
// filename   : BlessingUIHide.cs
// description: 축복 화면 UI 숨기기 (블루 아카이브·명일방주식 감상 모드).
//              오른쪽 위 눈 버튼, 우클릭, 스페이스로 대사판·선택지·이름판을 페이드로 빼고 캐릭터·배경만 남긴다.
//              숨긴 동안에는 화면 아무 곳이나 누르면(또는 같은 키) 돌아온다. 맵 HUD도 가리도록 캔버스 정렬을 잠시 올린다.
//              진입 연출(BlessingIntroCinematic) 중에는 동작하지 않는다.
// ============================================================
public class BlessingUIHide : MonoBehaviour
{
    [SerializeField] private CanvasGroup uiGroup;        // ChoiceOverlay (비우면 이름으로 찾음)
    [SerializeField] private Button toggleButton;        // 눈 버튼 — 숨기면 같이 사라진다
    [SerializeField] private Key toggleKey = Key.Space;
    [SerializeField] private bool rightClickToggles = true;
    [SerializeField, Min(0f)] private float fadeDuration = 0.25f;
    [SerializeField] private int sortingOrderWhileHidden = 200; // 맵 HUD 위로

    public bool IsHidden { get; private set; }

    private Canvas _canvas;
    private int _baseSorting;
    private bool _baseOverride;
    private Button _restoreCatcher;
    private CanvasGroup _buttonGroup;
    private BlessingIntroCinematic _intro;

    private void Awake()
    {
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
        _intro = GetComponent<BlessingIntroCinematic>();
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(Hide);
            _buttonGroup = toggleButton.GetComponent<CanvasGroup>();
            if (_buttonGroup == null) _buttonGroup = toggleButton.gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void OnDisable() => Show(true);

    private void Update()
    {
        if (_intro != null && _intro.IsPlaying) return;
        bool key = Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame;
        bool rightClick = rightClickToggles && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        if (key || rightClick)
        {
            if (IsHidden) Show(false);
            else if (!IsCardSelectorOpen()) Hide();
        }
    }

    // 카드 선택 창(강화·정화) 같은 다른 오버레이가 떠 있으면 숨기지 않는다
    private bool IsCardSelectorOpen() => CardListView.Instance != null && CardListView.Instance.IsOpen;

    public void Hide()
    {
        if (IsHidden || uiGroup == null) return;
        if (_intro != null && _intro.IsPlaying) return;
        IsHidden = true;

        if (_canvas != null)
        {
            _baseOverride = _canvas.overrideSorting;
            _baseSorting = _canvas.sortingOrder;
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = sortingOrderWhileHidden;
        }
        uiGroup.blocksRaycasts = false;
        uiGroup.DOKill();
        uiGroup.DOFade(0f, fadeDuration).SetUpdate(true).SetLink(gameObject);
        if (_buttonGroup != null)
        {
            _buttonGroup.blocksRaycasts = false;
            _buttonGroup.DOKill();
            _buttonGroup.DOFade(0f, fadeDuration).SetUpdate(true).SetLink(gameObject);
        }
        EnsureCatcher();
        _restoreCatcher.gameObject.SetActive(true);
        _restoreCatcher.transform.SetAsLastSibling();
    }

    public void Show() => Show(false);

    private void Show(bool instant)
    {
        if (!IsHidden) return;
        IsHidden = false;
        if (_restoreCatcher != null) _restoreCatcher.gameObject.SetActive(false);

        if (uiGroup != null)
        {
            uiGroup.DOKill();
            uiGroup.blocksRaycasts = true;
            if (instant) uiGroup.alpha = 1f;
            else uiGroup.DOFade(1f, fadeDuration).SetUpdate(true).SetLink(gameObject);
        }
        if (_buttonGroup != null)
        {
            _buttonGroup.DOKill();
            _buttonGroup.blocksRaycasts = true;
            if (instant) _buttonGroup.alpha = 1f;
            else _buttonGroup.DOFade(1f, fadeDuration).SetUpdate(true).SetLink(gameObject);
        }
        if (_canvas != null && _canvas.sortingOrder == sortingOrderWhileHidden)
        {
            _canvas.sortingOrder = _baseSorting;
            _canvas.overrideSorting = _baseOverride;
        }
        // 숨기기 버튼이 선택된 채 남아 스페이스가 버튼을 다시 누르지 않게 선택을 푼다
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    // 숨긴 동안 화면 전체를 덮는 투명 버튼 — 누르면 UI가 돌아온다
    private void EnsureCatcher()
    {
        if (_restoreCatcher != null) return;
        var go = new GameObject("UIHideCatcher", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        var rt = (RectTransform)go.transform;
        rt.SetParent(transform, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f);
        img.raycastTarget = true;
        _restoreCatcher = go.GetComponent<Button>();
        _restoreCatcher.transition = Selectable.Transition.None;
        _restoreCatcher.onClick.AddListener(Show);
        go.SetActive(false);
    }
}
