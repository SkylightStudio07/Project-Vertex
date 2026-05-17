using System.Collections.Generic;
using UnityEngine;

// 맵 패널 열기/닫기, 노드 및 연결선 생성, 상태 갱신을 담당
public class MapUIController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private RectTransform mapContent;
    [SerializeField] private MapNodeView nodePrefab;
    [SerializeField] private MapConnectionLine linePrefab;
    [SerializeField] private UnityEngine.UI.ScrollRect scrollRect;

    [Header("배치 설정")]
    [SerializeField] private float floorSpacing    = 120f;
    [SerializeField] private float columnSpacing   = 120f;
    [SerializeField] private float verticalPadding = 80f; // 맵 상하 여백 (픽셀)

    private readonly List<MapNodeView>       nodeViews = new();
    private readonly List<MapConnectionLine> lineViews = new();

    private void Start()
    {
        mapPanel.SetActive(false);
    }

    public void OpenMap()
    {
        mapPanel.SetActive(true);
        BuildMap();
    }

    public void CloseMap()
    {
        mapPanel.SetActive(false);
    }

    // Map 버튼 OnClick()에 바인딩
    public void ToggleMap()
    {
        if (mapPanel.activeSelf) CloseMap();
        else OpenMap();
    }
    // 맵 데이터에 따라 노드와 연결선을 생성. 기존 뷰는 모두 제거 후 새로 만듦.
    private void BuildMap()
    {

        // 기존 뷰 제거
        // Destroy()는 즉시 제거가 아니라 프레임 끝에 제거되므로, 리스트에서 먼저 제거해서 중복 참조 방지
        foreach (var view in nodeViews) Destroy(view.gameObject);
        foreach (var line in lineViews) Destroy(line.gameObject);


        // 리스트 초기화
        nodeViews.Clear();
        lineViews.Clear();

        MapData mapData = RunData.Instance.mapData;
        if (mapData == null)
        {
            Debug.LogWarning("[MapUIController] mapData가 null입니다. MapManager.InitializeMap()이 호출됐는지 확인하세요.");
            return;
        }

        float totalHeight = mapData.floors.Count * floorSpacing + verticalPadding * 2f;
        mapContent.sizeDelta = new Vector2(mapContent.sizeDelta.x, totalHeight);

        // column 0부터 시작하므로 최대 column 기준으로 가로 중앙 정렬
        int maxCol = 0;
        foreach (var floor in mapData.floors)
            foreach (var node in floor)
                if (node.column > maxCol) maxCol = node.column;
        float xOffset = -(maxCol * columnSpacing) / 2f;

        // 선은 노드보다 먼저 생성해야 계층 순서상 노드 뒤에 렌더링됨
        BuildLines(mapData, xOffset);

        foreach (var floor in mapData.floors)
        {
            foreach (var node in floor)
            {
                MapNodeView view = Instantiate(nodePrefab, mapContent);

                float x = node.column * columnSpacing + xOffset;
                float y = node.floorIndex * floorSpacing + verticalPadding;
                view.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);

                view.Setup(node, OnNodeClicked);
                nodeViews.Add(view);
            }
        }

        RefreshNodeStates();

        // Instantiate 직후 레이아웃을 강제 갱신한 뒤 스크롤을 하단(0층)으로 고정
        Canvas.ForceUpdateCanvases();
        scrollRect.velocity = Vector2.zero;
        mapContent.anchoredPosition = new Vector2(mapContent.anchoredPosition.x, 0f);
    }

    private void BuildLines(MapData mapData, float xOffset)
    {
        // 노드 pivot이 (0.5, 0)이므로 anchoredPosition이 노드 하단 기준.
        // 선이 노드 중앙에 연결되도록 절반 높이만큼 Y 오프셋 적용.
        float nodeHalfHeight = nodePrefab.GetComponent<RectTransform>().sizeDelta.y * 0.5f;

        foreach (var floor in mapData.floors)
        {
            foreach (var node in floor)
            {
                int nextFloor = node.floorIndex + 1;
                if (nextFloor >= mapData.floors.Count) continue;

                Vector2 from = new(
                    node.column * columnSpacing + xOffset,
                    node.floorIndex * floorSpacing + verticalPadding + nodeHalfHeight);

                foreach (int nextIndex in node.nextNodeIndices)
                {
                    MapNode nextNode = mapData.GetNode(nextFloor, nextIndex);
                    Vector2 to = new(
                        nextNode.column * columnSpacing + xOffset,
                        nextNode.floorIndex * floorSpacing + verticalPadding + nodeHalfHeight);

                    MapConnectionLine line = Instantiate(linePrefab, mapContent);
                    line.Setup(from, to);
                    lineViews.Add(line);
                }
            }
        }
    }

    private void RefreshNodeStates()
    {
        MapNode currentNode      = RunData.Instance.CurrentNode;
        List<MapNode> accessible = MapManager.Instance.GetAccessibleNodes();

        foreach (var view in nodeViews)
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

    private void OnNodeClicked(MapNode node)
    {
        MapManager.Instance.MoveToNode(node);
        RefreshNodeStates();
        CloseMap();
    }
}
