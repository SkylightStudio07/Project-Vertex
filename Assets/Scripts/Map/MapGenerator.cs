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
        return mapData;
    }
}
