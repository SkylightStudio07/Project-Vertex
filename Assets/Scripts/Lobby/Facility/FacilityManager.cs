using System;
using System.Collections.Generic;
using UnityEngine;

public enum FacilityUpgradeResult
{
    Success,
    Locked,
    NotUpgradeable,
    ExperienceManagerMissing,
    NotEnoughExperience
}

public class FacilityManager : MonoBehaviour
{
    [SerializeField] private List<Facility> facilities = new();
    [SerializeField] private List<FacilityInteractionHandler> interactionHandlers = new();

    private readonly Dictionary<FacilityType, Facility> facilityByType = new();
    private readonly Dictionary<FacilityType, bool> activeByType = new();
    private readonly Dictionary<FacilityType, FacilityInteractionHandler> handlerByType = new();

    private FacilityInteractionHandler currentInteractionHandler;
    private UserExpManager userExpManager;
    private int completedRunCount;

    public FacilityType CurrentInteractionType => currentInteractionHandler != null
        ? currentInteractionHandler.FacilityType
        : FacilityType.None;
    public IReadOnlyList<Facility> Facilities => facilities;

    public event Action<FacilityType> OnFacilityChanged;
    public event Action<FacilityType, bool> OnActiveChanged;
    public event Action<FacilityState> OnFacilityStateChanged;
    public event Action<FacilityType> OnFacilityInteractionStarted;
    public event Action<FacilityType> OnFacilityInteractionEnded;

    private void Awake()
    {
        userExpManager = GetComponent<UserExpManager>();
        BuildFacilityCache(); //설비들 Enum <-> Facility SO 끼리 Build
        BuildHandlerCache(); //설비들 Enum <-> Handler 끼리 Build
    }

    public void Initialize(int completedRunCount)
    {
        this.completedRunCount = Mathf.Max(0, completedRunCount);
        userExpManager = userExpManager != null ? userExpManager : GetComponent<UserExpManager>();
        BuildFacilityCache();
        BuildHandlerCache();
        RefreshFacilityUnlocks(this.completedRunCount);
    }

    public Facility GetFacility(FacilityType facilityType)
    {
        if (facilityType == FacilityType.None)
            return null;

        if (facilityByType.TryGetValue(facilityType, out Facility facility))
            return facility;

        Logger.LogError(this, $"{facilityType} facility is not registered.");
        return null;
    }

    public FacilityState GetFacilityState(FacilityType facilityType)
    {
        Facility facility = GetFacility(facilityType);
        bool isActive = facilityType != FacilityType.None &&
                        activeByType.TryGetValue(facilityType, out bool active) &&
                        active;
        return new FacilityState(facilityType, facility, isActive);
    }

    public bool IsFacilityActive(FacilityType facilityType)
    {
        return activeByType.TryGetValue(facilityType, out bool active) && active;
    }

    public bool IsFacilityUpgraded(FacilityType facilityType)
    {
        Facility facility = GetFacility(facilityType);
        return facility != null && facility.IsUpgradedFacility;
    }

    public bool CanInteract(FacilityType facilityType)
    {
        return facilityType != FacilityType.None &&
               GetFacility(facilityType) != null &&
               (CurrentInteractionType == FacilityType.None || CurrentInteractionType == facilityType);
    }

    public bool TryBeginInteraction(FacilityType facilityType)
    {
        if (CurrentInteractionType == facilityType && facilityType != FacilityType.None)
            return true;

        if (!CanInteract(facilityType))
            return false;

        if (!OpenInteraction(facilityType))
            return false;

        OnFacilityInteractionStarted?.Invoke(facilityType);
        return true;
    }

    public void EndInteraction(FacilityType facilityType)
    {
        if (facilityType == FacilityType.None || CurrentInteractionType != facilityType)
            return;

        CloseCurrentInteraction();
        OnFacilityInteractionEnded?.Invoke(facilityType);
    }

    public void RequestFacilityProgression(FacilityType facilityType)
    {
        FacilityState facilityState = GetFacilityState(facilityType);
        if (!facilityState.IsRegistered)
            return;

        if (!facilityState.IsActive)
        {
            UnlockFacility(facilityType);
            return;
        }

        UpgradeFacility(facilityType);
    }

    public bool UnlockFacility(FacilityType facilityType)
    {
        if (IsFacilityActive(facilityType))
            return true;

        Facility facility = GetFacility(facilityType);
        if (facility == null)
            return false;

        if (!facility.IsUnlockConditionMet(completedRunCount))
        {
            Logger.Log(this, $"{facilityType} unlock requires {facility.RequiredRunCount} completed runs. Current: {completedRunCount}");
            return false;
        }

        SetFacilityActive(facilityType, true);
        return true;
    }

    public FacilityUpgradeResult UpgradeFacility(FacilityType facilityType)
    {
        if (userExpManager == null)
            return LogUpgradeFailed(facilityType, FacilityUpgradeResult.ExperienceManagerMissing);

        if (!IsFacilityActive(facilityType))
            return LogUpgradeFailed(facilityType, FacilityUpgradeResult.Locked);

        Facility facility = GetFacility(facilityType);
        if (facility == null || !facility.CanUpgrade)
            return LogUpgradeFailed(facilityType, FacilityUpgradeResult.NotUpgradeable);

        if (!userExpManager.HasExperience(facility.UpgradeRequiredExp))
            return LogUpgradeFailed(facilityType, FacilityUpgradeResult.NotEnoughExperience);

        if (!userExpManager.TrySpendExperience(facility.UpgradeRequiredExp))
            return LogUpgradeFailed(facilityType, FacilityUpgradeResult.NotEnoughExperience);

        if (!ApplyUpgrade(facilityType))
            return LogUpgradeFailed(facilityType, FacilityUpgradeResult.NotUpgradeable);

        EndInteraction(facilityType);
        return FacilityUpgradeResult.Success;
    }

    public void RefreshFacilityUnlocks(int completedRunCount)
    {
        this.completedRunCount = Mathf.Max(0, completedRunCount);

        foreach (FacilityType facilityType in facilityByType.Keys)
        {
            Facility facility = GetFacility(facilityType);
            if (facility != null && facility.IsUnlockConditionMet(this.completedRunCount))
                SetFacilityActive(facilityType, true);
        }
    }

    private bool ApplyUpgrade(FacilityType facilityType)
    {
        Facility facility = GetFacility(facilityType);
        if (facility == null || !facility.CanUpgrade)
            return false;

        facilityByType[facilityType] = facility.UpgradeFacility;
        OnFacilityChanged?.Invoke(facilityType);
        NotifyFacilityStateChanged(facilityType);
        return true;
    }

    private FacilityUpgradeResult LogUpgradeFailed(FacilityType facilityType, FacilityUpgradeResult result)
    {
        Logger.Log(this, $"{facilityType} upgrade failed: {result}");
        return result;
    }

    private bool OpenInteraction(FacilityType facilityType)
    {
        if (!handlerByType.TryGetValue(facilityType, out FacilityInteractionHandler handler) || handler == null)
            return false;

        if (currentInteractionHandler == handler)
            return true;

        CloseCurrentInteraction();

        currentInteractionHandler = handler;
        handler.OpenInteraction(GetFacilityState(facilityType));
        return true;
    }

    private void CloseCurrentInteraction()
    {
        if (currentInteractionHandler == null)
            return;

        FacilityInteractionHandler handler = currentInteractionHandler;
        currentInteractionHandler = null;
        handler.CloseView();
    }

    public bool IsCurrentInteraction(FacilityType facilityType)
    {
        return CurrentInteractionType == facilityType;
    }

    private void BuildFacilityCache()
    {
        facilityByType.Clear();
        activeByType.Clear();

        foreach (Facility facility in facilities)
        {
            if (facility == null || facility.FacilityType == FacilityType.None)
                continue;

            if (facilityByType.ContainsKey(facility.FacilityType))
            {
                Logger.LogWarning(this, $"Only one base {facility.FacilityType} facility can be registered.");
                continue;
            }

            facilityByType.Add(facility.FacilityType, facility);
            activeByType.Add(facility.FacilityType, facility.DefaultActive);
        }
    }

    private void BuildHandlerCache()
    {
        handlerByType.Clear();

        foreach (FacilityInteractionHandler handler in interactionHandlers)
            RegisterHandler(handler);
    }

    private void RegisterHandler(FacilityInteractionHandler handler)
    {
        if (handler == null || handler.FacilityType == FacilityType.None)
            return;

        if (handlerByType.TryGetValue(handler.FacilityType, out FacilityInteractionHandler registeredHandler))
        {
            if (registeredHandler != handler)
                Logger.LogWarning(this, $"Only one {handler.FacilityType} interaction handler can be registered.");
            return;
        }

        handler.Bind(this);
        handlerByType.Add(handler.FacilityType, handler);
    }

    private void SetFacilityActive(FacilityType facilityType, bool active)
    {
        if (!facilityByType.ContainsKey(facilityType))
            return;

        if (activeByType.TryGetValue(facilityType, out bool currentActive) && currentActive == active)
            return;

        activeByType[facilityType] = active;
        OnActiveChanged?.Invoke(facilityType, active);
        NotifyFacilityStateChanged(facilityType);

        if (!active && IsCurrentInteraction(facilityType))
            CloseCurrentInteraction();
    }

    private void NotifyFacilityStateChanged(FacilityType facilityType)
    {
        OnFacilityStateChanged?.Invoke(GetFacilityState(facilityType));
    }

    [ContextMenu("Debug/Refresh Handlers")]
    private void DebugRefreshHandlers()
    {
        interactionHandlers.Clear();
        interactionHandlers.AddRange(FindObjectsByType<FacilityInteractionHandler>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None));

        BuildHandlerCache();
        Logger.Log(this, $"Refreshed {interactionHandlers.Count} facility interaction handlers.");
    }

    [ContextMenu("Debug/Refresh Facilities")]
    private void DebugRefreshFacilities()
    {
        Initialize(completedRunCount);
        Logger.Log(this, "Refreshed facilities.");
    }

    [ContextMenu("Debug/Log Facility States")]
    private void DebugLogFacilityStates()
    {
        foreach (Facility facility in facilities)
        {
            if (facility == null)
                continue;

            Facility currentFacility = GetFacility(facility.FacilityType);
            Logger.Log(this,
                $"{facility.FacilityType} / Active: {IsFacilityActive(facility.FacilityType)} / " +
                $"Upgraded: {currentFacility != null && currentFacility.IsUpgradedFacility} / " +
                $"Interacting: {IsCurrentInteraction(facility.FacilityType)}");
        }
    }

    [ContextMenu("Debug/Add RunCount")]
    private void DebugAddRunCount()
    {
        completedRunCount = Mathf.Max(0, completedRunCount + 1);
        RefreshFacilityUnlocks(completedRunCount);
        Logger.Log(this, "Added 1 completed run count and refreshed facility unlocks.");
    }
}
