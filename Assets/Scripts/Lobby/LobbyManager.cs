using UnityEngine;

[RequireComponent(typeof(UserExpManager), typeof(FacilityManager))]
public class LobbyManager : SingletonBehaviour<LobbyManager>
{
    [SerializeField, Min(0)] private int completedRunCount; // 런 완료 횟수. 시설 오픈에 사용

    public FacilityManager FacilityManager => facilityManager;
    public UserExpManager UserExpManager => userExpManager;
    public int CompletedRunCount => completedRunCount;

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
    }

    public void AddCompletedRun()
    {
        completedRunCount = Mathf.Max(0, completedRunCount + 1);
        facilityManager?.RefreshFacilityUnlocks(completedRunCount);
    }
}
