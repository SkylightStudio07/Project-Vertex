using System.Collections;
using UnityEngine;

// 손패에서 카드를 골라 버린다. 선택은 손패 UI에서 직접 클릭(HandCardSelector).
// 여러 장일 때는 한 장씩 고르고 즉시 버리는 방식 — 버린 카드가 손패에서 바로 빠지므로
// 중복 선택 처리가 따로 필요 없다.
// 선택 입력을 기다려야 하므로 카드 사용 경로(TryPlayCard → ExecuteCoroutine)에서만 동작한다.
[System.Serializable]
public class DiscardEffect : CardEffect
{
    public int count = 1;

    // 동기 경로(아이템·적 행동·테스트 등)에서는 선택 UI를 띄울 수 없다.
    public override void Execute(CardContext context)
    {
        Debug.LogWarning("[DiscardEffect] 동기 경로에서는 손패 선택을 할 수 없어 무시됨. 카드 효과로만 사용할 것.");
    }

    public override IEnumerator ExecuteCoroutine(CardContext ctx)
    {
        var battle = ctx.Battle;
        if (battle == null || HandCardSelector.Instance == null)
        {
            Debug.LogWarning("[DiscardEffect] BattleManager 또는 HandCardSelector 없음 — 효과 무시. 씬 배치 확인 필요.");
            yield break;
        }

        for (int i = 0; i < count; i++)
        {
            if (battle.Hand.Count == 0) yield break;

            string prompt = count > 1
                ? $"버릴 카드를 선택하세요 ({i + 1}/{count})"
                : "버릴 카드를 선택하세요";

            CardData chosen = null;
            // 버린 카드는 손패에서 즉시 빠지므로 exclude는 필요 없다
            yield return HandCardSelector.Instance.SelectOne(prompt, card => chosen = card);

            if (chosen == null) yield break;
            battle.DiscardCardFromHand(chosen);
        }
    }
}
