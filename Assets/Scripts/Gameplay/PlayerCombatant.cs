using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 전투 내 플레이어를 ICombatant로 표현하는 래퍼.
// HP/MaxHP는 런 영속 데이터인 GameManager에서 읽고,
// Block·Passives는 전투 스코프에서 관리한다.
public class PlayerCombatant : ICombatant
{
    private int _block;
    private readonly List<IPassiveLogic> _passives = new();

    public int HP     => GameManager.Instance.PlayerHP;
    public int MaxHP  => GameManager.Instance.MaxPlayerHP;
    public int Block  => _block;
    public bool IsDead => HP <= 0;
    public List<IPassiveLogic> Passives => _passives;

    public event Action<int> OnDamaged;
    public event Action<int> OnBlocked; // 블록으로 흡수한 데미지량. ResetBlock()으로 0이 되는 것과는 구분된 신호.
    public event Action      OnDied;

    public void TakeDamage(DamageInfo info)
    {
        int amount = info.Amount;

        if (!info.IsPiercing)
        {
            int absorbed = Math.Min(_block, amount);
            if (absorbed > 0)
            {
                _block -= absorbed;
                amount -= absorbed;
                OnBlocked?.Invoke(absorbed);
            }
        }

        if (amount > 0)
            GameManager.Instance.TakeDamage(amount);

        OnDamaged?.Invoke(amount);
        if (IsDead) OnDied?.Invoke();
    }

    // 민첩(DexterityStatus)은 데미지 파이프라인이 아니라 방어도 획득 시점에 보정되므로 여기서 직접 반영한다.
    // 음수 민첩으로 획득량이 음수가 되어도 기존 방어도까지 깎이지는 않도록 Max(0)로 하한 처리.
    public void AddBlock(int amount)
    {
        foreach (var p in _passives)
            if (p is DexterityStatus dex) amount += dex.Stacks;
        _block = Math.Max(0, _block + amount);
    }

    public void ResetBlock() => _block = 0;

    // 패시브 추가.
    // StatusEffectBase면 같은 타입과 스택 합산 시도.
    // PowerPassiveBase(영구 파워)면 merge 없이 직접 추가.
    public void AddPassive(IPassiveLogic passive)
    {
        if (passive is StatusEffectBase newStatus)
        {
            foreach (var p in _passives)
            {
                if (p is StatusEffectBase existing && existing.TryMerge(newStatus))
                    return;
            }
        }
        _passives.Add(passive);
    }

    public void TickPassives(BattleState state)
    {
        foreach (var p in _passives)
            p.OnTurnStart(state, this);

        for (int i = _passives.Count - 1; i >= 0; i--)
        {
            if (_passives[i] is StatusEffectBase s)
            {
                s.TickDown();
            }
        }

        RemoveExpiredPassives();
    }

    public void RemoveExpiredPassives()
    {
        for (int i = _passives.Count - 1; i >= 0; i--)
        {
            if (_passives[i] is StatusEffectBase s && s.IsExpired)
                _passives.RemoveAt(i);
        }
    }

#if UNITY_EDITOR
    public List<string> DebugPassiveInfo => _passives
        .Select(p => p is StatusEffectBase s
            ? $"{p.GetType().Name} (x{s.Stacks})"
            : p.GetType().Name)
        .ToList();
#endif
}
