// ============================================================
// filename   : CardEffect.cs
// 작성자    : SkylightStudio07 - 박영서
// 작성일    : 2026-04-12
// description   : 카드 효과의 기반 추상 클래스.
//             모든 개별 효과는 이 클래스를 상속하여 구현.
//             CardData의 effects 리스트에 SO 에셋으로 조합된다.
// ============================================================
// 업데이트 로그
// ------------------------------------------------------------
// 2026-04-12 | SkylightStudio07 | 최초 작성
// ============================================================

using UnityEngine;

public abstract class CardEffect : ScriptableObject
{
    public abstract void Execute(CardContext context);
}

// 타게팅. 자기 자신/단일 적/전체 적/랜덤 적.
public enum TargetType { Self, SingleEnemy, AllEnemies, RandomEnemy }
public enum StatusType { Weak, Vulnerable, Poison, Strength, Dexterity }

// 플레이어 덱의 어느 더미에 카드를 넣을지.
public enum PileType { DrawPile, DiscardPile, Hand }
