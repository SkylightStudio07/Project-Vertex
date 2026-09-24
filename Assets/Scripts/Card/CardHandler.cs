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
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster), typeof(CardView))]
[RequireComponent(typeof(CardInteractionView))]
public class CardHandler : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum CardState { Idle, Hover, Dragging, Targeting, Returning, Playing }

    [Header("Use Input")]
    [FormerlySerializedAs("dropYThreshold")]
    [SerializeField] private float _dropYThreshold = 400f; // 카드를 이 높이 이상으로 놓으면 사용 영역으로 판단하는 기준값.
    [FormerlySerializedAs("targetCancelYThreshold")]
    [SerializeField] private float _targetCancelYThreshold = 330f; // 타겟팅을 취소하고 드래그로 되돌아갈 화면 높이 기준값.

    private static bool _isAnyDragging; // 현재 어떤 카드든 드래그 중인지 공유하는 상태값.

    // 카드 호버/드래그 가능 조건: 전투 중 + 플레이어 턴 + 맵/이벤트 화면이 안 열려있을 때.
    // 다른 풀스크린 UI(보상, 상점 등)가 추가되면 같은 패턴으로 조건을 늘릴 것.
    private static bool IsInteractable =>
        BattleManager.Instance != null &&
        BattleManager.Instance.State?.Phase == BattlePhase.PlayerTurn &&
        !BattleManager.Instance.IsPlayerTurnEnding &&
        !HandCardSelector.IsSelecting &&   // 손패 선택 모드 중에는 카드 사용 불가
        (MapUIController.Instance == null || !MapUIController.Instance.IsMapOpen) &&
        (EventView.Instance == null || !EventView.Instance.IsEventOpen);

    private CardView _cardView; // 이 카드의 데이터 표시를 담당하는 CardView.
    private CardInteractionView _interactionView; // 이 카드의 화면 이동/정렬 연출을 담당하는 뷰.
    private CardState _state = CardState.Idle; // 현재 카드 입력 상태.
    private bool _isPointerOverCard; // 포인터가 현재 카드 위에 있는지 나타내는 상태값.
    private Vector2 _targetingPointerPosition; // 타겟팅 중 마지막으로 기록한 포인터 위치.
    // 타겟팅 중 포인터 아래의 적 — 설명문에 대상 측 보정(취약·버퍼)을 반영하기 위해 추적.
    private EnemyInstance _hoveredTarget;

    private void Awake()
    {
        _cardView = GetComponent<CardView>();
        _interactionView = GetComponent<CardInteractionView>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerOverCard = true;
        if (_interactionView.IsInteractionLocked) return;
        if (_state != CardState.Idle || _isAnyDragging) return;
        // 선택 모드에서는 드래그(사용)는 막되 호버 확대는 유지 — 어떤 카드를 고르는지 보여야 한다
        if (!IsInteractable && !HandCardSelector.IsSelecting) return;

        SetState(CardState.Hover);
    }

    // 선택 모드 전용 — 평소 카드 사용은 드래그로 하므로 클릭은 무시된다.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!HandCardSelector.IsSelecting) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (_cardView.Data == null) return;

        HandCardSelector.Instance.NotifyCardClicked(_cardView.Data);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOverCard = false;
        if (_state != CardState.Hover) return;

        SetState(CardState.Idle);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_interactionView.IsInteractionLocked) return;
        if (!IsInteractable) return;
        if (_state != CardState.Hover && _state != CardState.Idle) return;
        if (BattleManager.Instance == null ||
            _cardView.Data == null ||
            !BattleManager.Instance.IsCardPlayable(_cardView.Data))
            return;

        SetState(CardState.Dragging);
        _interactionView.MoveVisualCenterToPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_state == CardState.Targeting)
        {
            _targetingPointerPosition = eventData.position;
            _interactionView.UpdateTargetingPointer(eventData.position);

            // 포인터 아래 적이 바뀌었을 때만 설명문 갱신 — 대상 측 보정(취약·버퍼) 미리보기.
            // 적 위가 아니면 hovered가 null이 되어 공격자 측 보정만 반영된 표시로 돌아간다.
            EnemyTargeting.TryGetUnderPointer(eventData, out EnemyInstance hovered);
            if (hovered != _hoveredTarget)
            {
                _hoveredTarget = hovered;
                _cardView.RefreshDescription(hovered);
            }

            if (eventData.position.y <= _targetCancelYThreshold)
            {
                SetState(CardState.Dragging);
                _interactionView.MoveVisualCenterToPointer(eventData);
            }
            return;
        }

        if (_state != CardState.Dragging) return;
        if (_cardView.Data != null &&
            _cardView.Data.UseMode == CardData.CardUseMode.SelectEnemy &&
            eventData.position.y >= _dropYThreshold)
        {
            _targetingPointerPosition = eventData.position;
            SetState(CardState.Targeting);
            return;
        }

        _interactionView.MoveVisualCenterToPointer(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_state != CardState.Dragging && _state != CardState.Targeting) return;
        if (!TryDrop(eventData, out EnemyInstance target))
        {
            SetState(CardState.Returning);
            return;
        }

        bool played = BattleManager.Instance != null &&
                      BattleManager.Instance.TryPlayCard(_cardView.Data, target);
        SetState(played ? CardState.Playing : CardState.Returning);
    }

    private bool TryDrop(PointerEventData eventData, out EnemyInstance target)
    {
        target = null;
        if (_cardView.Data == null) return false;

        switch (_cardView.Data.UseMode)
        {
            case CardData.CardUseMode.DropToPlayArea:
                return eventData.position.y >= _dropYThreshold;

            case CardData.CardUseMode.SelectEnemy:
                return _state == CardState.Targeting &&
                       EnemyTargeting.TryGetUnderPointer(eventData, out target);

            default:
                return false;
        }
    }


    private void SetState(CardState next)
    {
        OnExitState(_state);
        _state = next;
        OnEnterState(_state);
    }

    private void OnEnterState(CardState state)
    {
        switch (state)
        {
            case CardState.Idle:
                _interactionView.EnterIdle();
                break;

            case CardState.Hover:
                _interactionView.EnterHover();
                break;

            case CardState.Dragging:
                _isAnyDragging = true;
                _interactionView.EnterDragging();
                break;

            case CardState.Targeting:
                _isAnyDragging = true;
                _interactionView.EnterTargeting(_targetingPointerPosition);
                break;

            case CardState.Returning:
                _interactionView.EnterReturning(FinishReturn);
                break;

            case CardState.Playing:
                _interactionView.EnterPlaying();
                break;
        }
    }

    private void OnExitState(CardState previous)
    {
        switch (previous)
        {
            case CardState.Dragging:
                _isAnyDragging = false;
                break;

            case CardState.Targeting:
                _isAnyDragging = false;
                _interactionView.ExitTargeting();
                // 타겟팅 종료 — 대상 측 보정이 반영됐던 설명문을 원래 표시로 되돌린다.
                if (_hoveredTarget != null)
                {
                    _hoveredTarget = null;
                    _cardView.RefreshDescription();
                }
                break;
        }
    }

    private void FinishReturn()
    {
        SetState(_isPointerOverCard ? CardState.Hover : CardState.Idle);
    }
}
