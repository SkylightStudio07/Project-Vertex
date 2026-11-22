using UnityEngine;

// '강탈한 크레딧' 상태용 규칙. 상태의 주인(적)이 처치되면 쌓인 스택만큼 플레이어 크레딧을 돌려준다.
// 스택은 StealGoldEffect가 실제로 빼앗은 양만큼 쌓는다. 사망 처리는 BattleManager가 개체당 한 번만 부른다.
[System.Serializable]
public sealed class ReturnStolenGoldStatusBehavior : StatusBehavior
{
    public override void OnDeath(StatusInstance status, CardContext context, ICombatant owner)
    {
        var game = GameManager.Instance;
        if (game == null || status == null || status.Stacks <= 0) return;

        game.PlayerGold += status.Stacks;
        Debug.Log($"[StealGold] 강탈당한 크레딧 {status.Stacks} 반환. 현재 크레딧: {game.PlayerGold}");
    }
}
