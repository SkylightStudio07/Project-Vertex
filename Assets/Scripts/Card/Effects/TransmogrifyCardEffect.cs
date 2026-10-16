using System;
using UnityEngine;

// 이벤트 선택지에서 덱의 카드를 다른 카드로 '변화'시키는 효과.
// 고른 카드는 같은 소유자의 획득 가능한 카드 중 무작위로 바뀐다(DeckManager.TransmogrifyCard).
// 선택창·확인창·반복 흐름은 DeckCardSelectEffect 참고. 결과가 무작위라 확인창에는 고른 카드만 보여준다.
[Serializable]
public class TransmogrifyCardEffect : DeckCardSelectEffect
{
    // 흐름 1회당 난수 하나 — 여러 장을 바꿔도 결과가 서로 다르고, 같은 노드에서는 같은 결과가 재현된다.
    [NonSerialized] private System.Random _rng;

    protected override string Verb => "변화";

    protected override bool OnFlowStart()
    {
        if (RunData.Instance == null) return false;
        _rng = RunRng.For(RngStream.Transmogrify, RunData.Instance.currentFloor, RunData.Instance.currentNodeIndex);
        return true;
    }

    protected override bool CanApply(DeckManager deck, CardData card) => CanTransmogrify(deck, card);

    protected override void Apply(DeckManager deck, CardData card, Action onDone)
        => CardActionFx.Transmogrify(card, apply: () => deck.TransmogrifyCard(card, _rng), onDone: onDone);

    // 덱에 변화 가능한 카드(같은 소유자의 다른 획득 가능 카드가 있는 카드)가 하나라도 있는지. 선택지 조건에서도 쓴다.
    public static bool HasTransmogrifiableCard()
    {
        var deck = DeckManager.Instance;
        return deck != null && deck.PlayerDeck.Exists(c => CanTransmogrify(deck, c));
    }

    private static bool CanTransmogrify(DeckManager deck, CardData card)
        => card != null && deck.GetTransmogrifiableCards(card).Count > 0;
}
