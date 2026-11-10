using System.Collections.Generic;
using UnityEngine;

// 카드로 적을 겨냥하는 동안, 그 카드를 쓰면 각 적의 HP가 얼마나 깎일지 HP바에 미리 보여준다.
// CardHandler가 겨냥 대상이 바뀔 때 Show, 겨냥이 끝나면 Clear를 부른다.
// 피해 계산은 EffectRunner.Preview(힘·취약 등 보정 반영)를 쓰고, 적 방어도를 먼저 깎는다.
// 광역 카드면 맞는 적 전부에 표시된다.
public static class DamagePreview
{
    private static readonly List<EnemyView> _shown = new();
    private static EnemyView _highlighted; // 특효 속성 칩을 켠 적

    public static void Show(CardData card, EnemyInstance target)
    {
        Clear();
        var battle = BattleManager.Instance;
        if (card == null || target == null || battle == null || battle.State == null) return;
        HighlightTraits(card, target);

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

    // 겨냥한 적의 속성 중 카드 특효에 걸리는 칩만 청록으로 (속성이 공개된 적만 — 칩이 '?'면 아무것도 켜지지 않는다)
    private static void HighlightTraits(CardData card, EnemyInstance target)
    {
        var traits = card.EffectiveTraitsOf(card.isUpgraded);
        if (traits == EnemyTrait.None || !MaterialArchive.TraitsRevealed(target.Data)) return;
        foreach (var view in Object.FindObjectsByType<EnemyView>(FindObjectsSortMode.None))
        {
            if (view.Instance != target) continue;
            view.HighlightTraits(traits);
            _highlighted = view;
        }
    }

    public static void Clear()
    {
        if (_highlighted != null) _highlighted.HighlightTraits(EnemyTrait.None);
        _highlighted = null;
        foreach (var view in _shown)
            if (view != null) view.ClearDamagePreview();
        _shown.Clear();
    }
}
