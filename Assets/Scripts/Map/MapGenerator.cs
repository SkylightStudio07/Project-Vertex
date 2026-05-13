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

        //=========== Phase 0: 초기화 =============== //

        // 랜덤 상태 시드 고정 및 MapData 생성.

        //========================================== //

        UnityEngine.Random.InitState(seed);
        MapData mapData = new MapData(seed, config.totalFloors);

        //=========== Phase 1: 보장 노드 =============== //

        // 보장 노드 처리.
        // 자세한 사항은 MapConfig의 FloorGuarantee 참고. 그러니까 0층이나 9층, 16층처럼 타입이 정해진 층.
        // guaranteedNodes 리스트  { 층 인덱스 / 고정 타입 목록 } 딕셔너리화해서 통합 관리(한 층에 보장 노드들 몰려있는 경우 예외처리용)
        // 보장 노드가 있는 층은 일단 그 타입으로 채움.
        // 3층 -> [Elite]
        // 6층 -> [Shop, Event] 뭐 이런 식.

        //========================================== //
        
        var guaranteeMap = new Dictionary<int, List<NodeType>>();

        // 혹여 중복 보장 노드가 있을 수 있으니 층별로 리스트에 추가하는 방식으로 처리.

        foreach (var guarantee in config.guaranteedNodes)
        {
            // 그러니까 같은 층에 보장 노드가 여러개 있어도 리스트에 계속 추가한다는 것. 
            // 이 상황이 발생하면 맵 생성에 무언가 문제가 생겼다는 것.
            if (!guaranteeMap.ContainsKey(guarantee.floorIndex))
                guaranteeMap[guarantee.floorIndex] = new List<NodeType>();
            guaranteeMap[guarantee.floorIndex].Add(guarantee.nodeType);
        }

        //=========== Phase 2: 일반 노드 배치 =============== //

        // MapNode에 보면 알겠지만 그리드 기반이라 칼럼(열) 개념이 있음. 노드 간 간격처리 문제때문에 도입.
        // 칼럼 개수는 config.maxNodesPerFloor로 일단 고정 (노드 수 max에 묶여 있음).
        // 현재는 maxNodesPerFloor을 한 층의 최대 노드 수 / 가로 칼럼 수로 동시 사용.

        //========================================== //

        int columnCount = config.maxNodesPerFloor;   // 그리드 가로 폭 (현재는 노드 수 max에 묶여 있음)
        int centerCol = columnCount / 2; // 중앙 칼럼. 짝수일 때는 오른쪽값이다.
        int lastFloor = config.totalFloors - 1; // 마지막 층(보스층). 0-based index라 -1


        // 일단 0층, 마지막 층은 어차피 하나밖에 없고 중앙 고정이니 따로 처리한다.


        // 0층: Blessing(STS 니오우의 축복 개념.) 1개, 가운데 열 고정,
        mapData.floors[0].Add(new MapNode
        {
            nodeType   = NodeType.Blessing,
            floorIndex = 0,
            nodeIndex  = 0,
            column     = centerCol
        });

        // 마지막 층: Boss 1개, 가운데 열 고정 (보장 노드 무관. 그건 그냥 시각화용이다.)
        mapData.floors[config.totalFloors - 1].Add(new MapNode
        {
            nodeType   = NodeType.Boss,
            floorIndex = lastFloor,
            nodeIndex  = 0,
            column     = centerCol
        });




        // 1층 ~ 마지막 층까지 순차 배치 (이전 층 column에 의존)
        for (int f = 1; f < config.totalFloors - 1; f++)
        {
            // 이 층의 보장 노드가 있는지 체크 : 있으면 그 타입으로 일단 채우고 본다는 것.
            // TryGetValue 함수 : 처음 본다. 여기서 out var guarantees는 보장 노드 리스트를 담을 변수. 
            // hasGuarantee는 자체는 보장 노드 존재 여부 bool.
            // 그러니까 false면 이 층에는 보장 노드가 없는 거고, true면 guarantees 변수에 보장 노드 리스트가 담긴다는 것.
            // guarantees 타입 자체는 List<NodeType>. var로 관리하는게 속편하다.
            bool hasGuarantee = guaranteeMap.TryGetValue(f, out var guarantees);

            List<NodeType> types;
            int nodeCount; // 이 층의 노드 수. 보장 노드가 있으면 그 수로 고정, 없으면 랜덤 (config 범위 내에서)

            if (hasGuarantee) // 만약 보장 노드가 있으면 그 타입들로 채우고 노드 수도 고정한다. (보장 노드가 여러개일 수 있으니 리스트로 관리한다는 것.)
            {
                types     = new List<NodeType>(guarantees);
                nodeCount = guarantees.Count;
            }
            else // 보장 노드가 없는 케이스.
            // 노드 수를 랜덤으로 뽑고, 타입도 가중치 기반으로 랜덤 추첨한다.
            // 타입 추첨 알고리즘은 GetRandomNodeType 함수 참고.
            {
                nodeCount = UnityEngine.Random.Range(config.minNodesPerFloor, config.maxNodesPerFloor + 1);
                types     = new List<NodeType>();
                for (int i = 0; i < nodeCount; i++)
                    types.Add(GetRandomNodeType(config.nodeTypeWeights));
            }

            //=========== Phase 3: 이전 층 컬럼 수집 =============== //

            // 단순 노드를 배열하는 게 아니라, 배열 배치가 문제라서 이전 층 열 정보를 수집한다.
            // 그러니까, 현재 층의 칼럼을 아무데나 고르는 게 아니라, 이전 층 노드들의 칼럼을 보고 현재 층에 놓일 수 있는 범위를 제한한다는 것.

            // 조금 예시를 들어보자면
            // 이전층 column: 1, 3 -> 그러면 prevCols는: { 1, 3 }.

            //========================================== //

            // 이전 층 column 집합 — 이 층 column은 이 안의 각 col ±1 범위에서만 뽑힘
            var prevCols = new HashSet<int>();
            // HashSet 쓰는 이유 : 중복 제거 + 빠른 탐색. 
            // 사실 이전 층 노드 수가 많지 않아서 List로 해도 큰 문제는 없긴 하다. 클로드 죽어.

            foreach (var prev in mapData.floors[f - 1]) { 
                prevCols.Add(prev.column); 
            }

            // 이 층의 column 후보군 뽑기.
            // 하단의 PickColumns 함수 참고. 이전 층 column ±1 범위에서 nodeCount개를 랜덤 선택해서 반환한다.

            var columns = PickColumns(columnCount, nodeCount, prevCols);

            // 허용 슬롯이 nodeCount보다 적으면 자동 축소. 
            // 일차적으로는 pickColumns 함수 내에서 먼저 수행하는데, 클로드가 여기서도 한 번 더 수행하라고 권해서 여기도 이렇게 함.
            // 당연히 이 경우 types도 같이 잘라낸다.

            if (columns.Count < nodeCount)
            {
                nodeCount = columns.Count;
                if (types.Count > nodeCount) types = types.GetRange(0, nodeCount);
            }

            // 여기서부터 실제 노드 생성 및 배치.
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

        //=========== Phase 4: 노드 연결 =============== //

        // 층 간 노드 연결. ConnectFloors 함수 참고.
        // 제약: (1) |curr.column - next.column| ≤ 1,
        // (2) 왼쪽 curr가 사용한 next.column 이상으로만 (교차 방지. 그러니까, 왼쪽 노드가 오른쪽 노드보다 더 왼쪽 노드랑 연결되는 상황 방지.)
        // 예외처리: 만약 제약 만족하는 후보가 없으면 제약

        //========================================== //

        for (int f = 0; f < config.totalFloors - 1; f++)
            ConnectFloors(mapData.floors[f], mapData.floors[f + 1]);

        return mapData;
    }

    //=========== static 헬퍼 메서드 =============== //

    // 1. PickColumns: 
    // 이전 층 column 정보 기반으로 이번 층 column 후보군을 뽑는 함수.

    // 2. ConnectFloors:
    // 현재 층 노드들과 다음 층 노드들을 간선 연결.

    //========================================== //


    private static List<int> PickColumns(int columnCount, int count, HashSet<int> prevCols)
    {

        // 반환 자체는 List<int> 형태로, 이번 층에 배치할 column 인덱스 리스트이다.

        // columncount: 그리드 가로 폭 (현재는 최대 노드 수에 바인딩)
        // count: 이번 층에 배치할 노드 수(그러니까, 뽑고 싶은 칼럼 수.)
        // prevCols: 이전 층에서 사용된 column 집합.


        // allowed 집합: 
        // 이전 층 column ±1 범위의 column을 허용. 예시: prevCols { 1, 3 } → allowed { 0, 1, 2, 3, 4 } (당연히 범위 내에서만)
        // 중복제거용 해시셋 사용
        var allowed = new HashSet<int>();

        // 예외처리용. 이전층 칼럼 정보가 없으면...
        if (prevCols == null || prevCols.Count == 0)
        {

            // 전체 칼럼을 허용해버리는 것. 
            // 그러니까 columnCount가 5면 allowed는 { 0, 1, 2, 3, 4 } 이렇게 된다.
            // 사실 0층이 아예 하드코딩되어있으므로 볼 일은 없삼. 나중에 4막 만들 때 한 번 지켜봐야 햘 듯.

            for (int i = 0; i < columnCount; i++) allowed.Add(i);
        }

        // 일반적인 경우. 이전 층 칼럼 ±1 범위의 칼럼을 허용한다는 것.

        else
        {
            foreach (var c in prevCols) // prevCols의 각 칼럼에 대해서 순회
            {
                // 이전 층의 각 column c에 대해:
                // c - 1, c, c + 1만 후보 칼럼으로 허용.
                for (int d = -1; d <= 1; d++)
                {
                    int nc = c + d; // nc : new column. 

                    // 예외처리용 메서드.
                    // 전체 노드 수 제한 넘어가는 경우 대비해서 조건문
                    if (nc >= 0 && nc < columnCount) 
                        allowed.Add(nc);
                }
            }
        }

        // 해쉬셋 리스트로 변환
        var slots = new List<int>(allowed);

        // 배치할 노드 수가 허용 슬롯 수보다 많으면 자동 축소. 사실 이런 경우가 있을진 잘 모르겠지만
        // 안 쓰면 무한루프 가능성이 있다. 엄연히 가능성은 있으니까. 
        // 이 로직은 generate 함수에서 PickColumns 호출 직후에도 한 번 더 적용된다. 
        if (slots.Count < count) 
            count = slots.Count;

        // 셔플. 
        Shuffle(slots);

        // 셔플된 리스트에서 앞의 count개만 가져옴.
        // 앞에서 셔플을 했으니 단순히 자르기만 해도 랜덤화해서 자르는 거랑 같은 효과... 라고 한다. 이 생각은 못했네.
        var picked = slots.GetRange(0, count);

        // 오름차순 정렬.
        // 그냥 시각화용이다. 나중에 에디터에서 노드 인덱스 볼 때 보려고...
        picked.Sort();

        return picked;
    }

    // 현재 층 → 다음 층 연결.
    // 두 제약: (1) |curr.column - next.column| ≤ 1, (2) 왼쪽 curr가 사용한 next.column 이상으로만 (교차 방지)
    private static void ConnectFloors(List<MapNode> current, List<MapNode> next)
    {

        // current : 현재 층 노드 리스트
        // next : 다음 층 노드 리스트

        // void인 것에서 짐작했듯이, 반환값이 아니라 current 노드들의 nextNodeIndices를 직접 수정하는 방식으로 연결을 구현한다.

        // column 오름차순으로 처리.
        // 일차적으로 PickColumns에서 오름차순으로 칼럼을 뽑기도 했고, Generate에서도 순서대로 노드가 생성되고 있음.
        // 클로드가 뭐 이렇게 예외처리를 많이 하라고 하는지는 모르겠는데... 하라니까 해야지. 

        var sortedCurr = new List<MapNode>(current);
        sortedCurr.Sort((a, b) => a.column.CompareTo(b.column));

        // 연결된 다음 층 노드 기록용.
        // 그러니까 이미 누군가에게 연결된 노드.
        // 이게 아닌 경우는 아무에게도 연결되지 않은 고립된 노드다.
        var reachedNext = new HashSet<int>();
        int maxNextColUsed = -1;  // 지금까지 사용된 next.column의 최댓값

        // sortedCurr 순회하면서 각 노드마다 연결할 next 노드 후보군을 뽑고 연결.
        foreach (var curr in sortedCurr)
        {
            // candidates : 현재 노드 curr에서 연결 가능한 next 노드 후보군. 제약 조건을 만족하는 next 노드들로 구성된다.
            var candidates = new List<MapNode>();
            foreach (var nextNode in next)
            {
                // 조건 1 : 현재 노드와 다음 노드의 칼럼 차이가 1 이하여야 한다는 것.
                // 이 조건은 사실 0층에서는 적용되면 안된다. 부채꼴 모양이던가? 슬더스 0->1층 이거.
                // 나중에 생각하자. 구현이 우선.
                if (Mathf.Abs(curr.column - nextNode.column) > 1) continue;

                // 조건 2 : 왼쪽 curr가 사용한 next.column 이상으로만 연결해야 한다는 것. (교차 방지)
                // 조금 구체적으로 말하자면, 현재까지 사용한 다음 층 column 중 가장 오른쪽 값이 maxNextColUsed에 들어 있다.
                // 여기서 maxNextColUsed보다 왼쪽 노드로 연결해버리면, 교차해버린다는 것.

                if (nextNode.column < maxNextColUsed) continue;

                // 이 조건을 모두 통과하는 노드만 후보군에 들어감.
                candidates.Add(nextNode);
            }

            // 예외처리용. 만약 제약 조건을 만족하는 후보가 하나도 없으면, 가장 가까운 노드 던져줌.
            // 그나마 발생할 수 있는 경우가 0층인데, 이미 하드코딩되어있으니 뭐...
            if (candidates.Count == 0)
                candidates.Add(FindClosest(curr, next));

            // 후보군 중 하나 랜덤으로 선택.
            // 여기서 picked가 다음 노드.
            var picked = candidates[UnityEngine.Random.Range(0, candidates.Count)];

            // 현재 노드 curr의 nextNodeIndices에 picked 노드의 인덱스 추가. 그리고 reachedNext에도 추가.
            // 조건문은 예외처리용.(중복 연결 방지)... 하나쯤이면 충분하지 않나 클로드야?

            if (!curr.nextNodeIndices.Contains(picked.nodeIndex))
                curr.nextNodeIndices.Add(picked.nodeIndex);

            // 여기다가도 넣고. 
            // 우리가 방금 고른 다음 층 노드는 이제 누군가에게 연결됐다!
            reachedNext.Add(picked.nodeIndex);

            // maxNextColUsed를 갱신
            // 그러니까, 방금 연결한 다음 층 노드가 기존보다 더 오른쪽 column이면 maxNextColUsed 값 갱신.
            if (picked.column > maxNextColUsed) 
            {
                maxNextColUsed = picked.column;
            }

            // 클로드가 이런 구조도 제안했다. 50% 확률!
            // 그러니까 후보가 2개 이상이고, 랜덤값이 0.5보다 작으면 추가 연결을 하나 더 만들어버리는 것.
            if (candidates.Count > 1 && UnityEngine.Random.value < 0.5f)
            {
                var extras = new List<MapNode>();
                foreach (var c in candidates)
                {
                    // 조건 1 : 이미 필수 연결로 선택한 노드는 제외.
                    if (c.nodeIndex == picked.nodeIndex) 
                        continue;
                    // 조건 2 : 방금 선택한 picked보다 왼쪽에 있는 노드도 제외. 당연히 교차되면 안되니까... 
                    if (c.column < picked.column) 
                        continue;
                    extras.Add(c);
                }

                // 간선은 최고 2개까지로만 상정해놔서, 추가 후보가 하나 이상 있으면 그 중에서 랜덤으로 하나만 고른다.
                if (extras.Count > 0)
                {
                    var extra = extras[UnityEngine.Random.Range(0, extras.Count)];
                    if (!curr.nextNodeIndices.Contains(extra.nodeIndex))
                        curr.nextNodeIndices.Add(extra.nodeIndex);
                    reachedNext.Add(extra.nodeIndex);
                    if (extra.column > maxNextColUsed) 
                        maxNextColUsed = extra.column; // 당연히 여기서도 maxNextColUsed는 갱신한다.
                }
            }
        }

        // 고립된 다음 층 노드 강제 연결 노드 배치가 잘 되면 거의 발생하지 않아야 하는데, 나는 내 코드를 못 믿는다.
        foreach (var nextNode in next) // 순회하면서 고립 노드 체크
        {
            if (reachedNext.Contains(nextNode.nodeIndex)) // 이미 누군가에게 연결되어 있다면 문제없음.
                continue; 
            var closest = FindClosest(nextNode, current); // 가장 가까운 현재 층 노드 찾기.

            if (!closest.nextNodeIndices.Contains(nextNode.nodeIndex))
                closest.nextNodeIndices.Add(nextNode.nodeIndex);  
                // 이렇게 해야 다음 층의 노든 노드가 적어도 하나의 이전 노드에서 도달 가능해진다.
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

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
