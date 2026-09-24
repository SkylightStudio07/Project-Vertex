// 런 덱(DeckManager.PlayerDeck)에 카드를 영구 추가하는 효과.
// 전투 컨텍스트 없이 동작하므로 이벤트 선택지 등 비전투 상황에서 쓴다.
// 전투 중 더미(뽑을/버린 카드/손패)에 넣는 건 AddCardToPileEffect 참고.
[System.Serializable]
public class AddCardToDeckEffect : CardEffect
{
    public CardData card;
    public int count = 1;

    public override void Execute(CardContext context)
    {
        if (DeckManager.Instance == null || card == null) return;
        for (int i = 0; i < count; i++)
            DeckManager.Instance.AddCardToPlayerDeck(card);
    }
}
