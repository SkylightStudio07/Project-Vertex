using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 카드 목록을 띄우는 범용 뷰.
// 덱 확인 / 뽑을 카드 더미 / 버린 카드 더미 / 상점 카드 제거 / 카드 강화 등등..에서 범용적으로 사용 가능하게끔
// 표시할 카드 목록을 주입받기만 하므로, 어떤 카드를 보여줄지는 호출부가 결정한다.
public class CardListView : MonoBehaviour
{
    public static CardListView Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private CardListEntry cardPrefab;
    [SerializeField] private Transform cardContainer; 
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button closeButton;

    // 선택 모드에서 카드를 고르면 호출. 보기 전용이면 null.
    private Action<CardData> onCardSelected;
    // 목록에 띄우되 선택은 막을 카드 판별식 (예: 제거 불가 카드). null이면 전부 선택 가능.
    // 근데 애초에 선택이 안되는 카드는 애초에 리스트에 안띄우는 방식이지 않나 싶긴한데 일단 구현함
    private Predicate<CardData> selectableFilter;

    private List<CardListEntry> cardEntries = new();

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (panel != null) panel.SetActive(false);
    }

    // 보기 전용 — 덱 확인, 뽑을/버린 카드 더미 확인
    public void OpenAsViewer(string title, IReadOnlyList<CardData> cards)
    { 
        Open(title, cards, null, null, canClose: true); 
    }
    // 선택 모드 — 카드 제거, 강화 등
    // 카드 선택 후 onCardSelected 호출
    // canClose가 false면 선택 후 반드시 onCardSelected 호출해야 닫힘. (취소 버튼 없음)
    public void OpenAsSelector(string title, IReadOnlyList<CardData> cards, Action<CardData> onCardSelected, Predicate<CardData> selectableFilter = null, bool canClose = true)
    {
        Open(title, cards, onCardSelected, selectableFilter, canClose);
    }

    private void Refresh(IReadOnlyList<CardData> cards)
    {
        ClearEntries();
        if (cards == null) return;

        bool selectMode = onCardSelected != null;

        foreach (var card in cards)
        {
            if (card == null) continue;

            var entry = Instantiate(cardPrefab, cardContainer);
            entry.GetComponent<CardView>().SetCard(card);

            // 보기 전용이면 좌클릭만 막는다 — 호버 확대·우클릭 상세는 그대로 동작
            bool selectable = selectMode && (selectableFilter == null || selectableFilter(card));
            entry.SetSelectable(selectable);

            if (selectMode) entry.OnClicked += HandleCardClicked;
            cardEntries.Add(entry);
        }
    }
    private void ClearEntries()
    {
        foreach(var entry in cardEntries)
        {
            if (entry != null)
            {
                entry.OnClicked -= HandleCardClicked;
                Destroy(entry.gameObject);
            }
        }
        cardEntries.Clear();
    }
    private void HandleCardClicked(CardData card)
    {
        onCardSelected?.Invoke(card);
        Close();
    }

    private void Open(string title, IReadOnlyList<CardData> cards, Action<CardData> onCardSelected, Predicate<CardData> selectableFilter, bool canClose)
    {
        if(panel == null || cardPrefab == null || cardContainer == null)
        {
            return;
        }

        this.onCardSelected = onCardSelected;
        this.selectableFilter = selectableFilter;

        if (titleText != null) titleText.text = title;
        if (closeButton != null) closeButton.gameObject.SetActive(canClose);

        Refresh(cards);
        if (panel != null) panel.SetActive(true);
    }
    public void Close()
    {
        ClearEntries();
        onCardSelected = null;
        selectableFilter = null;
        if(panel != null) panel.SetActive(false);
    }
}
