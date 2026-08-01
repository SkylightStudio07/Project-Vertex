using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TriggerEffectsOnTargetHpLossEffect : CardEffect
{
    [SerializeReference, SubclassPicker] public List<CardEffect> effectsOnHpLoss = new();

    public override void Execute(CardContext context)
    {
        var target = context.Target;
        if (target == null || target.IsDead) return;

        void HandleDamaged(int actualDamage)
        {
            if (actualDamage <= 0) return;

            var triggerContext = new CardContext
            {
                State = context.State,
                Battle = context.Battle,
                Card = context.Card,
                Target = target,
                AllEnemies = context.AllEnemies,
            };

            foreach (var effect in effectsOnHpLoss)
                effect?.Execute(triggerContext);
        }

        void HandleDied()
        {
            target.OnDamaged -= HandleDamaged;
            target.OnDied -= HandleDied;
        }

        target.OnDamaged += HandleDamaged;
        target.OnDied += HandleDied;
    }
}
