using System.Collections.Generic;
using UnityEngine;


// 특정 층에 반드시 등장해야 하는 노드 타입 지정
[System.Serializable]
public class FloorGuarantee // 층별 고정 노드. 
// 보스 마지막 층은 휴식 노드 보장, 2, 3층은 성소 보장.
{
    public int floorIndex;      // 0층부터 시작하는 거 주의
    public NodeType nodeType;
}

// 특정 타입이 몇 층부터 등장할 수 있는지 지정
[System.Serializable]
public class NodeTypeMinFloor
{
    public NodeType nodeType;
    public int minFloorIndex;   // 이 층(0-based)부터 등장 가능. 그보다 아래 층에서는 후보에서 아예 빠진다.
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
    // 가중치가 높을수록 해당 타입이 더 자주 등장한다. 층마다 배치 규칙(최소 등장 층 등)을 통과한 타입끼리 비율로 뽑는다.
    // 슬더스 1막 비율(전투 53 / 이벤트 22 / 엘리트 8 / 휴식 12 / 상점 5)을 기준으로 하되,
    // 우리 맵은 층당 노드가 3~5개라(슬더스 7열) 노드 총량이 적어서 상점·엘리트를 조금 올렸다.
    // 1000판 시뮬레이션 기준: 상점 맵당 3.3개(상점 없는 판 2%), 엘리트 맵당 3.2개(한 경로 최대 약 2개, 거의 다 회피 가능).
    // 보물상자(Treasure_Box)는 폐기된 기획이라 넣지 않는다.
    public List<NodeTypeWeight> nodeTypeWeights = new()
    {
        new NodeTypeWeight { nodeType = NodeType.Combat,    weight = 0.48f },
        new NodeTypeWeight { nodeType = NodeType.Event,     weight = 0.22f },
        new NodeTypeWeight { nodeType = NodeType.Elite,     weight = 0.10f },
        new NodeTypeWeight { nodeType = NodeType.Rest,      weight = 0.12f },
        new NodeTypeWeight { nodeType = NodeType.Shop,      weight = 0.08f },
        new NodeTypeWeight { nodeType = NodeType.Sanctuary, weight = 0.02f },
    };

    [Header("노드 배치 규칙")]
    // 지정한 층 이전에는 해당 타입이 아예 후보에서 빠진다.
    // floorIndex 기준(0-based)이라 "표시상 6층부터" = minFloorIndex 5 다.
    // 목록에 없는 타입은 제한 없음(0층부터 가능).
    public List<NodeTypeMinFloor> minFloorRules = new()
    {
        new NodeTypeMinFloor { nodeType = NodeType.Elite, minFloorIndex = 5 }, // 표시상 6층부터
        new NodeTypeMinFloor { nodeType = NodeType.Rest,  minFloorIndex = 5 }, // 표시상 6층부터
    };

    // 경로상 부모 노드와 같은 타입이 되지 않게 막을 타입 목록.
    // 층 단위가 아니라 "경로" 단위 제약이다. 휴식 노드에 서 있으면 다음 선택지에 휴식이 안 뜬다.
    // 전투/이벤트는 여기 안 넣는다. 연속으로 나와도 상관없는 타입이라.
    public List<NodeType> noConsecutiveTypes = new()
    {
        NodeType.Elite, NodeType.Rest, NodeType.Shop,
    };

    // 같은 부모를 공유하는 형제 노드끼리 같은 타입이 되지 않게 한다.
    // 선택지가 "휴식 vs 휴식"처럼 무의미해지는 걸 막는 용도.
    // noConsecutiveTypes에 올라간 타입에만 적용된다. 끄고 싶으면 false.
    public bool forbidSiblingDuplicates = true;

    // minFloorRules 조회용. 목록에 없으면 0(제한 없음)을 돌려준다.
    public int GetMinFloorIndex(NodeType type)
    {
        foreach (var rule in minFloorRules)
            if (rule.nodeType == type) return rule.minFloorIndex;
        return 0;
    }

    // minNodesPerFloor가 maxNodesPerFloor보다 값 안넘게 보정.
    private void OnValidate()
    {
        if (minNodesPerFloor > maxNodesPerFloor)
            minNodesPerFloor = maxNodesPerFloor;
    }

    [Header("조우 풀 (런 시작 시 소비형 큐로 생성 — EncounterQueueBuilder 참고)")]
    // 노드에 적을 박아두는 게 아니라, 런 시작 때 이 풀들에서 "전투 순서" 큐를 미리 뽑아
    // 전투 진입마다 앞에서 하나씩 소비한다. "어느 노드냐"가 아니라 "몇 번째 전투냐"로 조우가 정해짐.
    public List<EnemyEncounter> normalEncounterPool = new();
    public List<EnemyEncounter> eliteEncounterPool = new();
    public List<EnemyEncounter> bossEncounterPool = new();

    [Header("초반 약한 적 (weak-first)")]
    // 런 초반 weakEncounterCount번의 일반 전투는 이 약한 풀에서만 뽑아 초반 난이도를 완화한다(슬더스식).
    // 이후 전투부터 normalEncounterPool로 넘어간다. 비워두면 처음부터 normalEncounterPool을 쓴다.
    public List<EnemyEncounter> weakEncounterPool = new();
    [Min(0)] public int weakEncounterCount = 3;

    [Header("층별 고정 노드 설정(보스, 성소, 보물상자)")]
    // 지정한 층에 해당 타입의 노드를 반드시 1개 배치.
    // 성소, 보스 전 휴식 노드는 고정임.
    public List<FloorGuarantee> guaranteedNodes = new()
    {
        new FloorGuarantee { floorIndex = 0, nodeType = NodeType.Blessing }, // 1층 (0-based), 시작은 축복 노드로 고정
        new FloorGuarantee { floorIndex = 1, nodeType = NodeType.Combat },  // 2층 (0-based)
        new FloorGuarantee { floorIndex = 2, nodeType = NodeType.Sanctuary },  // 3층 (0-based)
        new FloorGuarantee { floorIndex = 14, nodeType = NodeType.Rest },  // 15층 (0-based), 보스전 직전 휴식 보장
        new FloorGuarantee { floorIndex = 15, nodeType = NodeType.Boss },  // 16층 (0-based), 보스 고정
    };
}

