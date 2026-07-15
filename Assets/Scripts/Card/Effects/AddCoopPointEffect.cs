// 다이얼로그 선택지 등에서 협력자 호감도 포인트를 지급하는 효과.
[System.Serializable]
public class AddCoopPointEffect : CardEffect
{
    public string charID;
    public int amount;

    public override void Execute(CardContext context)
    {
        if (CooperationManager.Instance == null) return;
        CooperationManager.Instance.AddCoopPoint(charID, amount);
    }
}
