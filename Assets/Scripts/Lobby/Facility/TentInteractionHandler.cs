using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TentInteractionHandler : FacilityInteractionHandler
{
    [SerializeField] private Button runStartButton;
    [SerializeField] private Transform cardSummaryParent;
    [SerializeField] private StartingDeckSummaryItem cardSummaryPrefab;
    [SerializeField] private List<CardData> startingDeckCards = new();
    [SerializeField] private string runSceneName = "SampleScene";
    [SerializeField] private UnityEvent onRunStarted;

    public IReadOnlyList<CardData> StartingDeckCards => startingDeckCards;

    private void Awake()
    {
        BindButtonListeners();
        RefreshStartingDeckView();
    }

    private void OnDestroy()
    {
        runStartButton?.onClick.RemoveListener(StartRun);
    }

    protected override void OnOpenInteraction()
    {
        RefreshStartingDeckView();
    }

    public void StartRun()
    {
        onRunStarted?.Invoke();

        if (string.IsNullOrWhiteSpace(runSceneName))
            return;

        if (!Application.CanStreamedLevelBeLoaded(runSceneName))
        {
            Logger.LogWarning(this, $"Run scene '{runSceneName}' is not included in Build Settings.");
            return;
        }

        SceneManager.LoadScene(runSceneName);
    }

    public void SetStartingDeckCards(List<CardData> cards)
    {
        startingDeckCards = cards;
        RefreshStartingDeckView();
    }

    [ContextMenu("Debug/Refresh Starting Deck View")]
    private void DebugRefreshStartingDeckView()
    {
        RefreshStartingDeckView();
    }

    private void BindButtonListeners()
    {
        runStartButton?.onClick.RemoveListener(StartRun);
        runStartButton?.onClick.AddListener(StartRun);
    }

    private void RefreshStartingDeckView()
    {
        if (cardSummaryParent == null || cardSummaryPrefab == null)
            return;

        ClearCardSummaryItems();

        List<CardStack> cardStacks = BuildCardStacks(startingDeckCards);

        foreach (CardStack stack in cardStacks)
        {
            StartingDeckSummaryItem item = Instantiate(cardSummaryPrefab, cardSummaryParent);
            item.Bind(stack.Card, stack.Count, "카드");
        }
    }

    private void ClearCardSummaryItems()
    {
        for (int i = cardSummaryParent.childCount - 1; i >= 0; i--)
        {
            Destroy(cardSummaryParent.GetChild(i).gameObject);
        }
    }

    private static List<CardStack> BuildCardStacks(IReadOnlyList<CardData> cards)
    {
        List<CardStack> cardStacks = new();
        Dictionary<CardData, int> indexByCard = new();

        if (cards == null)
            return cardStacks;

        foreach (CardData card in cards)
        {
            if (card == null)
                continue;

            if (indexByCard.TryGetValue(card, out int index))
            {
                CardStack stack = cardStacks[index];
                stack.Count++;
                cardStacks[index] = stack;
                continue;
            }

            indexByCard.Add(card, cardStacks.Count);
            cardStacks.Add(new CardStack(card, 1));
        }

        return cardStacks;
    }

    private struct CardStack
    {
        public CardStack(CardData card, int count)
        {
            Card = card;
            Count = count;
        }

        public CardData Card { get; }
        public int Count { get; set; }
    }
}
