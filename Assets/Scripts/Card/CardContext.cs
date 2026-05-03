// ============================================================
// 파일명    : CardContext.cs
// 작성자    : SkylightStudio07 - 박영서
// 작성일    : 2026-04-12
// 설명      : 카드 실행 시 필요한 맥락 정보를 담는 데이터 클래스.
//             BattleManager에서 생성 후 CardEffect.Execute()에 전달.
// ============================================================

using System.Collections.Generic;

public class CardContext
{
    public BattleManager Battle;        // 전투 상태 접근 (에너지, 탄약, 드로우 등)
    public CardData Card;               // 실행 중인 카드
    public EnemyInstance Target;        // 단일 대상용
    public List<EnemyInstance> AllEnemies; // 광역기용
}
