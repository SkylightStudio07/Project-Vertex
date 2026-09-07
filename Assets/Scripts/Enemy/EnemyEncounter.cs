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
}