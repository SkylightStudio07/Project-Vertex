using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyActivityPattern
{
    public ActivityType activityType; // 행동 종류 

    [Header("Action Details")]
    public float actionAmount; // 공격 수치 또는 방어 수치
    public int actionCount;    // 공격 횟수
}

public enum ActivityType
{
    Attack,
    Defend,
    Wait
}

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game Asset/Enemy")]
public class EnemyData : ScriptableObject
{
    // 프로퍼티 생각.
    public string enemyName;
    public int health;
    public Sprite enemyImage;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }



}
