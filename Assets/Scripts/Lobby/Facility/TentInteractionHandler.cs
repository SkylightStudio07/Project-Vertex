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

    protected override void OnOpenInteraction(FacilityState facilityState)
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

    public void SetStartingDeckCards()
    {
        startingDeckCards.Clear();
        DeckManager.Instance.InitializeStartingDeck();
        startingDeckCards = DeckManager.Instance.PlayerDeck;
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

        Dictionary<CardData, int> cardStacks = BuildCardStacks(startingDeckCards);

        foreach (var stack in cardStacks)
        {
            StartingDeckSummaryItem item = Instantiate(cardSummaryPrefab, cardSummaryParent);
            item.Bind(stack.Key, stack.Value);
        }
    }

    private void ClearCardSummaryItems()
    {
        for (int i = cardSummaryParent.childCount - 1; i >= 0; i--)
        {
            Destroy(cardSummaryParent.GetChild(i).gameObject);
        }
    }

    private static Dictionary<CardData, int> BuildCardStacks(IReadOnlyList<CardData> cards)
    {
        Dictionary<CardData, int> cardStacks = new();

        if (cards == null)
            return cardStacks;

        foreach (CardData card in cards)
        {
            if (card == null)
                continue;

            if (cardStacks.TryGetValue(card, out int index))
            {
                cardStacks[card] = index + 1;
                continue;
            }
            else
            {
                cardStacks.Add(card, 1);
            }
        }

        return cardStacks;
    }
}
