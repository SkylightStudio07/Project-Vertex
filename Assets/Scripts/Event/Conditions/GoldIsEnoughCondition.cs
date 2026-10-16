// 골드가 충분해야 만족하는 조건.
// 골드가 부족할 때 그 선택지가 잠긴다.
[System.Serializable]
public class GoldIsEnoughCondition : EventCondition
{
    public int requiredGold;
    public override bool IsMet()
    {
        if (GameManager.Instance == null) return false;
        return GameManager.Instance.PlayerGold >= requiredGold;
    }
}
