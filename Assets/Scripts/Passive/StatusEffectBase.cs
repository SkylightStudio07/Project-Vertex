using System;

public abstract class StatusEffectBase : IPassiveLogic
{
    public int Stacks { get; protected set; }

    // 만료 판정 — 기본은 "0 이하면 만료" (시한부 상태는 TickDown이 0에서 멈추므로 사실상 == 0).
    // 민첩·힘처럼 음수 스택이 유효한(감소 상태) 영구 패시브는 == 0으로 override할 것 —
    // 안 하면 음수가 되는 순간 TickPassives 정리 루프에서 즉시 제거된다.
    public virtual bool IsExpired => Stacks <= 0;

    protected StatusEffectBase(int stacks)
    {
        Stacks = stacks;
    }

    // 같은 타입의 패시브가 이미 있으면 스택 합산 후 true 반환
    public bool TryMerge(StatusEffectBase other)
    {
        if (other.GetType() != GetType()) return false;
        Stacks += other.Stacks;
        return true;
    }

    public void ReduceMagnitude(int amount)
    {
        if (amount <= 0) return;

        if (Stacks > 0)
            Stacks = Math.Max(0, Stacks - amount);
        else if (Stacks < 0)
            Stacks = Math.Min(0, Stacks + amount);
    }

    public void SetMagnitude(int amount)
    {
        amount = Math.Max(0, amount);

        if (Stacks > 0)
            Stacks = amount;
        else if (Stacks < 0)
            Stacks = -amount;
    }

    public void AddMagnitude(int amount)
    {
        if (amount == 0) return;

        if (Stacks > 0)
            Stacks = Math.Max(0, Stacks + amount);
        else if (Stacks < 0)
            Stacks = Math.Min(0, Stacks - amount);
    }

    public void MultiplyMagnitude(int multiplier)
    {
        if (multiplier < 0) return;

        if (Stacks > 0)
            Stacks *= multiplier;
        else if (Stacks < 0)
            Stacks *= multiplier;
    }

    // 매 턴 끝에 스택 1 감소 (Strength처럼 영구 효과는 override해서 막을 것)
    public virtual void TickDown() => Stacks = Math.Max(0, Stacks - 1);

    // 기본 구현은 no-op — 자식이 필요한 것만 override
    public virtual void OnBattleStart(BattleState state, ICombatant owner) { }
    public virtual void OnTurnStart(BattleState state, ICombatant owner) { }
    public virtual void OnCardPlayed(CardContext ctx, ICombatant owner) { }
    public virtual DamageInfo ModifyOutgoingDamage(DamageInfo info, BattleState state) => info;
    public virtual DamageInfo ModifyIncomingDamage(DamageInfo info, BattleState state) => info;
    public virtual void OnAfterDamageTaken(int actualDamage, BattleState state, ICombatant owner) { }
    public virtual void OnAfterDamageDealt(int actualDamage, ICombatant target, BattleState state, ICombatant owner) { }

    // 표시용 미리보기 — 기본은 Modify 위임.
    // Modify에서 내부 상태를 소모하는 패시브는 반드시 소모 없는 버전으로 override할 것.
    public virtual DamageInfo PreviewOutgoingDamage(DamageInfo info, BattleState state)
        => ModifyOutgoingDamage(info, state);
    public virtual DamageInfo PreviewIncomingDamage(DamageInfo info, BattleState state)
        => ModifyIncomingDamage(info, state);
}
