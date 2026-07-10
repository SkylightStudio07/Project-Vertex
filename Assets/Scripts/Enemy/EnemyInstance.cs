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

    public event Action<int> OnDamaged;       // 실제 HP 감소량
    public event Action<int> OnBlockChanged;  // 현재 블록 수치
    public event Action      OnDied;
    public event Action      OnIntentChanged; // GetCurrentAction()이 가리키는 행동이 바뀜 (TakeTurn 후)
    public event Action      OnActionStarted; // 행동 실행 직전 발화 — EnemyView가 공격 모션 재생에 사용

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

    // Intent UI에서 공격 수치를 미리 보여주기 위한 헬퍼.
    // GetCurrentAction()의 effects 중 DamageEffect들을 합산한다 (강화/약화 등 패시브 보정은 반영하지 않음 — TODO).
    // DamageEffect가 없는 행동(방어/버프 등)이면 null 반환.
    public int? GetIntentDamageAmount()
    {
        var action = GetCurrentAction();
        if (action == null || action.effects == null) return null;

        int total = 0;
        bool found = false;
        foreach (var effect in action.effects)
        {
            if (effect is DamageEffect dmg)
            {
                total += dmg.amount * dmg.hitCount;
                found = true;
            }
        }
        return found ? total : null;
    }

    // 행동 실행 직전 알림. EnemyView가 공격 모션(전진→후퇴 등)을 트리거하는 데 사용.
    // BattleManager(다른 클래스)에서 이벤트를 직접 invoke할 수 없어서 래퍼로 노출.
    public void NotifyActionStarted() => OnActionStarted?.Invoke();

    // 현재 패턴의 효과를 실행하고 다음 패턴으로 진행한다.
    // TickPassives/생존 확인은 호출부(BattleManager의 코루틴 등) 책임.
    // battle은 CardContext.Battle을 채우기 위해서만 필요 (DrawEffect 등 일부 효과가 참조).
    public void ExecuteCurrentAction(BattleState state, BattleManager battle)
    {
        var action = GetCurrentAction();
        if (action != null && action.effects != null)
        {
            var ctx = new CardContext
            {
                State       = state,
                Battle      = battle,
                ActingEnemy = this,
                AllEnemies  = state.Enemies,
            };
            foreach (var effect in action.effects)
                if (effect != null) effect.Execute(ctx);
        }

        AdvancePattern();
        OnIntentChanged?.Invoke(); // 다음 턴에 보여줄 인텐트가 바뀌었으니 뷰 갱신 알림
    }

    // 패시브 틱 → 생존 확인 → 행동 실행을 한 번에 처리하는 동기 버전.
    // BattleManager는 이제 코루틴으로 단계별 호출하지만, 연출 없이 즉시 처리해야 하는
    // 테스트/디버그 코드를 위해 남겨둔다.
    // 반환값: 이번 턴에 실제로 행동을 실행했는지 여부.
    public bool TakeTurn(BattleState state, BattleManager battle)
    {
        if (IsDead) return false;

        TickPassives(state);
        if (IsDead) return false; // 패시브로 죽었으면 행동하지 않음

        NotifyActionStarted();
        ExecuteCurrentAction(state, battle);
        return true;
    }
}
