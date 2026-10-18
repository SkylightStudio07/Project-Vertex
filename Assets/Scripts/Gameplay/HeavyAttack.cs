// 강한 공격 기준. 인텐트 표시(EnemyView), 적 예비 동작(BattleManager), 타격 연출(BattleImpactFx)이 같이 쓴다.
// 피해는 연타 합계(인텐트 숫자와 같은 값). 기획: Docs/타격연출.md
public static class HeavyAttack
{
    public const int Heavy = 15;      // 강함: 인텐트 아이콘 확대
    public const int Lethal = 30;     // 치명: 붉은 맥동 인텐트 + 예비 동작 + 줌
    public const int PlayerBig = 20;  // 플레이어가 한 번에 넣는 큰 피해: 줌 + 긴 히트스톱

    public enum Tier { Normal, Heavy, Lethal }

    public static Tier Of(int? damage) =>
        !damage.HasValue ? Tier.Normal : damage.Value >= Lethal ? Tier.Lethal : damage.Value >= Heavy ? Tier.Heavy : Tier.Normal;
}
