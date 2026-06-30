public readonly struct FacilityState
{
    public FacilityState(FacilityType facilityType, Facility facility, bool isActive)
    {
        FacilityType = facilityType;
        Facility = facility;
        IsActive = isActive;
    }

    public FacilityType FacilityType { get; }
    public Facility Facility { get; }
    public bool IsActive { get; }
    public bool IsRegistered => Facility != null;
    public bool IsUpgraded => Facility != null && Facility.IsUpgradedFacility;
    public bool CanUpgrade => Facility != null && Facility.CanUpgrade;
    public int RequiredRunCount => Facility != null ? Facility.RequiredRunCount : 0;
    public int UpgradeRequiredExp => Facility != null ? Facility.UpgradeRequiredExp : 0;
    public string DisplayName => Facility != null ? Facility.DisplayName : string.Empty;
    public string Description => Facility != null ? Facility.Description : string.Empty;
}
