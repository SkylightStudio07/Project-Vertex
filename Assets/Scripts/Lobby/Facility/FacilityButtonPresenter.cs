using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>메인 화면 시설 버튼의 클릭과 상태 표시를 관리한다.</summary>
public class FacilityButtonPresenter : MonoBehaviour
{
    [SerializeField] private FacilityType facilityType;
    [SerializeField] private Button interactionButton;
    [SerializeField] private TMP_Text upgradeStateText;

    private LobbyManager lobbyManager;
    private FacilityManager facilityManager;

    private void Awake()
    {
        if (interactionButton == null)
            interactionButton = GetComponent<Button>();
    }

    private void OnEnable()
    {
        interactionButton?.onClick.AddListener(HandleButtonClicked);
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
        interactionButton?.onClick.RemoveListener(HandleButtonClicked);
        UnbindFacilityManager();
    }

    public void Refresh()
    {
        if (lobbyManager == null)
            BindManagers();

        FacilityState facilityState = facilityManager != null
            ? facilityManager.GetFacilityState(facilityType)
            : new FacilityState(facilityType, null, false);

        Apply(facilityState);
    }

    private void Apply(FacilityState facilityState)
    {
        if (interactionButton != null)
            interactionButton.interactable = facilityState.IsRegistered;

        if (upgradeStateText != null)
            upgradeStateText.text = facilityState.IsUpgraded ? "Lv.2" : "Lv.1";
    }

    private void HandleButtonClicked()
    {
        if (lobbyManager == null)
            BindManagers();

        facilityManager?.TryBeginInteraction(facilityType);
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
            facilityManager.OnFacilityStateChanged += HandleFacilityStateChanged;
        }
    }

    private void UnbindFacilityManager()
    {
        if (facilityManager == null)
            return;

        facilityManager.OnFacilityStateChanged -= HandleFacilityStateChanged;
        facilityManager = null;
    }

    private void HandleFacilityStateChanged(FacilityState facilityState)
    {
        if (facilityState.FacilityType == facilityType)
            Apply(facilityState);
    }
}
