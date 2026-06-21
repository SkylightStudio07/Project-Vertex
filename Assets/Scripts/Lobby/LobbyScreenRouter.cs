using UnityEngine;

/// <summary>
/// Switches between the lobby main menu and a facility view.
/// Facility handlers own interaction state; this class only owns presentation.
/// </summary>
public class LobbyScreenRouter : MonoBehaviour
{
    [SerializeField] private GameObject mainView;

    private GameObject currentView;

    private void Start()
    {
        ShowMainView();
    }

    public void ShowFacilityView(GameObject facilityView)
    {
        if (facilityView == null)
            return;

        if (currentView != null && currentView != facilityView)
            currentView.SetActive(false);

        currentView = facilityView;
        mainView?.SetActive(false);
        currentView.SetActive(true);
    }

    public void ShowMainView()
    {
        if (currentView != null)
            currentView.SetActive(false);

        currentView = null;
        mainView?.SetActive(true);
    }
}
