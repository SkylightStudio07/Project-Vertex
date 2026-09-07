using UnityEngine;

// 전투 무기의 정적 정의. 로비 시작 덱(StartingWeaponData)과는 별개다.
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game Asset/Weapon")]
public class WeaponData : ScriptableObject
{
    [SerializeField] private string weaponName;
    [Tooltip("이 무기를 장착하면 모든 무기 사격 카드를 이 카드의 사본으로 교체합니다.")]
    [SerializeField] private CardData shootingCard;

    [Header("전환 시 탄약 (최대 탄창과 무관)")]
    [SerializeField] private bool setAmmoOnEquip;
    [SerializeField, Min(0)] private int ammoOnEquip;

    public string WeaponName => weaponName;
    public CardData ShootingCard => shootingCard;
    public bool SetAmmoOnEquip => setAmmoOnEquip;
    public int AmmoOnEquip => Mathf.Max(0, ammoOnEquip);
}
