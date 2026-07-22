// 이벤트 선택지 등에서 협력자를 파티에 즉시 합류시키는 효과.
// CooperationManager.SelectChar()를 그대로 호출 — 성소에서 합류할 때와 동일하게
// 합류 보상 카드/카드 풀 반영까지 처리된다.
[System.Serializable]
public class JoinCompanionEffect : CardEffect
{
    public string charID;

    public override void Execute(CardContext context)
    {
        if (CooperationManager.Instance == null) return;
        CooperationManager.Instance.SelectChar(charID);
    }
}
