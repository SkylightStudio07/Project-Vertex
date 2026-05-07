// ============================================================
// filename   : CardHandler.cs
// 작성자      : xidsf - 최성제
// 작성일      : 2026-05-07
// description : 카드 조작을 위한 메인 클래스. 각 카드에 부착되어 카드의 이동, 회전, 드래그 앤 드롭 등의 기능을 담당한다.
// 카드의 상태를 관리하고, 플레이어의 입력에 따라 카드의 행동을 제어한다.
// 카드 이동 시 기존 위치에 가상의 PlaceGolder를 생성하여 슬롯 레이아웃을 유지하는 방식으로 구현한다.
// ============================================================

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
public class CardHandler : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum CardState { Idle, Hover, Dragging, Returning, Playing }

    [Header("스케일 대상")]
    [SerializeField] private RectTransform visual;

    [Header("호버 설정")]
    [SerializeField] private Vector3 hoverScale = new Vector3(0.45f, 0.45f, 1f);
    [SerializeField] private int hoverSortingOrder = 10;

    [Header("드래그 설정")]
    [SerializeField] private int dragSortingOrder = 100;

    [Header("복귀 설정")]
    [SerializeField] private float returnDuration = 0.25f;

    [Header("드롭 판정 (추후 구현)")]
    [SerializeField] private float dropYThreshold = 0f;

    // 다른 카드의 hover 효과 차단용 전역 플래그
    private static bool isAnyDragging;

    private Canvas cardCanvas;

    private Vector3 cardOriginalScale;
    private Vector3 cardOriginalVisualLocalPos;
    private int cardOriginalSortingOrder;

    private CardState state = CardState.Idle;

    private Vector3 returnStartPos;
    private float returnElapsed;

    private void Awake()
    {
        cardCanvas = GetComponent<Canvas>();
        cardCanvas.overrideSorting = true;

        cardOriginalSortingOrder = cardCanvas.sortingOrder;
        if (visual != null)
        {
            cardOriginalScale = visual.localScale;
            cardOriginalVisualLocalPos = visual.localPosition;
        }
        else
        {
            cardOriginalScale = Vector3.one;
            cardOriginalVisualLocalPos = Vector3.zero;
        }
    }

    private void Update()
    {
        if (state == CardState.Returning) UpdateReturn();
    }

    // ─── Pointer Events ───────────────────────────────
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (state != CardState.Idle) return;
        if (isAnyDragging) return;
        SetState(CardState.Hover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (state != CardState.Hover) return;
        SetState(CardState.Idle);
    }

    // ─── Drag Events ──────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (state != CardState.Hover && state != CardState.Idle) return;
        SetState(CardState.Dragging);
    }

    // 카드 Root는 슬롯에 고정. Visual만 이동.
    public void OnDrag(PointerEventData eventData)
    {
        if (state != CardState.Dragging) return;
        if (visual == null) return;
        visual.anchoredPosition += eventData.delta / cardCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (state != CardState.Dragging) return;
        SetState(TryDrop(eventData) ? CardState.Playing : CardState.Returning);
    }

    // 추후: Y 임계값 + 타겟 raycast 조합으로 사용 판정. 현재는 항상 무효 → 복귀.
    private bool TryDrop(PointerEventData eventData)
    {
        return false;
    }

    // ─── State Machine ────────────────────────────────
    private void SetState(CardState next)
    {
        OnExitState(state);
        state = next;
        OnEnterState(state);
    }

    private void OnEnterState(CardState s)
    {
        switch (s)
        {
            case CardState.Idle:
                if (visual != null)
                {
                    visual.localScale = cardOriginalScale;
                    visual.localPosition = cardOriginalVisualLocalPos;
                }
                cardCanvas.sortingOrder = cardOriginalSortingOrder;
                break;

            case CardState.Hover:
                if (visual != null) visual.localScale = hoverScale;
                cardCanvas.sortingOrder = hoverSortingOrder;
                break;

            case CardState.Dragging:
                isAnyDragging = true;
                if (visual != null) visual.localScale = cardOriginalScale;
                cardCanvas.sortingOrder = dragSortingOrder;
                break;

            case CardState.Returning:
                BeginReturn();
                break;

            case CardState.Playing:
                // 추후 BattleManager 협업 후 구현 (효과 적용 + 묘지 이동 + Destroy)
                break;
        }
    }

    private void OnExitState(CardState s)
    {
        switch (s)
        {
            case CardState.Dragging:
                isAnyDragging = false;
                break;
        }
    }

    // ─── Return Implementation ────────────────────────
    private void BeginReturn()
    {
        if (visual != null) returnStartPos = visual.localPosition;
        returnElapsed = 0f;
    }

    private void UpdateReturn()
    {
        if (visual == null)
        {
            SetState(CardState.Idle);
            return;
        }

        returnElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(returnElapsed / returnDuration);
        float eased = 1f - Mathf.Pow(1f - t, 2f); // EaseOutQuad. 추후 DOTween 도입 시 여기 변경
        visual.localPosition = Vector3.Lerp(returnStartPos, cardOriginalVisualLocalPos, eased);

        if (t >= 1f) FinishReturn();
    }

    private void FinishReturn()
    {
        if (visual != null) visual.localPosition = cardOriginalVisualLocalPos;
        SetState(CardState.Idle);
    }
}
