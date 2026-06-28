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

    private readonly List<StartingDeckSummaryItem> spawnedSummaryItems = new();

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

        CollectExistingSummaryItems();

        List<CardStack> cardStacks = BuildCardStacks(startingDeckCards);
        EnsureSummaryItemCount(cardStacks.Count);

        for (int i = 0; i < spawnedSummaryItems.Count; i++)
        {
            StartingDeckSummaryItem item = spawnedSummaryItems[i];
            if (item == null)
                continue;

            bool shouldShow = i < cardStacks.Count;
            item.gameObject.SetActive(shouldShow);

            if (!shouldShow)
                continue;

            CardStack stack = cardStacks[i];
            item.Bind(stack.Card, stack.Count, "카드");
        }
    }

    private void EnsureSummaryItemCount(int count)
    {
        while (spawnedSummaryItems.Count < count)
        {
            StartingDeckSummaryItem item = Instantiate(cardSummaryPrefab, cardSummaryParent);
            SetLayerRecursively(item.gameObject, cardSummaryParent.gameObject.layer);
            item.gameObject.SetActive(true);
            spawnedSummaryItems.Add(item);
        }
    }

    private void CollectExistingSummaryItems()
    {
        if (spawnedSummaryItems.Count > 0)
            return;

        foreach (StartingDeckSummaryItem item in cardSummaryParent.GetComponentsInChildren<StartingDeckSummaryItem>(true))
        {
            if (item != null && item.transform.parent == cardSummaryParent)
                spawnedSummaryItems.Add(item);
        }
    }

    private static void SetLayerRecursively(GameObject target, int layer)
    {
        if (target == null)
            return;

        target.layer = layer;
        foreach (Transform child in target.transform)
            SetLayerRecursively(child.gameObject, layer);
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
