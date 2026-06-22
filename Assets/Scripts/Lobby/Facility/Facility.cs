using UnityEngine;
using UnityEngine.Serialization;

/// <summary>로비 시설의 해금 조건과 업그레이드 정보를 정의하는 데이터.</summary>
[CreateAssetMenu(fileName = "Facility", menuName = "Lobby/Facility")]
public class Facility : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string facilityId;
    [SerializeField] private string displayName;
    [SerializeField] private string description;

    [Header("해금 조건")]
    [SerializeField] private bool defaultActive = true;
    [SerializeField, Min(0)] private int requiredRunCount;

    [Header("업그레이드")]
    [SerializeField] private bool isUpgradedFacility;
    [SerializeField] private Facility upgradeFacility;
    [FormerlySerializedAs("requiredExp")]
    [SerializeField, Min(0)] private int upgradeRequiredExp;

    public string FacilityId => facilityId;
    public string DisplayName => displayName;
    public string Description => description;
    public bool DefaultActive => defaultActive;
    public int RequiredRunCount => requiredRunCount;
    public bool IsUpgradedFacility => isUpgradedFacility;
    public Facility UpgradeFacility => upgradeFacility;
    public int UpgradeRequiredExp => upgradeRequiredExp;
    public bool CanUpgrade => upgradeFacility != null;

    public bool IsUnlockConditionMet(int completedRunCount)
    {
        return defaultActive || Mathf.Max(0, completedRunCount) >= requiredRunCount;
    }

    private void OnValidate()
    {
        requiredRunCount = Mathf.Max(0, requiredRunCount);
        upgradeRequiredExp = Mathf.Max(0, upgradeRequiredExp);

        if (upgradeFacility == null)
            upgradeRequiredExp = 0;
    }
}
