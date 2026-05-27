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

    private int patternIndex;

    public event Action<int> OnDamaged;      // 실제 HP 감소량
    public event Action<int> OnBlockChanged; // 현재 블록 수치
    public event Action      OnDied;

    public EnemyInstance(EnemyData data)
    {
        Data = data;
        HP   = data.health;
        EnemySprite = data.enemyImage;
    }

    // 블록 흡수 후 vulnerable 계산, HP 감소.
    public void TakeDamage(int rawAmount)
    {
        if (IsDead || rawAmount <= 0) return;

        int amount = Status.vulnerable > 0
            ? Mathf.FloorToInt(rawAmount * 1.5f)
            : rawAmount;

        int absorbed = Mathf.Min(Block, amount);
        Block -= absorbed;
        if (absorbed > 0)
            OnBlockChanged?.Invoke(Block);

        int remaining = amount - absorbed;
        if (remaining > 0)
        {
            HP = Mathf.Max(0, HP - remaining);
            OnDamaged?.Invoke(remaining);
        }

        if (IsDead)
            OnDied?.Invoke();
    }

    public void AddBlock(int amount)
    {
        if (amount <= 0) return;
        Block += amount;
        OnBlockChanged?.Invoke(Block);
    }

    // 적 턴 시작 시 블록 초기화.
    public void ResetBlock()
    {
        if (Block == 0) return;
        Block = 0;
        OnBlockChanged?.Invoke(Block);
    }

    public void ApplyStatus(StatusType type, int amount)
    {
        switch (type)
        {
            case StatusType.Strength:   Status.strength   += amount; break;
            case StatusType.Weak:       Status.weak        = Mathf.Max(0, Status.weak       + amount); break;
            case StatusType.Vulnerable: Status.vulnerable  = Mathf.Max(0, Status.vulnerable + amount); break;
        }
    }

    // 적 턴 종료 시 상태이상 턴 감소.
    public void TickStatus()
    {
        if (Status.weak       > 0) Status.weak--;
        if (Status.vulnerable > 0) Status.vulnerable--;
    }

    public EnemyAction GetCurrentAction()
    {
        if (Data.activityPatterns == null || Data.activityPatterns.Count == 0)
            return null;

        return Data.activityPatterns[patternIndex % Data.activityPatterns.Count];
    }

    public void AdvancePattern()
    {
        if (Data.activityPatterns != null && Data.activityPatterns.Count > 0)
            patternIndex = (patternIndex + 1) % Data.activityPatterns.Count;
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
