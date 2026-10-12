using System;
using UnityEngine;

// 이벤트 선택지에서 덱의 카드를 골라 '강화'하는 효과.
// 강화 상태가 작성된 미강화 카드만 고를 수 있다(CardData.CanUpgrade — 휴식 노드와 같은 기준).
// 선택창·확인창·반복 흐름은 DeckCardSelectEffect 참고. 강화는 결과를 알 수 있으니 확인창에 강화 후 모습을 미리 보여준다.
[Serializable]
public class UpgradeCardEffect : DeckCardSelectEffect
{
    protected override string Verb => "강화";

    protected override bool CanApply(DeckManager deck, CardData card) => card != null && card.CanUpgrade;

    // 임시 복사본을 강화 상태로 띄운다(휴식 노드와 같은 방식). 원본은 건드리지 않으므로 취소해도 덱은 그대로다.
    protected override void Confirm(CardData card, Action onConfirm)
    {
        if (CardDetailView.Instance == null)
        {
            onConfirm();
            return;
        }

        var preview = UnityEngine.Object.Instantiate(card);
        preview.isUpgraded = true;
        CardDetailView.Instance.ShowWithConfirmation(
            preview, Verb,
            onConfirm: () => { UnityEngine.Object.Destroy(preview); onConfirm(); },
            onCancel:  () => UnityEngine.Object.Destroy(preview));
    }

    // 망치 연출 — 두 번째 망치질 순간 실제로 강화된다.
    protected override void Apply(DeckManager deck, CardData card, Action onDone)
        => CardActionFx.Upgrade(card, () => deck.UpgradeCard(card), onDone);

    // 덱에 강화할 수 있는 카드가 하나라도 있는지. 선택지 조건에서 쓴다.
    public static bool HasUpgradableCard()
    {
        var deck = DeckManager.Instance;
        return deck != null && deck.GetUpgradableCards().Count > 0;
    }
}
