using System;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : SingletonBehaviour<LobbyManager>
{
    [SerializeField] private List<FacilityController> facilities = new();
    [SerializeField, Min(0)] private int completedRunCount; // 런 완료 횟수. 시설 오픈에 사용

    private readonly Dictionary<FacilityType, FacilityController> facilityByType = new();

    public FacilityController CurrentInteractingFacility { get; private set; }
    public IReadOnlyList<FacilityController> Facilities => facilities;
    public int CompletedRunCount => completedRunCount;

    public event Action<FacilityController> OnFacilityInteractionStarted;
    public event Action<FacilityController> OnFacilityInteractionEnded;

    protected override void Init()
    {
        m_IsDestroyOnLoad = true; // Lobby 씬을 벗어나면 제거
        base.Init();
    }

    private void Start()
    {
        CollectSceneFacilities(); // 우선 지금은 FindObject로 작동. 추후 저장 기능이 나오면 저장된 데이터를 불러오는 방식으로 설정
        RefreshFacilityUnlocks();
    }

    // 시설 등록 및 제거
    private void RegisterFacility(FacilityController facility)
    {
        if (facility == null)
            return;

        FacilityType facilityType = facility.FacilityType;
        if (facilityType == FacilityType.None)
        {
            Logger.LogWarning(this, $"Facility type is not assigned: {facility.name}");
            return;
        }

        if (facilityByType.TryGetValue(facilityType, out FacilityController registeredFacility))
        {
            if (registeredFacility != facility)
                Logger.LogWarning(this, $"Only one {facilityType} facility can exist in the scene.");
            return;
        }

        facilityByType.Add(facilityType, facility);
        facilities.Add(facility);
    }

    public FacilityController GetFacility(FacilityType facilityType)
    {
        if (facilityType == FacilityType.None)
            return null;

        facilityByType.TryGetValue(facilityType, out FacilityController facility);
        return facility;
    }

    public bool IsFacilityRegistered(FacilityType facilityType)
    {
        return GetFacility(facilityType) != null;
    }

    //활성화 여부 반환. 사실상 시설 해금 여부를 반환하는 함메서드
    public bool IsFacilityActive(FacilityType facilityType)
    {
        FacilityController facility = GetFacility(facilityType);
        return facility != null && facility.IsActive;
    }

    public bool IsFacilityUpgraded(FacilityType facilityType)
    {
        FacilityController facility = GetFacility(facilityType);
        return facility != null && facility.IsUpgraded;
    }

    // 시설 상호작용 가능 여부 반환
    public bool CanInteract(FacilityType facilityType)
    {
        FacilityController facility = GetFacility(facilityType);
        return facility != null &&
               (CurrentInteractingFacility == null || CurrentInteractingFacility == facility);
    }

    // 시설 상호작용 시도. 메인 화면에서 각 시설 버튼을 누르면 이 함수가 호출됨
    public bool TryBeginInteraction(FacilityType facilityType)
    {
        FacilityController facility = GetFacility(facilityType);
        if (CurrentInteractingFacility == facility && facility != null)
            return true;

        if (!CanInteract(facilityType))
            return false;

        CurrentInteractingFacility = facility;
        OnFacilityInteractionStarted?.Invoke(facility);
        return true;
    }

    // 시설 상호작용을 끝내는 함수. 각 시설 UI에서 돌아가기를 누르면 이 함수가 실행됨
    public void EndInteraction(FacilityType facilityType)
    {
        FacilityController facility = GetFacility(facilityType);
        if (facility == null || CurrentInteractingFacility != facility)
            return;

        CurrentInteractingFacility = null;
        OnFacilityInteractionEnded?.Invoke(facility);
    }

    // 시설 잠금 해제 시도
    public bool TryUnlockFacility(FacilityType facilityType)
    {
        FacilityController facility = GetFacility(facilityType);
        if (facility == null)
            return false;

        if (facility.IsActive)
            return true;

        if (!facility.IsUnlockConditionMet(completedRunCount))
            return false;

        SetFacilityActive(facility, true);
        return true;
    }

    public void AddCompletedRun()
    {
        completedRunCount = Mathf.Max(0, completedRunCount + 1);
        RefreshFacilityUnlocks();
    }

    public void RefreshFacilityUnlocks()
    {
        foreach (FacilityController facility in facilities)
        {
            if (facility != null && facility.IsUnlockConditionMet(completedRunCount))
                SetFacilityActive(facility, true);
        }
    }

    private void CollectSceneFacilities()
    {
        facilities.Clear();
        facilityByType.Clear();

        foreach (FacilityController facility in FindObjectsByType<FacilityController>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            RegisterFacility(facility);
        }
    }

    // 시설 활성화 상태 변경 함수
    private void SetFacilityActive(FacilityController facility, bool active)
    {
        facility?.SetFacilityActive(active);
    }

    // 디버그용 함수들
    [ContextMenu("Debug/Refresh Facilities")]
    private void DebugRefreshFacilities()
    {
        CollectSceneFacilities();
        RefreshFacilityUnlocks();
        Logger.Log(this, $"Refreshed {facilities.Count} facilities.");
    }

    [ContextMenu("Debug/Log Facility States")]
    private void DebugLogFacilityStates()
    {
        foreach (FacilityController facility in facilities)
        {
            if (facility == null)
                continue;

            Logger.Log(this,
                $"{facility.FacilityType} / Active: {facility.IsActive} / " +
                $"Upgraded: {facility.IsUpgraded} / Interacting: {facility.IsInteracting}");
        }
    }
}
