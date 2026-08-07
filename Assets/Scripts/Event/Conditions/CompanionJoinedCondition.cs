// 지정한 협력자가 이번 런에 합류해 있어야 등장하는 조건.
// 예) 이카루스 군이 합류해야만 뜨는 2막 이카루스 군 패시브 강화 이벤트.
[System.Serializable]
public class CompanionJoinedCondition : EventCondition
{
    public string charID;

    public override bool IsMet()
    {
        if (CooperationManager.Instance == null) return false;
        return CooperationManager.Instance.IsJoinedInRun(charID);
    }
}
