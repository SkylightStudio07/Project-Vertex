using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyActivityPattern
{
    public ActivityType activityType;
    public int actionAmount;  // 공격/방어 수치
    public int actionCount;   // 공격 횟수 (연타용)
}

public enum ActivityType
{
    Attack,
    Defend,
    Wait,
    Buff,
    Debuff
}

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game Asset/Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int health;
    public Sprite enemyImage;

    [Header("행동 패턴")]
    public List<EnemyActivityPattern> activityPatterns = new();
}
