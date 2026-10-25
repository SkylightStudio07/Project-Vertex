// 소멸 더미에서 지정한 카드(같은 기본 이름) 한 장을 손패로 되돌린다. 손패가 가득이면 버린 더미로.
// 예: 회수 — 던진 "전술 개량형 투척 창술"을 다시 손에 쥔다.
[System.Serializable]
public class ReturnExhaustedCardEffect : CardEffect
{
    public CardData card;

    public override void Execute(CardContext context)
    {
        var state = context?.State;
        if (card == null || state == null) return;
        var found = state.ExhaustPile.Find(c => c != null && c.BaseCardName == card.BaseCardName);
        if (found == null) return;
        state.ExhaustPile.Remove(found);
        if (context.Battle != null) context.Battle.ReturnCardToHand(found);
        else state.Hand.Add(found);
    }
}
