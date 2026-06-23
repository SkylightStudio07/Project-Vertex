using UnityEngine;
using UnityEngine.UI;

/// <summary>메인 화면 시설 버튼의 클릭과 상태 배지를 관리한다.</summary>
public class FacilityButtonPresenter : MonoBehaviour
{
    [SerializeField] private FacilityType facilityType;
    [SerializeField] private Button facilityButton;
    [SerializeField] private GameObject lockedIndicator;
    [SerializeField] private GameObject upgradedIndicator;

    private LobbyManager lobbyManager;
    private FacilityManager facilityManager;

    private void Awake()
    {
        if (facilityButton == null)
            facilityButton = GetComponent<Button>();
    }

    private void OnEnable()
    {
        facilityButton?.onClick.AddListener(HandleButtonClicked);
        BindManagers();
        Refresh();
    }

    private void Start()
    {
        BindManagers();
        Refresh();
    }

    private void OnDisable()
    {
        facilityButton?.onClick.RemoveListener(HandleButtonClicked);
        UnbindFacilityManager();
    }

    public void Refresh()
    {
        if (lobbyManager == null)
            BindManagers();

        lockedIndicator?.SetActive(lobbyManager == null || !lobbyManager.IsFacilityActive(facilityType));
        upgradedIndicator?.SetActive(lobbyManager != null && lobbyManager.IsFacilityUpgraded(facilityType));
    }

    private void HandleButtonClicked()
    {
        if (lobbyManager == null)
            BindManagers();

        lobbyManager?.TryBeginInteraction(facilityType);
    }

    private void BindManagers()
    {
        lobbyManager = LobbyManager.Instance;
        FacilityManager nextFacilityManager = lobbyManager != null ? lobbyManager.FacilityManager : null;
        if (facilityManager == nextFacilityManager)
            return;

        UnbindFacilityManager();
        facilityManager = nextFacilityManager;

        if (facilityManager != null)
        {
            facilityManager.OnFacilityChanged += HandleFacilityChanged;
            facilityManager.OnActiveChanged += HandleActiveChanged;
        }
    }

    private void UnbindFacilityManager()
    {
        if (facilityManager == null)
            return;

        facilityManager.OnFacilityChanged -= HandleFacilityChanged;
        facilityManager.OnActiveChanged -= HandleActiveChanged;
        facilityManager = null;
    }

    private void HandleFacilityChanged(FacilityType changedFacilityType)
    {
        if (changedFacilityType == facilityType)
            Refresh();
    }

    private void HandleActiveChanged(FacilityType changedFacilityType, bool active)
    {
        if (changedFacilityType == facilityType)
            Refresh();
    }
}
