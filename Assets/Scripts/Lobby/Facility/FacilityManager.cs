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

    public FacilityType CurrentInteractionType => currentInteractionHandler != null
        ? currentInteractionHandler.FacilityType
        : FacilityType.None;
    public IReadOnlyList<Facility> Facilities => facilities;

    public event Action<FacilityType> OnFacilityChanged;
    public event Action<FacilityType, bool> OnActiveChanged;

    private void Awake()
    {
        BuildFacilityCache(); //설비들 Enum <-> Facility SO 끼리 Build
        BuildHandlerCache(); //설비들 Enum <-> Handler 끼리 Build
    }

    public void Initialize(int completedRunCount)
    {
        BuildFacilityCache();
        BuildHandlerCache();
        RefreshFacilityUnlocks(completedRunCount);
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

    public bool IsFacilityActive(FacilityType facilityType)
    {
        return activeByType.TryGetValue(facilityType, out bool active) && active;
    }

    public bool TryUnlockFacility(FacilityType facilityType, int completedRunCount)
    {
        if (IsFacilityActive(facilityType))
            return true;

        Facility facility = GetFacility(facilityType);
        if (facility == null || !facility.IsUnlockConditionMet(completedRunCount))
            return false;

        SetFacilityActive(facilityType, true);
        return true;
    }

    public void RefreshFacilityUnlocks(int completedRunCount)
    {
        foreach (FacilityType facilityType in facilityByType.Keys)
        {
            Facility facility = GetFacility(facilityType);
            if (facility != null && facility.IsUnlockConditionMet(completedRunCount))
                SetFacilityActive(facilityType, true);
        }
    }

    public bool ApplyUpgrade(FacilityType facilityType)
    {
        Facility facility = GetFacility(facilityType);
        if (facility == null || !facility.CanUpgrade)
            return false;

        facilityByType[facilityType] = facility.UpgradeFacility;
        OnFacilityChanged?.Invoke(facilityType);
        return true;
    }

    public bool OpenInteraction(FacilityType facilityType)
    {
        if (!handlerByType.TryGetValue(facilityType, out FacilityInteractionHandler handler) || handler == null)
            return false;

        if (currentInteractionHandler == handler)
            return true;

        CloseCurrentInteraction();

        currentInteractionHandler = handler;
        handler.OpenInteraction(GetFacility(facilityType));
        return true;
    }

    public void CloseCurrentInteraction()
    {
        if (currentInteractionHandler == null)
            return;

        FacilityInteractionHandler handler = currentInteractionHandler;
        currentInteractionHandler = null;
        handler.CloseInteractionFromManager();
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
            if (facility == null || facility.FacilityType == FacilityType.None || facility.IsUpgradedFacility)
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

        if (!active && IsCurrentInteraction(facilityType))
            CloseCurrentInteraction();
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

    [ContextMenu("Debug/Add RunCount")]
    private void DebugAddRunCount()
    {
        RefreshFacilityUnlocks(1);
        Logger.Log(this, "Added 1 completed run count and refreshed facility unlocks.");
    }
}
