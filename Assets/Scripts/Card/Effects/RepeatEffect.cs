using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class RepeatEffect : CardEffect
{
    [Min(0)] public int count = 1;
    [SerializeReference, SubclassPicker] public List<CardEffect> effects = new();

    public override void Execute(CardContext context)
    {
        for (int i = 0; i < count; i++)
            EffectRunner.ExecuteImmediate(effects, context);
    }

    public override IEnumerator ExecuteCoroutine(CardContext context)
    {
        for (int i = 0; i < count; i++)
            yield return EffectRunner.ExecuteSequence(effects, context);
    }
}

