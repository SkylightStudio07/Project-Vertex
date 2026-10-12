using System;
using System.Collections.Generic;
using UnityEngine;

// 덱에서 카드를 골라 무언가를 하는 이벤트 효과(변화·강화 등)의 공통 흐름.
//   선택지 클릭 → 선택창(닫기 불가, CanApply인 카드만 선택 가능) → 확인창 → 연출과 함께 적용 → count장 반복 → onDone
// IInteractiveEventEffect라서 EventView는 흐름이 끝날 때까지 결과 텍스트를 보류한다.
// 하위 클래스는 동사·선택 조건·적용(연출 포함)만 정하고, 필요하면 확인창(미리보기)과 시작 준비를 재정의한다.
// 추상 클래스라 SubclassPicker 목록에는 나오지 않는다.
[Serializable]
public abstract class DeckCardSelectEffect : CardEffect, IInteractiveEventEffect
{
    [Tooltip("처리할 카드 수. 가능한 카드가 그보다 적으면 있는 만큼만 진행한다.")]
    [Min(1)] public int count = 1;

    // 선택창 제목과 확인 버튼에 쓰는 동사 (예: "변화", "강화")
    protected abstract string Verb { get; }

    // 이 카드를 고를 수 있는지
    protected abstract bool CanApply(DeckManager deck, CardData card);

    // 연출과 함께 실제로 적용한다. 끝나면 onDone을 반드시 불러야 다음 장(또는 결과 텍스트)으로 넘어간다.
    protected abstract void Apply(DeckManager deck, CardData card, Action onDone);

    // 흐름 시작 시 한 번(난수 준비 등). false면 흐름을 건너뛴다.
    protected virtual bool OnFlowStart() => true;

    // 고른 카드를 확인받는다. 기본은 고른 카드만 보여준다 — 결과를 미리 보여줄 수 있으면 재정의.
    protected virtual void Confirm(CardData card, Action onConfirm)
    {
        if (CardDetailView.Instance != null)
            CardDetailView.Instance.ShowWithConfirmation(card, Verb, onConfirm: onConfirm);
        else
            onConfirm();
    }

    // EffectRunner로 일반 실행되면(이벤트 밖) 완료 통보 없이 흐름만 띄운다.
    public override void Execute(CardContext context) => Run(null);

    public void Run(Action onDone)
    {
        var deck = DeckManager.Instance;
        if (deck == null || CardListView.Instance == null || !OnFlowStart())
        {
            Debug.LogWarning($"[{GetType().Name}] 필요한 매니저가 없어 카드 {Verb}을(를) 건너뜀.");
            onDone?.Invoke();
            return;
        }
        Step(deck, 1, onDone);
    }

    private void Step(DeckManager deck, int step, Action onDone)
    {
        if (step > count || !deck.PlayerDeck.Exists(c => c != null && CanApply(deck, c)))
        {
            CardListView.Instance?.Close();
            onDone?.Invoke();
            return;
        }

        string title = count > 1 ? $"{Verb}할 카드를 선택하세요 ({step}/{count})" : $"{Verb}할 카드를 선택하세요";

        // 덱 전체를 보여주되 처리 가능한 카드만 고를 수 있게 한다.
        // 목록은 복사본으로 넘긴다 — 적용하면서 덱 리스트가 바뀔 수 있기 때문(변화).
        CardListView.Instance.OpenAsSelector(
            title,
            new List<CardData>(deck.PlayerDeck),
            onCardSelected: card => Confirm(card, () =>
            {
                CardListView.Instance?.Close();
                Apply(deck, card, () => Step(deck, step + 1, onDone));
            }),
            selectableFilter: c => CanApply(deck, c),
            canClose: false,       // 선택창에는 취소 콜백이 없어서, 닫히면 onDone이 영영 안 불리고 이벤트가 멈춘다
            closeOnSelect: false); // 확인창에서 취소하면 목록으로 돌아와 다시 고른다
    }
}
