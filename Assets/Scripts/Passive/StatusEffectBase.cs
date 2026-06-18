using System;

public abstract class StatusEffectBase : IPassiveLogic
{
    public int Stacks { get; protected set; }
    public bool IsExpired => Stacks <= 0;

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

    // 매 턴 끝에 스택 1 감소 (Strength처럼 영구 효과는 override해서 막을 것)
    public virtual void TickDown() => Stacks = Math.Max(0, Stacks - 1);

    // 기본 구현은 no-op — 자식이 필요한 것만 override
    public virtual void OnBattleStart(BattleState state, ICombatant owner) { }
    public virtual void OnTurnStart(BattleState state, ICombatant owner) { }
    public virtual void OnCardPlayed(CardContext ctx, ICombatant owner) { }
    public virtual DamageInfo ModifyOutgoingDamage(DamageInfo info, BattleState state) => info;
    public virtual DamageInfo ModifyIncomingDamage(DamageInfo info, BattleState state) => info;
    public virtual void OnAfterDamageTaken(int actualDamage, BattleState state, ICombatant owner) { }
}
