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
    [Tooltip("기본 의도와 함께 표시할 추가 의도. 같은 종류는 한 번만 표시합니다.")]
    public List<IntentType> additionalIntents = new();
    [Tooltip("적 턴에 행동할 때 인텐트 자리에 띄우는 행동 이름. 비우면 에셋 이름을 쓴다")]
    public string displayName;

    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

    [Header("실행 효과")]
    // 카드와 동일한 CardEffect를 인라인으로 조합. EnemyAction SO 자체는 유지되므로
    // 여러 적이 같은 행동 에셋을 공유하는 것은 여전히 가능하다.
    [SerializeReference, SubclassPicker] public List<CardEffect> effects = new();
}
