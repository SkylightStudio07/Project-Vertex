using System;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : SingletonBehaviour<LobbyManager>
{
    [SerializeField] private List<FacilityController> facilities = new List<FacilityController>();

    private LobbyData lobbyData;

    public FacilityController CurrentInteractingFacility { get; private set; }
    public IReadOnlyList<FacilityController> Facilities => facilities;
    public LobbyData LobbyData => lobbyData;

    public event Action<FacilityController> OnFacilityInteractionStarted;
    public event Action<FacilityController> OnFacilityInteractionEnded;

    protected override void Init()
    {
        m_IsDestroyOnLoad = true;
        base.Init();
        lobbyData = new LobbyData();
    }

    private void Start()
    {
        CollectSceneFacilities();
        lobbyData.SetDefaultData(facilities);
        ApplyLobbyData();
    }

    public void RegisterFacility(FacilityController facility)
    {
        if (facility == null || facilities.Contains(facility))
            return;

        facilities.Add(facility);
        ApplyLobbyData(facility);
    }

    public void UnregisterFacility(FacilityController facility)
    {
        if (facility == null)
            return;

        facilities.Remove(facility);

        if (CurrentInteractingFacility == facility)
            CurrentInteractingFacility = null;
    }

    public bool CanInteract(FacilityController facility)
    {
        return facility != null &&
               facility.IsActive &&
               (CurrentInteractingFacility == null || CurrentInteractingFacility == facility);
    }

    public bool TryBeginInteraction(FacilityController facility)
    {
        if (CurrentInteractingFacility == facility)
            return true;

        if (!CanInteract(facility))
            return false;

        RegisterFacility(facility);
        CurrentInteractingFacility = facility;
        OnFacilityInteractionStarted?.Invoke(facility);
        return true;
    }

    public void EndInteraction(FacilityController facility)
    {
        if (facility == null || CurrentInteractingFacility != facility)
            return;

        CurrentInteractingFacility = null;
        OnFacilityInteractionEnded?.Invoke(facility);
    }

    public void SetFacilityActive(FacilityController facility, bool active)
    {
        if (facility == null)
            return;

        facility.SetFacilityActive(active);

        LobbyDataProgressData progressData = lobbyData?.GetOrCreateFacilityProgressData(facility.FacilityId);
        if (progressData != null)
            progressData.isActive = active;
    }

    private void CollectSceneFacilities()
    {
        foreach (FacilityController facility in FindObjectsByType<FacilityController>(FindObjectsSortMode.None))
            RegisterFacility(facility);
    }

    private void ApplyLobbyData()
    {
        if (lobbyData == null)
            return;

        foreach (FacilityController facility in facilities)
            ApplyLobbyData(facility);
    }

    private void ApplyLobbyData(FacilityController facility)
    {
        if (facility == null || lobbyData == null)
            return;

        LobbyDataProgressData progressData = lobbyData.GetOrCreateFacilityProgressData(facility.FacilityId, facility.DefaultActive);
        facility.SetFacilityActive(progressData.isActive);
    }

    // Debug buttons for testing lobby facility behavior in the Inspector.
    [ContextMenu("Debug/Rebuild Lobby Data")]
    private void DebugRebuildLobbyData()
    {
        CollectSceneFacilities();
        lobbyData ??= new LobbyData();
        lobbyData.SetDefaultData(facilities);
        ApplyLobbyData();
        Logger.Log(this, "Lobby data rebuilt from current scene facilities.");
    }

    [ContextMenu("Debug/Log Facility States")]
    private void DebugLogFacilityStates()
    {
        foreach (FacilityController facility in facilities)
        {
            if (facility == null)
                continue;

            Logger.Log(this, $"{facility.FacilityId} / Active: {facility.IsActive} / Interacting: {facility.IsInteracting}");
        }
    }

    [ContextMenu("Debug/Toggle First Facility Active")]
    private void DebugToggleFirstFacilityActive()
    {
        if (facilities.Count == 0 || facilities[0] == null)
        {
            Logger.LogWarning(this, "No facility exists to toggle.");
            return;
        }

        SetFacilityActive(facilities[0], !facilities[0].IsActive);
        Logger.Log(this, $"{facilities[0].FacilityId} active changed to {facilities[0].IsActive}.");
    }
}
