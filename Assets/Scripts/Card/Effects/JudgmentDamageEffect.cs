[System.Serializable]
public class JudgmentDamageEffect : CardEffect
{
    public int baseDamage = 3;
    public int hitCount = 5;

    public override void Execute(CardContext context)
    {
        var player = context.State?.Player;
        if (player == null || player.IsDead) return;

        int gazeCount = 0;
        foreach (var passive in player.Passives)
        {
            if (passive is PressuredStatus gaze)
                gazeCount += gaze.Stacks;
        }

        int damagePerHit = gazeCount + baseDamage;
        for (int i = 0; i < hitCount && !player.IsDead; i++)
        {
            DamageCalculator.Resolve(
                new DamageInfo(damagePerHit, context.Attacker),
                player,
                context.State);
        }

        foreach (var passive in player.Passives)
        {
            if (passive is PressuredStatus gaze)
                gaze.ConsumeAll();
        }
        player.RemoveExpiredPassives();
    }
}
