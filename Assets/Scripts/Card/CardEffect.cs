// ============================================================
// filename   : CardEffect.cs
// 작성자    : SkylightStudio07 - 박영서
// 작성일    : 2026-04-12
// description   : 카드 효과의 기반 추상 클래스.
//             모든 개별 효과는 이 클래스를 상속하여 구현.
//             [SerializeReference]로 CardData/EnemyAction 등의 effects 리스트에
//             인라인 직렬화된다 — 별도 SO 에셋 없이 카드 인스펙터에서 직접 수치 편집.
// ============================================================
// 업데이트 로그
// ------------------------------------------------------------
// 2026-04-12 | SkylightStudio07 | 최초 작성
// 2026-07-11 | 박영서 | ScriptableObject → 순수 클래스 전환.
// 효과 SO가 수치 조합마다 에셋으로 늘어나는 문제(피해5 SO, 피해6 SO...)를 해소.
// 주의: [SerializeReference]는 클래스 이름을 에셋 YAML에 문자열로 저장하므로
// 이펙트 클래스 리네임 시 기존 데이터가 깨진다. 리네임이 불가피하면 [MovedFrom] 사용할 것.
// ============================================================

using System.Collections;

[System.Serializable]
public abstract class CardEffect
{
    public abstract void Execute(CardContext context);

    // 기본 구현은 Execute()를 동기 호출하고 끝낸다.
    // 연타처럼 히트 사이에 딜레이가 필요한 이펙트만 오버라이드한다.
    public virtual IEnumerator ExecuteCoroutine(CardContext context)
    {
        Execute(context);
        yield break;
    }
}

// 타게팅. 자기 자신/단일 적/전체 적/랜덤 적.
public enum TargetType { Self, SingleEnemy, AllEnemies, RandomEnemy }
public enum StatusType { Weak, Vulnerable, Poison, Strength, Dexterity, DamageNullified }

// 플레이어 덱의 어느 더미에 카드를 넣을지.
public enum PileType { DrawPile, DiscardPile, Hand }
