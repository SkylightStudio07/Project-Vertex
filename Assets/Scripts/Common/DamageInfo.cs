public struct DamageInfo
{
    public int Amount;
    public ICombatant Source; // null이면 환경 피해(burn 등)
    public bool IsPiercing;   // 블록 무시
    public bool IsAmmoAttack; // 탄약 코스트가 있는 공격 카드 — 죄와 벌 등 패시브 훅용
    public float PiercePercent; // 0~1: 이 비율만큼은 방어도를 건너뛰고 체력에 바로 들어간다 (카리나 '천부적 살인술')

    // 방어도로 막을 수 있는 몫 / 관통하는 몫 (PiercePercent 기준, 관통 몫은 반올림)
    public int PiercedPart => IsPiercing ? Amount : UnityEngine.Mathf.Clamp(UnityEngine.Mathf.RoundToInt(Amount * PiercePercent), 0, Amount);

    public DamageInfo(int amount, ICombatant source = null, bool isPiercing = false, bool isAmmoAttack = false)
    {
        Amount = amount;
        Source = source;
        IsPiercing = isPiercing;
        IsAmmoAttack = isAmmoAttack;
        PiercePercent = 0f;
    }
}
