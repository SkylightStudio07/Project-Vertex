using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeTypeWeight // 노드 타입에 따른 가중치. 승천마다 다르게 세팅해야 할 가능성도?
{
    public NodeType nodeType;
    [Min(0)] public float weight;   // 0이면 해당 타입은 등장하지 않아야 함... 일단은.
}

// 특정 층에 반드시 등장해야 하는 노드 타입 지정
[System.Serializable]
public class FloorGuarantee // 층별 고정 노드. 
// 보스 마지막 층은 휴식 노드 보장, 2, 3층은 성소 보장.
{
    public int floorIndex;      // 0층부터 시작하는 거 주의
    public NodeType nodeType;
}


// Inspector에서 조정하는 맵 생성 파라미터 모음.
// MapGenerator 세팅값이라고 생각하자.
[CreateAssetMenu(fileName = "MapConfig", menuName = "Game Asset/Map Config")]
public class MapConfig : ScriptableObject
{
    [Header("층 설정")]
    public int totalFloors = 16;            // 보스 층 포함 총 층 수

    [Header("층당 노드 수")]
    [Range(2, 5)] public int minNodesPerFloor = 3;
    [Range(2, 5)] public int maxNodesPerFloor = 5;

    [Header("노드 타입 가중치")]
    // 가중치가 높을수록 해당 타입이 더 자주 등장한다.
    public List<NodeTypeWeight> nodeTypeWeights = new()
    {
        new NodeTypeWeight { nodeType = NodeType.Combat, weight = 1f },
        new NodeTypeWeight { nodeType = NodeType.Elite, weight = 0.3f },
        new NodeTypeWeight { nodeType = NodeType.Sanctuary, weight = 0.1f },
        new NodeTypeWeight { nodeType = NodeType.Treasure_Box, weight = 0.1f },
        new NodeTypeWeight { nodeType = NodeType.Event, weight = 0.4f },
        new NodeTypeWeight { nodeType = NodeType.Shop, weight = 0.1f },
        new NodeTypeWeight { nodeType = NodeType.Rest, weight = 0.1f },
    };

    [Header("층별 고정 노드 설정(보스, 성소, 보물상자)")]
    // 지정한 층에 해당 타입의 노드를 반드시 1개 배치.
    // 성소, 보물상자, 보스 전 휴식 노드는 고정임.
    public List<FloorGuarantee> guaranteedNodes = new()
    {
        new FloorGuarantee { floorIndex = 0, nodeType = NodeType.Blessing }, // 1층 (0-based), 시작은 축복 노드로 고정
        new FloorGuarantee { floorIndex = 1, nodeType = NodeType.Combat },  // 2층 (0-based)
        new FloorGuarantee { floorIndex = 2, nodeType = NodeType.Sanctuary },  // 3층 (0-based)
        new FloorGuarantee { floorIndex = 9, nodeType = NodeType.Treasure_Box },  // 10층 (0-based)
        new FloorGuarantee { floorIndex = 14, nodeType = NodeType.Rest },  // 15층 (0-based), 보스전 직전 휴식 보장
        new FloorGuarantee { floorIndex = 15, nodeType = NodeType.Boss },  // 16층 (0-based), 보스 고정
    };
}

