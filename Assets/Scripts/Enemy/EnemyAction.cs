using System.Collections.Generic;
using UnityEngine;

public enum IntentType
{
    Attack,
    Defend,
    Buff,
    Debuff,
    Wait
}

[CreateAssetMenu(fileName = "EnemyAction", menuName = "Game Asset/Enemy Action")]
public class EnemyAction : ScriptableObject
{
    [Header("인텐트 UI")]
    public IntentType intentType;

    [Header("실행 효과")]
    // 카드와 동일한 CardEffect를 인라인으로 조합. EnemyAction SO 자체는 유지되므로
    // 여러 적이 같은 행동 에셋을 공유하는 것은 여전히 가능하다.
    [SerializeReference, SubclassPicker] public List<CardEffect> effects = new();
}
