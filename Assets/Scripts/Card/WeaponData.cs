using UnityEngine;
using UnityEngine.Serialization;

// 무기고 영구 강화 종류. 연사형(탄약 많음)은 탄창을, 한방형(탄약 적음·한 발이 강함)은 사격 피해를 키운다.
public enum WeaponUpgradeType
{
    None,
    MaxAmmo,    // 최대 탄약 증가 (전투 시작·재장전 탄약도 같이 오른다)
    ShotDamage, // 탄약을 쓰는 공격의 타격마다 피해 증가
}

// 전투 무기의 정적 정의. 로비 시작 덱(StartingWeaponData)과는 별개다.
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game Asset/Weapon")]
public class WeaponData : ScriptableObject
{
    [SerializeField] private string weaponName;
    [Tooltip("이 무기를 장착하면 모든 무기 사격 카드를 이 카드의 사본으로 교체합니다.")]
    [SerializeField] private CardData shootingCard;

    [Header("최대 탄약")]
    [Tooltip("재장전 및 총기 전환 시 현재 탄약을 이 수로 채웁니다.")]
    [FormerlySerializedAs("ammoOnEquip")]
    [SerializeField, Min(0)] private int maxAmmo = 3;

    [Header("초탄 · 막탄")]
    [Tooltip("켜면 재장전 후 처음 탄약을 쓰는 카드(초탄)와 탄창을 비우는 카드(막탄) 판정이 이 무기에 적용됩니다.")]
    [SerializeField] private bool usesAmmoBoundary;

    [Header("무기고 강화")]
    [SerializeField] private WeaponUpgradeType upgradeType;
    [Tooltip("단계별 누적 수치. [1, 2]면 1단계 +1, 2단계 +2. 배열 길이가 최대 단계다.")]
    [SerializeField] private int[] upgradeValues = new int[0];
    [Tooltip("각 단계로 올리는 비용 (재화 미정 — 임시 값)")]
    [SerializeField] private int[] upgradeCosts = new int[0];

    public string WeaponName => weaponName;
    public CardData ShootingCard => shootingCard;
    public int MaxAmmo => Mathf.Max(0, maxAmmo);
    public bool UsesAmmoBoundary => usesAmmoBoundary;

    public WeaponUpgradeType UpgradeType => upgradeType;
    public int MaxUpgradeLevel => upgradeType == WeaponUpgradeType.None || upgradeValues == null ? 0 : upgradeValues.Length;
    public int UpgradeValueAt(int level)
        => level <= 0 || MaxUpgradeLevel == 0 ? 0 : upgradeValues[Mathf.Min(level, MaxUpgradeLevel) - 1];
    // level 단계로 올리는 비용 (1부터). 비용 표가 없으면 0
    public int UpgradeCostTo(int level)
        => upgradeCosts == null || level <= 0 || level > upgradeCosts.Length ? 0 : upgradeCosts[level - 1];
    public int MaxAmmoBonusAt(int level) => upgradeType == WeaponUpgradeType.MaxAmmo ? UpgradeValueAt(level) : 0;
    public int ShotDamageBonusAt(int level) => upgradeType == WeaponUpgradeType.ShotDamage ? UpgradeValueAt(level) : 0;
}
