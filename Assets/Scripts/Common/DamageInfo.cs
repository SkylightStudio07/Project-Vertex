public struct DamageInfo
{
    public int Amount;
    public ICombatant Source; // null이면 환경 피해(burn 등)
    public bool IsPiercing;   // 블록 무시
    public bool IsAmmoAttack; // 탄약 코스트가 있는 공격 카드 — 죄와 벌 등 패시브 훅용

    public DamageInfo(int amount, ICombatant source = null, bool isPiercing = false, bool isAmmoAttack = false)
    {
        Amount = amount;
        Source = source;
        IsPiercing = isPiercing;
        IsAmmoAttack = isAmmoAttack;
    }
}
