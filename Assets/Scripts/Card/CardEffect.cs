// ============================================================
// 파일명    : CardEffect.cs
// 작성자    : SkylightStudio07 - 박영서
// 작성일    : 2026-04-12
// 설명      : 카드 효과의 기반 추상 클래스.
//             모든 개별 효과는 이 클래스를 상속하여 구현.
//             CardData의 effects 리스트에 SO 에셋으로 조합된다.
// 상속목록  : DamageEffect, BlockEffect, DrawEffect
// ============================================================
// 업데이트 로그
// ------------------------------------------------------------
// 2026-04-12 | SkylightStudio07 | 최초 작성
// ============================================================


using UnityEngine;

public abstract class CardEffect : ScriptableObject
{
    // 조금 고민을 해 봤는데... 결국 카드가 실행될 때 필요한 정보 처리 클래스가 필요했음. 그게 CardContext.
    // 예를 들어 대상, 광역기, 드로우, 전투 상태...
    // 이 쪽 프로퍼티는 좀 더 고민해봐야 한다. 데모에서 생각할 내용은 아닌 듯.
    public abstract void Execute(CardContext context);

    // 예를 들어서,
}

// 일단 드래프트. 각각 공격 / 방어 / 드로우.

[CreateAssetMenu(menuName = "Cards/Effects/Damage")]
public class DamageEffect : CardEffect
{
    public int amount;
    public int hitCount = 1;
    public override void Execute(CardContext context) {
    

    
    }
}

[CreateAssetMenu(menuName = "Cards/Effects/Block")]
public class BlockEffect : CardEffect
{
    public int amount;
    public override void Execute(CardContext context) { /* 차후 구현 */ }
}

[CreateAssetMenu(menuName = "Cards/Effects/Draw")]
public class DrawEffect : CardEffect
{
    public int count;
    public override void Execute(CardContext context) { /* 차후 구현 */ }
}

[CreateAssetMenu(menuName = "Cards/Effects/Apply Status")]
public class ApplyStatusEffect : CardEffect
{
    public StatusType statusType;
    public int amount;
    public TargetType target;  // Self, Enemy, AllEnemies

    public override void Execute(CardContext ctx)
    {
        
    }
}


/*
 
 추후 추가해야 할 요소.

 1. 디버프 범용 함수.
 2. 힐
 3. 그리고 파워카드 관련. 
 
 */


// 타게팅. 자기 자신/단일 적/전체 적/랜덤 적.
// 이걸... 여기서 처리하는 게 맞나 모르겠네. 그런데 여기 말고 달리 처리할 곳도 없음.
public enum TargetType { Self, SingleEnemy, AllEnemies, RandomEnemy }
public enum StatusType { Weak, Vulnerable, Poison, Strength, Dexterity }