using UnityEngine;

// 크레딧 강탈: 플레이어 크레딧을 빼앗아 시전자(적)에게 stolenStatus(강탈한 크레딧) 스택으로 쌓는다.
// 가진 만큼만 빼앗고, 실제로 빼앗은 양만 쌓인다. stolenStatus에 ReturnStolenGoldStatusBehavior가 붙어 있으면
// 그 적을 처치할 때 쌓인 만큼 돌려받는다. 단순 크레딧 차감(이벤트 비용 등)은 GainGoldEffect에 음수를 쓴다.
[System.Serializable]
public class StealGoldEffect : CardEffect
{
    [Min(0)] public int amount;
    [Tooltip("빼앗은 크레딧을 기록할 상태 (Status/StolenGold). 비우면 빼앗기만 하고 돌려주지 않는다")]
    public StatusDefinition stolenStatus;

    public override void Execute(CardContext context)
    {
        var game = GameManager.Instance;
        if (game == null || amount <= 0) return;

        int stolen = Mathf.Min(amount, game.PlayerGold);
        if (stolen <= 0) return;
        game.PlayerGold -= stolen;

        var thief = context?.Source;
        if (stolenStatus != null && thief != null && !thief.IsDead)
        {
            thief.AddPassive(stolenStatus.CreateInstance(stolen, source: thief));
            thief.Statuses.NotifyStatusGained(context, thief, stolenStatus, stolen);
        }
        Debug.Log($"[StealGold] 크레딧 {stolen} 강탈당함. 남은 크레딧: {game.PlayerGold}");
    }
}
