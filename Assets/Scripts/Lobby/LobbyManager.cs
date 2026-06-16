using System;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : SingletonBehaviour<LobbyManager>
{
    [SerializeField] private List<Facility> facilities = new List<Facility>();

    private LobbyData lobbyData;

    public Facility CurrentInteractingFacility { get; private set; }
    public IReadOnlyList<Facility> Facilities => facilities;
    public LobbyData LobbyData => lobbyData;

    public event Action<Facility> OnFacilityInteractionStarted;
    public event Action<Facility> OnFacilityInteractionEnded;

    protected override void Init()
    {
        m_IsDestroyOnLoad = true;
        base.Init();
        lobbyData = SaveDataManager.Instance.GetSaveData<LobbyData>();
    }

    private void Start()
    {
        CollectSceneFacilities();
        ApplyLobbyData();
    }

    public void RegisterFacility(Facility facility)
    {
        if (facility == null || facilities.Contains(facility))
            return;

        facilities.Add(facility);
        ApplyLobbyData(facility);
    }

    public void UnregisterFacility(Facility facility)
    {
        if (facility == null)
            return;

        facilities.Remove(facility);

        if (CurrentInteractingFacility == facility)
            CurrentInteractingFacility = null;
    }

    public bool CanInteract(Facility facility)
    {
        return facility != null &&
               facility.IsActive &&
               (CurrentInteractingFacility == null || CurrentInteractingFacility == facility);
    }

    public bool TryBeginInteraction(Facility facility)
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

    public void EndInteraction(Facility facility)
    {
        if (facility == null || CurrentInteractingFacility != facility)
            return;

        CurrentInteractingFacility = null;
        OnFacilityInteractionEnded?.Invoke(facility);
    }

    public void SetFacilityActive(Facility facility, bool active)
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
        foreach (Facility facility in FindObjectsByType<Facility>(FindObjectsSortMode.None))
            RegisterFacility(facility);
    }

    private void ApplyLobbyData()
    {
        if (lobbyData == null)
            return;

        foreach (Facility facility in facilities)
            ApplyLobbyData(facility);
    }

    private void ApplyLobbyData(Facility facility)
    {
        if (facility == null || lobbyData == null)
            return;

        LobbyDataProgressData progressData = lobbyData.GetOrCreateFacilityProgressData(facility.FacilityId);
        facility.SetFacilityActive(progressData.isActive);
    }
}
