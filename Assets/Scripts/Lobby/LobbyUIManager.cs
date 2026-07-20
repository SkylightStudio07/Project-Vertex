using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Owns lobby UI presentation, including screen switching and facility transition effects.
/// Facility handlers own interaction state.
/// </summary>
public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] private GameObject mainView;

    [Header("Transition")]
    [SerializeField] private bool useBonfireTransition = true;
    [SerializeField, Min(0.05f)] private float coverDuration = 0.3f;
    [SerializeField, Min(0.05f)] private float revealDuration = 0.42f;
    [SerializeField, Min(0f)] private float blackoutHoldDuration = 0.08f;
    [SerializeField, Min(2000f)] private float bandTravelDistance = 5400f;
    [SerializeField] private Vector2 transitionTravelDirection = new Vector2(0.866f, 0.5f);
    [SerializeField] private RectTransform transitionRoot;
    [SerializeField] private CanvasGroup transitionCanvasGroup;
    [SerializeField] private Image blocker;
    [SerializeField] private Image[] bands;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;

    private GameObject currentView;
    private FacilityState currentFacilityState;
    private Vector2[] bandVisiblePositions;
    private Coroutine transitionRoutine;

    public event System.Action<GameObject, FacilityState> FacilityViewShown;

    private void Awake()
    {
        CacheTransitionOverlay();
        SetOverlayVisible(false);
    }

    private void Start()
    {
        ShowMainViewImmediate();
    }

    public virtual void ShowFacilityView(GameObject facilityView)
    {
        if (TryGetFacilityState(facilityView, out FacilityState facilityState))
        {
            ShowFacilityView(facilityView, facilityState);
            return;
        }

        ShowFacilityViewImmediate(facilityView, default);
    }

    public virtual void ShowFacilityView(GameObject facilityView, FacilityState facilityState)
    {
        if (facilityView == null)
            return;

        if (!ShouldUseTransition(facilityState))
        {
            ShowFacilityViewImmediate(facilityView, facilityState);
            NotifyFacilityViewShown(facilityView, facilityState);
            return;
        }

        PlayTransition(
            GetFacilityTitle(facilityState),
            GetFacilitySubtitle(facilityState),
            () => ShowFacilityViewImmediate(facilityView, facilityState),
            () => NotifyFacilityViewShown(facilityView, facilityState));
    }

    public virtual void ShowMainView()
    {
        if (currentView == null)
        {
            ShowMainViewImmediate();
            return;
        }

        if (!ShouldUseTransition(currentFacilityState))
        {
            ShowMainViewImmediate();
            return;
        }

        PlayTransition(GetFacilityTitle(currentFacilityState), GetFacilitySubtitle(currentFacilityState), ShowMainViewImmediate, null);
    }

    private void ShowFacilityViewImmediate(GameObject facilityView, FacilityState facilityState)
    {
        if (facilityView == null)
            return;

        if (currentView != null && currentView != facilityView)
            currentView.SetActive(false);

        currentView = facilityView;
        currentFacilityState = facilityState;
        mainView?.SetActive(false);
        currentView.SetActive(true);
    }

    private void ShowMainViewImmediate()
    {
        if (currentView != null)
            currentView.SetActive(false);

        currentView = null;
        currentFacilityState = default;
        mainView?.SetActive(true);
    }

    private bool ShouldUseTransition(FacilityState facilityState)
    {
        return useBonfireTransition
            && facilityState.FacilityType == FacilityType.Bonfire;
    }

    private void PlayTransition(string title, string subtitle, System.Action swapView, System.Action afterReveal)
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
        {
            swapView?.Invoke();
            afterReveal?.Invoke();
            return;
        }

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(PlayTransitionRoutine(title, subtitle, swapView, afterReveal));
    }

    private IEnumerator PlayTransitionRoutine(string title, string subtitle, System.Action swapView, System.Action afterReveal)
    {
        CacheTransitionOverlay();
        if (!HasTransitionOverlay())
        {
            swapView?.Invoke();
            afterReveal?.Invoke();
            transitionRoutine = null;
            yield break;
        }

        SetTransitionText(title, subtitle);
        SetOverlayVisible(true);
        SetOverlayAlpha(0f);

        yield return AnimateTransition(-bandTravelDistance, 0f, 0f, 1f, coverDuration);
        SetOverlayAlpha(1f);
        yield return new WaitForSecondsRealtime(blackoutHoldDuration);

        swapView?.Invoke();
        transitionRoot.SetAsLastSibling();

        yield return AnimateTransition(0f, bandTravelDistance, 1f, 0f, revealDuration);
        SetOverlayAlpha(0f);

        SetOverlayVisible(false);
        afterReveal?.Invoke();
        transitionRoutine = null;
    }

    private IEnumerator AnimateTransition(float fromDistance, float toDistance, float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            ApplyBandPositions(fromDistance, toDistance, t);
            SetOverlayAlpha(Mathf.SmoothStep(fromAlpha, toAlpha, t));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        ApplyBandPositions(fromDistance, toDistance, 1f);
        SetOverlayAlpha(toAlpha);
    }

    private void CacheTransitionOverlay()
    {
        if (transitionRoot != null)
            return;

        Transform foundRoot = transform.Find("LobbyTransitionOverlay");
        if (foundRoot == null)
            return;

        transitionRoot = foundRoot as RectTransform;
        transitionCanvasGroup = foundRoot.GetComponent<CanvasGroup>();
    }

    private void ApplyBandPositions(float fromDistance, float toDistance, float progress)
    {
        if (bands == null)
            return;

        CacheBandVisiblePositions();
        Vector2 direction = GetTravelDirection();

        for (int i = 0; i < bands.Length; i++)
        {
            if (bands[i] == null)
                continue;

            float delayedProgress = Mathf.Clamp01((progress - i * 0.055f) / 0.82f);
            float eased = Mathf.SmoothStep(0f, 1f, delayedProgress);
            float distance = Mathf.Lerp(fromDistance, toDistance, eased);
            RectTransform bandTransform = bands[i].rectTransform;
            bandTransform.anchoredPosition = bandVisiblePositions[i] + direction * distance;
        }
    }

    private void SetTransitionText(string title, string subtitle)
    {
        if (titleText != null)
            titleText.text = title;

        if (subtitleText != null)
            subtitleText.text = subtitle;
    }

    private void SetOverlayVisible(bool visible)
    {
        if (transitionRoot == null)
            return;

        transitionRoot.gameObject.SetActive(visible);
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.blocksRaycasts = visible;
            transitionCanvasGroup.interactable = visible;
        }
        if (!visible)
            SetOverlayAlpha(0f);
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (transitionCanvasGroup != null)
            transitionCanvasGroup.alpha = Mathf.Clamp01(alpha);
    }

    private bool HasTransitionOverlay()
    {
        return transitionRoot != null
            && transitionCanvasGroup != null
            && blocker != null
            && bands != null
            && bands.Length > 0
            && titleText != null
            && subtitleText != null;
    }

    private void NotifyFacilityViewShown(GameObject facilityView, FacilityState facilityState)
    {
        FacilityViewShown?.Invoke(facilityView, facilityState);
    }

    private void CacheBandVisiblePositions()
    {
        if (bands == null)
            return;

        if (bandVisiblePositions != null && bandVisiblePositions.Length == bands.Length)
            return;

        bandVisiblePositions = new Vector2[bands.Length];
        for (int i = 0; i < bands.Length; i++)
            bandVisiblePositions[i] = bands[i] != null ? bands[i].rectTransform.anchoredPosition : Vector2.zero;
    }

    private Vector2 GetTravelDirection()
    {
        if (transitionTravelDirection.sqrMagnitude <= Mathf.Epsilon)
            return Vector2.right;

        return transitionTravelDirection.normalized;
    }

    private bool TryGetFacilityState(GameObject facilityView, out FacilityState facilityState)
    {
        facilityState = default;
        if (facilityView == null)
            return false;

        FacilityInteractionHandler handler = facilityView.GetComponentInParent<FacilityInteractionHandler>();
        if (handler == null)
            handler = facilityView.GetComponentInChildren<FacilityInteractionHandler>(true);

        FacilityManager facilityManager = LobbyManager.Instance != null ? LobbyManager.Instance.FacilityManager : null;
        if (handler == null || facilityManager == null)
            return false;

        facilityState = facilityManager.GetFacilityState(handler.FacilityType);
        return facilityState.IsRegistered;
    }

    private static string GetFacilityTitle(FacilityState facilityState)
    {
        if (!string.IsNullOrWhiteSpace(facilityState.DisplayName))
            return facilityState.DisplayName;

        return facilityState.FacilityType != FacilityType.None ? facilityState.FacilityType.ToString() : string.Empty;
    }

    private static string GetFacilitySubtitle(FacilityState facilityState)
    {
        return !string.IsNullOrWhiteSpace(facilityState.Description) ? facilityState.Description : string.Empty;
    }
}
