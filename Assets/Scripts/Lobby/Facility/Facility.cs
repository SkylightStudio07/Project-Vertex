using UnityEngine;

/// <summary>
/// 로비 설비들을 정의하는 SO
/// </summary>
[CreateAssetMenu(fileName = "Facility", menuName = "Lobby/Facility")]
public class Facility : ScriptableObject
{
    [SerializeField] private string facilityId;
    [SerializeField] private string displayName;
    [SerializeField] private bool defaultActive = true;
    [SerializeField] private int requiredExp = 0;
    [SerializeField] private string description;

    public string FacilityId => facilityId;
    public string DisplayName => displayName;
    public bool DefaultActive => defaultActive;
    public int RequiredExp => requiredExp;
    public string Description => description;
}
