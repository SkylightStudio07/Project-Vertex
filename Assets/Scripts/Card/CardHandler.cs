// ============================================================
// filename   : CardHandler.cs
// 작성자     : xidsf - 최성제
// 작성일     : 2026-05-23
// description: 카드의 입력과 사용 흐름을 관리하는 상호작용 제어 클래스.
//              Hover/Drag/Targeting 상태 전환, 사용 가능 여부 확인,
//              드롭 대상 판정 및 BattleManager에 카드 사용을 요청한다.
//              이동, 정렬, 화살표 등 화면 표현은 CardInteractionView에 위임한다.
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster), typeof(CardView))]
[RequireComponent(typeof(CardInteractionView))]
public class CardHandler : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum CardState { Idle, Hover, Dragging, Targeting, Returning, Playing }

    [Header("Use Input")]
    [SerializeField] private float dropYThreshold = 400f;
    [SerializeField] private float targetCancelYThreshold = 330f;

    private static bool isAnyDragging;

    private static bool IsInteractable =>
        BattleManager.Instance != null &&
        BattleManager.Instance.State?.Phase == BattlePhase.PlayerTurn &&
        (MapUIController.Instance == null || !MapUIController.Instance.IsMapOpen);

    private CardView cardView;
    private CardInteractionView interactionView;
    private CardState state = CardState.Idle;
    private bool isPointerOverCard;
    private Vector2 targetingPointerPosition;

    private void Awake()
    {
        cardView = GetComponent<CardView>();
        interactionView = GetComponent<CardInteractionView>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOverCard = true;
        if (state != CardState.Idle || isAnyDragging || !IsInteractable) return;

        SetState(CardState.Hover);
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
                       TryGetEnemyTarget(eventData, out target);

            default:
                return false;
        }
    }

    private static bool TryGetEnemyTarget(PointerEventData eventData, out EnemyInstance target)
    {
        target = null;
        if (EventSystem.current == null) return false;

        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, hits);

        foreach (RaycastResult hit in hits)
        {
            EnemyView enemyView = hit.gameObject.GetComponentInParent<EnemyView>();
            if (enemyView == null || enemyView.Instance == null || enemyView.Instance.IsDead)
                continue;

            target = enemyView.Instance;
            return true;
        }

        return false;
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
                break;
        }
    }

    private void FinishReturn()
    {
        SetState(isPointerOverCard ? CardState.Hover : CardState.Idle);
    }
}
