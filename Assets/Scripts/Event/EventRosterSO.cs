using System.Collections.Generic;
using UnityEngine;

// 챕터 하나의 이벤트 후보 목록. 씬(MapUIController)에 이벤트를 직접 박아두던 방식을 대체 —
// RewardProbabilityData와 동일하게 "챕터별 SO를 리스트로 들고, 런타임에 현재 챕터로 조회"하는 패턴.
[CreateAssetMenu(fileName = "EventRoster", menuName = "Game Asset/Event Roster")]
public class EventRosterSO : ScriptableObject
{
    public int chapter = 1;

    public List<EventRosterEntry> entries = new();

    // conditions 있는 엔트리가 전부 미충족이라 후보가 0개가 됐을 때 쓰는 무조건 등장 풀.
    // 여기 비어있으면 그 노드는 이벤트 없이 열림(EventView 미호출)으로 처리된다.
    public List<EventData> fallbackEvents = new();
}

// 이벤트 하나 + 등장 조건. conditions가 비어있으면 무조건 후보에 포함된다.
[System.Serializable]
public class EventRosterEntry
{
    public EventData eventData;
    [SerializeReference, SubclassPicker] public List<EventCondition> conditions = new();

    public bool IsEligible()
    {
        if (eventData == null) return false;
        if (conditions == null) return true;

        foreach (var condition in conditions)
        {
            // 슬롯은 늘렸지만 타입을 아직 안 고른 빈 조건은 "미충족"으로 취급 —
            // 설정이 덜 끝난 이벤트가 실수로 노출되는 것보다 안 뜨는 쪽이 안전하다.
            if (condition == null || !condition.IsMet()) return false;
        }
        return true;
    }
}
