using System.Collections.Generic;
using UnityEngine;

// 맵 상태 관리 및 노드 이동 처리.
// 맵 생성(MapGenerator), 데이터 저장(RunData)과 역할이 분리되어 있다.
public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("설정")]
    [SerializeField] private MapConfig mapConfig;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 런 시작 시 GameManager에서 호출.
    // 맵을 생성하고 RunData에 저장한 뒤 0층(Blessing)에 배치한다.
    public void InitializeMap()
    {
        MapData mapData = MapGenerator.Generate(mapConfig);

        RunData.Instance.mapData          = mapData;
        RunData.Instance.currentFloor     = 0;
        RunData.Instance.currentNodeIndex = 0;
    }

    // 지정한 노드로 이동. 이동 가능 여부는 호출 전에 확인할 것.
    public void MoveToNode(MapNode node)
    {
        node.isVisited = true;

        RunData.Instance.currentFloor     = node.floorIndex;
        RunData.Instance.currentNodeIndex = node.nodeIndex;
    }

    // 현재 노드에서 이동 가능한 다음 노드 목록 반환.
    // nextNodeIndices를 보고 다음 층 노드를 꺼낸다.
    public List<MapNode> GetAccessibleNodes()
    {
        MapNode current = RunData.Instance.CurrentNode;
        if (current == null) return new List<MapNode>();

        int nextFloor = current.floorIndex + 1;

        if (nextFloor >= RunData.Instance.mapData.floors.Count)
            return new List<MapNode>(); // 마지막 층이면 빈 리스트

        var result = new List<MapNode>();
        foreach (int nodeIndex in current.nextNodeIndices)
            result.Add(RunData.Instance.mapData.GetNode(nextFloor, nodeIndex));

        return result;
    }
}
