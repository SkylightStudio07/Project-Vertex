using System.Collections;
using UnityEngine;

[System.Serializable]
public class DamageEffect : CardEffect
{
    public int amount;
    public EffectValue scaling;
    public int hitCount = 1;
    public EffectTargetSelector targets = EffectTargetSelector.PrimaryTarget;
    public bool piercing;
    public bool environmentalDamage;
    public float hitInterval = 0.08f;

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

        int baseValue = rawValue;
        if (scaling.source != EffectValueSource.None)
        {
            var previewContext = new CardContext
            {
                State = state,
                Card = card,
                Target = target,
                AllEnemies = state.Enemies,
            };
            baseValue += scaling.Evaluate(previewContext, target);
        }

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
        => amount + scaling.Evaluate(context, target);

    private void DealDamage(CardContext context, ICombatant target)
    {
        int resolvedAmount = GetRawAmount(context, target);
        if (resolvedAmount <= 0) return;
        bool isAmmoAttack = context.Card != null && context.Card.AmmoCost > 0;
        DamageCalculator.Resolve(
            new DamageInfo(resolvedAmount, environmentalDamage ? null : context.Source, piercing, isAmmoAttack),
            target,
            context.State,
            context);
    }
}
