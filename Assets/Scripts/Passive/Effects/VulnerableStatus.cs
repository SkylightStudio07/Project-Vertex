using UnityEngine;

// 취약: 피해를 1.5배 받음. 매 턴 스택 1 감소.
public class VulnerableStatus : StatusEffectBase
{
    public VulnerableStatus(int stacks) : base(stacks) { }

    public override DamageInfo ModifyIncomingDamage(DamageInfo info, BattleState state)
    {
        info.Amount = Mathf.RoundToInt(info.Amount * 1.5f);
        return info;
    }
}
