// ============================================================
// filename   : EnemyInstance.cs
// description   : EnemyData SO의 런타임 래퍼.
//             전투 중 HP, 블록, 패시브(상태이상 포함)를 보유.
//             퍼포먼스 문제 때문에 일부러 MonoBehaviour 상속 안 했으니까 수정 ㄴㄴ.
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInstance : ICombatant
{
    public EnemyData Data { get; private set; }

    private int _hp;
    private int _block;
    private readonly List<IPassiveLogic> _passives = new();
    private int _patternIndex;

    public int  HP     => _hp;
    public int  MaxHP  => Data.health;
    public int  Block  => _block;
    public bool IsDead => _hp <= 0;
    public List<IPassiveLogic> Passives => _passives;

    public Sprite EnemySprite { get; private set; }

    public event Action<int> OnDamaged;      // 실제 HP 감소량
    public event Action<int> OnBlockChanged; // 현재 블록 수치
    public event Action      OnDied;

    public EnemyInstance(EnemyData data)
    {
        Data        = data;
        _hp         = data.health;
        EnemySprite = data.enemyImage;
    }

    // 블록 흡수 → HP 감소. 패시브 배율은 DamageCalculator가 호출 전에 이미 적용함.
    public void TakeDamage(DamageInfo info)
    {
        if (IsDead || info.Amount <= 0) return;

        int amount = info.Amount;

        if (!info.IsPiercing)
        {
            int absorbed = Math.Min(_block, amount);
            if (absorbed > 0)
            {
                _block -= absorbed;
                amount -= absorbed;
                OnBlockChanged?.Invoke(_block);
            }
        }

        if (amount > 0)
        {
            _hp = Math.Max(0, _hp - amount);
            OnDamaged?.Invoke(amount);
        }

        if (IsDead)
            OnDied?.Invoke();
    }

    public void AddBlock(int amount)
    {
        if (amount <= 0) return;
        _block += amount;
        OnBlockChanged?.Invoke(_block);
    }

    public void ResetBlock()
    {
        if (_block == 0) return;
        _block = 0;
        OnBlockChanged?.Invoke(_block);
    }

    // 패시브 추가 — 같은 타입이면 스택 합산
    public void AddPassive(StatusEffectBase passive)
    {
        foreach (var p in _passives)
        {
            if (p is StatusEffectBase existing && existing.TryMerge(passive))
                return;
        }
        _passives.Add(passive);
    }

    // 적 턴 시작: 패시브 OnTurnStart 호출 후 임시 패시브 스택 감소
    public void TickPassives(BattleState state)
    {
        foreach (var p in _passives)
            p.OnTurnStart(state, this);

        for (int i = _passives.Count - 1; i >= 0; i--)
        {
            if (_passives[i] is StatusEffectBase s)
            {
                s.TickDown();
                if (s.IsExpired) _passives.RemoveAt(i);
            }
        }
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
