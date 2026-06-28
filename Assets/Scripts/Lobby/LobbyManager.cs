using System;
using UnityEngine;

[RequireComponent(typeof(UserExpManager), typeof(FacilityManager))]
public class LobbyManager : SingletonBehaviour<LobbyManager>
{
    [SerializeField, Min(0)] private int completedRunCount; // 런 완료 횟수. 시설 오픈에 사용

    public FacilityType CurrentInteractingFacilityType { get; private set; } = FacilityType.None;
    public FacilityManager FacilityManager => facilityManager;
    public UserExpManager UserExpManager => userExpManager;
    public int CompletedRunCount => completedRunCount;

    public event Action<FacilityType> OnFacilityInteractionStarted;
    public event Action<FacilityType> OnFacilityInteractionEnded;

    private FacilityManager facilityManager;
    private UserExpManager userExpManager;

    protected override void Init()
    {
        m_IsDestroyOnLoad = true; // Lobby 씬을 벗어나면 제거
        base.Init();

        if (Instance != this)
            return;

        facilityManager = facilityManager != null ? facilityManager : GetComponent<FacilityManager>();
        userExpManager = userExpManager != null ? userExpManager : GetComponent<UserExpManager>();
    }

    private void Start()
    {
        facilityManager?.Initialize(completedRunCount);
        RefreshFacilityUnlocks(); // 우선 지금은 FindObject로 작동. 추후 저장 기능이 나오면 저장된 데이터를 불러오는 방식으로 설정
    }

    // 시설 등록
    public Facility GetFacility(FacilityType facilityType)
    {
        return facilityManager != null ? facilityManager.GetFacility(facilityType) : null;
    }

    //활성화 여부 반환. 사실상 시설 해금 여부를 반환하는 함메서드
    public bool IsFacilityActive(FacilityType facilityType)
    {
        return facilityManager != null && facilityManager.IsFacilityActive(facilityType);
    }

    public bool IsFacilityUpgraded(FacilityType facilityType)
    {
        Facility facility = GetFacility(facilityType);
        return facility != null && facility.IsUpgradedFacility;
    }

    // 시설 상호작용 가능 여부 반환
    public bool CanInteract(FacilityType facilityType)
    {
        return facilityType != FacilityType.None &&
               facilityManager != null &&
               facilityManager.GetFacility(facilityType) != null &&
               (CurrentInteractingFacilityType == FacilityType.None || CurrentInteractingFacilityType == facilityType);
    }

    // 시설 상호작용 시도. 메인 화면에서 각 시설 버튼을 누르면 이 함수가 호출됨
    public bool TryBeginInteraction(FacilityType facilityType)
    {
        if (CurrentInteractingFacilityType == facilityType && facilityType != FacilityType.None)
            return true;

        if (!CanInteract(facilityType))
            return false;

        if (!facilityManager.OpenInteraction(facilityType))
            return false;

        CurrentInteractingFacilityType = facilityType;
        OnFacilityInteractionStarted?.Invoke(facilityType);
        return true;
    }

    // 시설 상호작용을 끝내는 함수. 각 시설 UI에서 돌아가기를 누르면 이 함수가 실행됨
    public void EndInteraction(FacilityType facilityType)
    {
        if (facilityType == FacilityType.None || CurrentInteractingFacilityType != facilityType)
            return;

        facilityManager?.CloseCurrentInteraction();
        CurrentInteractingFacilityType = FacilityType.None;
        OnFacilityInteractionEnded?.Invoke(facilityType);
    }

    // 시설 잠금 해제 시도
    public bool TryUnlockFacility(FacilityType facilityType)
    {
        return facilityManager != null && facilityManager.TryUnlockFacility(facilityType, completedRunCount);
    }

    public FacilityUpgradeResult TryUpgradeFacility(FacilityType facilityType)
    {
        if (facilityManager == null)
            return FacilityUpgradeResult.NotUpgradeable;

        if (userExpManager == null)
            return FacilityUpgradeResult.ExperienceManagerMissing;

        if (!facilityManager.IsFacilityActive(facilityType))
            return FacilityUpgradeResult.Locked;

        Facility facility = facilityManager.GetFacility(facilityType);
        if (facility == null || !facility.CanUpgrade)
            return FacilityUpgradeResult.NotUpgradeable;

        if (!userExpManager.HasExperience(facility.UpgradeRequiredExp))
            return FacilityUpgradeResult.NotEnoughExperience;

        if (!userExpManager.TrySpendExperience(facility.UpgradeRequiredExp))
            return FacilityUpgradeResult.NotEnoughExperience;

        if (!facilityManager.ApplyUpgrade(facilityType))
            return FacilityUpgradeResult.NotUpgradeable;

        EndInteraction(facilityType);
        return FacilityUpgradeResult.Success;
    }

    public void AddCompletedRun()
    {
        completedRunCount = Mathf.Max(0, completedRunCount + 1);
        RefreshFacilityUnlocks();
    }

    public void RefreshFacilityUnlocks()
    {
        facilityManager?.RefreshFacilityUnlocks(completedRunCount);
    }

    // 디버그용 함수들
    [ContextMenu("Debug/Refresh Facilities")]
    private void DebugRefreshFacilities()
    {
        facilityManager?.Initialize(completedRunCount);
        RefreshFacilityUnlocks();
        Logger.Log(this, "Refreshed facilities.");
    }

    [ContextMenu("Debug/Log Facility States")]
    private void DebugLogFacilityStates()
    {
        if (facilityManager == null)
            return;

        foreach (Facility facility in facilityManager.Facilities)
        {
            if (facility == null)
                continue;

            Facility currentFacility = facilityManager.GetFacility(facility.FacilityType);
            Logger.Log(this,
                $"{facility.FacilityType} / Active: {facilityManager.IsFacilityActive(facility.FacilityType)} / " +
                $"Upgraded: {currentFacility != null && currentFacility.IsUpgradedFacility} / " +
                $"Interacting: {facilityManager.IsCurrentInteraction(facility.FacilityType)}");
        }
    }
}
