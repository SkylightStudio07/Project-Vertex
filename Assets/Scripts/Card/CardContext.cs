// ============================================================
// filename   : CardContext.cs
// 작성자    : SkylightStudio07 - 박영서
// 작성일    : 2026-04-12
// description   : 카드 실행 시 필요한 맥락 정보를 담는 데이터 클래스.
//             BattleManager에서 생성 후 CardEffect.Execute()에 전달.
// ============================================================

using System.Collections.Generic;

public class CardContext
{
    public BattleState State;              // 순수 데이터 접근 (패시브·이펙트용)
    public BattleManager Battle;           // 이벤트 발화가 필요한 연산용 (AddCardToHand 등)
    public CardData Card;                  // 실행 중인 카드 (플레이어 행동 시), 적 행동이면 null
    public EnemyInstance ActingEnemy;      // 적이 시전자일 때 세팅; 플레이어 행동이면 null
    public EnemyInstance Target;           // 단일 대상용 (플레이어가 적을 타겟팅할 때)
    public List<EnemyInstance> AllEnemies; // 광역기용

    // 공격자를 ICombatant로 반환 — 플레이어 행동이면 Player, 적 행동이면 ActingEnemy
    public ICombatant Attacker => ActingEnemy ?? (ICombatant)State?.Player;
}
