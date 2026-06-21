using UnityEngine;

// 약화: 가하는 피해가 0.75배. 매 턴 스택 1 감소.
public class WeakStatus : StatusEffectBase
{
    public WeakStatus(int stacks) : base(stacks) { }

    public override DamageInfo ModifyOutgoingDamage(DamageInfo info, BattleState state)
    {
        info.Amount = Mathf.RoundToInt(info.Amount * 0.75f);
        return info;
    }
}
