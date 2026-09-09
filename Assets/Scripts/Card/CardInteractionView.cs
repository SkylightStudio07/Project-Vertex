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
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class CardInteractionView : MonoBehaviour
{
    [Header("Visual")]
    [FormerlySerializedAs("visual")]
    [SerializeField] private RectTransform _visual; // 카드 확대/이동 연출을 적용할 시각 루트.

    [Header("Hover")]
    [FormerlySerializedAs("hoverScale")]
    [SerializeField] private Vector3 _hoverScale = new Vector3(0.45f, 0.45f, 1f); // 호버 상태에서 적용할 카드 확대 비율.

    [Header("Dragging")]
    [FormerlySerializedAs("dragSortingOrder")]
    [SerializeField] private int _dragSortingOrder = 100; // 드래그 중 카드가 앞에 보이도록 적용할 정렬 순서.

    [Header("Return")]
    [FormerlySerializedAs("returnDuration")]
    [SerializeField] private float _returnDuration = 0.25f; // 카드가 원래 위치로 돌아가는 시간.

    [Header("Targeting")]
    [FormerlySerializedAs("targetingCardYOffset")]
    [SerializeField] private float _targetingCardYOffset = 60f; // 타겟팅 중 카드가 손패 위로 이동할 높이.
    [FormerlySerializedAs("targetingMoveDuration")]
    [SerializeField] private float _targetingMoveDuration = 0.2f; // 타겟팅 위치까지 카드가 이동하는 시간.

    [Header("Debug Target Arrow")]
    [FormerlySerializedAs("showDebugTargetArrow")]
    [SerializeField] private bool _showDebugTargetArrow; // 개발 중 타겟 화살표 표시 여부.
    [FormerlySerializedAs("debugTargetArrowSortingOrder")]
    [SerializeField] private int _debugTargetArrowSortingOrder = 101; // 디버그 타겟 화살표의 기본 정렬 순서.
    [FormerlySerializedAs("debugTargetArrowColor")]
    [SerializeField] private Color _debugTargetArrowColor = new Color(0.85f, 0.15f, 0.15f, 0.95f); // 디버그 타겟 화살표 색상.

    private Canvas _cardCanvas; // 카드 프리팹의 Canvas 정렬을 제어하는 컴포넌트.
    private RectTransform _rootRect; // 카드 전체 위치와 회전을 담당하는 RectTransform.
    private RectTransform _targetingAnchor; // 타겟팅 중 카드 위치 계산 기준이 되는 부모 RectTransform.
    private TargetArrow _debugTargetArrow; // 개발용 타겟 화살표 UI.
    private Canvas _debugTargetArrowCanvas; // 개발용 타겟 화살표 정렬을 제어하는 Canvas.
    private RectTransform _debugArrowCanvasRect; // 개발용 화살표가 그려질 루트 Canvas RectTransform.
    private Vector3 _originalScale; // 카드 시각 루트의 기본 크기.
    private Vector3 _originalLocalPosition; // 카드 시각 루트의 기본 로컬 위치.
    private int _originalSortingOrder; // 손패 배치에서 카드가 가져야 하는 기본 정렬 순서.
    private Coroutine _movementCoroutine; // 현재 실행 중인 이동 코루틴.
    private Vector2 _targetingPointerPosition; // 타겟팅 중 마지막으로 기록한 포인터 위치.
    private bool _isDrawInPlaying; // 드로우 진입 애니메이션 중인지 나타내는 상태값.
    private Vector2 _drawInStartPosition; // 드로우 진입 애니메이션의 시작 위치.
    private Vector2 _drawInEndPosition; // 드로우 진입 애니메이션의 최종 손패 위치.
    private bool _isDiscardOutPlaying; // 턴 종료 퇴장 애니메이션 중인지 나타내는 상태값.
    private Vector2 _discardOutStartPosition; // 턴 종료 퇴장 애니메이션의 시작 위치.
    private Vector2 _discardOutEndPosition; // 턴 종료 퇴장 애니메이션의 최종 위치.
    private Action<CardInteractionView> _discardOutCompleted; // 퇴장 애니메이션 완료 후 호출할 콜백.
    private bool _isDestroying; // 오브젝트가 파괴 중인지 나타내는 상태값.

    // HandFanLayout이 지정한 "쉴 때" 자세. 부채꼴 미적용(레이아웃 미연결) 시 회전 0으로 그대로 둔다.
    private Vector2 _restingAnchoredPosition; // 손패에서 쉬고 있을 때의 기준 위치.
    private float _restingRotationZ; // 손패에서 쉬고 있을 때의 기준 회전값.

    // 드로우 진입 애니메이션 중이면 카드 입력을 막기 위해 true를 반환한다.
    public bool IsDrawInPlaying => _isDrawInPlaying;

    // 카드 이동 애니메이션 중이면 카드 입력을 막기 위해 true를 반환한다.
    public bool IsInteractionLocked => _isDrawInPlaying || _isDiscardOutPlaying;

    private void Awake()
    {
        _cardCanvas = GetComponent<Canvas>();
        _cardCanvas.overrideSorting = true;
        _originalSortingOrder = _cardCanvas.sortingOrder;
        _rootRect = transform as RectTransform;

        if (_visual != null)
        {
            _originalScale = _visual.localScale;
            _originalLocalPosition = _visual.localPosition;
        }
        else
        {
            _originalScale = Vector3.one;
            _originalLocalPosition = Vector3.zero;
        }
    }

    public void SetRestingSortingOrder(int sortingOrder)
    {
        _originalSortingOrder = sortingOrder;
        _cardCanvas.sortingOrder = sortingOrder;
    }

    public void SetTargetingAnchor(RectTransform anchor)
    {
        _targetingAnchor = anchor;
    }

    // HandFanLayout이 손패 갱신마다 호출 — 이 카드가 손패에서 쉬고 있을 때의 위치·회전(부채꼴 슬롯)을 지정한다.
    // 즉시 적용되며, 이후 EnterIdle()이 호출될 때마다 이 자세로 복귀한다.
    public void SetRestingPose(Vector2 anchoredPosition, float rotationZ)
    {
        _restingAnchoredPosition = anchoredPosition;
        _restingRotationZ = rotationZ;
        if (_rootRect == null) return;

        // 부채꼴 좌표/회전은 "카드 중심이 피벗"이라고 가정한 계산이다.
        // HorizontalLayoutGroup은 정렬 방식(기본 UpperLeft)에 따라 피벗/앵커를 모서리에
        // 남겨두는 경우가 많아서, 그대로 두면 중심이 아니라 모서리를 축으로 돌며 자리가 어긋난다.
        // (레이아웃 그룹 제거 후 부채꼴이 들쭉날쭉해 보이던 원인.) 매번 강제로 중앙 정렬한다.
        _rootRect.pivot     = new Vector2(0.5f, 0.5f);
        _rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        _rootRect.anchorMax = new Vector2(0.5f, 0.5f);

        _rootRect.anchoredPosition = _restingAnchoredPosition;
        _rootRect.localRotation = Quaternion.Euler(0f, 0f, _restingRotationZ);
    }

    public void EnterIdle()
    {
        StopMovement();
        HideDebugTargetArrow();
        if (_visual != null)
        {
            _visual.localScale = _originalScale;
            _visual.localPosition = _originalLocalPosition;
        }
        _cardCanvas.sortingOrder = _originalSortingOrder;
        // 부채꼴 각도로 복귀. 레이아웃 미연결 카드는 restingRotationZ가 0이라 무영향.
        if (_rootRect != null) _rootRect.localRotation = Quaternion.Euler(0f, 0f, _restingRotationZ);
    }

    public void EnterHover()
    {
        StopMovement();
        if (_visual != null) _visual.localScale = _hoverScale;
        BringCardToFront();
        // 부채꼴 각도를 펴서 카드 내용을 똑바로 보여준다 (STS 호버 연출).
        if (_rootRect != null) _rootRect.localRotation = Quaternion.identity;
    }

    public void EnterDragging()
    {
        StopMovement();
        HideDebugTargetArrow();
        if (_visual != null) _visual.localScale = _originalScale;
        BringCardToFront();
        if (_rootRect != null) _rootRect.localRotation = Quaternion.identity;
    }

    public void EnterTargeting(Vector2 pointerPosition)
    {
        StopMovement();
        _targetingPointerPosition = pointerPosition;
        if (_visual != null) _visual.localScale = _originalScale;
        BringCardToFront();
        if (_rootRect != null) _rootRect.localRotation = Quaternion.identity;
        EnsureDebugTargetArrow();
        BringDebugArrowToFront();
        UpdateDebugTargetArrow(pointerPosition);
        _movementCoroutine = StartCoroutine(TargetingMoveCoroutine());
    }

    public void UpdateTargetingPointer(Vector2 pointerPosition)
    {
        _targetingPointerPosition = pointerPosition;
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
        _movementCoroutine = StartCoroutine(ReturnCoroutine(onComplete));
    }

    public void EnterPlaying()
    {
        StopMovement();
        HideDebugTargetArrow();
    }

    // 드로우 대기 카드를 시작 위치에 모아두고 임시 렌더링 순서를 적용한다.
    public void PrepareDrawIn(Vector2 startAnchoredPosition, int temporarySortingOrder)
    {
        StopMovement();
        if (_rootRect == null) return;

        _isDrawInPlaying = true;
        _drawInStartPosition = startAnchoredPosition;
        _drawInEndPosition = _rootRect.anchoredPosition;
        _cardCanvas.sortingOrder = temporarySortingOrder;
        _rootRect.anchoredPosition = _drawInStartPosition;
    }

    // 준비된 드로우 진입 애니메이션을 시작한다.
    public void PlayPreparedDrawIn(float duration)
    {
        if (!_isDrawInPlaying) return;

        StopMovement();
        _isDrawInPlaying = true;
        _movementCoroutine = StartCoroutine(DrawInCoroutine(duration));
    }

    // 턴 종료 시 현재 손패 위치에서 EndPoint까지 이동하는 퇴장 애니메이션을 시작한다.
    public void PlayDiscardOut(
        Vector2 endAnchoredPosition,
        float duration,
        int temporarySortingOrder,
        Action<CardInteractionView> onComplete)
    {
        StopMovement();
        if (_rootRect == null) return;

        if (_visual != null)
        {
            _visual.localScale = _originalScale;
            _visual.localPosition = _originalLocalPosition;
        }
        _rootRect.localRotation = Quaternion.Euler(0f, 0f, _restingRotationZ);

        _isDiscardOutPlaying = true;
        _discardOutCompleted = onComplete;
        _discardOutStartPosition = _rootRect.anchoredPosition;
        _discardOutEndPosition = endAnchoredPosition;
        _cardCanvas.sortingOrder = temporarySortingOrder;
        _movementCoroutine = StartCoroutine(DiscardOutCoroutine(duration));
    }

    public void MoveVisualCenterToPointer(PointerEventData eventData)
    {
        if (_visual == null || _visual.parent is not RectTransform parent) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            return;

        Vector3 visualCenterWorld = _visual.TransformPoint(_visual.rect.center);
        Vector2 visualCenterLocal = parent.InverseTransformPoint(visualCenterWorld);
        _visual.anchoredPosition += localPoint - visualCenterLocal;
    }

    private void OnDestroy()
    {
        _isDestroying = true;
        if (_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);
        _movementCoroutine = null;
        _discardOutCompleted = null;
        if (_debugTargetArrow != null)
            Destroy(_debugTargetArrow.gameObject);
    }

    // 시작 위치에서 최종 손패 위치까지 카드 루트 위치만 이동시킨다.
    private IEnumerator DrawInCoroutine(float duration)
    {
        if (_rootRect == null)
        {
            _isDrawInPlaying = false;
            _cardCanvas.sortingOrder = _originalSortingOrder;
            _movementCoroutine = null;
            yield break;
        }

        _rootRect.anchoredPosition = _drawInStartPosition;

        float elapsed = 0f; // 지금까지 진행된 진입 애니메이션 시간.

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 0~1 범위로 정규화한 진행률.
            float t = duration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 2f); // 끝으로 갈수록 부드럽게 감속하는 진행률.
            _rootRect.anchoredPosition = Vector2.Lerp(_drawInStartPosition, _drawInEndPosition, eased);
            yield return null;
        }

        CompleteDrawIn();
    }

    // 드로우 애니메이션 종료 상태로 위치와 렌더링 순서를 복구한다.
    private void CompleteDrawIn()
    {
        if (_rootRect != null)
            _rootRect.anchoredPosition = _drawInEndPosition;

        _cardCanvas.sortingOrder = _originalSortingOrder;
        _isDrawInPlaying = false;
        _movementCoroutine = null;
    }

    // 현재 손패 위치에서 EndPoint까지 카드 루트 위치만 이동시킨다.
    private IEnumerator DiscardOutCoroutine(float duration)
    {
        if (_rootRect == null)
        {
            _isDiscardOutPlaying = false;
            _cardCanvas.sortingOrder = _originalSortingOrder;
            _movementCoroutine = null;
            yield break;
        }

        float elapsed = 0f; // 지금까지 진행된 퇴장 애니메이션 시간.

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 0~1 범위로 정규화한 진행률.
            float t = duration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 2f); // 끝으로 갈수록 부드럽게 감속하는 진행률.
            _rootRect.anchoredPosition = Vector2.Lerp(_discardOutStartPosition, _discardOutEndPosition, eased);
            yield return null;
        }

        CompleteDiscardOut();
    }

    // 퇴장 애니메이션 종료 상태로 위치와 렌더링 순서를 정리한다.
    private void CompleteDiscardOut()
    {
        if (_rootRect != null)
            _rootRect.anchoredPosition = _discardOutEndPosition;

        _cardCanvas.sortingOrder = _originalSortingOrder;
        _isDiscardOutPlaying = false;
        _movementCoroutine = null;

        Action<CardInteractionView> completed = _discardOutCompleted; // 콜백 호출 전 참조를 임시 보관한다.
        _discardOutCompleted = null;
        if (!_isDestroying)
            completed?.Invoke(this);
    }

    private IEnumerator TargetingMoveCoroutine()
    {
        if (_visual == null || !TryGetTargetingWorldPosition(out _))
        {
            _movementCoroutine = null;
            yield break;
        }

        Vector3 startPosition = _visual.position;
        float elapsed = 0f;

        while (elapsed < _targetingMoveDuration)
        {
            if (!TryGetTargetingWorldPosition(out Vector3 targetPosition))
                break;

            elapsed += Time.deltaTime;
            float t = _targetingMoveDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / _targetingMoveDuration);
            float eased = 1f - Mathf.Pow(1f - t, 2f);
            _visual.position = Vector3.Lerp(startPosition, targetPosition, eased);
            UpdateDebugTargetArrow(_targetingPointerPosition);
            yield return null;
        }

        if (_visual != null && TryGetTargetingWorldPosition(out Vector3 finalPosition))
        {
            _visual.position = finalPosition;
            UpdateDebugTargetArrow(_targetingPointerPosition);
        }

        _movementCoroutine = null;
    }

    private IEnumerator ReturnCoroutine(Action onComplete)
    {
        if (_visual == null)
        {
            _movementCoroutine = null;
            onComplete?.Invoke();
            yield break;
        }

        Vector3 startPosition = _visual.localPosition;
        float elapsed = 0f;

        while (elapsed < _returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = _returnDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / _returnDuration);
            float eased = 1f - Mathf.Pow(1f - t, 2f);
            _visual.localPosition = Vector3.Lerp(startPosition, _originalLocalPosition, eased);
            yield return null;
        }

        _visual.localPosition = _originalLocalPosition;
        _movementCoroutine = null;
        onComplete?.Invoke();
    }

    private bool TryGetTargetingWorldPosition(out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;
        if (_targetingAnchor == null) return false;

        Vector2 targetingPosition = _targetingAnchor.rect.center + Vector2.up * _targetingCardYOffset;
        worldPosition = _targetingAnchor.TransformPoint(targetingPosition);
        return true;
    }

    private void BringCardToFront()
    {
        int sortingOrder = _dragSortingOrder;
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

        _cardCanvas.sortingOrder = sortingOrder;
    }

    private void StopMovement()
    {
        if (_movementCoroutine == null) return;

        StopCoroutine(_movementCoroutine);
        if (_isDrawInPlaying)
            CompleteDrawIn();
        else if (_isDiscardOutPlaying)
            CompleteDiscardOut();
        else
            _movementCoroutine = null;
    }

    // Temporary runtime arrow for targeting development.
    private void BringDebugArrowToFront()
    {
        if (!_showDebugTargetArrow || _debugTargetArrowCanvas == null) return;

        _debugTargetArrowCanvas.sortingOrder =
            Mathf.Max(_debugTargetArrowSortingOrder, _cardCanvas.sortingOrder + 1);
    }

    private void EnsureDebugTargetArrow()
    {
        if (!_showDebugTargetArrow || _debugTargetArrow != null) return;

        Canvas rootCanvas = _cardCanvas.rootCanvas;
        _debugArrowCanvasRect = rootCanvas.transform as RectTransform;
        if (_debugArrowCanvasRect == null) return;

        var arrowObject = new GameObject(
            "Debug Card Target Arrow",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasRenderer));
        RectTransform rect = arrowObject.GetComponent<RectTransform>();
        rect.SetParent(_debugArrowCanvasRect, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsLastSibling();

        _debugTargetArrowCanvas = arrowObject.GetComponent<Canvas>();
        _debugTargetArrowCanvas.overrideSorting = true;
        _debugTargetArrowCanvas.sortingLayerID = _cardCanvas.sortingLayerID;
        BringDebugArrowToFront();

        _debugTargetArrow = arrowObject.AddComponent<TargetArrow>();
        _debugTargetArrow.raycastTarget = false;
        _debugTargetArrow.color = _debugTargetArrowColor;
        _debugTargetArrow.Hide();
    }

    private void UpdateDebugTargetArrow(Vector2 pointerPosition)
    {
        if (!_showDebugTargetArrow) return;

        EnsureDebugTargetArrow();
        if (_debugTargetArrow == null || _debugArrowCanvasRect == null || _visual == null) return;

        Camera camera = _cardCanvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _cardCanvas.rootCanvas.worldCamera;
        Vector3 originWorld = _visual.TransformPoint(_visual.rect.center);
        Vector2 originScreen = RectTransformUtility.WorldToScreenPoint(camera, originWorld);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _debugArrowCanvasRect, originScreen, camera, out Vector2 originLocal) ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _debugArrowCanvasRect, pointerPosition, camera, out Vector2 pointerLocal))
            return;

        _debugTargetArrow.SetPoints(originLocal, pointerLocal);
    }

    private void HideDebugTargetArrow()
    {
        if (_debugTargetArrow != null)
            _debugTargetArrow.Hide();
    }
}
