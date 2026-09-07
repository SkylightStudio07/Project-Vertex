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
    public BattleState State;              // 순수 데이터 접근 (패시브·이펙트용)
    public BattleManager Battle;           // 이벤트 발화가 필요한 연산용 (AddCardToHand 등)
    public CardData Card;                  // 실행 중인 카드 (플레이어 행동 시), 적 행동이면 null
    public ItemData Item;                  // 실행 중인 아이템 (플레이어 행동 시), 카드 행동이면 null
    public EnemyInstance ActingEnemy;      // 적이 시전자일 때 세팅; 플레이어 행동이면 null
    public EnemyInstance Target;           // 단일 대상용 (플레이어가 적을 타겟팅할 때)
    public List<EnemyInstance> AllEnemies; // 광역기용

    // 신규 효과 시스템의 진영 중립 필드. 레거시 필드는 마이그레이션 호환을 위해 유지한다.
    public ICombatant SourceOverride;
    public ICombatant PrimaryTargetOverride;
    public int ExecutionDepth;

    // 패시브가 CardEffect를 발사할 때 채워지는 이벤트 정보.
    // 일반 카드/아이템/적 행동에서는 null/0이다.
    public IPassiveLogic TriggeringPassive;
    public int ActualDamage;
    public StatusInstance TriggeringStatus => TriggeringPassive as StatusInstance;

    // 공격자를 ICombatant로 반환 — 플레이어 행동이면 Player, 적 행동이면 ActingEnemy
    public ICombatant Source => SourceOverride ?? ActingEnemy ?? (ICombatant)State?.Player;
    public ICombatant PrimaryTarget
    {
        get
        {
            // 패시브 사건에는 상대가 없을 수 있음.
            if (TriggeringPassive != null) return PrimaryTargetOverride;
            if (PrimaryTargetOverride != null) return PrimaryTargetOverride;
            if (Target != null) return Target;
            return ActingEnemy != null ? State?.Player : null;
        }
    }
    public ICombatant Attacker => Source;

    public static CardContext CreatePassiveContext(
        BattleState state,
        ICombatant owner,
        ICombatant primaryTarget,
        IPassiveLogic passive,
        int actualDamage = 0,
        CardContext parent = null)
    {
        state ??= parent?.State;
        return new CardContext
        {
            State = state,
            Battle = parent?.Battle ?? BattleManager.Instance,
            Card = parent?.Card,
            Item = parent?.Item,
            ActingEnemy = owner as EnemyInstance,
            Target = primaryTarget as EnemyInstance,
            AllEnemies = state?.Enemies ?? parent?.AllEnemies,
            SourceOverride = owner,
            PrimaryTargetOverride = primaryTarget,
            TriggeringPassive = passive,
            ActualDamage = actualDamage,
            ExecutionDepth = parent?.ExecutionDepth ?? 0,
        };
    }
}
