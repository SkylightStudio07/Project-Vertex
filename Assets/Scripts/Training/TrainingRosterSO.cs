using System.Collections.Generic;
using UnityEngine;

// 훈련장에서 고를 수 있는 조우 목록. 씬(TrainingView)에 조우를 직접 박아두던 방식을 대체 —
// 새 적을 훈련장에 넣을 때 로비 씬을 건드리지 않고 이 에셋만 고치면 된다.
// 일반 조우는 적 단위로 풀어 직접 편성하고, 엘리트 · 보스는 조우 그대로 보여준다.
[CreateAssetMenu(fileName = "TrainingRoster", menuName = "Game Asset/Training Roster")]
public class TrainingRosterSO : ScriptableObject
{
    public List<EnemyEncounter> encounters = new();
}
