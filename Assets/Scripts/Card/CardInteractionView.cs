// ============================================================
// filename   : CardInteractionView.cs
// 작성자     : xidsf - 최성제
// 작성일     : 2026-05-23
// description: 카드 상호작용 과정의 화면 표현을 담당하는 클래스.
//              Hover 확대, Drag/Targeting 이동, 복귀 애니메이션,
//              표시 순서 및 디버그 타겟 화살표를 관리한다.
// ============================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class CardInteractionView : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private RectTransform visual;

    [Header("Hover")]
    [SerializeField] private Vector3 hoverScale = new Vector3(0.45f, 0.45f, 1f);

    [Header("Dragging")]
    [SerializeField] private int dragSortingOrder = 100;

    [Header("Return")]
    [SerializeField] private float returnDuration = 0.25f;

    [Header("Targeting")]
    [SerializeField] private float targetingCardYOffset = 60f;
    [SerializeField] private float targetingMoveDuration = 0.2f;

    [Header("Debug Target Arrow")]
    [SerializeField] private bool showDebugTargetArrow;
    [SerializeField] private int debugTargetArrowSortingOrder = 101;
    [SerializeField] private Color debugTargetArrowColor = new Color(0.85f, 0.15f, 0.15f, 0.95f);

    private Canvas cardCanvas;
    private RectTransform targetingAnchor;
    private TargetArrow debugTargetArrow;
    private Canvas debugTargetArrowCanvas;
    private RectTransform debugArrowCanvasRect;
    private Vector3 originalScale;
    private Vector3 originalLocalPosition;
    private int originalSortingOrder;
    private Coroutine movementCoroutine;
    private Vector2 targetingPointerPosition;

    private void Awake()
    {
        cardCanvas = GetComponent<Canvas>();
        cardCanvas.overrideSorting = true;
        originalSortingOrder = cardCanvas.sortingOrder;

        if (visual != null)
        {
            originalScale = visual.localScale;
            originalLocalPosition = visual.localPosition;
        }
        else
        {
            originalScale = Vector3.one;
            originalLocalPosition = Vector3.zero;
        }
    }

    public void SetRestingSortingOrder(int sortingOrder)
    {
        originalSortingOrder = sortingOrder;
        cardCanvas.sortingOrder = sortingOrder;
    }

    public void SetTargetingAnchor(RectTransform anchor)
    {
        targetingAnchor = anchor;
    }

    public void EnterIdle()
    {
        StopMovement();
        HideDebugTargetArrow();
        if (visual != null)
        {
            visual.localScale = originalScale;
            visual.localPosition = originalLocalPosition;
        }
        cardCanvas.sortingOrder = originalSortingOrder;
    }

    public void EnterHover()
    {
        StopMovement();
        if (visual != null) visual.localScale = hoverScale;
        BringCardToFront();
    }

    public void EnterDragging()
    {
        StopMovement();
        HideDebugTargetArrow();
        if (visual != null) visual.localScale = originalScale;
        BringCardToFront();
    }

    public void EnterTargeting(Vector2 pointerPosition)
    {
        StopMovement();
        targetingPointerPosition = pointerPosition;
        if (visual != null) visual.localScale = originalScale;
        BringCardToFront();
        EnsureDebugTargetArrow();
        BringDebugArrowToFront();
        UpdateDebugTargetArrow(pointerPosition);
        movementCoroutine = StartCoroutine(TargetingMoveCoroutine());
    }

    public void UpdateTargetingPointer(Vector2 pointerPosition)
    {
        targetingPointerPosition = pointerPosition;
        UpdateDebugTargetArrow(pointerPosition);
    }

    public void ExitTargeting()
    {
        HideDebugTargetArrow();
    }

    public void EnterReturning(Action onComplete)
    {
        StopMovement();
        HideDebugTargetArrow();
        movementCoroutine = StartCoroutine(ReturnCoroutine(onComplete));
    }

    public void EnterPlaying()
    {
        StopMovement();
        HideDebugTargetArrow();
    }

    public void MoveVisualCenterToPointer(PointerEventData eventData)
    {
        if (visual == null || visual.parent is not RectTransform parent) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            return;

        Vector3 visualCenterWorld = visual.TransformPoint(visual.rect.center);
        Vector2 visualCenterLocal = parent.InverseTransformPoint(visualCenterWorld);
        visual.anchoredPosition += localPoint - visualCenterLocal;
    }

    private void OnDestroy()
    {
        StopMovement();
        if (debugTargetArrow != null)
            Destroy(debugTargetArrow.gameObject);
    }

    private IEnumerator TargetingMoveCoroutine()
    {
        if (visual == null || !TryGetTargetingWorldPosition(out _))
        {
            movementCoroutine = null;
            yield break;
        }

        Vector3 startPosition = visual.position;
        float elapsed = 0f;

        while (elapsed < targetingMoveDuration)
        {
            if (!TryGetTargetingWorldPosition(out Vector3 targetPosition))
                break;

            elapsed += Time.deltaTime;
            float t = targetingMoveDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / targetingMoveDuration);
            float eased = 1f - Mathf.Pow(1f - t, 2f);
            visual.position = Vector3.Lerp(startPosition, targetPosition, eased);
            UpdateDebugTargetArrow(targetingPointerPosition);
            yield return null;
        }

        if (visual != null && TryGetTargetingWorldPosition(out Vector3 finalPosition))
        {
            visual.position = finalPosition;
            UpdateDebugTargetArrow(targetingPointerPosition);
        }

        movementCoroutine = null;
    }

    private IEnumerator ReturnCoroutine(Action onComplete)
    {
        if (visual == null)
        {
            movementCoroutine = null;
            onComplete?.Invoke();
            yield break;
        }

        Vector3 startPosition = visual.localPosition;
        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = returnDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / returnDuration);
            float eased = 1f - Mathf.Pow(1f - t, 2f);
            visual.localPosition = Vector3.Lerp(startPosition, originalLocalPosition, eased);
            yield return null;
        }

        visual.localPosition = originalLocalPosition;
        movementCoroutine = null;
        onComplete?.Invoke();
    }

    private bool TryGetTargetingWorldPosition(out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;
        if (targetingAnchor == null) return false;

        Vector2 targetingPosition = targetingAnchor.rect.center + Vector2.up * targetingCardYOffset;
        worldPosition = targetingAnchor.TransformPoint(targetingPosition);
        return true;
    }

    private void BringCardToFront()
    {
        int sortingOrder = dragSortingOrder;
        if (transform.parent != null)
        {
            foreach (Transform sibling in transform.parent)
            {
                if (sibling == transform) continue;

                Canvas siblingCanvas = sibling.GetComponent<Canvas>();
                if (siblingCanvas != null)
                    sortingOrder = Mathf.Max(sortingOrder, siblingCanvas.sortingOrder + 1);
            }
        }

        cardCanvas.sortingOrder = sortingOrder;
    }

    private void StopMovement()
    {
        if (movementCoroutine == null) return;

        StopCoroutine(movementCoroutine);
        movementCoroutine = null;
    }

    // Temporary runtime arrow for targeting development.
    private void BringDebugArrowToFront()
    {
        if (!showDebugTargetArrow || debugTargetArrowCanvas == null) return;

        debugTargetArrowCanvas.sortingOrder =
            Mathf.Max(debugTargetArrowSortingOrder, cardCanvas.sortingOrder + 1);
    }

    private void EnsureDebugTargetArrow()
    {
        if (!showDebugTargetArrow || debugTargetArrow != null) return;

        Canvas rootCanvas = cardCanvas.rootCanvas;
        debugArrowCanvasRect = rootCanvas.transform as RectTransform;
        if (debugArrowCanvasRect == null) return;

        var arrowObject = new GameObject(
            "Debug Card Target Arrow",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasRenderer));
        RectTransform rect = arrowObject.GetComponent<RectTransform>();
        rect.SetParent(debugArrowCanvasRect, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsLastSibling();

        debugTargetArrowCanvas = arrowObject.GetComponent<Canvas>();
        debugTargetArrowCanvas.overrideSorting = true;
        debugTargetArrowCanvas.sortingLayerID = cardCanvas.sortingLayerID;
        BringDebugArrowToFront();

        debugTargetArrow = arrowObject.AddComponent<TargetArrow>();
        debugTargetArrow.raycastTarget = false;
        debugTargetArrow.color = debugTargetArrowColor;
        debugTargetArrow.Hide();
    }

    private void UpdateDebugTargetArrow(Vector2 pointerPosition)
    {
        if (!showDebugTargetArrow) return;

        EnsureDebugTargetArrow();
        if (debugTargetArrow == null || debugArrowCanvasRect == null || visual == null) return;

        Camera camera = cardCanvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : cardCanvas.rootCanvas.worldCamera;
        Vector3 originWorld = visual.TransformPoint(visual.rect.center);
        Vector2 originScreen = RectTransformUtility.WorldToScreenPoint(camera, originWorld);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                debugArrowCanvasRect, originScreen, camera, out Vector2 originLocal) ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                debugArrowCanvasRect, pointerPosition, camera, out Vector2 pointerLocal))
            return;

        debugTargetArrow.SetPoints(originLocal, pointerLocal);
    }

    private void HideDebugTargetArrow()
    {
        if (debugTargetArrow != null)
            debugTargetArrow.Hide();
    }
}
