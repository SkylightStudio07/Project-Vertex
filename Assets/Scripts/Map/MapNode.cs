using System.Collections.Generic;

/*
Mapnode는 노드만 처리.
맵 구조 전체는 MapData. 
*/

[System.Serializable]
public class MapNode
{   // ◾◾◾◾◾◾◾◾
    // ◾◾◾◾◾◾◾◾ 

    public NodeType nodeType; // 노드 유형
    public int floorIndex; // 층 인덱스 - 

    public int nodeIndex; // 노드 인덱스 - 층 내 순서 (0, 1, 2...)
    public int column;   // 그리드 열 위치 (0 ~ maxNodesPerFloor-1), 연결 및 UI 배치에 사용

    public List<int> nextNodeIndices = new();
    public bool isVisited;

    // 맵 생성 시 할당되는 조우 데이터 — Combat/Elite/Boss 노드에만 채워짐
    public List<EnemyData> encounter = new();
    
}
