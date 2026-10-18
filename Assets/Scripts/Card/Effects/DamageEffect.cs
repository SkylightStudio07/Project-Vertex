using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DamageEffect : CardEffect
{
    public int amount;
    public EffectValue scaling;
    [Tooltip("조건을 만족하는 증가량을 기본 피해에 더한 뒤 한 타격으로 처리합니다.")]
    public List<ConditionalDamageBonus> conditionalBonuses = new();
    public int hitCount = 1;
    public EffectTargetSelector targets = EffectTargetSelector.PrimaryTarget;
    public bool piercing;
    public bool environmentalDamage;
    public float hitInterval = 0.08f;
    [SerializeReference, SubclassPicker] public List<CardEffect> onKillEffects = new();
    [Tooltip("특효 속성. 대상이 이 중 하나라도 가지면 피해에 특효 배율을 한 번 곱한다 (카드 키워드 \"○○ 특효\"로 표시)")]
    public EnemyTrait effectiveAgainst;
    public const float EffectiveMultiplier = 1.2f; // 특효 배율 (기획: 2할)

    public override void Execute(CardContext context)
    {
        if (context?.State == null) return;
        foreach (var target in targets.Resolve(context))
            for (int i = 0; i < hitCount && !target.IsDead; i++)
                DealDamage(context, target);
    }

    public override IEnumerator ExecuteCoroutine(CardContext context)
    {
        if (hitCount <= 1 || hitInterval <= 0f)
        {
            Execute(context);
            yield break;
        }
        if (context?.State == null) yield break;

        var delay = new WaitForSeconds(hitInterval);
        foreach (var target in targets.Resolve(context))
        {
            for (int i = 0; i < hitCount && !target.IsDead; i++)
            {
                DealDamage(context, target);
                if (i < hitCount - 1 && !target.IsDead) yield return delay;
            }
        }
    }

    public override int GetDisplayValue(string fieldName, int rawValue, BattleState state, CardData card, EnemyInstance target = null)
    {
        if (fieldName != nameof(amount) || state?.Player == null) return rawValue;

        var previewContext = new CardContext
            {
                State = state,
                Card = card,
                Target = target,
                AllEnemies = state.Enemies,
            }.CreateAmmoPreview();
        int baseValue = GetRawAmount(previewContext, target);

        bool isAmmoAttack = card != null && card.AmmoCost > 0;
        var info = new DamageInfo(baseValue, state.Player, piercing, isAmmoAttack);
        foreach (var passive in state.Player.Passives)
            info = passive.PreviewOutgoingDamage(info, state);

        if (target != null)
            foreach (var passive in target.Passives)
                info = passive.PreviewIncomingDamage(info, state);

        return info.Amount;
    }

    public int GetRawAmount(CardContext context, ICombatant target)
    {
        int result = amount + scaling.Evaluate(context, target) + AmmoStatusEvents.DamageBonus(context);
        if (conditionalBonuses != null)
            foreach (var bonus in conditionalBonuses)
                if (bonus?.condition != null && bonus.condition.IsMet(context)) result += bonus.amount;
        if (result > 0 && EnemyTraits.HasAny(target, effectiveAgainst))
            result = Mathf.FloorToInt(result * EffectiveMultiplier);
        return result;
    }

    private void DealDamage(CardContext context, ICombatant target)
    {
        int resolvedAmount = GetRawAmount(context, target);
        if (resolvedAmount <= 0) return;
        bool isAmmoAttack = context.Card != null && context.Card.AmmoCost > 0;
        bool killed = DamageCalculator.Resolve(
            new DamageInfo(resolvedAmount, environmentalDamage ? null : context.Source, piercing, isAmmoAttack),
            target,
            context.State,
            context);
        if (killed) EffectRunner.ExecuteImmediate(onKillEffects, context);
    }
}

[System.Serializable]
public class ConditionalDamageBonus
{
    [SerializeReference, SubclassPicker] public CardCondition condition;
    public int amount;
}
