// ============================================================
// filename   : HandView.cs
// description   : BattleManager.Hand 보고 그대로 CardView 인스턴스를 생성/갱신.
//             씬에 하나만 존재하는 손패 컨테이너 오브젝트에 부착.
// ============================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HandView : MonoBehaviour
{
    [FormerlySerializedAs("cardPrefab")]
    [SerializeField] private CardView _cardPrefab; // 카드 UI를 새로 만들 때 사용할 프리팹.
    [FormerlySerializedAs("cardContainer")]
    [SerializeField] private RectTransform _cardContainer; // 카드 UI들이 배치될 부모 RectTransform.
    // 부채꼴 배치 담당. 비워두면 카드가 부모의 기본 배치(레이아웃 그룹 등)를 그대로 따름.
    [FormerlySerializedAs("fanLayout")]
    [SerializeField] private HandFanLayout _fanLayout; // 손패 카드들의 최종 위치와 회전을 계산하는 레이아웃.

    [Header("Draw In Animation")]
    [FormerlySerializedAs("playDrawInAnimation")]
    [SerializeField] private bool _playDrawInAnimation = true; // 새 카드 진입 애니메이션 사용 여부.
    [FormerlySerializedAs("drawStartPoint")]
    [SerializeField] private RectTransform _drawStartPoint; // 새 카드들이 출발 대기할 UI 기준점.
    [FormerlySerializedAs("drawInDuration")]
    [SerializeField] private float _drawInDuration = 0.25f; // 카드 한 장이 손패 위치까지 이동하는 시간.
    [FormerlySerializedAs("drawInStaggerDelay")]
    [SerializeField] private float _drawInStaggerDelay = 0.04f; // 다음 카드 이동을 시작하기 전 대기 시간.
    [FormerlySerializedAs("drawInSortingOrderBase")]
    [SerializeField] private int _drawInSortingOrderBase = 200; // 드로우 중 카드가 손패 카드보다 앞에 보이도록 하는 기준 순서.

    [Header("Discard Out Animation")]
    [SerializeField] private bool _playDiscardOutAnimation = true; // 턴 종료 시 손패 퇴장 애니메이션 사용 여부.
    [SerializeField] private RectTransform _discardEndPoint; // 턴 종료 카드들이 도착할 UI 기준점.
    [SerializeField] private float _discardOutDuration = 0.25f; // 카드 한 장이 EndPoint까지 이동하는 시간.
    [SerializeField] private float _discardOutStaggerDelay = 0.04f; // 다음 카드 퇴장을 시작하기 전 대기 시간.
    [SerializeField] private int _discardOutSortingOrderBase = 200; // 퇴장 중 카드가 손패 카드보다 앞에 보이도록 하는 기준 순서.

    private readonly HashSet<CardData> _currentHandCards = new(); // 현재 BattleManager.Hand에 들어 있는 카드 참조 집합.
    private readonly List<CardView> _cardViews = new(); // 현재 화면에서 생존 중인 카드 UI 목록.
    private readonly List<CardInteractionView> _layoutInteractionViews = new(); // 현재 손패 배치에 사용할 카드 상호작용 뷰 목록.
    private readonly List<CardInteractionView> _newInteractionViews = new(); // 이번 Refresh에서 새로 들어온 카드 뷰 재사용 목록.
    private readonly List<CardInteractionView> _discardInteractionViews = new(); // 턴 종료 시 퇴장할 카드 뷰 재사용 목록.
    private readonly List<CardView> _discardCardViews = new(); // 턴 종료 시 추적에서 제거할 카드 UI 재사용 목록.
    private readonly Dictionary<CardData, CardView> _cardViewByCard = new(); // CardData 참조로 현재 CardView를 찾기 위한 재사용 맵.
    private readonly Dictionary<CardData, CardInteractionView> _interactionViewByCard = new(); // CardData 참조로 현재 CardInteractionView를 찾기 위한 재사용 맵.
    private readonly HashSet<CardData> _discardPileCards = new(); // 현재 DiscardPile에 들어 있는 카드 참조 집합.
    private WaitForSeconds _drawInStaggerWait; // 카드별 순차 지연에 재사용할 대기 객체.
    private WaitForSeconds _discardOutStaggerWait; // 카드별 퇴장 지연에 재사용할 대기 객체.
    private Coroutine _drawInRoutine; // 현재 실행 중인 손패 진입 애니메이션 코루틴.
    private Coroutine _discardOutRoutine; // 현재 실행 중인 손패 퇴장 애니메이션 코루틴.

    // 순차 드로우 애니메이션에서 반복 사용할 대기 객체를 준비한다.
    private void Awake()
    {
        _drawInStaggerWait = new WaitForSeconds(_drawInStaggerDelay);
        _discardOutStaggerWait = new WaitForSeconds(_discardOutStaggerDelay);
    }

    private void Start()
    {
        BattleManager.Instance.OnHandChanged += Refresh;
        BattleManager.Instance.OnHandExitAnimationRequested += PlayHandExitAnimation;
        // 손패 선택 모드 진입/종료 시 대상 아닌 카드의 흐림 표시를 갱신한다
        HandCardSelector.OnSelectionModeChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnHandChanged -= Refresh;
            BattleManager.Instance.OnHandExitAnimationRequested -= PlayHandExitAnimation;
        }
        HandCardSelector.OnSelectionModeChanged -= Refresh;

        if (_drawInRoutine != null)
            StopCoroutine(_drawInRoutine);
        if (_discardOutRoutine != null)
            StopCoroutine(_discardOutRoutine);
    }

    // BattleManager의 손패 데이터를 기준으로 카드 UI를 다시 만들고 배치한다.
    private void Refresh()
    {
        if (_drawInRoutine != null)
        {
            StopCoroutine(_drawInRoutine);
            _drawInRoutine = null;
        }
        if (_discardOutRoutine != null)
        {
            StopCoroutine(_discardOutRoutine);
            _discardOutRoutine = null;
        }

        IReadOnlyList<CardData> hand = BattleManager.Instance.Hand;
        CacheCurrentHandCards(hand);
        CacheDiscardPileCards();
        RemoveMissingCardViews();
        _layoutInteractionViews.Clear();
        _newInteractionViews.Clear();

        for (int i = 0; i < hand.Count; i++)
        {
            CardData card = hand[i]; // 현재 손패 순서에서 표시할 카드 데이터.
            CardView view = GetOrCreateCardView(card); // 재사용하거나 새로 생성한 카드 UI.
            if (view == null) continue;

            view.SetCard(card);
            view.transform.SetSiblingIndex(i);
            ApplySelectionAlpha(view, card);

            if (!_interactionViewByCard.TryGetValue(card, out CardInteractionView interactionView)) continue;
            interactionView.SetRestingSortingOrder(i);
            interactionView.SetTargetingAnchor(_cardContainer);
            _layoutInteractionViews.Add(interactionView);
        }

        // 카드를 전부 생성한 뒤 한 번에 배치 — 개수(n)를 알아야 부채꼴 간격/각도를 계산할 수 있다.
        if (_fanLayout != null)
            _fanLayout.Arrange(_layoutInteractionViews);

        if (_playDrawInAnimation && _drawStartPoint != null && _newInteractionViews.Count > 0)
        {
            _drawInRoutine = StartCoroutine(PlayDrawInRoutine(_newInteractionViews));
        }
    }

    // 현재 손패에 없는 카드 UI만 퇴장 애니메이션 후 제거한다.
    private void RemoveMissingCardViews()
    {
        for (int i = _cardViews.Count - 1; i >= 0; i--)
        {
            CardView view = _cardViews[i]; // 현재 생존 카드 UI 목록에서 검사할 항목.
            if (view == null || view.Data == null)
            {
                _cardViews.RemoveAt(i);
                continue;
            }

            CardData card = view.Data; // 현재 카드 UI가 표시 중인 카드 데이터.
            if (_currentHandCards.Contains(card)) continue;

            RemoveCardViewTracking(card, view);
            PlayOrDestroyLeavingCardView(view, _discardOutSortingOrderBase);
        }
    }

    // 현재 손패에 들어 있는 카드 참조를 재사용 HashSet에 기록한다.
    private void CacheCurrentHandCards(IReadOnlyList<CardData> hand)
    {
        _currentHandCards.Clear();
        for (int i = 0; i < hand.Count; i++)
            if (hand[i] != null)
                _currentHandCards.Add(hand[i]);
    }

    // 현재 DiscardPile에 들어 있는 카드 참조를 재사용 HashSet에 기록한다.
    private void CacheDiscardPileCards()
    {
        _discardPileCards.Clear();
        IReadOnlyList<CardData> discardPile = BattleManager.Instance.State?.DiscardPile; // 버린 카드 더미의 현재 카드 목록.
        if (discardPile == null) return;

        for (int i = 0; i < discardPile.Count; i++)
            if (discardPile[i] != null)
                _discardPileCards.Add(discardPile[i]);
    }

    // 카드 데이터에 해당하는 기존 UI를 찾거나 없으면 새로 만든다.
    private CardView GetOrCreateCardView(CardData card)
    {
        if (card == null || _cardPrefab == null || _cardContainer == null) return null;
        if (_cardViewByCard.TryGetValue(card, out CardView existingView) && existingView != null)
            return existingView;

        CardView view = Instantiate(_cardPrefab, _cardContainer); // 새 손패 카드 UI 인스턴스.
        CardInteractionView interactionView = view.GetComponent<CardInteractionView>(); // 새 카드 UI의 상호작용 표현 컴포넌트.

        _cardViews.Add(view);
        _cardViewByCard[card] = view;
        if (interactionView != null)
        {
            _interactionViewByCard[card] = interactionView;
            _newInteractionViews.Add(interactionView);
        }

        return view;
    }

    // 선택 모드 상태에 따라 카드 UI의 투명도를 갱신한다.
    private void ApplySelectionAlpha(CardView view, CardData card)
    {
        bool shouldDim = HandCardSelector.IsSelecting && HandCardSelector.Instance != null &&
                         !HandCardSelector.Instance.IsSelectable(card); // 선택 불가능한 카드라 흐리게 표시해야 하는지 저장한다.
        CanvasGroup group = view.GetComponent<CanvasGroup>(); // 카드 투명도를 제어할 CanvasGroup 컴포넌트.

        if (shouldDim)
        {
            if (group == null) group = view.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0.4f;
        }
        else if (group != null)
        {
            group.alpha = 1f;
        }
    }

    // 현재 손패에서 빠진 카드 UI 추적 정보를 제거한다.
    private void RemoveCardViewTracking(CardData card, CardView view)
    {
        _cardViews.Remove(view);
        if (card == null) return;

        _cardViewByCard.Remove(card);
        _interactionViewByCard.Remove(card);
    }

    // 카드 UI를 EndPoint까지 이동시킨 뒤 제거하거나, 애니메이션 없이 즉시 제거한다.
    private void PlayOrDestroyLeavingCardView(CardView view, int sortingOrder)
    {
        if (view == null) return;
        if (!_discardPileCards.Contains(view.Data) || !_playDiscardOutAnimation || _discardEndPoint == null ||
            !view.TryGetComponent(out CardInteractionView interactionView))
        {
            Destroy(view.gameObject);
            return;
        }

        Vector2 endPosition = GetDiscardEndPosition(); // 퇴장 카드가 도착할 cardContainer 기준 좌표.
        interactionView.PlayDiscardOut(endPosition, _discardOutDuration, sortingOrder, DestroyDiscardedCardView);
    }

    // 턴 종료 시 현재 손패에서 빠져나간 카드 UI를 EndPoint까지 이동시킨다.
    private float PlayHandExitAnimation()
    {
        if (!_playDiscardOutAnimation || _discardEndPoint == null || BattleManager.Instance == null)
            return 0f;

        if (_drawInRoutine != null)
        {
            StopCoroutine(_drawInRoutine);
            _drawInRoutine = null;
        }

        if (_discardOutRoutine != null)
        {
            StopCoroutine(_discardOutRoutine);
            _discardOutRoutine = null;
        }

        CacheCurrentHandCards(BattleManager.Instance.Hand);
        CacheDiscardPileCards();
        _discardInteractionViews.Clear();
        _discardCardViews.Clear();

        for (int i = 0; i < _cardViews.Count; i++)
        {
            CardView view = _cardViews[i]; // 턴 종료 직전 화면에 살아 있는 카드 UI.
            if (view == null || view.Data == null)
                continue;

            CardData card = view.Data; // 현재 카드 UI가 표시 중인 카드 데이터.
            if (_currentHandCards.Contains(card)) continue;

            if (_discardPileCards.Contains(card) &&
                _interactionViewByCard.TryGetValue(card, out CardInteractionView interactionView))
            {
                _discardInteractionViews.Add(interactionView);
            }
            else
            {
                Destroy(view.gameObject);
            }
            _discardCardViews.Add(view);
        }

        if (_discardCardViews.Count == 0)
            return 0f;

        for (int i = 0; i < _discardCardViews.Count; i++)
        {
            CardView view = _discardCardViews[i]; // 퇴장 대상으로 확정되어 추적에서 제거할 카드 UI.
            RemoveCardViewTracking(view.Data, view);
        }

        if (_discardInteractionViews.Count == 0)
            return 0f;

        _discardOutRoutine = StartCoroutine(PlayDiscardOutRoutine(_discardInteractionViews));
        return GetSequentialAnimationDuration(_discardInteractionViews.Count, _discardOutDuration, _discardOutStaggerDelay);
    }

    // 퇴장 대상 카드들을 현재 위치에서 EndPoint까지 순서대로 이동시킨다.
    private IEnumerator PlayDiscardOutRoutine(IReadOnlyList<CardInteractionView> cards)
    {
        Vector2 endPosition = GetDiscardEndPosition(); // 퇴장 카드들이 도착할 cardContainer 기준 좌표.

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
            {
                int sortingOrder = _discardOutSortingOrderBase + cards.Count - i; // 먼저 나가는 카드가 앞에 보이도록 계산한 임시 순서.
                cards[i].PlayDiscardOut(endPosition, _discardOutDuration, sortingOrder, DestroyDiscardedCardView);
            }

            if (_discardOutStaggerDelay > 0f)
                yield return _discardOutStaggerWait;
        }

        _discardOutRoutine = null;
    }

    // 퇴장 애니메이션이 끝난 카드 UI만 제거한다.
    private void DestroyDiscardedCardView(CardInteractionView interactionView)
    {
        if (interactionView == null) return;
        Destroy(interactionView.gameObject);
    }

    // 카드 수와 순차 지연을 기준으로 전체 애니메이션 예상 시간을 계산한다.
    private float GetSequentialAnimationDuration(int count, float duration, float staggerDelay)
    {
        if (count <= 0) return 0f;
        return Mathf.Max(0f, duration) + Mathf.Max(0f, staggerDelay) * (count - 1);
    }

    // 새 카드들을 시작 위치에 먼저 모아둔 뒤 순서대로 손패 위치까지 이동시킨다.
    private IEnumerator PlayDrawInRoutine(IReadOnlyList<CardInteractionView> cards)
    {
        Vector2 startPosition = GetDrawStartPosition(); // 진입 카드들이 대기할 cardContainer 기준 좌표.

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
            {
                int sortingOrder = _drawInSortingOrderBase + cards.Count - i; // 먼저 나오는 카드가 앞에 보이도록 계산한 임시 순서.
                cards[i].PrepareDrawIn(startPosition, sortingOrder);
            }
        }

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
                cards[i].PlayPreparedDrawIn(_drawInDuration);

            if (_drawInStaggerDelay > 0f)
                yield return _drawInStaggerWait;
        }

        _drawInRoutine = null;
    }

    // drawStartPoint의 화면 위치를 cardContainer의 anchoredPosition 좌표로 변환한다.
    private Vector2 GetDrawStartPosition()
    {
        Vector3 worldPosition = _drawStartPoint.TransformPoint(_drawStartPoint.rect.center); // drawStartPoint 중심의 월드 좌표.
        Vector2 localPosition = _cardContainer.InverseTransformPoint(worldPosition); // cardContainer 기준 로컬 좌표.
        return localPosition - _cardContainer.rect.center;
    }

    // discardEndPoint의 화면 위치를 cardContainer의 anchoredPosition 좌표로 변환한다.
    private Vector2 GetDiscardEndPosition()
    {
        Vector3 worldPosition = _discardEndPoint.TransformPoint(_discardEndPoint.rect.center); // discardEndPoint 중심의 월드 좌표.
        Vector2 localPosition = _cardContainer.InverseTransformPoint(worldPosition); // cardContainer 기준 로컬 좌표.
        return localPosition - _cardContainer.rect.center;
    }
}
