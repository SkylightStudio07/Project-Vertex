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

    public List<int> nextNodeIndices = new(); // 해당 노드가 가리키는 다음 층의 노드 인덱스 목록
    public bool isVisited; // 방문 여부
    
}
