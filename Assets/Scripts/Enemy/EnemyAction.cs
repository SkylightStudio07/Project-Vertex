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
    public List<CardEffect> effects = new();
}
