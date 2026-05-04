using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Damage")]
public class DamageEffect : CardEffect
{
    public int amount;
    public int hitCount = 1;

    public override void Execute(CardContext context)
    {
        // 일단 단일 대상용으로 구현. 광역기는 나중에.
        if (context.Target != null)
        {

        }
    }
}
