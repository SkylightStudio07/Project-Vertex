using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Apply Status")]
public class ApplyStatusEffect : CardEffect
{
    public StatusType statusType;
    public int amount;
    public TargetType target;

    public override void Execute(CardContext context) { /* 차후 구현 */ }
}
