// 손패에 남은 상태이상 카드가 턴 종료 시 주는 피해. 일반 공격의 힘 보정은 받지 않는다.
[System.Serializable]
public sealed class DamageAtEndTurnInHandEffect : CardEffect, ICardEndTurnInHandEffect
{
    public int amount = 5;
    public bool piercing;

    public override void Execute(CardContext context) { }

    public bool OnTurnEndInHand(CardContext context)
    {
        var player = context?.State?.Player;
        if (player != null && !player.IsDead && amount > 0)
            player.TakeDamage(new DamageInfo(amount, null, piercing));
        return false;
    }
}
