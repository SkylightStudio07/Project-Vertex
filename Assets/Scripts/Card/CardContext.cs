// ============================================================
// filename   : CardContext.cs
// 작성자    : SkylightStudio07 - 박영서
// 작성일    : 2026-04-12
// description   : 카드 실행 시 필요한 맥락 정보를 담는 데이터 클래스.
//             BattleManager에서 생성 후 CardEffect.Execute()에 전달.
// ============================================================
// 업데이트 로그
// ------------------------------------------------------------
// 2026-06-05 | 박근혁 | 
// 아이템 사용 구현 위해 ItemData 필드 추가. 
// ============================================================

using System.Collections.Generic;

public class CardContext
{
    public BattleManager Battle;           // 전투 상태 접근 (에너지, 탄약, 드로우 등)
    public CardData Card;                  // 실행 중인 카드 (플레이어 행동 시), 적 행동이면 null
    public ItemData Item;                  // 실행 중인 아이템 (플레이어 행동 시), 카드 행동이면 null
    public EnemyInstance ActingEnemy;      // 적이 시전자일 때 세팅; 플레이어 행동이면 null
    public EnemyInstance Target;           // 단일 대상용 (플레이어가 적을 타겟팅할 때)
    public List<EnemyInstance> AllEnemies; // 광역기용
}
