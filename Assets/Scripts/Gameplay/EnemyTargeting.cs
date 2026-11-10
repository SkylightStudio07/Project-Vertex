// ============================================================
// filename   : EnemyTargeting.cs
// 작성자     : -
// 작성일     : 2026-07-10
// description: 포인터 위치 아래의 적(EnemyInstance)을 UI 레이캐스트로
//              판정하는 공용 유틸. 카드/아이템 등 타겟 지정 UI가 공유한다.
//              (구 CardHandler.TryGetEnemyTarget 를 추출)
// ============================================================

using System.Collections.Generic;
using UnityEngine.EventSystems;

public static class EnemyTargeting
{
    // 포인터(eventData.position) 아래에서 살아있는 EnemyView를 찾아 그 EnemyInstance를 반환.
    // 죽은 적/EnemyView 없음/EventSystem 없음이면 false.
    public static bool TryGetUnderPointer(PointerEventData eventData, out EnemyInstance target)
    {
        target = null;
        if (eventData == null || EventSystem.current == null) return false;

        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, hits);

        foreach (RaycastResult hit in hits)
        {
            EnemyView enemyView = hit.gameObject.GetComponentInParent<EnemyView>();
            if (enemyView == null || enemyView.Instance == null || enemyView.Instance.IsDead)
                continue;
            if (IsProtected(enemyView.Instance, BattleManager.Instance != null ? BattleManager.Instance.State : null))
                continue;

            target = enemyView.Instance;
            return true;
        }

        return false;
    }

    // 다른 적이 살아 있어서 지금은 노릴 수 없는 적인가 ('포격 엄호' 상태)
    public static bool IsProtected(EnemyInstance enemy, BattleState state)
        => FindProtection(enemy) != null && HasLivingAlly(enemy, state);

    public static bool HasLivingAlly(EnemyInstance enemy, BattleState state)
    {
        if (enemy == null || state?.Enemies == null) return false;
        foreach (var other in state.Enemies)
            if (other != null && other != enemy && !other.IsDead) return true;
        return false;
    }

    // 혼자 남았으면 엄호 상태를 지운다 (아이콘도 함께 사라진다). 적이 쓰러질 때마다 BattleManager가 부른다.
    public static void ReleaseProtectionIfAlone(EnemyInstance enemy, BattleState state)
    {
        var status = FindProtection(enemy);
        if (status == null || HasLivingAlly(enemy, state)) return;
        status.RemoveAll();
        enemy.Statuses.RemoveExpired();
    }

    private static StatusInstance FindProtection(EnemyInstance enemy)
    {
        if (enemy == null) return null;
        foreach (var entry in enemy.Statuses.Entries)
        {
            if (entry is not StatusInstance status || status.Definition == null) continue;
            foreach (var behavior in status.Definition.Behaviors)
                if (behavior is ProtectedByAlliesStatusBehavior) return status;
        }
        return null;
    }
}
