using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConditionalEffect : CardEffect
{
    [SerializeReference, SubclassPicker] public CardCondition condition;
    [SerializeReference, SubclassPicker] public List<CardEffect> effectsWhenMet = new();

    public override void Execute(CardContext context)
    {
        if (!IsConditionMet(context)) return;

        foreach (var effect in effectsWhenMet)
            effect?.Execute(context);
    }

    public override IEnumerator ExecuteCoroutine(CardContext context)
    {
        if (!IsConditionMet(context)) yield break;

        foreach (var effect in effectsWhenMet)
        {
            if (effect != null)
                yield return effect.ExecuteCoroutine(context);
        }
    }

    private bool IsConditionMet(CardContext context)
    {
        return condition != null && condition.IsMet(context);
    }
}
