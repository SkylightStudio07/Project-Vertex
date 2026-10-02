using System.Collections.Generic;
using UnityEngine;

public enum EnemyEncounterType
{
    Normal,
    Elite,
    Boss
}

[System.Serializable]
public class NodeTypeWeight // 노드 타입에 따른 가중치. 승천마다 다르게 세팅해야 할 가능성도?
{
    public NodeType nodeType;
    [Min(0)] public float weight;   // 0이면 해당 타입은 등장하지 않아야 함... 일단은.
}

[System.Serializable, CreateAssetMenu(fileName = "EnemyEncounter", menuName = "Game Asset/Create EnemyEncounterData", order = 1)]
public class EnemyEncounter : ScriptableObject
{
    public int chapter = 1;
    public EnemyEncounterType encounterType;
    public List<EnemyData> enemies = new();
    [Min(0f)] public float weight = 1f;

    [Header("보스 연출 (encounterType이 Boss일 때)")]
    [Tooltip("등장 배너 윗줄 (예: 1막 보스). 비우면 'N막 보스'")]
    public string bossLabel;
    [Tooltip("등장 배너 큰 글씨. 비우면 첫 번째 적 이름")]
    public string bossTitle;
    [Tooltip("등장 대사 후보. 스토리 플래그 조건을 위에서부터 검사해 처음 맞는 하나를 재생 (없으면 대사 없이 시작)")]
    public List<StoryDialogue> introDialogues = new();
}