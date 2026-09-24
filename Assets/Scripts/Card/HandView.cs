// ============================================================
// filename   : HandView.cs
// description   : BattleManager.Hand 보고 그대로 CardView 인스턴스를 생성/갱신.
//             씬에 하나만 존재하는 손패 컨테이너 오브젝트에 부착.
// ============================================================

using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HandView : MonoBehaviour
{
    [SerializeField] private CardView cardPrefab;    // 카드 프리팹 (CardView 부착된 것)
    [SerializeField] private RectTransform cardContainer; // 카드들이 나열될 부모 Transform
    // 부채꼴 배치 담당. 비워두면 카드가 부모의 기본 배치(레이아웃 그룹 등)를 그대로 따름.
    [SerializeField] private HandFanLayout fanLayout;

    [Header("드로우 연출")]
    // 새로 뽑은 카드가 날아 나오는 곳(뽑을 카드 더미). 비우면 손패 왼쪽 아래 화면 밖에서 나온다.
    [SerializeField] private RectTransform drawPileAnchor;
    [SerializeField] private Vector2 fallbackDrawOffset = new(-900f, -300f);
    [SerializeField, Min(0f)] private float drawDuration = 0.3f;
    [SerializeField, Min(0f)] private float drawStagger = 0.07f;
    // 카드를 쓰거나 버려 손패가 줄면 남은 카드가 새 부채꼴 자리로 미끄러져 간다
    [SerializeField, Min(0f)] private float rearrangeDuration = 0.18f;

    [Header("소멸 연출")]
    [SerializeField] private Texture exhaustNoise;
    [SerializeField, Min(0f)] private float exhaustDuration = 0.5f;

    // 손패는 갱신 때마다 뷰를 전부 새로 만든다. 그래서 "어떤 카드가 새로 들어왔는지 / 어디 있었는지"를
    // 직전 갱신 기준으로 기억해 두고 연출에 쓴다. (런타임 카드는 전투마다 새 인스턴스라 참조로 구분 가능)
    private readonly Dictionary<CardData, (Vector2 pos, Quaternion rot)> _lastPoses = new();

    private void Start()
    {
        BattleManager.Instance.OnHandChanged += Refresh;
        // 손패 선택 모드 진입/종료 시 대상 아닌 카드의 흐림 표시를 갱신한다
        HandCardSelector.OnSelectionModeChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnHandChanged -= Refresh;
        HandCardSelector.OnSelectionModeChanged -= Refresh;
    }

    private void Refresh()
    {
        IReadOnlyList<CardData> hand = BattleManager.Instance.Hand;
        var inHand = new HashSet<CardData>(hand);
        var exhausted = BattleManager.Instance.State?.ExhaustPile;

        // 기존 카드 뷰 전부 제거. 단, 방금 소멸된 카드는 제자리에서 디졸브로 사라지게 남겨둔다.
        var previousPoses = new Dictionary<CardData, (Vector2, Quaternion)>(_lastPoses);
        _lastPoses.Clear();
        var toRemove = new List<Transform>();
        foreach (Transform child in cardContainer) toRemove.Add(child);
        foreach (Transform child in toRemove)
        {
            var old = child.GetComponent<CardView>();
            if (old != null && old.Data != null && !inHand.Contains(old.Data)
                && exhausted != null && exhausted.Contains(old.Data))
                PlayExhaust(old);
            else
                Destroy(child.gameObject);
        }

        // 손패의 각 CardData마다 CardView 생성. 늘 그렇듯 이런 식이 퍼포먼스에 썩 좋을지는 모르겠는데, 달리 대안이 없음.
        var interactionViews = new List<CardInteractionView>(hand.Count);
        var views = new List<CardView>(hand.Count);

        for (int i = 0; i < hand.Count; i++)
        {
            var view = Instantiate(cardPrefab, cardContainer);
            view.SetCard(hand[i]);
            views.Add(view);

            // 선택 모드에서 대상이 아닌 카드는 흐리게 — 어떤 카드를 고를 수 있는지 보이도록
            if (HandCardSelector.IsSelecting && HandCardSelector.Instance != null &&
                !HandCardSelector.Instance.IsSelectable(hand[i]))
            {
                var group = view.GetComponent<CanvasGroup>();
                if (group == null) group = view.gameObject.AddComponent<CanvasGroup>();
                group.alpha = 0.4f;
            }

            CardInteractionView interactionView = view.GetComponent<CardInteractionView>();
            if (interactionView != null)
            {
                interactionView.SetRestingSortingOrder(i);
                interactionView.SetTargetingAnchor(cardContainer);
                interactionViews.Add(interactionView);
            }
        }

        // 카드를 전부 생성한 뒤 한 번에 배치 — 개수(n)를 알아야 부채꼴 간격/각도를 계산할 수 있다.
        if (fanLayout != null)
            fanLayout.Arrange(interactionViews);

        AnimateIntoPlace(views, previousPoses);
    }

    // 부채꼴 자리(SetRestingPose로 이미 적용된 값)를 목표로, 새 카드는 드로우 더미에서, 기존 카드는 직전 자리에서 출발시킨다.
    private void AnimateIntoPlace(List<CardView> views, Dictionary<CardData, (Vector2 pos, Quaternion rot)> previous)
    {
        Vector2 drawOrigin = GetDrawOrigin();
        int drawnIndex = 0;

        foreach (var view in views)
        {
            var rt = (RectTransform)view.transform;
            Vector2 targetPos = rt.anchoredPosition;
            Quaternion targetRot = rt.localRotation;
            _lastPoses[view.Data] = (targetPos, targetRot);

            if (previous.TryGetValue(view.Data, out var from))
            {
                if ((from.pos - targetPos).sqrMagnitude < 1f) continue;
                rt.anchoredPosition = from.pos;
                rt.localRotation = from.rot;
                rt.DOAnchorPos(targetPos, rearrangeDuration).SetEase(Ease.OutCubic).SetLink(view.gameObject);
                rt.DOLocalRotateQuaternion(targetRot, rearrangeDuration).SetLink(view.gameObject);
                continue;
            }

            // 새로 뽑은 카드: 더미에서 작게 기울어진 채 날아와 제자리에서 펴진다 (전투 첫 드로우는 촤르륵)
            Vector3 scale = rt.localScale;
            float delay = drawnIndex++ * drawStagger;
            rt.anchoredPosition = drawOrigin;
            rt.localRotation = Quaternion.Euler(0f, 0f, 40f);
            rt.localScale = scale * 0.35f;
            DOTween.Sequence().SetLink(view.gameObject)
                   .Insert(delay, rt.DOAnchorPos(targetPos, drawDuration).SetEase(Ease.OutCubic))
                   .Insert(delay, rt.DOLocalRotateQuaternion(targetRot, drawDuration).SetEase(Ease.OutCubic))
                   .Insert(delay, rt.DOScale(scale, drawDuration).SetEase(Ease.OutBack));
        }
    }

    private Vector2 GetDrawOrigin()
    {
        if (drawPileAnchor == null) return fallbackDrawOffset;
        // 카드 루트는 앵커가 컨테이너 중앙(0.5,0.5)이라, 컨테이너 로컬 좌표에서 rect 중앙을 빼야 anchoredPosition이 된다
        Vector2 local = cardContainer.InverseTransformPoint(drawPileAnchor.TransformPoint(drawPileAnchor.rect.center));
        return local - cardContainer.rect.center;
    }

    // 소멸 카드는 손패 컨테이너 밖으로 빼서(다음 Refresh에서 지워지지 않도록) 제자리에서 디졸브한다.
    private void PlayExhaust(CardView view)
    {
        var go = view.gameObject;
        foreach (var handler in go.GetComponents<CardHandler>()) handler.enabled = false; // 더 이상 드래그·호버 안 받게
        go.transform.SetParent(cardContainer.parent != null ? cardContainer.parent : transform, true);

        UIDissolve.Out(go, exhaustNoise, exhaustDuration, UIDissolve.DefaultEdge)
                  .Join(go.transform.DOBlendableLocalMoveBy(new Vector3(0f, 40f, 0f), exhaustDuration))
                  .OnComplete(() => Destroy(go));
    }
}
