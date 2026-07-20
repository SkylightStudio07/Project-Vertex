using System.Collections.Generic;
using UnityEngine;

public enum EnemyActivityPatternType
{
    Sequential, // 순차 실행
    Random      // 랜덤 실행
}

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game Asset/Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int health;
    public Sprite enemyImage;

    [Header("행동 패턴")]
    [Tooltip("행동 패턴 타입(랜덤/순차)")]
    public EnemyActivityPatternType activityPatternType;

    [Tooltip("전투 시작 시 순서대로 1회씩만 실행하는 오프닝 행동. 모두 소진되면 activityPatterns로 넘어간다.")]
    public List<EnemyAction> openingActions = new();

    [Tooltip("오프닝 이후 반복하는 행동 풀. Sequential이면 순서 순환, Random이면 매 턴 랜덤 1개.")]
    public List<EnemyAction> activityPatterns = new();
}
