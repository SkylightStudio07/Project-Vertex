using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class FacilityInteractionHandler : MonoBehaviour
{
    [Header("공통 시설 UI")]
    [SerializeField] private FacilityType facilityType;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject facilityRoot;

    [Header("시설 상태별 표시 대상")]
    [SerializeField] private List<GameObject> activeWhenLocked = new();
    [SerializeField] private List<GameObject> activeWhenUnlocked = new();

    private LobbyScreenConverter screenConverter;

    public FacilityType FacilityType => facilityType;
    public Facility CurrentFacility { get; private set; }
    public FacilityManager FacilityManager { get; private set; }
    protected GameObject FacilityRoot => facilityRoot != null ? facilityRoot : gameObject;

    protected virtual void OnEnable()
    {
        upgradeButton?.onClick.AddListener(HandleUpgradeButtonClicked);
        closeButton?.onClick.AddListener(CloseInteraction);
    }

    protected virtual void OnDisable()
    {
        upgradeButton?.onClick.RemoveListener(HandleUpgradeButtonClicked);
        closeButton?.onClick.RemoveListener(CloseInteraction);
    }

    public void Bind(FacilityManager facilityManager)
    {
        if (FacilityManager != null)
        {
            FacilityManager.OnFacilityChanged -= HandleFacilityChanged;
            FacilityManager.OnActiveChanged -= HandleActiveChanged;
        }

        FacilityManager = facilityManager;

        if (FacilityManager != null)
        {
            FacilityManager.OnFacilityChanged += HandleFacilityChanged;
            FacilityManager.OnActiveChanged += HandleActiveChanged;
        }
    }

    public void OpenInteraction(Facility facility)
    {
        CurrentFacility = facility;
        RefreshFacilityUI();
        ShowFacilityView();
        OnOpenInteraction();
    }

    public void CloseInteraction()
    {
        LobbyManager lobbyManager = LobbyManager.Instance;
        if (lobbyManager != null)
        {
            lobbyManager.EndInteraction(FacilityType);
            return;
        }

        FacilityManager?.CloseCurrentInteraction();
    }

    public void CloseInteractionFromManager()
    {
        HideFacilityView();
        OnCloseInteraction();
    }

    public void RefreshFacilityUI()
    {
        CurrentFacility = FacilityManager != null
            ? FacilityManager.GetFacility(FacilityType)
            : CurrentFacility;

        bool isUnlocked = FacilityManager != null && FacilityManager.IsFacilityActive(FacilityType);
        SetObjectsActive(activeWhenLocked, !isUnlocked);
        SetObjectsActive(activeWhenUnlocked, isUnlocked);

        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(!isUnlocked || (CurrentFacility != null && CurrentFacility.CanUpgrade));
    }

    private void HandleUpgradeButtonClicked()
    {
        RequestUpgradeOrUnlock();
    }

    public void RequestUpgradeOrUnlock()
    {
        if (FacilityManager == null)
            return;

        LobbyManager lobbyManager = LobbyManager.Instance;
        if (lobbyManager == null)
            return;

        if (!FacilityManager.IsFacilityActive(FacilityType))
        {
            if (!lobbyManager.TryUnlockFacility(FacilityType))
            {
                Facility facility = FacilityManager.GetFacility(FacilityType);
                int requiredRunCount = facility != null ? facility.RequiredRunCount : 0;
                Logger.Log(this, $"{FacilityType} unlock requires {requiredRunCount} completed runs. Current: {lobbyManager.CompletedRunCount}");
            }

            RefreshFacilityUI();
            return;
        }

        FacilityUpgradeResult result = lobbyManager.TryUpgradeFacility(FacilityType);
        if (result != FacilityUpgradeResult.Success)
            Logger.Log(this, $"{FacilityType} upgrade failed: {result}");
    }

    private void HandleFacilityChanged(FacilityType changedFacilityType)
    {
        if (changedFacilityType == FacilityType)
            RefreshFacilityUI();
    }

    private void HandleActiveChanged(FacilityType changedFacilityType, bool active)
    {
        if (changedFacilityType == FacilityType)
            RefreshFacilityUI();
    }

    private void ShowFacilityView()
    {
        LobbyScreenConverter converter = GetScreenConverter();
        if (converter != null)
            converter.ShowFacilityView(FacilityRoot);
        else
            FacilityRoot.SetActive(true);
    }

    private void HideFacilityView()
    {
        LobbyScreenConverter converter = GetScreenConverter();
        if (converter != null)
            converter.ShowMainView();
        else
            FacilityRoot.SetActive(false);
    }

    private LobbyScreenConverter GetScreenConverter()
    {
        if (screenConverter != null)
            return screenConverter;

        screenConverter = GetComponentInParent<LobbyScreenConverter>();
        if (screenConverter == null)
            screenConverter = FindFirstObjectByType<LobbyScreenConverter>();

        return screenConverter;
    }

    private static void SetObjectsActive(List<GameObject> targets, bool active)
    {
        foreach (GameObject target in targets)
            target?.SetActive(active);
    }

    protected virtual void OnOpenInteraction()
    {
    }

    protected virtual void OnCloseInteraction()
    {
    }
}
