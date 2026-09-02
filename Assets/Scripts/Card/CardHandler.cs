// ============================================================
// filename   : CardHandler.cs
// 작성자     : xidsf - 최성제
// 작성일     : 2026-05-23
// description: 카드의 입력과 사용 흐름을 관리하는 상호작용 제어 클래스.
//              Hover/Drag/Targeting 상태 전환, 사용 가능 여부 확인,
//              드롭 대상 판정 및 BattleManager에 카드 사용을 요청한다.
//              이동, 정렬, 화살표 등 화면 표현은 CardInteractionView에 위임한다.
// ============================================================

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster), typeof(CardView))]
[RequireComponent(typeof(CardInteractionView))]
public class CardHandler : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum CardState { Idle, Hover, Dragging, Targeting, Returning, Playing }

    [Header("Use Input")]
    [SerializeField] private float dropYThreshold = 400f;
    [SerializeField] private float targetCancelYThreshold = 330f;

    private static bool isAnyDragging;

    // 카드 호버/드래그 가능 조건: 전투 중 + 플레이어 턴 + 맵/이벤트 화면이 안 열려있을 때.
    // 다른 풀스크린 UI(보상, 상점 등)가 추가되면 같은 패턴으로 조건을 늘릴 것.
    private static bool IsInteractable =>
        BattleManager.Instance != null &&
        BattleManager.Instance.State?.Phase == BattlePhase.PlayerTurn &&
        !HandCardSelector.IsSelecting &&   // 손패 선택 모드 중에는 카드 사용 불가
        (MapUIController.Instance == null || !MapUIController.Instance.IsMapOpen) &&
        (EventView.Instance == null || !EventView.Instance.IsEventOpen);

    private CardView cardView;
    private CardInteractionView interactionView;
    private CardState state = CardState.Idle;
    private bool isPointerOverCard;
    private Vector2 targetingPointerPosition;
    // 타겟팅 중 포인터 아래의 적 — 설명문에 대상 측 보정(취약·버퍼)을 반영하기 위해 추적.
    private EnemyInstance hoveredTarget;

    private void Awake()
    {
        cardView = GetComponent<CardView>();
        interactionView = GetComponent<CardInteractionView>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOverCard = true;
        if (state != CardState.Idle || isAnyDragging) return;
        // 선택 모드에서는 드래그(사용)는 막되 호버 확대는 유지 — 어떤 카드를 고르는지 보여야 한다
        if (!IsInteractable && !HandCardSelector.IsSelecting) return;

        SetState(CardState.Hover);
    }

    // 선택 모드 전용 — 평소 카드 사용은 드래그로 하므로 클릭은 무시된다.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!HandCardSelector.IsSelecting) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (cardView.Data == null) return;

        HandCardSelector.Instance.NotifyCardClicked(cardView.Data);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOverCard = false;
        if (state != CardState.Hover) return;

        SetState(CardState.Idle);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsInteractable) return;
        if (state != CardState.Hover && state != CardState.Idle) return;
        if (BattleManager.Instance == null ||
            cardView.Data == null ||
            !BattleManager.Instance.IsCardPlayable(cardView.Data))
            return;

        SetState(CardState.Dragging);
        interactionView.MoveVisualCenterToPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (state == CardState.Targeting)
        {
            targetingPointerPosition = eventData.position;
            interactionView.UpdateTargetingPointer(eventData.position);

            // 포인터 아래 적이 바뀌었을 때만 설명문 갱신 — 대상 측 보정(취약·버퍼) 미리보기.
            // 적 위가 아니면 hovered가 null이 되어 공격자 측 보정만 반영된 표시로 돌아간다.
            EnemyTargeting.TryGetUnderPointer(eventData, out EnemyInstance hovered);
            if (hovered != hoveredTarget)
            {
                hoveredTarget = hovered;
                cardView.RefreshDescription(hovered);
            }

            if (eventData.position.y <= targetCancelYThreshold)
            {
                SetState(CardState.Dragging);
                interactionView.MoveVisualCenterToPointer(eventData);
            }
            return;
        }

        if (state != CardState.Dragging) return;
        if (cardView.Data != null &&
            cardView.Data.UseMode == CardData.CardUseMode.SelectEnemy &&
            eventData.position.y >= dropYThreshold)
        {
            targetingPointerPosition = eventData.position;
            SetState(CardState.Targeting);
            return;
        }

        interactionView.MoveVisualCenterToPointer(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (state != CardState.Dragging && state != CardState.Targeting) return;
        if (!TryDrop(eventData, out EnemyInstance target))
        {
            SetState(CardState.Returning);
            return;
        }

        bool played = BattleManager.Instance != null &&
                      BattleManager.Instance.TryPlayCard(cardView.Data, target);
        SetState(played ? CardState.Playing : CardState.Returning);
    }

    private bool TryDrop(PointerEventData eventData, out EnemyInstance target)
    {
        target = null;
        if (cardView.Data == null) return false;

        switch (cardView.Data.UseMode)
        {
            case CardData.CardUseMode.DropToPlayArea:
                return eventData.position.y >= dropYThreshold;

            case CardData.CardUseMode.SelectEnemy:
                return state == CardState.Targeting &&
                       EnemyTargeting.TryGetUnderPointer(eventData, out target);

            default:
                return false;
        }
    }


    private void SetState(CardState next)
    {
        OnExitState(state);
        state = next;
        OnEnterState(state);
    }

    private void OnEnterState(CardState state)
    {
        switch (state)
        {
            case CardState.Idle:
                interactionView.EnterIdle();
                break;

            case CardState.Hover:
                interactionView.EnterHover();
                break;

            case CardState.Dragging:
                isAnyDragging = true;
                interactionView.EnterDragging();
                break;

            case CardState.Targeting:
                isAnyDragging = true;
                interactionView.EnterTargeting(targetingPointerPosition);
                break;

            case CardState.Returning:
                interactionView.EnterReturning(FinishReturn);
                break;

            case CardState.Playing:
                interactionView.EnterPlaying();
                break;
        }
    }

    private void OnExitState(CardState previous)
    {
        switch (previous)
        {
            case CardState.Dragging:
                isAnyDragging = false;
                break;

            case CardState.Targeting:
                isAnyDragging = false;
                interactionView.ExitTargeting();
                // 타겟팅 종료 — 대상 측 보정이 반영됐던 설명문을 원래 표시로 되돌린다.
                if (hoveredTarget != null)
                {
                    hoveredTarget = null;
                    cardView.RefreshDescription();
                }
                break;
        }
    }

    private void FinishReturn()
    {
        SetState(isPointerOverCard ? CardState.Hover : CardState.Idle);
    }
}
