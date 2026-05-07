using System.Collections.Generic;

// MapGenerator가 생성한 맵 전체 구조를 담는 데이터 컨테이너.
// floors[층 인덱스][노드 인덱스] 로 접근한다.
[System.Serializable]
public class MapData
{
    public int seed;                       // 이 맵을 생성한 시드 (재현용)
    public List<List<MapNode>> floors;     // 층별 노드 목록

    public MapData(int seed, int totalFloors)
    {
        this.seed = seed;

        // 층 개수만큼 빈 리스트를 미리 생성해 둔다.
        // MapNode 리스트의 리스트를 만드는 것. totalfloors만큼.

        // MapGenerator가 나중에 각 층에 노드를 채워 넣는다.
        floors = new List<List<MapNode>>(totalFloors);
        for (int i = 0; i < totalFloors; i++)
            floors.Add(new List<MapNode>());
    }

    // 특정 노드를 반환하는 메서드
    public MapNode GetNode(int floorIndex, int nodeIndex)
        => floors[floorIndex][nodeIndex];
}
