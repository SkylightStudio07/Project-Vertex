using UnityEngine;

// R&C 컴퍼니 스피드로더 카드용. 매 턴 탄약을 추가로 지급하는 영구 패시브를 등록한다.
// 파워 카드라 전투 내내 유지 — 해제 로직 없음 (PowerPassiveBase 참고).
[CreateAssetMenu(menuName = "Cards/Effects/AmmoPerTurnPower")]
public class AmmoPerTurnPowerEffect : CardEffect
{
    public int ammoPerTurn = 1;

    public override void Execute(CardContext ctx)
    {
        if (ctx.State?.Player == null) return;
        ctx.State.Player.AddPassive(new AmmoPerTurnPassive(ammoPerTurn));
    }
}
