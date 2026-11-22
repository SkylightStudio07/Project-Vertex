[System.Serializable]
public class DrawEffect : CardEffect
{
    public int count;
    [UnityEngine.Tooltip("count에 더할 장수 (예: 발동한 상태의 스택만큼 — 끝없는 굴레 중첩). 비우면 count만 뽑는다")]
    public EffectValue scaling;

    public override void Execute(CardContext context)
    {
        int total = count + scaling.Evaluate(context);
        if (total > 0) context?.Battle?.DrawCards(total);
    }
}
