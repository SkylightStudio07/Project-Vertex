using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class FacilityInteractionHandler : MonoBehaviour
{
    [Header("공통 시설 UI")]
    [SerializeField] private FacilityType facilityType;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text facilityStateText;
    [SerializeField] private GameObject lockedContent;
    [SerializeField] private GameObject unlockedContent;

    public FacilityType FacilityType => facilityType;
    public Facility CurrentFacility { get; private set; }
    public FacilityManager FacilityManager { get; private set; }

    protected virtual void OnEnable()
    {
        upgradeButton?.onClick.AddListener(HandleUpgradeButtonClicked);
    }

    protected virtual void OnDisable()
    {
        upgradeButton?.onClick.RemoveListener(HandleUpgradeButtonClicked);
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
        OnCloseInteraction();
    }

    public void RefreshFacilityUI()
    {
        CurrentFacility = FacilityManager != null
            ? FacilityManager.GetFacility(FacilityType)
            : CurrentFacility;

        bool isUnlocked = FacilityManager != null && FacilityManager.IsFacilityActive(FacilityType);
        lockedContent?.SetActive(!isUnlocked);
        unlockedContent?.SetActive(isUnlocked);

        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(!isUnlocked || (CurrentFacility != null && CurrentFacility.CanUpgrade));

        if (facilityStateText == null || CurrentFacility == null)
            return;

        if (!isUnlocked)
        {
            facilityStateText.text = string.Empty;
            return;
        }

        facilityStateText.text = CurrentFacility.CanUpgrade
            ? $"업그레이드 필요 EXP: {CurrentFacility.UpgradeRequiredExp}"
            : "최대 단계입니다.";
    }

    private void HandleUpgradeButtonClicked()
    {
        if (FacilityManager == null)
            return;

        LobbyManager lobbyManager = LobbyManager.Instance;
        if (lobbyManager == null)
            return;

        if (!FacilityManager.IsFacilityActive(FacilityType))
        {
            int completedRuns = lobbyManager.CompletedRunCount;

            if (lobbyManager.TryUnlockFacility(FacilityType))
            {
                RefreshFacilityUI();
                SetStateMessage("시설이 해금되었습니다.");
                return;
            }

            Facility facility = FacilityManager.GetFacility(FacilityType);
            int requiredRunCount = facility != null ? facility.RequiredRunCount : 0;
            SetStateMessage($"런 {requiredRunCount}회 완료 후 해금됩니다. (현재 {completedRuns}회)");
            return;
        }

        FacilityUpgradeResult result = lobbyManager.TryUpgradeFacility(FacilityType);
        switch (result)
        {
            case FacilityUpgradeResult.Locked:
                SetStateMessage("아직 해금되지 않은 시설입니다.");
                break;
            case FacilityUpgradeResult.NotUpgradeable:
                SetStateMessage("더 이상 업그레이드할 수 없습니다.");
                break;
            case FacilityUpgradeResult.ExperienceManagerMissing:
                SetStateMessage("EXP 정보를 찾을 수 없습니다.");
                break;
            case FacilityUpgradeResult.NotEnoughExperience:
                Facility facility = FacilityManager.GetFacility(FacilityType);
                int requiredExperience = facility != null ? facility.UpgradeRequiredExp : 0;
                SetStateMessage($"EXP가 부족합니다. 필요 EXP: {requiredExperience}");
                break;
        }
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

    private void SetStateMessage(string message)
    {
        if (facilityStateText != null)
            facilityStateText.text = message;
    }

    protected abstract void OnOpenInteraction();

    protected virtual void OnCloseInteraction()
    {
    }
}
