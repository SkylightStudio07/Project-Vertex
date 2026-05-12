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
        UnityEngine.Random.InitState(seed);
        MapData mapData = new MapData(seed, config.totalFloors);

        // guaranteedNodes 리스트  { 층 인덱스 / 고정 타입 목록 } 딕셔너리화
        var guaranteeMap = new Dictionary<int, List<NodeType>>();


        // 혹여 중복 보장 노드가 있을 수 있으니 층별로 리스트에 추가하는 방식으로 처리.
        // 사실 이 경우는 그냥 기획서상 오류에 가까워서 예외처리 개념으로 접근한다.

        foreach (var guarantee in config.guaranteedNodes)
        {
            if (!guaranteeMap.ContainsKey(guarantee.floorIndex))
                guaranteeMap[guarantee.floorIndex] = new List<NodeType>();
            guaranteeMap[guarantee.floorIndex].Add(guarantee.nodeType);
        }

        // ── Step 1: 각 층에 노드 배치 ──────────────────────────────────────

        // 0층: Blessing 1개, 가운데 열 고정
        mapData.floors[0].Add(new MapNode
        {
            nodeType   = NodeType.Blessing,
            floorIndex = 0,
            nodeIndex  = 0,
            column     = config.maxNodesPerFloor / 2
        });

        // 마지막 층: Boss 1개, 가운데 열 고정
        int lastFloor = config.totalFloors - 1;
        mapData.floors[lastFloor].Add(new MapNode
        {
            nodeType   = NodeType.Boss,
            floorIndex = lastFloor,
            nodeIndex  = 0,
            column     = config.maxNodesPerFloor / 2
        });

        // 1층 ~ (totalFloors-2)층 채우는 로직
        for (int f = 1; f < config.totalFloors - 1; f++)
        {

            // 차피 타입은 지정되어 있으니(List<NodeType>) var로 하고 보장 노드가 있는 층인지 여부만 bool로 처리한다.

            bool hasGuarantee = guaranteeMap.TryGetValue(f, out var guarantees);

            List<NodeType> types; // 이 층에 배치할 노드 타입 목록. 보장 노드가 있으면 그 타입들, 없으면 가중치 추첨으로 채움
            int nodeCount; // 이 층에 배치할 노드 수. 보장 노드가 있으면 그 수만큼, 없으면 랜덤 결정

            if (hasGuarantee)
            {
                // 보장 노드가 있는 층: 보장 노드만 배치
                types     = new List<NodeType>(guarantees);
                nodeCount = guarantees.Count;
            }
            else
            {
                // 일반 층: 노드 수 랜덤 결정 후 가중치 추첨으로 타입 채우기
                nodeCount = UnityEngine.Random.Range(config.minNodesPerFloor, config.maxNodesPerFloor + 1);
                types     = new List<NodeType>();
                for (int i = 0; i < nodeCount; i++)
                    types.Add(GetRandomNodeType(config.nodeTypeWeights));
            }

            // 열 위치 랜덤 배정: 0 ~ maxNodesPerFloor-1 슬롯 중 nodeCount개 선택 후 정렬
            var columns = PickColumns(config.maxNodesPerFloor, nodeCount);

            // 노드 생성 및 층에 추가
            for (int n = 0; n < nodeCount; n++)
            {
                mapData.floors[f].Add(new MapNode
                {
                    nodeType   = types[n],
                    floorIndex = f,
                    nodeIndex  = n,
                    column     = columns[n]
                });
            }
        }

        // ── Step 2: 층 간 연결 ─────────────────────────────────────────────
        for (int f = 0; f < config.totalFloors - 1; f++)
            ConnectFloors(mapData.floors[f], mapData.floors[f + 1]);

        return mapData;
    }

    // ── 내부 헬퍼 ─────────────────────────────────────────────────────────

    // 슬롯 0 ~ maxNodes-1 중 count개를 랜덤 선택, 정렬해서 반환 (왼→오 순서 보장)
    private static List<int> PickColumns(int maxNodes, int count)
    {
        var slots = new List<int>();
        for (int i = 0; i < maxNodes; i++) slots.Add(i);
        Shuffle(slots);
        var picked = slots.GetRange(0, count);
        picked.Sort();
        return picked;
    }

    // 현재 층 → 다음 층 연결.
    // |현재 column - 다음 column| <= 1 인 노드끼리만 연결 -> 교차 못하게...
    private static void ConnectFloors(List<MapNode> current, List<MapNode> next)
    {
        var reachedNext = new HashSet<int>(); // 연결된 다음 층 노드 인덱스 추적

        foreach (var curr in current)
        {
            // 이 노드와 열이 인접한 다음 층 노드 목록
            var candidates = new List<MapNode>();
            foreach (var nextNode in next)
            {
                if (Mathf.Abs(curr.column - nextNode.column) <= 1)
                    candidates.Add(nextNode);
            }

            // 인접 노드가 아예 없으면 가장 가까운 노드로 대체
            if (candidates.Count == 0)
                candidates.Add(FindClosest(curr, next));

            // 필수 연결 1개
            var picked = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            curr.nextNodeIndices.Add(picked.nodeIndex);
            reachedNext.Add(picked.nodeIndex);

            // 50% 확률로 후보 중 다른 노드 1개 추가 연결 (경로 다양성)
            if (candidates.Count > 1 && UnityEngine.Random.value < 0.5f)
            {
                MapNode extra;
                do { extra = candidates[UnityEngine.Random.Range(0, candidates.Count)]; }
                while (extra.nodeIndex == picked.nodeIndex);

                curr.nextNodeIndices.Add(extra.nodeIndex);
                reachedNext.Add(extra.nodeIndex);
            }
        }

        // 고립된 다음 층 노드 강제 연결
        foreach (var nextNode in next)
        {
            if (reachedNext.Contains(nextNode.nodeIndex)) continue;
            var closest = FindClosest(nextNode, current);
            if (!closest.nextNodeIndices.Contains(nextNode.nodeIndex))
                closest.nextNodeIndices.Add(nextNode.nodeIndex);
        }
    }

    // column 기준으로 가장 가까운 노드 반환
    private static MapNode FindClosest(MapNode from, List<MapNode> candidates)
    {
        MapNode closest = candidates[0];
        int minDist = Mathf.Abs(from.column - closest.column);
        foreach (var c in candidates)
        {
            int dist = Mathf.Abs(from.column - c.column);
            if (dist < minDist) { minDist = dist; closest = c; }
        }
        return closest;
    }

    // 가중치 기반 노드 타입 추첨
    private static NodeType GetRandomNodeType(List<NodeTypeWeight> weights)
    {
        float total = 0f;
        foreach (var w in weights) total += w.weight;

        float roll = UnityEngine.Random.Range(0f, total);
        float cumulative = 0f;
        foreach (var w in weights)
        {
            cumulative += w.weight;
            if (roll <= cumulative) return w.nodeType;
        }

        return NodeType.Combat; // 부동소수점 오차 보정
    }

    // Fisher-Yates 셔플
    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
