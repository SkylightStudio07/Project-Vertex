using UnityEngine;

// 런 전체에서 유지되는 상태 데이터.
// 맵, 현재 위치, 덱, HP 등 런이 끝날 때까지 살아있어야 하는 데이터를 여기서 관리한다.
public class RunData : MonoBehaviour
{

    // 차후 게임매니저가 들고 있는 hp, player 덱 등은 다 여기서 원칙적으로 관리해야 함.
    // 아직은 맵 관련 처리하고 있으니 여기서 일단 멈춤.

    public static RunData Instance { get; private set; }

    [Header("맵")]
    public MapData mapData;          // MapGenerator가 생성한 맵 전체 구조
    public int currentFloor;         // 현재 층 인덱스
    public int currentNodeIndex;     // 현재 층에서의 노드 인덱스

    // Inspector에서 현재 노드 타입 확인용. 
    // MapManager가 이동할 때마다 갱신한다.
    // 프로퍼티라 field 문법 사용
    [field: SerializeField] public NodeType CurrentNodeType { get; set; }

    // 현재 위치의 MapNode를 바로 꺼내는 편의 프로퍼티
    public MapNode CurrentNode => mapData?.GetNode(currentFloor, currentNodeIndex);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
