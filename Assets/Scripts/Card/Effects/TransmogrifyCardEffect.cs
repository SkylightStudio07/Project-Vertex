using System;
using System.Collections.Generic;
using UnityEngine;

// 이벤트 선택지에서 덱의 카드를 다른 카드로 '변화'시키는 효과.
// 선택지를 누르는 즉시 카드 선택창이 뜬다(닫기 불가 — 선택지를 골랐으니 반드시 변화한다).
// 고른 카드는 같은 소유자의 획득 가능한 카드 중 무작위로 바뀐다(DeckManager.TransmogrifyCard).
// IInteractiveEventEffect라서 EventView는 변화가 끝날 때까지 결과 텍스트를 보류한다.
[Serializable]
public class TransmogrifyCardEffect : CardEffect, IInteractiveEventEffect
{
    [Tooltip("변화시킬 카드 수. 변화 가능한 카드가 그보다 적으면 있는 만큼만 진행한다.")]
    [Min(1)] public int count = 1;

    // EffectRunner로 일반 실행되면(이벤트 밖) 완료 통보 없이 흐름만 띄운다.
    public override void Execute(CardContext context) => Run(null);

    public void Run(Action onDone)
    {
        var deck = DeckManager.Instance;
        if (deck == null || CardListView.Instance == null || RunData.Instance == null)
        {
            Debug.LogWarning("[Transmogrify] DeckManager·CardListView·RunData 중 없는 것이 있어 카드 변화를 건너뜀.");
            onDone?.Invoke();
            return;
        }

        // 흐름 1회당 난수 하나 — 여러 장을 바꿔도 결과가 서로 다르고, 같은 노드에서는 같은 결과가 재현된다.
        var rng = RunRng.For(RngStream.Transmogrify, RunData.Instance.currentFloor, RunData.Instance.currentNodeIndex);
        Step(deck, rng, 1, onDone);
    }

    // 덱에 변화 가능한 카드(같은 소유자의 다른 획득 가능 카드가 있는 카드)가 하나라도 있는지. 선택지 조건에서도 쓴다.
    public static bool HasTransmogrifiableCard()
    {
        var deck = DeckManager.Instance;
        return deck != null && deck.PlayerDeck.Exists(c => CanTransmogrify(deck, c));
    }

    private static bool CanTransmogrify(DeckManager deck, CardData card)
        => card != null && deck.GetTransmogrifiableCards(card).Count > 0;

    private void Step(DeckManager deck, System.Random rng, int step, Action onDone)
    {
        if (step > count || !deck.PlayerDeck.Exists(c => CanTransmogrify(deck, c)))
        {
            CardListView.Instance?.Close();
            onDone?.Invoke();
            return;
        }

        string title = count > 1 ? $"변화시킬 카드를 선택하세요 ({step}/{count})" : "변화시킬 카드를 선택하세요";

        // 덱 전체를 보여주되 변화 후보가 있는 카드만 고를 수 있게 한다.
        // 목록은 복사본으로 넘긴다 — 변화하면서 덱 리스트가 바뀌기 때문.
        CardListView.Instance.OpenAsSelector(
            title,
            new List<CardData>(deck.PlayerDeck),
            onCardSelected: card => Confirm(deck, rng, card, step, onDone),
            selectableFilter: c => CanTransmogrify(deck, c),
            canClose: false,       // 선택창에는 취소 콜백이 없어서, 닫히면 onDone이 영영 안 불리고 이벤트가 멈춘다
            closeOnSelect: false); // 확인창에서 취소하면 목록으로 돌아와 다시 고른다
    }

    private void Confirm(DeckManager deck, System.Random rng, CardData card, int step, Action onDone)
    {
        void Apply()
        {
            CardListView.Instance?.Close();
            CardActionFx.Transmogrify(
                card,
                apply: () => deck.TransmogrifyCard(card, rng),
                onDone: () => Step(deck, rng, step + 1, onDone));
        }

        // 결과가 무작위라 미리보기 없이 고른 카드만 보여주고 확인받는다.
        if (CardDetailView.Instance != null)
            CardDetailView.Instance.ShowWithConfirmation(card, "변화", onConfirm: Apply);
        else
            Apply();
    }
}
