using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TentInteractionHandler : FacilityInteractionHandler
{
    [SerializeField] private LobbyScreenRouter screenRouter;
    [SerializeField] private GameObject interactionView;
    [SerializeField] private Button runStartButton;
    [SerializeField] private string runSceneName = "SampleScene";
    [SerializeField] private UnityEvent onRunStarted;

    private void Awake()
    {
        runStartButton?.onClick.AddListener(StartRun);
    }

    private void OnDestroy()
    {
        runStartButton?.onClick.RemoveListener(StartRun);
    }

    protected override void OnOpenInteraction()
    {
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
}
