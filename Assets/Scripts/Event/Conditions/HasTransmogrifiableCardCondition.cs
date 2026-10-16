// 덱에 변화시킬 수 있는 카드가 하나라도 있어야 만족하는 조건.
// 카드 변화(TransmogrifyCardEffect) 선택지에 걸어두면, 변화할 카드가 없을 때 그 선택지가 잠긴다.
[System.Serializable]
public class HasTransmogrifiableCardCondition : EventCondition
{
    public override bool IsMet() => TransmogrifyCardEffect.HasTransmogrifiableCard();
}
