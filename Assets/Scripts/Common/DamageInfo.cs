public struct DamageInfo
{
    public int Amount;
    public ICombatant Source; // null이면 환경 피해(burn 등)
    public bool IsPiercing;   // 블록 무시

    public DamageInfo(int amount, ICombatant source = null, bool isPiercing = false)
    {
        Amount = amount;
        Source = source;
        IsPiercing = isPiercing;
    }
}
