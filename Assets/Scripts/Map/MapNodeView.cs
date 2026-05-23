using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 노드가 현재 어떤 상태인지 열거형
// MapUIController가 이 상태를 계산해서 SetState()로 전달함
public enum MapNodeState
{
    Locked,     // 아직 갈 수 없는 노드 (회색). 디폴트
    Accessible, // 현재 위치에서 이동 가능한 노드 (흰색, 클릭 가능) - 살짝 백색 색조 더하기
    Current,    // 플레이어가 현재 있는 노드 (황금색) - 일단 디버깅 차원임.
    Visited     // 이미 방문한 노드 - 회색
}

// 노드 하나하나 시각적 처리 클래스. MapUIController가 프리팹을 Instantiate한 뒤 Setup()으로 초기화하고, 맵 이동 시마다 SetState()로 상태 갱신.
public class MapNodeView : MonoBehaviour
{
    // NodeType별 스프라이트 매핑 - 어차피 9개니까 선형으로 무식하게 해도 문제없음.
    [Serializable]
    public struct NodeSprite
    {
        public NodeType nodeType;
        public Sprite sprite;
    }

    [SerializeField] private List<NodeSprite> nodeSprites;

    [SerializeField] private Image iconImage; // 노드 아이콘 표시용 Image 컴포넌트 (프리팹에서 연결)
    [SerializeField] private Button button; 
    [SerializeField] private GameObject currentMarker;    // 현재 위치 마커

    // 각 상태별 색상 (iconImage에 tinting으로 적용) - 클로드야 고마워
    private static readonly Color ColorLocked     = new(0.35f, 0.35f, 0.35f, 1f);
    private static readonly Color ColorAccessible = Color.white;
    private static readonly Color ColorCurrent    = new(1f, 0.95f, 0.6f, 1f);
    private static readonly Color ColorVisited    = new(0.55f, 0.55f, 0.55f, 1f);

    // 이 View가 표현하는 MapNode 데이터
    public MapNode Data { get; private set; }

    // 노드 초기화. MapUIController가 프리팹을 생성한 직후 호출함.
    // Action<MapNode>: "MapNode를 인자로 받는 함수"를 변수처럼 전달하는 C# 문법.
    // 클릭 시 이 함수를 호출해서 MapUIController 쪽에 "어떤 노드가 눌렸는지" 알려줌.
    public void Setup(MapNode data, Action<MapNode> onClick)
    {
        Data = data;
        iconImage.sprite = GetSprite(data.nodeType); // 타입에 맞는 스프라이트를 내부에서 조회

        // 중복 등록 방지 후 클릭 리스너 등록
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(Data));
    }

    // 노드의 시각 상태를 변경. MapUIController가 맵 이동 시마다 전체 노드에 호출함.
    public void SetState(MapNodeState state)
    {
        // switch expression: state 값에 따라 색상을 결정하는 간결한 분기 문법
        iconImage.color = state switch
        {
            MapNodeState.Locked     => ColorLocked,
            MapNodeState.Accessible => ColorAccessible,
            MapNodeState.Current    => ColorCurrent,
            MapNodeState.Visited    => ColorVisited,
            _                       => Color.white  // 예외 케이스 (발생하지 않음)
        };

        // Accessible 상태일 때만 버튼 클릭 가능
        button.interactable = state == MapNodeState.Accessible;

        // 현재 위치 마커는 Current 상태일 때만 표시
        if (currentMarker != null)
            currentMarker.SetActive(state == MapNodeState.Current);
    }

    // nodeType에 맞는 스프라이트 반환. 일반 선형 탐색.
    private Sprite GetSprite(NodeType type)
    {
        foreach (var entry in nodeSprites)
            if (entry.nodeType == type) return entry.sprite;
        return null;
    }
}
