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

            guaranteeMap[guarantee.floorIndex].Add(guarantee.nodeType); // 이게 문제의 보장 노드 여러개인 경우. 이 경우는 사실 누군가 MapConfig를 잘못 만졌다고 봐야 함.
        }

        return mapData;
    }
}
