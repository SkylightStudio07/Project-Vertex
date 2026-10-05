using UnityEngine;
using UnityEngine.Serialization;

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

    public string WeaponName => weaponName;
    public CardData ShootingCard => shootingCard;
    public int MaxAmmo => Mathf.Max(0, maxAmmo);
    public bool UsesAmmoBoundary => usesAmmoBoundary;
}
