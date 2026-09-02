using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 손패에서 카드를 직접 클릭해 고르게 하는 선택 모드.
// 버리기·소멸·강화 등 "손패의 카드 한 장을 지목해야 하는" 효과가 공용으로 사용한다.
// 카드 효과가 코루틴으로 입력을 기다리는 동안, 카드 사용·턴 종료는 차단된다
// (CardHandler / BattleManager에서 IsSelecting 확인).
public class HandCardSelector : MonoBehaviour
{
    public static HandCardSelector Instance { get; private set; }

    [SerializeField] private GameObject promptPanel;  // 손패 클릭을 막지 않도록 Raycast Target 전부 해제할 것
    [SerializeField] private TextMeshProUGUI promptText;

    public static bool IsSelecting { get; private set; }

    // 선택 모드 진입/종료 시 발화 — HandView가 흐림 처리를 갱신하는 데 사용
    public static event Action OnSelectionModeChanged;

    private CardData picked;
    private Predicate<CardData> selectableFilter;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (promptPanel != null) promptPanel.SetActive(false);
    }

    // 손패에서 한 장을 고를 때까지 대기한다. 취소는 없다 —
    // 카드 효과 실행 도중이라 중간에 빠져나가면 효과가 어중간하게 끝나기 때문.
    // canSelect : 고를 수 있는 카드 판별식 (강화 불가 카드 제외 등). null이면 전부 허용
    // exclude   : 이미 선택된 카드. 고른 카드가 손패에 남는 효과(강화 등)에서 중복 선택을 막는다
    //             (버리기처럼 손패에서 즉시 빠지는 효과는 넘길 필요 없음)
    // 고를 수 있는 카드가 없으면 onSelected에 null을 넘기고 즉시 끝난다 — 무한 대기 방지.
    public IEnumerator SelectOne(
        string prompt,
        Action<CardData> onSelected,
        Predicate<CardData> canSelect = null,
        IReadOnlyList<CardData> exclude = null)
    {
        if (BattleManager.Instance == null)
        {
            onSelected?.Invoke(null);
            yield break;
        }

        selectableFilter = card =>
            card != null &&
            (canSelect == null || canSelect(card)) &&
            (exclude == null || !ContainsCard(exclude, card));

        if (!HasSelectableCard())
        {
            selectableFilter = null;
            onSelected?.Invoke(null);
            yield break;
        }

        picked = null;
        IsSelecting = true;

        if (promptPanel != null) promptPanel.SetActive(true);
        if (promptText != null) promptText.text = prompt;
        OnSelectionModeChanged?.Invoke(); // 대상 아닌 카드 흐리게

        yield return new WaitUntil(() => picked != null);

        IsSelecting = false;
        selectableFilter = null;
        if (promptPanel != null) promptPanel.SetActive(false);
        OnSelectionModeChanged?.Invoke(); // 흐림 해제

        var result = picked;
        picked = null;
        onSelected?.Invoke(result);
    }

    // 이 카드가 지금 고를 수 있는 대상인지. CardHandler의 클릭 허용, HandView의 흐림 처리에 사용.
    public bool IsSelectable(CardData card)
    {
        if (!IsSelecting || card == null) return false;
        return selectableFilter == null || selectableFilter(card);
    }

    // CardHandler가 선택 모드에서 카드를 클릭했을 때 호출
    public void NotifyCardClicked(CardData card)
    {
        if (!IsSelectable(card)) return;
        picked = card;
    }

    // IReadOnlyList에는 Contains가 없어서 직접 순회한다.
    // (List.Contains를 쓰려면 파라미터 타입을 List로 좁혀야 하고, LINQ를 쓰면 불필요한 의존이 생긴다)
    private static bool ContainsCard(IReadOnlyList<CardData> cards, CardData target)
    {
        for (int i = 0; i < cards.Count; i++)
            if (ReferenceEquals(cards[i], target)) return true;
        return false;
    }

    private bool HasSelectableCard()
    {
        foreach (var card in BattleManager.Instance.Hand)
            if (selectableFilter(card)) return true;
        return false;
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        Instance = null;
        IsSelecting = false; // 씬 전환 중 모드가 켜진 채 남지 않도록
    }
}
