// 덱에 강화할 수 있는 카드가 하나라도 있어야 만족하는 조건.
// 카드 강화(UpgradeCardEffect) 선택지에 걸어두면, 강화할 카드가 없을 때 그 선택지가 잠긴다.
[System.Serializable]
public class HasUpgradableCardCondition : EventCondition
{
    public override bool IsMet() => UpgradeCardEffect.HasUpgradableCard();
}
