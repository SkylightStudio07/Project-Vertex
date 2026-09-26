using System.Collections.Generic;
using UnityEngine;

// 카드로 적을 겨냥하는 동안, 그 카드를 쓰면 각 적의 HP가 얼마나 깎일지 HP바에 미리 보여준다.
// CardHandler가 겨냥 대상이 바뀔 때 Show, 겨냥이 끝나면 Clear를 부른다.
// 피해 계산은 EffectRunner.Preview(힘·취약 등 보정 반영)를 쓰고, 적 방어도를 먼저 깎는다.
// 광역 카드면 맞는 적 전부에 표시된다.
public static class DamagePreview
{
    private static readonly List<EnemyView> _shown = new();

    public static void Show(CardData card, EnemyInstance target)
    {
        Clear();
        var battle = BattleManager.Instance;
        if (card == null || target == null || battle == null || battle.State == null) return;

        var context = new CardContext
        {
            State      = battle.State,
            Battle     = battle,
            Card       = card,
            Target     = target,
            AllEnemies = battle.State.Enemies,
        };
        var preview = EffectRunner.Preview(card.ActiveEffects, context);
        if (!preview.HasDamage) return;

        foreach (var view in Object.FindObjectsByType<EnemyView>(FindObjectsSortMode.None))
        {
            var enemy = view.Instance;
            if (enemy == null || enemy.IsDead) continue;
            int loss = preview.GetHpLoss(enemy, enemy.Block);
            if (loss <= 0) continue;
            view.ShowDamagePreview(loss);
            _shown.Add(view);
        }
    }

    public static void Clear()
    {
        foreach (var view in _shown)
            if (view != null) view.ClearDamagePreview();
        _shown.Clear();
    }
}
