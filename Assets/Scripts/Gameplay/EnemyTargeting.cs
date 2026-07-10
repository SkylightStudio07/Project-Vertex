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

            target = enemyView.Instance;
            return true;
        }

        return false;
    }
}
