using System.Collections.Generic;
using UnityEngine;

// 맵 생성 알고리즘. 인스턴스 불필요 → static class.
// MapConfig 세팅값을 읽어 MapData를 생성해 반환한다.
public static class MapGenerator
{
    // 시드 없이 호출하면 랜덤 시드로 생성
    public static MapData Generate(MapConfig config)
    {
        int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        return Generate(config, seed);
    }

    public static MapData Generate(MapConfig config, int seed)
    {
        // 시드로 랜덤 초기화
        UnityEngine.Random.InitState(seed);
        // 맵 데이터 컨테이너 생성
        MapData mapData = new MapData(seed, config.totalFloors);

        /*
        config.guaranteedNodes를 먼저 처리해서 보장 노드들을 배치.
        
        guaranteedNodes 현재 형식 :
        [{ floorIndex: 2, nodeType: Sanctuary }, { floorIndex: 9, nodeType: Treasure_Box }, ...]
        
        나중에 조회할 거 생각하면 이건 Dictionary<int, NodeType> 형태로 바꿔도 될 듯. 층 인덱스 -> 보장 노드 타입 매핑 식으로.

        */

        var guaranteeMap = new Dictionary<int, List<NodeType>>();

        foreach (var guarantee in config.guaranteedNodes)
        {

            // 만약 층에 보장 노드가 여러 개 인 경우를 상정했음.
            // 당연히 기획서에 그런 내용은 없음! 현재로서는 예외처리에 가까운 내용이지만 
            if (!guaranteeMap.ContainsKey(guarantee.floorIndex)) // 같은 층에 키가 이미 존재하지 않는 경우. 그러니까 통상적인 경우
                guaranteeMap[guarantee.floorIndex] = new List<NodeType>(); // 층에 대한 빈 리스트 생성

            guaranteeMap[guarantee.floorIndex].Add(guarantee.nodeType); 
            // 이게 문제의 보장 노드 여러개인 경우. 이 경우는 사실 누군가 MapConfig를 잘못 만졌다고 봐야 함.
        }
        /* 층별 노드 수 결정 
        
        0, 15층은 고정 노드 1개만 있으니까 따로 처리.
        
        */

        // floor = 0일때는 첫 시작 노드.

        mapData.floors[0].Add(new MapNode
        {
            nodeType = NodeType.Blessing,
            floorIndex = 0,
            nodeIndex = 0
        });


        // floor = config.totalFloors - 1일때는 보스 노드 고정.

        mapData.floors[config.totalFloors - 1].Add(new MapNode
        {
            nodeType = NodeType.Boss,
            floorIndex = config.totalFloors - 1,
            nodeIndex = 0
        });


        // 시작 노드(축복), 끝 노드(보스) 제외
        for (int floor = 1; floor < config.totalFloors - 1; floor++)
        {
            int nodesToGenerate = UnityEngine.Random.Range(config.minNodesPerFloor, config.maxNodesPerFloor + 1);

            // 보장 노드가 있는 층은 보장 노드만 존재. 보장 노드 수가 nodesToGenerate보다 많을 경우는 예외적으로 보장 노드 수만큼만 생성.
            if (guaranteeMap.ContainsKey(floor))
            {
                foreach (var guaranteedNodeType in guaranteeMap[floor])
                {
                    mapData.floors[floor].Add(new MapNode
                    {
                        nodeType = guaranteedNodeType,
                        floorIndex = floor,
                        nodeIndex = mapData.floors[floor].Count // 현재 층에 이미 추가된 노드 수를 인덱스로 사용
                    });
                }
            }
            else // 보장 노드 없는 층은 일반적으로 노드 생성
            {
                for (int i = 0; i < nodesToGenerate; i++)
                {
                    NodeType randomNodeType = GetRandomNodeType(config.nodeTypeWeights);
                    mapData.floors[floor].Add(new MapNode
                    {
                        nodeType = randomNodeType,
                        floorIndex = floor,
                        nodeIndex = i
                    });
                }
            }

            /* 

            여기서부터는 노드 간선 연결 관련 정리.

            1. 선이 교차하면 안 됨 (UX상 혼란)
            2. 다음 층 노드 중 연결 안 된 노드가 없어야 함 (고립 방지)
            3. 각 노드는 다음 층으로 최소 1개 연결
            4. 각 노드는 다음 층으로 최대 2개 연결 (너무 복잡해지는 거 방지)
            5. 노드 간선은 층별로 랜덤하게 배치 (매번 다른 맵이 나오도록)

            */

            

        }
            
        
        



        return mapData;
    }

    private static NodeType GetRandomNodeType(List<NodeTypeWeight> nodeTypeWeights)
    {
        // 가중치 기반 노드 타입 선택 로직
        // ntw : nodeTypeWeights 리스트의 각 요소. 
        // NodeType과 weight를 가지고 있음.

        float totalWeight = 0f;
        foreach (var ntw in nodeTypeWeights)
            totalWeight += ntw.weight;

        // 0부터 totalWeight 사이의 랜덤 값 생성

        float randomValue = UnityEngine.Random.Range(0, totalWeight);
        
        // cumulativeWeight는 nodeTypeWeights 리스트를 순회하면서 각 노드 타입의 weight를 누적해서 더해가는 변수.
        
        float cumulativeWeight = 0f;

        // randomValue가 누적 가중치 범위 내에 들어오는 첫 번째 노드 타입을 반환
        foreach (var ntw in nodeTypeWeights)
        {
            cumulativeWeight += ntw.weight;
            if (randomValue <= cumulativeWeight)
                return ntw.nodeType;
        }

        // 혹시나 해서 기본값 반환 (여기까지 오면 사고임)
        return NodeType.Combat;
    }
}
