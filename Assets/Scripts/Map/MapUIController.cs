using System.Collections.Generic;
using UnityEngine;

// 맵 패널의 열고 닫기, 노드 생성 및 배치, 상태 갱신을 담당하는 컨트롤러
// MapManager/RunData에서 데이터를 읽어 MapNodeView 프리팹을 MapContent에 Instantiate함
public class MapUIController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private RectTransform mapContent;
    [SerializeField] private MapNodeView nodePrefab;
    [SerializeField] private MapConnectionLine linePrefab; // 연결선 프리팹
    [SerializeField] private UnityEngine.UI.ScrollRect scrollRect;

    [Header("배치 설정")]
    [SerializeField] private float floorSpacing  = 120f;
    [SerializeField] private float columnSpacing = 120f;

    private readonly List<MapNodeView>       _nodeViews = new();
    private readonly List<MapConnectionLine> _lineViews = new();

    private void Start()
    {
        mapPanel.SetActive(false);
    }

    // M키 등 외부에서 호출. 맵을 열고 현재 상태로 빌드함.
    // InitializeMap()은 여기서 호출하지 않음 — 런 시작 시 GameManager에서 한 번만 호출해야 함
    public void OpenMap()
    {
        mapPanel.SetActive(true);
        BuildMap();
    }

    public void CloseMap()
    {
        mapPanel.SetActive(false);
    }

    // Map 버튼의 OnClick()에 바인딩. 열려있으면 닫고, 닫혀있으면 엶.
    public void ToggleMap()
    {
        if (mapPanel.activeSelf) CloseMap();
        else OpenMap();
    }

    // MapData를 읽어 노드 프리팹을 생성하고 위치를 배치함
    private void BuildMap()
    {
        // 기존에 생성된 노드/선 전부 제거 (맵 재진입 시 중복 방지)
        foreach (var view in _nodeViews) Destroy(view.gameObject);
        foreach (var line in _lineViews) Destroy(line.gameObject);
        _nodeViews.Clear();
        _lineViews.Clear();

        MapData mapData = RunData.Instance.mapData;
        if (mapData == null)
        {
            Debug.LogWarning("[MapUIController] mapData가 null입니다. MapManager.InitializeMap()이 호출됐는지 확인하세요.");
            return;
        }

        // Content 세로 크기를 층 수에 맞게 조정
        // MapContent anchor: bottom-center, pivot: (0.5, 0) 으로 설정해야 Y=0이 화면 하단이 됨
        float totalHeight = mapData.floors.Count * floorSpacing;
        mapContent.sizeDelta = new Vector2(mapContent.sizeDelta.x, totalHeight);
        mapContent.anchoredPosition = Vector2.zero; // 이전 스크롤 위치 잔재 초기화

        // 가로 중앙 정렬을 위한 오프셋 계산
        // column은 0부터 시작하므로 전체 너비의 절반만큼 왼쪽으로 밀어서 중앙 정렬
        int maxCol = 0;
        foreach (var floor in mapData.floors)
            foreach (var node in floor)
                if (node.column > maxCol) maxCol = node.column;
        float xOffset = -(maxCol * columnSpacing) / 2f;

        // 연결선을 먼저 생성 (노드보다 뒤에 렌더링되도록 계층 순서상 앞에 둠)
        BuildLines(mapData, xOffset);

        // 전체 노드 순회하며 프리팹 생성
        foreach (var floor in mapData.floors)
        {
            foreach (var node in floor)
            {
                MapNodeView view = Instantiate(nodePrefab, mapContent);

                // floorIndex가 클수록 위에 배치 (0층 = 하단)
                float x = node.column * columnSpacing + xOffset;
                float y = node.floorIndex * floorSpacing;
                view.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);

                view.Setup(node, OnNodeClicked);
                _nodeViews.Add(view);
            }
        }

        RefreshNodeStates();

        // normalizedPosition은 ScrollRect LateUpdate가 덮어쓸 수 있어서 직접 위치를 설정함
        // anchoredPosition.y = 0 → content 하단이 viewport 하단에 정렬 → 0층이 하단에 표시됨
        Canvas.ForceUpdateCanvases();
        scrollRect.velocity = Vector2.zero;
        mapContent.anchoredPosition = new Vector2(mapContent.anchoredPosition.x, 0f);
    }

    // 노드 연결 정보(nextNodeIndices)를 읽어 연결선을 생성함
    // 선은 노드보다 먼저 Instantiate되어 계층 순서상 뒤에 그려짐
    private void BuildLines(MapData mapData, float xOffset)
    {
        // 노드 pivot이 (0.5, 0)이라 anchoredPosition이 노드 하단 기준임.
        // 선이 노드 중앙에 연결되도록 노드 높이의 절반만큼 Y를 올림.
        float nodeHalfHeight = nodePrefab.GetComponent<RectTransform>().sizeDelta.y * 0.5f;

        foreach (var floor in mapData.floors)
        {
            foreach (var node in floor)
            {
                int nextFloor = node.floorIndex + 1;
                if (nextFloor >= mapData.floors.Count) continue;

                Vector2 from = new(
                    node.column * columnSpacing + xOffset,
                    node.floorIndex * floorSpacing + nodeHalfHeight);

                foreach (int nextIndex in node.nextNodeIndices)
                {
                    MapNode nextNode = mapData.GetNode(nextFloor, nextIndex);
                    Vector2 to = new(
                        nextNode.column * columnSpacing + xOffset,
                        nextNode.floorIndex * floorSpacing + nodeHalfHeight);

                    MapConnectionLine line = Instantiate(linePrefab, mapContent);
                    line.Setup(from, to);
                    _lineViews.Add(line);
                }
            }
        }
    }

    // 현재 RunData 상태를 기준으로 전체 노드의 시각 상태를 다시 계산함
    // 이동할 때마다 호출해서 클릭 가능 여부와 색상을 최신 상태로 유지
    private void RefreshNodeStates()
    {
        MapNode currentNode    = RunData.Instance.CurrentNode;
        List<MapNode> accessible = MapManager.Instance.GetAccessibleNodes();

        foreach (var view in _nodeViews)
        {
            MapNodeState state;

            if (view.Data == currentNode)
                state = MapNodeState.Current;
            else if (view.Data.isVisited)
                state = MapNodeState.Visited;
            else if (accessible.Contains(view.Data))
                state = MapNodeState.Accessible;
            else
                state = MapNodeState.Locked;

            view.SetState(state);
        }
    }

    // 노드 클릭 시 MapNodeView에서 콜백으로 호출됨
    private void OnNodeClicked(MapNode node)
    {
        MapManager.Instance.MoveToNode(node);
        RefreshNodeStates();
        // 이동 완료 후 맵 닫기. 필요 시 제거하고 열어둬도 됨.
        CloseMap();
    }


}
