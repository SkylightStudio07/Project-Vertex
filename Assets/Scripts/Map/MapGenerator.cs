using System.Collections.Generic;
using UnityEngine;

// 맵 생성 알고리즘. 인스턴스 불필요 → static class.
// MapConfig 세팅값을 읽어 MapData를 생성해 반환한다.
public static class MapGenerator
{
    // 시드 없이 호출하면 랜덤 시드로 생성
    public static MapData Generate(MapConfig config)
    {
        int seed = new System.Random().Next(int.MinValue, int.MaxValue);
        return Generate(config, seed);
    }

    public static MapData Generate(MapConfig config, int seed)
    {

        //=========== Phase 0: 초기화 =============== //

        // 랜덤 상태 시드 고정 및 MapData 생성.

        //========================================== //

        // 전역 UnityEngine.Random 대신 맵 전용 System.Random 인스턴스 사용 (재현성/독립성 보장)
        var rng = new System.Random(seed);
        MapData mapData = new MapData(seed, config.totalFloors);

        //=========== Phase 1: 보장 노드 =============== //

        // 보장 노드 처리.
        // 자세한 사항은 MapConfig의 FloorGuarantee 참고. 그러니까 0층이나 9층, 16층처럼 타입이 정해진 층.
        // guaranteedNodes 리스트를 { 층 인덱스 → 고정 타입 } 딕셔너리로 바꿔서 통합 관리.
        // 보장 노드가 있는 층은 "그 층 전체"가 그 타입으로 채워진다.
        // 단, 노드 수는 다른 층과 똑같이 랜덤으로 뽑는다. 보장 노드는 "무슨 타입이냐"만 정하지 "몇 개냐"는 정하지 않는다.

        //========================================== //

        var guaranteeMap = new Dictionary<int, NodeType>();

        foreach (var guarantee in config.guaranteedNodes)
        {
            // 한 층에 보장 노드를 두 개 이상 지정하는 건 "층 전체를 이 타입으로"와 의미상 모순이다.
            // 맵 생성 설정에 문제가 생겼다는 뜻이므로, 경고만 띄우고 첫 항목을 쓴다.
            if (guaranteeMap.ContainsKey(guarantee.floorIndex))
            {
                Debug.LogWarning(
                    $"[MapGenerator] {guarantee.floorIndex}층에 보장 노드가 중복 지정됨 " +
                    $"({guaranteeMap[guarantee.floorIndex]} / {guarantee.nodeType}). 첫 항목만 적용한다.");
                continue;
            }
            guaranteeMap[guarantee.floorIndex] = guarantee.nodeType;
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

            // 이 층의 노드 수. 보장 노드가 있든 없든 항상 config 범위에서 랜덤으로 뽑는다.
            // (예전엔 보장 노드 개수를 그대로 노드 수로 썼다. 그래서 보장 층이 죄다 1노드짜리 병목이 됐었음.
            //  보장 노드는 층을 "무슨 타입으로 채울지"만 정하지, "몇 개 놓을지"는 정하지 않는다.)
            int nodeCount = rng.Next(config.minNodesPerFloor, config.maxNodesPerFloor + 1);

            // 이 층의 column 후보군 뽑기.
            // 하단의 PickColumns 함수 참고. 이전 층 column ±1 범위에서 nodeCount개를 랜덤 선택해서 반환한다.
            // 허용 슬롯이 모자라면 PickColumns가 알아서 줄여서 돌려주니까, 노드 수는 그 결과를 그대로 따라간다.
            // (예전에 여기서 한 번 더 축소 + types 잘라내기를 했는데, 아래처럼 타입을 나중에 만들면 잘라낼 게 없다.)
            var columns = PickColumns(columnCount, nodeCount, prevCols, rng);
            nodeCount = columns.Count;

            // 타입은 여기서 정하지 않는다. 간선이 다 만들어진 뒤 Phase 5에서 부모 타입을 보고 정한다.
            // (간선은 column만 보고 만들어지므로, 타입이 비어 있어도 연결에는 아무 지장이 없다)
            for (int n = 0; n < nodeCount; n++)
            {
                var node = new MapNode
                {
                    floorIndex = f,
                    nodeIndex  = n,
                    column     = columns[n]
                };
                // 조우는 더 이상 노드에 박지 않는다 — 런 시작 시 EncounterQueueBuilder가
                // "전투 순서" 큐로 생성하고, 전투 진입마다 소비한다(RunData 참고).
                mapData.floors[f].Add(node);
            }
        }

        //=========== Phase 4: 노드 연결 =============== //

        // 층 간 노드 연결. ConnectFloors 함수 참고.
        // 제약: (1) |curr.column - next.column| ≤ 1, 그러니까 플마 1.
        // (2) 왼쪽 curr가 사용한 next.column 이상으로만 (교차 방지. 그러니까, 왼쪽 노드가 오른쪽 노드보다 더 왼쪽 노드랑 연결되는 상황 방지.)
        // 예외처리: 만약 제약 만족하는 후보가 없으면 제약

        //========================================== //

        for (int f = 0; f < config.totalFloors - 1; f++)
            ConnectFloors(mapData.floors[f], mapData.floors[f + 1], rng);

        //=========== Phase 5: 노드 타입 배정 =============== //

        // 간선이 다 만들어진 뒤에야 "이 노드의 부모가 누구인지"를 알 수 있다.
        // 그래서 타입 배정을 연결 다음으로 뺐다. 배치 규칙(최소 등장 층 / 경로상 연속 금지 /
        // 형제 중복 금지)이 전부 부모 정보를 필요로 하기 때문.
        // 자세한 건 AssignNodeTypes 참고.

        //========================================== //

        AssignNodeTypes(mapData, config, guaranteeMap, rng);

        return mapData;
    }

    //=========== static 헬퍼 메서드 =============== //

    // 1. PickColumns: 
    // 이전 층 column 정보 기반으로 이번 층 column 후보군을 뽑는 함수.

    // 2. ConnectFloors:
    // 현재 층 노드들과 다음 층 노드들을 간선 연결.

    // 3. AssignNodeTypes / PickNodeType / IsTypeAllowed:
    // 간선까지 다 만들어진 맵에 노드 타입을 배정. 배치 규칙은 전부 여기 모여 있다.

    //========================================== //


    // 층을 아래에서 위로 훑으면서 타입을 정한다.
    // 아래에서 위로 가는 이유 : f층 타입을 정하려면 f-1층 타입이 이미 확정돼 있어야 하니까.
    private static void AssignNodeTypes(
        MapData mapData, MapConfig config, Dictionary<int, NodeType> guaranteeMap, System.Random rng)
    {
        int lastFloor = mapData.floors.Count - 1;

        // 0층(Blessing)과 마지막 층(Boss)은 Phase 2에서 이미 박아뒀으니 건너뛴다.
        for (int f = 1; f < lastFloor; f++)
        {
            var floor = mapData.floors[f];

            // 보장 층은 규칙 무시하고 층 전체를 그 타입으로 도배한다.
            // (14층 휴식처럼 "어느 경로로 와도 이건 나온다"가 목적이라 규칙보다 우선이다)
            if (guaranteeMap.TryGetValue(f, out var guaranteedType))
            {
                foreach (var node in floor) 
                    node.nodeType = guaranteedType;
                continue;
            }

            // 이 층 각 노드의 부모 목록을 역인덱싱해둔다.
            // 간선은 이전 층 노드의 nextNodeIndices에 들어 있어서, 자식 입장에선 뒤집어야 부모를 안다.
            var parents = new List<List<int>>(floor.Count);
            for (int i = 0; i < floor.Count; i++) 
                parents.Add(new List<int>());

            foreach (var prev in mapData.floors[f - 1])
                foreach (int childIndex in prev.nextNodeIndices)
                    if (childIndex >= 0 && childIndex < floor.Count)
                        parents[childIndex].Add(prev.nodeIndex);

            for (int n = 0; n < floor.Count; n++)
            {
                // 규칙 2용 : 이 노드의 부모들이 무슨 타입인지
                var parentTypes = new List<NodeType>();
                foreach (int p in parents[n])
                    parentTypes.Add(mapData.floors[f - 1][p].nodeType);

                // 규칙 3용 : 부모를 공유하면서 이미 타입이 정해진 형제들
                // m < n 으로만 도는 이유 : 아직 타입이 안 정해진 오른쪽 노드는 볼 게 없다.
                var siblingTypes = new List<NodeType>();
                if (config.forbidSiblingDuplicates)
                {
                    for (int m = 0; m < n; m++)
                    {
                        bool sharesParent = false;
                        foreach (int p in parents[m])
                        {
                            if (parents[n].Contains(p)) { sharesParent = true; break; }
                        }
                        if (sharesParent) 
                            siblingTypes.Add(floor[m].nodeType);
                    }
                }

                floor[n].nodeType = PickNodeType(config, f, parentTypes, siblingTypes, guaranteeMap, rng);
            }
        }
    }

    // 규칙을 통과하는 타입만 남겨놓고 가중치 추첨.
    // 재추첨 루프를 돌리는 대신 후보를 먼저 걸러내는 방식이라, 무한루프도 없고 가중치 비율도 그대로 유지된다.
    private static NodeType PickNodeType(
        MapConfig config, int floorIndex,
        List<NodeType> parentTypes, List<NodeType> siblingTypes,
        Dictionary<int, NodeType> guaranteeMap, System.Random rng)
    {
        var allowed = new List<NodeTypeWeight>();
        foreach (var w in config.nodeTypeWeights)
        {
            if (w.weight <= 0f) continue;   // 가중치 0이면 애초에 안 나오는 타입
            if (IsTypeAllowed(config, w.nodeType, floorIndex, parentTypes, siblingTypes, guaranteeMap))
                allowed.Add(w);
        }

        // 규칙이 후보를 전부 막아버리는 경우 대비.
        // 전투는 어느 층에서든, 어떤 부모 밑에서든 허용되는 타입이라 폴백으로 안전하다.
        if (allowed.Count == 0) 
            return NodeType.Combat;

        return GetRandomNodeType(allowed, rng);
    }

    // 배치 규칙 판정. 규칙을 추가하고 싶으면 여기에 조건을 하나 더 붙이면 된다.
    private static bool IsTypeAllowed(
        MapConfig config, NodeType type, int floorIndex,
        List<NodeType> parentTypes, List<NodeType> siblingTypes,
        Dictionary<int, NodeType> guaranteeMap)
    {
        // 규칙 1 : 최소 등장 층. 이 층 전에는 후보에서 아예 빠진다.
        // 엘리트/휴식은 표시상 6층부터 = minFloorIndex 5.
        if (floorIndex < config.GetMinFloorIndex(type)) 
            return false;

        // 규칙 2, 3은 "연속 금지" 목록에 올라간 타입한테만 건다.
        // 전투/이벤트는 연속으로 나와도 상관없으니 여기서 바로 통과.
        if (!config.noConsecutiveTypes.Contains(type)) 
            return true;

        // 규칙 2 : 경로상 부모와 같은 타입 금지.
        // 휴식 노드에 서 있으면 다음 선택지에 휴식이 안 뜬다는 뜻.
        if (parentTypes.Contains(type)) 
            return false;

        // 규칙 2-b : 다음 층이 같은 타입으로 도배되는 보장 층이면 이 층엔 놓지 않는다.
        // 이게 없으면 13층 휴식 -> 14층(보장 휴식층) 휴식으로 규칙 2가 그냥 뚫린다.
        if (guaranteeMap.TryGetValue(floorIndex + 1, out var nextGuaranteed) && nextGuaranteed == type)
            return false;

        // 규칙 3 : 부모를 공유하는 형제와 같은 타입 금지.
        // 선택지가 "휴식 vs 휴식"처럼 무의미해지는 걸 막는다.
        if (config.forbidSiblingDuplicates && siblingTypes.Contains(type)) 
            return false;

        return true;
    }


    private static List<int> PickColumns(int columnCount, int count, HashSet<int> prevCols, System.Random rng)
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
        Shuffle(slots, rng);

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
    private static void ConnectFloors(List<MapNode> current, List<MapNode> next, System.Random rng)
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
            var picked = candidates[rng.Next(0, candidates.Count)];

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
            // 이런 식으로 확률론은 처음 보네. ㅇㅎ
            if (candidates.Count > 1 && rng.NextDouble() < 0.5)
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
                    var extra = extras[rng.Next(0, extras.Count)];
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

        // 가장 가까운 노드 찾는 기준은 column 간의 절대값 차이.
        if(candidates == null || candidates.Count == 0) 
            return null; // 예외처리용. 후보가 없으면 null 반환.
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
    private static NodeType GetRandomNodeType(List<NodeTypeWeight> weights, System.Random rng)
    {
        float total = 0f;
        foreach (var w in weights) total += w.weight;

        float roll = (float)(rng.NextDouble() * total);
        float cumulative = 0f;
        foreach (var w in weights)
        {
            cumulative += w.weight;
            if (roll <= cumulative) return w.nodeType;
        }

        return NodeType.Combat; // 부동소수점 오차 보정
    }

    private static void Shuffle<T>(List<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
