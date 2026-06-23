using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TentInteractionHandler : FacilityInteractionHandler
{
    [SerializeField] private LobbyScreenRouter screenRouter;
    [SerializeField] private GameObject interactionView;
    [SerializeField] private Button runStartButton;
    [SerializeField] private Button deckCheckButton;
    [SerializeField] private GameObject deckPreviewView;
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private TMP_Text deckPreviewText;
    [SerializeField] private StartingWeaponData selectedStartingWeapon;
    [SerializeField] private string runSceneName = "SampleScene";
    [SerializeField] private UnityEvent onRunStarted;

    private void Awake()
    {
        runStartButton?.onClick.AddListener(StartRun);
        deckCheckButton?.onClick.AddListener(ToggleDeckPreview);
    }

    private void OnDestroy()
    {
        runStartButton?.onClick.RemoveListener(StartRun);
        deckCheckButton?.onClick.RemoveListener(ToggleDeckPreview);
    }

    protected override void OnOpenInteraction()
    {
        RefreshDeckPreview();

        if (screenRouter != null)
            screenRouter.ShowFacilityView(interactionView);
        else
            interactionView?.SetActive(true);
    }

    protected override void OnCloseInteraction()
    {
        interactionView?.SetActive(false);
        screenRouter?.ShowMainView();
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

    public void ToggleDeckPreview()
    {
        if (deckPreviewView == null)
            return;

        deckPreviewView.SetActive(!deckPreviewView.activeSelf);
        RefreshDeckPreview();
    }

    public void SetStartingWeapon(StartingWeaponData startingWeapon)
    {
        selectedStartingWeapon = startingWeapon;
        RefreshDeckPreview();
    }

    private void RefreshDeckPreview()
    {
        if (selectedStartingWeapon == null)
        {
            if (weaponNameText != null)
                weaponNameText.text = "선택된 시작 무기 없음";
            if (deckPreviewText != null)
                deckPreviewText.text = string.Empty;
            return;
        }

        if (weaponNameText != null)
            weaponNameText.text = selectedStartingWeapon.WeaponName;

        if (deckPreviewText != null)
        {
            string attackName = GetCardName(selectedStartingWeapon.AttackCard, "타격");
            string defenseName = GetCardName(selectedStartingWeapon.DefenseCard, "수비");
            string reloadName = GetCardName(selectedStartingWeapon.ReloadCard, "재장전");
            deckPreviewText.text =
                $"{attackName} × {selectedStartingWeapon.AttackCardCount}\n" +
                $"{defenseName} × {selectedStartingWeapon.DefenseCardCount}\n" +
                $"{reloadName} × {selectedStartingWeapon.ReloadCardCount}\n" +
                $"총 {selectedStartingWeapon.TotalCardCount}장";
        }
    }

    private static string GetCardName(CardData card, string fallbackName)
    {
        return card != null ? card.CardName : fallbackName;
    }
}
