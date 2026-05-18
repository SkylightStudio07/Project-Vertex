// ============================================================
// filename   : EnemyInstance.cs
// description   : EnemyData SO의 런타임 래퍼.
//             전투 중 HP, 블록, 버프/디버프 상태를 보유.
//             퍼포먼스 문제 때문에 일부러 MonoBehaviour 상속 안 했으니까 수정 ㄴㄴ.
// ============================================================

using System;
using UnityEngine;

public class EnemyInstance
{
    public EnemyData Data { get; private set; }

    public int  HP      { get; private set; }
    public int  MaxHP   => Data.health;
    public int  Block   { get; private set; }
    public bool IsDead  => HP <= 0;

    // 적 스프라이트
    public Sprite EnemySprite { get; private set; }

    public EnemyStatusSet Status { get; private set; } = new();

    private int _patternIndex;

    public event Action<int> OnDamaged;   // 피해량
    public event Action      OnDied;

    public EnemyInstance(EnemyData data)
    {
        Data = data;
        HP   = data.health;
        EnemySprite = data.enemyImage;
    }

    public EnemyAction GetCurrentAction()
    {
        if (Data.activityPatterns == null || Data.activityPatterns.Count == 0)
            return null;

        return Data.activityPatterns[_patternIndex % Data.activityPatterns.Count];
    }

    public void AdvancePattern()
    {
        if (Data.activityPatterns != null && Data.activityPatterns.Count > 0)
            _patternIndex = (_patternIndex + 1) % Data.activityPatterns.Count;
    }
}

// 적 상태이상 컨테이너 (추후 필요 항목 추가)
public class EnemyStatusSet
{
    public int strength;    // 힘 — 공격 피해 증가
    public int weak;        // 약화 (턴 수)
    public int vulnerable;  // 취약 (턴 수)
    public int burn;        // 화상 스택
}
